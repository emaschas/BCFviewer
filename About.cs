using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

namespace BCFviewer {

  /// <Summary> Main class of the "About" form </Summary>
  public partial class AboutForm : Form {
    /// <Summary> "About" Form constructor </Summary>
    public AboutForm() {
      InitializeComponent();
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.FixedDialog;
      MinimizeBox = false;
      MaximizeBox = false;
      Padding = new Padding(20, 40, 20, 40);
      SplitContainer vsp = new SplitContainer();
      vsp.Dock = DockStyle.Fill;
      vsp.Orientation = Orientation.Horizontal;
      vsp.SplitterDistance = 32;
      vsp.SplitterWidth = 40;
      vsp.IsSplitterFixed = true;
      vsp.TabStop = false;
      PictureBox img = new PictureBox();
      //img.Bounds = new Rectangle(0,0,48,48);
      img.Dock = DockStyle.Fill;
      img.TabStop = false;
      img.SizeMode = PictureBoxSizeMode.Zoom;
      img.BorderStyle = BorderStyle.None;
      img.Image = new Bitmap("icons\\BCFicon.ico");
      RichTextBox txt = new RichTextBox();
      txt.Dock = DockStyle.Fill;
      txt.BorderStyle = BorderStyle.None;
      txt.ReadOnly = true;
      txt.Text = "BCF File Viewer Version 1.1.0\n\n" +
                 "By Emmanuel Maschas\n" +
                 "November 2020\n\n" +
                 "Report issues at https://github.com/emaschas/BCFviewer/issues";
      txt.TabStop = false;
      txt.LinkClicked += new LinkClickedEventHandler(report);
      vsp.Panel1.Controls.Add(img);
      vsp.Panel2.Controls.Add(txt);
      Controls.Add(vsp);
    }
    private void report(object sender, LinkClickedEventArgs args) {
      var psi = new System.Diagnostics.ProcessStartInfo {
        FileName = args.LinkText,
        UseShellExecute = true
      };
      System.Diagnostics.Process.Start(psi);
    }
  }
}