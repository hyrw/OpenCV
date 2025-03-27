using OpenCvSharp;
using ShortCircuitDetect.Lib;

// 读取图像
using Mat srcImage = Cv2.ImRead(@"C:\Users\Coder\Pictures\calibration_images\cross.png", ImreadModes.Color);

using Mat gray = srcImage.CvtColor(ColorConversionCodes.BGR2GRAY);
using Mat binary = gray.Threshold(120, 255, ThresholdTypes.Binary);

using Mat erodeKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
using Mat dilateKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
Cv2.MorphologyEx(binary, binary, MorphTypes.Erode, erodeKernel);
Cv2.MorphologyEx(binary, binary, MorphTypes.Dilate, dilateKernel);

using Mat edges = binary.Canny(150, 180);

CVOperation.ImShow("gray", gray);
CVOperation.ImShow("binary", binary);
CVOperation.ImShow("edges", edges);

// var lines = Cv2.HoughLinesP(edges, 1, Cv2.PI / 180, 50);
// foreach (var l in lines)
// {
//     Cv2.Line(srcImage, l.P1, l.P2, Scalar.Red);
// }

Size size = srcImage.Size();
LineSegmentPolar[] linePolars = Cv2.HoughLines(edges, 1, Cv2.PI / 180, 50);
List<LineSegmentPoint> lines = [];
List<Point2f> points = [];
foreach (var i in linePolars)
{
    var rho = i.Rho;
    var theta = i.Theta;
    if (theta < Cv2.PI / 4 || theta > 3 * Cv2.PI / 4)
    {
        // 接近于垂直线条
        Point point1 = new Point(rho / Math.Cos(theta), 0);
        Point point2 = new Point((size.Height * Math.Sin(theta)) / Math.Cos(theta), size.Height);
        Cv2.Line(srcImage, point1, point2, Scalar.Red);
    }
    else
    {
        // 接近于水平线条 r=xcox(theta)+ysin(theta)
        Point point1 = new Point(0, rho / Math.Sin(theta));
        Point point2 = new Point(size.Width, (size.Width * Math.Cos(theta)) / Math.Sin(theta));
        Cv2.Line(srcImage, point1, point2, Scalar.Green);
    }
}

CVOperation.ImShow("lines", srcImage);

Cv2.WaitKey();
