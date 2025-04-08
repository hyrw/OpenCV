using System.Diagnostics;
using CRCore;
using OpenCvSharp;

namespace ShortCircuitDetect.Lib;

public class RegionFillTest
{
    public static void Test()
    {
        string file = @"C:\Users\Coder\Desktop\填充路径.png";
        // string file = @"C:\Users\Coder\Desktop\圆.png";
        // string file = @"C:\Users\Coder\Desktop\多孔洞.png";
        string saveFile = string.Empty;
        using var image = Cv2.ImRead(file, ImreadModes.Grayscale);
        Stopwatch stopwatch = new();


        // stopwatch.Restart();
        // using Mat horizontal = TestRegionFillHorizontal(image);
        // Console.WriteLine($"耗时：{stopwatch.Elapsed.TotalMilliseconds}ms");
        // stopwatch.Stop();
        // saveFile = @"C:\Users\Coder\Desktop\填充路径_水平.png";
        // Cv2.ImWrite(saveFile, horizontal);
        //
        // stopwatch.Restart();
        // using Mat vertical = TestRegionFillVertical(image);
        // Console.WriteLine($"耗时：{stopwatch.Elapsed.TotalMilliseconds}ms");
        // stopwatch.Stop();
        // saveFile = @"C:\Users\Coder\Desktop\填充路径_垂直.png";
        // Cv2.ImWrite(saveFile, vertical);
        //
        // stopwatch.Restart();
        // using Mat horizontalSnake = TestRegionFillHorizontalSnake(image);
        // Console.WriteLine($"耗时：{stopwatch.Elapsed.TotalMilliseconds}ms");
        // stopwatch.Stop();
        // saveFile = @"C:\Users\Coder\Desktop\填充路径_水平弓形.png";
        // Cv2.ImWrite(saveFile, horizontalSnake);
        //
        // stopwatch.Restart();
        // using Mat verticalSnake = TestRegionFillVerticalSnake(image);
        // Console.WriteLine($"耗时：{stopwatch.Elapsed.TotalMilliseconds}ms");
        // stopwatch.Stop();
        // saveFile = @"C:\Users\Coder\Desktop\填充路径_垂直弓形.png";
        // Cv2.ImWrite(saveFile, verticalSnake);

        stopwatch.Restart();
        using Mat outsideRing = TestRegionFillOutsideRing(image, 5);
        Console.WriteLine($"耗时：{stopwatch.Elapsed.TotalMilliseconds}ms");
        stopwatch.Stop();
        saveFile = @"C:\Users\Coder\Desktop\填充路径_外向内.png";
        Cv2.ImWrite(saveFile, outsideRing);

        stopwatch.Restart();
        using Mat insideRing = TestRegionFillInsideRing(image, 5);
        Console.WriteLine($"耗时：{stopwatch.Elapsed.TotalMilliseconds}ms");
        stopwatch.Stop();
        saveFile = @"C:\Users\Coder\Desktop\填充路径_内向外.png";
        Cv2.ImWrite(saveFile, insideRing);

    }

    static Mat TestRegionFillHorizontal(Mat mat)
    {
        var result = RegionFillHelper.GetFillPath(mat, 5, RegionFillHelper.FillType.Horizontal);
        var color = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
        foreach (var line in result)
        {
            Queue<Point> queue = new(line);
            while (queue.Count >= 2)
            {
                var p1 = queue.Dequeue();
                var p2 = queue.Dequeue();
                if (p2.X > p1.X)
                {
                    Cv2.Line(color, p1, p2, Scalar.DodgerBlue);
                }
                else
                {
                    Cv2.Line(color, p1, p2, Scalar.DarkGoldenrod);
                }
            }
        }
        return color;
    }

    static Mat TestRegionFillVertical(Mat mat)
    {
        var result = RegionFillHelper.GetFillPath(mat, 5, RegionFillHelper.FillType.Vertical);
        var color = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
        foreach (var line in result)
        {
            Queue<Point> queue = new(line);
            while (queue.Count >= 2)
            {
                var p1 = queue.Dequeue();
                var p2 = queue.Dequeue();
                if (p2.Y < p1.Y)
                {
                    Cv2.Line(color, p1, p2, Scalar.DodgerBlue);
                }
                else
                {
                    Cv2.Line(color, p1, p2, Scalar.DarkGoldenrod);
                }
            }
        }
        return color;
    }

