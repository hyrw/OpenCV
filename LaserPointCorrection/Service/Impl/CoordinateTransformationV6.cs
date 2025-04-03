using OpenCvSharp;

public class CoordinateTransformationV6 : ICoordinateTransformation
{
    readonly double[] coeffX;
    readonly double[] coeffY;

    public CoordinateTransformationV6(IList<Point> src, IList<Point> dst)
    {
        (this.coeffX, this.coeffY) = FitCorrectionModel(src.Select(p => new double[] { p.X, p.Y }).ToList(),
                           dst.Select(p => new double[] { p.X, p.Y }).ToList());
    }

    public IList<Point> GetPath(IList<Point> path)
    {
        if (path is null || !path.Any()) throw new ArgumentException("路径不能为空");

        return path.Select(p => ApplyCorrection(new double[] { p.X, p.Y }, this.coeffX, this.coeffY))
            .Select(p => new Point(p[0], p[1]))
            .ToList();
    }

    // 拟合校正模型
    static (double[] coeffX, double[] coeffY) FitCorrectionModel(
        List<double[]> theory, List<double[]> real)
    {
        // 构建系数矩阵
        double[,] A = new double[real.Count, 6];
        for (int i = 0; i < real.Count; i++)
        {
            double x = real[i][0], y = real[i][1];
            A[i, 0] = 1;
            A[i, 1] = x;
            A[i, 2] = y;
            A[i, 3] = x * x;
            A[i, 4] = x * y;
            A[i, 5] = y * y;
        }

        // 构建目标向量
        double[] bX = theory.Select(p => p[0]).ToArray();
        double[] bY = theory.Select(p => p[1]).ToArray();

        // 计算系数 (A^T A)^-1 A^T b
        var AT = Transpose(A);
        var ATA = Multiply(AT, A);
        var ATbX = Multiply(AT, bX);
        var ATbY = Multiply(AT, bY);

        return (Solve(ATA, ATbX), Solve(ATA, ATbY));
    }

    // 矩阵转置
    static double[,] Transpose(double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        double[,] result = new double[cols, rows];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[j, i] = matrix[i, j];
        return result;
    }

    // 矩阵乘法
    static double[,] Multiply(double[,] a, double[,] b)
    {
        int aRows = a.GetLength(0);
        int aCols = a.GetLength(1);
        int bCols = b.GetLength(1);
        double[,] result = new double[aRows, bCols];
        for (int i = 0; i < aRows; i++)
            for (int k = 0; k < aCols; k++)
                for (int j = 0; j < bCols; j++)
                    result[i, j] += a[i, k] * b[k, j];
        return result;
    }

    // 矩阵与向量乘法
    static double[] Multiply(double[,] a, double[] b)
    {
        int rows = a.GetLength(0);
        int cols = a.GetLength(1);
        double[] result = new double[rows];
        for (int i = 0; i < rows; i++)
            for (int k = 0; k < cols; k++)
                result[i] += a[i, k] * b[k];
        return result;
    }

    // 求解线性方程组 (高斯消元法)
    static double[] Solve(double[,] A, double[] b)
    {
        int n = b.Length;
        double[,] aug = new double[n, n + 1];

        // 构建增广矩阵
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                aug[i, j] = A[i, j];
            aug[i, n] = b[i];
        }

        // 前向消元
        for (int i = 0; i < n; i++)
        {
            // 寻找主元
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
                if (Math.Abs(aug[k, i]) > Math.Abs(aug[maxRow, i]))
                    maxRow = k;

            // 交换行
            if (maxRow != i)
                for (int k = i; k <= n; k++)
                    (aug[i, k], aug[maxRow, k]) = (aug[maxRow, k], aug[i, k]);

            // 归一化
            double div = aug[i, i];
            if (div == 0) throw new InvalidOperationException("矩阵奇异");

            for (int k = i; k <= n; k++)
                aug[i, k] /= div;

            // 消元
            for (int k = i + 1; k < n; k++)
            {
                double factor = aug[k, i];
                for (int j = i; j <= n; j++)
                    aug[k, j] -= factor * aug[i, j];
            }
        }

        // 回代
        double[] x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = aug[i, n];
            for (int j = i + 1; j < n; j++)
                x[i] -= aug[i, j] * x[j];
        }
        return x;
    }

    // 应用校正
    static double[] ApplyCorrection(double[] point, double[] coeffX, double[] coeffY)
    {
        double x = point[0], y = point[1];
        double[] terms = { 1, x, y, x * x, x * y, y * y };
        return new[] {
            terms.Zip(coeffX, (t, c) => t * c).Sum(),
            terms.Zip(coeffY, (t, c) => t * c).Sum()
        };
    }

    public IList<Point> CorrectionCoordinate(IList<Point> path) => GetPath(path);
}
