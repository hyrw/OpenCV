using MathNet.Numerics;
using OpenCvSharp;

// 50 87 125 162 200
var grid = GridGenerator.GenerateGrid((50, 200), (50, 200), 25);
// 模拟的假数据
Point[] sampleGrid = [
    new(52, 48), new(88, 48), new (125, 48), new (161, 48), new(198, 48),
    new(52, 85), new(88, 85), new (125, 85), new (161, 85), new(198, 85),
    new(52, 123), new(88, 123), new (125, 123), new (161, 123), new(198, 123),
    new(52, 160), new(88, 160), new (125, 160), new (161, 160), new(198, 160),
    //new(52, 198), new(88, 198), new (125, 198), new (162, 198), new(198,198),
    new(52, 202), new(88, 202), new (125, 202), new (162, 202), new(198,202),
];
//Point[] sampleGrid = [
//    new(50, 50), new(87, 50), new (125, 50), new (162, 50), new(200, 50),
//    new(50, 87), new(87, 87), new (125, 87), new (162, 87), new(200, 87),
//    new(50, 125), new(87, 125), new (125, 125), new (162, 125), new(200, 125),
//    new(50, 162), new(87, 162), new (125, 162), new (162, 162), new(200, 162),
//    new(50, 200), new(87, 200), new (125, 200), new (162, 200), new(200,200),
//];


var x = grid.Select(p => (double)p.X);
var y = grid.Select(p => (double)p.Y);
double[] dxValues = new double[grid.Count];
double[] dyValues = new double[grid.Count];
for (int i = 0; i < grid.Count; i++)
{
    Point p = grid[i];
    Point sp = sampleGrid[i];
    //dxValues[i] = sp.X - p.X;
    //dyValues[i] = sp.Y - p.Y;
    dxValues[i] = p.X - sp.X;
    dyValues[i] = p.Y - sp.Y;
}

var xInterpolate = Interpolate.Common(x, dxValues);
var yInterpolate = Interpolate.Common(y, dyValues);

Point[] src_points = grid.ToArray();
Point[] after_points = new Point[src_points.Length];
for (int i = 0; i < src_points.Length; i++)
{
    Point p = src_points[i];
    double dx = xInterpolate.Interpolate(p.X);
    double dy = yInterpolate.Interpolate(p.Y);
    Point point = new()
    {
        X = (int)(p.X + dx),
        Y = (int)(p.Y + dy),
    };
    after_points[i] = point;
}

using Mat beforImg = Mat.Zeros(900, 800, MatType.CV_8UC3);
using Mat afterImg = Mat.Zeros(900, 800, MatType.CV_8UC3);

for (int i = 0; i < grid.Count; i++)
{
    Point srcPoint = grid[i];
    Point afterPoint = sampleGrid[i];
    Cv2.Circle(beforImg, srcPoint, 2, Scalar.Green, -1);
    Cv2.DrawMarker(beforImg, afterPoint, Scalar.Red, MarkerTypes.Cross, 10);
}

for (int i = 0; i < src_points.Length; i++)
{
    Point srcPoint = src_points[i];
    Point afterPoint = after_points[i];
    Cv2.Circle(afterImg, srcPoint, 2, Scalar.Green, -1);
    Cv2.DrawMarker(afterImg, afterPoint, Scalar.Red, MarkerTypes.Cross, 10);
}


Cv2.ImShow("before", beforImg);
Cv2.ImShow("after", afterImg);

Cv2.WaitKey();

/// <summary>
/// 根据范围生成网格点
/// </summary>
public static class GridGenerator
{
    public static List<Point> GenerateGrid((int Min, int Max) xRange, (int Min, int Max) yRange, int numPoints)
    {
        int n = (int)Math.Sqrt(numPoints);
        var xPoints = Linspace(xRange.Min, xRange.Max, n);
        var yPoints = Linspace(yRange.Min, yRange.Max, n);

        var grid = new List<Point>();
        foreach (var y in yPoints)
        {
            foreach (var x in xPoints)
            {
                grid.Add(new Point(x, y));
            }
        }
        return grid;
    }

    /// <summary>
    /// 线性等分向量
    /// </summary>
    static IEnumerable<double> Linspace(double start, double end, int num)
    {
        if (num <= 1)
        {
            yield return start;
            yield break;
        }

        double step = (end - start) / (num - 1);
        for (int i = 0; i < num; i++)
        {
            yield return start + i * step;
        }
    }
}
