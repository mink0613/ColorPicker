using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker.Helper
{
    public class ScreenCaptureHelper
    {
        public static Image CaptureWindow(IntPtr handle, int pointX = 0, int pointY = 0, int targetWidth = 0, int targetHeight = 0)
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

        public static ImageSource ConvertImageToImageSource(Image image)
        {
            using (Stream str = new MemoryStream())
            {
                image.Save(str, System.Drawing.Imaging.ImageFormat.Bmp);
                str.Seek(0, SeekOrigin.Begin);
                BitmapDecoder bdc = new BmpBitmapDecoder(str, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return bdc.Frames[0];
            }
        }
    }
}
