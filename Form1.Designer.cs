namespace FlappyPaimon
{
	partial class Form1
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.GameUI = new FlappyPaimon.BufferedPanel();
			this.SuspendLayout();
			// 
			// GameUI
			// 
			this.GameUI.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.GameUI.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GameUI.Location = new System.Drawing.Point(0, 0);
			this.GameUI.Name = "GameUI";
			this.GameUI.Size = new System.Drawing.Size(800, 450);
			this.GameUI.TabIndex = 0;
			this.GameUI.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GameUI_MouseClick);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(97)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.GameUI);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(320, 240);
			this.Name = "Form1";
			this.Text = "Flappy Paimon";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			this.Resize += new System.EventHandler(this.Form1_Resize);
			this.ResumeLayout(false);

		}

		#endregion

		public BufferedPanel GameUI;
	}
}

