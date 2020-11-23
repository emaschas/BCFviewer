
// Read BCF comments file
// Emmanuel Maschas - 10-2020
// Class

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

namespace BCF_Read {

  #region "BCF Structures"

  public class Viewpoint {
    public string GUID             {get;set;}
    public string Index            {get;set;}
    public string Bcfv             {get;set;}
    public string Snapshot         {get;set;}
    public double CamX             {get;set;}
    public double CamY             {get;set;}
    public double CamZ             {get;set;}
    public double DirX             {get;set;}
    public double DirY             {get;set;}
    public double DirZ             {get;set;}
    public double UpX              {get;set;}
    public double UpY              {get;set;}
    public double UpZ              {get;set;}
    public double Field            {get;set;}
    public List<string> Components {get;set;}
    public Bitmap Image            {get;set;}
  }

  public class Comment {
    public string Date            {get;set;}
    public string Author          {get;set;}
    public string Text            {get;set;}
    public string ModifiedDate    {get;set;}
    public string ModifiedAuthor  {get;set;}
    public string VPGuid          {get;set;}
    public Viewpoint Viewpoint    {get;set;}
  }

  public class Topic {
    public string ZipFile             {get;set;}
    public string TopicType           {get;set;}
    public string TopicStatus         {get;set;}
    public string Title               {get;set;}
    public string Priority            {get;set;}
    public string Index               {get;set;}
    public string CreationDate        {get;set;}
    public string CreationAuthor      {get;set;}
    public string ModifiedDate        {get;set;}
    public string ModifiedAuthor      {get;set;}
    public string Description         {get;set;}
    public List<Comment> Comments     {get;set;}
    public List<Viewpoint> Viewpoints {get;set;}
  }

  #endregion

  /// <summary> BCF file class </summary>
  public partial class BCFfile : UserControl {

    /// <summary> List of BCF Topics loaded in Navisworks </summary>
    public List<Topic> TopicsList = new List<Topic>();

    #region "BCF Utilities"

    /// <summary> Utility to read an xml field </summary>
    /// <returns> The value of the XML element or "-" if unset </returns>
    private string ReadXML(XElement obj, string field) {
      if(obj.Element(field) != null)
        return obj.Element(field).Value;
      else
        return "-";
    }
    
    /// <summary> Utility to read an xml attribute </summary>
    /// <returns> The attribute value or "-" if unset </returns>
    private string ReadATT(XElement obj, string attribute) {
      string res = "-";
      foreach(XAttribute att in obj.Attributes(attribute)) res = att.Value;
      return res;
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

    #endregion

    #region "Read Routines"

    /// <summary> Read the Camera settings for the selected viewpoint.bcfv file </summary>
    /// <param name="bcfzip">ZipArchive in which the bcfv file is located</param>
    /// <param name="filename">Name of bcfv viewpoint file within the ZIP archive</param>
    /// <param name="NewVP">Viewpoint in which the camera settings will be stored</param>
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
    /// <param name="Append">Load if false or Append if true </param>
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

    /// <summary> BCFfile constructor </summary>
    public BCFfile(string FileName, Boolean Append) {
      this.ReadBCF(FileName, Append);
    }
    /// <summary> BCFfile constructor overload </summary>
    public BCFfile(string FileName) {
      this.ReadBCF(FileName, false);
    }
    /// <summary> BCFfile constructor overload </summary>
    public BCFfile() {
      // Do nothing...
    }
  }

}
