using OpenCvSharp;

namespace ShortCircuitDetect.Lib.Service.Impl;

// 0327_2
/* Param param = new Param()
{
    Thresh = 50,
    ErodeWidth = 3,
    ErodeHeight = 3,
    ErodeIterations = 3,  // 0327_2
    MatchMode = TemplateMatchModes.CCoeff,
    MatchLocScore = 0.9,

    OpenWidth = 11,
    OpenHeight = 11,
    OpenIterations = 1,
    CloseWidth = 7,
    CloseHeight = 7,
    CloseIterations = 1,
    DilateWidth = 5,
    DilateHeight = 5,
    DilateIterations = 1,

    MinArea = 1000,
}; */
public class ImageProcessorV3 : IImageProcessor
{
    private IImageProcessor.Param param;

    public IImageProcessor.Param GetParam() => this.param;

    public Mat GetShortCircuit(Mat norm, Mat gray)
    {
        int erodeIterations = this.param.ErodeIterations;
        int openIterations = this.param.OpenIterations;
        int closeIterations = this.param.CloseIterations;
        using Mat binaryUV = gray.Threshold(this.param.Thresh, 255, ThresholdTypes.Binary);


        // 1. 腐蚀uv图像，直到比例和cam图一致
        GetErodeSize(out var erodeSize);
        using Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, erodeSize);
        using Mat erodeUV = binaryUV.Erode(kernel, iterations: erodeIterations);

        // 2. 模板匹配
        Cv2.BitwiseNot(erodeUV, erodeUV); // 必需
        (var score, var loc) = CVOperation.FindMatchLoc(norm, erodeUV, this.param.MatchMode);
        if (score < this.param.MatchLocScore) return Mat.Zeros(gray.Size(), MatType.CV_8UC1);

        // 3. cam roi
        using Mat camROI = norm.SubMat(new Rect(loc, gray.Size()));
        Cv2.Threshold(camROI, camROI, 10, 255, ThresholdTypes.BinaryInv); // 基板roi

        // 4. 将cam roi 进行处理
        //Cv2.Dilate(camROI, camROI, kernel, iterations: erodeIterations - 4); // 膨胀至uv图像比例，但不能太多，不然边缘不好处理
        //Cv2.Threshold(camROI, camROI, 200, 255, ThresholdTypes.Binary); // 膨胀后图像边缘又灰色的东西

        // 5. 获取基板
        Mat result = Mat.Zeros(gray.Size(), MatType.CV_8UC1);
        // Cv2.Subtract(camROI, binaryUV, result, camROI);
        Cv2.BitwiseAnd(camROI, binaryUV, result, binaryUV);

        // 6. 清除小的连通域
        GetOpenAndCloseSize(out var openSize, out var closeSize);
        using Mat openKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, openSize);
        using Mat closeKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, closeSize);
        Cv2.MorphologyEx(result, result, MorphTypes.Open, openKernel, iterations: openIterations);
        Cv2.MorphologyEx(result, result, MorphTypes.Close, closeKernel, iterations: closeIterations);

        // 7. 清理较小的连通域
        var contours = GetContours(result).Where(c => FilterContourArea(c, this.param.MinArea));
        Cv2.DrawContours(result, contours, -1, Scalar.Black, -1);

        // 以上都是处理基板无铜区域

        GetDilateSize(out var dilateSize);
        using Mat dilateKernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, dilateSize);
        Cv2.BitwiseNot(result, result, camROI);
        Cv2.MorphologyEx(result, result, MorphTypes.Open, openKernel, iterations: openIterations);
        Cv2.MorphologyEx(result, result, MorphTypes.Close, closeKernel, iterations: closeIterations);
        Cv2.MorphologyEx(result, result, MorphTypes.Dilate, dilateKernel, iterations: this.param.DilateIterations);

        var contours2 = GetContours(result).Where(c => FilterContourArea(c, this.param.MinArea));
        Cv2.DrawContours(result, contours2, -1, Scalar.Black, -1);

        return result;
    }

    public Mat GetShortCircuit(Mat norm, Mat gray, Mat color) => throw new NotImplementedException();

    public void SetupParam(IImageProcessor.Param param) => this.param = param;

    void GetOpenAndCloseSize(out Size openSize, out Size closeSize)
    {
        var defaultSize = new Size(3, 3);
        openSize = new Size(this.param.OpenWidth, this.param.OpenHeight);
        closeSize = new Size(this.param.CloseWidth, this.param.CloseHeight);

        if (openSize.Width == 0 || openSize.Height == 0)
        {
            openSize = defaultSize;
        }
        if (closeSize.Width == 0 || closeSize.Height == 0)
        {
            closeSize = defaultSize;
        }
    }

    void GetErodeSize(out Size size)
    {
        var defaultSize = new Size(3, 3);
        size = new Size(this.param.ErodeWidth, this.param.ErodeHeight);

        if (size.Width == 0 || size.Height == 0)
        {
            size = defaultSize;
        }
    }

    void GetDilateSize(out Size size)
    {
        var defaultSize = new Size(3, 3);
        size = new Size(this.param.DilateWidth, this.param.DilateHeight);

        if (size.Width == 0 || size.Height == 0)
        {
            size = defaultSize;
        }
    }

    static Point[][] GetContours(Mat mat)
    {
        mat.FindContours(out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);
        return contours;
    }

    static bool FilterContourArea(Point[] contour, int area) => Cv2.ContourArea(contour) < area;
}
