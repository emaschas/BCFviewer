/// \mainpage 
/// <h1>BCFclass</h1>
/// The BCFclass class contains the structures and the methods to read a BCF file.<br/><br/>
/// <h2>BCF</h2>
/// BCF = BIM Collaboration Format<br/>
/// BCF files are exported from 3D model review softwares and contain the topics and comments without the model itself.
///
/// <h2>References</h2>
/// Decsription of BCF files and their usage : <br/>
/// https://technical.buildingsmart.org/standards/bcf/<br/>
/// The BCF file schema is detailed in :<br/>
/// https://github.com/buildingSMART/BCF-XML/tree/master/Documentation<br/>
/// <hr>
/// By Emmanuel Maschas - 10-2020

#region "Usings"
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Xml.Linq;
using System.IO;
using System.IO.Compression;
using System.Globalization;
#endregion

namespace BCFclass {

  #region "BCF Structures"

  /// <summary> Viewpoint structure<br/>
  /// The viewpoint includes :
  /// <list type="table">
  /// <item><term>GUID</term><description> Globally Unique Identifier</description></item>
  /// <item><term>Index</term><description> Index of the viewpoint</description></item>
  /// <item><term>Bcfv</term><description> View position definition file name</description></item>
  /// <item><term>Snapshot</term><description> Snapshot file name</description></item>
  /// <item><term>CamX,CamY, CamZ</term><description> Position of the Camera (in meters)</description></item>
  /// <item><term>DirX,DirY, DirZ</term><description> View direction</description></item>
  /// <item><term>UpX,UpY, UpZ</term><description> Up direction</description></item>
  /// <item><term>Field</term><description> Field of view in degress</description></item>
  /// <item><term>Components</term><description> List of visible components</description></item>
  /// <item><term>Image</term><description> Content of the snapshot file (Bitmap)</description></item>
  /// </list>
  /// </summary>
  public class Viewpoint {
    /// <summary>Globally Unique Identifier</summary>
    public string GUID             {get;set;}
    /// <summary>Index of the viewpoint</summary>
    public string Index            {get;set;}
    /// <summary>View position definition file name</summary>
    public string Bcfv             {get;set;}
    /// <summary>snapshot file name</summary>
    public string Snapshot         {get;set;}
    /// <summary>Position of the Camera, X component in meters</summary>
    public double CamX             {get;set;}
    /// <summary>Position of the Camera, Y component in meters</summary>
    public double CamY             {get;set;}
    /// <summary>Position of the Camera, Z component in meters</summary>
    public double CamZ             {get;set;}
    /// <summary>View direction, X component</summary>
    public double DirX             {get;set;}
    /// <summary>View direction, Y component</summary>
    public double DirY             {get;set;}
    /// <summary>View direction, Z component</summary>
    public double DirZ             {get;set;}
    /// <summary>Up direction, X component</summary>
    public double UpX              {get;set;}
    /// <summary>UpY : Up direction, Y component</summary>
    public double UpY              {get;set;}
    /// <summary>Up direction, Z component</summary>
    public double UpZ              {get;set;}
    /// <summary>Field of view in degress</summary>
    public double Field            {get;set;}
    /// <summary>List of visible components</summary>
    public List<string> Components {get;set;}
    /// <summary>Content (Bitmap) of the snapshot file </summary>
    public Bitmap Image            {get;set;}
  }

  /// <summary> Comment Structure<br/>
  /// The Comment structure includes :
  /// <list type="table">
  /// <item><term>Date</term><description>Creation date of the comment</description></item>
  /// <item><term>Author</term><description>Creation author of the comment</description></item>
  /// <item><term>Text</term><description>Text of the comment</description></item>
  /// <item><term>ModifiedDate</term><description>Last modification date of the comment</description></item>
  /// <item><term>ModifiedAuthor</term><description>Last modification author of the comment</description></item>
  /// <item><term>VPGuid</term><description>GUID of the viewpoint associated to the comment (optional)</description></item>
  /// <item><term>Viewpoint</term><description>Viewpoint associated to the comment or <c>null</c></description></item>
  /// </list>
  /// </summary>
  public class Comment {
    /// <summary>Creation date of the comment</summary>
    public string Date            {get;set;}
    /// <summary>Creation author of the comment</summary>
    public string Author          {get;set;}
    /// <summary>Text of the comment</summary>
    public string Text            {get;set;}
    /// <summary>Last modification date of the comment</summary>
    public string ModifiedDate    {get;set;}
    /// <summary>Last modification author of the comment</summary>
    public string ModifiedAuthor  {get;set;}
    /// <summary>GUID of the viewpoint associated to the comment (optional)</summary>
    public string VPGuid          {get;set;}
    /// <summary>Viewpoint associated to the comment or <i>null</i></summary>
    public Viewpoint Viewpoint    {get;set;}
  }

