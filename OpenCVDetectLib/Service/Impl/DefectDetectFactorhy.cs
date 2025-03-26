using OpenCvSharp;

namespace ShortCircuitDetect.Lib.Service.Impl;

public static class DefectDetectFactorhy
{
    public static IDefectDetect Create(Mat cam, Mat color, Mat uv, DefectOptions options, int v)
    {
        return v switch
        {
            0 => new DefectDetectV1(cam, color, uv, options),
            1 => new DefectDetectV2(cam, color, uv, options),
            2 => new DefectDetectV3(cam, color, uv, options),
            _ => throw new NotImplementedException()
        };
    }
}
