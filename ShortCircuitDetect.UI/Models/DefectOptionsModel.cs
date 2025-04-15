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
    public partial int HMin { get; set; }
    [ObservableProperty]
    public partial int HMax { get; set; }
    [ObservableProperty]
    public partial int SMin { get; set; }
    [ObservableProperty]
    public partial int SMax { get; set; }
    [ObservableProperty]
    public partial int VMin { get; set; }
    [ObservableProperty]
    public partial int VMax { get; set; }

    [ObservableProperty]
    public partial int MinArea { get; set; }
    [ObservableProperty]
    public partial int Thresh { get; set; }

    [ObservableProperty]
    public partial int OpenWidth { get; set; }
    [ObservableProperty]
    public partial int OpenHeight { get; set; }
    [ObservableProperty]
    public partial int OpenIterations { get; set; }

    [ObservableProperty]
    public partial int CloseWidth { get; set; }
    [ObservableProperty]
    public partial int CloseHeight { get; set; }
    [ObservableProperty]
    public partial int CloseIterations { get; set; }

    [ObservableProperty]
    public partial int ErodeWidth { get; set; }
    [ObservableProperty]
    public partial int ErodeHeight { get; set; }
    [ObservableProperty]
    public partial int ErodeIterations { get; set; }

    [ObservableProperty]
    public partial int DilateWidth { get; set; }
    [ObservableProperty]
    public partial int DilateHeight { get; set; }
    [ObservableProperty]
    public partial int DilateIterations { get; set; }

    [ObservableProperty]
    public partial TemplateMatchModes MatchMode { get; set; }
    [ObservableProperty]
    public partial double MatchLocScore { get; set; }

}

public static class DefectOptionsModelEx
{
    public static DefectOptions ToDefectOptions(this DefectOptionsModel model)
    {
        return new DefectOptions
        {
            HMin = model.HMin,
            HMax = model.HMax,
            SMin = model.SMin,
            SMax = model.SMax,
            VMin = model.VMin,
            VMax = model.VMax,

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
            CloseWidth = model.CloseWidth,
            CloseHeight = model.CloseHeight,

            MatchLocScore = model.MatchLocScore,
            MatchMode = model.MatchMode,

            ErodeWidth = model.ErodeWidth,
            ErodeHeight = model.ErodeHeight,
            ErodeIterations = model.ErodeIterations,

            DilateWidth = model.DilateWidth,
            DilateHeight = model.DilateHeight,
            DilateIterations = model.DilateIterations,
        };
    }

    public static IImageProcessor.Param ToParams(this DefectOptionsModel model)
    {
        return new IImageProcessor.Param
        {
            HMin = model.HMin,
            HMax = model.HMax,
            SMin = model.SMin,
            SMax = model.SMax,
            VMin = model.VMin,
            VMax = model.VMax,

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
            CloseWidth = model.CloseWidth,
            CloseHeight = model.CloseHeight,

            MatchLocScore = model.MatchLocScore,
            MatchMode = model.MatchMode,

            ErodeWidth = model.ErodeWidth,
            ErodeHeight = model.ErodeHeight,
            ErodeIterations = model.ErodeIterations,

            DilateWidth = model.DilateWidth,
            DilateHeight = model.DilateHeight,
            DilateIterations = model.DilateIterations,
        };
    }

    public static DefectOptionsModel Clone(this DefectOptionsModel model)
    {
        return new()
        {
            HMin = model.HMin,
            HMax = model.HMax,
            SMin = model.SMin,
            SMax = model.SMax,
            VMin = model.VMin,
            VMax = model.VMax,

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
            CloseWidth = model.CloseWidth,
            CloseHeight = model.CloseHeight,

            MatchLocScore = model.MatchLocScore,
            MatchMode = model.MatchMode,

            ErodeWidth = model.ErodeWidth,
            ErodeHeight = model.ErodeHeight,
            ErodeIterations = model.ErodeIterations,

            DilateWidth = model.DilateWidth,
            DilateHeight = model.DilateHeight,
            DilateIterations = model.DilateIterations,
        };
    }
}