    static Mat TestRegionFillHorizontalSnake(Mat mat)
    {
        int fillStep = 5;
        var result = RegionFillHelper.GetFillPath(mat, fillStep, RegionFillHelper.FillType.HorizontalSnake);
        var color = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
        foreach (var line in result)
        {
            for (int i = 1; i < line.Count; i++)
            {
                if (line[i].Y > line[i - 1].Y + fillStep)
                {
                    throw new Exception("路径错误");
                }

            }
            Scalar lineColor = Scalar.RandomColor();
            Scalar startColor = Scalar.Red;
            Scalar endColor = Scalar.Green;
            IList<IList<Point>> points = [line];
            Cv2.Polylines(color, points, false, lineColor);
            Point start = line.First();
            Point end = line.Last();
            color.At<Vec3b>(end.Y, end.X) = new Vec3b((byte)endColor.Val0, (byte)endColor.Val1, (byte)endColor.Val2);
            color.At<Vec3b>(start.Y, start.X) = new Vec3b((byte)startColor.Val0, (byte)startColor.Val1, (byte)startColor.Val2);
        }
        return color;
    }

    static Mat TestRegionFillVerticalSnake(Mat mat)
    {
        int fillStep = 5;
        var result = RegionFillHelper.GetFillPath(mat, fillStep, RegionFillHelper.FillType.VerticalSnake);
        var color = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
        foreach (var line in result)
        {
            for (int i = 1; i < line.Count; i++)
            {
                if (line[i].X > line[i - 1].X + fillStep)
                {
                    throw new Exception("路径错误");
                }

            }
            Scalar lineColor = Scalar.RandomColor();
            Scalar startColor = Scalar.Red;
            Scalar endColor = Scalar.Green;
            IList<IList<Point>> points = [line];
            Cv2.Polylines(color, points, false, lineColor);
            Point start = line.First();
            Point end = line.Last();
            color.At<Vec3b>(end.Y, end.X) = new Vec3b((byte)endColor.Val0, (byte)endColor.Val1, (byte)endColor.Val2);
            color.At<Vec3b>(start.Y, start.X) = new Vec3b((byte)startColor.Val0, (byte)startColor.Val1, (byte)startColor.Val2);
        }
        return color;
    }

    static Mat TestRegionFillOutsideRing(Mat mat, int fillStep)
    {
        Scalar red = Scalar.Red;
        Scalar green = Scalar.Green;
        var result = RegionFillHelper.GetFillPath(mat, fillStep, RegionFillHelper.FillType.OutsideRing);
        var color = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
        foreach (var line in result)
        {
            if (line.Count == 2)
            {
                Point start = line.First();
                Point end = line.Last();
                Cv2.Polylines(color, [line], false, Scalar.LightCoral);
                color.At<Vec3b>(start.Y, start.X) = new Vec3b((byte)red.Val0, (byte)red.Val1, (byte)red.Val2);
                color.At<Vec3b>(end.Y, end.X) = new Vec3b((byte)green.Val0, (byte)green.Val1, (byte)green.Val2);
                continue;
            }
            Scalar lineColor = Scalar.RandomColor();
            Scalar startColor = Scalar.Red;
            Scalar endColor = Scalar.Green;
            IList<IList<Point>> points = [line];
            Cv2.Polylines(color, points, false, lineColor);
            Cv2.ImShow("Outside Ring", color);
            Cv2.WaitKey();
        }
        return color;
    }

    static Mat TestRegionFillInsideRing(Mat mat, int fillStep)
    {
        Scalar red = Scalar.Red;
        Scalar green = Scalar.Green;
        var result = RegionFillHelper.GetFillPath(mat, fillStep, RegionFillHelper.FillType.InsideRing);
        var color = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
        foreach (var line in result)
        {
            if (line.Count == 2)
            {
                Point start = line.First();
                Point end = line.Last();
                Cv2.Polylines(color, [line], false, Scalar.LightCoral);
                color.At<Vec3b>(start.Y, start.X) = new Vec3b((byte)red.Val0, (byte)red.Val1, (byte)red.Val2);
                color.At<Vec3b>(end.Y, end.X) = new Vec3b((byte)green.Val0, (byte)green.Val1, (byte)green.Val2);
                continue;
            }
            Scalar lineColor = Scalar.RandomColor();
            Scalar startColor = Scalar.Red;
            Scalar endColor = Scalar.Green;
            IList<IList<Point>> points = [line];
            Cv2.Polylines(color, points, false, lineColor);
            Cv2.ImShow("Inside Ring", color);
            Cv2.WaitKey();
        }
        return color;
    }
}
