using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using BCFclass;
using BCFpanel;

namespace BCFviewer {

  /// <Summary> Main class of the BCFform </Summary>
  public partial class BCFform : Form {

    /// <summary> Required designer variable. </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> Main Panel. </summary>
    private Panel mainpanel;
    /// <summary> Main Panel components. </summary>
    private BCFpanelContent panelcontent;

    /// <summary> BCFform constructor </summary>
    public BCFform() {
      InitializeComponent();
      String exe = Application.ExecutablePath;
      String pat = Path.GetDirectoryName(exe);
      this.Icon = new Icon(pat + "\\icons\\BCFicon.ico");
      this.Padding = new Padding(10, 0, 10, 10);
      this.SetStyle(ControlStyles.StandardClick, true);
      this.SetStyle(ControlStyles.StandardDoubleClick, true);
      // Menu
      MenuStrip strip = new MenuStrip();
      ToolStripMenuItem menu1 = new ToolStripMenuItem("BCF &File");
      menu1.ShowShortcutKeys = true;
      ToolStripMenuItem menu2 = new ToolStripMenuItem("&About", null, new EventHandler(About_Menu));
      menu2.ShortcutKeys = Keys.F1;
      ToolStripMenuItem smenu11 = new ToolStripMenuItem("&Load New BCF file", null, new EventHandler(OpenFile_Menu));
      smenu11.ShortcutKeys = Keys.Control | Keys.O;
      smenu11.ShortcutKeyDisplayString = "Ctrl-O";
      smenu11.ShowShortcutKeys = true;
      ToolStripMenuItem smenu12 = new ToolStripMenuItem("&Append BCF file", null, new EventHandler(AppendFile_Menu));
      smenu12.ShortcutKeys = Keys.Control | Keys.A;
      ToolStripMenuItem smenu13 = new ToolStripMenuItem("&Quit", null, new EventHandler(Quit_Menu));
      menu1.DropDownItems.Add(smenu11);
      menu1.DropDownItems.Add(smenu12);
      menu1.DropDownItems.Add("-");
      menu1.DropDownItems.Add(smenu13);
      ((ToolStripDropDownMenu)(menu1.DropDown)).ShowImageMargin = false;
      ((ToolStripDropDownMenu)(menu1.DropDown)).ShowCheckMargin = true;
      strip.Items.Add(menu1);
      strip.Items.Add(menu2);
      strip.Dock = DockStyle.Top;
      this.MainMenuStrip = strip;
      //-----------------------
      mainpanel = new Panel();
      mainpanel.TabIndex = 0;
      mainpanel.Dock = DockStyle.Fill;
      mainpanel.MinimumSize = new Size(200, 200);
      //-----------------------
      this.Controls.Add(mainpanel);
      this.Controls.Add(strip);
      // Form
      this.MinimumSize = new Size(300, 300);
      // Fill then Panel
      panelcontent = new BCFpanelContent(mainpanel);
    }

    /// <summary> Select a BCF file </summary>
    /// <returns>The file name or an empty string if the user cancels.</returns>
    private string SelectFile() {
      string filePath = "";
      using(OpenFileDialog openFileDialog = new OpenFileDialog()) {
        openFileDialog.Filter = "BCFZIP files (*.bcfzip)|*.bcfzip|BCF files (*.bcf)|*.bcf|All files (*.*)|*.*";
        openFileDialog.FilterIndex = 1;
        openFileDialog.RestoreDirectory = true;
        if(openFileDialog.ShowDialog() == DialogResult.OK) filePath = openFileDialog.FileName;
      }
      return filePath;
    }


    /// <summary> Response to the menu event : load BCF file </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void OpenFile_Menu(Object sender, EventArgs args) {
      panelcontent.OpenFile();
    }

    /// <summary> Response to the menu event : append BCF file </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void AppendFile_Menu(Object sender, EventArgs args) {
      panelcontent.AppendFile();
    }

    #region "Form utilities"

    /// <summary> Response to the menu event : About </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void About_Menu(Object sender, EventArgs args) {
      /*
      MessageBox.Show("BCF File Viewer Version 1.1.0\n\n" +
                      "By Emmanuel Maschas\n" +
                      "November 2020\n\n" +
                      "Report issues at https://github.com/emaschas/BCFviewer/issues", "BCF File Viewer");
      */
      Form about = new AboutForm();
      about.ShowDialog();
    }

    /// <summary> Response to the menu event : Quit </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void Quit_Menu(Object sender, EventArgs args) {
      this.Close();
    }

    /// <summary> Clean up any resources being used. </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if(disposing && (this.components != null)) {
        this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(900, 600);
      this.Text = "BCF Viewer"; // Title
    }

    #endregion

  }
}
