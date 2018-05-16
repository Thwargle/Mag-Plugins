using System;

using Mag.Shared;

using Decal.Adapter;
using System.Windows.Forms;
using System.Drawing;

namespace MagTools.Client
{
	class WindowFrameRemover : IDisposable
	{
        public WindowFrameRemover()
		{
			try
			{
				CoreManager.Current.CharacterFilter.Login += new EventHandler<Decal.Adapter.Wrappers.LoginEventArgs>(CharacterFilter_Login);
			}
			catch (Exception ex) { Debug.LogException(ex); }
		}

		private bool disposed;

		public void Dispose()
		{
			Dispose(true);

			// Use SupressFinalize in case a subclass
			// of this type implements a finalizer.
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			// If you need thread safety, use a lock around these 
			// operations, as well as in your methods that use the resource.
			if (!disposed)
			{
				if (disposing)
				{
					CoreManager.Current.CharacterFilter.Login -= new EventHandler<Decal.Adapter.Wrappers.LoginEventArgs>(CharacterFilter_Login);
				}

				// Indicate that the instance has been disposed.
				disposed = true;
			}
		}

		void CharacterFilter_Login(object sender, Decal.Adapter.Wrappers.LoginEventArgs e)
		{
			try
			{
				if (!Settings.SettingsManager.Misc.RemoveWindowFrame.Value)
					return;

				RemoveWindowFrame();
			}
			catch (Exception ex) { Debug.LogException(ex); }
		}

		static void RemoveWindowFrame()
		{
			User32.RECT rect = new User32.RECT();

			User32.GetWindowRect(CoreManager.Current.Decal.Hwnd, ref rect);

			// 1686 1078 -> 1680 1050
			//Debug.WriteToChat((rect.Right - rect.Left) + " " + (rect.Bottom - rect.Top));

			const int GWL_STYLE = -16;
			const int WS_BORDER = 0x00800000; //window with border
			const int WS_DLGFRAME = 0x00400000; //window with double border but no title
			const int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

			int style = User32.GetWindowLong(CoreManager.Current.Decal.Hwnd, GWL_STYLE);

			User32.SetWindowLong(CoreManager.Current.Decal.Hwnd, GWL_STYLE, (style & ~WS_CAPTION));

			User32.MoveWindow(CoreManager.Current.Decal.Hwnd, rect.Left, rect.Top, (rect.Right - rect.Left) - TotalWindowFrameWidth, (rect.Bottom - rect.Top) - TotalWindowFrameHeight, true);
		}

		static int TotalWindowFrameWidth
		{
			get
			{
                User32.RECT wrect = new User32.RECT();
                User32.RECT crect = new User32.RECT();

                User32.GetWindowRect(CoreManager.Current.Decal.Hwnd, ref wrect);
                User32.GetClientRect(CoreManager.Current.Decal.Hwnd, ref crect);

                var returnWidth = wrect.Width - crect.Width;
                return returnWidth;
			}
		}

		static int TotalWindowFrameHeight
		{
			get
			{
                // This is a hack because I don't know how to get the windows current theme border information
                // So, I just compare the current form size to the known AC client window sizes

                User32.RECT wrect = new User32.RECT();
                User32.RECT crect = new User32.RECT();

                User32.GetWindowRect(CoreManager.Current.Decal.Hwnd, ref wrect);
                User32.GetClientRect(CoreManager.Current.Decal.Hwnd, ref crect);

                var returnHeight = wrect.Height - crect.Height;
                return returnHeight;

            }
		}
	}
}
