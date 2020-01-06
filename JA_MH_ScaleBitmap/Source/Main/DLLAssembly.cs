using System.Runtime.InteropServices;

namespace BitmapScale
{
    public static class DLLAssembly
    {
        private const string _path = @"\..\..\..\..\x64\Release\DLLAssembly.dll";

        [DllImport(_path)] public static extern void ScaleBitmap(int start, int end, float ratioX, float ratioY, int widthIn, int widthOut, byte[] bitmapOut, byte[] bitmap);
    }
}
