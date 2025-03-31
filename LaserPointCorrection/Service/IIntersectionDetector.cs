using OpenCvSharp;

public interface IIntersectionDetector
{
    IList<Point> FindCrossPoints(Mat img);
}
