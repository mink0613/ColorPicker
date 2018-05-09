using AutomatedPackagingSystem.Common;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker
{
    public class ColorPickerViewModel : BaseViewModel
    {
        private readonly int _captureWidth = 50;

        private readonly int _captureHeight = 50;

        private readonly int _pixelWidth = 1;

        private readonly int _pixelHeight = 1;

        private Bitmap _screen;

        private Size _currentSize;

        private Graphics _captureGraphics;

        private ImageSource _screenPreview;

        private ImageSource _pixelPreview;

        private Timer _mainThread;

        private Timer _pixelThread;

        private ICommand _captureClick;

        private ICommand _releaseClick;

        private ICommand _selectPixcelClick;

        private ICommand _RGBCopyClick;

        private ICommand _hexCodeCopyClick;

        private string _RGBValue;

        private string _hexCodeValue;

        private bool _isCaptured;

        public ImageSource ScreenPreview
        {
            get
            {
                return _screenPreview;
            }
            set
            {
                _screenPreview = value;
                OnPropertyChanged();
            }
        }

        public ImageSource PixelPreview
        {
            get
            {
                return _pixelPreview;
            }
            set
            {
                _pixelPreview = value;
                OnPropertyChanged();
            }
        }

        public ICommand CaptureClick
        {
            get
            {
                return _captureClick;
            }
        }

        public ICommand ReleaseClick
        {
            get
            {
                return _releaseClick;
            }
        }

        public ICommand RGBCopyClick
        {
            get
            {
                return _RGBCopyClick;
            }
        }

        public ICommand HexCodeCopyClick
        {
            get
            {
                return _hexCodeCopyClick;
            }
        }

        public ICommand SelectPixcelClick
        {
            get
            {
                return _selectPixcelClick;
            }
        }

        public string RGBValue
        {
            get
            {
                return _RGBValue;
            }
            set
            {
                _RGBValue = value;
                OnPropertyChanged();
            }
        }

        public string HexCodeValue
        {
            get
            {
                return _hexCodeValue;
            }
            set
            {
                _hexCodeValue = value;
                OnPropertyChanged();
            }
        }

        private void Initialize()
        {
            _screen = new Bitmap(_captureWidth, _captureHeight);
            _currentSize = new Size(_captureWidth, _captureHeight);
            _captureGraphics = Graphics.FromImage(_screen);

            _captureClick = new RelayCommand((param) => OnCaptureClick(param), true);
            ((RelayCommand)_captureClick).GestureKey = Key.F6;

            _releaseClick = new RelayCommand((param) => OnResetClick(param), true);
            ((RelayCommand)_releaseClick).GestureKey = Key.F7;

            _selectPixcelClick = new RelayCommand((param) => OnSelectPixelClick(param), true);

            _RGBCopyClick = new RelayCommand((param) => OnRGBCopyClick(param), true);
            _hexCodeCopyClick = new RelayCommand((param) => OnHexCodeCopyClick(param), true);
        }

        private void OnCaptureClick(object param)
        {
            if(_isCaptured == false)
            {
                _mainThread.Stop();
                _isCaptured = true;
                RunCapturePixel();
            }
        }

        private void OnResetClick(object param)
        {
            if(_isCaptured == true)
            {
                _mainThread.Start();
                _isCaptured = false;
            }
        }

        private void OnSelectPixelClick(object param)
        {
            if(_isCaptured == true)
            {
                _pixelThread.Stop();
                BitmapSource bitmapImage = (BitmapSource)PixelPreview;

                int height = bitmapImage.PixelHeight;
                int width = bitmapImage.PixelWidth;
                int nStride = (bitmapImage.PixelWidth * bitmapImage.Format.BitsPerPixel + 7) / 8;
                byte[] pixelByteArray = new byte[bitmapImage.PixelHeight * nStride];
                bitmapImage.CopyPixels(pixelByteArray, nStride, 0);

                RGBValue = string.Format("R: {0}\nG: {1}\nB: {2}", pixelByteArray[2], pixelByteArray[1], pixelByteArray[0]);

                System.Drawing.Color myColor = System.Drawing.Color.FromArgb(pixelByteArray[1], pixelByteArray[2], pixelByteArray[3]);

                HexCodeValue = string.Format("{0:X2}{1:X2}{2:X2}", pixelByteArray[2], pixelByteArray[1], pixelByteArray[0]);// myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");
            }
        }

        private void OnRGBCopyClick(object param)
        {
            if(RGBValue != null && RGBValue != "")
            {
                Clipboard.SetText(RGBValue);
                MessageBox.Show("Copied!");
            }
        }

        private void OnHexCodeCopyClick(object param)
        {
            if(HexCodeValue != null && HexCodeValue != "")
            {
                Clipboard.SetText(HexCodeValue);
                MessageBox.Show("Copied!");
            }
        }

        private Image CaptureWindow(IntPtr handle, int pointX = 0, int pointY = 0, int targetWidth = 0, int targetHeight = 0)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32Helper.GetWindowDC(handle);
            
            // get the size
            User32Helper.RECT windowRect = new User32Helper.RECT();
            User32Helper.GetWindowRect(handle, ref windowRect);

            int width, height;
            if (targetWidth == 0 || targetHeight == 0)
            {
                width = windowRect.right - windowRect.left;
                height = windowRect.bottom - windowRect.top;
            }
            else
            {
                width = targetWidth;
                height = targetHeight;
            }

            // create a device context we can copy to
            IntPtr hdcDest = GDI32Helper.CreateCompatibleDC(hdcSrc);
            
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32Helper.CreateCompatibleBitmap(hdcSrc, width, height);
            
            // select the bitmap object
            IntPtr hOld = GDI32Helper.SelectObject(hdcDest, hBitmap);
            
            // bitblt over
            GDI32Helper.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, pointX, pointY, GDI32Helper.SRCCOPY);
            
            // restore selection
            GDI32Helper.SelectObject(hdcDest, hOld);
            
            // clean up 
            GDI32Helper.DeleteDC(hdcDest);
            User32Helper.ReleaseDC(handle, hdcSrc);
            
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32Helper.DeleteObject(hBitmap);
            return img;
        }

        private ImageSource ConvertBitmapToImageSource(Bitmap image)
        {
            using (Stream str = new MemoryStream())
            {
                image.Save(str, System.Drawing.Imaging.ImageFormat.Bmp);
                str.Seek(0, SeekOrigin.Begin);
                BitmapDecoder bdc = new BmpBitmapDecoder(str, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return bdc.Frames[0];
            }
        }

        private ImageSource ConvertImageToImageSource(Image image)
        {
            using (Stream str = new MemoryStream())
            {
                image.Save(str, System.Drawing.Imaging.ImageFormat.Bmp);
                str.Seek(0, SeekOrigin.Begin);
                BitmapDecoder bdc = new BmpBitmapDecoder(str, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return bdc.Frames[0];
            }
        }

        private ImageSource CaptureScreen()
        {
            Point point = System.Windows.Forms.Cursor.Position;
            Console.WriteLine("X: " + point.X + "\t Y: " + point.Y);
            point.X -= _captureWidth / 2;
            point.Y -= _captureHeight / 2;

            //_captureGraphics.CopyFromScreen(point.X, point.Y, 0, 0, _currentSize, CopyPixelOperation.SourceCopy);
            var image = CaptureWindow(User32Helper.GetDesktopWindow(), point.X, point.Y, _captureWidth, _captureHeight);

            return ConvertImageToImageSource(image);
        }

        private ImageSource CapturePixel()
        {
            Point point = System.Windows.Forms.Cursor.Position;
            Console.WriteLine("X: " + point.X + "\t Y: " + point.Y);
            point.X -= _pixelWidth / 2;
            point.Y -= _pixelHeight / 2;

            //_captureGraphics.CopyFromScreen(point.X, point.Y, 0, 0, _currentSize, CopyPixelOperation.SourceCopy);
            var image = CaptureWindow(User32Helper.GetDesktopWindow(), point.X, point.Y, _pixelWidth, _pixelHeight);

            return ConvertImageToImageSource(image);
        }

        private void RunCaptureScreen()
        {
            _mainThread = new Timer();
            _mainThread.Interval = 50;
            _mainThread.Tick += MainThreadTick;
            _mainThread.Start();
        }

        private void RunCapturePixel()
        {
            _pixelThread = new Timer();
            _pixelThread.Interval = 50;
            _pixelThread.Tick += PixelThreadTick;
            _pixelThread.Start();
        }

        private void MainThreadTick(object sender, System.EventArgs e)
        {
            _mainThread.Stop();

            ScreenPreview = CaptureScreen();

            _mainThread.Start();

        }
        private void PixelThreadTick(object sender, EventArgs e)
        {
            _pixelThread.Stop();

            PixelPreview = CapturePixel();

            _pixelThread.Start();
        }

        public ColorPickerViewModel()
        {
            Initialize();
            RunCaptureScreen();
        }
    }
}
