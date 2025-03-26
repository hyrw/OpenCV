using OpenCvSharp;

// 棋盘格参数
Size boardSize = new Size(4, 6); // 内角点数（列数，行数）
float squareSize = 25.0f; // 每个方格的物理尺寸（毫米）

// 存储世界坐标和图像坐标
List<Mat> objectPoints = new List<Mat>();
List<Mat> imagePoints = new List<Mat>();

// 生成世界坐标点（Z=0）
List<Point3f> objPoints = new List<Point3f>();
for (int y = 0; y < boardSize.Height; y++)
{
    for (int x = 0; x < boardSize.Width; x++)
    {
        objPoints.Add(new Point3f(x * squareSize, y * squareSize, 0));
    }
}
Mat objPtMat = Mat.FromArray(objPoints.ToArray());

string[] imageFiles = Directory.GetFiles(@"C:\Users\Coder\Pictures\calibration_images\", "*.png");
Size imageSize = new Size();

foreach (string file in imageFiles)
{
    using Mat image = Cv2.ImRead(file);
    imageSize = image.Size();
    using Mat gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);

    // 检测棋盘格角点
    Point2f[] corners;
    bool found = Cv2.FindChessboardCorners(gray, boardSize, out corners);

    if (found)
    {
        // 亚像素精确化
        Cv2.CornerSubPix(gray, corners, new Size(11, 11), new Size(-1, -1),
            new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 30, 0.001));

        // 保存结果
        imagePoints.Add(Mat.FromArray(corners));
        objectPoints.Add(objPtMat.Clone());
        Cv2.DrawChessboardCorners(image, boardSize, corners, true);
        Cv2.ImShow("test", image);
        Cv2.WaitKey();
    }
}

Mat cameraMatrix = new Mat();
Mat distCoeffs = new Mat();
Mat[] rvecs, tvecs;

double reprojError = Cv2.CalibrateCamera(
    objectPoints, imagePoints, imageSize,
    cameraMatrix, distCoeffs, out rvecs, out tvecs
);

// 输出结果
Console.WriteLine($"相机矩阵:\n{cameraMatrix.Dump()}");
Console.WriteLine($"畸变系数:\n{distCoeffs.Dump()}");
Console.WriteLine($"平均重投影误差: {reprojError}");

// 保存参数
// using (FileStorage fs = new FileStorage("camera_params.yml", FileStorage.Mode.Write))
// {
//     fs.Write("camera_matrix", cameraMatrix);
//     fs.Write("distortion_coefficients", distCoeffs);
// }

var matrix = Cv2.GetOptimalNewCameraMatrix(cameraMatrix, distCoeffs, imageSize, default, imageSize, out var rect);

//测试矫正
Mat testImage = Cv2.ImRead(@"C:\Users\Coder\Pictures\calibration_images\1.png");
Mat undistorted = new Mat();
Cv2.Undistort(testImage, undistorted, cameraMatrix, distCoeffs, matrix);

Cv2.ImShow("Original", testImage);
Cv2.ImShow("Undistorted", undistorted);
Cv2.WaitKey(0);
