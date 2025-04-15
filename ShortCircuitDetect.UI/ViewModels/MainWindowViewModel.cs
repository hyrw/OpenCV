using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ShortCircuitDetect.Lib.Service.Impl;
using ShortCircuitDetect.UI.Models;
using ShortCircuitDetect.UI.Services.Impl;

namespace ShortCircuitDetect.UI.ViewModels;

partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Dir { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string OutputDir { get; set; } = string.Empty;

    [ObservableProperty]
    public partial WriteableBitmap? ResultImage { get; set; }

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
            Thresh = 30,
            ErodeWidth = 3,
            ErodeHeight = 3,
            ErodeIterations = 12,
            MatchMode = TemplateMatchModes.CCoeff,
            MatchLocScore = 0.9,

            OpenWidth = 3,
            OpenHeight = 3,
            OpenIterations = 1,
            CloseWidth = 7,
            CloseHeight = 7,
            CloseIterations = 1,
            DilateWidth = 5,
            DilateHeight = 5,
            DilateIterations = 1,

            MinArea = 200,
        };
    }

    [RelayCommand]
    async Task TestAllAsync()
    {
        if (Options is null) return;

        await Task.Factory.StartNew(async () =>
        {
            while (imageNames.Count != 0)
            {
                this.imageName = imageNames.Dequeue();
                await LoadMatAsync(this.imageName);
                var processor = new ImageProcessorCollect(new ImageProcessorV3(), OutputDir, this.imageName);
                processor.SetupParam(Options.ToParams());

                using var mask = processor.GetShortCircuit(this.cam!, this.uv!);
                using Mat result = GetDrawResult(this.uv!, mask);
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (this.ResultImage is null)
                    {
                        this.ResultImage = result.ToWriteableBitmap();
                    }
                    else
                    {
                        WriteableBitmapConverter.ToWriteableBitmap(result, this.ResultImage);
                    }
                });
            }
        }, TaskCreationOptions.LongRunning);
    }

    [RelayCommand]
    async Task TestNextAsync()
    {
        if (Options is null) return;
        if (imageNames.Count == 0) return;
        this.imageName = imageNames.Dequeue();
        await LoadMatAsync(this.imageName);

        var processor = new ImageProcessorV3();
        processor.SetupParam(Options.ToParams());
        using var mask = processor.GetShortCircuit(this.cam!, this.uv!);
        using Mat result = GetDrawResult(this.uv!, mask);
        if (this.ResultImage is null)
        {
            this.ResultImage = result.ToWriteableBitmap();
        }
        else
        {
            WriteableBitmapConverter.ToWriteableBitmap(result, this.ResultImage);
        }
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
        string camFile = Path.Combine(Dir, $"{imageName}_CAM.png");
        // string colorFile = Path.Combine(Dir, $"{imageName}_Color.png");
        string uvFile = Path.Combine(Dir, $"{imageName}_UV.png");

        ThrowIfFileNotFound(camFile);
        // ThrowIfFileNotFound(colorFile);
        ThrowIfFileNotFound(uvFile);

        this.cam?.Dispose();
        this.color?.Dispose();
        this.uv?.Dispose();

        this.cam = Cv2.ImRead(camFile, ImreadModes.Grayscale);
        // this.color = Cv2.ImRead(colorFile);
        this.uv = Cv2.ImRead(uvFile, ImreadModes.Grayscale);

        return Task.CompletedTask;
    }

    static void ThrowIfFileNotFound(string file)
    {
        if (!File.Exists(file)) throw new FileNotFoundException(file);
    }

    static Mat GetDrawResult(Mat gray, Mat mask)
    {
        Mat result = gray.CvtColor(ColorConversionCodes.GRAY2BGR);
        Cv2.FindContours(mask, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxNone);
        Cv2.DrawContours(result, contours, -1, Scalar.Red);
        return result;
    }

}
