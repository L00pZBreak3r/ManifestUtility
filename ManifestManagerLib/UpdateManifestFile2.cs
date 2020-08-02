/* This class puts all information into DeployManifest.
 * 
 * Update destination for every file is stored in group attribute.
 * 
 * I couldn't make DeployManifest class save EntryPoint to xml file,
 * so I used SupportUrl attribute as a storage for post processor executable.
 * Post processor executable information is stored in a string of the following form:
 * <executable source path>;<update destination>;<command line arguments>;<delete after run flag>
 * 
 * File transfer type is stored as a prefix of update location.
 * */

using System;
using System.IO;
using System.Collections;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;

namespace ManifestManagerLib
{
  public class UpdateManifestFile2 : ArrayListHelperBase, IUpdateManifestFile
  {
    private ArrayList files;
    private ArrayList assemblies;
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

    private const string DEFAULT_MANIFEST_FILENAME = "update.manifest";

    public const int ERROR_SUCCESS = 0;
    public const int ERROR_ROOTPATH_NOT_SPECIFIED = -1;
    public const int ERROR_PROCUCT_NOT_SPECIFIED = -2;
    public const int ERROR_PUBLISHER_NOT_SPECIFIED = -3;
    public const int ERROR_UPDATELOCATION_NOT_SPECIFIED = -4;
    public const int ERROR_NEWVERSION_NOT_SPECIFIED = -5;
    public const int ERROR_ROOTPATH_NOT_FOUND = -6;

    public UpdateManifestFile2(string rootpath, string filename)
    {
      rootPath = rootpath;
      manifestFileName = filename;
    }
    public UpdateManifestFile2(string rootpath) : this(rootpath, null) { }

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
                  DeployManifest appMan = new DeployManifest();
                  if (!string.IsNullOrEmpty(description))
                    appMan.Description = description;
                  appMan.Product = applicationId;
                  appMan.Publisher = publisherId;
                  appMan.AssemblyIdentity.Name = applicationId;
                  appMan.AssemblyIdentity.ProcessorArchitecture = platform.ToString();
                  if (targetApplicationVersion != null)
                    appMan.MinimumRequiredVersion = targetApplicationVersion.ToString();
                  appMan.AssemblyIdentity.Version = newApplicationVersion.ToString();
                  string s1 = globalUpdateLocation;
                  if ((copyMethod == FileCopyMethod.http) || (copyMethod == FileCopyMethod.ftp))
                    s1 = s1.Replace("\\", "/");
                  if (!s1.StartsWith(FileCopyMethod.file.ToString() + "://", StringComparison.CurrentCultureIgnoreCase) && !s1.StartsWith(FileCopyMethod.http.ToString() + "://", StringComparison.CurrentCultureIgnoreCase) && !s1.StartsWith(FileCopyMethod.ftp.ToString() + "://", StringComparison.CurrentCultureIgnoreCase))
                  {
                    if ((copyMethod == FileCopyMethod.http) || (copyMethod == FileCopyMethod.ftp))
                      s1 = copyMethod.ToString() + "://" + s1;
                  }
                  appMan.DeploymentUrl = s1;
                  if ((postUpdateCommand != null) && !string.IsNullOrEmpty(postUpdateCommand.executable))
                  {
                    appMan.SupportUrl = postUpdateCommand.executable + ";" + ((postUpdateCommand.targetpath == null) ? string.Empty : postUpdateCommand.targetpath) + ";" + ((postUpdateCommand.arguments == null) ? string.Empty : postUpdateCommand.arguments) + ";" + postUpdateCommand.delete.ToString();
                    if (GetFileReferenceIndex(postUpdateCommand.executable) < 0)
                      AddFileReference(postUpdateCommand.executable, postUpdateCommand.targetpath);
                  }
                  if (GetListCount(files) > 0)
                    foreach (Object obj in files)
                      appMan.FileReferences.Add(obj as FileReference);
                  if (GetListCount(assemblies) > 0)
                    foreach (Object obj in assemblies)
                      appMan.AssemblyReferences.Add(obj as AssemblyReference);
                  s1 = manifestFileName;
                  if (string.IsNullOrEmpty(s1))
                    s1 = Path.Combine(rootPath, DEFAULT_MANIFEST_FILENAME);
                  else
                    if (!s1.Contains(Path.DirectorySeparatorChar.ToString()) && !s1.Contains(Path.AltDirectorySeparatorChar.ToString()))
                      s1 = Path.Combine(rootPath, s1);
                  appMan.SourcePath = s1;
                  appMan.ResolveFiles(new string[] { rootPath });
                  if (useValidation)
                    appMan.UpdateFileInfo();
                  if (string.IsNullOrEmpty(path))
                    ManifestWriter.WriteManifest(appMan);
                  else
                    ManifestWriter.WriteManifest(appMan, path);
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

