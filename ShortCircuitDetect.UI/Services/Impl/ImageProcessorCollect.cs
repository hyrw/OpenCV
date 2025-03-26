using System.IO;
using OpenCvSharp;

namespace ShortCircuitDetect.UI.Services.Impl;

public class ImageProcessorCollect(IImageProcessor impl, string outputDir, string filename) : IImageProcessor
{
    private IImageProcessor.Param param;

    public IImageProcessor.Param GetParam() => impl.GetParam();

    public void SetupParam(IImageProcessor.Param param)
    {
        this.param = param;
        impl.SetupParam(param);
    }

    public Mat GetShortCircuit(Mat norm, Mat gray)
    {
        string maskFile = Path.Combine(outputDir, $"{filename}_mask.png");
        string resultFile = Path.Combine(outputDir, $"{filename}_result.png");

        var mask = impl.GetShortCircuit(norm, gray);

        using Mat result = GetResult(gray, mask);
        Cv2.ImWrite(maskFile, mask);
        Cv2.ImWrite(resultFile, result);

        return mask;
    }

    public Mat GetShortCircuit(Mat norm, Mat gray, Mat color)
    {
        string maskFile = Path.Combine(outputDir, $"{filename}_mask.png");
        string resultFile = Path.Combine(outputDir, $"{filename}_result.png");

        var mask = impl.GetShortCircuit(norm, gray, color);

        using Mat result = GetResult(gray, mask);
        Cv2.ImWrite(maskFile, mask);
        Cv2.ImWrite(resultFile, result);

        return mask;
    }

    Mat GetResult(Mat gray, Mat mask)
    {
        Mat result = Mat.Zeros(gray.Size(), MatType.CV_8UC3);
        Cv2.CvtColor(gray, result, ColorConversionCodes.GRAY2BGR);

        Cv2.FindContours(mask, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
        Cv2.DrawContours(result, contours, -1, Scalar.Red);

        return result;
    }

}
