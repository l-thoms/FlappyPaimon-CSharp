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
using System.IO;

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
		bool PerfMod = false;
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
		public void Logics()
		{
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
			if (e.KeyCode == Keys.F5) { PerfMod = !PerfMod; return; }
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
		Point MouseRelative = new Point();
		Point ScreenRelative = new Point();
		bool IsFSMouseOver = false;
		bool CanSetTouch = false;
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