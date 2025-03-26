using OpenCvSharp;

namespace ShortCircuitDetect.Lib.Service.Impl;


//var options = new DefectOptions()
//{
//    // 基板颜色
//    BMin = 5,
//    BMax = 140,
//    GMin = 200,
//    GMax = 255,
//    RMin = 0,
//    RMax = 173,
//    //MinArea = 900,
//    MinArea = 500,

//    OpenWidth = 5,
//    OpenHeight = 5,

//    CloseWidth = 30,
//    CloseHeight = 30,

//    MatchMode = TemplateMatchModes.CCoeff,
//    MatchLocScore = 0.8,
//};
public class DefectDetectV1 : IDefectDetect
{
    readonly Mat cam;
    readonly Mat color;
    readonly Mat uv;
    readonly DefectOptions options;

    Rect roi;
    Mat? mask;
    private bool disposed;

    public DefectDetectV1(Mat cam, Mat color, Mat uv, DefectOptions options)
    {
        if (cam is null || cam.Empty()) throw new ArgumentException("cam图不能为空");
        if (color is null || color.Empty()) throw new ArgumentException("color图不能为空");
        if (uv is null || uv.Empty()) throw new ArgumentException("uv图不能为空");
        ArgumentNullException.ThrowIfNull(options);

        this.cam = cam;
        this.color = color;
        this.uv = uv;
        this.options = options;
    }

    public Mat GetDefectMask()
    {

        // BUG: X116738Y26238_Cam.png 生成的mask会有一条细线，相机的没有
        (var score, var loc) = CVOperation.FindMatchLoc(cam, uv, options.MatchMode);

        // 匹配分数过小，无法提取ROI进行下一步操作
        if (score < options.MatchLocScore) return Mat.Zeros(uv.Size(), MatType.CV_8U);

        // 基板ROI
        roi = new(loc, new Size(uv.Width, uv.Height));

        // 提取基板 mask
        mask = cam.SubMat(roi).Threshold(10, 255, ThresholdTypes.BinaryInv);

        Mat hsv = new Mat(color.Size(), MatType.CV_8UC3);
        color.CvtColor(ColorConversionCodes.BGR2HSV).CopyTo(hsv, mask);

        // 此时是板子的Mask
        var copperMask = ExtractBoardMask(hsv);

        GetOpenAndCloseSize(out Size openSize, out Size closeSize);

        var openElement = Cv2.GetStructuringElement(MorphShapes.Rect, openSize);
        Cv2.MorphologyEx(copperMask, copperMask, MorphTypes.Open, openElement);

        var closeElement = Cv2.GetStructuringElement(MorphShapes.Rect, closeSize);
        Cv2.MorphologyEx(copperMask, copperMask, MorphTypes.Close, closeElement);

        // 此时是铜的Mask
        Cv2.Subtract(mask, copperMask, copperMask);
        Cv2.MorphologyEx(copperMask, copperMask, MorphTypes.Open, openElement);

        // 提取轮廓，提取面积小于MinArea的轮廓
        IList<Point[]> contours = GetContours(copperMask).Where(c => FilterContourArea(c, options.MinArea)).ToList();

        Mat mat = Mat.Zeros(copperMask.Size(), MatType.CV_8U);
        // 将面积小的轮廓填充到mask
        Cv2.DrawContours(mat, contours, -1, Scalar.White, -1);

        //var temp = new Mat();
        //color.CopyTo(temp, mask);
        //Cv2.ImShow("test", temp);

        return copperMask - mat;
    }

    private void GetOpenAndCloseSize(out Size openSize, out Size closeSize)
    {
        var defaultSize = new Size(3, 3);
        openSize = new Size(options.OpenWidth, options.OpenHeight);
        closeSize = new Size(options.CloseWidth, options.OpenHeight);

        if (openSize.Width == 0 || openSize.Height == 0)
        {
            openSize = defaultSize;
        }
        if (closeSize.Width == 0 || closeSize.Height == 0)
        {
            closeSize = defaultSize;
        }
    }

    private static Point[][] GetContours(Mat mat)
    {
        mat.FindContours(out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);
        return contours;
    }

    static bool FilterContourArea(Point[] contour, int area) => Cv2.ContourArea(contour) < area;

    Mat ExtractBoardMask(Mat hsv)
    {
        var white = Scalar.White.ToVec3b();
        var mask = new Mat(hsv.Size(), MatType.CV_8U);
        for (int row = 0; row < hsv.Rows; row++)
        {
            for (int col = 0; col < hsv.Cols; col++)
            {
                ref var bgr = ref hsv.At<Vec3b>(row, col);
                if (bgr[1] <= options.GMax && bgr[1] >= options.GMin &&
                    bgr[0] <= options.BMax && bgr[0] >= options.BMin &&
                    bgr[2] <= options.RMax && bgr[2] >= options.RMin)
                {
                    mask.Set(row, col, white);
                }
            }
        }
        return mask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                cam.Dispose();
                uv.Dispose();
                color.Dispose();
                mask?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposed = true;
        }
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~DefectDetect()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
