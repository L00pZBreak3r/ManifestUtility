using System;

namespace ManifestManagerLib
{
  class PostUpdateCommand
  {
    public string executable;
    public string arguments;
    public string targetpath;
    public bool delete;
    public PostUpdateCommand(string exec, string args, string path, bool del)
    {
      executable = exec;
      arguments = args;
      targetpath = path;
      delete = del;
    }
    public PostUpdateCommand(string exec, string args, string path) : this(exec, args, path, false) { }
    public PostUpdateCommand(string exec, string path, bool del) : this(exec, null, path, del) { }
    public PostUpdateCommand(string exec, string path) : this(exec, null, path, false) { }
    public PostUpdateCommand(string exec, bool del) : this(exec, null, null, del) { }
    public PostUpdateCommand(string exec) : this(exec, null, null, false) { }
  }
}
