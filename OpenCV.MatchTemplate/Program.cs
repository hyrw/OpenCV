using OpenCvSharp;

var exit = 113;
int lastKey = default;

bool userMask;
Mat img = new Mat();
Mat templ = new();
Mat mask = new Mat();
Mat result = new Mat();
const string imageWindow = "Source Image";
const string resultWindow = "Result window";

int matchMethod = 4;
int maxTrackbar = 5;

string dir = @"C:\Users\Coder\Workspace\OpencvTest\";

Queue<string> names = new(Directory.EnumerateFiles(dir, "*.png").
    Select(f => f.Split('_').First())
    .Distinct());
string name = string.Empty;
string color = string.Empty;

Cv2.NamedWindow("Trackbars");
string trackbarLabel = "Method: \n 0: SQDIFF \n 1: SQDIFF NORMED \n 2: TM CCORR \n 3: TM CCORR NORMED \n 4: TM COEFF \n 5: TM COEFF NORMED"; ;
Cv2.CreateTrackbar(trackbarLabel, "Trackbars", ref matchMethod, maxTrackbar, MatchingMethod);

while (names.Count != 0)
{

    name = names.Dequeue();

    string imageName = Path.Combine(dir, $"{name}_Cam.png");
    string templateName = Path.Combine(dir, $"{name}_UV.png");
    string maskName = "";

    img = Cv2.ImRead(imageName);
    templ = Cv2.ImRead(templateName);

    if (img.Empty() || templ.Empty())
    {
        throw new Exception("Can't read images");
    }
    Cv2.NamedWindow(imageWindow);
    Cv2.NamedWindow(resultWindow, WindowFlags.AutoSize);
    Cv2.ResizeWindow(imageWindow, new Size(2500, 1920));

    MatchingMethod(0, 0);

    lastKey = Cv2.WaitKey(0);
    if (lastKey == exit) break;
}

void MatchingMethod(int pos, nint userData)
{
    if (img.Empty()) return;

    Mat imgDisplay = new Mat();
    img.CopyTo(imgDisplay);

    int result_rows = img.Rows - templ.Rows + 1;
    int result_cols = img.Cols - templ.Cols + 1;

    result.Create(result_rows, result_cols, MatType.CV_32FC1);

    TemplateMatchModes templateMatchMode = (TemplateMatchModes)matchMethod;

    bool method_accepts_mask = (TemplateMatchModes.SqDiff == templateMatchMode || TemplateMatchModes.CCorrNormed == templateMatchMode);

    if (method_accepts_mask)
    {
        Cv2.MatchTemplate(img, templ, result, templateMatchMode, mask);
    }
    else
    {
        Cv2.MatchTemplate(img, templ, result, templateMatchMode);
    }

    Cv2.Normalize(result, result, 0, 1, NormTypes.MinMax);

    double minVal, maxVal;
    Point minLoc, maxLoc;
    Point matchLoc;

    Cv2.MinMaxLoc(result, out minVal, out maxVal, out minLoc, out maxLoc, new Mat());

    // 这两种方法，最佳匹配值是最低值，其它方法的最佳匹配值是最高值
    if (templateMatchMode == TemplateMatchModes.SqDiff || templateMatchMode == TemplateMatchModes.SqDiffNormed)
    {
        matchLoc = minLoc;
    }
    else
    {
        matchLoc = maxLoc;
    }

    Cv2.PutText(imgDisplay, name, new Point(100, 100), HersheyFonts.Italic, 1, Scalar.White);

    Cv2.Rectangle(imgDisplay, matchLoc, new Point(matchLoc.X + templ.Cols, matchLoc.Y + templ.Rows), Scalar.White, 2, LineTypes.Link8, 0);
    Cv2.Rectangle(result, matchLoc, new Point(matchLoc.X + templ.Cols, matchLoc.Y + templ.Rows), Scalar.All(0), 2, LineTypes.Link8, 0);
    Cv2.PutText(imgDisplay, name, new Point(100, 100), HersheyFonts.Italic, 1, Scalar.White);

    Cv2.ImShow(imageWindow, imgDisplay);
    Cv2.ImShow(resultWindow, result);
}