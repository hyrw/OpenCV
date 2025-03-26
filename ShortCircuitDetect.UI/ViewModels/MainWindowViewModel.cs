using System.IO;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ShortCircuitDetect.Lib.Service;
using ShortCircuitDetect.Lib.Service.Impl;
using ShortCircuitDetect.UI.Models;
using ShortCircuitDetect.UI.Services.Impl;
using static IImageProcessor;

namespace ShortCircuitDetect.UI.ViewModels;

partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Dir { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OutputDir { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ImageSource? ResultImage { get; set; }

    [ObservableProperty]
    public partial DefectOptionsModel Options { get; set; }

    [ObservableProperty]
    public partial IEnumerable<TemplateMatchModes> MatchModes { get; set; } = Enum.GetValues<TemplateMatchModes>();

    public IEnumerable<int> DetectImpls => [0, 1, 2];

    [ObservableProperty]
    public partial int SelectedImpl { get; set; } = 0;

    Queue<string> imageNames = new();
    string imageName = string.Empty;
    Mat? cam;
    Mat? color;
    Mat? uv;
    private readonly OpenFolderDialog openFolderDialog;

    public MainWindowViewModel()
    {
        this.openFolderDialog = new OpenFolderDialog()
        {
            Multiselect = false,
        };
        Options = new()
        {
            Thresh = 200,
            MatchMode = TemplateMatchModes.CCoeff,
            MatchLocScore = 0.8,

            OpenWidth = 7,
            OpenHeight = 7,
            CloseWidth = 3,
            CloseHeight = 3,

            BMin = 31,
            BMax = 180,
            GMin = 0,
            GMax = 255,
            RMin = 0,
            RMax = 255,

            MinArea = 500,
        };
    }

    [RelayCommand]
    async Task TestAllAsync()
    {
        if (Options is null) return;
        while (imageNames.Count != 0)
        {
            this.imageName = imageNames.Dequeue();
            await LoadMatAsync(this.imageName);

            //Param param = new Param()
            //{
            //    Thresh = Options.Thresh,
            //    MatchMode = Options.MatchMode,
            //    MatchLocScore = Options.MatchLocScore,

            //    OpenWidth = Options.OpenWidth,
            //    OpenHeight = Options.OpenHeight,
            //    CloseWidth = Options.CloseWidth,
            //    CloseHeight = Options.CloseHeight,

            //    BMin = Options.BMin,
            //    BMax = Options.BMax,
            //    GMin = Options.GMin,
            //    GMax = Options.GMax,
            //    RMin = Options.RMin,
            //    RMax = Options.RMax,

            //    MinArea = Options.MinArea,
            //};

            //IImageProcessor processor = new ImageProcessorCollect(new ImageProcessor(), OutputDir, this.imageName);
            //processor.SetupParam(param);
            //Dispatcher.CurrentDispatcher.Invoke(() =>
            //{
            //    using var mask = processor.GetShortCircuit(this.cam!, this.uv!); // thresh=140
            //    //var mask = processor.GetShortCircuit(this.cam!, this.uv!, this.color!);
            //    this.ResultImage = mask.ToWriteableBitmap();
            //});

            var options = new DefectOptions()
            {
                BMin = Options.BMin,
                BMax = Options.BMax,
                GMin = Options.GMin,
                GMax = Options.GMax,
                RMin = Options.RMin,
                RMax = Options.RMax,

                Thresh = Options.Thresh,
                MinArea = Options.MinArea,
            };
            using IDefectDetect defectDetect = new DefectDetectV4(this.cam!, this.color!, this.uv!, options);
            using var mask = defectDetect.GetDefectMask();
            Cv2.FindContours(mask, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
            Cv2.DrawContours(this.color!, contours, -1, Scalar.Red);
            this.ResultImage = this.color!.ToWriteableBitmap();

        }
        return;
    }

    [RelayCommand]
    async Task TestNextAsync()
    {
        if (Options is null) return;
        //if (imageNames.Count == 0) return;
        //this.imageName = imageNames.Dequeue();
        this.imageName = @"C:\Users\Coder\Workspace\玻璃基板图像\新建文件夹\X132207Y64236";
        await LoadMatAsync(this.imageName);

        //Param param = new Param()
        //{
        //    Thresh = Options.Thresh,
        //    MatchMode = Options.MatchMode,
        //    MatchLocScore = Options.MatchLocScore,

        //    OpenWidth = Options.OpenWidth,
        //    OpenHeight = Options.OpenHeight,
        //    CloseWidth = Options.CloseWidth,
        //    CloseHeight = Options.CloseHeight,

        //    BMin = Options.BMin,
        //    BMax = Options.BMax,
        //    GMin = Options.GMin,
        //    GMax = Options.GMax,
        //    RMin = Options.RMin,
        //    RMax = Options.RMax,

        //    MinArea = Options.MinArea,
        //};

        //IImageProcessor processor = new ImageProcessor();
        //processor.SetupParam(param);
        //var mask = processor.GetShortCircuit(this.cam!, this.uv!, this.color!);
        //this.ResultImage = mask.ToWriteableBitmap();

        var options = new DefectOptions()
        {
            BMin = Options.BMin,
            BMax = Options.BMax,
            GMin = Options.GMin,
            GMax = Options.GMax,
            RMin = Options.RMin,
            RMax = Options.RMax,

            Thresh = Options.Thresh,
            MinArea = Options.MinArea,
        };
        using IDefectDetect defectDetect = new DefectDetectV4(this.cam!, this.color!, this.uv!, options);
        //using IDefectDetect defectDetect = new DefectDetectV3(this.cam!, this.color!, this.uv!, options);
        using var mask = defectDetect.GetDefectMask();
        Cv2.FindContours(mask, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
        Cv2.DrawContours(this.color!, contours, -1, Scalar.Red);
        this.ResultImage = this.color!.ToWriteableBitmap();
    }

    [RelayCommand]
    void SelectTestImageFolder()
    {
        if (this.openFolderDialog.ShowDialog() != true) return;

        Dir = this.openFolderDialog.FolderName;
    }

    [RelayCommand]
    void SelectOutputImageFolder()
    {
        if (this.openFolderDialog.ShowDialog() != true) return;

        OutputDir = this.openFolderDialog.FolderName;
    }

    partial void OnDirChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        imageNames = new Queue<string>(GetImageNameFromDir(value));
    }

    [RelayCommand]
    void ReloadImageName()
    {
        if (string.IsNullOrWhiteSpace(Dir)) return;

        imageNames = new Queue<string>(GetImageNameFromDir(Dir));
    }

    static IEnumerable<string> GetImageNameFromDir(string dir)
    {
        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException(dir);

        return Directory.EnumerateFiles(dir, "*.png", SearchOption.TopDirectoryOnly).
            Select(f => Path.GetFileName(f).Split('_').First())
            .Distinct();
    }

    Task LoadMatAsync(string imageName)
    {
        string camFile = Path.Combine(Dir, $"{imageName}_Cam.png");
        string colorFile = Path.Combine(Dir, $"{imageName}_Color.png");
        string uvFile = Path.Combine(Dir, $"{imageName}_UV.png");

        ThrowIfFileNotFound(camFile);
        ThrowIfFileNotFound(colorFile);
        ThrowIfFileNotFound(uvFile);

        this.cam?.Dispose();
        this.color?.Dispose();
        this.uv?.Dispose();

        this.cam = Cv2.ImRead(camFile, ImreadModes.Grayscale);
        this.color = Cv2.ImRead(colorFile);
        this.uv = Cv2.ImRead(uvFile, ImreadModes.Grayscale);

        return Task.CompletedTask;
    }

    static void ThrowIfFileNotFound(string file)
    {
        if (!File.Exists(file)) throw new FileNotFoundException(file);
    }

}
