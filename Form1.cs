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
		string ResourcePath;
		Stopwatch UIWatch = new Stopwatch(), EndWatch = new Stopwatch();
		Control GameUI;
		IntPtr GLContext;
		[DllImport("user32.dll")]
		public static extern IntPtr GetDC(IntPtr hwnd);
		long GameTime;
		bool isLoaded = false;
		LoadControl loadControl = new LoadControl();
		public Form1()
		{
			CheckForIllegalCrossThreadCalls = false;
			InitializeComponent();
			ResourcePath = Application.StartupPath + "\\Resources\\";
			t0.Tick += (object o, EventArgs a) => Render();
			t0.Start();
		}
		void InitGame()
		{
			GameUI = this;
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
			float DPI = this.CreateGraphics().DpiX / 96f;
			pTimer.Elapsed += PTimer_Tick;
			RestAni.Animated = (object o, EventArgs a) =>
			{
				ReRest();
			};
			RestAni.Restart();
			pTimer.Start();
			RestChecker.Tick += (object o, EventArgs a) =>
			{
				if (RestAni.IsAnimating == false) ReRest();
			};
			InitDevices();
			LoadImage();
			UIWatch.Start();
			//LoadSounds();
			BGMPlayer.MediaEnded += (object o, EventArgs a) => { PlayBGM(); };
			t1.Tick+=(object o,EventArgs a)=> DispatchRender(); 
			RCThread = new System.Threading.Thread(new System.Threading.ThreadStart(CompatibleLoop));
			UseCompatibleMode = false;
		}
		Timer t0 = new Timer() { Interval = 1 },t1 = new Timer() { Interval = 1 };
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
		SharpDX.Direct2D1.Bitmap CloudBitmap, StoneBitmap, GroundBitmap, ForestBitmap, PNormal, PFly, TitleBitmap, PDead, TubeUpper, TubeLower, Slime0, Slime1, Slime2, YSBitmap,
		One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Zero, FSBitmap, Sound, DisableSound, SDX, FPS, GDI;
		System.Drawing.Bitmap GZero, GOne, GTwo, GThree, GFour, GFive, GSix, GSeven, GEight, GNine;
		void LoadImage()
		{
			GameUI.CreateGraphics().DrawString("Loading resources...", new Font("", 12), new SolidBrush(Color.Black), new Point());
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
			Slime0 = ConvertBitmap(Properties.Resources.slime0);
			Slime1 = ConvertBitmap(Properties.Resources.slime1);
			Slime2 = ConvertBitmap(Properties.Resources.slime2);
			YSBitmap = ConvertBitmap(Properties.Resources.yuanshi_smaller);
			List<System.Drawing.Bitmap> numberList = new System.Collections.Generic.List<System.Drawing.Bitmap>();
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					System.Drawing.Bitmap numBitmap = new System.Drawing.Bitmap(25, 40);
					Graphics numGraphics = Graphics.FromImage(numBitmap);
					numGraphics.DrawImage(Properties.Resources.number, -Properties.Resources.number.Width / 5 * j, -45 * i);
					numGraphics.Dispose();
					numberList.Add(numBitmap);
				}
			}
			Three = ConvertBitmap(numberList[0]);
			Two = ConvertBitmap(numberList[1]);
			Six = ConvertBitmap(numberList[2]);
			Four = ConvertBitmap(numberList[3]);
			Zero = ConvertBitmap(numberList[4]);
			Nine = ConvertBitmap(numberList[5]);
			Five = ConvertBitmap(numberList[6]);
			Eight = ConvertBitmap(numberList[7]);
			One = ConvertBitmap(numberList[8]);
			Seven = ConvertBitmap(numberList[9]);

			GThree = numberList[0];
			GTwo = numberList[1];
			GSix = numberList[2];
			GFour = numberList[3];
			GZero = numberList[4];
			GNine = numberList[5];
			GFive = numberList[6];
			GEight = numberList[7];
			GOne = numberList[8];
			GSeven = numberList[9];
			FSBitmap = ConvertBitmap(Properties.Resources.Fullscreen);
			Sound = ConvertBitmap(Properties.Resources.Sound);
			DisableSound = ConvertBitmap(Properties.Resources.DisableSound);
			SDX = ConvertBitmap(Properties.Resources.SDX);
			FPS = ConvertBitmap(Properties.Resources.FPS);
			GDI = ConvertBitmap(Properties.Resources.GDI);
		}
		bool isPlaySound = true;
		System.Windows.Media.MediaPlayer BGMPlayer = new System.Windows.Media.MediaPlayer() { Volume = 1 };
		System.Windows.Media.MediaPlayer PressPlayer = new System.Windows.Media.MediaPlayer() { Volume = 1 };
		System.Windows.Media.MediaPlayer PassPlayer = new System.Windows.Media.MediaPlayer() { Volume = 1 };
		System.Windows.Media.MediaPlayer HitPlayer = new System.Windows.Media.MediaPlayer() { Volume = 1 };

		void PlayBGM()
		{
			BGMPlayer.Open(new Uri(ResourcePath + "bgm.mp3"));
			BGMPlayer.Play();
		}
		void PlayPress()
		{
			PressPlayer.Open(new Uri(ResourcePath + "press.mp3"));
			PressPlayer.Play();
		}
		void PlayHit()
		{
			HitPlayer.Open(new Uri(ResourcePath + "hit.mp3"));
			HitPlayer.Play();
		}
		void PlayPass()
		{
			PassPlayer.Open(new Uri(ResourcePath + "pass.mp3"));
			PassPlayer.Play();
		}
		THAnimations.EasyAni RestAni = new THAnimations.EasyAni() { Description = "up", From = -10, To = 10, EasingFunction = THAnimations.EasingFunction.PowerInOut, Pow = 2, Duration = 0.5 };
		System.Windows.Forms.Timer RestChecker = new System.Windows.Forms.Timer() { Interval = 1, Enabled = true };
		private void PTimer_Tick(object sender, EventArgs e)
		{
			pState = pState == 0 ? pState = 1 : pState = 0;
		}

		public const int UI_HEIGHT = 600;
		public const int MOVE_UNIT = 720;
		public int UI_WIDTH = 1024;
		public const int BG_WIDTH = 2048;
		public const int FOREST_WIDTH = 1604;
		public const int GROUND_LOCATION = 305;
		public const int TOP_0 = 30;
		public const int MAP_HEIGHT = 424;

		int playState = -1;
		int pState = 0;
		long BeginTime = 0;
		System.Timers.Timer pTimer = new System.Timers.Timer() { Interval = 333, Enabled = true };
		int Score = 0;

		double PLocation = 50, PRotation = 0;

		Point MouseAbsolute = new Point();
		Point ScreenAbsolute = new Point();
		int EnterPosition = 0;
		int TouchIndex = 0;
		private void GameUI_MouseMove(object sender, MouseEventArgs e)
		{
			MouseAbsolute = e.Location;
			MouseRelative = new Point(Convert.ToInt32(MouseAbsolute.X / (float)ClientSize.Width * UI_WIDTH), Convert.ToInt32(MouseAbsolute.Y / (float)ClientSize.Height * UI_HEIGHT));
			ScreenRelative = new Point(Convert.ToInt32(MousePosition.X / (float)ClientSize.Width * UI_WIDTH), Convert.ToInt32(MousePosition.Y / (float)ClientSize.Height * UI_HEIGHT));
			IsFSMouseOver = false;
			if (MouseRelative.X >= UI_WIDTH - 54 && MouseRelative.X < UI_WIDTH - 6 && MouseRelative.Y >= 6 && MouseRelative.Y < 54)
				IsFSMouseOver = true;
			else IsFSMouseOver = false;
			if (e.Button == MouseButtons.Left && CanSetTouch && playState != 2 &&ScreenAbsolute!=MousePosition)
			{
				int tempIndex = (EnterPosition - ScreenRelative.Y) / 30;
				if (tempIndex > TouchIndex)
				{
					Press(sender, e);
					TouchIndex = tempIndex;
				}
			}
			ScreenAbsolute = MousePosition;
		}
		bool UseCompatibleMode = false;
		protected override void OnPaint(PaintEventArgs e)
		{
			if (playState == -1) return;
			if (RCThread.IsAlive) RCThread.Abort();
			if (RCThread.IsAlive) RCThread.Join();
			RenderCompatible(e.Graphics);
		}
		int TmpFps = 0;
		int Fps = 0;
		long GameSeconds = 0;
		bool ShowFPS = false;
		public void Render()
		{
			if (this.WindowState != FormWindowState.Maximized && isFullScreen) FullScreen();
			if (playState == -1) return;
			CanSetTouch = true;
			long tempGameSeconds = (UIWatch.ElapsedMilliseconds + EndWatch.ElapsedMilliseconds) / 1000;
			if (tempGameSeconds != GameSeconds)
			{
				GameSeconds = tempGameSeconds;
				Fps = TmpFps;
				TmpFps = 0;
			}
			#region Logics
			if (playState == 0 && RotationAni != null && RotationAni.IsAnimating) RotationAni.Stop();
			if (playState == 1)
			{
				if (LastTime != (UIWatch.ElapsedMilliseconds - BeginTime) / 1950)
					AddObstacle();
				LastTime = (int)(UIWatch.ElapsedMilliseconds - BeginTime) / 1950;
				if (PLocation < 0)
				{
					GameAni.Stop();
					PLocation = 0;
					AniDown();

				}
				//Hit ground
				if (PLocation >= 100)
					GameOver();
				//Hit tube
				for (int i = Tubes.Count - 1; i >= 0; i--)
				{
					Tube tube = Tubes[i];
					if (tube.animationX.GetValue() <= 104 && tube.animationX.GetValue() >= -104 && (tube.y - 10 > PLocation || tube.y + 10 < PLocation))
					{
						GameOver(); AniDown();
						break;
					}
					//pass
					if (tube.isPass == false && tube.animationX.GetValue() < 0)
					{
						tube.isPass = true; Score++; PlayPass();
					}
				}
				//Hit Slime
				{
					foreach (var slime in Slimes)
					{
						if (slime.animationX.GetValue() <= 120 && slime.animationX.GetValue() >= -120 && PLocation > slime.y - 10 && PLocation < slime.y + 10)
						{
							GameOver(); AniDown();
							break;
						}
					}
				}
				//Hit Yuanshi
				{
					foreach (var yuanshi in Yuanshis)
					{
						if (yuanshi.animationX.GetValue() <= 96 && yuanshi.animationX.GetValue() >= -96 && PLocation > yuanshi.y - 10 && PLocation < yuanshi.y + 10)
						{
							GetYuanshi(yuanshi); break;
						}
					}
				}
			}
			//Control Slime
			foreach (var slime in Slimes)
			{
				if (!slime.animationY.IsAnimating && playState != 0)
					RegestryAnimationY(slime);
			}
			if (playState == 2)
			{
				if (GameAni.GetValue() >= 100) RotationAni.Stop();
			}
			#endregion
			if (!isLoaded) { UIWatch.Restart(); PlayBGM(); isLoaded = true; }
			//DispatchRender();
		}
		public void DispatchRender()
		{
			if (this.WindowState != FormWindowState.Minimized)
			{
				try
				{
					if (UseCompatibleMode)
					{
						if (!RCThread.IsAlive)
							RCThread = new System.Threading.Thread(new System.Threading.ThreadStart(CompatibleLoop)); RCThread.Start(); return;
					}
					if (RCThread.IsAlive) RCThread.Abort();
					RenderSDX();
				}
				catch
				{
					try
					{
						UseCompatibleMode = true; return;
					}
					catch (Exception e)
					{
						GameUI.CreateGraphics().DrawString(e.Message, new Font("", 12), new SolidBrush(Color.Black), new Point());
					}
				}
			}
			else
			{
				System.Threading.Thread.Sleep(1);
				if (RCThread.IsAlive) RCThread.Abort();
			}
		}
		SharpGL.OpenGL Gl = new SharpGL.OpenGL();
		void CompatibleLoop()
		{
			while (true) RenderCompatible(this.CreateGraphics());
		}
		public void RenderSDX()
		{
			/*
				if (!isinited) GLInit();
				SharpGL.Win32.wglMakeCurrent(WindowDC, GLContext);
				Gl.Enable(SharpGL.OpenGL.GL_SMOOTH);
				Gl.ClearColor(97 / 255f, 224 / 255f, 1, .5f);
				Gl.Clear(SharpGL.OpenGL.GL_COLOR_BUFFER_BIT|SharpGL.OpenGL.GL_DEPTH_BUFFER_BIT);
				Gl.Ortho2D(0, 0, ClientSize.Width, ClientSize.Height);
				Gl.MatrixMode(SharpGL.Enumerations.MatrixMode.Projection);
				Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
				Gl.LoadIdentity();

				float h = (float)(2 * Math.Tan(22.5) * 0.1);
				//Gl.Perspective(45, ClientSize.Height / h, 0.1, 1000);
				//Gl.Scale(1f/ClientSize.Width, -1f/ClientSize.Height,1);
				//Gl.Translate(-.5, -.5,0);

				float density = (float)ClientSize.Height / UI_HEIGHT;
				float din = (float)Math.Ceiling(density * 2) / 2;
				UI_WIDTH = (int)(ClientSize.Width / density);

				//Draw Background
				int cloudComp = -BG_WIDTH;
				while (cloudComp < UI_WIDTH)
				{
					cloudComp += BG_WIDTH;
					//bGraphics.DrawImage(Properties.Resources.cloud, -UIWatch.ElapsedMilliseconds / 20 % BG_WIDTH + cloudComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * CloudBitmap.Size.Height / CloudBitmap.Size.Width);

					GLLoadBitmap(Properties.Resources.tube_lower);
					Gl.Begin(SharpGL.Enumerations.BeginMode.Quads);
					Gl.Color(255, 0, 255,255);
					Gl.Vertex(-1, -1);
					Gl.Vertex(1, -1);
					Gl.Vertex(1, 1);
					Gl.Vertex(-1, 1);
					Gl.End();
				}

				SharpGL.Win32.SwapBuffers(WindowDC);
				Gl.Flush();
				return;*/
			if (playState == -1) return;
			#region Direct2D
			GameUI.BackgroundImage = null;
			float density = (float)ClientSize.Height / UI_HEIGHT;
			float din = (float)Math.Ceiling(density * 2) / 2;
			UI_WIDTH = (int)(ClientSize.Width / density);
			RenderTarget.DotsPerInch = new Size2F(96 * din, 96 * din);
			RenderTarget.Resize(new Size2(Convert.ToInt32(UI_WIDTH * din), Convert.ToInt32(UI_HEIGHT * din)));
			RenderTarget.BeginDraw();
			RenderTarget.Transform = new RawMatrix3x2(1, 0, 0, 1, 0, 0);
			RenderTarget.FillRectangle(new RawRectangleF(0, 0, UI_WIDTH, UI_HEIGHT), new SolidColorBrush(RenderTarget, ConvertColor(Color.FromArgb(97, 224, 255))));//Draw BackColor

			//Draw Background
			int cloudComp = -BG_WIDTH;
			while (cloudComp < UI_WIDTH)
			{
				cloudComp += BG_WIDTH;
				RenderTarget.DrawBitmap(CloudBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 20 % BG_WIDTH + cloudComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * CloudBitmap.Size.Height / CloudBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);
			}

			int forestComp = -FOREST_WIDTH;
			while (forestComp < UI_WIDTH)
			{
				forestComp += FOREST_WIDTH;
				RenderTarget.DrawBitmap(ForestBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 10 % FOREST_WIDTH + forestComp, UI_HEIGHT - MAP_HEIGHT, FOREST_WIDTH, FOREST_WIDTH * ForestBitmap.Size.Height / ForestBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);
			}
			//Draw Obstacle
			foreach (var tubes in Tubes)
			{
				//upper
				RenderTarget.DrawBitmap(TubeUpper, RelRectangleF((float)(tubes.animationX.GetValue() - TubeUpper.PixelSize.Width) / 2 + UI_WIDTH / 2,
				-TubeUpper.PixelSize.Height + (float)((float)UI_HEIGHT * GROUND_LOCATION / MAP_HEIGHT * (tubes.y) / 100) - 75 + TOP_0,
				TubeUpper.PixelSize.Width, TubeUpper.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
				//lower
				RenderTarget.DrawBitmap(TubeLower, RelRectangleF((float)(tubes.animationX.GetValue() - TubeLower.PixelSize.Width) / 2 + UI_WIDTH / 2,
				(float)((float)UI_HEIGHT * GROUND_LOCATION / MAP_HEIGHT * (tubes.y) / 100) + 75 + TOP_0,
				TubeLower.PixelSize.Width, TubeLower.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
			}
			//Draw Yuanshi
			foreach (var yuanshi in Yuanshis)
			{
				RenderTarget.DrawBitmap(YSBitmap, RelRectangleF(((float)yuanshi.animationX.GetValue() - YSBitmap.PixelSize.Width + UI_WIDTH) / 2,
				-YSBitmap.PixelSize.Height / 2 + (float)UI_HEIGHT * GROUND_LOCATION / MAP_HEIGHT * (float)yuanshi.y / 100 + TOP_0, YSBitmap.PixelSize.Width, YSBitmap.PixelSize.Height),
				1, BitmapInterpolationMode.NearestNeighbor);
			}
			//Draw Slime
			foreach (var slime in Slimes)
			{
				SharpDX.Direct2D1.Bitmap SCurrent = Slime0;
				switch ((UIWatch.ElapsedMilliseconds + EndWatch.ElapsedMilliseconds - slime.enterTime) / 200 % 4)
				{
					case 0: case 2: SCurrent = Slime0; break;
					case 1: SCurrent = Slime1; break;
					case 3: SCurrent = Slime2; break;
				}
				RenderTarget.DrawBitmap(SCurrent, RelRectangleF((float)(slime.animationX.GetValue() - SCurrent.PixelSize.Width) / 2 + UI_WIDTH / 2,
				(float)((float)UI_HEIGHT * (GROUND_LOCATION + SCurrent.PixelSize.Height / 4) / MAP_HEIGHT * slime.animationY.GetValue() / 100),
				SCurrent.PixelSize.Width, SCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
			}
			//Draw Stone
			int bgComp = -BG_WIDTH;
			while (bgComp < UI_WIDTH)
			{
				bgComp += BG_WIDTH;
				RenderTarget.DrawBitmap(StoneBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * StoneBitmap.Size.Height / StoneBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);

			}
			//Draw Paimon
			SharpDX.Direct2D1.Bitmap PCurrent = PNormal;
			if (pState == 0)
				PCurrent = PNormal;
			else if (pState == 1)
				PCurrent = PFly;
			if (playState == 0)
			{
				RenderTarget.DrawBitmap(PCurrent, RelRectangleF((UI_WIDTH - PCurrent.PixelSize.Width) / 2,
				(float)((UI_HEIGHT - PCurrent.PixelSize.Height) / 2 * (GROUND_LOCATION / (float)MAP_HEIGHT) + RestAni.GetValue() + TOP_0)
				, PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
			}
			else
			{
				RawMatrix3x2 oldMatrix = RenderTarget.Transform;
				RenderTarget.Transform = ConvertMatrix(Matrix3x2.CreateRotation((float)(RotationAni.GetValue() / 180 * Math.PI), new Vector2(
				UI_WIDTH / 2, (float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) + TOP_0)));
				if (playState == 1)
					RenderTarget.DrawBitmap(PCurrent, RelRectangleF((UI_WIDTH - PCurrent.PixelSize.Width) / 2,
					(float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) - PCurrent.PixelSize.Height / 2 + TOP_0, PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
				else
				{
					PCurrent = PDead;
					Matrix3x2 currentMatrix;
					if (RotationAni.IsAnimating)
						currentMatrix = Matrix3x2.CreateRotation((float)(RotationAni.GetValue() / 180 * Math.PI), new Vector2(
					UI_WIDTH / 2, (float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) + TOP_0));
					else
						currentMatrix = Matrix3x2.CreateRotation((float)(PRotation / 180 * Math.PI), new Vector2(
					UI_WIDTH / 2, (float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) + TOP_0));
					RenderTarget.Transform = ConvertMatrix(currentMatrix);
					RenderTarget.Transform = new RawMatrix3x2(RenderTarget.Transform.M11, RenderTarget.Transform.M12, RenderTarget.Transform.M21, RenderTarget.Transform.M22, RenderTarget.Transform.M31, RenderTarget.Transform.M32);
					RenderTarget.DrawBitmap(PCurrent, RelRectangleF((UI_WIDTH - PCurrent.PixelSize.Width) / 2,
				  (float)((UI_HEIGHT) * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) - PCurrent.PixelSize.Height / 2 + TOP_0, PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
				}
				RenderTarget.Transform = new RawMatrix3x2(1, 0, 0, 1, 0, 0);
			}

			//Draw Ground
			bgComp = -BG_WIDTH;
			while (bgComp < UI_WIDTH)
			{
				bgComp += BG_WIDTH;
				RenderTarget.DrawBitmap(GroundBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * GroundBitmap.Size.Height / GroundBitmap.Size.Width), 1, BitmapInterpolationMode.NearestNeighbor);

			}
			//Draw Title
			if (playState == 0)
				RenderTarget.DrawBitmap(TitleBitmap, RelRectangleF((UI_WIDTH - TitleBitmap.PixelSize.Width) / 2, 96, TitleBitmap.PixelSize.Width, TitleBitmap.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);


			//Display Score
			int digits = 0;
			if (Score != 0)
				digits = (int)Math.Log10(Score);
			if (playState != 0)
			{
				for (int i = digits; i >= 0; i--)
				{
					SharpDX.Direct2D1.Bitmap numBitmap = Zero;
					switch (Score / (int)Math.Pow(10, i) % 10)
					{
						case 0: numBitmap = Zero; break;
						case 1: numBitmap = One; break;
						case 2: numBitmap = Two; break;
						case 3: numBitmap = Three; break;
						case 4: numBitmap = Four; break;
						case 5: numBitmap = Five; break;
						case 6: numBitmap = Six; break;
						case 7: numBitmap = Seven; break;
						case 8: numBitmap = Eight; break;
						case 9: numBitmap = Nine; break;
					}
					int numWidth = numBitmap.PixelSize.Width, numHeight = numBitmap.PixelSize.Height;
					int numBegin = digits * numWidth;
					RenderTarget.DrawBitmap(numBitmap, RelRectangleF((UI_WIDTH - numBegin) / 2 + (digits - i) * numWidth, 64, numWidth, numHeight), 1, BitmapInterpolationMode.NearestNeighbor);
				}
			}
			//Draw Buttons
			if (!IsFSMouseOver)
				RenderTarget.DrawBitmap(FSBitmap, RelRectangleF(UI_WIDTH - 48 - 6, 6 * GetEnterAni(), 48, 48), 1, BitmapInterpolationMode.NearestNeighbor);
			else
				RenderTarget.DrawBitmap(FSBitmap, RelRectangleF(UI_WIDTH - 48 - 6, 6 * GetEnterAni(), 48, 48), 0.5f, BitmapInterpolationMode.NearestNeighbor);
			if (MouseRelative.X >= UI_WIDTH - 54 - 54 && MouseRelative.X < UI_WIDTH - 6 - 54 && MouseRelative.Y >= 6 && MouseRelative.Y < 54)
			{
				if (isPlaySound)
					RenderTarget.DrawBitmap(Sound, RelRectangleF(UI_WIDTH - 48 - 54 - 6, 6 * GetEnterAni(), 48, 48), 0.5f, BitmapInterpolationMode.NearestNeighbor);
				else
					RenderTarget.DrawBitmap(DisableSound, RelRectangleF(UI_WIDTH - 48 - 54 - 6, 6 * GetEnterAni(), 48, 48), 0.5f, BitmapInterpolationMode.NearestNeighbor);
			}
			else
			{
				if (isPlaySound)
					RenderTarget.DrawBitmap(Sound, RelRectangleF(UI_WIDTH - 48 - 54 - 6, 6 * GetEnterAni(), 48, 48), 1, BitmapInterpolationMode.NearestNeighbor);
				else
					RenderTarget.DrawBitmap(DisableSound, RelRectangleF(UI_WIDTH - 48 - 54 - 6, 6 * GetEnterAni(), 48, 48), 1, BitmapInterpolationMode.NearestNeighbor);
			}
			if (ShowFPS)
			{
				RenderTarget.DrawBitmap(SDX, RelRectangleF(UI_WIDTH - SDX.PixelSize.Width - 4, UI_HEIGHT - SDX.PixelSize.Height - 4, SDX.PixelSize.Width, SDX.PixelSize.Height), 1,
				BitmapInterpolationMode.NearestNeighbor);
				RenderTarget.DrawBitmap(FPS, RelRectangleF(4, UI_HEIGHT - FPS.PixelSize.Height - 4, FPS.PixelSize.Width, FPS.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);

				int fDigits = 0;
				if (Fps != 0)
					fDigits = (int)Math.Log10(Fps);
				for (int i = fDigits; i >= 0; i--)
				{
					SharpDX.Direct2D1.Bitmap fNumBitmap = Zero;
					switch (Fps / (int)Math.Pow(10, i) % 10)
					{
						case 0: fNumBitmap = Zero; break;
						case 1: fNumBitmap = One; break;
						case 2: fNumBitmap = Two; break;
						case 3: fNumBitmap = Three; break;
						case 4: fNumBitmap = Four; break;
						case 5: fNumBitmap = Five; break;
						case 6: fNumBitmap = Six; break;
						case 7: fNumBitmap = Seven; break;
						case 8: fNumBitmap = Eight; break;
						case 9: fNumBitmap = Nine; break;
					}
					int numWidth = fNumBitmap.PixelSize.Width, numHeight = fNumBitmap.PixelSize.Height;
					int numBegin = digits * numWidth;
					RenderTarget.DrawBitmap(fNumBitmap, RelRectangleF(4 + FPS.PixelSize.Width + 2 + (fDigits - i) * fNumBitmap.PixelSize.Width, UI_HEIGHT - fNumBitmap.PixelSize.Height - 6, numWidth, numHeight), 1, BitmapInterpolationMode.NearestNeighbor);
				}
			}
			RenderTarget.EndDraw();
			TmpFps++;
			#endregion

		}
		float GetEnterAni()
		{
			float enterAni = 0;
			enterAni = UIWatch.ElapsedMilliseconds / 10f - 10;
			if (enterAni > 1) enterAni = 1;
			if (playState != 0)
			{
				enterAni = -(UIWatch.ElapsedMilliseconds - BeginTime) / 10f;
				if (enterAni < -100) enterAni = 100;
			}
			return enterAni;
		}

		private void Form1_ResizeBegin(object sender, EventArgs e)
		{
			if (playState != -1) t1.Start();
		}

		private void Form1_ResizeEnd(object sender, EventArgs e)
		{
			if (playState != -1) t1.Stop();
		}

		void GLLoadBitmap(System.Drawing.Bitmap source)
		{
			Gl.Bitmap(source.Width, source.Height, 0, 0, 0, 0, GetBitmapData(source));
		}
		byte[] GetBitmapData(System.Drawing.Bitmap source)
		{
			System.Drawing.Bitmap compressedBitmap = new System.Drawing.Bitmap(source.Width, source.Height);
			Graphics.FromImage(compressedBitmap).DrawImage(source, 0, 0);
			var bits = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			byte[] data = new byte[source.Width * source.Height * 4];
			Marshal.Copy(bits.Scan0, data, 0, data.Length);
			//compressedBitmap.UnlockBits(bits);
			compressedBitmap.Dispose();
			return data;
		}
		bool isinited = false;
		System.Drawing.Bitmap renderBitmap;
		System.Threading.Thread RCThread;
		[DllImport("gdi32.dll")]
		public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
		[DllImport("user32.dll")]
		public static extern bool ReleaseDC(IntPtr hWND, IntPtr hDC);
		public void RenderCompatible(Graphics g)
		{
			if (!isLoaded || playState == -1) return;
			float density = (float)ClientSize.Height / UI_HEIGHT;
			float din = (float)Math.Ceiling(density * 2) / 2;
			UI_WIDTH = (int)(ClientSize.Width / density);
			renderBitmap = new System.Drawing.Bitmap(Convert.ToInt32(UI_WIDTH * din), Convert.ToInt32(UI_HEIGHT * din));
			Graphics bGraphics = Graphics.FromImage(renderBitmap);
			bGraphics.Clear(Color.FromArgb(97, 224, 255));
			bGraphics.TranslateTransform(0, 1);
			bGraphics.ScaleTransform(din, din);
			bGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			//Draw Background
			int cloudComp = -BG_WIDTH;
			while (cloudComp < UI_WIDTH)
			{
				cloudComp += BG_WIDTH;
				bGraphics.DrawImage(Properties.Resources.cloud, -UIWatch.ElapsedMilliseconds / 20 % BG_WIDTH + cloudComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * CloudBitmap.Size.Height / CloudBitmap.Size.Width);
			}

			int forestComp = -FOREST_WIDTH;
			while (forestComp < UI_WIDTH)
			{
				forestComp += FOREST_WIDTH;
				bGraphics.DrawImage(Properties.Resources.forest, -UIWatch.ElapsedMilliseconds / 10 % FOREST_WIDTH + forestComp, UI_HEIGHT - MAP_HEIGHT, FOREST_WIDTH, FOREST_WIDTH * ForestBitmap.Size.Height / ForestBitmap.Size.Width);
			}
			//Draw Obstacle
			for (int i = 0; i < Tubes.Count; i++)
			{
				if (i >= Tubes.Count) break;
				var tubes = Tubes[i];
				//upper
				bGraphics.DrawImage(Properties.Resources.tube_upper, (float)(tubes.animationX.GetValue() - TubeUpper.PixelSize.Width) / 2 + UI_WIDTH / 2,
				-TubeUpper.PixelSize.Height + (float)((float)UI_HEIGHT * GROUND_LOCATION / MAP_HEIGHT * (tubes.y) / 100) - 75 + TOP_0,
				TubeUpper.PixelSize.Width, TubeUpper.PixelSize.Height);
				//lower
				bGraphics.DrawImage(Properties.Resources.tube_lower, (float)(tubes.animationX.GetValue() - TubeLower.PixelSize.Width) / 2 + UI_WIDTH / 2,
				(float)((float)UI_HEIGHT * GROUND_LOCATION / MAP_HEIGHT * (tubes.y) / 100) + 75 + TOP_0,
				TubeLower.PixelSize.Width, TubeLower.PixelSize.Height);
			}
			//Draw Yuanshi
			for (int i = 0; i < Yuanshis.Count; i++)
			{
				if (i >= Yuanshis.Count) break;
				var yuanshi = Yuanshis[i];
				bGraphics.DrawImage(Properties.Resources.yuanshi_smaller, ((float)yuanshi.animationX.GetValue() - YSBitmap.PixelSize.Width + UI_WIDTH) / 2,
						-YSBitmap.PixelSize.Height / 2 + (float)UI_HEIGHT * GROUND_LOCATION / MAP_HEIGHT * (float)yuanshi.y / 100 + TOP_0, YSBitmap.PixelSize.Width, YSBitmap.PixelSize.Height);
			}
			//Draw Slime
			for (int i = 0; i < Slimes.Count; i++)
			{
				if (i >= Slimes.Count) break;
				var slime = Slimes[i];
				System.Drawing.Bitmap SCurrent = Properties.Resources.slime0;
				switch ((UIWatch.ElapsedMilliseconds + EndWatch.ElapsedMilliseconds - slime.enterTime) / 200 % 4)
				{
					case 0: case 2: SCurrent = Properties.Resources.slime0; break;
					case 1: SCurrent = Properties.Resources.slime1; break;
					case 3: SCurrent = Properties.Resources.slime2; break;
				}
				bGraphics.DrawImage(SCurrent, (float)(slime.animationX.GetValue() - SCurrent.Size.Width) / 2 + UI_WIDTH / 2,
				(float)((float)UI_HEIGHT * (GROUND_LOCATION + SCurrent.Size.Height / 4) / MAP_HEIGHT * slime.animationY.GetValue() / 100),
				SCurrent.Size.Width, SCurrent.Size.Height);
			}
			//Draw Stone
			int bgComp = -BG_WIDTH;
			while (bgComp < UI_WIDTH)
			{
				bgComp += BG_WIDTH;
				bGraphics.DrawImage(Properties.Resources.stone, -UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * StoneBitmap.Size.Height / StoneBitmap.Size.Width);
			}
			//Draw Paimon
			System.Drawing.Bitmap PCurrent = Properties.Resources.pNormal;
			if (pState == 0)
				PCurrent = Properties.Resources.pNormal;
			else if (pState == 1)
				PCurrent = Properties.Resources.pFly;
			if (playState == 0)
			{
				bGraphics.DrawImage(PCurrent, (UI_WIDTH - PCurrent.Size.Width) / 2,
				(float)((UI_HEIGHT - PCurrent.Size.Height) / 2 * (GROUND_LOCATION / (float)MAP_HEIGHT) + RestAni.GetValue() + TOP_0)
				, PCurrent.Size.Width, PCurrent.Size.Height);
			}
			else
			{
				var rMatrix = Matrix3x2.CreateRotation((float)(RotationAni.GetValue() / 180 * Math.PI), new Vector2(
				UI_WIDTH / 2, (float)(UI_HEIGHT * (PLocation / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) + TOP_0));
				bGraphics.Transform = new System.Drawing.Drawing2D.Matrix(rMatrix.M11 * din, rMatrix.M12 * din, rMatrix.M21 * din, rMatrix.M22 * din, rMatrix.M31 * din, rMatrix.M32 * din);

				if (playState == 1)
					bGraphics.DrawImage(PCurrent, (UI_WIDTH - PCurrent.Size.Width) / 2,
					(float)(UI_HEIGHT * (PLocation / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) - PCurrent.Size.Height / 2 + TOP_0, PCurrent.Size.Width, PCurrent.Size.Height);
				else
				{
					PCurrent = Properties.Resources.pDead;
					if (RotationAni.IsAnimating)
						rMatrix = Matrix3x2.CreateRotation((float)(RotationAni.GetValue() / 180 * Math.PI), new Vector2(
							UI_WIDTH / 2, (float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) + TOP_0));
					else
						rMatrix = Matrix3x2.CreateRotation((float)(PRotation / 180 * Math.PI), new Vector2(
							UI_WIDTH / 2, (float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) + TOP_0));
					bGraphics.Transform = new System.Drawing.Drawing2D.Matrix(rMatrix.M11 * din, rMatrix.M12 * din, rMatrix.M21 * din, rMatrix.M22 * din, rMatrix.M31 * din, rMatrix.M32 * din);
					bGraphics.DrawImage(PCurrent, (UI_WIDTH - PCurrent.Size.Width) / 2,
				  (float)((UI_HEIGHT) * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) - PCurrent.Size.Height / 2 + TOP_0, PCurrent.Size.Width, PCurrent.Size.Height);
				}

				bGraphics.ResetTransform();
				bGraphics.TranslateTransform(0, 1);
				bGraphics.ScaleTransform(din, din);
			}
			//Draw Ground
			bgComp = -BG_WIDTH;
			while (bgComp < UI_WIDTH)
			{
				bgComp += BG_WIDTH;
				bGraphics.DrawImage(Properties.Resources.ground, -UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * GroundBitmap.Size.Height / GroundBitmap.Size.Width);

			}
			//Draw Title
			if (playState == 0)
				bGraphics.DrawImage(Properties.Resources.title, (UI_WIDTH - TitleBitmap.PixelSize.Width) / 2, 96, TitleBitmap.PixelSize.Width, TitleBitmap.PixelSize.Height);

			//Display Score
			int digits = 0;
			if (Score != 0)
				digits = (int)Math.Log10(Score);
			if (playState != 0)
			{
				for (int i = digits; i >= 0; i--)
				{
					System.Drawing.Bitmap numBitmap = GZero;
					switch (Score / (int)Math.Pow(10, i) % 10)
					{
						case 0: numBitmap = GZero; break;
						case 1: numBitmap = GOne; break;
						case 2: numBitmap = GTwo; break;
						case 3: numBitmap = GThree; break;
						case 4: numBitmap = GFour; break;
						case 5: numBitmap = GFive; break;
						case 6: numBitmap = GSix; break;
						case 7: numBitmap = GSeven; break;
						case 8: numBitmap = GEight; break;
						case 9: numBitmap = GNine; break;
					}
					int numWidth = numBitmap.Size.Width;
					int numHeight = numBitmap.Size.Height;
					int numBegin = digits * numWidth;
					bGraphics.DrawImage(numBitmap, (UI_WIDTH - numBegin) / 2 + (digits - i) * numWidth, 64, numWidth, numHeight);
				}
			}
			//Draw Buttons
			int aniTime = Convert.ToInt32(6 * GetEnterAni());
			if (!IsFSMouseOver)
				bGraphics.DrawImage(Properties.Resources.Fullscreen, UI_WIDTH - 48 - 6, aniTime, 48, 48);
			else
				bGraphics.DrawImage(Properties.Resources.Fullscreen, new Rectangle(UI_WIDTH - 48 - 6, aniTime, 48, 48), 0, 0, Properties.Resources.Fullscreen.Width, Properties.Resources.Fullscreen.Height, GraphicsUnit.Pixel, SetOpacity(0.5f)); ;
			if (MouseRelative.X >= UI_WIDTH - 54 - 54 && MouseRelative.X < UI_WIDTH - 6 - 54 && MouseRelative.Y >= 6 && MouseRelative.Y < 54)
			{
				if (isPlaySound)
					bGraphics.DrawImage(Properties.Resources.Sound, new Rectangle(UI_WIDTH - 48 - 54 - 6, aniTime, 48, 48), 0, 0, Properties.Resources.Sound.Width, Properties.Resources.Sound.Height, GraphicsUnit.Pixel, SetOpacity(0.5f));
				else
					bGraphics.DrawImage(Properties.Resources.DisableSound, new Rectangle(UI_WIDTH - 48 - 54 - 6, aniTime, 48, 48), 0, 0, Properties.Resources.DisableSound.Width, Properties.Resources.DisableSound.Height, GraphicsUnit.Pixel, SetOpacity(0.5f));
			}
			else
			{
				if (isPlaySound)
					bGraphics.DrawImage(Properties.Resources.Sound, UI_WIDTH - 48 - 54 - 6, aniTime, 48, 48);
				else
					bGraphics.DrawImage(Properties.Resources.DisableSound, UI_WIDTH - 48 - 54 - 6, aniTime, 48, 48);
			}
			if (ShowFPS)
			{
				bGraphics.DrawImage(Properties.Resources.GDI, new Rectangle(UI_WIDTH - GDI.PixelSize.Width - 4, UI_HEIGHT - GDI.PixelSize.Height - 4, GDI.PixelSize.Width, GDI.PixelSize.Height));
				bGraphics.DrawImage(Properties.Resources.FPS, new Rectangle(4, UI_HEIGHT - FPS.PixelSize.Height - 4, FPS.PixelSize.Width, FPS.PixelSize.Height));

				int fDigits = 0;
				if (Fps != 0)
					fDigits = (int)Math.Log10(Fps);
				for (int i = fDigits; i >= 0; i--)
				{
					System.Drawing.Bitmap fNumBitmap = GZero;
					switch (Fps / (int)Math.Pow(10, i) % 10)
					{
						case 0: fNumBitmap = GZero; break;
						case 1: fNumBitmap = GOne; break;
						case 2: fNumBitmap = GTwo; break;
						case 3: fNumBitmap = GThree; break;
						case 4: fNumBitmap = GFour; break;
						case 5: fNumBitmap = GFive; break;
						case 6: fNumBitmap = GSix; break;
						case 7: fNumBitmap = GSeven; break;
						case 8: fNumBitmap = GEight; break;
						case 9: fNumBitmap = GNine; break;
					}
					int numWidth = fNumBitmap.Width, numHeight = fNumBitmap.Height;
					int numBegin = digits * numWidth;
					bGraphics.DrawImage(fNumBitmap, new Rectangle(4 + FPS.PixelSize.Width + 2 + (fDigits - i) * fNumBitmap.Width, UI_HEIGHT - fNumBitmap.Height - 6, numWidth, numHeight));
				}
			}
			g.DrawImage(renderBitmap, 0, 0, ClientSize.Width, ClientSize.Height);
			bGraphics.Dispose();
			renderBitmap.Dispose();
			TmpFps++;
		}
		System.Drawing.Imaging.ImageAttributes SetOpacity(float opacity)
		{
			System.Drawing.Imaging.ImageAttributes attributes = new System.Drawing.Imaging.ImageAttributes();
			attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix() { Matrix33 = opacity });
			return attributes;
		}
		Point MouseRelative = new Point();
		Point ScreenRelative = new Point();
		bool IsFSMouseOver = false;
		void GetYuanshi(Yuanshi yuanshi)
		{
			yuanshi.animationX.Stop();
			Yuanshis.Remove(yuanshi);
			Score += 10;
			PlayPass();
		}
		int LastTime = 0;
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
			Random random = new Random();
			double delta = MOVE_UNIT / 2 * 20.0 / 19.5;
			THAnimations.EasyAni tubeAnimation = new THAnimations.EasyAni();
			tubeAnimation.From = MOVE_UNIT * 2; tubeAnimation.To = -MOVE_UNIT * 4; tubeAnimation.Pow = 1; tubeAnimation.EasingFunction = THAnimations.EasingFunction.Linear; tubeAnimation.Duration = 11;
			Tube tube = new Tube() { x = MOVE_UNIT, y = random.NextDouble() * 60 + 20, animationX = tubeAnimation };
			tubeAnimation.Animated = (object o, EventArgs a) => { Tubes.Remove(tube); };
			tube.isPass = false;
			Tubes.Add(tube);
			THAnimations.EasyAni slimeAnimationX = new THAnimations.EasyAni()
			{
				From = MOVE_UNIT * 2 + delta,
				To = -MOVE_UNIT * 4 + delta,
				Pow = 1,
				EasingFunction = THAnimations.EasingFunction.Linear,
				Duration = 11
			};
			Slime slime = new Slime()
			{
				x = MOVE_UNIT * 2 + delta,
				y = random.NextDouble() * 80 + 10,
				enterTime = UIWatch.ElapsedMilliseconds,
				animationX = slimeAnimationX,
				direction = Convert.ToInt32(random.NextDouble())
			};//
			slimeAnimationX.Animated = (object o, EventArgs a) =>
			{
				if (slime.animationY != null)
					slime.animationY.Stop();
				Slimes.Remove(slime);
			};
			tubeAnimation.Restart();
			slimeAnimationX.Restart();
			RegestryAnimationY(slime);
			Slimes.Add(slime);
			int yuanshiNum = Convert.ToInt32(random.NextDouble());
			if (yuanshiNum == 0)
			{
				Yuanshi yuanshi = new Yuanshi() { y = random.NextDouble() * 80 + 10 };
				THAnimations.EasyAni yuanshiAnimationX = new THAnimations.EasyAni()
				{
					From = MOVE_UNIT * 2 + delta,
					To = -MOVE_UNIT * 4 + delta,
					Pow = 1,
					EasingFunction = THAnimations.EasingFunction.Linear,
					Duration = 11
				};
				yuanshi.animationX = yuanshiAnimationX;
				yuanshiAnimationX.Animated = (object o, EventArgs a) =>
				{
					Yuanshis.Remove(yuanshi);
				};
				yuanshiAnimationX.Start();
				Yuanshis.Add(yuanshi);
			}
		}
		void RegestryAnimationY(Slime slime)
		{
			if (slime.animationY != null) slime.animationY.Stop();
			THAnimations.EasyAni animationY = new THAnimations.EasyAni();
			animationY.From = slime.y;
			switch (slime.direction)
			{
				case 0: animationY.To = slime.y - 37.5; break;
				case 1: animationY.To = slime.y + 37.5; break;
			}
			animationY.EasingFunction = THAnimations.EasingFunction.PowerInOut; animationY.Pow = 2;
			animationY.Animating = (object o, EventArgs a) =>
			{
				if ((slime.direction == 0 && animationY.GetValue() >= 0) || (slime.direction == 1 && animationY.GetValue() <= 100))
					slime.y = animationY.GetValue();
				else
				{
					slime.direction = slime.direction == 0 ? slime.direction = 1 : slime.direction = 0;
					RegestryAnimationY(slime);
				}
			};
			slime.animationY = animationY;
			animationY.Start();
		}
		void GameOver()
		{
			PlayHit();
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
			EndWatch.Restart();
		}
		RawRectangleF RelRectangleF(float x, float y, float w, float h)
		{
			return new RawRectangleF(x, y, w + x, h + y);
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			//Render();
		}

		private RawMatrix3x2 ConvertMatrix(Matrix3x2 src)
		{
			return new RawMatrix3x2(src.M11, src.M12, src.M21, src.M22, src.M31, src.M32);
		}
		public bool isFullScreen = false, allowState = true;
		FormWindowState rState;
		bool CanSetTouch = false;
		protected override void WndProc(ref Message m)
		{
			var ustate = this.WindowState;
			base.WndProc(ref m);
			if (playState != -1)
			{
				float DPI = this.CreateGraphics().DpiX / 96;
			}
		}
		private void GameUI_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				UseCompatibleMode = !UseCompatibleMode;
				return;
			}
			if (playState == 0)
			{
				if (IsFSMouseOver)
				{
					FullScreen();
					CanSetTouch = false; EnterPosition = 0;
					return;
				}
				if (MouseRelative.X >= UI_WIDTH - 54 - 54 && MouseRelative.X < UI_WIDTH - 6 - 54 && MouseRelative.Y >= 6 && MouseRelative.Y < 54 && playState == 0)
				{
					if (isPlaySound)
					{
						BGMPlayer.IsMuted = true;
						HitPlayer.IsMuted = true;
						PassPlayer.IsMuted = true;
						PressPlayer.IsMuted = true;
					}
					else
					{
						BGMPlayer.IsMuted = false;
						HitPlayer.IsMuted = false;
						PassPlayer.IsMuted = false;
						PressPlayer.IsMuted = false;
					}
					isPlaySound = !isPlaySound;
					CanSetTouch = false; EnterPosition = 0;
					return;
				}
			}
			Press(sender, e);
			if (CanSetTouch)
			{
				TouchIndex = 0;
				EnterPosition = ScreenRelative.Y;
			}
		}
		public void FullScreen()
		{
			float DPI = this.CreateGraphics().DpiX / 96;
			this.MinimumSize = new Size(0, 0);
			if (!isFullScreen)
			{
				rState = this.WindowState;
				if (this.WindowState == FormWindowState.Maximized)
				{
					this.FormBorderStyle = FormBorderStyle.None;
					this.WindowState = FormWindowState.Normal;
				}
				else
				{
					this.WindowState = FormWindowState.Maximized;
					this.FormBorderStyle = FormBorderStyle.None;
					this.WindowState = FormWindowState.Normal;
					this.WindowState = FormWindowState.Maximized;
				}
				this.WindowState = FormWindowState.Maximized;
				isFullScreen = true;
				this.MinimumSize = new Size(Convert.ToInt32(320 * DPI), Convert.ToInt32(240 * DPI));
			}
			else
			{
				allowState = false;
				this.FormBorderStyle = FormBorderStyle.Sizable;
				this.WindowState = rState;
				isFullScreen = false;
				allowState = true;
				this.MinimumSize = new Size(this.Width - this.ClientSize.Width + Convert.ToInt32(320 * DPI), this.Height - this.ClientSize.Height + Convert.ToInt32(240 * DPI));
			}
		}
		THAnimations.EasyAni GameAni;
		THAnimations.EasyAni RotationAni;
		private void Press(object sender, EventArgs e)
		{
			if (playState == -1) return;
			if (playState == 0)
			{
				EndWatch.Stop();
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
				GameTime = UIWatch.ElapsedMilliseconds;
				Press(sender, e);
			}
			else if (playState == 1)
			{
				GameAni?.Stop();
				GameAni = new THAnimations.EasyAni(); GameAni.Pow = 2;
				GameAni.Progress = 0;
				GameAni.From = PLocation; GameAni.To = PLocation - 10; GameAni.Description = "up"; GameAni.EasingFunction = THAnimations.EasingFunction.PowerOut;
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
				RotationAni = new THAnimations.EasyAni() { From = -3, To = 117, Duration = 2, EasingFunction = THAnimations.EasingFunction.PowerIn, Pow = 2 };
				RotationAni.Animating = (object o, EventArgs a) => { if (RotationAni.IsAnimating) { PRotation = RotationAni.GetValue(); } };
				RotationAni.Restart();
				PlayPress();
				GC.Collect();
			}
			else if (playState == 2)
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
				Score = 0;
				EndWatch.Restart(); EndWatch.Stop();
				GameAni.Restart();GameAni.Stop();
				PlayBGM();
				TmpFps = 0;
				Fps = 0;
				GC.Collect();
			}
		}
		private void AniUp()
		{

		}
		double LastLocation;
		private void AniDown()
		{
			GameAni = new THAnimations.EasyAni();
			GameAni.From = PLocation;
			LastLocation = PLocation;
			GameAni.To = LastLocation + 100;
			GameAni.Description = "down";
			GameAni.EasingFunction = THAnimations.EasingFunction.PowerIn;
			GameAni.Pow = 2;
			GameAni.Duration = 0.8;
			GameAni.Animating += (object o, EventArgs a) =>
			{
				if (playState == 1)
					PLocation = GameAni.GetValue();
				if (GameAni.GetValue() >= 100)
				{
					GameAni.To = 100 + PRotation / 30; GameAni.Stop();
					RotationAni.To = RotationAni.GetValue(); RotationAni.Stop();
				}
			};
			GameAni.Restart();
			GC.Collect();
		}
		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F11) { FullScreen(); return; }
			if (e.KeyCode == Keys.F12) { System.Diagnostics.Process.Start("https://g.evkgame.cn/214101"); return; }
			if (e.KeyCode == Keys.F3) { ShowFPS = !ShowFPS; return; }
			if (e.Alt || e.Control || e.Shift || e.KeyCode == Keys.LWin || e.KeyCode == Keys.RWin) return;
			Press(sender, e);
		}
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (e.Delta > 0 && playState != 2) Press(null, new EventArgs());
		}
		WindowRenderTarget RenderTarget;
		RawColor4 ConvertColor(Color source)
		{
			return new RawColor4((float)source.R / 255, (float)source.G / 255, (float)source.B / 255, (float)source.A / 255);
		}
		Factory factory = null;
		void InitDevices()
		{
			factory = new Factory(FactoryType.SingleThreaded);
			RenderTargetProperties properties = new RenderTargetProperties()
			{
				PixelFormat = new PixelFormat(),
				Usage = RenderTargetUsage.None,
				Type = RenderTargetType.Default
			};
			HwndRenderTargetProperties hwProperties = new HwndRenderTargetProperties()
			{
				Hwnd = GameUI.Handle,
				PixelSize = new Size2(GameUI.Width, GameUI.Height),
				PresentOptions = PresentOptions.Immediately
			};
			RenderTarget = new WindowRenderTarget(factory, properties, hwProperties)
			{
				AntialiasMode = AntialiasMode.PerPrimitive
			};

		}
		SharpDX.Direct2D1.Bitmap ConvertBitmap(System.Drawing.Bitmap source)
		{
			System.Drawing.Bitmap formattedBitmap = new System.Drawing.Bitmap(source.Width, source.Height);
			Graphics.FromImage(formattedBitmap).DrawImage(source, new RectangleF(0, 0, source.Width, source.Height));
			System.Drawing.Imaging.BitmapData bitmapData = formattedBitmap.LockBits(new Rectangle(0, 0, source.Width, source.Height),
			System.Drawing.Imaging.ImageLockMode.ReadOnly,
			formattedBitmap.PixelFormat);
			byte[] memory = new byte[bitmapData.Stride * formattedBitmap.Height];
			IntPtr scan = bitmapData.Scan0;
			//MessageBox.Show("ot");
			System.Runtime.InteropServices.Marshal.Copy(scan, memory, 0, bitmapData.Stride * formattedBitmap.Height);
			formattedBitmap.UnlockBits(bitmapData);
			BitmapProperties bp = new BitmapProperties()
			{
				PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied),
				DpiX = formattedBitmap.HorizontalResolution,
				DpiY = formattedBitmap.VerticalResolution
			};
			SharpDX.Direct2D1.Bitmap dBitmap = new SharpDX.Direct2D1.Bitmap(RenderTarget, new Size2(source.Width, source.Height), bp);
			dBitmap.CopyFromMemory(memory, bitmapData.Stride);
			formattedBitmap.Dispose();
			return dBitmap;
		}
		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			this.Hide();
			System.Environment.Exit(0);
		}
		IntPtr WindowDC;
		private void Form1_Load(object sender, EventArgs e)
		{
			float DPI = this.CreateGraphics().DpiX / 96;
			ClientSize = new Size(Convert.ToInt32(1066 * DPI), Convert.ToInt32(600 * DPI));
			this.Controls.Add(loadControl);
			loadControl.Dock = DockStyle.Fill;
			loadControl.Failed = (object o, EventArgs a) => { };
			loadControl.Completed = (object o, EventArgs a) => { playState = 0; loadControl.LoadResources(); InitGame(); this.Controls.Remove(loadControl); loadControl.Dispose(); };
			this.MinimumSize = new Size(this.Width - this.ClientSize.Width + Convert.ToInt32(320 * DPI), this.Height - this.ClientSize.Height + Convert.ToInt32(240 * DPI));
			this.Left = (SystemInformation.WorkingArea.Width - this.Width) / 2;
			this.Top = (SystemInformation.WorkingArea.Height - this.Height) / 2;
			if (this.Width > SystemInformation.WorkingArea.Width || this.Height > SystemInformation.WorkingArea.Height) FullScreen();
			loadControl.CheckResources();
		}
		void GLInit()
		{
			WindowDC = GetDC(this.Handle);
			SharpGL.Win32.PIXELFORMATDESCRIPTOR pfd = new SharpGL.Win32.PIXELFORMATDESCRIPTOR();
			pfd.Init();
			pfd.nVersion = 1;
			pfd.dwFlags = SharpGL.Win32.PFD_DRAW_TO_WINDOW | SharpGL.Win32.PFD_SUPPORT_OPENGL | SharpGL.Win32.PFD_DOUBLEBUFFER;
			pfd.iPixelType = SharpGL.Win32.PFD_TYPE_RGBA;
			pfd.cColorBits = (byte)32;
			pfd.cDepthBits = 16;
			pfd.cStencilBits = 8;
			pfd.iLayerType = SharpGL.Win32.PFD_MAIN_PLANE;
			int iPixelFormat = SharpGL.Win32.ChoosePixelFormat(WindowDC, pfd);
			SharpGL.Win32.SetPixelFormat(WindowDC, iPixelFormat, pfd);

			//GLDevice = OpenGL.DeviceContext.Create(IntPtr.Zero, this.Handle);
			GLContext = SharpGL.Win32.wglCreateContext(WindowDC);
			//GLDevice.ChoosePixelFormat(new OpenGL.DevicePixelFormat(24));
			isinited = true;
		}
	}
	public class BufferedPanel : Panel
	{
		public BufferedPanel()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}
	}
	public class Slime
	{
		public Slime() { }
		public double x { get; set; }
		public double y { get; set; }
		public long enterTime { get; set; }
		public int direction { get; set; }
		public THAnimations.EasyAni animationX { get; set; }
		public THAnimations.EasyAni animationY { get; set; }
	}
	public class Tube
	{
		public Tube() { }
		public double x { get; set; }
		public double y { get; set; }
		public bool isPass { get; set; }
		public THAnimations.EasyAni animationX { get; set; }
	}
}