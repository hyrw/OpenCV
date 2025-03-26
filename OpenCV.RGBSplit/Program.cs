using OpenCvSharp;
using ShortCircuitDetect.Lib;
using ShortCircuitDetect.Lib.Service;
using ShortCircuitDetect.Lib.Service;
using ShortCircuitDetect.Lib.Service.Impl;

double scale = 0.5;

string dir = @"D:\source\OpenCV\玻璃基板图像";

var files = Directory.EnumerateFiles(dir, "*.png", SearchOption.AllDirectories).
    Select(f => Path.GetFileName(f).Split('_').First())
    .Distinct();

//X228117Y18186_Cam 轮廓会有问题，应该提取一个环，而不是一个圆

Queue<string> names = new(files);
//Queue<string> names = new([
//    Path.Combine(dir, "X228117Y18186")
//    ]);


Mat imuv;
Mat imcolor;
Mat imcam;

var options = new DefectOptions()
{
    MinArea = 900,
    Thresh = 151,

    OpenWidth = 10,
    OpenHeight = 10,

    CloseWidth = 5,
    CloseHeight = 5,

    MatchMode = TemplateMatchModes.CCoeff,
    MatchLocScore = 0.8,
};

Cv2.NamedWindow("tool");
int thresh = 0;
Cv2.CreateTrackbar("thresh", "tool", ref thresh, 255);

while (names.Count != 0)
{
    string name = names.Dequeue();
    //string name = Path.Combine(dir, "X228117Y18186");
    string uvFile = Path.Combine(dir, $"{name}_UV.png");
    string camFile = Path.Combine(dir, $"{name}_Cam.png");
    string colorFile = Path.Combine(dir, $"{name}_Color.png");

    imuv = Cv2.ImRead(uvFile, ImreadModes.Grayscale);
    imcolor = Cv2.ImRead(colorFile);
    imcam = Cv2.ImRead(camFile, ImreadModes.Grayscale);

    //options.Thresh = thresh;
    using IDefectDetect defectDetect = new DefectDetectV3(imcam, imcolor, imuv, options);
    var mask = defectDetect.GetDefectMask();
    //CVOperation.ImShow("mask", mask, scale);
    var result = new Mat();
    Cv2.CopyTo(imcolor, result, mask);
    CVOperation.ImShow("color", imcolor, scale);
    CVOperation.ImShow("result", result, scale);

    if (Cv2.WaitKey() != -1) continue;
    //Cv2.WaitKey(1);
}
