using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

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
				ReRest();
			};
			RestAni.Restart();
			pTimer.Start();
			RestChecker.Tick += (object o, EventArgs a) =>
			{
				if (RestAni.IsAnimating == false) ReRest();
			};
			InitDevices();
		}
		void ReRest()
		{
			if (RestAni.Description == "up")
			{
				RestAni = new THAnimations.EasyAni() { Description = "down", EasingFunction = THAnimations.EasingFunction.PowerInOut, Pow = 2, Duration = 0.5 };
				RestAni.From = 10; RestAni.Restart(); RestAni.To = -10;
			}
			else if (RestAni.Description == "down")
			{
				RestAni = new THAnimations.EasyAni() { Description = "up", EasingFunction = THAnimations.EasingFunction.PowerInOut, Pow = 2, Duration = 0.5 };
				RestAni.From = -10; RestAni.Restart(); RestAni.To = 10;
			}
			GC.Collect();
		}
		SharpDX.Direct2D1.Bitmap CloudBitmap, StoneBitmap, GroundBitmap, ForestBitmap,PNormal,PFly,TitleBitmap,PDead,TubeUpper,TubeLower;
		void LoadImage()
		{
			CloudBitmap = ConvertBitmap(Properties.Resources.cloud);
			StoneBitmap = ConvertBitmap(Properties.Resources.stone);
			GroundBitmap = ConvertBitmap(Properties.Resources.ground);
			ForestBitmap = ConvertBitmap(Properties.Resources.forest);
			PNormal = ConvertBitmap(Properties.Resources.pNormal);
			PFly = ConvertBitmap(Properties.Resources.pFly);
			TitleBitmap = ConvertBitmap(Properties.Resources.title);
			PDead = ConvertBitmap(Properties.Resources.pDead);
			TubeUpper = ConvertBitmap(Properties.Resources.tube_upper);
			TubeLower = ConvertBitmap(Properties.Resources.tube_lower);
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
		const int GROUND_LOCATION = 347;

		int playState = 0;
		int pState = 0;
		long BeginTime = 0;
		System.Timers.Timer pTimer = new System.Timers.Timer() { Interval = 333,Enabled = true };
		bool EnableGPU = true;
		double PLocation = 50,PRotation = 0;
		public void Render()
		{
			try
			{
				{
					#region Direct2D
					GameUI.BackgroundImage = null;
					float density = (float)ClientSize.Height / UI_HEIGHT;
					int din = (int)Math.Ceiling(density);
					int UI_WIDTH = (int)(ClientSize.Width / density);
					RenderTarget.DotsPerInch = new Size2F(96*din,96*din);
					RenderTarget.Resize(new Size2(UI_WIDTH*din, UI_HEIGHT*din));
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
					//Draw Obstacle
					foreach (var tubes in Tubes)
					{
						//upper
						RenderTarget.DrawBitmap(TubeUpper, RelRectangleF((float)(tubes.animationX.GetValue() - TubeUpper.PixelSize.Width) / 2 + UI_WIDTH / 2,
						-TubeUpper.PixelSize.Height + (float)((float)UI_HEIGHT * GROUND_LOCATION / 424 * (tubes.y) / 100) - 100,
						TubeUpper.PixelSize.Width, TubeUpper.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
						//lower
						RenderTarget.DrawBitmap(TubeLower, RelRectangleF((float)(tubes.animationX.GetValue() - TubeLower.PixelSize.Width) / 2 + UI_WIDTH / 2,
						(float)((float)UI_HEIGHT * GROUND_LOCATION / 424 * (tubes.y ) / 100) + 100,
						TubeLower.PixelSize.Width, TubeLower.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
					}
					//Draw Stone
					int bgComp = -BG_WIDTH;
					while (bgComp < UI_WIDTH)
					{
						bgComp += BG_WIDTH;
						RenderTarget.DrawBitmap(StoneBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - 424, BG_WIDTH, BG_WIDTH * StoneBitmap.Size.Height / StoneBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);

					}
					//Draw Paimon
					SharpDX.Direct2D1.Bitmap PCurrent = new SharpDX.Direct2D1.Bitmap(IntPtr.Zero);
					if (pState == 0)
						PCurrent = PNormal;
					else if (pState == 1)
						PCurrent = PFly;
					if (playState == 0)
					{
						RenderTarget.DrawBitmap(PCurrent, RelRectangleF((UI_WIDTH - PCurrent.PixelSize.Width) / 2,
						(float)((UI_HEIGHT - PCurrent.PixelSize.Height) / 2 * (GROUND_LOCATION / 424f) + RestAni.GetValue()), PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
					}
					else
					{
						RawMatrix3x2 oldMatrix = RenderTarget.Transform;
						RenderTarget.Transform = ConvertMatrix(Matrix3x2.CreateRotation((float)(PRotation / 180 * Math.PI), new Vector2(
						UI_WIDTH / 2, (float)(UI_HEIGHT * (PLocation / 100) * (GROUND_LOCATION / 424f)))));
						if (playState == 1)
							RenderTarget.DrawBitmap(PCurrent, RelRectangleF((UI_WIDTH - PCurrent.PixelSize.Width) / 2,
							(float)((UI_HEIGHT) * (PLocation / 100) * (GROUND_LOCATION / 424f)) - PCurrent.PixelSize.Height / 2, PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
						else
						{
							PCurrent = PDead;
							RenderTarget.Transform = ConvertMatrix(Matrix3x2.CreateRotation((float)(PRotation / 180 * Math.PI), new Vector2(
							UI_WIDTH / 2, (float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / 424f)))));
							RenderTarget.DrawBitmap(PCurrent, RelRectangleF((UI_WIDTH - PCurrent.PixelSize.Width) / 2,
						  (float)((UI_HEIGHT) * (GameAni.GetValue() / 100) * (GROUND_LOCATION / 424f)) - PCurrent.PixelSize.Height / 2, PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
						}
						RenderTarget.Transform = oldMatrix;
					}

					//Draw Ground
					bgComp = -BG_WIDTH;
					while (bgComp < UI_WIDTH)
					{
						bgComp += BG_WIDTH;
						RenderTarget.DrawBitmap(GroundBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - 424, BG_WIDTH, BG_WIDTH * GroundBitmap.Size.Height / GroundBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);

					}

					if (playState == 0)
						RenderTarget.DrawBitmap(TitleBitmap, RelRectangleF((UI_WIDTH - TitleBitmap.PixelSize.Width) / 2, 128, TitleBitmap.Size.Width, TitleBitmap.Size.Height), 1, BitmapInterpolationMode.NearestNeighbor);
					
					
					RenderTarget.EndDraw();
					#endregion
					//Logics
					if (playState == 1)
					{
						if (LastTime != (UIWatch.ElapsedMilliseconds - BeginTime) / 2000)
							AddObstacle();
						LastTime = (int)(UIWatch.ElapsedMilliseconds - BeginTime) / 2000;
						if (PLocation < 0)
						{
							GameAni.Stop();
							PLocation = 0;
							AniDown();

						}
						//Hit ground
						if(PLocation>=100)
							GameOver();
						//Hit tube
						foreach(var tube in Tubes)
						{
							if (tube.animationX.GetValue() <= 104 && tube.animationX.GetValue() >= -104 && (tube.y - 10 > PLocation || tube.y + 10 < PLocation))
							{
								GameOver();AniDown();
								
								break;
							}
						}
					}
					if(playState==2)
					{
						if(GameAni.GetValue()>=100)RotationAni.Stop();
					}
				}
			}
			catch {; }
		}
		int LastTime = 0;
		[StructLayout(LayoutKind.Sequential)]
		struct Slime
		{
			public double x;
			public double y;
			public THAnimations.EasyAni animationX;
			public THAnimations.EasyAni animationY;
		}
		[StructLayout(LayoutKind.Sequential)]
		struct Tube
		{
			public double x;
			public double y;
			public THAnimations.EasyAni animationX;
		}
		[StructLayout(LayoutKind.Sequential)]
		struct Yuanshi
		{
			public double x;
			public double y;
			public THAnimations.EasyAni animationX;
		}
		List<Slime> Slimes = new List<Slime>();
		List<Tube> Tubes = new List<Tube>();
		List<Yuanshi> Yuanshis = new List<Yuanshi>();
		void AddObstacle()
		{
			THAnimations.EasyAni tubeAnimation = new THAnimations.EasyAni();
			tubeAnimation.From = UI_HEIGHT*2;tubeAnimation.To = -UI_HEIGHT*4;tubeAnimation.Pow = 1;tubeAnimation.EasingFunction = THAnimations.EasingFunction.Linear;tubeAnimation.Duration = 12;
			Tube tube = new Tube() { x = UI_HEIGHT, y = new Random().NextDouble() * 60 + 20, animationX = tubeAnimation };
			tubeAnimation.Animated = (object o, EventArgs a) => { Tubes.Remove(tube); };
			Tubes.Add(tube);
			//MessageBox.Show(tube.y.ToString());
			tubeAnimation.Restart();
		}
		void GameOver()
		{
			foreach (var tube in Tubes)
			{
				tube.animationX.Pause();
			}
			foreach (var slime in Slimes)
			{
				slime.animationX.Pause();
			}
			foreach (var yuanshi in Yuanshis)
			{
				yuanshi.animationX.Pause();
			}
			GameAni.Stop();
			UIWatch.Stop();
			pTimer.Stop();
			playState = 2;
		}
		RawRectangleF RelRectangleF(float x,float y,float w,float h)
		{
			return new RawRectangleF(x, y, w + x, h + y);
		}
		private RawMatrix3x2 ConvertMatrix(Matrix3x2 src)
		{
			return new RawMatrix3x2(src.M11, src.M12, src.M21, src.M22, src.M31, src.M32);
		}
		private void GameUI_MouseClick(object sender, MouseEventArgs e)
		{
			Press(sender, e);
		}
		THAnimations.EasyAni GameAni;
		THAnimations.EasyAni RotationAni;
		private void Press(object sender, EventArgs e)
		{
			if(playState==0)
			{
				Tubes.Clear();
				Slimes.Clear();
				Yuanshis.Clear();
				if (GameAni != null)
				{
					GameAni.To = 50;
					GameAni.From = 50;
				}
				BeginTime = UIWatch.ElapsedMilliseconds;
				LastTime = 0;
				pTimer.Interval = 200;
				RestChecker.Stop();
				RestAni.Stop();
				AddObstacle();
				playState = 1;
				Press(sender, e);
			}
			else if(playState ==1)
			{
				GameAni?.Stop();
				GameAni = new THAnimations.EasyAni();GameAni.Pow = 2;
				GameAni.Progress = 0;
				GameAni.From = PLocation;GameAni.To = PLocation - 10;GameAni.Description = "up"; GameAni.EasingFunction = THAnimations.EasingFunction.PowerOut;
				GameAni.Duration = 0.2;
				GameAni.Animated = (object o, EventArgs a) =>
				{
					if (GameAni.Description == "up")
					{
						AniDown();
					}
				};
				GameAni.Animating += (object o, EventArgs a) =>
				{
					PLocation = GameAni.GetValue();
				};
				GameAni.Restart();
				RotationAni = new THAnimations.EasyAni() { From = 0, To = 22.5, Duration = 1, EasingFunction = THAnimations.EasingFunction.PowerIn, Pow = 2 };
				RotationAni.Animating = (object o, EventArgs a) => { PRotation = RotationAni.GetValue(); };
				RotationAni.Restart();
				GC.Collect();
			}
			else if(playState==2)
			{
				for (int i = Tubes.Count - 1; i >= 0; i--)
				{
					Tubes[i].animationX.Stop();
				}
				for (int i = Slimes.Count - 1; i >= 0; i--)
				{
					Slimes[i].animationX.Stop();
				}
				for (int i = Yuanshis.Count - 1; i >= 0; i--)
				{
					Yuanshis[i].animationX.Stop();
				}
				PLocation = 50;
				UIWatch.Restart();
				pTimer.Start();
				pTimer.Interval = 333;
				RestChecker.Start();
				ReRest();
				PRotation = 0;
				playState = 0;
				GC.Collect();
			}
		}
		private void AniUp()
		{

		}
		private void AniDown()
		{
			GameAni = new THAnimations.EasyAni();
			GameAni.From = PLocation;
			GameAni.To = PLocation + 100;
			GameAni.Description = "down";
			GameAni.EasingFunction = THAnimations.EasingFunction.PowerIn;
			GameAni.Pow = 2;
			GameAni.Duration = 0.8;
			GameAni.Animating += (object o, EventArgs a) =>
			{
				if(playState==1)
				PLocation = GameAni.GetValue();
				if (GameAni.GetValue() >= 100) { GameAni.To = 100; GameAni.Stop();
					RotationAni.To = RotationAni.GetValue(); RotationAni.Stop();
				}
			};
			GameAni.Restart();
			GC.Collect();
		}
		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			Press(sender, e);
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
