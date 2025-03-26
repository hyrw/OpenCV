using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using ShortCircuitDetect.Lib.Service;

namespace ShortCircuitDetect.UI.Models;

public partial class DefectOptionsModel : ObservableObject
{
    [ObservableProperty]
    public partial int RMin { get; set; }

    [ObservableProperty]
    public partial int RMax { get; set; }

    [ObservableProperty]
    public partial int GMin { get; set; }
    [ObservableProperty]
    public partial int GMax { get; set; }
    [ObservableProperty]
    public partial int BMin { get; set; }
    [ObservableProperty]
    public partial int BMax { get; set; }
    [ObservableProperty]

    public partial int MinArea { get; set; }
    [ObservableProperty]
    public partial int Thresh { get; set; }
    [ObservableProperty]

    public partial int OpenWidth { get; set; }
    [ObservableProperty]
    public partial int OpenHeight { get; set; }
    [ObservableProperty]

    public partial int CloseWidth { get; set; }
    [ObservableProperty]
    public partial int CloseHeight { get; set; }

    [ObservableProperty]
    public partial double MatchLocScore { get; set; }

    [ObservableProperty]
    public partial TemplateMatchModes MatchMode { get; set; }

}

public static class DefectOptionsModelEx
{
    public static DefectOptions ToDefectOptions(this DefectOptionsModel model)
    {
        return new DefectOptions
        {
            BMin = model.BMin,
            BMax = model.BMax,
            GMin = model.GMin,
            GMax = model.GMax,
            RMin = model.RMin,
            RMax = model.RMax,

            Thresh = model.Thresh,
            MinArea = model.MinArea,

            OpenWidth = model.OpenWidth,
            OpenHeight = model.OpenHeight,
            CloseWidth  = model.CloseWidth,
            CloseHeight = model.CloseHeight,

            MatchLocScore = model.MatchLocScore,
            MatchMode = model.MatchMode,
        };
    }

    public static DefectOptionsModel Clone(this DefectOptionsModel model)
    {
        return new()
        {
            BMin = model.BMin,
            BMax = model.BMax,
            GMin = model.GMin,
            GMax = model.GMax,
            RMin = model.RMin,
            RMax = model.RMax,

            Thresh = model.Thresh,
            MinArea = model.MinArea,

            OpenWidth = model.OpenWidth,
            OpenHeight = model.OpenHeight,
            CloseWidth  = model.CloseWidth,
            CloseHeight = model.CloseHeight,

            MatchLocScore = model.MatchLocScore,
            MatchMode = model.MatchMode,
        };
    }
}
