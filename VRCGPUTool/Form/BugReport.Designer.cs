namespace VRCGPUTool.Form
{
    partial class BugReport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BugReport));
            this.body = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bug = new System.Windows.Forms.RadioButton();
            this.func = new System.Windows.Forms.RadioButton();
            this.submit = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.emailinput = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.fileadd = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.fileCount = new System.Windows.Forms.Label();
            this.RepoTypeGroup = new System.Windows.Forms.GroupBox();
            this.RepoTypeGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // body
            // 
            this.body.Location = new System.Drawing.Point(21, 77);
            this.body.Multiline = true;
            this.body.Name = "body";
            this.body.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.body.Size = new System.Drawing.Size(429, 165);
            this.body.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "内容を入力してください";
            // 
            // bug
            // 
            this.bug.AutoSize = true;
            this.bug.Checked = true;
            this.bug.Location = new System.Drawing.Point(25, 19);
            this.bug.Name = "bug";
            this.bug.Size = new System.Drawing.Size(68, 17);
            this.bug.TabIndex = 2;
            this.bug.TabStop = true;
            this.bug.Text = "バグ報告";
            this.bug.UseVisualStyleBackColor = true;
            // 
            // func
            // 
            this.func.AutoSize = true;
            this.func.Location = new System.Drawing.Point(111, 19);
            this.func.Name = "func";
            this.func.Size = new System.Drawing.Size(73, 17);
            this.func.TabIndex = 3;
            this.func.Text = "機能要望";
            this.func.UseVisualStyleBackColor = true;
            // 
            // submit
            // 
            this.submit.Location = new System.Drawing.Point(366, 348);
            this.submit.Name = "submit";
            this.submit.Size = new System.Drawing.Size(75, 24);
            this.submit.TabIndex = 4;
            this.submit.Text = "送信";
            this.submit.UseVisualStyleBackColor = true;
            this.submit.Click += new System.EventHandler(this.Submit);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 255);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Email(オプション）";
            // 
            // emailinput
            // 
            this.emailinput.Location = new System.Drawing.Point(21, 271);
            this.emailinput.Name = "emailinput";
            this.emailinput.Size = new System.Drawing.Size(239, 20);
            this.emailinput.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 294);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "※入力すると開発者からの返信が届きます";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "画像ファイル(*.png, *.jpg)|*.png;*.jpg";
            // 
            // fileadd
            // 
            this.fileadd.Location = new System.Drawing.Point(21, 319);
            this.fileadd.Name = "fileadd";
            this.fileadd.Size = new System.Drawing.Size(139, 23);
            this.fileadd.TabIndex = 8;
            this.fileadd.Text = "画像を添付(オプション）";
            this.fileadd.UseVisualStyleBackColor = true;
            this.fileadd.Click += new System.EventHandler(this.fileadd_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 345);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "※ファイルは4MBまでです";
            // 
            // fileCount
            // 
            this.fileCount.AutoSize = true;
            this.fileCount.Location = new System.Drawing.Point(166, 324);
            this.fileCount.Name = "fileCount";
            this.fileCount.Size = new System.Drawing.Size(98, 13);
            this.fileCount.TabIndex = 10;
            this.fileCount.Text = "選択されていません";
            this.fileCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RepoTypeGroup
            // 
            this.RepoTypeGroup.Controls.Add(this.bug);
            this.RepoTypeGroup.Controls.Add(this.func);
            this.RepoTypeGroup.Location = new System.Drawing.Point(21, 12);
            this.RepoTypeGroup.Name = "RepoTypeGroup";
            this.RepoTypeGroup.Size = new System.Drawing.Size(251, 45);
            this.RepoTypeGroup.TabIndex = 11;
            this.RepoTypeGroup.TabStop = false;
            this.RepoTypeGroup.Text = "報告種別";
            // 
            // BugReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 384);
            this.Controls.Add(this.RepoTypeGroup);
            this.Controls.Add(this.fileCount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.fileadd);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.emailinput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.submit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.body);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BugReport";
            this.Text = "問題報告フォーム";
            this.RepoTypeGroup.ResumeLayout(false);
            this.RepoTypeGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox body;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton bug;
        private System.Windows.Forms.RadioButton func;
        private System.Windows.Forms.Button submit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox emailinput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button fileadd;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label fileCount;
        private System.Windows.Forms.GroupBox RepoTypeGroup;
    }
}