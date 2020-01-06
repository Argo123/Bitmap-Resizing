using System.Runtime.InteropServices;

namespace BitmapScale
{
    public static class DLLCpp
    {
        private const string _path = @"\..\..\..\..\x64\Release\DLLCfin.dll";

        [DllImport(_path)] public static extern void ScaleBitmap([MarshalAs(UnmanagedType.LPArray)] byte[] bitmap, int start, int end, float ratioX, float ratioY, int widthIn, int widthOut, [MarshalAs(UnmanagedType.LPArray)] byte[] bitmapOut);
    }
}
