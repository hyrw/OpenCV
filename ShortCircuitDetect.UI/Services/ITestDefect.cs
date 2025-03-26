using OpenCvSharp;

namespace ShortCircuitDetect.UI.Services;

internal interface ITestDefect : IDisposable
{
    Mat GetResultImage();
}
