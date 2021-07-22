using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.IO;

namespace FlappyPaimon
{
	public partial class Form1
	{

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
			Logics();
			if (!isLoaded) { UIWatch.Restart(); PlayBGM(); isLoaded = true; }
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
		void CompatibleLoop()
		{
			/*while (true) RenderCompatible(this.CreateGraphics());*/
			while (true) { this.Invoke(new Action(() => { this.Refresh(); })); }
		}
		public float Fit(float input)
		{
			if (PerfMod)
				return Convert.ToInt32(input) / 2 * 2;
			return input;
		}
		public void RenderSDX()
		{
			if (playState == -1) return;
			#region Direct2D
			GameUI.BackgroundImage = null;
			float density = (float)ClientSize.Height / UI_HEIGHT;
			float din = (float)Math.Ceiling(density * 2) / 2;
			if (PerfMod) din = 0.5f;
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
				RenderTarget.DrawBitmap(CloudBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 20 % BG_WIDTH + cloudComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * CloudBitmap.PixelSize.Height / (float)CloudBitmap.PixelSize.Width), 1, BitmapInterpolationMode.NearestNeighbor);
			}

			int forestComp = -FOREST_WIDTH;
			while (forestComp < UI_WIDTH)
			{
				forestComp += FOREST_WIDTH;
				RenderTarget.DrawBitmap(ForestBitmap, RelRectangleF(Fit(-UIWatch.ElapsedMilliseconds / 10 % FOREST_WIDTH + forestComp), UI_HEIGHT - MAP_HEIGHT, FOREST_WIDTH, FOREST_WIDTH * ForestBitmap.PixelSize.Height / (float)ForestBitmap.PixelSize.Width), 1, BitmapInterpolationMode.NearestNeighbor);
			}
			//Draw Obstacle
			foreach (var tubes in Tubes)
			{
				//upper
				RenderTarget.DrawBitmap(TubeUpper, RelRectangleF(Fit((float)(tubes.animationX.GetValue() - TubeUpper.PixelSize.Width) / 2 + UI_WIDTH / 2),
				Fit(-TubeUpper.PixelSize.Height + (float)((float)UI_HEIGHT * GROUND_LOCATION / MAP_HEIGHT * (tubes.y) / 100) - 75 + TOP_0),
				TubeUpper.PixelSize.Width, TubeUpper.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
				//lower
				RenderTarget.DrawBitmap(TubeLower, RelRectangleF(Fit((float)(tubes.animationX.GetValue() - TubeLower.PixelSize.Width) / 2 + UI_WIDTH / 2),
				Fit((float)((float)UI_HEIGHT * GROUND_LOCATION / MAP_HEIGHT * (tubes.y) / 100) + 75 + TOP_0),
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
				RenderTarget.DrawBitmap(SCurrent, RelRectangleF(Fit((float)(slime.animationX.GetValue() - SCurrent.PixelSize.Width) / 2 + UI_WIDTH / 2),
				Fit((float)((float)UI_HEIGHT * (GROUND_LOCATION + SCurrent.PixelSize.Height / 4) / MAP_HEIGHT * slime.animationY.GetValue() / 100)),
				SCurrent.PixelSize.Width, SCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
			}
			//Draw Stone
			int bgComp = -BG_WIDTH;
			while (bgComp < UI_WIDTH)
			{
				bgComp += BG_WIDTH;
				RenderTarget.DrawBitmap(StoneBitmap, RelRectangleF(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp, UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * StoneBitmap.PixelSize.Height / (float)StoneBitmap.PixelSize.Width), 1, BitmapInterpolationMode.NearestNeighbor);

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
				Fit((float)((UI_HEIGHT - PCurrent.PixelSize.Height) / 2 * (GROUND_LOCATION / (float)MAP_HEIGHT) + RestAni.GetValue() + TOP_0))
				, PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
			}
			else
			{
				RawMatrix3x2 oldMatrix = RenderTarget.Transform;
				RenderTarget.Transform = ConvertMatrix(Matrix3x2.CreateRotation((float)(RotationAni.GetValue() / 180 * Math.PI), new Vector2(
				UI_WIDTH / 2, (float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) + TOP_0)));
				if (playState == 1)
					RenderTarget.DrawBitmap(PCurrent, RelRectangleF((UI_WIDTH - PCurrent.PixelSize.Width) / 2,
					Fit((float)(UI_HEIGHT * (GameAni.GetValue() / 100) * (GROUND_LOCATION / (float)MAP_HEIGHT)) - PCurrent.PixelSize.Height / 2 + TOP_0),
					PCurrent.PixelSize.Width, PCurrent.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);
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
				RenderTarget.DrawBitmap(GroundBitmap, RelRectangleF(Fit(-UIWatch.ElapsedMilliseconds / 5 % BG_WIDTH + bgComp), UI_HEIGHT - MAP_HEIGHT, BG_WIDTH, BG_WIDTH * GroundBitmap.PixelSize.Height / (float)GroundBitmap.PixelSize.Width), 1, BitmapInterpolationMode.NearestNeighbor);

			}
			//Draw Title
			if (playState == 0)
				RenderTarget.DrawBitmap(TitleBitmap, RelRectangleF(Fit((UI_WIDTH - TitleBitmap.PixelSize.Width) / 2), 96, TitleBitmap.PixelSize.Width, TitleBitmap.PixelSize.Height), 1, BitmapInterpolationMode.NearestNeighbor);


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
					RenderTarget.DrawBitmap(numBitmap, RelRectangleF(Fit((UI_WIDTH - numBegin) / 2 + (digits - i) * numWidth), 64, numWidth, numHeight), 1, BitmapInterpolationMode.NearestNeighbor);
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

		private void Form1_ResizeBegin(object sender, EventArgs e)
		{
			if (playState != -1) t1.Start();
		}

		private void Form1_ResizeEnd(object sender, EventArgs e)
		{
			if (playState != -1) t1.Stop();
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
			if (PerfMod) din = 0.5f;
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
			if (PerfMod) g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
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
		RawRectangleF RelRectangleF(float x, float y, float w, float h)
		{
			return new RawRectangleF(x, y, w + x, h + y);
		}
		private RawMatrix3x2 ConvertMatrix(Matrix3x2 src)
		{
			return new RawMatrix3x2(src.M11, src.M12, src.M21, src.M22, src.M31, src.M32);
		}
		public bool isFullScreen = false, allowState = true;
		FormWindowState rState;
		protected override void WndProc(ref Message m)
		{
			var ustate = this.WindowState;
			base.WndProc(ref m);
			if (playState != -1)
			{
				float DPI = this.CreateGraphics().DpiX / 96;
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
		byte[] ImageToBytes(System.Drawing.Bitmap source)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				source.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
				byte[] data = new byte[stream.Length];
				stream.Seek(0, SeekOrigin.Begin);
				stream.Read(data, 0, Convert.ToInt32(stream.Length));
				return data;
			}
		}
	}
}
