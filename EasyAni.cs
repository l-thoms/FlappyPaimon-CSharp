using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
/*
	The core engine, which can be used to control the movements and animations.
 */
namespace THAnimations
{
	public enum EasingFunction
	{
		Linear, PowerIn, PowerOut, PowerInOut, SineIn, SineOut, SineInOut
	}
	public class EasyAni
	{
		public EasingFunction EasingFunction { get; set; }
		public double Pow { get; set; }
		public double Progress { get; set; }
		public double Duration { get; set; }
		public double From { get; set; }
		public double To{ get; set; }
		public string Description{ get; set; }
		public bool IsAnimating{ get{
				if (aniTimer == null) return false;
				else return true;
		}
		}
		public EventHandler Animating;
		public EventHandler Animated;
		Timer aniTimer = null;
		Stopwatch aniWatch;
		public EasyAni()
		{
			Pow = 3; Duration = 1; From = 0; To = 100; EasingFunction = EasingFunction.PowerOut;
			aniWatch = new Stopwatch();
		}
		public EasyAni(double duration,double from,double to,double pow,EasingFunction easingFunction)
		{
			Pow = pow; Duration = duration; From = from; To = to; EasingFunction = EasingFunction;
		}
		public void Start()
		{
			if(aniTimer == null)
			{
				aniTimer = new Timer() { Interval = 1 };
				aniTimer.Tick += AniTimer_Tick;
				aniWatch.Restart();
				aniTimer.Start();
				aniTimer.Tick += Animating;
				aniTimer.Disposed += Animated;
			}
			else
			{
				aniWatch.Start();
			}
		}
		private void AniTimer_Tick(object sender, EventArgs e)
		{
			long tick = aniWatch.ElapsedMilliseconds;
			if (Convert.ToDouble(tick)/1000<=Duration)
			{
				Progress = Convert.ToDouble(tick)/1000/Duration;
			}
			else
			{
				Progress = 1;
				Stop();
			}
		}
		public double GetValue()
		{
			double current = 0;
			double Progress;
			if (IsAnimating)
				Progress = Convert.ToDouble(aniWatch.ElapsedMilliseconds) / 1000 / Duration;
			else
				Progress = this.Progress;
			switch (EasingFunction)
			{
				default:
					current = Progress;	
				break;
				case EasingFunction.PowerIn:
					current = Math.Pow(Progress, Pow);
				break;
				case EasingFunction.PowerOut:
					current = -Math.Pow(-Progress + 1, Pow) + 1;
				break;
				case EasingFunction.PowerInOut:
					if (Progress <= 0.5) current = Math.Pow(Progress * 2, Pow) / 2;
					else current = -(Math.Pow(-2 * Progress + 2, Pow) - 2) / 2;
				break;
				case EasingFunction.SineIn:
					current = Math.Sin(0.5 * Math.PI * (Progress - 1)) + 1;
				break;
				case EasingFunction.SineOut:
					current = Math.Sin(0.5 * Math.PI * Progress);	
				break;
				case EasingFunction.SineInOut:
					current = Math.Sin(Math.PI * (Progress - 0.5)) * 0.5 + 0.5;
				break;
			}
			current = current * (To - From) + From;
			return current;
		}
		public void Stop()
		{
			if (aniTimer != null)
			{
				Progress = 1;
				aniTimer.Dispose();
				aniTimer = null;
			}
		}
		public void Pause()
		{
			aniWatch.Stop();
		}
		public void Restart()
		{
			if(aniTimer!=null)
				aniTimer.Disposed -= Animated;
			Stop();
			Start();
		}
	}
}
