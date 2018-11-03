using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArchiveExplorer
{
  public partial class KeyRequestForm : Form
  {
    public string Passcode
    {
      get
      {
        if (checkBox1.Checked)
        {
          StringBuilder sb = new StringBuilder();
          var key = passcodeTextBox.Text.Replace(" ", "");
          for (var x = 0; x < key.Length; x += 2)
          {
            sb.Append((char)Convert.ToByte(key.Substring(x, 2), 16));
          }
          return sb.ToString();
        }
        return passcodeTextBox.Text;
      }
    }
    public KeyRequestForm(string request = "")
    {
      InitializeComponent();
      label1.Text += request;
    }
  }
}
