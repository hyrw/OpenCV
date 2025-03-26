using OpenCvSharp;
using ShortCircuitDetect.Lib;
using ShortCircuitDetect.Lib.Service;
using ShortCircuitDetect.Lib.Service.Impl;

double scale = 0.5;

string dir = @"C:\Users\Coder\Workspace\玻璃基板图像\新建文件夹";

var files = Directory.EnumerateFiles(dir, "*.png", SearchOption.AllDirectories).
    Select(f => Path.GetFileName(f).Split('_').First())
    .Distinct();

Queue<string> names = new(files);
//Queue<string> names = new([
//    Path.Combine(dir, "X228117Y18186")
//    ]);


Mat imuv;
Mat imcolor;
Mat imcam;

var options = new DefectOptions()
{
    // 基板颜色
    BMin = 5,
    BMax = 140,
    GMin = 200,
    GMax = 255,
    RMin = 0,
    RMax = 173,
    //MinArea = 900,
    MinArea = 500,

    OpenWidth = 5,
    OpenHeight = 5,

    CloseWidth = 30,
    CloseHeight = 30,

    MatchMode = TemplateMatchModes.CCoeff,
    MatchLocScore = 0.8,
};

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

    using IDefectDetect defectDetect = new DefectDetectV1(imcam, imcolor, imuv, options);
    var mask = defectDetect.GetDefectMask();
    //CVOperation.ImShow("mask", mask, scale);
    var result = new Mat();
    Cv2.CopyTo(imcolor, result, mask);
    CVOperation.ImShow("color", imcolor, scale);
    CVOperation.ImShow("result", result, scale);

    if (Cv2.WaitKey() != -1) continue;
}
