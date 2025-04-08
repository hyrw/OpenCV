using OpenCvSharp;

namespace ShortCircuitDetect.Lib.Extensions;

public static class ScalarEx
{
    public static (Scalar lower, Scalar upper) GetHRange(this Scalar bgr, int threshold = 20)
    {
        using Mat mat = new Mat(new Size(1, 1), MatType.CV_8UC3, bgr);
        using Mat color = mat.CvtColor(ColorConversionCodes.BGR2HSV);
        var hsv = color.At<Vec3b>();
        var lower = new Scalar(hsv[0] - threshold, 0, 0);
        var upper = new Scalar(hsv[1] + threshold, 255, 255);
        return (lower, upper);
    }

    public static (Scalar lower, Scalar upper) GetRGBRange(this Scalar self, int threshold = 20)
    {
        var lower = new Scalar(self[0] - threshold, self[1] - threshold, self[2] - threshold);
        var upper = new Scalar(self[0] + threshold, self[1] + threshold, self[2] + threshold);

        return (lower, upper);
    }
}
