using OpenCvSharp;

namespace ShortCircuitDetect.Lib.Service.Impl;


//var options = new DefectOptions()
//{
//    Thresh = 200,
//    HMin = 31,
//    HMax = 180,
//    SMin = 0,
//    SMax = 255,
//    VMin = 0,
//    VMax = 255,
//    MinArea = 500,

//    OpenWidth = 7,
//    OpenHeight = 7,

//    CloseWidth = 3,
//    CloseHeight = 3,

//    MatchMode = TemplateMatchModes.CCoeff,
//    MatchLocScore = 0.8,
//};

// 暗黄，发红的无法检测
public class DefectDetectV4 : IDefectDetect
{
    readonly Mat cam;
    readonly Mat color;
    readonly Mat uv;
    readonly DefectOptions options;

    private bool disposed;

    public DefectDetectV4(Mat cam, Mat color, Mat uv, DefectOptions options)
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
        (var score, var loc) = CVOperation.FindMatchLoc(cam, uv, options.MatchMode);

        // 匹配分数过小，无法提取ROI进行下一步操作
        if (score < options.MatchLocScore) return Mat.Zeros(uv.Size(), MatType.CV_8U);

        // 基板ROI
        Rect roi = new(loc, new Size(uv.Width, uv.Height));

        // 提取基板 mask
        using Mat mask = cam.SubMat(roi).Threshold(1, 255, ThresholdTypes.BinaryInv);

        Mat result = Mat.Zeros(uv.Size(), MatType.CV_8U);

        // 从灰度图中找亮的区域
        using Mat uvMask = Mat.Zeros(uv.Size(), MatType.CV_8U);
        uv.Threshold(options.Thresh, 255, ThresholdTypes.Binary).CopyTo(uvMask, mask);

        // 从彩色图中找绿色
        var lower = new Scalar(options.HMin, options.SMin, options.VMin);
        var upper = new Scalar(options.HMax, options.SMax, options.VMax);
        using var hsv = color.CvtColor(ColorConversionCodes.BGR2HSV);
        using var hsvMask = hsv.InRange(lower, upper);
        Cv2.BitwiseNot(hsvMask, hsvMask);

        // CVOperation.ImShow("hsv mask", hsvMask);
        // CVOperation.ImShow("uv mask", uvMask);

        Cv2.BitwiseAnd(mask, uvMask, result, mask);
        Cv2.BitwiseAnd(result, hsvMask, result, mask);

        GetOpenAndCloseSize(out Size openSize, out Size closeSize);

        using var openKernel = Cv2.GetStructuringElement(MorphShapes.Cross, openSize);
        using var closeKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, closeSize);
        Cv2.MorphologyEx(result, result, MorphTypes.Open, openKernel);
        Cv2.MorphologyEx(result, result, MorphTypes.Close, closeKernel);

        var contours = GetContours(result).Where(c => !ContourAreaGreaterThen(c, options.MinArea));

        Cv2.DrawContours(result, contours, -1, Scalar.Black, -1);

        return result;
    }

    void GetOpenAndCloseSize(out Size openSize, out Size closeSize)
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

    static Point[][] GetContours(Mat mat)
    {
        mat.FindContours(out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);
        return contours;
    }

    static bool ContourAreaGreaterThen(Point[] contour, int area) => Cv2.ContourArea(contour) > area;

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
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
