using System.IO;
using OpenCvSharp;
using ShortCircuitDetect.Lib.Service;

namespace ShortCircuitDetect.UI.Services.Impl;

internal class TestDefectCollect(IDefectDetect defectDetect, ITestDefect impl, string filename, string outputDir) : ITestDefect
{
    private bool disposed;

    public Mat GetResultImage()
    {
        string maskFile = Path.Combine(outputDir, $"{filename}_mask.png");
        string resultFile = Path.Combine(outputDir, $"{filename}_result.png");
        var result = impl.GetResultImage();

        Cv2.ImWrite(maskFile, defectDetect.GetDefectMask());
        Cv2.ImWrite(resultFile, result);

        return result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                defectDetect.Dispose();
                impl.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
