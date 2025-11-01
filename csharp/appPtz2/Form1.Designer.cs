namespace appPtz2
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.connect0 = new System.Windows.Forms.Button();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.txtData = new System.Windows.Forms.TextBox();
            this.setDP = new System.Windows.Forms.Button();
            this.panTilt = new System.Windows.Forms.Button();
            this.txtType = new System.Windows.Forms.TextBox();
            this.txtConnect0 = new System.Windows.Forms.TextBox();
            this.liveview = new System.Windows.Forms.PictureBox();
            this.txtPreset = new System.Windows.Forms.TextBox();
            this.setPreset = new System.Windows.Forms.Button();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.command = new System.Windows.Forms.Button();
            this.getDP = new System.Windows.Forms.Button();
            this.incDP = new System.Windows.Forms.Button();
            this.decDP = new System.Windows.Forms.Button();
            this.txtSpeedMax = new System.Windows.Forms.TextBox();
            this.checkLiveview = new System.Windows.Forms.CheckBox();
            this.txtBlindZone = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtConnect1 = new System.Windows.Forms.TextBox();
            this.connect1 = new System.Windows.Forms.Button();
            this.txtComport = new System.Windows.Forms.TextBox();
            this.labelDP = new System.Windows.Forms.Label();
            this.buttonSelect0 = new System.Windows.Forms.Button();
            this.buttonSelect1 = new System.Windows.Forms.Button();
            this.txtConnect2 = new System.Windows.Forms.TextBox();
            this.connect2 = new System.Windows.Forms.Button();
            this.buttonSelect2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.liveview)).BeginInit();
            this.SuspendLayout();
            // 
            // connect0
            // 
            this.connect0.Location = new System.Drawing.Point(243, 10);
            this.connect0.Margin = new System.Windows.Forms.Padding(4);
            this.connect0.Name = "connect0";
            this.connect0.Size = new System.Drawing.Size(72, 30);
            this.connect0.TabIndex = 0;
            this.connect0.Text = "connect";
            this.connect0.UseVisualStyleBackColor = true;
            this.connect0.Click += new System.EventHandler(this.connect0_Click);
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(13, 191);
            this.txtCode.Margin = new System.Windows.Forms.Padding(4);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(167, 22);
            this.txtCode.TabIndex = 2;
            this.txtCode.Text = "ShutterSpeed";
            // 
            // txtData
            // 
            this.txtData.Location = new System.Drawing.Point(188, 191);
            this.txtData.Margin = new System.Windows.Forms.Padding(4);
            this.txtData.Name = "txtData";
            this.txtData.Size = new System.Drawing.Size(71, 22);
            this.txtData.TabIndex = 3;
            this.txtData.Text = "30000";
            // 
            // setDP
            // 
            this.setDP.Location = new System.Drawing.Point(264, 189);
            this.setDP.Margin = new System.Windows.Forms.Padding(4);
            this.setDP.Name = "setDP";
            this.setDP.Size = new System.Drawing.Size(59, 24);
            this.setDP.TabIndex = 4;
            this.setDP.Text = "setDP";
            this.setDP.UseVisualStyleBackColor = true;
            this.setDP.Click += new System.EventHandler(this.setDP_Click);
            // 
            // panTilt
            // 
            this.panTilt.Location = new System.Drawing.Point(307, 152);
            this.panTilt.Margin = new System.Windows.Forms.Padding(4);
            this.panTilt.Name = "panTilt";
            this.panTilt.Size = new System.Drawing.Size(61, 22);
            this.panTilt.TabIndex = 7;
            this.panTilt.Text = "panTilt";
            this.panTilt.UseVisualStyleBackColor = true;
            this.panTilt.Click += new System.EventHandler(this.panTilt_Click);
            // 
            // txtType
            // 
            this.txtType.Location = new System.Drawing.Point(13, 152);
            this.txtType.Margin = new System.Windows.Forms.Padding(4);
            this.txtType.Name = "txtType";
            this.txtType.Size = new System.Drawing.Size(241, 22);
            this.txtType.TabIndex = 8;
            this.txtType.Text = "1 100000000 0 50 50";
            // 
            // txtConnect0
            // 
            this.txtConnect0.Location = new System.Drawing.Point(13, 15);
            this.txtConnect0.Margin = new System.Windows.Forms.Padding(4);
            this.txtConnect0.Name = "txtConnect0";
            this.txtConnect0.Size = new System.Drawing.Size(222, 22);
            this.txtConnect0.TabIndex = 1;
            this.txtConnect0.Text = "192.168.11.2 admin Sony0000";
            // 
            // liveview
            // 
            this.liveview.Location = new System.Drawing.Point(386, 13);
            this.liveview.Margin = new System.Windows.Forms.Padding(4);
            this.liveview.Name = "liveview";
            this.liveview.Size = new System.Drawing.Size(1136, 755);
            this.liveview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.liveview.TabIndex = 11;
            this.liveview.TabStop = false;
            // 
            // txtPreset
            // 
            this.txtPreset.Location = new System.Drawing.Point(188, 290);
            this.txtPreset.Margin = new System.Windows.Forms.Padding(4);
            this.txtPreset.Name = "txtPreset";
            this.txtPreset.Size = new System.Drawing.Size(71, 22);
            this.txtPreset.TabIndex = 12;
            this.txtPreset.Text = "1";
            // 
            // setPreset
            // 
            this.setPreset.Location = new System.Drawing.Point(284, 288);
            this.setPreset.Margin = new System.Windows.Forms.Padding(4);
            this.setPreset.Name = "setPreset";
            this.setPreset.Size = new System.Drawing.Size(89, 24);
            this.setPreset.TabIndex = 13;
            this.setPreset.Text = "set preset";
            this.setPreset.UseVisualStyleBackColor = true;
            this.setPreset.Click += new System.EventHandler(this.setPreset_Click);
            // 
            // txtCommand
            // 
            this.txtCommand.Location = new System.Drawing.Point(9, 250);
            this.txtCommand.Margin = new System.Windows.Forms.Padding(4);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(245, 22);
            this.txtCommand.TabIndex = 14;
            this.txtCommand.Text = "RemoteKeyDown 1 0";
            // 
            // command
            // 
            this.command.Location = new System.Drawing.Point(273, 248);
            this.command.Margin = new System.Windows.Forms.Padding(4);
            this.command.Name = "command";
            this.command.Size = new System.Drawing.Size(100, 29);
            this.command.TabIndex = 15;
            this.command.Text = "Command";
            this.command.UseVisualStyleBackColor = true;
            this.command.Click += new System.EventHandler(this.command_Click);
            // 
            // getDP
            // 
            this.getDP.Location = new System.Drawing.Point(319, 189);
            this.getDP.Margin = new System.Windows.Forms.Padding(4);
            this.getDP.Name = "getDP";
            this.getDP.Size = new System.Drawing.Size(59, 24);
            this.getDP.TabIndex = 16;
            this.getDP.Text = "getDP";
            this.getDP.UseVisualStyleBackColor = true;
            this.getDP.Click += new System.EventHandler(this.getDP_Click);
            // 
            // incDP
            // 
            this.incDP.Location = new System.Drawing.Point(264, 217);
            this.incDP.Margin = new System.Windows.Forms.Padding(4);
            this.incDP.Name = "incDP";
            this.incDP.Size = new System.Drawing.Size(59, 24);
            this.incDP.TabIndex = 17;
            this.incDP.Text = "incDP";
            this.incDP.UseVisualStyleBackColor = true;
            this.incDP.Click += new System.EventHandler(this.incDP_Click);
            // 
            // decDP
            // 
            this.decDP.Location = new System.Drawing.Point(319, 217);
            this.decDP.Margin = new System.Windows.Forms.Padding(4);
            this.decDP.Name = "decDP";
            this.decDP.Size = new System.Drawing.Size(59, 24);
            this.decDP.TabIndex = 18;
            this.decDP.Text = "decDP";
            this.decDP.UseVisualStyleBackColor = true;
            this.decDP.Click += new System.EventHandler(this.decDP_Click);
            // 
            // txtSpeedMax
            // 
            this.txtSpeedMax.Location = new System.Drawing.Point(262, 152);
            this.txtSpeedMax.Margin = new System.Windows.Forms.Padding(4);
            this.txtSpeedMax.Name = "txtSpeedMax";
            this.txtSpeedMax.Size = new System.Drawing.Size(39, 22);
            this.txtSpeedMax.TabIndex = 19;
            this.txtSpeedMax.Text = "127";
            // 
            // checkLiveview
            // 
            this.checkLiveview.AutoSize = true;
            this.checkLiveview.Checked = true;
            this.checkLiveview.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkLiveview.Location = new System.Drawing.Point(264, 320);
            this.checkLiveview.Margin = new System.Windows.Forms.Padding(4);
            this.checkLiveview.Name = "checkLiveview";
            this.checkLiveview.Size = new System.Drawing.Size(85, 19);
            this.checkLiveview.TabIndex = 20;
            this.checkLiveview.Text = "LiveView";
            this.checkLiveview.UseVisualStyleBackColor = true;
            // 
            // txtBlindZone
            // 
            this.txtBlindZone.Location = new System.Drawing.Point(154, 105);
            this.txtBlindZone.Name = "txtBlindZone";
            this.txtBlindZone.Size = new System.Drawing.Size(100, 22);
            this.txtBlindZone.TabIndex = 21;
            this.txtBlindZone.Text = "5000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(261, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 15);
            this.label1.TabIndex = 22;
            this.label1.Text = "joystick不感帯";
            // 
            // txtConnect1
            // 
            this.txtConnect1.Location = new System.Drawing.Point(13, 45);
            this.txtConnect1.Margin = new System.Windows.Forms.Padding(4);
            this.txtConnect1.Name = "txtConnect1";
            this.txtConnect1.Size = new System.Drawing.Size(222, 22);
            this.txtConnect1.TabIndex = 24;
            this.txtConnect1.Text = "192.168.11.5 admin aaaa1111";
            // 
            // connect1
            // 
            this.connect1.Location = new System.Drawing.Point(243, 40);
            this.connect1.Margin = new System.Windows.Forms.Padding(4);
            this.connect1.Name = "connect1";
            this.connect1.Size = new System.Drawing.Size(72, 30);
            this.connect1.TabIndex = 23;
            this.connect1.Text = "connect";
            this.connect1.UseVisualStyleBackColor = true;
            this.connect1.Click += new System.EventHandler(this.connect1_Click);
            // 
            // txtComport
            // 
            this.txtComport.Location = new System.Drawing.Point(13, 105);
            this.txtComport.Name = "txtComport";
            this.txtComport.Size = new System.Drawing.Size(100, 22);
            this.txtComport.TabIndex = 25;
            this.txtComport.Text = "COM";
            // 
            // labelDP
            // 
            this.labelDP.AutoSize = true;
            this.labelDP.Font = new System.Drawing.Font("Courier New", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDP.Location = new System.Drawing.Point(380, 735);
            this.labelDP.Name = "labelDP";
            this.labelDP.Size = new System.Drawing.Size(65, 31);
            this.labelDP.TabIndex = 26;
            this.labelDP.Text = "xxx";
            // 
            // buttonSelect0
            // 
            this.buttonSelect0.Location = new System.Drawing.Point(323, 10);
            this.buttonSelect0.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSelect0.Name = "buttonSelect0";
            this.buttonSelect0.Size = new System.Drawing.Size(61, 30);
            this.buttonSelect0.TabIndex = 27;
            this.buttonSelect0.Text = "select";
            this.buttonSelect0.UseVisualStyleBackColor = true;
            this.buttonSelect0.Click += new System.EventHandler(this.buttonSelect0_Click);
            // 
            // buttonSelect1
            // 
            this.buttonSelect1.Location = new System.Drawing.Point(323, 40);
            this.buttonSelect1.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSelect1.Name = "buttonSelect1";
            this.buttonSelect1.Size = new System.Drawing.Size(61, 30);
            this.buttonSelect1.TabIndex = 28;
            this.buttonSelect1.Text = "select";
            this.buttonSelect1.UseVisualStyleBackColor = true;
            this.buttonSelect1.Click += new System.EventHandler(this.buttonSelect1_Click);
            // 
            // txtConnect2
            // 
            this.txtConnect2.Location = new System.Drawing.Point(13, 75);
            this.txtConnect2.Margin = new System.Windows.Forms.Padding(4);
            this.txtConnect2.Name = "txtConnect2";
            this.txtConnect2.Size = new System.Drawing.Size(222, 22);
            this.txtConnect2.TabIndex = 29;
            this.txtConnect2.Text = "192.168.11.5 admin aaaa1111";
            // 
            // connect2
            // 
            this.connect2.Location = new System.Drawing.Point(243, 70);
            this.connect2.Margin = new System.Windows.Forms.Padding(4);
            this.connect2.Name = "connect2";
            this.connect2.Size = new System.Drawing.Size(72, 30);
            this.connect2.TabIndex = 30;
            this.connect2.Text = "connect";
            this.connect2.UseVisualStyleBackColor = true;
            this.connect2.Click += new System.EventHandler(this.connect2_Click);
            // 
            // buttonSelect2
            // 
            this.buttonSelect2.Location = new System.Drawing.Point(323, 70);
            this.buttonSelect2.Margin = new System.Windows.Forms.Padding(4);
            this.buttonSelect2.Name = "buttonSelect2";
            this.buttonSelect2.Size = new System.Drawing.Size(61, 30);
            this.buttonSelect2.TabIndex = 31;
            this.buttonSelect2.Text = "select";
            this.buttonSelect2.UseVisualStyleBackColor = true;
            this.buttonSelect2.Click += new System.EventHandler(this.buttonSelect2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1644, 785);
            this.Controls.Add(this.buttonSelect2);
            this.Controls.Add(this.connect2);
            this.Controls.Add(this.txtConnect2);
            this.Controls.Add(this.buttonSelect1);
            this.Controls.Add(this.buttonSelect0);
            this.Controls.Add(this.labelDP);
            this.Controls.Add(this.txtComport);
            this.Controls.Add(this.txtConnect1);
            this.Controls.Add(this.connect1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBlindZone);
            this.Controls.Add(this.checkLiveview);
            this.Controls.Add(this.txtSpeedMax);
            this.Controls.Add(this.decDP);
            this.Controls.Add(this.incDP);
            this.Controls.Add(this.getDP);
            this.Controls.Add(this.command);
            this.Controls.Add(this.txtCommand);
            this.Controls.Add(this.setPreset);
            this.Controls.Add(this.txtPreset);
            this.Controls.Add(this.liveview);
            this.Controls.Add(this.txtConnect0);
            this.Controls.Add(this.txtType);
            this.Controls.Add(this.panTilt);
            this.Controls.Add(this.setDP);
            this.Controls.Add(this.txtData);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.connect0);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.liveview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connect0;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.Button setDP;
        private System.Windows.Forms.Button panTilt;
        private System.Windows.Forms.TextBox txtType;
        private System.Windows.Forms.TextBox txtConnect0;
        private System.Windows.Forms.PictureBox liveview;
        private System.Windows.Forms.TextBox txtPreset;
        private System.Windows.Forms.Button setPreset;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.Button command;
        private System.Windows.Forms.Button getDP;
        private System.Windows.Forms.Button incDP;
        private System.Windows.Forms.Button decDP;
        private System.Windows.Forms.TextBox txtSpeedMax;
        private System.Windows.Forms.CheckBox checkLiveview;
        private System.Windows.Forms.TextBox txtBlindZone;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConnect1;
        private System.Windows.Forms.Button connect1;
        private System.Windows.Forms.TextBox txtComport;
        private System.Windows.Forms.Label labelDP;
        private System.Windows.Forms.Button buttonSelect0;
        private System.Windows.Forms.Button buttonSelect1;
        private System.Windows.Forms.TextBox txtConnect2;
        private System.Windows.Forms.Button connect2;
        private System.Windows.Forms.Button buttonSelect2;
    }
}