  /// <summary> Topic Structure 
  /// <list type="table">
  /// <item><term>ZipFile</term><description>Full name of the *.bcfzip file, source of this Topic</description></item>
  /// <item><term>TopicType</term><description>Type of topic</description></item>
  /// <item><term>TopicStatus</term><description>Status of the topic</description></item>
  /// <item><term>Title</term><description>Title of the Topic</description></item>
  /// <item><term>Priority</term><description>Priority of the topic</description></item>
  /// <item><term>Index</term><description>Index of the topic</description></item>
  /// <item><term>CreationDate</term><description>Date of creation of the Topic</description></item>
  /// <item><term>CreationAuthor</term><description>Author that created the Topic</description></item>
  /// <item><term>ModifiedDate</term><description>Last date of modification of the Topic</description></item>
  /// <item><term>ModifiedAuthor</term><description>Last Author that modified the Topic</description></item>
  /// <item><term>Description</term><description>Decriptio of the Topic</description></item>
  /// <item><term>Comments</term><description>List of Comments associated to the Topic</description></item>
  /// <item><term>Viewpoints</term><description>List of Viewpoints associated to the Topic</description></item>
  /// </list>
  /// </summary>
  public class Topic {
    /// <summary>ZipFile : Full name of the *.bcfzip file, source of this Topic<br/>
    /// Since multiple BCF files may be appendedn this information is recorder for each individual Topic</summary>
    public string ZipFile             {get;set;}
    /// <summary>Type of topic</summary>
    /// <value><list type="bullet"><item>Comment</item><item>Issue</item><item>Request</item><item>Solution</item></list></value>
    public string TopicType           {get;set;}
    /// <summary>Status of the topic</summary>
    /// <value><list type="bullet"><item>Open</item><item>In Progress</item><item>Closed</item><item>ReOpened</item></list></value>
    public string TopicStatus         {get;set;}
    /// <summary>Title of the Topic</summary>
    public string Title               {get;set;}
    /// <summary>Priority of the topic</summary>
    public string Priority            {get;set;}
    /// <summary>Index of the topic</summary>
    public string Index               {get;set;}
    /// <summary>Date of creation of the Topic</summary>
    public string CreationDate        {get;set;}
    /// <summary>Author that created the Topic</summary>
    public string CreationAuthor      {get;set;}
    /// <summary>Last date of modification of the Topic</summary>
    public string ModifiedDate        {get;set;}
    /// <summary>Last Author that modified the Topic</summary>
    public string ModifiedAuthor      {get;set;}
    /// <summary>Decriptio of the Topic</summary>
    public string Description         {get;set;}
    /// <summary>List of Comments associated to the Topic</summary>
    public List<Comment> Comments     {get;set;}
    /// <summary>List of Viewpoints associated to the Topic</summary>
    public List<Viewpoint> Viewpoints {get;set;}
  }

  #endregion

  /// <summary> BCF file class </summary>
  public class BCFfile {
  //public partial class BCFfile : UserControl {

    /// <summary> List of BCF Topics loaded in the BCFfile object </summary>
    public List<Topic> TopicsList = new List<Topic>();

    #region "BCF Utilities"

    /// <summary> Utility to read the value of the XML element <paramref name="field"/> of the XML <paramref name="element"/></summary>
    /// <returns> The value of the XML element <paramref name="field"/> or "-" if unset </returns>
    private string ReadXML(XElement element, string field) {
      if(element.Element(field) != null)
        return element.Element(field).Value;
      else
        return "-";
    }
    
    /// <summary> Utility to read the <paramref name="attribute"/> of the XML <paramref name="element"/> </summary>
    /// <returns> The <paramref name="attribute"/> value or "-" if unset </returns>
    private string ReadATT(XElement element, string attribute) {
      string res = "-";
      foreach(XAttribute att in element.Attributes(attribute)) res = att.Value;
      return res;
    }
    
    /// <summary> Format the supplied <paramref name="ISOdate"/> ISO date in a readable string </summary>
    /// <param name="ISOdate">Date in ISO format. Ex : 2014-10-16T14:35:29+00:00</param>
    /// <returns>A string reprsenting the supplied <paramref name="ISOdate"/> ISO date</returns>
    private string formatDate(string ISOdate) {
      string res;
      try { 
        DateTime dat = DateTime.Parse(ISOdate);
        res = dat.ToString("d", CultureInfo.CreateSpecificCulture("fr-FR")); 
      }
      catch { res = ISOdate; }
      return res;
    }

    #endregion

    #region "Read Routines"

