using OpenCvSharp;
using ShortCircuitDetect.Lib;

int width = 1000;
int height = 1000;

using Mat img = Mat.Zeros(new Size(width, height), MatType.CV_8UC3);

IList<Point> src = [
    new Point(80, 80), new Point(100, 80),new Point(120, 80),
    new Point(80, 100), new Point(100, 100), new Point(120, 100),
    new Point(80,120),new Point(100, 120), new Point(120, 120)];

//IList<Point> laserPoints = [
//     new Point(82, 78), new Point(100, 78),new Point(118, 78),
//     new Point(82, 98), new Point(101, 98), new Point(118, 98),
//     new Point(82,118),new Point(100, 118), new Point(118, 118)];

// 标准误差
IList<Point> laserPoints = [
    new Point(85, 85), new Point(105, 85),new Point(125, 85),
    new Point(85, 105), new Point(105, 105), new Point(125, 105),
    new Point(85,125),new Point(105, 125), new Point(125, 125)];

foreach (var p in src)
{
    DrawCross(img, p, Scalar.Green, 10, 1);
}

foreach (var p in laserPoints)
{
    DrawCross(img, p, Scalar.Red, 10, 1);
}


IList<Point> beforPath = [
    new Point(400, 400), new Point(500, 400),new Point(600, 400),
    new Point(400, 500), new Point(500, 500), new Point(600, 500),
    new Point(400, 600), new Point(500, 600), new Point(600, 600)];

//var transform = new CoordinateTransformationV1(laserPoints.ToList(), src.ToList());
//var transform = new CoordinateTransformationV1(src.ToList(), laserPoints.ToList());
//var transform = new CoordinateTransformationV2(laserPoints.ToList(), src.ToList());
//var transform = new CoordinateTransformationV2(src.ToList(), laserPoints.ToList());
//var transform = new CoordinateTransformationV3(src.ToList(), laserPoints.ToList());
var transform = new CoordinateTransformationV3(laserPoints.ToList(), src.ToList());
//var transform = new CoordinateTransformationV4(src.ToList(), laserPoints.ToList());

foreach (var p in beforPath)
{
    DrawCross(img, p, Scalar.Green, 10, 1);
}

var afterPath = transform.GetPath([[.. beforPath.ToList()]], width, height);
//var afterPath = transform.GetPath([[.. beforPath.ToList()]], width, height);
foreach (var i in afterPath)
{
    foreach (var p in i)
    {
        DrawCross(img, p, Scalar.Yellow, 10, 1);
    }
}

CVOperation.ImShow("test", img, 1);
Cv2.WaitKey();


static void DrawCross(Mat mat, Point point, Scalar color, int size, int thickness)
{
    Cv2.Line(mat, new Point(point.X - size / 2, point.Y), new Point(point.X + size / 2, point.Y), color, thickness, LineTypes.Link8, 0);
    Cv2.Line(mat, new Point(point.X, point.Y - size / 2), new Point(point.X, point.Y + size / 2), color, thickness, LineTypes.Link8, 0);
}

