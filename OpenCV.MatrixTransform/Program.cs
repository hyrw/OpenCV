using System.Runtime.InteropServices;
using OpenCvSharp;

using Mat before = Mat.Zeros(900, 800, MatType.CV_8UC3);
using Mat after = Mat.Zeros(900, 800, MatType.CV_8UC3);


List<Point> theory = GridGenerator.GenerateGrid((50, 200), (50, 200), 25);
List<Point> real = [
   new (50,50), new (90,58), new (125,60), new (158,58), new (200,50),
   new (56,89), new (90,91.5), new (123.6,94.5), new (160.7,92.8), new (193,89),
   new (60,125), new (92,125), new (125,125), new (157,125), new (190, 125),
   new (56,160), new (90.4,161), new (125,159.6), new (160.3,160.7), new (195,162),
   new (50,200), new (86.9,194), new (125,190), new (162,194), new (200,200),
];

var v7 = new CoordinateTransformationV7(theory, real, 25);
var fixTheory = v7.GetPath(CollectionsMarshal.AsSpan(theory));

// 绘制原始点
foreach (var p in theory)
{
    Cv2.Circle(before, p, 4, Scalar.Green, -1);
}
// 绘制采集点
foreach (var p in real)
{
    Cv2.DrawMarker(before, p, Scalar.Red);
}
// 绘制原始点
foreach (var p in theory)
{
    Cv2.Circle(after, p, 4, Scalar.Green, -1);
}
// 绘制校正点
foreach (var p in fixTheory)
{
    Cv2.DrawMarker(after, p, Scalar.Red);
}

Cv2.ImShow("before", before);
Cv2.ImShow("after", after);
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
