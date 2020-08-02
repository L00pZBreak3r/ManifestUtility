/* This class implements my own xml based file format.
 * All tags and their attributes seem to be easy to understand. :)
 * */

using System;
using System.Xml;
using System.Text;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

namespace ManifestManagerLib
{
  class FileItem
  {
    public string Name;
    public string validation;
    public string destination;
    public long size = -1;
    public FileItem(string name, string dest, string val)
    {
      Name = name;
      validation = val;
      destination = dest;
    }
    public FileItem(string name, string dest) : this(name, dest, null) { }
    public FileItem(string name) : this(name, null, null) { }
  }

  public class UpdateManifestFile1 : ArrayListHelperBase, IUpdateManifestFile
  {
    private ArrayList files;
    private FileCopyMethod copyMethod;
    private string globalUpdateLocation;
    private Version targetApplicationVersion;
    private Version newApplicationVersion;
    private PostUpdateCommand postUpdateCommand;
    private string applicationId;
    private string publisherId;
    private ApplicationPlatform platform;
    private string description;
    private readonly string rootPath;
    private readonly string manifestFileName;
    private bool useValidation = true;

    private const string DEFAULT_MANIFEST_FILENAME = "update.xml";

    protected const string XML_COMMENT = "Updater Manifest File";
    protected const string XML_PROCESSING_INSTRUCTION = "version=\"1.0\"";
    protected const string XML_ELEMENT_ROOT = "UpdaterManifestFile";
    protected const string XML_ELEMENT_ROOT_ATTRIBUTE_APPLICATION_ID = "Product";
    protected const string XML_ELEMENT_ROOT_ATTRIBUTE_PUBLISHER_ID = "Publisher";
    protected const string XML_ELEMENT_ROOT_ATTRIBUTE_APPLICATION_PLATFORM = "Platform";
    protected const string XML_ELEMENT_ROOT_ATTRIBUTE_APPLICATION_TARGETVERSION = "ApplicationTargetVersion";
    protected const string XML_ELEMENT_DESCRIPTION = "Description";
    protected const string XML_ELEMENT_NEW_APPLICATION_VERSION = "ApplicationNewVersion";
    protected const string XML_ELEMENT_POST_UPDATE_COMMAND = "PostUpdateCommand";
    protected const string XML_ELEMENT_POST_UPDATE_COMMAND_ATTRIBUTE_EXECUTABLE = "File";
    protected const string XML_ELEMENT_POST_UPDATE_COMMAND_ATTRIBUTE_TARGETPATH = "TargetPath";
    protected const string XML_ELEMENT_POST_UPDATE_COMMAND_ATTRIBUTE_ARGUMENTS = "Parameters";
    protected const string XML_ELEMENT_POST_UPDATE_COMMAND_ATTRIBUTE_DELETE = "Delete";
    protected const string XML_ELEMENT_UPDATE_LOCATION = "UpdateLocation";
    protected const string XML_ELEMENT_FILES = "Files";
    protected const string XML_ELEMENT_FILES_ITEM = "Item";
    protected const string XML_ELEMENT_FILES_ITEMAttributeName = "Name";
    protected const string XML_ELEMENT_FILES_ITEM_ATTRIBUTE_TARGETPATH = "TargetPath";
    protected const string XML_ELEMENT_FILES_ITEMAttributeSize = "Size";
    protected const string XML_ELEMENT_FILES_ITEMAttributeHash = "Hash";

    public const int ERROR_SUCCESS = 0;
    public const int ERROR_ROOTPATH_NOT_SPECIFIED = -1;
    public const int ERROR_PROCUCT_NOT_SPECIFIED = -2;
    public const int ERROR_PUBLISHER_NOT_SPECIFIED = -3;
    public const int ERROR_UPDATELOCATION_NOT_SPECIFIED = -4;
    public const int ERROR_NEWVERSION_NOT_SPECIFIED = -5;
    public const int ERROR_ROOTPATH_NOT_FOUND = -6;

    private XmlDocument manifestDocument;

