using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using BCFclass;

namespace BCFviewer {

  /// <summary> 
  /// BCFTreeNode if a class derivated from TreeNode with additional properties : <br/>
  ///   - NodeTopic   : Topic reference if the TReeNodes refers to this type of element <br/>
  ///   - NodeComment : Comment reference if the TReeNodes refers to this type of element <br/>
  /// Only one of them shall be defined, the other shall be null
  /// </summary>
  public class BCFTreeNode : TreeNode {
    ///<summary>Added property : Topic NodeTopic</summary>
    public Topic NodeTopic;
    ///<summary>Added property : Comment NodeComment</summary>
    public Comment NodeComment;
  }
  
  /// <Summary> Main class of the BCFform </Summary>
  public partial class BCFform : Form {

    // Components
    private SplitContainer Hsplit, Vsplit;
    private TreeView tree;
    private ListView list;
    private PictureBox pict;
    private Bitmap btmp;
    // Image ratio
    private double ratio = 0.8;
    // BCF file object
    private BCFfile bcf;

    /// <summary> Add a Topic in the tree </summary>
    /// <param name="topic">Topic to be added in tree</param>
    private BCFTreeNode AddTopic(Topic topic) {
      BCFTreeNode tn = new BCFTreeNode();
      tn.Text = topic.Title;
      tn.NodeTopic = topic;
      tn.NodeComment = null;
      tree.Nodes.Add(tn);
      return tn;
    }

    /// <Summary> Add a Comment in the tree node of the Topic </Summary>
    /// <param name="topic">Topic owning this comment</param>
    /// <param name="comment">Comment to be added in tree node of a Topic</param>
    private BCFTreeNode AddComment(BCFTreeNode topic, Comment comment) {
      BCFTreeNode tn = new BCFTreeNode();
      tn.Text = comment.Text;
      tn.NodeTopic = null;
      tn.NodeComment = comment;
      topic.Nodes.Add(tn);
      return tn;
    }

    /// <summary> BCFform constructor </summary>
    public BCFform()
    {
      InitializeComponent();
      String exe = Application.ExecutablePath;
      String pat = Path.GetDirectoryName(exe);
      this.Icon = new Icon( pat + "\\icons\\BCF.ico");
      this.Padding = new Padding(10, 0, 10, 10);
      // Menu
      MenuStrip ms = new MenuStrip();
      ToolStripMenuItem menu1   = new ToolStripMenuItem("BCF &File");
      menu1.ShowShortcutKeys = true;
      ToolStripMenuItem menu2   = new ToolStripMenuItem("&About", null, new EventHandler(About));
      menu2.ShortcutKeys = Keys.F1;
      ToolStripMenuItem smenu11 = new ToolStripMenuItem("&Load New BCF file", null, new EventHandler(OpenFile));
      smenu11.ShortcutKeys = Keys.Control | Keys.O;
      smenu11.ShortcutKeyDisplayString = "Ctrl-O";
      smenu11.ShowShortcutKeys = true;
      ToolStripMenuItem smenu12 = new ToolStripMenuItem("&Append BCF file", null, new EventHandler(AppendFile));
      smenu12.ShortcutKeys = Keys.Control | Keys.A;
      ToolStripMenuItem smenu13 = new ToolStripMenuItem("&Quit", null, new EventHandler(Quit));
      menu1.DropDownItems.Add(smenu11);
      menu1.DropDownItems.Add(smenu12);
      menu1.DropDownItems.Add("-");
      menu1.DropDownItems.Add(smenu13);
      ((ToolStripDropDownMenu)(menu1.DropDown)).ShowImageMargin = false;
      ((ToolStripDropDownMenu)(menu1.DropDown)).ShowCheckMargin = true;
      //ms.MdiWindowListItem = menu1;
      //ms.MdiWindowListItem = menu2;
      ms.Items.Add(menu1);
      ms.Items.Add(menu2);
      ms.Dock = DockStyle.Top;
      this.MainMenuStrip = ms;
      //-----------------------
      // Tree View
      tree = new TreeView();
      tree.TabIndex = 2;
      tree.Dock = DockStyle.Fill;
      tree.CheckBoxes = false;
      tree.AfterSelect += new TreeViewEventHandler(ShowTopicDetails);
      // List View
      list = new ListView();
      list.TabIndex = 3;
      list.Dock = DockStyle.Fill;
      list.View = View.Details;
      list.FullRowSelect = true;
      list.GridLines = false;
      // Image
      pict = new PictureBox();
      pict.Dock = DockStyle.Fill;
      pict.TabStop = false;
      pict.SizeMode = PictureBoxSizeMode.Zoom;
      pict.BorderStyle = BorderStyle.FixedSingle;
      pict.BackColor = Color.Beige;
      // Vertical Split Container
      Vsplit = new SplitContainer();
      Vsplit.TabIndex = 1;
      Vsplit.Dock = DockStyle.Fill;
      Vsplit.ForeColor = SystemColors.Control;
      Vsplit.Name = "Vertical Split";
      Vsplit.Orientation = Orientation.Horizontal;
      Vsplit.SplitterDistance = 295;
      Vsplit.SplitterWidth = 10;
      Vsplit.IsSplitterFixed = true;
      Vsplit.Panel1.Name = "Image";
      Vsplit.Panel2.Name = "Properties";
      Vsplit.Panel2MinSize = 100;
      // Horizontal Split Container
      Hsplit = new SplitContainer();
      Hsplit.TabIndex = 0;
      Hsplit.Dock = DockStyle.Fill;
      Hsplit.ForeColor = SystemColors.Control;
      Hsplit.Name = "Horizontal Split";
      Hsplit.Orientation = Orientation.Vertical;
      Hsplit.SplitterDistance = 80;
      Hsplit.SplitterWidth = 10;
      Hsplit.SplitterMoved += new SplitterEventHandler(HSplitterMoved);
      Hsplit.SplitterMoving += new SplitterCancelEventHandler(HSplitterMoving);
      Hsplit.Panel1.Name = "Topics List";
      Hsplit.Panel2.Name = "Details";
      // Fill the form
      Vsplit.Panel1.Controls.Add(pict);
      Vsplit.Panel2.Controls.Add(list);
      Hsplit.Panel1.Controls.Add(tree);
      Hsplit.Panel2.Controls.Add(Vsplit);
      this.Controls.Add(Hsplit);
      this.Controls.Add(ms);
      // Form
      this.MinimumSize = new Size(400,400);
      // Form resize handler
      this.Resize += new EventHandler(BCFform_Resize);
      // Resize picture
      ImageResize();
      // Construct the BCF file object
      bcf = new BCFfile();
    }

