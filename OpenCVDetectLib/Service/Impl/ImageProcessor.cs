using OpenCvSharp;

namespace ShortCircuitDetect.Lib.Service.Impl;

public class ImageProcessor : IImageProcessor
{
    private IImageProcessor.Param param;

    public IImageProcessor.Param GetParam() => this.param;

    public Mat GetShortCircuit(Mat norm, Mat gray)
    {
        (var score, var loc) = CVOperation.FindMatchLoc(norm, gray, this.param.MatchMode);
        if (score < param.MatchLocScore) return Mat.Zeros(gray.Size(), gray.Type());

        using var mask = norm.SubMat(new Rect(loc, gray.Size())).Threshold(10, 255, ThresholdTypes.BinaryInv);
        using var bin_gray = gray.Threshold(this.param.Thresh, 255, ThresholdTypes.Binary);
        using Mat result = Mat.Zeros(gray.Size(), gray.Type());
        Cv2.CopyTo(bin_gray, result, mask);

        var openElement = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(this.param.OpenWidth, this.param.OpenHeight));
        var closeElement = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(this.param.CloseWidth, this.param.CloseHeight));
        Cv2.MorphologyEx(result, result, MorphTypes.Close, closeElement);
        Cv2.MorphologyEx(result, result, MorphTypes.Open, openElement);

        // 提取轮廓，提取面积小于MinArea的轮廓
        IList<Point[]> contours = GetContours(result).Where(c => FilterContourArea(c, this.param.MinArea)).ToList();
        // 将面积小的轮廓填充到mask
        using Mat mat = Mat.Zeros(result.Size(), result.Type());
        Cv2.DrawContours(mat, contours, -1, Scalar.White, -1);

        return result - mat;
    }

    public Mat GetShortCircuit(Mat norm, Mat gray, Mat color)
    {
        (var colorMatchScore, var colorLoc) = CVOperation.FindMatchLoc(norm, color.CvtColor(ColorConversionCodes.BGR2GRAY), this.param.MatchMode);
        (var grayMatchScore, var grayLoc) = CVOperation.FindMatchLoc(norm, gray, this.param.MatchMode);
        if (colorMatchScore < param.MatchLocScore ||
            grayMatchScore < param.MatchLocScore)
        {
            return Mat.Zeros(gray.Size(), gray.Type());
        }
        var mask = norm.SubMat(new Rect(grayLoc, gray.Size())).Threshold(10, 255, ThresholdTypes.BinaryInv);

        var r = color.Split()[2];
        Cv2.Threshold(r, r, this.param.Thresh, 255, ThresholdTypes.Binary);

        using Mat result = Mat.Zeros(color.Size(), MatType.CV_8UC1);
        Cv2.CopyTo(r, result, mask);

        GetOpenAndCloseSize(out var openSize, out var closeSize);
        var open = Cv2.GetStructuringElement(MorphShapes.Ellipse, openSize);
        var close = Cv2.GetStructuringElement(MorphShapes.Ellipse, closeSize);

        Cv2.MorphologyEx(result, result, MorphTypes.Open, open);
        //Cv2.MorphologyEx(result, result, MorphTypes.Close, close);

        // 提取轮廓，提取面积小于MinArea的轮廓
        IList<Point[]> contours = GetContours(result).Where(c => FilterContourArea(c, this.param.MinArea)).ToList();
        // 将面积小的轮廓填充到mask
        using Mat mat = Mat.Zeros(result.Size(), result.Type());
        Cv2.DrawContours(mat, contours, -1, Scalar.White, -1);

        return result - mat;

    }

    public void SetupParam(IImageProcessor.Param param)
    {
        this.param = param;
    }

    private void GetOpenAndCloseSize(out Size openSize, out Size closeSize)
    {
        var defaultSize = new Size(3, 3);
        openSize = new Size(this.param.OpenWidth, this.param.OpenHeight);
        closeSize = new Size(this.param.CloseWidth, this.param.OpenHeight);

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

}
