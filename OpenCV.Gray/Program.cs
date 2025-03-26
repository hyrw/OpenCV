using OpenCvSharp;
string dir = @"C:\Users\Coder\Workspace\OpencvTest";

var exit = 113;
int lastKey = default;
//var files = Directory.EnumerateFiles(dir, "*.png").
//    Select(f => f.Split('_').First())
//    .Distinct();

//Queue<string> names = new(files);
Queue<string> names = new Queue<string>([@"D:\source\OpenCV\玻璃基板图像\X228117Y18186"]);

string filename = string.Empty;
//int thresh = 250;
int thresh = 190;
Cv2.NamedWindow("Trackbars");
Cv2.CreateTrackbar("Thresh", "Trackbars", ref thresh, 255);
Cv2.NamedWindow("threshold", WindowFlags.KeepRatio);
Cv2.ResizeWindow("threshold", (int)(1920 * 0.7), (int)(1200 * 0.7));

Mat camImg = new ();
Mat uvImg = new();
Mat colorImg = new();


while (true)
{
    if (lastKey != -1)
    {
        filename = names.Dequeue();
    }
    LoadImages(filename);

    ProcessImage();
    lastKey = Cv2.WaitKey(10);
    if (lastKey == exit) break;
    /**
     * j n 106 110
     * k p 107 112
     */
    //Console.WriteLine(key);
}
Cv2.DestroyAllWindows();

void ProcessImage()
{
    Cv2.NamedWindow("ttt", WindowFlags.KeepRatio);
    Cv2.ResizeWindow("ttt", (int)(1920 * 0.7), (int)(1200 * 0.7));
    Cv2.ImShow("ttt", uvImg);

    Cv2.Threshold(uvImg, uvImg, thresh, 255, ThresholdTypes.Binary);
    Cv2.ImShow("threshold", uvImg);

    var closeElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
    Cv2.MorphologyEx(uvImg, uvImg, MorphTypes.Close, closeElement);
    Cv2.ImShow("close", uvImg);

}

void LoadImages(string filename)
{
    camImg = Cv2.ImRead(GetCamFilename(filename));
    uvImg = Cv2.ImRead(GetUVFilename(filename));
    colorImg = Cv2.ImRead(GetColorFilename(filename));
}

static string GetCamFilename(string filename) => $"{filename}_Cam.png";
static string GetUVFilename(string filename) => $"{filename}_UV.png";
static string GetColorFilename(string filename) => $"{filename}_Color.png";

