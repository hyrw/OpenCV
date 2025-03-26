using OpenCvSharp;
using ShortCircuitDetect.Lib;

///// 1、找角点
//Size patternsize = new Size(5, 4);
//Mat gray = Cv2.ImRead(@"C:\Users\Coder\Desktop\checkerboard 1.png", ImreadModes.Grayscale);
//var corners = new List<Point2f>();
//bool patternfound = Cv2.FindChessboardCorners(gray, patternsize, OutputArray.Create<Point2f>(corners),
// ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage | ChessboardFlags.FastCheck);

//if (patternfound)
//{
//    Cv2.CornerSubPix(gray, corners, new Size(11, 11), new Size(-1, -1),
//      new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.Count, 100, 0.01));
//}

///// 2、显示结果
//var img = gray.Clone();
//Cv2.CvtColor(img, img, ColorConversionCodes.GRAY2RGB);
//Cv2.DrawChessboardCorners(img, patternsize, corners, patternfound);

///// 3、计算映射关系
//corners.Reverse();
//var corners_realworld = new List<Point2f>()      {
//       new Point2f(00.0f, 00.0f),        new Point2f(02.5f, 00.0f),        new Point2f(05.0f, 00.0f),        new Point2f(07.5f, 00.0f),        new Point2f(10.0f, 00.0f),
//       new Point2f(00.0f, 02.5f),        new Point2f(02.5f, 02.5f),        new Point2f(05.0f, 02.5f),        new Point2f(07.5f, 02.5f),        new Point2f(10.0f, 02.5f),
//       new Point2f(00.0f, 05.0f),        new Point2f(02.5f, 05.0f),        new Point2f(05.0f, 05.0f),        new Point2f(07.5f, 05.0f),        new Point2f(10.0f, 05.0f),
//       new Point2f(00.0f, 07.5f),        new Point2f(02.5f, 07.5f),        new Point2f(05.0f, 07.5f),        new Point2f(07.5f, 07.5f),        new Point2f(10.0f, 07.5f),
//     };
//var from = InputArray.Create<Point2f>(corners);
//var to = InputArray.Create<Point2f>(corners_realworld);
//var inliers = new Mat();
//var affine_mat = Cv2.EstimateAffine2D(from, to, inliers)!;

///// 4、计算投影误差
//var result = new List<Point2f>();
//var m00 = affine_mat.At<double>(0, 0);
//var m01 = affine_mat.At<double>(0, 1);
//var m02 = affine_mat.At<double>(0, 2);
//var m10 = affine_mat.At<double>(1, 0);
//var m11 = affine_mat.At<double>(1, 1);
//var m12 = affine_mat.At<double>(1, 2);
//for (int i = 0; i < corners.Count; i++)
//{
//    var x = corners[i].X;
//    var y = corners[i].Y;
//    result.Add(new Point2f((float)(m00 * x + m01 * y + m02 * 1), (float)(m10 * x + m11 * y + m12 * 1)));

//    Console.WriteLine($"{i,2}: {(result[i].X - corners_realworld[i].X).ToString("+0.00000;-0.00000;0.00000")}, {(result[i].Y - corners_realworld[i].Y).ToString("+0.00000;-0.00000;0.00000")}");
//}
//Cv2.ImShow("img", img);
//Cv2.WaitKey();