    /// <summary> Add a set line with (name, value) in the list view </summary>
    /// <param name="name">Name of the property</param>
    /// <param name="value">Value of the property</param>
    private void AddProperty(string name, string value) {
      ListViewItem newItem = new ListViewItem(name,0);
      newItem.SubItems.Add(value);
      list.Items.Add(newItem);
    }

    /// <summary> Fills the details of the selected Topic in the list view and the image </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void ShowTopicDetails(Object sender, TreeViewEventArgs args) {
      list.Clear();
      if(this.bcf.TopicsList.Count>0) {
        BCFTreeNode tn = (BCFTreeNode)tree.SelectedNode;
        if(tn.NodeComment==null) {
          list.Columns.Add("TOPIC", -1, HorizontalAlignment.Left);
          list.Columns.Add("", -1, HorizontalAlignment.Left);
          Topic topic = tn.NodeTopic;
          AddProperty("Title", topic.Title);
          AddProperty("Description",topic.Description);
          AddProperty("Topic Type",topic.TopicType);
          AddProperty("Topic Status",topic.TopicStatus);
          AddProperty("Topic Index",topic.Index.ToString());
          AddProperty("Creation Date",formatDate(topic.CreationDate));
          AddProperty("Creation Author",topic.CreationAuthor);
          AddProperty("Modified Date",formatDate(topic.ModifiedDate));
          AddProperty("Modified Author",topic.ModifiedAuthor);
          // Viewpoint
          if(topic.Viewpoints.Count > 0) {
            Viewpoint vp = topic.Viewpoints[0];
            // Viewpoint-Image
            if(vp.Image != null) {
              btmp = new Bitmap(vp.Image);
              pict.Image = btmp;
              ratio = (double)btmp.Height / (double)btmp.Width;
              ImageResize();
            } else { // No Image
              pict.Image = null;
            }
          }
        } else {
          list.Columns.Add("COMMENT", -1, HorizontalAlignment.Left);
          list.Columns.Add("", -1, HorizontalAlignment.Left);
          Comment comment = tn.NodeComment;
          AddProperty("Comment", comment.Text);
          AddProperty("Modified Date",formatDate(comment.ModifiedDate));
          AddProperty("Modified Author",comment.ModifiedAuthor);
          BCFTreeNode tp = (BCFTreeNode)tn.Parent;
          Topic topic = tp.NodeTopic;
          // Viewpoint
          Viewpoint vp = null;
          if(comment.Viewpoint != null) {
            vp = comment.Viewpoint;
          } else if(topic.Viewpoints.Count > 0) {
            vp = topic.Viewpoints[0];
          }
          if(vp != null) { // Viewpoint-Image
            if(vp.Image != null) {
              btmp = new Bitmap(vp.Image);
              pict.Image = btmp;
              ratio = (double)btmp.Height / (double)btmp.Width;
              ImageResize();
            } else { // No Image
              pict.Image = null;
            }
          }
        }
      }
      list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
    }

