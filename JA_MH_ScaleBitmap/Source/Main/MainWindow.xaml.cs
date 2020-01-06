using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BitmapScale
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _threadAmount = 1; // Environment.ProcessorCount; //current amount of used threads
        public byte[] bitmap; //bitmap
        public byte[] bitmapOut;
        private Bitmap _bmp;

        private int _widthIn;
        private int _heightIn;
        private const int header = 54;
        private int _widthOut;
        private int _heightOut;
        private float _ratioY;
        private float _ratioX;
        private int _end = 54;
        private int _start = 54;

        private readonly List<ManualResetEvent> _events;

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        private byte[] createBitmapOut(int width, int height)
        {
            byte[] bmp;
            int BmpBufferSize = width * height * 3;

            bmp = new byte[width * height * 3 + 54];
            bmp[0] = 0x42;
            bmp[1] = 0x4d;

            // 2~6 Size of the BMP file - Bit cound + Header 54
            Array.Copy(BitConverter.GetBytes(BmpBufferSize + 54), 0, bmp, 2, 4);

            // 6~8 Application Specific : normally, set zero
            Array.Copy(BitConverter.GetBytes(0), 0, bmp, 6, 2);

            // 8~10 Application Specific : normally, set zero
            Array.Copy(BitConverter.GetBytes(0), 0, bmp, 8, 2);

            // 10~14 Offset where the pixel array can be found - 24bit bitmap data always starts at 54 offset.
            Array.Copy(BitConverter.GetBytes(54), 0, bmp, 10, 4);

            // 14~18 Number of bytes in the DIB header. 40 bytes constant.
            Array.Copy(BitConverter.GetBytes(40), 0, bmp, 14, 4);

            // 18~22 Width of the bitmap.
            Array.Copy(BitConverter.GetBytes(width), 0, bmp, 18, 4);

            // 22~26 Height of the bitmap.
            Array.Copy(BitConverter.GetBytes(height), 0, bmp, 22, 4);

            // 26~28 Number of color planes being used
            Array.Copy(BitConverter.GetBytes(0), 0, bmp, 26, 2);

            // 28~30 Number of bits. If you don't know the pixel format, trying to calculate it with the quality of the video/image source.
        
            Array.Copy(BitConverter.GetBytes(24), 0, bmp, 28, 2);

            // 30~34 BI_RGB no pixel array compression used : most of the time, just set zero if it is raw data.
            Array.Copy(BitConverter.GetBytes(0), 0, bmp, 30, 4);

            // 34~38 Size of the raw bitmap data ( including padding )
            Array.Copy(BitConverter.GetBytes(BmpBufferSize), 0, bmp, 34, 4);

            // 38~46 Print resolution of the image, 72 DPI x 39.3701 inches per meter yields

           Array.Copy(BitConverter.GetBytes(0), 0, bmp, 38, 4);
           Array.Copy(BitConverter.GetBytes(0), 0, bmp, 42, 4);


            // 46~50 Number of colors in the palette
            Array.Copy(BitConverter.GetBytes(0), 0, bmp, 46, 4);

            // 50~54 means all colors are important
            Array.Copy(BitConverter.GetBytes(0), 0, bmp, 50, 4);

            // 54~end : Pixel Data : Finally, time to combine your raw data, BmpBuffer in this code, with a bitmap header you've just created.
            Array.Copy(bmp as Array, 0, bmp, 54, BmpBufferSize);

            return bmp;
        }

        public MainWindow()
        {
            InitializeComponent();
            slider.Value = Environment.ProcessorCount;
            slider.Minimum = 1;
            slider.Maximum = 32;
            _events = new List<ManualResetEvent>();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Bitmap (*.bmp)|*.bmp";
            file.ShowDialog();
            _bmp = new Bitmap(file.FileName);
            bitmap = ImageToByte(_bmp);
        }

        private void TaskCallBackASM(Object end)
        {
            var bitmapCopy = (byte[])bitmap.Clone();
            DLLAssembly.ScaleBitmap(_start, (int)end, _ratioX, _ratioY, _widthIn, _widthOut, bitmapOut, bitmapCopy);
        }

        private void TaskCallBackC(Object end)
        {
            var bitmapCopy = (byte[])bitmap.Clone();
            DLLCpp.ScaleBitmap(bitmapCopy, _start, (int)end, _ratioX, _ratioY, _widthIn, _widthOut, bitmapOut);
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            
            int.TryParse(textBox.Text, out _widthOut);
            int.TryParse(textBox1.Text, out _heightOut);
            _widthIn = _bmp.Width;
            _heightIn = _bmp.Height;

            bitmapOut = createBitmapOut(_widthOut, _heightOut);

            if (bitmap == null)
                MessageBox.Show("No bitmap loaded", "Warning!", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                _ratioX = _widthIn / (float)_widthOut;
                _ratioY = _heightIn / (float)_heightOut;
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    var semaphoreObject = new CountdownEvent(_threadAmount);

                    for (int i = _threadAmount; i >= 1; i--)
                    {

                        _end = ((_heightOut * _widthOut * 3) / _threadAmount) * i + header;
                        _start = _end - ((_heightOut * _widthOut * 3) / _threadAmount) * i;

                        if ((bool)AsmRadioButton.IsChecked)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback((end) =>
                            {
                                TaskCallBackASM(end);
                                semaphoreObject.Signal();
                            }), _end);
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback((end) =>
                            {
                                TaskCallBackC(end);
                                semaphoreObject.Signal();
                            }), _end);
                        }
                    }

                    semaphoreObject.Wait();
                    semaphoreObject.Dispose();
                    watch.Stop();

                    textBox2.Text = (watch.Elapsed.TotalMilliseconds).ToString();
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Bitmap (*.bmp)|*.bmp";

                var result = saveDialog.ShowDialog();

                if (result == true)
                {
                    File.WriteAllBytes(saveDialog.FileName, bitmapOut);
                }
            }
        }

        private void textBox1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _threadAmount = (int)slider.Value;
            label3.Content = ((int)slider.Value).ToString();
        }
    }

}
