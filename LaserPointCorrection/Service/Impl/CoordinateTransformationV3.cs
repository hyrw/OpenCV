using OpenCvSharp;

/// <summary>
/// 透视变换
/// </summary>
public class CoordinateTransformationV3 : ICoordinateTransformation
{
    readonly double a11, a12, a13, a21, a22, a23, a31, a32, a33;

    public CoordinateTransformationV3(IList<Point> from, IList<Point> to)
    {
        using Mat matrix = Cv2.GetPerspectiveTransform([.. from], [.. to]);

        a11 = matrix.At<double>(0, 0);
        a12 = matrix.At<double>(0, 1);
        a13 = matrix.At<double>(0, 2);
        a21 = matrix.At<double>(1, 0);
        a22 = matrix.At<double>(1, 1);
        a23 = matrix.At<double>(1, 2);
        a31 = matrix.At<double>(2, 0);
        a32 = matrix.At<double>(2, 1);
        a33 = matrix.At<double>(2, 2);
    }

    IList<Point> GetPath(IList<Point> paths)
    {
        if (paths == null || paths.Count == 0) throw new ArgumentException($"{nameof(paths)}不能为空");

        return paths.Select(point => WarpPerspective(point, a11, a12, a13, a21, a22, a23, a31, a32, a33)).ToList();
    }

    static Point WarpPerspective(Point point, double m11, double m12, double m13, double m21, double m22, double m23, double m31, double m32, double m33)
    {
        int x = point.X;
        int y = point.Y;
        point.X = (int)((m11 * x + m12 * y + m13) / (m31 * x + m32 * y + m33));
        point.Y = (int)((m21 * x + m22 * y + m23) / (m31 * x + m32 * y + m33));
        return point;
    }

    public IList<Point> CorrectionCoordinate(IList<Point> path) => GetPath(path);
}
