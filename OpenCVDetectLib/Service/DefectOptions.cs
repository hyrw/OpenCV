using OpenCvSharp;

namespace ShortCircuitDetect.Lib.Service;

public class DefectOptions
{
    public int RMin { get; set; }
    public int RMax { get; set; }
    public int GMin { get; set; }
    public int GMax { get; set; }
    public int BMin { get; set; }
    public int BMax { get; set; }

    public int HMin { get; set; }
    public int HMax { get; set; }
    public int SMin { get; set; }
    public int SMax { get; set; }
    public int VMin { get; set; }
    public int VMax { get; set; }


    public int MinArea { get; set; }
    public int Thresh { get; set; }

    public int OpenWidth { get; set; }
    public int OpenHeight { get; set; }

    public int CloseWidth { get; set; }
    public int CloseHeight { get; set; }

    public double MatchLocScore { get; set; }
    public TemplateMatchModes MatchMode { get; set; }

}
