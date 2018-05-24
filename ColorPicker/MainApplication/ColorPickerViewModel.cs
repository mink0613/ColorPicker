using AutomatedPackagingSystem.Common;
using ColorPicker.Helper;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker.MainApplication
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

        private ICommand _selectPixcelClick;

        private ICommand _RGBCopyClick;

        private ICommand _hexCodeCopyClick;

        private string _buttonContent;

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

        public string ButtonContent
        {
            get
            {
                return _buttonContent;
            }
            set
            {
                _buttonContent = value;
                OnPropertyChanged();
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
            ((RelayCommand)_captureClick).GestureKey = Key.F2;

            _selectPixcelClick = new RelayCommand((param) => OnSelectPixelClick(param), true);

            _RGBCopyClick = new RelayCommand((param) => OnRGBCopyClick(param), true);
            _hexCodeCopyClick = new RelayCommand((param) => OnHexCodeCopyClick(param), true);

            ButtonContent = "Capture (F2)";
        }

        private void OnCaptureClick(object param)
        {
            if (_isCaptured == false)
            {
                _mainThread.Stop();
                _isCaptured = true;
                ButtonContent = "Release (F2)";
                RunCapturePixel();
            }
            else
            {
                StopCapturePixel();
                _mainThread.Start();
                ButtonContent = "Capture (F2)";
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

                HexCodeValue = string.Format("{0:X2}{1:X2}{2:X2}", pixelByteArray[2], pixelByteArray[1], pixelByteArray[0]);

                _pixelThread.Start();
            }
        }

        private void OnRGBCopyClick(object param)
        {
            if(RGBValue != null && RGBValue != "")
            {
                Clipboard.SetText(RGBValue);
                MessageBox.Show("Copied to clipboard!");
            }
        }

        private void OnHexCodeCopyClick(object param)
        {
            if(HexCodeValue != null && HexCodeValue != "")
            {
                Clipboard.SetText(HexCodeValue);
                MessageBox.Show("Copied to clipboard!");
            }
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

        private ImageSource CaptureScreen()
        {
            Point point = System.Windows.Forms.Cursor.Position;
            Console.WriteLine("X: " + point.X + "\t Y: " + point.Y);
            point.X -= _captureWidth / 2;
            point.Y -= _captureHeight / 2;
            
            var image = ScreenCaptureHelper.CaptureWindow(User32Helper.GetDesktopWindow(), point.X, point.Y, _captureWidth, _captureHeight);

            return ScreenCaptureHelper.ConvertImageToImageSource(image);
        }

        private ImageSource CapturePixel()
        {
            Point point = System.Windows.Forms.Cursor.Position;
            Console.WriteLine("X: " + point.X + "\t Y: " + point.Y);
            point.X -= _pixelWidth / 2;
            point.Y -= _pixelHeight / 2;
            
            var image = ScreenCaptureHelper.CaptureWindow(User32Helper.GetDesktopWindow(), point.X, point.Y, _pixelWidth, _pixelHeight);

            return ScreenCaptureHelper.ConvertImageToImageSource(image);
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
            StopCapturePixel();

            _pixelThread = new Timer();
            _pixelThread.Interval = 50;
            _pixelThread.Tick += PixelThreadTick;
            _pixelThread.Start();
        }

        private void StopCapturePixel()
        {
            if (_pixelThread != null)
            {
                _pixelThread.Stop();
                _pixelThread = null;
            }
        }

        private void MainThreadTick(object sender, EventArgs e)
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
