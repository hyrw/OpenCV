using OpenCvSharp;

public static class ICoordinateTransformationFactory
{
    public static ICoordinateTransformation Create(IList<Point> from, IList<Point> to, CoordinateTransformationImpl implVersion, int width, int height)
    {
        return implVersion switch
        {
            CoordinateTransformationImpl.V1 => new CoordinateTransformationV1(from, to),
            CoordinateTransformationImpl.V2 => new CoordinateTransformationV2(from, to, width, height),
            CoordinateTransformationImpl.V3 => new CoordinateTransformationV3(from, to),
            CoordinateTransformationImpl.V4 => new CoordinateTransformationV4(from, to, width, height),
            CoordinateTransformationImpl.V5 => new CoordinateTransformationV5(from, to),
            _ => throw new ArgumentException(),
        };
    }

    public enum CoordinateTransformationImpl
    {
        V1,
        V2,
        V3,
        V4,
        V5,
    }
}
