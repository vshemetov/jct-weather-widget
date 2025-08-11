using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace JctWeatherWidget.Helpers;

public static class JctBitmapImageHelper
{
    public static WriteableBitmap ApplyBlackAndWhite(this BitmapImage source)
    {
        var writeableBitmap = new WriteableBitmap(source);

        int width = writeableBitmap.PixelWidth;
        int height = writeableBitmap.PixelHeight;
        int stride = width * 4; // (BGRA)
        byte[] pixels = new byte[height * stride];

        writeableBitmap.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];
            byte gray = (byte)(0.92 * (0.299 * r + 0.587 * g + 0.114 * b));
            pixels[i] = pixels[i + 1] = pixels[i + 2] = gray;
        }

        writeableBitmap.WritePixels(
            new Int32Rect(0, 0, width, height),
            pixels,
            stride,
            0);

        return writeableBitmap;
    }

    public static WriteableBitmap ApplyGrayscale(this BitmapImage source)
    {
        var wb = new WriteableBitmap(source);
        int width = wb.PixelWidth;
        int height = wb.PixelHeight;
        int stride = width * 4;
        var pixels = new byte[height * stride];

        wb.CopyPixels(pixels, stride, 0);

        const double ContrastFactor = 1.4; // 1.0 = без изменений, 1.4–1.8 = сильнее
        const double Midpoint = 128.0;

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];

            // Яркость (оттенок серого)
            double gray = 0.299 * r + 0.587 * g + 0.114 * b;

            // Усиливаем контраст: растягиваем градации
            double adjusted = (gray - Midpoint) * ContrastFactor + Midpoint;

            // Ограничиваем диапазон
            byte finalGray = (byte)Clamp(adjusted, 0, 255);

            pixels[i] = pixels[i + 1] = pixels[i + 2] = finalGray;
            // Альфа остаётся неизменной: pixels[i + 3]
        }

        var result = new WriteableBitmap(width, height, wb.DpiX, wb.DpiY, PixelFormats.Bgra32, null);
        result.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        return result;
    }

    private static double Clamp(double value, double min, double max)
    {
        return value < min ? min : value > max ? max : value;
    }
}
