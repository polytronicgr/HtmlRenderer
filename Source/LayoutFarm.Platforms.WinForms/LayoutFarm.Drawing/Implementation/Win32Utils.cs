﻿// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace DrawingBridge
{
    /// <summary>
    /// Utility for Win32 API.
    /// </summary>
    static class Win32Utils
    {
        /// <summary>
        /// Const for BitBlt copy raster-operation code.
        /// </summary>
        public const int BitBltCopy = 0x00CC0020;

        /// <summary>
        /// Const for BitBlt paint raster-operation code.
        /// </summary>
        public const int BitBltPaint = 0x00EE0086;

        /// <summary>
        /// Create a compatible memory HDC from the given HDC.<br/>
        /// The memory HDC can be rendered into without effecting the original HDC.<br/>
        /// The returned memory HDC and <paramref name="dib"/> must be released using <see cref="ReleaseMemoryHdc"/>.
        /// </summary>
        /// <param name="hdc">the HDC to create memory HDC from</param>
        /// <param name="width">the width of the memory HDC to create</param>
        /// <param name="height">the height of the memory HDC to create</param>
        /// <param name="dib">returns used bitmap memory section that must be released when done with memory HDC</param>
        /// <returns>memory HDC</returns>
        public static IntPtr CreateMemoryHdc(IntPtr hdc, int width, int height, out IntPtr dib)
        {
            // Create a memory DC so we can work off-screen
            IntPtr memoryHdc = CreateCompatibleDC(hdc);
            SetBkMode(memoryHdc, 1);

            // Create a device-independent bitmap and select it into our DC
            var info = new LayoutFarm.BitMapInfo();
            info.biSize = Marshal.SizeOf(info);
            info.biWidth = width;
            info.biHeight = -height;
            info.biPlanes = 1;
            info.biBitCount = 32;
            info.biCompression = 0; // BI_RGB
            IntPtr ppvBits;
            dib = CreateDIBSection(hdc, ref info, 0, out ppvBits, IntPtr.Zero, 0);
            SelectObject(memoryHdc, dib);

            return memoryHdc;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hdc);
        
        
        /// <summary>
        /// Release the given memory HDC and dib section created from <see cref="CreateMemoryHdc"/>.
        /// </summary>
        /// <param name="memoryHdc">Memory HDC to release</param>
        /// <param name="dib">bitmap section to release</param>
        public static void ReleaseMemoryHdc(IntPtr memoryHdc, IntPtr dib)
        {
            DeleteObject(dib);
            DeleteDC(memoryHdc);
        }

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromDC(IntPtr hdc);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <remarks>
        /// In conformance with conventions for the RECT structure, the bottom-right coordinates of the returned rectangle are exclusive. In other words, 
        /// the pixel at (right, bottom) lies immediately outside the rectangle.
        /// </remarks>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpRect">A pointer to a RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("User32", SetLastError = true)]
        public static extern int GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        /// <summary>
        /// Retrieves the dimensions of the bounding rectangle of the specified window. The dimensions are given in screen coordinates that are relative to the upper-left corner of the screen.
        /// </summary>
        /// <remarks>
        /// In conformance with conventions for the RECT structure, the bottom-right coordinates of the returned rectangle are exclusive. In other words, 
        /// the pixel at (right, bottom) lies immediately outside the rectangle.
        /// </remarks>
        /// <param name="handle">A handle to the window.</param>
        /// <returns>RECT structure that receives the screen coordinates of the upper-left and lower-right corners of the window.</returns>
        public static Rectangle GetWindowRectangle(IntPtr handle)
        {
            Rectangle rect;
            GetWindowRect(handle, out rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hdc, int mode);

        [DllImport("gdi32.dll")]
        public static extern int SelectObject(IntPtr hdc, IntPtr hgdiObj);

        [DllImport("gdi32.dll")]
        public static extern int SetTextColor(IntPtr hdc, int color);

        [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W")]
        public static extern int GetTextExtentPoint32(IntPtr hdc, [MarshalAs(UnmanagedType.LPWStr)] string str, int len, ref Size size);

        [DllImport("gdi32.dll", EntryPoint = "GetTextExtentPoint32W")]
        public static unsafe extern int UnsafeGetTextExtentPoint32(
            IntPtr hdc, char* str, int len, ref Size size);


        [DllImport("gdi32.dll", EntryPoint = "GetTextExtentExPointW")]
        public static extern bool GetTextExtentExPoint(IntPtr hDc, [MarshalAs(UnmanagedType.LPWStr)]string str, int nLength, int nMaxExtent, int[] lpnFit, int[] alpDx, ref Size size);

        [DllImport("gdi32.dll", EntryPoint = "GetTextExtentExPointW")]
        public static unsafe extern bool UnsafeGetTextExtentExPoint(
            IntPtr hDc, char* str, int len, int nMaxExtent, int[] lpnFit, int[] alpDx, ref System.Drawing.Size size);



        [DllImport("gdi32.dll", EntryPoint = "TextOutW")]
        public static extern bool TextOut(IntPtr hdc, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string str, int len);


        [DllImport("gdi32.dll", EntryPoint = "TextOutW")]
        public static unsafe extern bool TextOut2(IntPtr hdc, int x, int y, char* s, int len);


        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern int GetClipBox(IntPtr hdc, out Rectangle lprc);

        [DllImport("gdi32.dll")]
        public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll", EntryPoint = "GdiAlphaBlend")]
        public static extern bool AlphaBlend(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, LayoutFarm.BlendFunction blendFunction);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref  LayoutFarm.BitMapInfo pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);
    }


}