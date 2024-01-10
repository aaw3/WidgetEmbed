namespace WidgetEmbed
{
    partial class Form1
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.SaveEmbedsCheckbox = new System.Windows.Forms.CheckBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.BoundsDetectionCheckbox = new System.Windows.Forms.CheckBox();
            this.CheckBoundsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox1.Location = new System.Drawing.Point(197, 112);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(362, 143);
            this.textBox1.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(591, 166);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 44);
            this.button1.TabIndex = 2;
            this.button1.Text = "Create Embed";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(565, 112);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(129, 21);
            this.comboBox1.TabIndex = 3;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // SaveEmbedsCheckbox
            // 
            this.SaveEmbedsCheckbox.AutoSize = true;
            this.SaveEmbedsCheckbox.Location = new System.Drawing.Point(1, 23);
            this.SaveEmbedsCheckbox.Name = "SaveEmbedsCheckbox";
            this.SaveEmbedsCheckbox.Size = new System.Drawing.Size(138, 17);
            this.SaveEmbedsCheckbox.TabIndex = 4;
            this.SaveEmbedsCheckbox.Text = "Save Embeds On Close";
            this.SaveEmbedsCheckbox.UseVisualStyleBackColor = true;
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(708, 1);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(91, 26);
            this.SaveButton.TabIndex = 5;
            this.SaveButton.Text = "Save Now";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // BoundsDetectionCheckbox
            // 
            this.BoundsDetectionCheckbox.AutoSize = true;
            this.BoundsDetectionCheckbox.Location = new System.Drawing.Point(1, 1);
            this.BoundsDetectionCheckbox.Name = "BoundsDetectionCheckbox";
            this.BoundsDetectionCheckbox.Size = new System.Drawing.Size(138, 17);
            this.BoundsDetectionCheckbox.TabIndex = 6;
            this.BoundsDetectionCheckbox.Text = "Check Bounds On Start";
            this.BoundsDetectionCheckbox.UseVisualStyleBackColor = true;
            // 
            // CheckBoundsButton
            // 
            this.CheckBoundsButton.Location = new System.Drawing.Point(708, 33);
            this.CheckBoundsButton.Name = "CheckBoundsButton";
            this.CheckBoundsButton.Size = new System.Drawing.Size(91, 26);
            this.CheckBoundsButton.TabIndex = 7;
            this.CheckBoundsButton.Text = "Check Bounds";
            this.CheckBoundsButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.CheckBoundsButton);
            this.Controls.Add(this.BoundsDetectionCheckbox);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.SaveEmbedsCheckbox);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.Text = "WidgetEmbed";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckBox SaveEmbedsCheckbox;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.CheckBox BoundsDetectionCheckbox;
        private System.Windows.Forms.Button CheckBoundsButton;
    }
}

