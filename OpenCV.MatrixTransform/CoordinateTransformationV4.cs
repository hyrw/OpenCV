using OpenCvSharp;

/// <summary>
/// 透视变换, 4区域
/// </summary>
public class CoordinateTransformationV4
{
    readonly double a11, a12, a13, a21, a22, a23, a31, a32, a33;
    readonly double b11, b12, b13, b21, b22, b23, b31, b32, b33;
    readonly double c11, c12, c13, c21, c22, c23, c31, c32, c33;
    readonly double d11, d12, d13, d21, d22, d23, d31, d32, d33;

    public CoordinateTransformationV4(IList<Point> from, IList<Point> to)
    {
        IEnumerable<Point2f> fromLeftTop = [from[0], from[1], from[3], from[4]];
        IEnumerable<Point2f> fromRightTop = [from[1], from[2], from[4], from[5]];
        IEnumerable<Point2f> fromLeftBottom = [from[3], from[4], from[6], from[7]];
        IEnumerable<Point2f> fromRightBottom = [from[4], from[5], from[7], from[8]];

        IEnumerable<Point2f> toLeftTop = [to[0], to[1], to[3], to[4]];
        IEnumerable<Point2f> toRightTop = [to[1], to[2], to[4], to[5]];
        IEnumerable<Point2f> toLeftBottom = [to[3], to[4], to[6], to[7]];
        IEnumerable<Point2f> toRightBottom = [to[4], to[5], to[7], to[8]];

        using Mat matrixA = Cv2.GetPerspectiveTransform(fromLeftTop, toLeftTop);
        using Mat matrixB = Cv2.GetPerspectiveTransform(fromRightTop, fromRightTop);
        using Mat matrixC = Cv2.GetPerspectiveTransform(fromLeftBottom, toLeftBottom);
        using Mat matrixD = Cv2.GetPerspectiveTransform(fromRightBottom, toRightBottom);

        a11 = matrixA.At<double>(0, 0);
        a12 = matrixA.At<double>(0, 1);
        a13 = matrixA.At<double>(0, 2);
        a21 = matrixA.At<double>(1, 0);
        a22 = matrixA.At<double>(1, 1);
        a23 = matrixA.At<double>(1, 2);
        a31 = matrixA.At<double>(2, 0);
        a32 = matrixA.At<double>(2, 1);
        a33 = matrixA.At<double>(2, 2);

        b11 = matrixB.At<double>(0, 0);
        b12 = matrixB.At<double>(0, 1);
        b13 = matrixB.At<double>(0, 2);
        b21 = matrixB.At<double>(1, 0);
        b22 = matrixB.At<double>(1, 1);
        b23 = matrixB.At<double>(1, 2);
        b31 = matrixB.At<double>(2, 0);
        b32 = matrixB.At<double>(2, 1);
        b33 = matrixB.At<double>(2, 2);

        c11 = matrixC.At<double>(0, 0);
        c12 = matrixC.At<double>(0, 1);
        c13 = matrixC.At<double>(0, 2);
        c21 = matrixC.At<double>(1, 0);
        c22 = matrixC.At<double>(1, 1);
        c23 = matrixC.At<double>(1, 2);
        c31 = matrixC.At<double>(2, 0);
        c32 = matrixC.At<double>(2, 1);
        c33 = matrixC.At<double>(2, 2);

        d11 = matrixD.At<double>(0, 0);
        d12 = matrixD.At<double>(0, 1);
        d13 = matrixD.At<double>(0, 2);
        d21 = matrixD.At<double>(1, 0);
        d22 = matrixD.At<double>(1, 1);
        d23 = matrixD.At<double>(1, 2);
        d31 = matrixD.At<double>(2, 0);
        d32 = matrixD.At<double>(2, 1);
        d33 = matrixD.At<double>(2, 2);
    }

    public IList<Point> GetPath(ReadOnlySpan<Point> path, int width, int height)
    {
        int rectWidth = width / 2;
        int rectHeight = height / 2;
        Rect leftTop = new(0, 0, rectWidth, rectHeight);
        Rect rightTop = new(width / 2, 0, rectWidth, rectHeight);
        Rect leftBottom = new(0, height / 2, rectWidth, rectHeight);
        Rect rightBottom = new(width / 2, height / 2, rectWidth, rectHeight);
        if (path == null || path.Length == 0) throw new ArgumentException($"{nameof(path)}不能为空");

        List<Point> result = new(path.Length);
        for (int i = 0; i < path.Length; i++)
        {
            var p = path[i];
            if (leftTop.Contains(p))
            {
                p = WarpPerspective(p, a11, a12, a13, a21, a22, a23, a31, a32, a33);
            }
            else if (rightTop.Contains(p))
            {
                p = WarpPerspective(p, b11, b12, b13, b21, b22, b23, b31, b32, b33);
            }
            else if (leftBottom.Contains(p))
            {
                p = WarpPerspective(p, c11, c12, c13, c21, c22, c23, c31, c32, c33);
            }
            else if (rightBottom.Contains(p))
            {
                p = WarpPerspective(p, d11, d12, d13, d21, d22, d23, d31, d32, d33);
            }
            result[i] = p;
        }

        return result;
    }

    static Point WarpPerspective(Point point, double m11, double m12, double m13, double m21, double m22, double m23, double m31, double m32, double m33)
    {
        (double x, double y) = (point.X, point.Y);
        point.X = (int)((m11 * x + m12 * y + m13) / (m31 * x + m32 * y + m33));
        point.Y = (int)((m21 * x + m22 * y + m23) / (m31 * x + m32 * y + m33));
        return point;
    }
}

