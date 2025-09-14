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
            ((System.ComponentModel.ISupportInitialize)(this.liveview)).BeginInit();
            this.SuspendLayout();
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(44, 27);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(75, 23);
            this.connect.TabIndex = 0;
            this.connect.Text = "connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // disconnect
            // 
            this.disconnect.Enabled = false;
            this.disconnect.Location = new System.Drawing.Point(47, 52);
            this.disconnect.Name = "disconnect";
            this.disconnect.Size = new System.Drawing.Size(75, 23);
            this.disconnect.TabIndex = 1;
            this.disconnect.Text = "disconnect";
            this.disconnect.UseVisualStyleBackColor = true;
            this.disconnect.Click += new System.EventHandler(this.disconnect_Click);
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(47, 112);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(147, 19);
            this.txtCode.TabIndex = 2;
            this.txtCode.Text = "ZoomOperationWithInt16";
            // 
            // txtData
            // 
            this.txtData.Location = new System.Drawing.Point(219, 114);
            this.txtData.Name = "txtData";
            this.txtData.Size = new System.Drawing.Size(54, 19);
            this.txtData.TabIndex = 3;
            this.txtData.Text = "30000";
            // 
            // setDP
            // 
            this.setDP.Location = new System.Drawing.Point(286, 111);
            this.setDP.Name = "setDP";
            this.setDP.Size = new System.Drawing.Size(75, 23);
            this.setDP.TabIndex = 4;
            this.setDP.Text = "setDP";
            this.setDP.UseVisualStyleBackColor = true;
            this.setDP.Click += new System.EventHandler(this.setDP_Click);
            // 
            // panTilt
            // 
            this.panTilt.Location = new System.Drawing.Point(294, 77);
            this.panTilt.Name = "panTilt";
            this.panTilt.Size = new System.Drawing.Size(66, 23);
            this.panTilt.TabIndex = 7;
            this.panTilt.Text = "panTilt";
            this.panTilt.UseVisualStyleBackColor = true;
            this.panTilt.Click += new System.EventHandler(this.panTilt_Click);
            // 
            // txtType
            // 
            this.txtType.Location = new System.Drawing.Point(47, 81);
            this.txtType.Name = "txtType";
            this.txtType.Size = new System.Drawing.Size(207, 19);
            this.txtType.TabIndex = 8;
            this.txtType.Text = "1 100000000 0 50 50";
            // 
            // txtConnect
            // 
            this.txtConnect.Location = new System.Drawing.Point(126, 27);
            this.txtConnect.Name = "txtConnect";
            this.txtConnect.Size = new System.Drawing.Size(204, 19);
            this.txtConnect.TabIndex = 9;
            this.txtConnect.Text = "192.168.1.12 admin test0000";
            // 
            // updateLiveview
            // 
            this.updateLiveview.Location = new System.Drawing.Point(219, 206);
            this.updateLiveview.Name = "updateLiveview";
            this.updateLiveview.Size = new System.Drawing.Size(88, 19);
            this.updateLiveview.TabIndex = 10;
            this.updateLiveview.Text = "updateLiveview";
            this.updateLiveview.UseVisualStyleBackColor = true;
            this.updateLiveview.Visible = false;
            this.updateLiveview.Click += new System.EventHandler(this.updateLiveview_Click);
            // 
            // liveview
            // 
            this.liveview.Location = new System.Drawing.Point(365, 12);
            this.liveview.Name = "liveview";
            this.liveview.Size = new System.Drawing.Size(852, 604);
            this.liveview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.liveview.TabIndex = 11;
            this.liveview.TabStop = false;
            // 
            // txtPreset
            // 
            this.txtPreset.Location = new System.Drawing.Point(219, 170);
            this.txtPreset.Name = "txtPreset";
            this.txtPreset.Size = new System.Drawing.Size(54, 19);
            this.txtPreset.TabIndex = 12;
            this.txtPreset.Text = "1";
            // 
            // setPreset
            // 
            this.setPreset.Location = new System.Drawing.Point(294, 170);
            this.setPreset.Name = "setPreset";
            this.setPreset.Size = new System.Drawing.Size(67, 19);
            this.setPreset.TabIndex = 13;
            this.setPreset.Text = "set preset";
            this.setPreset.UseVisualStyleBackColor = true;
            this.setPreset.Click += new System.EventHandler(this.setPreset_Click);
            // 
            // txtCommand
            // 
            this.txtCommand.Location = new System.Drawing.Point(47, 138);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(226, 19);
            this.txtCommand.TabIndex = 14;
            this.txtCommand.Text = "RemoteKeyDown 1 0";
            // 
            // command
            // 
            this.command.Location = new System.Drawing.Point(286, 138);
            this.command.Name = "command";
            this.command.Size = new System.Drawing.Size(75, 23);
            this.command.TabIndex = 15;
            this.command.Text = "Command";
            this.command.UseVisualStyleBackColor = true;
            this.command.Click += new System.EventHandler(this.command_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1233, 628);
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
    }
}

