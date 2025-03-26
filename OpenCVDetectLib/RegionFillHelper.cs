using OpenCvSharp;
using ShortCircuitDetect.Lib;

namespace CRCore;

public sealed class RegionFillHelper
{
    public enum FillType
    {
        Horizontal,
        Vertical,
        HorizontalSnake,
        VerticalSnake,
        OutsideRing,
        InsideRing,
    }

    /// <summary>
    /// 获取输入图像非零部分的填充路径
    /// </summary>
    /// <param name="monochrome">输入的二值图</param>
    /// <param name="fillStep">填充线间距</param>
    /// <param name="fillType">填充方式</param>
    /// <returns>返回填充填充路径，一个包含点集的数组，每个点集表示一条折线，点集中相邻的两点表示一条直线</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IList<IList<Point>> GetFillPath(Mat monochrome, int fillStep, FillType fillType) => fillType switch
    {
        FillType.Horizontal => GetHorizontalFillPath(monochrome, fillStep),
        FillType.Vertical => GetVerticalFillPath(monochrome, fillStep),
        FillType.HorizontalSnake => GetHorizontalSnakeFillPath(monochrome, fillStep),
        FillType.VerticalSnake => GetVerticalSnakeFillPath(monochrome, fillStep),
        FillType.OutsideRing => GetOutsideRingFillPath(monochrome, fillStep),
        FillType.InsideRing => GetInsideRingFillPath(monochrome, fillStep),
        _ => throw new ArgumentOutOfRangeException(nameof(fillType))
    };


    public static IList<IList<Point>> GetHorizontalFillPath(Mat monochrome, int fillStep)
    {
        List<IList<Point>> result = new(200);

        Point? start = default;
        Point? end = default;
        bool needTurnRound = false;

        using var shapeExtractor = new ShapeExtractor(monochrome);
        while (shapeExtractor.HasNext)
        {
            using var shape = shapeExtractor.GetNextShape();
            for (int y = 0; y < shape.Rows; y += fillStep)
            {
                for (int x = 0; x < shape.Cols; x++)
                {
                    var isZero = shape.At<byte>(y, x) == 0;
                    // 遇到0值，说明线段结束了
                    if (isZero && start.HasValue && end.HasValue)
                    {
                        List<Point> line = new(2);
                        if (needTurnRound)
                        {
                            line.Add(end.Value);
                            line.Add(start.Value);
                        }
                        else
                        {
                            line.Add(start.Value);
                            line.Add(end.Value);
                        }
                        result.Add(line);
                        start = default;
                        end = default;
                        continue;
                    }

                    if (!isZero)
                    {
                        start ??= new Point(x, y);
                        end = new Point(x, y);
                    }
                }
                // 行遍历结束才需要折回头
                needTurnRound = !needTurnRound;
            }
        }

        return result;
    }

    public static IList<IList<Point>> GetVerticalFillPath(Mat monochrome, int fillStep)
    {
        List<IList<Point>> result = new(200);

        Point? start = default;
        Point? end = default;
        bool needTurnRound = false;

        using var shapeExtractor = new ShapeExtractor(monochrome);
        while (shapeExtractor.HasNext)
        {
            using var shape = shapeExtractor.GetNextShape();
            for (int x = 0; x < shape.Cols; x += fillStep)
            {
                for (int y = 0; y < shape.Rows; y++)
                {
                    var isZero = shape.At<byte>(y, x) == 0;
                    // 遇到0值，说明线段结束了
                    if (isZero && start.HasValue && end.HasValue)
                    {
                        List<Point> line = new(2);
                        if (needTurnRound)
                        {
                            line.Add(end.Value);
                            line.Add(start.Value);
                        }
                        else
                        {
                            line.Add(start.Value);
                            line.Add(end.Value);
                        }
                        result.Add(line);
                        start = default;
                        end = default;
                        continue;
                    }

                    if (!isZero)
                    {
                        start ??= new Point(x, y);
                        end = new Point(x, y);
                    }
                }
                // 列遍历结束才需要折回头
                needTurnRound = !needTurnRound;
            }
        }

        return result;
    }