    /// <summary> Read the Camera settings for the selected <paramref name="viewpoint"/>.bcfv file </summary>
    /// <param name="bcfzip">ZipArchive in which the bcfv file is located</param>
    /// <param name="filename">Name of bcfv viewpoint file within the ZIP archive</param>
    /// <param name="viewpoint">Viewpoint in which the camera settings will be stored</param>
    /// <returns>Nothing</returns>
    private void readBCFV(ZipArchive bcfzip, string filename, Viewpoint viewpoint) {
      ZipArchiveEntry bcfvzip = bcfzip.GetEntry(filename);
      // Default values
      viewpoint.CamX = 0.0; viewpoint.CamY = 0.0; viewpoint.CamZ = 0.0;
      viewpoint.DirX = 1.0; viewpoint.DirY = 0.0; viewpoint.DirZ = 0.0;
      viewpoint.UpX  = 0.0; viewpoint.UpY  = 0.0; viewpoint.UpZ  = 1.0;
      viewpoint.Field = 0.0; // Identifies unset camera.
      viewpoint.Components = new List<String>();
      if(bcfvzip!=null) {
        XDocument bcfv = XDocument.Load(bcfvzip.Open()); // Parse bcfv XML document
        XElement camera = bcfv.Root.Element("PerspectiveCamera");
        if(camera!=null) {
          XElement point = camera.Element("CameraViewPoint");
          viewpoint.CamX = Double.Parse(point.Element("X").Value);
          viewpoint.CamY = Double.Parse(point.Element("Y").Value);
          viewpoint.CamZ = Double.Parse(point.Element("Z").Value);
          point = camera.Element("CameraDirection");
          viewpoint.DirX = Double.Parse(point.Element("X").Value);
          viewpoint.DirY = Double.Parse(point.Element("Y").Value);
          viewpoint.DirZ = Double.Parse(point.Element("Z").Value);
          point = camera.Element("CameraUpVector");
          viewpoint.UpX = Double.Parse(point.Element("X").Value);
          viewpoint.UpY = Double.Parse(point.Element("Y").Value);
          viewpoint.UpZ = Double.Parse(point.Element("Z").Value);
          viewpoint.Field = Double.Parse(camera.Element("FieldOfView").Value);
          XElement cps = bcfv.Root.Element("Components");
          if(cps != null) {
            foreach(XElement comp in cps.Elements("Component")) {
              viewpoint.Components.Add(ReadATT(comp, "IfcGuid"));
            }
          }
        }
      } else {
        MessageBox.Show(filename + "  Not found");
      }
    }
    
