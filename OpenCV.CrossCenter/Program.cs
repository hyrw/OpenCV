using OpenCvSharp;
using OpenCvSharp.XImgProc;
using ShortCircuitDetect.Lib;

string file = @"C:/Users/Coder/Pictures/calibration_images/cross2.png";

// 读取图像
using Mat img = Cv2.ImRead(file, ImreadModes.Grayscale);

// 应用中值模糊
using Mat imgBlur = new Mat();
Cv2.MedianBlur(img, imgBlur, 5);

// 二值化
using Mat imgThresh = new Mat();
Cv2.Threshold(imgBlur, imgThresh, 180, 255, ThresholdTypes.Binary);

// 形态学开运算
using Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
using Mat imgOpen = new Mat();
Cv2.MorphologyEx(imgThresh, imgOpen, MorphTypes.Open, kernel);

// 骨架化
using Mat skel = Skeletonize(imgOpen);
CVOperation.ImShow("skel", skel);
// Cv2.ImWrite(@"C:\Users\Coder\AppData\Local\Temp\opencv\skel.png", skel);
Cv2.WaitKey();

// 检测交叉点
IList<Point> crossPoints = DetectCrossCenters(skel);

// 显示结果
using Mat imgColor = Cv2.ImRead(file);
foreach (var p in crossPoints)
{
    Cv2.Circle(imgColor, p.X, p.Y, 1, Scalar.Red, -1);
}
Cv2.ImShow("Cross Points on Original", imgColor);
Cv2.WaitKey();

static IList<Point> DetectCrossCenters(Mat skeleton)
{
    // 确保输入是二值图像
    using Mat binary = new Mat();
    Cv2.Threshold(skeleton, binary, 127, 255, ThresholdTypes.Binary);

    // 邻域核
    byte[,] neighborKernel = new byte[3, 3]
    {
        {1,1,1},
        {1,0,1},
        {1,1,1}
    };

    // 计算连接数
    using Mat neighborCount = new Mat(); // 每一点表示当前点的邻居数量
    Cv2.Filter2D(binary / 255, neighborCount, -1, InputArray.Create(neighborKernel));

    // 检测交叉点
    return FindCrossPoints(neighborCount, binary);
}

static Mat Skeletonize(Mat inputImage)
{
    // Opencv的细化提取没有Skimage的好，线段接触到图像边缘时会变成T，Skimage的不会
    // 可以考虑将图像的四边缘置黑
    Mat skel = new(inputImage.Size(), inputImage.Type());
    CvXImgProc.Thinning(inputImage, skel, ThinningTypes.ZHANGSUEN);
    return skel;
}

static IList<Point> FindCrossPoints(Mat neighborCount, Mat binary)
{
    int height = binary.Rows;
    int width = binary.Cols;
    List<Point> points = [];

    for (int i = 0; i < height; i++)
    {
        for (int j = 0; j < width; j++)
        {
            if (neighborCount.At<byte>(i, j) >= 3 && binary.At<byte>(i, j) > 0)
            {
                points.Add(new Point(j, i));
            }
        }
    }
    return points;
}



// var lines = Cv2.HoughLinesP(edges, 1, Cv2.PI / 180, 50);
// foreach (var l in lines)
// {
//     Cv2.Line(srcImage, l.P1, l.P2, Scalar.Red);
// }

// Size size = srcImage.Size();
// LineSegmentPolar[] linePolars = Cv2.HoughLines(edges, 1, Cv2.PI / 180, 50);
// List<LineSegmentPoint> lines = [];
// List<Point2f> points = [];
// foreach (var i in linePolars)
// {
//     var rho = i.Rho;
//     var theta = i.Theta;
//     if (theta < Cv2.PI / 4 || theta > 3 * Cv2.PI / 4)
//     {
//         // 接近于垂直线条
//         Point point1 = new Point(rho / Math.Cos(theta), 0);
//         Point point2 = new Point((size.Height * Math.Sin(theta)) / Math.Cos(theta), size.Height);
//         Cv2.Line(srcImage, point1, point2, Scalar.Red);
//     }
//     else
//     {
//         // 接近于水平线条 r=xcox(theta)+ysin(theta)
//         Point point1 = new Point(0, rho / Math.Sin(theta));
//         Point point2 = new Point(size.Width, (size.Width * Math.Cos(theta)) / Math.Sin(theta));
//         Cv2.Line(srcImage, point1, point2, Scalar.Green);
//     }
// }
//
// CVOperation.ImShow("lines", srcImage);
//
// Cv2.WaitKey();