    public static IList<IList<Point>> GetHorizontalSnakeFillPath(Mat monochrome, int fillStep)
    {
        using var shapeExtractor = new ShapeExtractor(monochrome);

        IList<IList<Point>> result = [];
        while (shapeExtractor.HasNext)
        {
            using var shape = shapeExtractor.GetNextShape();
            var allRowLines = GetHorizontalAllLines(shape, fillStep)
                .OrderBy(linesOfRow => linesOfRow.First().start.Y).ToList();

            foreach (var line in GetHorizontalSnakeLine(allRowLines, fillStep))
            // foreach (var line in GetHorizontalSnakeLineV2(allRowLines, fillStep))
            {
                if (line.Count != 0)
                {
                    result.Add(line);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 获取所有水平方向的弓形路径
    /// 不会有过多的线段，但是会多一些飞线
    /// </summary>
    /// <param name="allRowLines"></param>
    /// <param name="fillStep"></param>
    /// <returns></returns>
    static IList<IList<Point>> GetHorizontalSnakeLine(IList<IList<(Point start, Point end)>> allRowLines, int fillStep)
    {
        if (allRowLines is null || allRowLines.Count == 0) return [];

        IList<IList<Point>> result = [];
        while (allRowLines.Count > 0)
        {
            IList<Point> line = [];
            (Point start, Point end)? lastLine = default;
            bool flag = false;
            foreach (var rowLines in allRowLines)
            {
                if (null == lastLine)
                {
                    var first = rowLines.First();
                    lastLine = first;
                    rowLines.Remove(first);
                    line.Add(first.start);
                    line.Add(first.end);
                }
                else
                {
                    // 上一个线段和当前线段已经有了，需要判断是否掉头，增加转折点
                    var overlapLine = TryGetHorizontalOverlapLine(lastLine.Value, rowLines);
                    if (overlapLine is null ||
                        lastLine.Value.start.Y + fillStep != overlapLine.Value.start.Y) // 线段Y方向不相连，不处理
                    {
                        lastLine = default;
                        break;
                    };

                    rowLines.Remove(overlapLine.Value);
                    if (flag)
                    {
                        line.Add(lastLine.Value.end);
                        line.Add(overlapLine.Value.end);
                        line.Add(overlapLine.Value.end);
                        line.Add(overlapLine.Value.start);
                    }
                    else
                    {
                        line.Add(lastLine.Value.start);
                        line.Add(overlapLine.Value.start);
                        line.Add(overlapLine.Value.start);
                        line.Add(overlapLine.Value.end);
                    }
                    lastLine = overlapLine;
                }
                flag = !flag;
            }
            if (line.Count != 0)
            {
                result.Add(line);
            }
            allRowLines = allRowLines.Where(line => line.Count > 0).ToList();
        }
        return result;
    }

    /// <summary>
    /// 获取所有水平方向的弓形路径
    /// 不会有过多的孔洞，但是会线段会增多
    /// </summary>
    /// <param name="allRowLines"></param>
    /// <param name="fillStep"></param>
    /// <returns></returns>
    static IList<IList<Point>> GetHorizontalSnakeLineV2(IList<IList<(Point start, Point end)>> allRowLines, int fillStep)
    {
        if (allRowLines is null || allRowLines.Count == 0) return [];

        IList<IList<Point>> result = [];
        while (allRowLines.Count > 0)
        {
            IList<Point> line = [];
            (Point start, Point end)? lastLine = default;
            bool flag = false;
            foreach (var rowLines in allRowLines)
            {
                if (null == lastLine)
                {
                    var first = rowLines.First();
                    lastLine = first;
                    rowLines.Remove(first);
                    line.Add(first.start);
                    line.Add(first.end);
                }
                else
                {
                    // 上一个线段和当前线段已经有了，需要判断是否掉头，增加转折点
                    var overlapLine = TryGetHorizontalOverlapLine(lastLine.Value, rowLines);
                    if (overlapLine is null ||
                        lastLine.Value.start.Y + fillStep != overlapLine.Value.start.Y) // 线段Y方向不相连，不处理
                    {
                        lastLine = default;
                        break;
                    };

                    int min, max, overlapLineX;

                    if (flag)
                    {
                        min = lastLine.Value.end.X - fillStep;
                        max = lastLine.Value.end.X + fillStep;
                        overlapLineX = overlapLine.Value.end.X;
                        if (overlapLineX < min || overlapLineX > max) continue;
                        line.Add(lastLine.Value.end);
                        line.Add(overlapLine.Value.end);
                        line.Add(overlapLine.Value.end);
                        line.Add(overlapLine.Value.start);
                        rowLines.Remove(overlapLine.Value);
                    }
                    else
                    {
                        min = lastLine.Value.start.X - fillStep;
                        max = lastLine.Value.start.X + fillStep;
                        overlapLineX = overlapLine.Value.start.X;
                        if (overlapLineX < min || overlapLineX > max) continue;
                        line.Add(lastLine.Value.start);
                        line.Add(overlapLine.Value.start);
                        line.Add(overlapLine.Value.start);
                        line.Add(overlapLine.Value.end);
                        rowLines.Remove(overlapLine.Value);
                    }
                    lastLine = overlapLine;
                }
                flag = !flag;
            }
            if (line.Count != 0)
            {
                result.Add(line);
            }
            allRowLines = allRowLines.Where(line => line.Count > 0).ToList();
        }
        return result;
    }

    public static IList<IList<Point>> GetVerticalSnakeFillPath(Mat monochrome, int fillStep)
    {
        using var shapeExtractor = new ShapeExtractor(monochrome);

        IList<IList<Point>> result = [];
        while (shapeExtractor.HasNext)
        {
            using var shape = shapeExtractor.GetNextShape();
            var allColLines = GetVerticalAllLines(shape, fillStep)
                .OrderBy(linesOfRow => linesOfRow.First().start.X).ToList();

            foreach (var line in GetVerticalSnakeLine(allColLines, fillStep))
            // foreach (var line in GetVerticalSnakeLineV2(allColLines, fillStep))
            {
                if (line.Count != 0)
                {
                    result.Add(line);
                }
            }
        }

        return result;
    }

    static IList<IList<Point>> GetVerticalSnakeLineV2(List<IList<(Point start, Point end)>> allColLines, int fillStep)
    {
        if (allColLines is null || allColLines.Count == 0) return [];

        IList<IList<Point>> result = [];
        while (allColLines.Count > 0)
        {
            IList<Point> line = [];
            (Point start, Point end)? lastLine = default;
            bool flag = false;
            foreach (var colLines in allColLines)
            {
                if (null == lastLine)
                {
                    var first = colLines.First();
                    lastLine = first;
                    colLines.Remove(first);
                    line.Add(first.start);
                    line.Add(first.end);
                }
                else
                {
                    // 上一个线段和当前线段已经有了，需要判断是否掉头，增加转折点
                    // NOTE: 如果两个转折点距离过远，或许可以考虑截断线段
                    var overlapLine = TryGetVerticalOverlapLine(lastLine.Value, colLines);
                    if (overlapLine is null ||
                        lastLine.Value.start.X + fillStep != overlapLine.Value.start.X) // 线段X方向不相邻，不处理
                    {
                        lastLine = default;
                        break;
                    };

                    int min, max, overlapLineY;

                    if (flag)
                    {
                        min = lastLine.Value.end.Y - fillStep;
                        max = lastLine.Value.end.Y + fillStep;
                        overlapLineY = overlapLine.Value.end.Y;
                        if (overlapLineY < min || overlapLineY > max) continue;
                        line.Add(lastLine.Value.end);
                        line.Add(overlapLine.Value.end);
                        line.Add(overlapLine.Value.end);
                        line.Add(overlapLine.Value.start);
                        colLines.Remove(overlapLine.Value);
                    }
                    else
                    {
                        min = lastLine.Value.start.Y - fillStep;
                        max = lastLine.Value.start.Y + fillStep;
                        overlapLineY = overlapLine.Value.start.Y;
                        if (overlapLineY < min || overlapLineY > max) continue;
                        line.Add(lastLine.Value.start);
                        line.Add(overlapLine.Value.start);
                        line.Add(overlapLine.Value.start);
                        line.Add(overlapLine.Value.end);
                        colLines.Remove(overlapLine.Value);
                    }
                    lastLine = overlapLine;
                }
                flag = !flag;
            }
            if (line.Count != 0)
            {
                result.Add(line);
            }
            allColLines = allColLines.Where(line => line.Count > 0).ToList();
        }
        return result;
    }

    private static IList<IList<Point>> GetVerticalSnakeLine(List<IList<(Point start, Point end)>> allColLines, int fillStep)
    {
        if (allColLines is null || allColLines.Count == 0) return [];

        IList<IList<Point>> result = [];
        while (allColLines.Count > 0)
        {
            IList<Point> line = [];
            (Point start, Point end)? lastLine = default;
            bool flag = false;
            foreach (var colLines in allColLines)
            {
                if (null == lastLine)
                {
                    var first = colLines.First();
                    lastLine = first;
                    colLines.Remove(first);
                    line.Add(first.start);
                    line.Add(first.end);
                }
                else
                {
                    // 上一个线段和当前线段已经有了，需要判断是否掉头，增加转折点
                    var overlapLine = TryGetVerticalOverlapLine(lastLine.Value, colLines);
                    if (overlapLine is null ||
                        lastLine.Value.start.X + fillStep != overlapLine.Value.start.X) // 线段X方向不相邻，不处理
                    {
                        lastLine = default;
                        break;
                    };

                    colLines.Remove(overlapLine.Value);
                    if (flag)
                    {
                        line.Add(lastLine.Value.end); // 两个转折点
                        line.Add(overlapLine.Value.end);
                        line.Add(overlapLine.Value.end);
                        line.Add(overlapLine.Value.start);
                    }
                    else
                    {
                        line.Add(lastLine.Value.start);
                        line.Add(overlapLine.Value.start);
                        line.Add(overlapLine.Value.start);
                        line.Add(overlapLine.Value.end);
                    }
                    lastLine = overlapLine;
                }
                flag = !flag;
            }
            if (line.Count != 0)
            {
                result.Add(line);
            }
            allColLines = allColLines.Where(line => line.Count > 0).ToList();
        }
        return result;
    }

    static (Point start, Point end)? TryGetVerticalOverlapLine((Point start, Point end) line, IList<(Point start, Point end)> lines)
    {
        if (lines is null || lines.Count == 0) throw new ArgumentException("线段列表不能为空");

        var orderedLines = lines.OrderBy(l => l.start.Y);
        try
        {
            return orderedLines.First(l =>
            {
                if (l.start.Y > line.end.Y || l.end.Y < line.start.Y)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            });
        }
        catch
        {
            return default;
        }
    }

    public static IList<IList<Point>> GetOutsideRingFillPath(Mat monochrome, int fillStep)
    {
        var result = new List<IList<Point>>();
        var shapePath = new List<IList<Point>>(); // 单个连通域的所有内缩路径
        using var shapeExtractor = new ShapeExtractor(monochrome);
        var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(fillStep, fillStep));

        Queue<Mat> shapes = new();

        // 先提取连通域为单个图像
        while (shapeExtractor.HasNext)
        {
            shapes.Enqueue(shapeExtractor.GetNextShape());
        }

        while (shapes.Count > 0)
        {
            Mat mat = shapes.Dequeue();
            while (IsSingleConnectedComponents(mat))
            {
                IList<Point> externalContour = GetExternalContour(mat, fillStep);

                if (null != externalContour && externalContour.Count > 0)
                {
                    shapePath.Add(externalContour);
                }
                var eroded = ErodeExternal(mat, kernel);
                if (IsSingleConnectedComponents(eroded))
                {
                    mat.Dispose();
                    mat = eroded;
                }
                else
                {
                    using var extractor = new ShapeExtractor(eroded);
                    while (extractor.HasNext)
                    {
                        // NOTE: 可以考虑做一下连通域面积过滤，避免使用很小的连通域生成内缩路径
                        shapes.Enqueue(extractor.GetNextShape());
                    }
                    break;
                }
            }
            mat.Dispose();
            /* 
             * NOTE: Debug
            IList<Point> lastPath = shapePath[0];
            for (int i = 1; i < shapePath.Count; i++)
            {
                Point start = lastPath.Last();
                Point end = shapePath[i].First();
                result.Add(lastPath);
                result.Add([start, end]);
                lastPath = shapePath[i];
            }
            result.Add(lastPath);
             */

            // 拼接路径
            if (shapePath.Count == 0) continue;
            List<Point> path = [];
            IList<Point> lastPath = shapePath[0];
            for (int i = 1; i < shapePath.Count; i++)
            {
                Point start = lastPath.Last();
                Point end = shapePath[i].First();
                path.AddRange(lastPath);
                path.Add(start);
                path.Add(end);
                lastPath = shapePath[i];
            }
            path.AddRange(lastPath);
            result.Add(path);
            shapePath.Clear();
        }

        return result;
    }

    private static IList<Point> GetExternalContour(Mat img, int fillStep)
    {
        var kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(fillStep, fillStep));
        List<Point> result = new();


        // 未腐蚀前的轮廓
        Cv2.FindContours(img, out var srcContours, out var srcHierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
        using var eroded = ErodeExternal(img, kernel);
        Cv2.FindContours(eroded, out var erodedContours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);

        if (srcContours.Length == 1 || erodedContours.Length == 1)
        {
            result.AddRange(srcContours[0]);
        }

        return result;
    }

    public static IList<IList<Point>> GetInsideRingFillPath(Mat monochrome, int fillStep)
    {
        var result = new List<IList<Point>>();
        var shapePath = new List<IList<Point>>(); // 单个连通域的所有内缩路径
        using var shapeExtractor = new ShapeExtractor(monochrome);
        var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(fillStep, fillStep));

        Queue<Mat> shapes = new();

        // 先提取连通域为单个图像
        while (shapeExtractor.HasNext)
        {
            shapes.Enqueue(shapeExtractor.GetNextShape());
        }

        while (shapes.Count > 0)
        {
            Mat mat = shapes.Dequeue();
            while (IsSingleConnectedComponents(mat))
            {
                IList<Point> externalContour = GetExternalContour(mat, fillStep);

                if (null != externalContour && externalContour.Count > 0)
                {
                    shapePath.Add(externalContour);
                }
                var eroded = ErodeExternal(mat, kernel);
                if (IsSingleConnectedComponents(eroded))
                {
                    mat.Dispose();
                    mat = eroded;
                }
                else
                {
                    using var extractor = new ShapeExtractor(eroded);
                    while (extractor.HasNext)
                    {
                        // NOTE: 可以考虑做一下连通域面积过滤，避免使用很小的连通域生成内缩路径
                        shapes.Enqueue(extractor.GetNextShape());
                    }
                    break;
                }
            }
            mat.Dispose();
            // /* 
            //  * NOTE: Debug
            // IList<Point> lastPath = shapePath.Last();
            // for (int i = shapePath.Count - 2; i >= 0; i--)
            // {
            //     Point start = lastPath.Last();
            //     Point end = shapePath[i].First();
            //     result.Add(lastPath);
            //     result.Add([start, end]);
            //     lastPath = shapePath[i];
            // }
            // result.Add(lastPath);
            // */

            // 拼接路径
            if (shapePath.Count == 0) continue;
            List<Point> path = [];
            IList<Point> lastPath = shapePath.Last();
            for (int i = shapePath.Count - 2; i >= 0; i--)
            {
                Point start = lastPath.Last();
                Point end = shapePath[i].First();
                path.AddRange(lastPath);
                path.Add(start);
                path.Add(end);
                lastPath = shapePath[i];
            }
            path.AddRange(lastPath);
            result.Add(path);

            shapePath.Clear();
        }

        result.Reverse();
        return result;
    }

    static IList<IList<(Point start, Point end)>> GetHorizontalAllLines(Mat shape, int fillStep)
    {
        List<IList<(Point, Point)>> result = new();

        Point? start = default;
        Point? end = default;

        for (int y = 0; y < shape.Rows; y += fillStep)
        {
            List<(Point, Point)> line = new();
            for (int x = 0; x < shape.Cols; x++)
            {
                var isZero = shape.At<byte>(y, x) == 0;
                // 遇到0值，说明线段结束了
                if (isZero && start.HasValue && end.HasValue)
                {
                    line.Add((start.Value, end.Value));
                    start = default;
                    end = default;
                    continue;
                }

                if (!isZero)
                {
                    start ??= new Point(x, y);
                    end = new Point(x, y);
                }
            }
            if (line.Count != 0)
            {
                result.Add(line);
            }
        }

        return result;
    }

    static IList<IList<(Point start, Point end)>> GetVerticalAllLines(Mat shape, int fillStep)
    {
        IList<IList<(Point start, Point end)>> result = [];

        Point? start = default;
        Point? end = default;

        for (int x = 0; x < shape.Cols; x += fillStep)
        {
            List<(Point, Point)> lines = new();
            for (int y = 0; y < shape.Rows; y++)
            {
                var isZero = shape.At<byte>(y, x) == 0;

                if (!isZero)
                {
                    start ??= new Point(x, y);
                    end = new Point(x, y);
                    continue; // 非零值只做点的记录，不做其它
                }
                // 遇到0值，说明线段结束了
                if (isZero && start.HasValue && end.HasValue)
                {
                    // NOTE: 这里可以考虑做一下排序，外部就不用额外排序

                    lines.Add((start.Value, end.Value));
                    start = default;
                    end = default;
                }
            }
            if (lines.Count != 0)
            {
                result.Add(lines);
            }
        }
        return result;
    }

    static (Point start, Point end)? TryGetHorizontalOverlapLine((Point start, Point end) line, IList<(Point start, Point end)> lines)
    {
        if (lines is null || lines.Count == 0) throw new ArgumentException("线段列表不能为空");

        var orderedLines = lines.OrderBy(l => l.start.X);
        try
        {
            return orderedLines.First(l =>
            {
                if (l.start.X > line.end.X || l.end.X < line.start.X)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            });
        }
        catch
        {
            return default;
        }
    }

    static bool IsSingleConnectedComponents(Mat img)
    {
        using Mat labels = new();
        int maxLabel = Cv2.ConnectedComponents(img, labels);
        return (maxLabel - 1) == 1;
    }

    static Mat ErodeExternal(Mat img, Mat kernel)
    {
        using Mat nonHoleImg = Mat.Zeros(img.Size(), img.Type());
        // 填充孔洞
        Cv2.FindContours(img, out var externalContours, out var externalHierarchy, RetrievalModes.External, ContourApproximationModes.ApproxNone);
        Cv2.FillPoly(nonHoleImg, externalContours, Scalar.White);

        Cv2.Erode(nonHoleImg, nonHoleImg, kernel);

        Mat resutl = Mat.Zeros(img.Size(), img.Type());
        Cv2.BitwiseAnd(nonHoleImg, img, resutl, img);
        return resutl;
    }
}
