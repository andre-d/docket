using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace docket
{

    class Win32
    {

        // ReSharper disable InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            int x;
            int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;    // x offest from the upperleft of bitmap
            public int yBitmap;    // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGEINFO
        {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public RECT rcImage;
        }

        #region Private ImageList COM Interop (XP)
        [ComImportAttribute]
        [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        //helpstring("Image List"),
        public interface IImageList
        {
            [PreserveSig]
            int Add(
                IntPtr hbmImage,
                IntPtr hbmMask,
                ref int pi);

            [PreserveSig]
            int ReplaceIcon(
                int i,
                IntPtr hicon,
                ref int pi);

            [PreserveSig]
            int SetOverlayImage(
                int iImage,
                int iOverlay);

            [PreserveSig]
            int Replace(
                int i,
                IntPtr hbmImage,
                IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(
                IntPtr hbmImage,
                int crMask,
                ref int pi);

            [PreserveSig]
            int Draw(
                ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(
            int i);

            [PreserveSig]
            int GetIcon(
                int i,
                int flags,
                ref IntPtr picon);

            [PreserveSig]
            int GetImageInfo(
                int i,
                ref IMAGEINFO pImageInfo);

            [PreserveSig]
            int Copy(
                int iDst,
                IImageList punkSrc,
                int iSrc,
                int uFlags);

            [PreserveSig]
            int Merge(
                int i1,
                IImageList punk2,
                int i2,
                int dx,
                int dy,
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int Clone(
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int GetImageRect(
                int i,
                ref RECT prc);

            [PreserveSig]
            int GetIconSize(
                ref int cx,
                ref int cy);

            [PreserveSig]
            int SetIconSize(
                int cx,
                int cy);

            [PreserveSig]
            int GetImageCount(
            ref int pi);

            [PreserveSig]
            int SetImageCount(
                int uNewCount);

            [PreserveSig]
            int SetBkColor(
                int clrBk,
                ref int pclr);

            [PreserveSig]
            int GetBkColor(
                ref int pclr);

            [PreserveSig]
            int BeginDrag(
                int iTrack,
                int dxHotspot,
                int dyHotspot);

            [PreserveSig]
            int EndDrag();

            [PreserveSig]
            int DragEnter(
                IntPtr hwndLock,
                int x,
                int y);

            [PreserveSig]
            int DragLeave(
                IntPtr hwndLock);

            [PreserveSig]
            int DragMove(
                int x,
                int y);

            [PreserveSig]
            int SetDragCursorImage(
                ref IImageList punk,
                int iDrag,
                int dxHotspot,
                int dyHotspot);

            [PreserveSig]
            int DragShowNolock(
                int fShow);

            [PreserveSig]
            int GetDragImage(
                ref POINT ppt,
                ref POINT pptHotspot,
                ref Guid riid,
                ref IntPtr ppv);

            [PreserveSig]
            int GetItemFlags(
                int i,
                ref int dwFlags);

            [PreserveSig]
            int GetOverlayImage(
                int iOverlay,
                ref int piIndex);
        };
        #endregion

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_SYSICONINDEX = 0x4000;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

        public const int SHIL_JUMBO = 0x4;
        public const int SHIL_EXTRALARGE = 0x2;

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern UInt32 PrivateExtractIcons(String lpszFile, int nIconIndex, int cxIcon, int cyIcon, IntPtr[] phicon, IntPtr[] piconid, UInt32 nIcons, UInt32 flags);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        // ReSharper restore InconsistentNaming

    }


    class Utils {
        public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            var stream = new MemoryStream();
            var bitmapImage = new BitmapImage();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public static Icon ExtractIcon(String path)
        {
            var phicon = new[] { IntPtr.Zero };
            var piconid = new[] { IntPtr.Zero };

            Win32.PrivateExtractIcons(path, 0, 256, 256, phicon, piconid, 1, 0);

            if (phicon[0] != IntPtr.Zero)
            {
                return Icon.FromHandle(phicon[0]);
            }

            return null;
        }

        static public Bitmap TrimBitmap(Bitmap source)
        {
            Rectangle srcRect;
            BitmapData data = null;
            try
            {
                data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var buffer = new byte[data.Height * data.Stride];
                Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

                int xMax = int.MinValue,
                    yMax = int.MinValue;

                bool foundPixel = false;

                // Find xMax
                for (int x = data.Width - 1; x >= 0; x--)
                {
                    bool stop = false;
                    for (int y = 0; y < data.Height; y++)
                    {
                        byte alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha != 0)
                        {
                            xMax = x;
                            stop = true;
                            foundPixel = true;
                            break;
                        }
                    }
                    if (stop)
                        break;
                }

                // Image is empty...
                if (!foundPixel)
                    return null;

                // Find yMax
                for (int y = data.Height - 1; y >= 0; y--)
                {
                    bool stop = false;
                    for (int x = 0; x <= xMax; x++)
                    {
                        byte alpha = buffer[y * data.Stride + 4 * x + 3];
                        if (alpha != 0)
                        {
                            yMax = y;
                            stop = true;
                            break;
                        }
                    }
                    if (stop)
                        break;
                }


                srcRect = Rectangle.FromLTRB(0, 0, xMax, yMax);
            }
            finally
            {
                if (data != null)
                    source.UnlockBits(data);
            }

            var dest = new Bitmap(srcRect.Width, srcRect.Height);
            var destRect = new Rectangle(0, 0, srcRect.Width, srcRect.Height);
            using (Graphics graphics = Graphics.FromImage(dest))
            {
                graphics.DrawImage(source, destRect, srcRect, GraphicsUnit.Pixel);
            }
            return dest;
        }

        public static Icon GetShellIcon(String path)
        {
            try
            {
                return ExtractIcon(path) ??
                       GetImageListIcon(path) ??
                       ExtractIconFi(path) ??
                       Icon.ExtractAssociatedIcon(@path);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static Icon ExtractIconFi(String path)
        {
            var shinfo = new Win32.SHFILEINFO();
            Win32.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | 4);
            if (shinfo.hIcon != IntPtr.Zero)
            {
                return Icon.FromHandle(shinfo.hIcon);
            }

            return null;
        }

        public static Icon GetImageListIcon(String path)
        {
            var type = IsHigherThanXp() ? Win32.SHIL_JUMBO : Win32.SHIL_EXTRALARGE;

            var iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            var shinfo = new Win32.SHFILEINFO();
            Win32.SHGetFileInfo(path, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), Win32.SHGFI_SYSICONINDEX);

            if (shinfo.iIcon == IntPtr.Zero)
            {
                return null;
            }

            int index = shinfo.iIcon.ToInt32();

            Win32.IImageList iml;

            Win32.SHGetImageList(type, ref iidImageList, out iml);

            var flags = 0;
            var handle = IntPtr.Zero;

            iml.GetItemFlags(index, ref flags);

            iml.GetIcon(index, 1, ref handle);

            if (handle != IntPtr.Zero)
            {
                return Icon.FromHandle(handle);
            }

            return null;
        }

        public static bool IsHigherThanXp()
        {
            var osInfo = Environment.OSVersion;
            return (osInfo.Platform == PlatformID.Win32NT && osInfo.Version.Major > 5);
        }
    }

}
