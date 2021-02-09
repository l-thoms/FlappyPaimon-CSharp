using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace FlappyPaimon
{
	public partial class Form1 : Form
	{
		Stopwatch UIWatch = new Stopwatch();
		public Form1()
		{
			InitializeComponent();
			UIWatch.Start();
			UItimer.Start();
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
		}
		THAnimations.EasyAni RestAni = new THAnimations.EasyAni() { Description = "up", From = -10, To = 10,EasingFunction = THAnimations.EasingFunction.PowerInOut,Pow=2,Duration = 0.5 };
		Timer RestChecker = new Timer() { Interval = 1, Enabled = true };
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
		private void UItimer_Tick(object sender, EventArgs e)
		{
			Render();
		}
		//void Render(Graphics g)
		void Render()
		{
			float density = (float)ClientSize.Height / UI_HEIGHT;
			int UI_WIDTH = (int)(ClientSize.Width / density);
			Bitmap b = new Bitmap(UI_WIDTH/2, UI_HEIGHT/2);
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
			if(playState==0)
			{
				Bitmap pCurrent = Properties.Resources.pNormal;
				if (pState == 0)
					pCurrent = Properties.Resources.pNormal;
				else if (pState == 1)
					pCurrent = Properties.Resources.pFly;
				bg.DrawImage(pCurrent, (UI_WIDTH - pCurrent.Width) / 2, (int)((UI_HEIGHT - pCurrent.Height) / 2 + RestAni.GetValue()),pCurrent.Width,pCurrent.Height);
			}
			//最终输出
			Bitmap scaler = new Bitmap(UI_WIDTH * (int)(density + 1), UI_HEIGHT * (int)(density + 1));
			Graphics sg = Graphics.FromImage(scaler);
			sg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			sg.DrawImage(b, 0, 0, scaler.Width, scaler.Height);
			//g.DrawImage(scaler, 0,0,UI_WIDTH,UI_HEIGHT);
			this.BackgroundImage = scaler;
			bg.Dispose();
			b.Dispose();
			//scaler.Dispose();
			sg.Dispose();
		}
	}
}
