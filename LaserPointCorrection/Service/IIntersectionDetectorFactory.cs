using OpenCvSharp;

public static class ICoordinateTransformationFactory
{
    public static ICoordinateTransformation Create(IList<Point> theory, IList<Point> real, CoordinateTransformationImpl implVersion, int width, int height)
    {
        return implVersion switch
        {
            CoordinateTransformationImpl.V1 => new CoordinateTransformationV1(theory, real),
            CoordinateTransformationImpl.V2 => new CoordinateTransformationV2(theory, real, width, height),
            CoordinateTransformationImpl.V3 => new CoordinateTransformationV3(theory, real),
            CoordinateTransformationImpl.V4 => new CoordinateTransformationV4(theory, real, width, height),
            CoordinateTransformationImpl.V5 => new CoordinateTransformationV5(theory, real),
            CoordinateTransformationImpl.V6 => new CoordinateTransformationV6(theory, real),
            CoordinateTransformationImpl.V7 => new CoordinateTransformationV7(theory, real),
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
        V6,
        V7,
    }
}
