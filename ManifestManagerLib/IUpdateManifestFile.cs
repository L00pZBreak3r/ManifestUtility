using System;

namespace ManifestManagerLib
{
  public enum FileCopyMethod
  {
    file,
    http,
    ftp
  };

  public enum ApplicationPlatform
  {
    Any,
    x86,
    x64,
    ia64
  };

  public interface IUpdateManifestFile
  {
    int CreateManifestDocument(string path);
    int CreateManifestDocument();

    void SetPostUpdateCommand(string executable, string arguments, string path, bool delete);
    void SetPostUpdateCommand(string executable, string arguments, string path);
    void SetPostUpdateCommand(string executable, string path, bool delete);
    void SetPostUpdateCommand(string executable, string path);
    void SetPostUpdateCommand(string executable, bool delete);
    void SetPostUpdateCommand(string executable);

    int AddFileReference(string fullname, string path);
    int AddFileReference(string fullname);
    int GetFileReferenceIndex(string fullname);
    bool RemoveFileReference(int index);
    bool RemoveFileReference(string fullname);
    void ClearFileReferences();
    int FileListCapacity { get; set; }
    int FileListLength { get; }

    int AddAssemblyReference(string fullname, string path);
    int AddAssemblyReference(string fullname);
    int GetAssemblyReferenceIndex(string fullname);
    bool RemoveAssemblyReference(int index);
    bool RemoveAssemblyReference(string fullname);
    void ClearAssemblyReferences();
    int AssemblyListCapacity { get; set; }
    int AssemblyListLength { get; }

    FileCopyMethod CopyMethod { get; set; }
    string UpdateLocation { get; set; }
    Version TargetApplicationVersion { get; set; }
    Version NewApplicationVersion { get; set; }
    string Product { get; set; }
    string Publisher { get; set; }
    string Description { get; set; }
    ApplicationPlatform Platform { get; set; }
    bool UseValidation { get; set; }
  }
}