    /// <summary> Response to the menu event : load BCF file </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void OpenFile(Object sender, EventArgs args) {
      LoadFile(false);
    }

    /// <summary> Response to the menu event : append BCF file </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void AppendFile(Object sender, EventArgs args) {
      LoadFile(true);
    }

    /// <summary> Read or Append a BCF file according to the Append parameter </summary>
    /// <param name="Append">Load if false or Append if true </param>
    private void LoadFile(Boolean Append) {
      using(OpenFileDialog openFileDialog = new OpenFileDialog()) {
        openFileDialog.Filter = "BCFZIP files (*.bcfzip)|*.bcfzip|BCF files (*.bcf)|*.bcf|All files (*.*)|*.*";
        openFileDialog.FilterIndex = 1;
        openFileDialog.RestoreDirectory = true;
        if(openFileDialog.ShowDialog() == DialogResult.OK) {
          //Get the path of specified file
          string filePath = openFileDialog.FileName;
          // Read the BCF file
          bcf.ReadBCF(filePath, Append);
          tree.Nodes.Clear();
          foreach(Topic topic in bcf.TopicsList) {
            BCFTreeNode tn = AddTopic(topic);
            foreach(Comment comment in topic.Comments) {
              AddComment(tn, comment);
            }
          }
          if(tree.Nodes.Count > 0) tree.SelectedNode = tree.Nodes[0];
          // Resize picture
          ImageResize();
        }
      }
    }

    /// <summary> Response to the menu event : About </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void About(Object sender, EventArgs args) {
      MessageBox.Show("BCF File Viewer by Emmanuel Maschas - Nov. 2020", "BCF File Viewer");
    }

    /// <summary> Move the vertical split to match the image ratio (Width / Height) </summary>
    private void ImageResize() {
      int maxHeight = Vsplit.Height - Vsplit.Panel2MinSize - Vsplit.SplitterWidth;
      int newHeight = (int)((double)Vsplit.Width * ratio);
      if(maxHeight > 0) {
        Vsplit.SplitterDistance = (newHeight < maxHeight ? newHeight : maxHeight);
      }
    }

    /// <summary> Response to the event that occurs when the form is resized </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void BCFform_Resize(Object sender, EventArgs args) {
      ImageResize();
    }

    /// <summary> Response to the event that occurs when the horizontal splitter is moving </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void HSplitterMoving(Object sender, SplitterCancelEventArgs args) {
        Cursor.Current = Cursors.VSplit;
    }

    /// <summary> Response to the event that occurs after the horizontal splitter has been moved </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void HSplitterMoved(Object sender, SplitterEventArgs args) {
        Cursor.Current = Cursors.Default;
        ImageResize();
    }

    /// <summary> Format an ISO date in readable string </summary>
    /// <param name="isodate">Date in ISO format. Ex : 2014-10-16T14:35:29+00:00</param>
    private string formatDate(string isodate) {
      string res;
      try { 
        DateTime dat = DateTime.Parse(isodate);
        res = dat.ToString("d", CultureInfo.CreateSpecificCulture("fr-FR")); 
      }
      catch { res = isodate; }
      return res;
    }

    /// <summary> Response to the menu event : Quit </summary>
    /// <param name="sender">Not used</param>
    /// <param name="args">Not used</param>
    private void Quit(Object sender, EventArgs args) {
      this.Close();
    }
  }
}
