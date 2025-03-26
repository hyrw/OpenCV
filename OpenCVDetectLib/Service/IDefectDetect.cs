using OpenCvSharp;

namespace ShortCircuitDetect.Lib.Service;

public interface IDefectDetect : IDisposable
{
    public Mat GetDefectMask();
}
