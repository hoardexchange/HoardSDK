namespace HoardTests.HW
{
    partial class PINWindow
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
            this.pinBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pinBox
            // 
            this.pinBox.Location = new System.Drawing.Point(15, 25);
            this.pinBox.Name = "pinBox";
            this.pinBox.Size = new System.Drawing.Size(205, 20);
            this.pinBox.TabIndex = 3;
            this.pinBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PinEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Enter PIN based on hardwarewallet values";
            // 
            // PINWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(228, 57);
            this.ControlBox = false;
            this.Controls.Add(this.pinBox);
            this.Controls.Add(this.label1);
            this.Name = "PINWindow";
            this.ShowInTaskbar = false;
            this.Text = "PINWindow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox pinBox;
        private System.Windows.Forms.Label label1;
    }
}