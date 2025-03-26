using OpenCvSharp;

namespace ShortCircuitDetect.Lib;

public static class CVOperation
{

    public static (double score, Point loc) FindMatchLoc(Mat image, Mat templ, TemplateMatchModes matchMode, Mat? mask = default)
    {
        int result_rows = image.Rows - templ.Rows + 1;
        int result_cols = image.Cols - templ.Cols + 1;


        Mat result = new(result_rows, result_cols, MatType.CV_32FC1);

        bool methodAcceptsMsk = TemplateMatchModes.SqDiff == matchMode || TemplateMatchModes.CCorrNormed == matchMode;

        if (methodAcceptsMsk)
        {
            Cv2.MatchTemplate(image, templ, result, matchMode, mask ?? new Mat());
        }
        else
        {
            Cv2.MatchTemplate(image, templ, result, matchMode);
        }

        Cv2.Normalize(result, result, 0, 1, NormTypes.MinMax);

        double score;
        Point matchLoc;

        Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out Point minLoc, out Point maxLoc, new Mat());

        // 这两种方法，最佳匹配值是最低值，其它方法的最佳匹配值是最高值
        if (matchMode == TemplateMatchModes.SqDiff || matchMode == TemplateMatchModes.SqDiffNormed)
        {
            score = 1 - minVal;
            matchLoc = minLoc;
        }
        else
        {
            score = maxVal;
            matchLoc = maxLoc;
        }

        return (score, matchLoc);
    }

    public static (Scalar lower, Scalar upper) GetRGBRange(Scalar color, int threshold = 20)
    {
        var lower = new Scalar(color[0] - threshold, color[1] - threshold, color[2] - threshold);
        var upper = new Scalar(color[0] + threshold, color[1] + threshold, color[2] + threshold);
        return (lower, upper);
    }

    /// <summary>
    /// 腐蚀外部轮廓
    /// </summary>
    /// <param name="img"></param>
    /// <param name="kernel"></param>
    /// <returns></returns>
    public static Mat ErodeExternal(Mat img, Mat kernel)
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

    public static bool IsSingleConnectedComponents(Mat img)
    {
        using Mat labels = new();
        int maxLabel = Cv2.ConnectedComponents(img, labels);
        return (maxLabel - 1) == 1;
    }

    public static void ImShow(string title, Mat mat, double scale = 1)
    {
        Cv2.NamedWindow(title, WindowFlags.KeepRatio);
        Cv2.ResizeWindow(title, (int)(mat.Width * scale), (int)(mat.Height * scale));
        Cv2.ImShow(title, mat);
    }
}