    public UpdateManifestFile1(string rootpath, string filename)
    {
      rootPath = rootpath;
      manifestFileName = filename;
    }
    public UpdateManifestFile1(string rootpath) : this(rootpath, null) { }

    private XmlElement CreateFileItem(FileItem fileItem)
    {
      XmlElement xFile = null;
      if (!string.IsNullOrEmpty(fileItem.Name))
      {
        xFile = manifestDocument.CreateElement(XML_ELEMENT_FILES_ITEM);
        xFile.SetAttribute(XML_ELEMENT_FILES_ITEMAttributeName, fileItem.Name);
        if (!string.IsNullOrEmpty(fileItem.destination))
          xFile.SetAttribute(XML_ELEMENT_FILES_ITEM_ATTRIBUTE_TARGETPATH, fileItem.destination);
        if (fileItem.size >= 0)
          xFile.SetAttribute(XML_ELEMENT_FILES_ITEMAttributeSize, fileItem.size.ToString());
        if (!string.IsNullOrEmpty(fileItem.validation))
          xFile.SetAttribute(XML_ELEMENT_FILES_ITEMAttributeHash, fileItem.validation);
      }
      return xFile;
    }

    private XmlElement CreateFileList()
    {
      XmlElement xFiles = null;
      if (GetListCount(files) > 0)
      {
        xFiles = manifestDocument.CreateElement(XML_ELEMENT_FILES);
        foreach (Object obj in files)
        {
          XmlElement xEl = CreateFileItem(obj as FileItem);
          if (xEl != null)
            xFiles.AppendChild(xEl);
        }
      }
      return xFiles;
    }

    private XmlElement CreatePostUpdateCommand(PostUpdateCommand postUpdateCommand)
    {
      XmlElement xCom = null;
      if (postUpdateCommand != null)
      {
        xCom = manifestDocument.CreateElement(XML_ELEMENT_POST_UPDATE_COMMAND);
        xCom.SetAttribute(XML_ELEMENT_POST_UPDATE_COMMAND_ATTRIBUTE_EXECUTABLE, postUpdateCommand.executable);
        xCom.SetAttribute(XML_ELEMENT_POST_UPDATE_COMMAND_ATTRIBUTE_TARGETPATH, postUpdateCommand.targetpath);
        xCom.SetAttribute(XML_ELEMENT_POST_UPDATE_COMMAND_ATTRIBUTE_ARGUMENTS, postUpdateCommand.arguments);
        xCom.SetAttribute(XML_ELEMENT_POST_UPDATE_COMMAND_ATTRIBUTE_DELETE, postUpdateCommand.delete.ToString());
      }
      return xCom;
    }

