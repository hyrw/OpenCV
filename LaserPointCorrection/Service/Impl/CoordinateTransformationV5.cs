using OpenCvSharp;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics;

public class CoordinateTransformationV5 : ICoordinateTransformation
{
    readonly IInterpolation xInterpolate;
    readonly IInterpolation yInterpolate;

    /// <summary>
    /// 插值
    /// </summary>
    /// <param name="srcPoints">理论点</param>
    /// <param name="samplePoints">采样点</param>
    public CoordinateTransformationV5(IList<Point> srcPoints, IList<Point> samplePoints)
    {
        var x = srcPoints.Select(p => (double)p.X);
        var y = srcPoints.Select(p => (double)p.Y);
        double[] dxValues = new double[srcPoints.Count];
        double[] dyValues = new double[srcPoints.Count];
        for (int i = 0; i < srcPoints.Count; i++)
        {
            Point p = srcPoints[i];
            Point sp = samplePoints[i];
            //dxValues[i] = sp.X - p.X;
            //dyValues[i] = sp.Y - p.Y;
            dxValues[i] = p.X - sp.X;
            dyValues[i] = p.Y - sp.Y;
        }

        this.xInterpolate = Interpolate.Common(x, dxValues);
        this.yInterpolate = Interpolate.Common(y, dyValues);
    }

    public IList<Point> CorrectionCoordinate(IList<Point> path) => GetPath(path);

    IList<Point> GetPath(IList<Point> points)
    {
        if (points == null || points.Count == 0) throw new ArgumentException($"{nameof(points)}不能为空");

        List<Point> result = new(points.Count);
        foreach (var p in points)
        {
            double dx = xInterpolate.Interpolate(p.X);
            double dy = yInterpolate.Interpolate(p.Y);
            Point point = new()
            {
                X = double.IsNaN(dx) ? p.X : (int)(p.X + dx),
                Y = double.IsNaN(dy) ? p.Y : (int)(p.Y + dy),
            };
            result.Add(point);

        }
        return result;
    }
}
