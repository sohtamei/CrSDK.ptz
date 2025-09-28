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
            this.connect = new System.Windows.Forms.Button();
            this.disconnect = new System.Windows.Forms.Button();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.txtData = new System.Windows.Forms.TextBox();
            this.setDP = new System.Windows.Forms.Button();
            this.panTilt = new System.Windows.Forms.Button();
            this.txtType = new System.Windows.Forms.TextBox();
            this.txtConnect = new System.Windows.Forms.TextBox();
            this.updateLiveview = new System.Windows.Forms.Button();
            this.liveview = new System.Windows.Forms.PictureBox();
            this.txtPreset = new System.Windows.Forms.TextBox();
            this.setPreset = new System.Windows.Forms.Button();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.command = new System.Windows.Forms.Button();
            this.getDP = new System.Windows.Forms.Button();
            this.incDP = new System.Windows.Forms.Button();
            this.decDP = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.liveview)).BeginInit();
            this.SuspendLayout();
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(59, 34);
            this.connect.Margin = new System.Windows.Forms.Padding(4);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(100, 29);
            this.connect.TabIndex = 0;
            this.connect.Text = "connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // disconnect
            // 
            this.disconnect.Enabled = false;
            this.disconnect.Location = new System.Drawing.Point(63, 65);
            this.disconnect.Margin = new System.Windows.Forms.Padding(4);
            this.disconnect.Name = "disconnect";
            this.disconnect.Size = new System.Drawing.Size(100, 29);
            this.disconnect.TabIndex = 1;
            this.disconnect.Text = "disconnect";
            this.disconnect.UseVisualStyleBackColor = true;
            this.disconnect.Click += new System.EventHandler(this.disconnect_Click);
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(63, 140);
            this.txtCode.Margin = new System.Windows.Forms.Padding(4);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(195, 22);
            this.txtCode.TabIndex = 2;
            this.txtCode.Text = "ShutterSpeed";
            // 
            // txtData
            // 
            this.txtData.Location = new System.Drawing.Point(292, 142);
            this.txtData.Margin = new System.Windows.Forms.Padding(4);
            this.txtData.Name = "txtData";
            this.txtData.Size = new System.Drawing.Size(71, 22);
            this.txtData.TabIndex = 3;
            this.txtData.Text = "30000";
            // 
            // setDP
            // 
            this.setDP.Location = new System.Drawing.Point(368, 140);
            this.setDP.Margin = new System.Windows.Forms.Padding(4);
            this.setDP.Name = "setDP";
            this.setDP.Size = new System.Drawing.Size(58, 24);
            this.setDP.TabIndex = 4;
            this.setDP.Text = "setDP";
            this.setDP.UseVisualStyleBackColor = true;
            this.setDP.Click += new System.EventHandler(this.setDP_Click);
            // 
            // panTilt
            // 
            this.panTilt.Location = new System.Drawing.Point(392, 96);
            this.panTilt.Margin = new System.Windows.Forms.Padding(4);
            this.panTilt.Name = "panTilt";
            this.panTilt.Size = new System.Drawing.Size(88, 29);
            this.panTilt.TabIndex = 7;
            this.panTilt.Text = "panTilt";
            this.panTilt.UseVisualStyleBackColor = true;
            this.panTilt.Click += new System.EventHandler(this.panTilt_Click);
            // 
            // txtType
            // 
            this.txtType.Location = new System.Drawing.Point(63, 101);
            this.txtType.Margin = new System.Windows.Forms.Padding(4);
            this.txtType.Name = "txtType";
            this.txtType.Size = new System.Drawing.Size(275, 22);
            this.txtType.TabIndex = 8;
            this.txtType.Text = "1 100000000 0 50 50";
            // 
            // txtConnect
            // 
            this.txtConnect.Location = new System.Drawing.Point(168, 34);
            this.txtConnect.Margin = new System.Windows.Forms.Padding(4);
            this.txtConnect.Name = "txtConnect";
            this.txtConnect.Size = new System.Drawing.Size(271, 22);
            this.txtConnect.TabIndex = 9;
            this.txtConnect.Text = "192.168.1.49 admin aaaa1111";
            // 
            // updateLiveview
            // 
            this.updateLiveview.Location = new System.Drawing.Point(288, 285);
            this.updateLiveview.Margin = new System.Windows.Forms.Padding(4);
            this.updateLiveview.Name = "updateLiveview";
            this.updateLiveview.Size = new System.Drawing.Size(117, 24);
            this.updateLiveview.TabIndex = 10;
            this.updateLiveview.Text = "updateLiveview";
            this.updateLiveview.UseVisualStyleBackColor = true;
            this.updateLiveview.Visible = false;
            this.updateLiveview.Click += new System.EventHandler(this.updateLiveview_Click);
            // 
            // liveview
            // 
            this.liveview.Location = new System.Drawing.Point(487, 15);
            this.liveview.Margin = new System.Windows.Forms.Padding(4);
            this.liveview.Name = "liveview";
            this.liveview.Size = new System.Drawing.Size(1136, 755);
            this.liveview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.liveview.TabIndex = 11;
            this.liveview.TabStop = false;
            // 
            // txtPreset
            // 
            this.txtPreset.Location = new System.Drawing.Point(288, 239);
            this.txtPreset.Margin = new System.Windows.Forms.Padding(4);
            this.txtPreset.Name = "txtPreset";
            this.txtPreset.Size = new System.Drawing.Size(71, 22);
            this.txtPreset.TabIndex = 12;
            this.txtPreset.Text = "1";
            // 
            // setPreset
            // 
            this.setPreset.Location = new System.Drawing.Point(388, 239);
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
            this.txtCommand.Location = new System.Drawing.Point(59, 199);
            this.txtCommand.Margin = new System.Windows.Forms.Padding(4);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(300, 22);
            this.txtCommand.TabIndex = 14;
            this.txtCommand.Text = "RemoteKeyDown 1 0";
            // 
            // command
            // 
            this.command.Location = new System.Drawing.Point(377, 199);
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
            this.getDP.Location = new System.Drawing.Point(423, 140);
            this.getDP.Margin = new System.Windows.Forms.Padding(4);
            this.getDP.Name = "getDP";
            this.getDP.Size = new System.Drawing.Size(58, 24);
            this.getDP.TabIndex = 16;
            this.getDP.Text = "getDP";
            this.getDP.UseVisualStyleBackColor = true;
            this.getDP.Click += new System.EventHandler(this.getDP_Click);
            // 
            // incDP
            // 
            this.incDP.Location = new System.Drawing.Point(368, 167);
            this.incDP.Margin = new System.Windows.Forms.Padding(4);
            this.incDP.Name = "incDP";
            this.incDP.Size = new System.Drawing.Size(58, 24);
            this.incDP.TabIndex = 17;
            this.incDP.Text = "incDP";
            this.incDP.UseVisualStyleBackColor = true;
            this.incDP.Click += new System.EventHandler(this.incDP_Click);
            // 
            // decDP
            // 
            this.decDP.Location = new System.Drawing.Point(423, 167);
            this.decDP.Margin = new System.Windows.Forms.Padding(4);
            this.decDP.Name = "decDP";
            this.decDP.Size = new System.Drawing.Size(58, 24);
            this.decDP.TabIndex = 18;
            this.decDP.Text = "decDP";
            this.decDP.UseVisualStyleBackColor = true;
            this.decDP.Click += new System.EventHandler(this.decDP_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1644, 785);
            this.Controls.Add(this.decDP);
            this.Controls.Add(this.incDP);
            this.Controls.Add(this.getDP);
            this.Controls.Add(this.command);
            this.Controls.Add(this.txtCommand);
            this.Controls.Add(this.setPreset);
            this.Controls.Add(this.txtPreset);
            this.Controls.Add(this.liveview);
            this.Controls.Add(this.updateLiveview);
            this.Controls.Add(this.txtConnect);
            this.Controls.Add(this.txtType);
            this.Controls.Add(this.panTilt);
            this.Controls.Add(this.setDP);
            this.Controls.Add(this.txtData);
            this.Controls.Add(this.txtCode);
            this.Controls.Add(this.disconnect);
            this.Controls.Add(this.connect);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.liveview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.Button disconnect;
        private System.Windows.Forms.TextBox txtCode;
        private System.Windows.Forms.TextBox txtData;
        private System.Windows.Forms.Button setDP;
        private System.Windows.Forms.Button panTilt;
        private System.Windows.Forms.TextBox txtType;
        private System.Windows.Forms.TextBox txtConnect;
        private System.Windows.Forms.Button updateLiveview;
        private System.Windows.Forms.PictureBox liveview;
        private System.Windows.Forms.TextBox txtPreset;
        private System.Windows.Forms.Button setPreset;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.Button command;
        private System.Windows.Forms.Button getDP;
        private System.Windows.Forms.Button incDP;
        private System.Windows.Forms.Button decDP;
    }
}