    public int CreateManifestDocument(string path)
    {
      int r = ERROR_ROOTPATH_NOT_SPECIFIED;
      if (!string.IsNullOrEmpty(rootPath))
      {
        r = ERROR_PROCUCT_NOT_SPECIFIED;
        if (!string.IsNullOrEmpty(applicationId))
        {
          r = ERROR_PUBLISHER_NOT_SPECIFIED;
          if (!string.IsNullOrEmpty(publisherId))
          {
            r = ERROR_NEWVERSION_NOT_SPECIFIED;
            if (newApplicationVersion != null)
            {
              r = ERROR_UPDATELOCATION_NOT_SPECIFIED;
              if (!string.IsNullOrEmpty(globalUpdateLocation))
              {
                r = ERROR_ROOTPATH_NOT_FOUND;
                if (Directory.Exists(rootPath))
                {
                  r = ERROR_SUCCESS;
                  manifestDocument = new XmlDocument();
                  XmlProcessingInstruction xPI = manifestDocument.CreateProcessingInstruction("xml", XML_PROCESSING_INSTRUCTION + " encoding=\"" + System.Text.Encoding.UTF8.WebName + "\"");
                  manifestDocument.AppendChild(xPI);
                  XmlComment xComment = manifestDocument.CreateComment(XML_COMMENT);
                  manifestDocument.AppendChild(xComment);
                  XmlElement xElmntRoot = manifestDocument.CreateElement(XML_ELEMENT_ROOT);
                  xElmntRoot.SetAttribute(XML_ELEMENT_ROOT_ATTRIBUTE_APPLICATION_ID, applicationId);
                  xElmntRoot.SetAttribute(XML_ELEMENT_ROOT_ATTRIBUTE_PUBLISHER_ID, publisherId);
                  xElmntRoot.SetAttribute(XML_ELEMENT_ROOT_ATTRIBUTE_APPLICATION_PLATFORM, platform.ToString());
                  if (targetApplicationVersion != null)
                    xElmntRoot.SetAttribute(XML_ELEMENT_ROOT_ATTRIBUTE_APPLICATION_TARGETVERSION, targetApplicationVersion.ToString());
                  if (!string.IsNullOrEmpty(description))
                  {
                    XmlElement xDesc = manifestDocument.CreateElement(XML_ELEMENT_DESCRIPTION);
                    xDesc.InnerText = description;
                    xElmntRoot.AppendChild(xDesc);
                  }
                  string s = globalUpdateLocation;
                  if ((copyMethod == FileCopyMethod.http) || (copyMethod == FileCopyMethod.ftp))
                    s = s.Replace("\\", "/");
                  if (!s.StartsWith(FileCopyMethod.file.ToString() + "://", StringComparison.CurrentCultureIgnoreCase) && !s.StartsWith(FileCopyMethod.http.ToString() + "://", StringComparison.CurrentCultureIgnoreCase) && !s.StartsWith(FileCopyMethod.ftp.ToString() + "://", StringComparison.CurrentCultureIgnoreCase))
                  {
                    if ((copyMethod == FileCopyMethod.http) || (copyMethod == FileCopyMethod.ftp))
                      s = copyMethod.ToString() + "://" + s;
                  }
                  XmlElement xCopy = manifestDocument.CreateElement(XML_ELEMENT_UPDATE_LOCATION);
                  xCopy.InnerText = s;
                  xElmntRoot.AppendChild(xCopy);
                  xCopy = manifestDocument.CreateElement(XML_ELEMENT_NEW_APPLICATION_VERSION);
                  xCopy.InnerText = newApplicationVersion.ToString();
                  xElmntRoot.AppendChild(xCopy);
                  xCopy = CreatePostUpdateCommand(postUpdateCommand);
                  if (xCopy != null)
                  {
                    xElmntRoot.AppendChild(xCopy);
                    if (GetFileReferenceIndex(postUpdateCommand.executable) < 0)
                      AddFileReference(postUpdateCommand.executable, postUpdateCommand.targetpath);
                  }
                  xCopy = CreateFileList();
                  if (xCopy != null)
                    xElmntRoot.AppendChild(xCopy);
                  manifestDocument.AppendChild(xElmntRoot);
                  if (string.IsNullOrEmpty(path))
                    path = manifestFileName;
                  if (string.IsNullOrEmpty(path))
                    path = DEFAULT_MANIFEST_FILENAME;
                  manifestDocument.Save(path);
                }
              }
            }
          }
        }
      }
      return r;
    }
    public int CreateManifestDocument()
    {
      return CreateManifestDocument(null);
    }

    public void SetPostUpdateCommand(string executable, string arguments, string path, bool delete)
    {
      postUpdateCommand = new PostUpdateCommand(executable, arguments, path, delete);
    }

    public void SetPostUpdateCommand(string executable, string arguments, string path)
    {
      postUpdateCommand = new PostUpdateCommand(executable, arguments, path);
    }

    public void SetPostUpdateCommand(string executable, string path, bool delete)
    {
      postUpdateCommand = new PostUpdateCommand(executable, path, delete);
    }

    public void SetPostUpdateCommand(string executable, string path)
    {
      postUpdateCommand = new PostUpdateCommand(executable, path);
    }

    public void SetPostUpdateCommand(string executable, bool delete)
    {
      postUpdateCommand = new PostUpdateCommand(executable, delete);
    }

