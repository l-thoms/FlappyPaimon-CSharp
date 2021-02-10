using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FlappyPaimon
{
	public partial class Form1 : Form
	{
		Stopwatch UIWatch = new Stopwatch();
		public Form1()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
			UIWatch.Start();
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
			ClientSize = new Size(1024, 768);
			pTimer.Elapsed += PTimer_Tick;
			RestAni.Animated = (object o, EventArgs a) => {
				if (RestAni.Description=="up")
				{
					RestAni.Description = "down";RestAni.From =10;RestAni.Restart();RestAni.To = -10;
				}
				else
				{
					RestAni.Description = "up";RestAni.From = -10;RestAni.Restart(); RestAni.To = 10;
				}
			};
			RestAni.Restart();
			pTimer.Start();
			RestChecker.Tick += (object o, EventArgs a) =>
			{
				if (RestAni.IsAnimating == false) RestAni.Restart();
			};
			InitDevices();
		}
		SharpDX.Direct2D1.Bitmap CloudBitmap, StoneBitmap, GroundBitmap, ForestBitmap,PNormal,PFly,TitleBitmap;
		void LoadImage()
		{
			CloudBitmap = ConvertBitmap(Properties.Resources.cloud);
			StoneBitmap = ConvertBitmap(Properties.Resources.stone);
			GroundBitmap = ConvertBitmap(Properties.Resources.ground);
			ForestBitmap = ConvertBitmap(Properties.Resources.forest);
			PNormal = ConvertBitmap(Properties.Resources.pNormal);
			PFly = ConvertBitmap(Properties.Resources.pFly);
			TitleBitmap = ConvertBitmap(Properties.Resources.title);
		}
		THAnimations.EasyAni RestAni = new THAnimations.EasyAni() { Description = "up", From = -10, To = 10,EasingFunction = THAnimations.EasingFunction.PowerInOut,Pow=2,Duration = 0.5 };
		System.Windows.Forms.Timer RestChecker = new System.Windows.Forms.Timer() { Interval = 1, Enabled = true };
		private void PTimer_Tick(object sender, EventArgs e)
		{
			pState = pState == 0 ? pState = 1 : pState = 0;
		}

		const int UI_HEIGHT = 768;
		const int BG_WIDTH = 2048;
		const int FOREST_WIDTH = 1604;

		int playState = 0;
		int pState = 0;
		System.Timers.Timer pTimer = new System.Timers.Timer() { Interval = 333,Enabled = true };
		bool EnableGPU = true;
		public void Render()
		{
			try
			{
				if (!EnableGPU)
			{
				#region GDI+

				float density = (float)ClientSize.Height / UI_HEIGHT;
				int UI_WIDTH = (int)(ClientSize.Width / density);
				System.Drawing.Bitmap b = new System.Drawing.Bitmap(UI_WIDTH / 2, UI_HEIGHT / 2);
				Graphics bg = Graphics.FromImage(b);
				bg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				bg.ScaleTransform(0.5f, 0.5f);
				bg.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

				//背景
				bg.DrawImage(Properties.Resources.cloud, new PointF(-UIWatch.ElapsedMilliseconds / 20 % BG_WIDTH, UI_HEIGHT - 424));
				int cloudComp = 0;
				while (cloudComp < UI_WIDTH)
				{
					cloudComp += BG_WIDTH;
					bg.DrawImage(Properties.Resources.cloud, new PointF(-UIWatch.ElapsedMilliseconds / 20 % BG_WIDTH + cloudComp, UI_HEIGHT - 424));
				}

				int forestComp = 0;
				bg.DrawImage(Properties.Resources.forest, new PointF(-UIWatch.ElapsedMilliseconds / 10 % FOREST_WIDTH, UI_HEIGHT - 424));
				while (forestComp < UI_WIDTH)
				{
					forestComp += FOREST_WIDTH;
					bg.DrawImage(Properties.Resources.forest, new PointF(-UIWatch.ElapsedMilliseconds / 10 % FOREST_WIDTH + forestComp, UI_HEIGHT - 424));
				}

				int bgComp = 0;
				bg.DrawImage(Properties.Resources.stone, new PointF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH, UI_HEIGHT - 424));
				bg.DrawImage(Properties.Resources.ground, new PointF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH, UI_HEIGHT - 424));
				while (bgComp < UI_WIDTH)
				{
					bgComp += BG_WIDTH;
					bg.DrawImage(Properties.Resources.stone, new PointF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - 424));
					bg.DrawImage(Properties.Resources.ground, new PointF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - 424));
				}
				if (playState == 0)
					bg.DrawImage(Properties.Resources.title, new PointF((UI_WIDTH - Properties.Resources.title.Width) / 2, 128));
				//Paimon
				if (playState == 0)
				{
					System.Drawing.Bitmap pCurrent = Properties.Resources.pNormal;
					if (pState == 0)
						pCurrent = Properties.Resources.pNormal;
					else if (pState == 1)
						pCurrent = Properties.Resources.pFly;
					bg.DrawImage(pCurrent, (UI_WIDTH - pCurrent.Width) / 2, (int)((UI_HEIGHT - pCurrent.Height) / 2 + RestAni.GetValue()), pCurrent.Width, pCurrent.Height);
				}
				//最终输出
				System.Drawing.Bitmap scaler = new System.Drawing.Bitmap(UI_WIDTH * (int)(density + 1), UI_HEIGHT * (int)(density + 1));
				Graphics sg = Graphics.FromImage(scaler);
				sg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				sg.DrawImage(b, 0, 0, scaler.Width, scaler.Height);
				//g.DrawImage(scaler, 0,0,UI_WIDTH,UI_HEIGHT);
				GameUI.BackgroundImage = scaler;
				bg.Dispose();
				b.Dispose();
				//scaler.Dispose();
				sg.Dispose();

				#endregion
			}

			else
			{
					#region Direct2D
					GameUI.BackgroundImage = null;
					float density = (float)ClientSize.Height / UI_HEIGHT;
					int UI_WIDTH = (int)(ClientSize.Width / density);
					RenderTarget.Resize(new Size2(UI_WIDTH, UI_HEIGHT));

					RenderTarget.BeginDraw();
					RenderTarget.FillRectangle(new RawRectangleF(0, 0, UI_WIDTH, UI_HEIGHT), new SolidColorBrush(RenderTarget, ConvertColor(BackColor)));//Draw BackColor
																																						 //Draw Background
					int cloudComp = -BG_WIDTH;
					while (cloudComp < UI_WIDTH)
					{
						cloudComp += BG_WIDTH;
						RenderTarget.DrawBitmap(CloudBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 20 % BG_WIDTH + cloudComp, UI_HEIGHT - 424, BG_WIDTH, BG_WIDTH * CloudBitmap.Size.Height / CloudBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);
					}

					int forestComp = -FOREST_WIDTH;
					while (forestComp < UI_WIDTH)
					{
						forestComp += FOREST_WIDTH;
						RenderTarget.DrawBitmap(ForestBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 10 % FOREST_WIDTH + forestComp, UI_HEIGHT - 424, FOREST_WIDTH, FOREST_WIDTH * ForestBitmap.Size.Height / ForestBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);
					}

					int bgComp = -BG_WIDTH;
					while (bgComp < UI_WIDTH)
					{
						bgComp += BG_WIDTH;
						RenderTarget.DrawBitmap(StoneBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - 424, BG_WIDTH, BG_WIDTH * StoneBitmap.Size.Height / StoneBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);
						RenderTarget.DrawBitmap(GroundBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - 424, BG_WIDTH, BG_WIDTH * GroundBitmap.Size.Height / GroundBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);

					}
					if (playState == 0)
						RenderTarget.DrawBitmap(TitleBitmap, RelRectangleF((UI_WIDTH - TitleBitmap.PixelSize.Width) / 2, 128, TitleBitmap.Size.Width, TitleBitmap.Size.Height), 1, BitmapInterpolationMode.NearestNeighbor);
					//Draw Paimon
					if (playState == 0)
					{
						SharpDX.Direct2D1.Bitmap PCurrent = PNormal;
						if (pState == 0)
							PCurrent = PNormal;
						else if (pState == 1)
							PCurrent = PFly;
						RenderTarget.DrawBitmap(PCurrent, RelRectangleF((UI_WIDTH - PCurrent.PixelSize.Width) / 2,
						(float)((UI_HEIGHT - PCurrent.PixelSize.Height) / 2 + RestAni.GetValue()), PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
					}
					RenderTarget.EndDraw();
					#endregion
				}
			}
			catch {; }
			GC.Collect();
		}
		RawRectangleF RelRectangleF(float x,float y,float w,float h)
		{
			return new RawRectangleF(x, y, w + x, h + y);
		}

		private void GameUI_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right) EnableGPU = !EnableGPU;
		}

		WindowRenderTarget RenderTarget;
		RawColor4 ConvertColor(Color source)
		{
			return new RawColor4((float)source.R / 255, (float)source.G / 255, (float)source.B / 255, (float)source.A / 255);
		}
		void InitDevices()
		{
			Factory factory = new Factory(FactoryType.SingleThreaded);
			RenderTargetProperties properties = new RenderTargetProperties()
			{
				PixelFormat =new PixelFormat(),
				Usage = RenderTargetUsage.None,
				Type = RenderTargetType.Default
			};
			HwndRenderTargetProperties hwProperties = new HwndRenderTargetProperties()
			{
				Hwnd = GameUI.Handle,
				PixelSize = new Size2(GameUI.Width, GameUI.Height),
				PresentOptions = PresentOptions.None
			};
			RenderTarget = new WindowRenderTarget(factory, properties, hwProperties)
			{
				AntialiasMode = AntialiasMode.PerPrimitive
			};
			
		}
		SharpDX.Direct2D1.Bitmap ConvertBitmap(System.Drawing.Bitmap source)
		{
			System.Drawing.Imaging.BitmapData bitmapData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
			System.Drawing.Imaging.ImageLockMode.ReadOnly,
			source.PixelFormat);
			byte[] memory = new byte[bitmapData.Stride*source.Height];
			IntPtr scan = bitmapData.Scan0;
			//MessageBox.Show("ot");
			System.Runtime.InteropServices.Marshal.Copy(scan, memory, 0, bitmapData.Stride*source.Height);
			source.UnlockBits(bitmapData);
			BitmapProperties bp = new BitmapProperties()
			{
				PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
				DpiX = source.HorizontalResolution,
				DpiY = source.VerticalResolution
			};
			SharpDX.Direct2D1.Bitmap dBitmap = new SharpDX.Direct2D1.Bitmap(RenderTarget, new Size2(source.Width, source.Height),bp);
			dBitmap.CopyFromMemory(memory, bitmapData.Stride);
			return dBitmap;
		}
		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			System.Environment.Exit(0);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			LoadImage();
		}
	}
	public class BufferedPanel:Panel
	{
		public BufferedPanel()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}
	}
}
