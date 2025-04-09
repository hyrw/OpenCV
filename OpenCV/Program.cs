using OpenCvSharp;
using ShortCircuitDetect.Lib;
using ShortCircuitDetect.Lib.Service.Impl;

//string camFile = @"C:/Users/Coder/Workspace/OpencvTest/0327/00_CAM.png";
//string uvFile = @"C:/Users/Coder/Workspace/OpencvTest/0327/00_UV.png";

//string camFile = @"C:/Users/Coder/Workspace/OpencvTest/0327/10_CAM.png";
//string uvFile = @"C:/Users/Coder/Workspace/OpencvTest/0327/10_UV.png";

string camFile = @"C:/Users/Coder/Workspace/OpencvTest/0327_2/01_CAM.png";
string uvFile = @"C:/Users/Coder/Workspace/OpencvTest/0327_2/01_UV.png";

using Mat cam = Cv2.ImRead(camFile, ImreadModes.Grayscale);
using Mat uv = Cv2.ImRead(uvFile, ImreadModes.Grayscale);

IImageProcessor.Param parma = new IImageProcessor.Param()
{
    Thresh = 50,
    ErodeWidth = 3,
    ErodeHeight = 3,
    ErodeIterations = 12,

    MatchMode = TemplateMatchModes.CCoeff,
    MatchLocScore = 0.9,

    OpenWidth = 5,
    OpenHeight = 5,
    OpenIterations = 1,
    CloseWidth = 5,
    CloseHeight = 5,
    CloseIterations = 6,

    MinArea = 500,
};

ImageProcessorV2 v2 = new();

v2.SetupParam(parma);
using Mat result = v2.GetShortCircuit(cam, uv);

Cv2.FindContours(result, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxNone);

using Mat color = uv.CvtColor(ColorConversionCodes.GRAY2BGR);
Cv2.DrawContours(color, contours, -1, Scalar.Red);
CVOperation.ImShow("mask", result, 0.7);
CVOperation.ImShow("result", color, 0.7);
Cv2.WaitKey();
