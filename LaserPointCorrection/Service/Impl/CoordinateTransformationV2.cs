using OpenCvSharp;

/// <summary>
/// 仿射变换,四区域
/// </summary>
public class CoordinateTransformationV2 : ICoordinateTransformation
{
    int width, height;
    readonly double a11, a12, a21, a22;
    readonly double aX, aY;

    readonly double b11, b12, b21, b22;
    readonly double bX, bY;

    readonly double c11, c12, c21, c22;
    readonly double cX, cY;

    readonly double d11, d12, d21, d22;
    readonly double dX, dY;

    public CoordinateTransformationV2(IList<Point> from, IList<Point> to, int width, int height)
    {
        this.width = width;
        this.height = height;
        IEnumerable<Point2f> fromLeftTop = [from[0], from[1], from[3], from[4]];
        IEnumerable<Point2f> fromRightTop = [from[1], from[2], from[4], from[5]];
        IEnumerable<Point2f> fromLeftBottom = [from[3], from[4], from[6], from[7]];
        IEnumerable<Point2f> fromRightBottom = [from[4], from[5], from[7], from[8]];

        IEnumerable<Point2f> toLeftTop = [to[0], to[1], to[3], to[4]];
        IEnumerable<Point2f> toRightTop = [to[1], to[2], to[4], to[5]];
        IEnumerable<Point2f> toLeftBottom = [to[3], to[4], to[6], to[7]];
        IEnumerable<Point2f> toRightBottom = [to[4], to[5], to[7], to[8]];

        using Mat matrixA = Cv2.GetAffineTransform(fromLeftTop, toLeftTop);
        using Mat matrixB = Cv2.GetAffineTransform(fromRightTop, fromRightTop);
        using Mat matrixC = Cv2.GetAffineTransform(fromLeftBottom, toLeftBottom);
        using Mat matrixD = Cv2.GetAffineTransform(fromRightBottom, toRightBottom);

        //using Mat matrixA = Cv2.EstimateAffine2D(InputArray.Create(fromLeftTop), InputArray.Create(toLeftTop))!;
        //using Mat matrixB = Cv2.EstimateAffine2D(InputArray.Create(fromRightTop), InputArray.Create(fromRightTop))!;
        //using Mat matrixC = Cv2.EstimateAffine2D(InputArray.Create(fromLeftBottom), InputArray.Create(toLeftBottom))!;
        //using Mat matrixD = Cv2.EstimateAffine2D(InputArray.Create(fromRightBottom), InputArray.Create(toRightBottom))!;

        a11 = matrixA.At<double>(0, 0);
        a12 = matrixA.At<double>(0, 1);
        a21 = matrixA.At<double>(1, 0);
        a22 = matrixA.At<double>(1, 1);
        aX = matrixA.At<double>(0, 2);
        aY = matrixA.At<double>(1, 2);

        b11 = matrixB.At<double>(0, 0);
        b12 = matrixB.At<double>(0, 1);
        b21 = matrixB.At<double>(1, 0);
        b22 = matrixB.At<double>(1, 1);
        bX = matrixB.At<double>(0, 2);
        bY = matrixB.At<double>(1, 2);

        c11 = matrixC.At<double>(0, 0);
        c12 = matrixC.At<double>(0, 1);
        c21 = matrixC.At<double>(1, 0);
        c22 = matrixC.At<double>(1, 1);
        cX = matrixC.At<double>(0, 2);
        cY = matrixC.At<double>(1, 2);

        d11 = matrixD.At<double>(0, 0);
        d12 = matrixD.At<double>(0, 1);
        d21 = matrixD.At<double>(1, 0);
        d22 = matrixD.At<double>(1, 1);
        dX = matrixD.At<double>(0, 2);
        dY = matrixD.At<double>(1, 2);
    }

    public IList<Point> GetPath(IList<Point> path)
    {
        int rectWidth = width / 2;
        int rectHeight = height / 2;
        Rect leftTop = new(0, 0, rectWidth, rectHeight);
        Rect rightTop = new(width / 2, 0, rectWidth, rectHeight);
        Rect leftBottom = new(0, height / 2, rectWidth, rectHeight);
        Rect rightBottom = new(width / 2, height / 2, rectWidth, rectHeight);
        if (path == null || path.Count == 0) throw new ArgumentException($"{nameof(path)}不能为空");

        List<Point> result = new(path.Count);
        for (var i = 0; i < path.Count; i++)
        {
            var point = path[i];
            if (leftTop.Contains(point))
            {
                point = WarpAffine(point, a11, a12, a21, a22, aX, aY);
            }
            else if (rightTop.Contains(point))
            {
                point = WarpAffine(point, b11, b12, b21, b22, bX, bY);
            }
            else if (leftBottom.Contains(point))
            {
                point = WarpAffine(point, c11, c12, c21, c22, cX, cY);
            }
            else if (rightBottom.Contains(point))
            {
                point = WarpAffine(point, d11, d12, d21, d22, dX, dY);
            }
            result.Add(point);
        }
        return path;
    }

    static Point WarpAffine(Point point, double m11, double m12, double m21, double m22, double tX, double tY)
    {
        int x = point.X;
        int y = point.Y;
        point.X = (int)(m11 * x + m12 * y + tX);
        point.Y = (int)(m21 * x + m22 * y + tY);
        return point;
    }

    public IList<Point> CorrectionCoordinate(IList<Point> path) => GetPath(path);
}
