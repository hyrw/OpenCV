using OpenCvSharp;

public interface ICoordinateTransformation
{
    public IList<Point> CorrectionCoordinate(IList<Point> path);
}
