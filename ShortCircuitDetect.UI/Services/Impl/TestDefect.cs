using OpenCvSharp;
using ShortCircuitDetect.Lib.Service;

namespace ShortCircuitDetect.UI.Services.Impl;

internal class TestDefect : ITestDefect
{
    //private readonly Mat cam;
    //private readonly Mat color;
    private readonly Mat uv;
    private readonly IDefectDetect defectDetect;
    private bool disposed;

    public TestDefect(Mat uv, IDefectDetect defectDetect)
    {
        //ThrowIfNullOfEmpty(cam, "cam 图像未加载");
        //ThrowIfNullOfEmpty(color, "color 图像未加载");
        ThrowIfNullOfEmpty(uv, "uv 图像未加载");
        //if (cam.Type() != MatType.CV_8U) throw new Exception("cam 图像类型错误");
        //if (color.Type() != MatType.CV_8UC3) throw new Exception("color 图像类型错误");
        if (uv.Type() != MatType.CV_8U) throw new Exception("uv 图像类型错误");

        //this.cam = cam;
        //this.color = color;
        this.uv = uv;
        this.defectDetect = defectDetect;
    }

    void ThrowIfNullOfEmpty(Mat mat, string msg)
    {
        if (mat is null || mat.Empty()) throw new Exception(msg);
    }

    public Mat GetResultImage()
    {
        var defectMask = this.defectDetect.GetDefectMask();
        Mat result = Mat.Zeros(this.uv.Size(), MatType.CV_8UC3);
        Cv2.CvtColor(this.uv, result, ColorConversionCodes.GRAY2BGR);

        Cv2.FindContours(defectMask, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
        Cv2.DrawContours(result, contours, -1, Scalar.Red);

        return result;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                uv.Dispose();
                defectDetect.Dispose();
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
