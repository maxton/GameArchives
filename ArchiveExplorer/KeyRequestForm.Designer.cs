namespace ArchiveExplorer
{
  partial class KeyRequestForm
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
      this.button1 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.passcodeTextBox = new System.Windows.Forms.TextBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button1.Location = new System.Drawing.Point(148, 83);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "OK";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(150, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "A passcode or key is needed: ";
      // 
      // passcodeTextBox
      // 
      this.passcodeTextBox.Location = new System.Drawing.Point(15, 26);
      this.passcodeTextBox.Name = "passcodeTextBox";
      this.passcodeTextBox.Size = new System.Drawing.Size(344, 20);
      this.passcodeTextBox.TabIndex = 2;
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.Location = new System.Drawing.Point(15, 52);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(107, 17);
      this.checkBox1.TabIndex = 3;
      this.checkBox1.Text = "Decode from hex";
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // KeyRequestForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(371, 118);
      this.ControlBox = false;
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.passcodeTextBox);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button1);
      this.Name = "KeyRequestForm";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Decryption Key";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox passcodeTextBox;
    private System.Windows.Forms.CheckBox checkBox1;
  }
}