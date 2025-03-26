using OpenCvSharp;
using ShortCircuitDetect.Lib;

var exit = 113;
int lastKey = default;
int nextKey = 32;

string dir = @"D:\source\OpenCV\玻璃基板图像";

var files = Directory.EnumerateFiles(dir, "*.png", SearchOption.AllDirectories).
    Select(f => Path.GetFileName(f).Split('_').First())
    .Distinct();

//X228117Y18186_Cam 轮廓会有问题，应该提取一个环，而不是一个圆

Queue<string> names = new(files);
//Queue<string> names = new([
//    Path.Combine(dir, "X228117Y18186")
//    ]);
string name = string.Empty;

Mat imuv;
Mat imcolor;
Mat imcam;

Mat roiMask;

// 铜hsv 有些泛绿的颜色无法识别，看起来像胶
//int hmin = 15;
//int hmax = 25;
//int smin = 0;
//int smax = 255;
//int vmin = 160;
//int vmax = 255;

// 避开铜,泛红的hsv
//int hmin = 22;
//int hmax = 63;
// h 46
//int hmin = 8;
//int hmax = 179;
//int smin = 200;
//int smax = 255;
//int vmin = 0;
//int vmax = 173;

// color -> hsv -> 将hsv显示的RGB值查找黄色
(var lower, var upper) = Scalar.FromRgb(240, 254, 20).GetRGBRange(45);
int hmin = (int)lower.Val0;
int hmax = (int)upper.Val0;
int smin = (int)lower.Val1;
int smax = (int)upper.Val1;
int vmin = (int)lower.Val2;
int vmax = (int)upper.Val2;


Cv2.NamedWindow("Trackbars");
Cv2.CreateTrackbar("H Min", "Trackbars", ref hmin, 179);
Cv2.CreateTrackbar("H Max", "Trackbars", ref hmax, 179);
Cv2.CreateTrackbar("S Min", "Trackbars", ref smin, 255);
Cv2.CreateTrackbar("S Max", "Trackbars", ref smax, 255);
Cv2.CreateTrackbar("V Min", "Trackbars", ref vmin, 255);
Cv2.CreateTrackbar("V Max", "Trackbars", ref vmax, 255);
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

    (var _, var loc) = CVOperation.FindMatchLoc(imcam, imuv, TemplateMatchModes.CCoeff);

    var mask = imcam.SubMat(new Rect(loc, imuv.Size())).Threshold(10, 255, ThresholdTypes.BinaryInv);
    var colorRoi = new Mat();
    Cv2.CopyTo(imcolor, colorRoi, mask);
    imcolor.CopyTo(colorRoi, mask);
    var hsv = new Mat();
    imcolor.CvtColor(ColorConversionCodes.BGR2HSV).CopyTo(hsv, mask); ;
    CVOperation.ImShow("hsv", hsv,scale);
    CVOperation.ImShow("color", colorRoi, scale);

    var hsvMask = hsv.InRange(Scalar.FromRgb(vmin, smin, hmin), Scalar.FromRgb(vmax, smax, hmax));

    // 找铜的处理
    var result = new Mat();
    Cv2.BitwiseAnd(colorRoi, colorRoi, result, hsvMask);
    CVOperation.ImShow("result", result, scale);

    // 避开铜的处理
    //CVOperation.ImShow("hsvMask 处理前", hsvMask, scale);
    //var openElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
    //var closeElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(30, 30));
    //Cv2.MorphologyEx(hsvMask, hsvMask, MorphTypes.Open, openElement);
    //Cv2.MorphologyEx(hsvMask, hsvMask, MorphTypes.Close, closeElement); 
    //CVOperation.ImShow("hsvMask 处理后", hsvMask, scale);
    //Cv2.BitwiseNot(hsvMask, hsvMask, mask);
    //var result = new Mat();
    //Cv2.BitwiseAnd(colorRoi, colorRoi, result, hsvMask);
    //CVOperation.ImShow("result", result, scale);

    var key = Cv2.WaitKey();
    if (key == exit) return;
    if (key == 32) continue;

}
