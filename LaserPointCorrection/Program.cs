using OpenCvSharp;
using static ICoordinateTransformationFactory;

// 基于像素坐标点范围
var laserXRange = (50, 200);
var laserYRange = (50, 200);
List<Point> srcPoint = GridGenerator.GenerateGrid(laserXRange, laserYRange, 25);
// 模拟的假数据
Point[] samplePoint = [
    new(52, 48), new(88, 48), new (125, 48), new (161, 48), new(198, 48),
    new(52, 85), new(88, 85), new (125, 85), new (161, 85), new(198, 85),
    new(52, 123), new(88, 123), new (125, 123), new (161, 123), new(198, 123),
    new(52, 160), new(88, 160), new (125, 160), new (161, 160), new(198, 160),
    //new(52, 198), new(88, 198), new (125, 198), new (162, 198), new(198,198),
    new(52, 202), new(88, 202), new (125, 202), new (162, 202), new(198,202),
];
CoordinateCorrection coordinateCorrection = new CoordinateCorrection(laserXRange, laserYRange, 25);
coordinateCorrection.Correction();
IList<Point> result = coordinateCorrection.CorrectionCoordinate(srcPoint);

using Mat beforImg = Mat.Zeros(900, 800, MatType.CV_8UC3);
using Mat afterImg = Mat.Zeros(900, 800, MatType.CV_8UC3);

for (int i = 0; i < srcPoint.Count; i++)
{
    Point p = srcPoint[i];
    Point sp = samplePoint[i];
    Cv2.Circle(beforImg, p, 5, Scalar.Green, -1);
    Cv2.DrawMarker(beforImg, sp, Scalar.Red, MarkerTypes.Cross, 10);
}

for (int i = 0; i < srcPoint.Count; i++)
{
    Point p = srcPoint[i];
    Point r = result[i];
    Cv2.Circle(afterImg, p, 5, Scalar.Green, -1);
    Cv2.DrawMarker(afterImg, r, Scalar.Red, MarkerTypes.Cross, 10);
}

Cv2.ImShow("before", beforImg);
Cv2.ImShow("after", afterImg);

Cv2.WaitKey();



public class CoordinateCorrection
{
    readonly (int Min, int Max) laserXRange;
    readonly (int Min, int Max) laserYRange;
    readonly int numPoints;

    ICoordinateTransformation? coordinateTransformation;

    public CoordinateCorrection((int Min, int Max) laserXRange, (int Min, int Max) laserYRange, int numPoints)
    {
        this.laserXRange = laserXRange;
        this.laserYRange = laserYRange;
        this.numPoints = numPoints;
    }

    /// <summary>
    /// 标刻
    /// </summary>
    IList<Point> LaserMarking((int Min, int Max) xRange, (int Min, int Max) yRange, int numPoints = 9)
    {
        var srcPoint = GridGenerator.GenerateGrid(xRange, yRange, numPoints);
        // TODO: 标刻

        return srcPoint;
    }

    /// <summary>
    /// 获取图像
    /// </summary>
    Mat AcquireCameraImage()
    {
        // TODO: 获取图像
        return Cv2.ImRead(@"C:/Users/Coder/Pictures\calibration_images\cross.png", ImreadModes.Grayscale);
    }

    /// <summary>
    /// 查找交叉点
    /// </summary>
    IList<Point> FindIntersectionPoints(Mat img)
    {
        //IIntersectionDetector intersectionDetector = new SkeletonizeCrossPoint();
        //return intersectionDetector.FindCrossPoints(img);
        // 模拟的假数据
        Point[] samplePoint = [
            new(52, 48), new(88, 48), new (125, 48), new (161, 48), new(198, 48),
            new(52, 85), new(88, 85), new (125, 85), new (161, 85), new(198, 85),
            new(52, 123), new(88, 123), new (125, 123), new (161, 123), new(198, 123),
            new(52, 160), new(88, 160), new (125, 160), new (161, 160), new(198, 160),
            //new(52, 198), new(88, 198), new (125, 198), new (162, 198), new(198,198),
            new(52, 202), new(88, 202), new (125, 202), new (162, 202), new(198,202),
        ];
        return samplePoint;
    }

    /// <summary>
    /// 矫正
    /// </summary>
    public void Correction()
    {
        IList<Point> srcPoint = LaserMarking(this.laserXRange, this.laserXRange, this.numPoints);
        using Mat gray = AcquireCameraImage();
        IList<Point> crossPoints = FindIntersectionPoints(gray);

        if (crossPoints is null || crossPoints.Count != srcPoint.Count) throw new Exception("理论点和标刻点数量不一致");
        int width = this.laserXRange.Max - this.laserXRange.Min;
        int height = this.laserYRange.Max - this.laserYRange.Min;
        this.coordinateTransformation = ICoordinateTransformationFactory.Create(srcPoint, crossPoints, CoordinateTransformationImpl.V7, width, height);
    }

    /// <summary>
    /// 矫正坐标点
    /// </summary>
    public IList<Point> CorrectionCoordinate(IList<Point> points)
    {
        if (coordinateTransformation is null) throw new Exception("还没执行矫正");

        return this.coordinateTransformation.CorrectionCoordinate(points);
    }
}

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
