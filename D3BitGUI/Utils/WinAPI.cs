using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace D3BitGUI.Utils
{
	public static class WinAPI
	{
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		public const int CS_DROPSHADOW = 0x00020000;

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();
		
		[DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
		public static extern System.IntPtr CreateRoundRectRgn
		(
		 int nLeftRect, // x-coordinate of upper-left corner
		 int nTopRect, // y-coordinate of upper-left corner
		 int nRightRect, // x-coordinate of lower-right corner
		 int nBottomRect, // y-coordinate of lower-right corner
		 int nWidthEllipse, // height of ellipse
		 int nHeightEllipse // width of ellipse
		);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		public static extern bool DeleteObject(System.IntPtr hObject);

		[DllImport("user32", SetLastError = true)]
		public static extern int GetForegroundWindow();

		[DllImport("user32", SetLastError = true)]
		public static extern int GetWindowThreadProcessId(int hwnd, ref int lProcessId);

		public static int GetProcessThreadFromWindow(int hwnd)
		{
			int procid = 0;
			int threadid = GetWindowThreadProcessId(hwnd, ref procid);
			return procid;
		}

		public static int GetForegroundProcessId()
		{
			int hwnd = GetForegroundWindow();
			return GetProcessThreadFromWindow(hwnd);
		}
	}
}