    /// <summary> Load or Append a BCF File </summary> 
    /// <param name="FileName">Full path of the file to be loadded or appended </param>
    /// <param name="Append">Load if <c>false</c> or Append if <c>true</c> </param>
    /// <returns>Nothing</returns>
    public void ReadBCF(String FileName, Boolean Append) {
      if(!Append) TopicsList.Clear();
      using(ZipArchive bcfzip = ZipFile.OpenRead(FileName)) {
        foreach(ZipArchiveEntry entry in bcfzip.Entries) {
          if(entry.FullName.EndsWith("markup.bcf", StringComparison.OrdinalIgnoreCase)) {
            string folder = entry.FullName.Substring(0, entry.FullName.LastIndexOf("/") + 1);
            XDocument markup = XDocument.Load(entry.Open()); // Parse XML document
            // Topic
            XElement topic = markup.Root.Element("Topic");
            Topic NewTopic = new Topic();
            NewTopic.ZipFile        = FileName;
            NewTopic.TopicType      = ReadATT(topic, "TopicType");
            NewTopic.TopicStatus    = ReadATT(topic, "TopicStatus");
            NewTopic.Title          = ReadXML(topic, "Title");
            NewTopic.Priority       = ReadXML(topic, "Priority");
            NewTopic.Index          = ReadXML(topic, "Index");
            NewTopic.CreationDate   = ReadXML(topic, "CreationDate");
            NewTopic.CreationAuthor = ReadXML(topic, "CreationAuthor");
            NewTopic.ModifiedDate   = ReadXML(topic, "ModifiedDate");
            NewTopic.ModifiedAuthor = ReadXML(topic, "ModifiedAuthor");
            NewTopic.Description    = ReadXML(topic, "Description");
            if(NewTopic.ModifiedDate  =="-") NewTopic.ModifiedDate=NewTopic.CreationDate;
            if(NewTopic.ModifiedAuthor=="-") NewTopic.ModifiedAuthor=NewTopic.CreationAuthor;
            // Comments
            NewTopic.Comments = new List<Comment>();
            foreach(XElement com in markup.Root.Elements("Comment")) {
              Comment NewComment = new Comment();
              NewComment.Text =           ReadXML(com, "Comment");
              NewComment.Date =           ReadXML(com, "Date");
              NewComment.Author =         ReadXML(com, "Author");
              NewComment.ModifiedAuthor = ReadXML(com, "ModifiedAuthor");
              NewComment.ModifiedDate =   ReadXML(com, "ModifiedDate");
              if(NewComment.ModifiedAuthor=="-") NewComment.ModifiedAuthor=NewComment.Author;
              if(NewComment.ModifiedDate  =="-") NewComment.ModifiedDate=NewComment.Date;
              XElement vp = com.Element("Viewpoint");
              NewComment.VPGuid = (vp != null ? ReadATT(vp, "Guid") : "-");
              NewTopic.Comments.Add(NewComment);
            }
            // Viewpoints
            NewTopic.Viewpoints = new List<Viewpoint>();
            // Search for Viewpoints in Version 2.0
            foreach(XElement vp in markup.Root.Elements("Viewpoints")) {
              Viewpoint NewVP = new Viewpoint();
              NewVP.GUID     = ReadATT(vp, "Guid");
              NewVP.Bcfv     = ReadXML(vp, "Viewpoint");
              NewVP.Snapshot = ReadXML(vp, "Snapshot");
              if(NewVP.Snapshot!="-") NewVP.Snapshot = folder + NewVP.Snapshot;
              readBCFV(bcfzip, folder + NewVP.Bcfv, NewVP);
              // Viewpoint-Image
              try {
                if(NewVP.Snapshot!="-") {
                  ZipArchiveEntry snap = bcfzip.GetEntry(NewVP.Snapshot);
                  Bitmap img = new Bitmap(snap.Open());
                  NewVP.Image = img;
                }
              } catch {
                MessageBox.Show("Error Image");
              }
              NewTopic.Viewpoints.Add(NewVP);
            }
            // If Viewpoints is empty : 
            // try version 1.0 BCF : viewpoint.bcfv and snapshot.png files.
            if(NewTopic.Viewpoints.Count == 0) {
              ZipArchiveEntry bcfvzip = bcfzip.GetEntry(folder + "viewpoint.bcfv");
              if(bcfvzip!=null) { // Found viewpoint.bcfv !
                Viewpoint NewVP = new Viewpoint();
                NewVP.GUID = "viewpoint.bcfv"; // pseudo att.
                NewVP.Bcfv = "viewpoint.bcfv";
                if(bcfzip.GetEntry(folder + "snapshot.png") != null) {
                  NewVP.Snapshot = folder + "snapshot.png";
                  ZipArchiveEntry snap = bcfzip.GetEntry(NewVP.Snapshot);
                  Bitmap img = new Bitmap(snap.Open());
                  NewVP.Image = img;
                } else {
                  NewVP.Snapshot = "-";
                  NewVP.Image = null;
                }
                readBCFV(bcfzip, folder + NewVP.Bcfv, NewVP);
                NewTopic.Viewpoints.Add(NewVP);
              }
            }
            // Link Viewpoint and Comments
            foreach(Comment com in NewTopic.Comments) {
              if(com.VPGuid != "-") {
                foreach (Viewpoint vp in NewTopic.Viewpoints) {
                  if(vp.GUID == com.VPGuid) com.Viewpoint = vp;
                }
              }
            }
            TopicsList.Add(NewTopic);
          }
        }
      }
    }

    #endregion

    #region "Constructors"

    /// <summary>
    /// BCFclass :<br/>
    /// Contains the structures and the methods to read a BCF file.<br/>
    /// BCF : BIM Collaboration Format<br/>
    /// See : https://technical.buildingsmart.org/standards/bcf/<br/>
    /// and : https://github.com/buildingSMART/BCF-XML/tree/master/Documentation<br/>
    /// BCFfile constructors :<br/>
    /// - BCFfile() : Create an empty BCFfile object<br/>
    /// - BCFfile(filename) : Create a BCFfile object, and read the given file
    /// </summary>
    /// <param name="FileName">Name of the BCF file to read</param>
    public BCFfile(string FileName) {
      this.ReadBCF(FileName, false);
    }

    /// <summary>
    /// BCFclass :<br/>
    /// Contains the structures and the methods to read a BCF file.<br/>
    /// BCF : BIM Collaboration Format<br/>
    /// See : https://technical.buildingsmart.org/standards/bcf/<br/>
    /// and : https://github.com/buildingSMART/BCF-XML/tree/master/Documentation<br/>
    /// BCFfile constructors :<br/>
    /// - BCFfile() : Create an empty BCFfile object<br/>
    /// - BCFfile(filename) : Create a BCFfile object, and read the given file
    /// </summary>
    public BCFfile() {}

    #endregion

  }

}
