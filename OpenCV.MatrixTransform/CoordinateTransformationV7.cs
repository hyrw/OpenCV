using OpenCV.MatrixTransform;
using OpenCvSharp;

///<summary>
///分段插值
///</summary>
public class CoordinateTransformationV7
{
    readonly BilinearInterpolator xInterpolator;
    readonly BilinearInterpolator yInterpolator;

    public CoordinateTransformationV7(IList<Point> src, IList<Point> dst, int pointNum)
    {
        if (pointNum == 9)
        {
            (var x, var y, var z_x, var z_y) = MakeData(src, dst, 3);
            this.xInterpolator = new BilinearInterpolator(x, y, z_x);
            this.yInterpolator = new BilinearInterpolator(x, y, z_y);
        }
        else if (pointNum == 25)
        {
            (var x, var y, var z_x, var z_y) = MakeData(src, dst, 5);
            this.xInterpolator = new BilinearInterpolator(x, y, z_x);
            this.yInterpolator = new BilinearInterpolator(x, y, z_y);
        }
        else
        {
            throw new ArgumentException($"{nameof(pointNum)}只支出9点，25点");
        }
    }

    public IList<Point> GetPath(ReadOnlySpan<Point> path)
    {
        if (path == null || path.Length == 0) throw new ArgumentException("路径不能为空");
        List<Point> result = new(path.Length);
        for (int i = 0; i < path.Length; i++)
        {
            var point = path[i];
            var x = xInterpolator.Interpolate(point.X, point.Y);
            var y = yInterpolator.Interpolate(point.X, point.Y);
            var diffX = point.X - x;
            var diffY = point.Y - y;

            result.Add(new Point(x + diffX, y + diffY));
        }
        return result;
    }

    (double[] x, double[] y, double[,] xValue, double[,] yValue) MakeData(IList<Point> src, IList<Point> dst, int n)
    {
        double[] x = [.. src.Select(p => (double)p.X).Distinct().Order().Take(n)];
        double[] y = [.. src.Select(p => (double)p.Y).Distinct().Order().Take(n)];
        double[,] z_x = new double[n, n];
        double[,] z_y = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            List<Point> row = row = dst.Skip(i * n).Take(n).ToList();
            for (int j = 0; j < n; j++)
            {
                z_x[i, j] = row[j].X;
                z_y[i, j] = row[j].Y;
            }
        }

        return (x, y, z_x, z_y);
    }

}
