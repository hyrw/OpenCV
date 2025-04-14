
/// <summary>
/// 分段线性插值
/// </summary>
public class BilinearInterpolator
{
    readonly double[] _x;
    readonly double[] _y;
    readonly double[,] _z;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x">x 轴的点</param>
    /// <param name="y">y 轴的点</param>
    /// <param name="z">x,y 对应的值</param>
    public BilinearInterpolator(double[] x, double[] y, double[,] z)
    {
        _x = x;
        _y = y;
        _z = z;
    }

    public double Interpolate(double x, double y)
    {
        // 查找 x 方向的区间
        int i = Array.BinarySearch(_x, x);
        if (i < 0) i = ~i - 1;
        i = Math.Clamp(i, 0, _x.Length - 2);

        // 查找 y 方向的区间
        int j = Array.BinarySearch(_y, y);
        if (j < 0) j = ~j - 1;
        j = Math.Clamp(j, 0, _y.Length - 2);

        // 提取四个角点的值和坐标
        double x0 = _x[i], x1 = _x[i + 1];
        double y0 = _y[j], y1 = _y[j + 1];
        double z00 = _z[i, j], z01 = _z[i, j + 1];
        double z10 = _z[i + 1, j], z11 = _z[i + 1, j + 1];

        // 计算插值权重
        double dx = x1 - x0;
        double dy = y1 - y0;
        double tx = (x - x0) / dx;
        double ty = (y - y0) / dy;

        // 双线性插值公式
        return (1 - tx) * (1 - ty) * z00
             + tx * (1 - ty) * z10
             + (1 - tx) * ty * z01
             + tx * ty * z11;
    }
}