    public void SetPostUpdateCommand(string executable)
    {
      postUpdateCommand = new PostUpdateCommand(executable);
    }

    public int FileListCapacity
    {
      get
      {
        return GetListCapacity(files);
      }
      set
      {
        SetListCapacity(ref files, value);
      }
    }

    public int AddFileReference(string fullname, string path)
    {
      int r = -1;
      if (!string.IsNullOrEmpty(fullname))
      {
        if (files == null)
          files = new ArrayList();
        FileItem fi = new FileItem(fullname, path);
        if (useValidation)
        {
          string s = Path.Combine(rootPath, fullname);
          if (File.Exists(s))
          {
            FileInfo fl = new FileInfo(s);
            fi.size = fl.Length;
            byte[] hash;
            using (FileStream fs = fl.OpenRead())
            {
              SHA1 ha = new SHA1CryptoServiceProvider();
              hash = ha.ComputeHash(fs);
              fs.Close();
              ha.Clear();
            }
            fi.validation = Convert.ToBase64String(hash);
          }
        }
        r = files.Add(fi);
      }
      return r;
    }

    public int AddFileReference(string fullname)
    {
      return AddFileReference(fullname, null);
    }

    public int GetFileReferenceIndex(string fullname)
    {
      int r = -1;
      if (files != null)
      {
        int cc = files.Count;
        for (int i = 0; i < cc; ++i)
        {
          FileItem fi = files[i] as FileItem;
          if (string.Compare(fi.Name, fullname, true) == 0)
          {
            r = i;
            break;
          }
        }
      }
      return r;
    }

    public bool RemoveFileReference(int index)
    {
      bool r = (index >= 0) && (files != null) && (index < files.Count);
      if (r)
        files.RemoveAt(index);
      return r;
    }

    public bool RemoveFileReference(string fullname)
    {
      int r = GetFileReferenceIndex(fullname);
      return (r >= 0) && RemoveFileReference(r);
    }

    public void ClearFileReferences()
    {
      files = null;
    }

    public int AddAssemblyReference(string fullname, string path)
    {
      return -1;
    }
    public int AddAssemblyReference(string fullname)
    {
      return -1;
    }
    public int GetAssemblyReferenceIndex(string fullname)
    {
      return -1;
    }
    public bool RemoveAssemblyReference(int index)
    {
      return false;
    }
    public bool RemoveAssemblyReference(string fullname)
    {
      return false;
    }
    public void ClearAssemblyReferences()
    {
    }
    public int AssemblyListCapacity 
    {
      get
      {
        return 0;
      }
      set { }
    }

    public FileCopyMethod CopyMethod
    {
      get
      {
        return copyMethod;
      }
      set
      {
        copyMethod = value;
      }
    }

    public string UpdateLocation
    {
      get
      {
        return globalUpdateLocation;
      }
      set
      {
        globalUpdateLocation = value;
      }
    }

    public Version TargetApplicationVersion
    {
      get
      {
        return targetApplicationVersion;
      }
      set
      {
        targetApplicationVersion = value;
      }
    }

    public Version NewApplicationVersion
    {
      get
      {
        return newApplicationVersion;
      }
      set
      {
        newApplicationVersion = value;
      }
    }

    public string Product
    {
      get
      {
        return applicationId;
      }
      set
      {
        applicationId = value;
      }
    }

    public string Publisher
    {
      get
      {
        return publisherId;
      }
      set
      {
        publisherId = value;
      }
    }

    public string Description
    {
      get
      {
        return description;
      }
      set
      {
        description = value;
      }
    }

    public ApplicationPlatform Platform
    {
      get
      {
        return platform;
      }
      set
      {
        platform = value;
      }
    }

    public bool UseValidation
    {
      get
      {
        return useValidation;
      }
      set
      {
        useValidation = value;
      }
    }

    public int FileListLength
    {
      get
      {
        return GetListCount(files);
      }
    }

    public int AssemblyListLength
    {
      get
      {
        return 0;
      }
    }
  }
}
