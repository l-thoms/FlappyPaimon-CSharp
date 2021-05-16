using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlappyPaimon.Properties;
namespace FlappyPaimon
{
	public partial class LoadControl : UserControl
	{
		public LoadControl()
		{
			InitializeComponent();
			CheckForIllegalCrossThreadCalls = false;
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
			ResTimer.Tick += ResTimer_Tick;
		}

		private void LoadControl_Load(object sender, EventArgs e)
		{
		}

		private void ResTimer_Tick(object sender, EventArgs e)
		{
			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.StartupPath + "\\Resources\\");
			FileCount = di.GetFiles().Length;
			this.Refresh();
			if(IsFailed)
			{
				this.Refresh();
				if (Failed != null) Failed.Invoke(null, new EventArgs());ResTimer.Stop();
			}
			if(IsCompleted)
			{
				if (Completed != null) Completed.Invoke(null, new EventArgs());ResTimer.Stop();
			}
		}

		const int UI_HEIGHT = 600;
		int FileCount = 0;
		int ResCount = 32;
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			int UI_WIDTH = Convert.ToInt32((float)UI_HEIGHT / this.Height * this.Width);
			e.Graphics.Clear(Color.White);
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
			e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			e.Graphics.ScaleTransform(Width / (float)UI_WIDTH, Height / (float)UI_HEIGHT);
			if (!IsFailed)
			{
				var font = new Font(System.Drawing.SystemFonts.MenuFont.FontFamily, 32);
				var content = "Loading resources...";
				SizeF measureSize = e.Graphics.MeasureString(content, font);
				e.Graphics.DrawString(content, font, Brushes.Black, new RectangleF(UI_WIDTH / 2 - measureSize.Width / 2, 470, measureSize.Width, measureSize.Height));
				string resPath = Application.StartupPath + "\\Resources\\";
				if (System.IO.File.Exists(resPath + "title.png") && FileCount != 0)
				{
					Bitmap Title = Bitmap.FromFile(resPath + "title.png") as Bitmap;
					Bitmap ProgressMap = new Bitmap(Convert.ToInt32(Title.Width / (float)ResCount * FileCount), Title.Height);
					Graphics.FromImage(ProgressMap).DrawImage(Title, new Point(0, 0));
					e.Graphics.DrawImage(ProgressMap, new Rectangle(UI_WIDTH / 2 - Title.Width / 2, 96,ProgressMap.Width,Title.Height));
					Title.Dispose();
					ProgressMap.Dispose();
				}
				font.Dispose();
			}
			else
			{
				e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				e.Graphics.DrawImage(Properties.Resources.Failed, new Rectangle(UI_WIDTH / 2 - Properties.Resources.Failed.Width / 2, 84, Properties.Resources.Failed.Width, Properties.Resources.Failed.Height));
				e.Graphics.TranslateTransform(0, -20);
				var font = new Font(System.Drawing.SystemFonts.MenuFont.FontFamily, 32);
				var content = "Failed to load resources.\nPlease check your network settings.";
				SizeF measureSize = e.Graphics.MeasureString(content, font);
				SizeF normalSize = e.Graphics.MeasureString("Exit", font);
				e.Graphics.DrawString(content, font, Brushes.Black, new RectangleF(UI_WIDTH / 2 - measureSize.Width / 2, 490-normalSize.Height, measureSize.Width, measureSize.Height),
				new StringFormat() { Alignment = StringAlignment.Center });
				font = new Font(System.Drawing.SystemFonts.MenuFont.FontFamily, 16);
				e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0x00, 0x78, 0xd7)), new Rectangle(UI_WIDTH / 2 - 40, 555, 80, 30));
				normalSize = e.Graphics.MeasureString("Exit", font);
				e.Graphics.DrawString("Exit", font, Brushes.White, new RectangleF(UI_WIDTH / 2 - normalSize.Width / 2, 556, normalSize.Width, normalSize.Height),
				new StringFormat() { Alignment = StringAlignment.Center });
			}
		}
		public bool IsFailed = false;
		public EventHandler Failed;
		public EventHandler Completed;
		Timer ResTimer = new Timer() { Interval = 1 };
		public void CheckResources()
		{
			ResTimer.Start();
			System.Threading.Thread checkThread = new System.Threading.Thread(new System.Threading.ThreadStart(check));
			checkThread.Start();
			void check()
			{
				if (System.IO.Directory.Exists(Application.StartupPath + "\\Resources") == false) System.IO.Directory.CreateDirectory(Application.StartupPath + "\\Resources");
				System.Xml.XmlDocument resourcesDoc = new System.Xml.XmlDocument();
				resourcesDoc.LoadXml(Properties.Resources.ResourceList);
				foreach (System.Xml.XmlNode x in resourcesDoc.DocumentElement.ChildNodes)
				{
					if (x.Name == "file")
					{
						string name = "", src = "";
						foreach (System.Xml.XmlAttribute attr in x.Attributes) { if (attr.Name == "name") name = attr.Value; if (attr.Name == "src") src = attr.Value; }
						string path = Application.StartupPath + "\\Resources\\" + name;
						if (System.IO.File.Exists(path) == false)
						{
							System.Net.HttpWebResponse res;
							System.Net.WebRequest request = System.Net.WebRequest.Create(src);
							try
							{
								res = request.GetResponse() as System.Net.HttpWebResponse;
							}
							catch
							{
								IsFailed = true;
								return;
							}
							if (res.StatusCode == System.Net.HttpStatusCode.OK)
							{
								byte[] fByte;
								System.IO.MemoryStream ms = new System.IO.MemoryStream();
								res.GetResponseStream().CopyTo(ms);
								fByte = ms.ToArray();
								ms.Close();
								ms.Flush();
								System.IO.File.WriteAllBytes(path, fByte);
							}
						}
					}
					if (x.Name == "base64")
					{
						string name = "";
						foreach (System.Xml.XmlAttribute attr in x.Attributes) if (attr.Name == "name") name = attr.Value;
						string path = Application.StartupPath + "\\Resources\\" + name;
						byte[] data = Convert.FromBase64String(x.InnerText);
						System.IO.File.WriteAllBytes(path, data);
					}
					if (x.Name == "array")
					{
						string src = "";
						foreach (System.Xml.XmlAttribute attr in x.Attributes) { if (attr.Name == "src") src = attr.Value; }
						bool fullExist = true;
						foreach (System.Xml.XmlNode crop in x.ChildNodes)
						{
							string cropName = "";
							foreach (System.Xml.XmlAttribute cAttr in crop.Attributes) if (cAttr.Name == "name") cropName = cAttr.Value;
							if (System.IO.File.Exists(Application.StartupPath + "\\Resources\\" + cropName) == false) fullExist = false;
						}
						System.Drawing.Image aImage = null;
						if (!fullExist)
						{
							System.Net.HttpWebResponse res;
							System.Net.WebRequest request = System.Net.WebRequest.Create(src);
							try
							{
								res = request.GetResponse() as System.Net.HttpWebResponse;
							}
							catch
							{
								IsFailed = true;
								return;
							}
							if (res.StatusCode == System.Net.HttpStatusCode.OK)
							{
								aImage = System.Drawing.Image.FromStream(res.GetResponseStream());
								foreach (System.Xml.XmlNode crop in x.ChildNodes)
								{
									string cropName = "", rectStr = "";
									foreach (System.Xml.XmlAttribute cAttr in crop.Attributes) { if (cAttr.Name == "name") cropName = cAttr.Value; if (cAttr.Name == "rect") rectStr = cAttr.Value; }
									int[] rect = new int[4];
									int currentDigits = 0;
									string tmpStr = "";
									for (int i = 0; i < rectStr.Length; i++)
									{
										if (rectStr[i] != ' ')
										{
											tmpStr += rectStr[i];
										}
										else
										{
											rect[currentDigits] = Convert.ToInt32(tmpStr);
											currentDigits++;
											tmpStr = "";
										}
										if (i == rectStr.Length - 1)
										{
											rect[currentDigits] = Convert.ToInt32(tmpStr);
										}
									}
									Bitmap childMap = new Bitmap(rect[2], rect[3]);
									Graphics.FromImage(childMap).DrawImage(aImage, new Rectangle(-rect[0], -rect[1], aImage.Width, aImage.Height));
									if (System.IO.File.Exists(Application.StartupPath + "\\Resources\\" + cropName) == false)
										childMap.Save(Application.StartupPath + "\\Resources\\" + cropName);
									childMap.Dispose();
								}
								if (aImage != null)
									aImage.Dispose();
							}
						}
					}
				}
				IsCompleted = true;
			}
		}
		bool IsCompleted = false;
		public void LoadResources()
		{
			string resPath = Application.StartupPath + "\\Resources\\";
			Resources.bainianji = Bitmap.FromFile(resPath + "preserve1.png")as Bitmap;
			Resources.cloud = Bitmap.FromFile(resPath + "cloud.png")as Bitmap;
			Resources.DisableSound = Bitmap.FromFile(resPath + "disablesound.png")as Bitmap;
			Resources.distance = Bitmap.FromFile(resPath + "preserve2.png")as Bitmap;
			Resources.forest = Bitmap.FromFile(resPath + "forest.png")as Bitmap;
			Resources.FPS = Bitmap.FromFile(resPath + "fps.png")as Bitmap;
			Resources.Fullscreen = Bitmap.FromFile(resPath + "fullscreen.png")as Bitmap;
			Resources.GDI = Bitmap.FromFile(resPath + "gdi.png")as Bitmap;
			Resources.ground = Bitmap.FromFile(resPath + "ground.png")as Bitmap;
			Resources.number = Bitmap.FromFile(resPath + "number.png")as Bitmap;
			Resources.particle1 = Bitmap.FromFile(resPath + "particle1.png")as Bitmap;
			Resources.particle2 = Bitmap.FromFile(resPath + "particle2.png")as Bitmap;
			Resources.particle3 = Bitmap.FromFile(resPath + "particle3.png")as Bitmap;
			Resources.particle4 = Bitmap.FromFile(resPath + "particle4.png")as Bitmap;
			Resources.pDead = Bitmap.FromFile(resPath + "pdead.png")as Bitmap;
			Resources.pFly = Bitmap.FromFile(resPath + "pfly.png")as Bitmap;
			Resources.pNormal = Bitmap.FromFile(resPath + "pnormal.png")as Bitmap;
			Resources.SDX = Bitmap.FromFile(resPath + "sdx.png")as Bitmap;
			Resources.slime0 = Bitmap.FromFile(resPath + "mob0.png")as Bitmap;
			Resources.slime1 = Bitmap.FromFile(resPath + "mob1.png")as Bitmap;
			Resources.slime2 = Bitmap.FromFile(resPath + "mob2.png")as Bitmap;
			Resources.Sound = Bitmap.FromFile(resPath + "sound.png")as Bitmap;
			Resources.stone = Bitmap.FromFile(resPath + "stone.png")as Bitmap;
			Resources.title = Bitmap.FromFile(resPath + "title.png")as Bitmap;
			Resources.tube_lower = Bitmap.FromFile(resPath + "tube_lower.png")as Bitmap;
			Resources.tube_upper = Bitmap.FromFile(resPath + "tube_upper.png")as Bitmap;
			Resources.yuanshi = Bitmap.FromFile(resPath + "bonus.png")as Bitmap;
			Resources.yuanshi_smaller = Bitmap.FromFile(resPath + "bonus_smaller.png")as Bitmap;
			Resources.bgm = System.IO.File.ReadAllBytes(resPath + "bgm.mp3");
			Resources.hit = System.IO.File.ReadAllBytes(resPath + "hit.mp3");
			Resources.pass = System.IO.File.ReadAllBytes(resPath + "pass.mp3");
			Resources.press = System.IO.File.ReadAllBytes(resPath + "press.mp3");
		}

		private void LoadControl_Click(object sender, EventArgs e)
		{	
			if (IsFailed) System.Environment.Exit(0);

		}
	}
}

namespace FlappyPaimon.Properties
{
	public static partial class Resources
	{
		public static Bitmap bainianji,cloud,DisableSound,distance,forest,FPS,Fullscreen,GDI,ground,number,particle1,particle2,particle3,particle4,pDead,pFly,pNormal,SDX,slime0,slime1,slime2,Sound,stone,
		title,tube_lower,tube_upper,yuanshi,yuanshi_smaller;
		public static byte[] bgm, hit, pass, press;
	}
}