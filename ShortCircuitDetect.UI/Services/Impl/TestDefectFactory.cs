using OpenCvSharp;
using ShortCircuitDetect.Lib.Service;
using ShortCircuitDetect.Lib.Service.Impl;

namespace ShortCircuitDetect.UI.Services.Impl;

internal static class TestDefectFactory
{
    public static ITestDefect Create(Mat cam, Mat color, Mat uv, DefectOptions options, string imageName, string outputDir, int v = 0)
    {
        IDefectDetect defectDetect = DefectDetectFactorhy.Create(cam, color, uv, options, v);
        ITestDefect impl = new TestDefect(uv, defectDetect);

        return new TestDefectCollect(defectDetect, impl, imageName, outputDir);
    }
}
