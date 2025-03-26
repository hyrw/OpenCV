using OpenCvSharp;
using ShortCircuitDetect.Lib;
using ShortCircuitDetect.Lib.Service;
using ShortCircuitDetect.Lib.Service;

var options = new DefectOptions()
{
    // 铜hsv 有些泛绿的颜色无法识别，看起来像胶
    BMin = 15,
    BMax = 25,
    GMin = 0,
    GMax = 255,
    RMin = 160,
    RMax = 255,
    MinArea = 900,

    OpenWidth = 3,
    OpenHeight = 3,

    CloseWidth = 30,
    CloseHeight = 30,

    MatchMode = TemplateMatchModes.CCoeff,
    MatchLocScore = 0.8,
};

// color -> hsv -> 将hsv显示的RGB值查找黄色
//(var lower, var upper) = Scalar.FromRgb(240, 254, 20).GetRGBRange(45);
//int hmin = (int)lower.Val0;
//int hmax = (int)upper.Val0;
//int smin = (int)lower.Val1;
//int smax = (int)upper.Val1;
//int vmin = (int)lower.Val2;
//int vmax = (int)upper.Val2;
//var options = new DefectOptions()
//{
//    BMin = hmin,
//    BMax = hmax,
//    GMin = smin,
//    GMax = smax,
//    RMin = vmin,
//    RMax = vmax,
//    MinArea = 900,

//    OpenWidth = 3,
//    OpenHeight = 3,

//    CloseWidth = 30,
//    CloseHeight = 30,

//    MatchMode = TemplateMatchModes.CCoeff,
//    MatchLocScore = 0.8,
//};

var exit = 113;
int lastKey = default;
int nextKey = 32;

string dir = @"D:\source\OpenCV\玻璃基板图像";

var files = Directory.EnumerateFiles(dir, "*.png", SearchOption.AllDirectories).
    Select(f => Path.GetFileName(f).Split('_').First())
    .Distinct();


Queue<string> names = new(files);
//Queue<string> names = new([
//    Path.Combine(dir, "X228117Y18186")
//    ]);
string name = string.Empty;

Mat imuv;
Mat imcolor;
Mat imcam;


double scale = 0.5;
while (names.Count != 0)
{
    name = names.Dequeue();
    //string name = Path.Combine(dir, "X228117Y18186");
    string uvFile = Path.Combine(dir, $"{name}_UV.png");
    string camFile = Path.Combine(dir, $"{name}_Cam.png");
    string colorFile = Path.Combine(dir, $"{name}_Color.png");

    imuv = Cv2.ImRead(uvFile, ImreadModes.Grayscale);
    imcolor = Cv2.ImRead(colorFile);
    imcam = Cv2.ImRead(camFile, ImreadModes.Grayscale);

    IDefectDetect defectDetect = new DefectDetectV2(imcam, imcolor, imuv, options);
    var mask = defectDetect.GetDefectMask();

    var result = new Mat();
    Cv2.CopyTo(imcolor, result, mask);

    CVOperation.ImShow("color", imcolor, scale);
    CVOperation.ImShow("mask", mask, scale);
    CVOperation.ImShow("result", result, scale);


    var key = Cv2.WaitKey();
    if (key == exit) return;
    if (key == 32) continue;
}
