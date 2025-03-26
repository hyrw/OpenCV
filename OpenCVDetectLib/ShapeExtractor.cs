using OpenCvSharp;

namespace ShortCircuitDetect.Lib;

public class ShapeExtractor : IDisposable
{
    readonly Mat labels;
    int currentLabel;
    bool disposedValue;
    readonly int maxLabel;

    public ShapeExtractor(Mat binaryImage)
    {
        if (binaryImage is null ||
            binaryImage.Type().Value != MatType.CV_8UC1 ||
            binaryImage.Empty())
        {
            throw new ArgumentException($"{nameof(binaryImage)}必须是二值图");
        }
        // 获取连通域标签图（背景为0，形状从1开始编号）
        labels = new Mat();
        maxLabel = Cv2.ConnectedComponents(binaryImage, labels);
        currentLabel = 1;  // 跳过背景标签0
    }

    public bool HasNext => currentLabel < maxLabel;

    public Mat GetNextShape()
    {
        if (currentLabel >= maxLabel) throw new IndexOutOfRangeException("没有更多图形了");

        // 创建掩码，仅保留当前标签区域
        var shapeMask = new Mat();
        Cv2.Compare(labels, currentLabel, shapeMask, CmpType.EQ);

        currentLabel++;
        return shapeMask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                labels.Dispose();
            }

            disposedValue = true;
        }
    }

    ~ShapeExtractor()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

