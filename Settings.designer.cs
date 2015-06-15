namespace LiveSplit.SplitsBet
{
    partial class Settings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtMinBetTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.chkCancelBets = new System.Windows.Forms.CheckBox();
            this.txtCancelingPenalty = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numScores = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbTimingMethod = new System.Windows.Forms.ComboBox();
            this.chkAllowMods = new System.Windows.Forms.CheckBox();
            this.chkGlobalTime = new System.Windows.Forms.CheckBox();
            this.chkSingleLineScores = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbTimeToShow = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDelay = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScores)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 199F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.txtMinBetTime, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.numScores, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.cmbTimingMethod, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.chkAllowMods, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.chkGlobalTime, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.chkSingleLineScores, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.cmbTimeToShow, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.txtDelay, 1, 8);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 7);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 9;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(462, 312);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // txtMinBetTime
            // 
            this.txtMinBetTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtMinBetTime.Location = new System.Drawing.Point(202, 83);
            this.txtMinBetTime.Name = "txtMinBetTime";
            this.txtMinBetTime.Size = new System.Drawing.Size(120, 20);
            this.txtMinBetTime.TabIndex = 1;
            this.txtMinBetTime.Text = "00:00:01";
            this.txtMinBetTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtMinBetTime.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMinBetTime_KeyPress);
            this.txtMinBetTime.Leave += new System.EventHandler(this.txtMinBetTime_Leave);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(193, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Minimal Time Players Can Bet:";
            // 
            // groupBox1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 2);
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(456, 73);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Canceling Bets";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.88889F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.11111F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.chkCancelBets, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtCancelingPenalty, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(450, 54);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Points Penalty for Canceling Bet:";
            // 
            // chkCancelBets
            // 
            this.chkCancelBets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCancelBets.AutoSize = true;
            this.chkCancelBets.Location = new System.Drawing.Point(7, 31);
            this.chkCancelBets.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.chkCancelBets.Name = "chkCancelBets";
            this.chkCancelBets.Size = new System.Drawing.Size(183, 17);
            this.chkCancelBets.TabIndex = 1;
            this.chkCancelBets.Text = "Allow Players to Cancel Bets";
            this.chkCancelBets.UseVisualStyleBackColor = true;
            this.chkCancelBets.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // txtCancelingPenalty
            // 
            this.txtCancelingPenalty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtCancelingPenalty.Location = new System.Drawing.Point(196, 3);
            this.txtCancelingPenalty.Name = "txtCancelingPenalty";
            this.txtCancelingPenalty.Size = new System.Drawing.Size(120, 20);
            this.txtCancelingPenalty.TabIndex = 0;
            this.txtCancelingPenalty.Text = "50";
            this.txtCancelingPenalty.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtCancelingPenalty.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCancelingPenalty_KeyPress);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(193, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Number of Scores to Show:";
            // 
            // numScores
            // 
            this.numScores.Location = new System.Drawing.Point(202, 111);
            this.numScores.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numScores.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numScores.Name = "numScores";
            this.numScores.Size = new System.Drawing.Size(120, 20);
            this.numScores.TabIndex = 2;
            this.numScores.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 232);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(193, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Timing Method:";
            // 
            // cmbTimingMethod
            // 
            this.cmbTimingMethod.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbTimingMethod.FormattingEnabled = true;
            this.cmbTimingMethod.Items.AddRange(new object[] {
            "Current Timing Method",
            "Real Time",
            "Game Time"});
            this.cmbTimingMethod.Location = new System.Drawing.Point(202, 228);
            this.cmbTimingMethod.Name = "cmbTimingMethod";
            this.cmbTimingMethod.Size = new System.Drawing.Size(120, 21);
            this.cmbTimingMethod.TabIndex = 6;
            // 
            // chkAllowMods
            // 
            this.chkAllowMods.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAllowMods.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkAllowMods, 2);
            this.chkAllowMods.Location = new System.Drawing.Point(7, 201);
            this.chkAllowMods.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.chkAllowMods.Name = "chkAllowMods";
            this.chkAllowMods.Size = new System.Drawing.Size(452, 17);
            this.chkAllowMods.TabIndex = 5;
            this.chkAllowMods.Text = "Allow Mods to Start and Stop SplitsBet";
            this.chkAllowMods.UseVisualStyleBackColor = true;
            // 
            // chkGlobalTime
            // 
            this.chkGlobalTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkGlobalTime.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkGlobalTime, 2);
            this.chkGlobalTime.Location = new System.Drawing.Point(7, 172);
            this.chkGlobalTime.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.chkGlobalTime.Name = "chkGlobalTime";
            this.chkGlobalTime.Size = new System.Drawing.Size(452, 17);
            this.chkGlobalTime.TabIndex = 4;
            this.chkGlobalTime.Text = "Use Global Time Instead of Segment Time";
            this.chkGlobalTime.UseVisualStyleBackColor = true;
            // 
            // chkSingleLineScores
            // 
            this.chkSingleLineScores.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.chkSingleLineScores.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkSingleLineScores, 2);
            this.chkSingleLineScores.Location = new System.Drawing.Point(7, 143);
            this.chkSingleLineScores.Margin = new System.Windows.Forms.Padding(7, 3, 3, 3);
            this.chkSingleLineScores.Name = "chkSingleLineScores";
            this.chkSingleLineScores.Size = new System.Drawing.Size(452, 17);
            this.chkSingleLineScores.TabIndex = 3;
            this.chkSingleLineScores.Text = "Scores Show on a Single Message Instead of Multiple Lines";
            this.chkSingleLineScores.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 254);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(193, 26);
            this.label5.TabIndex = 9;
            this.label5.Text = "Comparison to show at the beginning of a split:";
            // 
            // cmbTimeToShow
            // 
            this.cmbTimeToShow.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.cmbTimeToShow.FormattingEnabled = true;
            this.cmbTimeToShow.Location = new System.Drawing.Point(202, 257);
            this.cmbTimeToShow.Name = "cmbTimeToShow";
            this.cmbTimeToShow.Size = new System.Drawing.Size(120, 21);
            this.cmbTimeToShow.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 290);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(193, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Delay (in seconds)";
            // 
            // txtDelay
            // 
            this.txtDelay.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtDelay.Location = new System.Drawing.Point(202, 287);
            this.txtDelay.Name = "txtDelay";
            this.txtDelay.Size = new System.Drawing.Size(120, 20);
            this.txtDelay.TabIndex = 12;
            this.txtDelay.Text = "0";
            this.txtDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtDelay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDelay_KeyPress);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Settings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(476, 326);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScores)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chkCancelBets;
        private System.Windows.Forms.TextBox txtCancelingPenalty;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbTimingMethod;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numScores;
        private System.Windows.Forms.CheckBox chkGlobalTime;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox txtMinBetTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkAllowMods;
        private System.Windows.Forms.CheckBox chkSingleLineScores;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbTimeToShow;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDelay;
    }
}
