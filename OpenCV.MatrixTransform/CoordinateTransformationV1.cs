using OpenCvSharp;

/// <summary>
/// 仿射变换
/// </summary>
public class CoordinateTransformationV1
{
    readonly double m11, m12, m21, m22;
    readonly double tX, tY;

    public CoordinateTransformationV1(IList<Point> from, IList<Point> to)
    {

        // using Mat matrix = Cv2.GetAffineTransform([.. from], [.. to]);
        using Mat matrix = Cv2.EstimateAffine2D(InputArray.Create(from), InputArray.Create(to))!;

        m11 = matrix.At<double>(0, 0);
        m12 = matrix.At<double>(0, 1);
        m21 = matrix.At<double>(1, 0);
        m22 = matrix.At<double>(1, 1);
        tX = matrix.At<double>(0, 2);
        tY = matrix.At<double>(1, 2);
    }

    public IList<Point> GetPath(ReadOnlySpan<Point> path)
    {
        if (path == null || path.Length == 0) throw new ArgumentException($"{nameof(path)}不能为空");

        List<Point> result = new(path.Length);
        for (var i = 0; i < path.Length; i++)
        {
            result[i] = WarpAffine(path[i], m11, m12, m21, m22, tX, tY);
        }
        return result;
    }

    static Point WarpAffine(Point point, double m11, double m12, double m21, double m22, double tX, double tY)
    {
        (double x, double y) = (point.X, point.Y);
        point.X = (int)(m11 * x + m12 * y + tX);
        point.Y = (int)(m21 * x + m22 * y + tY);
        return point;
    }
}
