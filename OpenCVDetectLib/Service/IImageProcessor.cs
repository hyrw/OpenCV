using OpenCvSharp;

public interface IImageProcessor
{
    /// <summary>
    /// 获取短路区域
    /// </summary>
    /// <param name="norm">标准图，二值图</param>
    /// <param name="gray">灰度成像</param>
    /// <returns>返回识别结果，一张二值图，其尺寸等同于输入的灰度成像</returns>
    Mat GetShortCircuit(Mat norm, Mat gray);

    /// <summary>
    /// 获取短路区域
    /// </summary>
    /// <param name="norm">标准图，二值图</param>
    /// <param name="gray">灰度成像</param>
    /// <param name="color">彩色成像</param>
    /// <returns>回识别结果，一张二值图，其尺寸等同于输入的灰度成像</returns>
    Mat GetShortCircuit(Mat norm, Mat gray, Mat color);

    Param GetParam();

    void SetupParam(Param param);

    /// <summary>
    /// 参数，自行添加所需参数
    /// </summary>
    public struct Param
    {
        public int RMin { get; set; }
        public int RMax { get; set; }
        public int GMin { get; set; }
        public int GMax { get; set; }
        public int BMin { get; set; }
        public int BMax { get; set; }

        public int MinArea { get; set; }
        public int Thresh { get; set; }

        public int OpenWidth { get; set; }
        public int OpenHeight { get; set; }

        public int CloseWidth { get; set; }
        public int CloseHeight { get; set; }

        public TemplateMatchModes MatchMode;
        public double MatchLocScore { get; set; }
    }
}