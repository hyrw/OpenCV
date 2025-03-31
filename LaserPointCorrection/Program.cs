using OpenCvSharp;

// 基于像素坐标点范围
var laserXRange = (100, 200);
var laserYRange = (100, 200);

List<Point> srcPoint = GridGenerator.GenerateGrid(laserXRange, laserYRange, 9);

CoordinateCorrection coordinateCorrection = new CoordinateCorrection(laserXRange, laserYRange, 9);
coordinateCorrection.Correction();
IList<Point> result = coordinateCorrection.CorrectionCoordinate(srcPoint);

using Mat color = Mat.Zeros(900, 800, MatType.CV_8UC3);

foreach (var p in srcPoint)
{
    Cv2.Circle(color, p, 2, Scalar.Green, -1);
}
Cv2.ImShow("src point", color);

var point = new Point(srcPoint[0].X + 13, srcPoint[0].Y + 13);
result[0] = point;
foreach (var p in result)
{
    Cv2.DrawMarker(color, p, Scalar.Red, MarkerTypes.Cross, 10);
}
Cv2.ImShow("fix point", color);
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
        IIntersectionDetector intersectionDetector = new SkeletonizeCrossPoint();
        return intersectionDetector.FindCrossPoints(img);
    }

    /// <summary>
    /// 矫正
    /// </summary>
    public void Correction()
    {
        IList<Point> srcPoint = LaserMarking(this.laserXRange, this.laserXRange, this.numPoints);
        using Mat gray = AcquireCameraImage();
        IList<Point> crossPoints = FindIntersectionPoints(gray);
        int width = this.laserXRange.Max - this.laserXRange.Min;
        int height = this.laserYRange.Max - this.laserYRange.Min;
        this.coordinateTransformation = ICoordinateTransformationFactory.Create(
                crossPoints.Take(srcPoint.Count).ToList(),
                srcPoint,
                ICoordinateTransformationFactory.CoordinateTransformationImpl.V1,
                width,
                height);
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