    private static int AddBaseReference(ref ArrayList list, BaseReference baseRef)
    {
      if ((baseRef != null) && !string.IsNullOrEmpty(baseRef.SourcePath))
      {
        if (list == null)
          list = new ArrayList();
        return list.Add(baseRef);
      }
      return -1;
    }

    private static int AddBaseReference(ref ArrayList list, bool isAssembly, string fullname, string path)
    {
      if (!string.IsNullOrEmpty(fullname))
      {
        if (list == null)
          list = new ArrayList();
        BaseReference fr;
        if (isAssembly)
          fr = new AssemblyReference(fullname);
        else
          fr = new FileReference(fullname);
        if (!string.IsNullOrEmpty(path))
          fr.Group = path;
        return list.Add(fr);
      }
      return -1;
    }

    private static int AddBaseReference(ref ArrayList list, bool isAssembly, string fullname)
    {
      return AddBaseReference(ref list, isAssembly, fullname, null);
    }

    private static int GetBaseReferenceIndex(ArrayList list, string fullname)
    {
      int r = -1;
      if ((list != null) && !string.IsNullOrEmpty(fullname))
      {
        int cc = list.Count;
        for (int i = 0; i < cc; ++i)
        {
          BaseReference fi = list[i] as BaseReference;
          if (string.Compare(fi.SourcePath, fullname, true) == 0)
          {
            r = i;
            break;
          }
        }
      }
      return r;
    }

    private static bool RemoveBaseReference(ArrayList list, int index)
    {
      bool r = (index >= 0) && (list != null) && (index < list.Count);
      if (r)
        list.RemoveAt(index);
      return r;
    }

    private static bool RemoveBaseReference(ArrayList list, string fullname)
    {
      int r = GetBaseReferenceIndex(list, fullname);
      return (r >= 0) && RemoveBaseReference(list, r);
    }

    private static BaseReference GetBaseReference(ArrayList list, int index)
    {
      return ((index >= 0) && (list != null) && (index < list.Count)) ? list[index] as BaseReference : null;
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
      return AddBaseReference(ref files, false, fullname, path);
    }

    public int AddFileReference(string fullname)
    {
      return AddFileReference(fullname, null);
    }

    public int GetFileReferenceIndex(string fullname)
    {
      return GetBaseReferenceIndex(files, fullname);
    }

    public bool RemoveFileReference(int index)
    {
      return RemoveBaseReference(files, index);
    }

    public bool RemoveFileReference(string fullname)
    {
      return RemoveBaseReference(files, fullname);
    }

    public void ClearFileReferences()
    {
      files = null;
    }

    public int AssemblyListCapacity
    {
      get
      {
        return GetListCapacity(assemblies);
      }
      set
      {
        SetListCapacity(ref assemblies, value);
      }
    }

    public int AddAssemblyReference(string fullname, string path)
    {
      return AddBaseReference(ref assemblies, true, fullname, path);
    }

    public int AddAssemblyReference(string fullname)
    {
      return AddAssemblyReference(fullname, null);
    }

    public int GetAssemblyReferenceIndex(string fullname)
    {
      return GetBaseReferenceIndex(assemblies, fullname);
    }

    public bool RemoveAssemblyReference(int index)
    {
      return RemoveBaseReference(assemblies, index);
    }

    public bool RemoveAssemblyReference(string fullname)
    {
      return RemoveBaseReference(assemblies, fullname);
    }

    public void ClearAssemblyReferences()
    {
      assemblies = null;
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
        return GetListCount(assemblies);
      }
    }
  }
}
