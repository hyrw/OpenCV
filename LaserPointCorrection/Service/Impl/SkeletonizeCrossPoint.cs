using OpenCvSharp;
using OpenCvSharp.XImgProc;

/// <summary>
/// 骨架化提取交叉点
/// </summary>
public class SkeletonizeCrossPoint : IIntersectionDetector
{
    static IList<Point> GetCrossPoint(Mat img)
    {
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
        // Cv2.ImWrite(@"C:\Users\Coder\AppData\Local\Temp\opencv\skel.png", skel);
        Cv2.WaitKey();

        // 检测交叉点
        return DetectCrossCenters(skel);
    }

    static Mat Skeletonize(Mat binary)
    {
        // Opencv的细化提取没有Skimage的好，线段接触到图像边缘时会变成T，Skimage的不会
        // 可以考虑将图像的四边缘置黑
        Mat skel = new(binary.Size(), binary.Type());
        CvXImgProc.Thinning(binary, skel, ThinningTypes.ZHANGSUEN);
        return skel;
    }

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

        // 计算当前点邻居数
        using Mat neighborCount = new Mat(); // 每一点表示当前点的邻居数量
        Cv2.Filter2D(binary / 255, neighborCount, -1, InputArray.Create(neighborKernel));

        // 检测交叉点
        return FindCrossPoints(neighborCount, binary);
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
                // 必须是前景，且邻居数>=3
                if (binary.At<byte>(i, j) > 0 && neighborCount.At<byte>(i, j) >= 3)
                {
                    points.Add(new Point(j, i));
                }
            }
        }
        return points;
    }

    public IList<Point> FindCrossPoints(Mat img) => GetCrossPoint(img);
}
