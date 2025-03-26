using OpenCvSharp;
using ShortCircuitDetect.Lib;


//string file = @"C:\Users\Coder\Workspace\OpencvTest\X76758Y66649_Color.png";
string file = @"D:\source\OpenCV\玻璃基板图像\X228117Y18186_Color.png";

var scale = 0.5;
var color = Cv2.ImRead(file);
var cam = Cv2.ImRead(file, ImreadModes.Grayscale);
var uv = Cv2.ImRead(file, ImreadModes.Grayscale);


var hsv = color.CvtColor(ColorConversionCodes.BGR2HSV);


Cv2.ImWrite(@"C:\Users\14402\Desktop\新建文件夹\hsv.png", hsv);
(var lower, var upper) = CVOperation.GetRGBRange(Scalar.YellowGreen, 20);

CVOperation.ImShow("hsv", hsv, scale);


var mask = hsv.InRange(Scalar.FromRgb(0, 0, 19), Scalar.FromRgb(255, 255, 25));
CVOperation.ImShow("mask", mask, scale);

Cv2.WaitKey();

//var hsvChannel = Cv2.Split(hsv);
//var h = hsvChannel[0];
//var s = hsvChannel[1];
//var v = hsvChannel[2];

//var hVis = new Mat();
//Cv2.ConvertScaleAbs(h, hVis, alpha: 255 / 179.0);
//CVOperation.ImShow("H Channel (Gray)", hVis, scale);
//var hColor = new Mat();
//Cv2.ApplyColorMap(hVis, hColor, ColormapTypes.Hsv);
//CVOperation.ImShow("H Channel (Pseudo-Color)", hColor, scale);

//CVOperation.ImShow("S Channel", s, scale);
//CVOperation.ImShow("V Channel", v, scale);
