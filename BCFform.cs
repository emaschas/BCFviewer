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

<<<<<<< HEAD
    private Panel mainpanel;
    private BCFpanelContent panelcontent;
=======
    // Components
    private SplitContainer Hsplit, Vsplit;
    private TreeView tree;
    private ListView list;
    private PictureBox pict;
    // Image ratio
    private double ratio = 0.8;
    // BCF file object
    private BCFfileList bcfs = new BCFfileList();

    /// <summary> Add a File in the tree </summary>
    /// <param name="file">BCF file to be added in tree</param>
    private BCFTreeNode AddFile(BCFfile file) {
      BCFTreeNode tn = new BCFTreeNode();
      tn.Text = file.Name;
      tn.NodeFile = file;
      tn.NodeTopic = null;
      tn.NodeComment = null;
      tree.Nodes.Add(tn);
      return tn;
    }

    /// <summary> Add a Topic in the tree </summary>
    /// <param name="parent">File owning this topic</param>
    /// <param name="topic">Topic to be added in tree</param>
    private BCFTreeNode AddTopic(BCFTreeNode parent, Topic topic) {
      BCFTreeNode tn = new BCFTreeNode();
      tn.Text = topic.Title;
      tn.NodeFile = null;
      tn.NodeTopic = topic;
      tn.NodeComment = null;
      parent.Nodes.Add(tn);
      return tn;
    }

    /// <Summary> Add a Comment in the tree node of the Topic </Summary>
    /// <param name="parent">Topic owning this comment</param>
    /// <param name="comment">Comment to be added in tree node of a Topic</param>
    private BCFTreeNode AddComment(BCFTreeNode parent, Comment comment) {
      BCFTreeNode tn = new BCFTreeNode();
      tn.Text = comment.Text;
      tn.NodeFile = null;
      tn.NodeTopic = null;
      tn.NodeComment = comment;
      parent.Nodes.Add(tn);
      return tn;
    }

    /// <summary> Add a line with (name, value) in the list view </summary>
    /// <param name="name">Name of the property</param>
    /// <param name="value">Value of the property</param>
    private void AddProperty(string name, string value) {
      ListViewItem newItem = new ListViewItem(name, 0);
      newItem.SubItems.Add(value);
      list.Items.Add(newItem);
    }

    /// <summary> Node sorter to sort BCF by File Index, Topics Index, and Comment Date (ascending)</summary>
    public class BCFsorter : IComparer {
      /// <summary> Compare two <see cref="BCFTreeNode"/> refering to a File, a Topic, or a Comment</summary>
      /// The comparison is done on :
      /// <list><item>File Index,</item><item>Topics Index,</item><item>Comment Date (ascending)</item></list>
      public int Compare(object x, object y) {
        int result = 0;
        BCFTreeNode tx = x as BCFTreeNode;
        BCFTreeNode ty = y as BCFTreeNode;
        if(tx.NodeFile != null && ty.NodeFile != null) {
          result = tx.NodeFile.Index - ty.NodeFile.Index;
        } else if(tx.NodeTopic != null && ty.NodeTopic != null) {
          result = tx.NodeTopic.Index - ty.NodeTopic.Index;
        } else if(tx.NodeComment != null && ty.NodeComment != null) {
          DateTime dx = DateTime.Parse(tx.NodeComment.ModifiedDate);
          DateTime dy = DateTime.Parse(ty.NodeComment.ModifiedDate);
          result = DateTime.Compare(dx, dy);
        } else // should not happen !
          result = string.Compare(tx.Text, ty.Text);
        return result;
      }
    }
>>>>>>> a0234a900266fbeb90475bca59a0ccf0dcf67362

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
      //ms.MdiWindowListItem = menu1;
      //ms.MdiWindowListItem = menu2;
      strip.Items.Add(menu1);
      strip.Items.Add(menu2);
      strip.Dock = DockStyle.Top;
      this.MainMenuStrip = strip;
      //-----------------------
      mainpanel = new Panel();
      mainpanel.TabIndex = 0;
      mainpanel.Dock = DockStyle.Fill;
      //-----------------------
      this.Controls.Add(mainpanel);
      this.Controls.Add(strip);
      // Form
      this.MinimumSize = new Size(400, 400);
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

    #endregion

  }
}
