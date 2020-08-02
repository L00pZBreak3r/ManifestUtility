//If defined, my own manifest file format is used, otherwise DeployManifest is used
//#define UseMyXmlBasedManifest

using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using ManifestManagerLib;

namespace ManifestUtility
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private const string SETTING_ROOTFOLDER = "RootFolder";
    private const string SETTING_INCLUDEMASK = "IncludeMask";
    private const string SETTING_EXCLUDEMASK = "ExcludeMask";
    private const string SETTING_INCLUDESUBFOLDERS = "IncludeSubfolders";
    private const string SETTING_USEVALIDATION = "UseValidation";
    private const string SETTING_RUNCOMMAND = "RunCommand";
    private const string SETTING_RUNPATH = "RunPath";
    private const string SETTING_RUNARGUMENTS = "RunParamerters";
    private const string SETTING_DELETEAFTERRUN = "DeleteAfterRun";
    private const string SETTING_UPDATELOCATION = "UpdateLocation";
    private const string SETTING_COPYMETHOD = "CopyMethod";
    private const string SETTING_TARGETPATH = "TargetPath";
    private const string SETTING_PRODUCT_ID = "Product";
    private const string SETTING_PUBLISHER_ID = "Publisher";
    private const string SETTING_TARGETVERSION = "TargetVersion";
    private const string SETTING_NEWVERSION = "NewVersion";
    private const string SETTING_DESCRIPTION = "Description";
    private const string SETTING_PLATFORM = "Platform";
    private const string SETTING_MANIFEST_FILENAME = "ManifestFileName";

    private string[] includeList;
    private string[] excludeList;
    private string destinationPath;
    private IUpdateManifestFile manFile;

    private static readonly string[] RegExSpecChars = { ".", "+", "$", "^", "(", ")", "[", "]", "{", "}" };

    private static string[] BuildList(string s)
    {
      string[] r = null;
      if (!string.IsNullOrEmpty(s))
      {
        r = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      }
      return r;
    }

    private int BuildIncludeList()
    {
      includeList = BuildList(textBox2.Text);
      return (includeList != null) ? includeList.Length : 0;
    }

    private int BuildExcludeList()
    {
      excludeList = BuildList(textBox3.Text);
      return (excludeList != null) ? excludeList.Length : 0;
    }

    private static string ConvertMaskToRegex(string mask)
    {
      string r = mask;
      if (!string.IsNullOrEmpty(r))
      {
        r = r.Replace("\\", "");
        foreach (string s in RegExSpecChars)
          r = r.Replace(s, "\\" + s);
        r = r.Replace("?", ".?").Replace("*", ".*");
      }
      return r;
    }

    private static Version GetVersionFromString(string s)
    {
      Version r = null;
      if (!string.IsNullOrEmpty(s))
      {
        try
        {
          r = new Version(s);
        }
        catch { }
      }
      return r;
    }

    private void SaveSettings()
    {
      RegistryKey setts = null;
      try
      {
        setts = Registry.CurrentUser.CreateSubKey(Path.Combine("Software", Path.Combine(Application.CompanyName, Application.ProductName)));
      }
      catch { }
      if (setts != null)
      {
        setts.SetValue(SETTING_ROOTFOLDER, textBox1.Text);
        setts.SetValue(SETTING_INCLUDEMASK, textBox2.Text);
        setts.SetValue(SETTING_EXCLUDEMASK, textBox3.Text);
        setts.SetValue(SETTING_INCLUDESUBFOLDERS, (checkBox1.Checked) ? 1 : 0);
        setts.SetValue(SETTING_USEVALIDATION, (checkBox2.Checked) ? 1 : 0);
        setts.SetValue(SETTING_RUNCOMMAND, textBox9.Text);
        setts.SetValue(SETTING_RUNPATH, textBox10.Text);
        setts.SetValue(SETTING_RUNARGUMENTS, textBox5.Text);
        setts.SetValue(SETTING_DELETEAFTERRUN, (checkBox4.Checked) ? 1 : 0);
        setts.SetValue(SETTING_UPDATELOCATION, textBox8.Text);
        setts.SetValue(SETTING_COPYMETHOD, comboBox2.SelectedIndex);
        setts.SetValue(SETTING_TARGETPATH, textBox4.Text);
        setts.SetValue(SETTING_PRODUCT_ID, textBox11.Text);
        setts.SetValue(SETTING_PUBLISHER_ID, textBox6.Text);
        Version v = GetVersionFromString(maskedTextBox1.Text);
        setts.SetValue(SETTING_TARGETVERSION, (v != null) ? v.ToString() : null);
        v = GetVersionFromString(maskedTextBox2.Text);
        setts.SetValue(SETTING_NEWVERSION, (v != null) ? v.ToString() : null);
        setts.SetValue(SETTING_DESCRIPTION, textBox12.Text);
        setts.SetValue(SETTING_PLATFORM, comboBox3.SelectedIndex);
        setts.SetValue(SETTING_MANIFEST_FILENAME, textBox13.Text);
        setts.Close();
      }
    }

    private void ReadSettings()
    {
      RegistryKey setts = null;
      try
      {
        setts = Registry.CurrentUser.OpenSubKey(Path.Combine("Software", Path.Combine(Application.CompanyName, Application.ProductName)));
      }
      catch { }
      if (setts != null)
      {
        textBox1.Text = (string)setts.GetValue(SETTING_ROOTFOLDER, string.Empty);
        textBox2.Text = (string)setts.GetValue(SETTING_INCLUDEMASK, string.Empty);
        textBox3.Text = (string)setts.GetValue(SETTING_EXCLUDEMASK, string.Empty);
        checkBox1.Checked = (int)setts.GetValue(SETTING_INCLUDESUBFOLDERS, 0) != 0;
        checkBox2.Checked = (int)setts.GetValue(SETTING_USEVALIDATION, 0) != 0;
        textBox9.Text = (string)setts.GetValue(SETTING_RUNCOMMAND, string.Empty);
        textBox10.Text = (string)setts.GetValue(SETTING_RUNPATH, string.Empty);
        textBox5.Text = (string)setts.GetValue(SETTING_RUNARGUMENTS, string.Empty);
        checkBox4.Checked = (int)setts.GetValue(SETTING_DELETEAFTERRUN, 0) != 0;
        textBox8.Text = (string)setts.GetValue(SETTING_UPDATELOCATION, string.Empty);
        comboBox2.SelectedIndex = (int)setts.GetValue(SETTING_COPYMETHOD, 0);
        textBox4.Text = (string)setts.GetValue(SETTING_TARGETPATH, string.Empty);
        textBox11.Text = (string)setts.GetValue(SETTING_PRODUCT_ID, string.Empty);
        textBox6.Text = (string)setts.GetValue(SETTING_PUBLISHER_ID, string.Empty);
        maskedTextBox1.Text = (string)setts.GetValue(SETTING_TARGETVERSION, string.Empty);
        maskedTextBox2.Text = (string)setts.GetValue(SETTING_NEWVERSION, string.Empty);
        textBox12.Text = (string)setts.GetValue(SETTING_DESCRIPTION, string.Empty);
        comboBox3.SelectedIndex = (int)setts.GetValue(SETTING_PLATFORM, 0);
        textBox13.Text = (string)setts.GetValue(SETTING_MANIFEST_FILENAME, string.Empty);
        setts.Close();
      }
    }

    private void ProcessDirectory(DirectoryInfo directoryInfo)
    {
      if (directoryInfo != null)
      {
        int rootDirLen = directoryInfo.FullName.Length;
        foreach (string mask in includeList)
        {
          FileInfo[] fls = directoryInfo.GetFiles(mask, (checkBox1.Checked) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
          foreach (FileInfo fl in fls)
          {
            bool bInc = true;
            if (excludeList != null)
              foreach (string excl in excludeList)
              {
                string m1 = ConvertMaskToRegex(excl);
                if (Regex.IsMatch(fl.Name, m1))
                {
                  bInc = false;
                  break;
                }
              }
            if (bInc)
            {
              string loc = fl.DirectoryName.Substring(rootDirLen);
              if (loc.StartsWith(Path.DirectorySeparatorChar.ToString()))
                loc = loc.Substring(1);
              manFile.AddFileReference(Path.Combine(loc, fl.Name), destinationPath);
            }
          }
          if (mask == "*")
            break;
        }
      }
    }

    private FileCopyMethod GetCopyMethod()
    {
      int i = comboBox2.SelectedIndex;
      if (i < 0)
        i = 0;
      return (FileCopyMethod)i;
    }

    private static bool WildcardAllExists(string s)
    {
      return s == "*";
    }

    private void ProcessRootFolder()
    {
      manFile.ClearFileReferences();
      string s = textBox1.Text;
      if (!string.IsNullOrEmpty(s) && Directory.Exists(s))
      {
        destinationPath = textBox4.Text;
        manFile.UseValidation = checkBox2.Checked;
        if ((BuildExcludeList() != 0) && Array.Exists(excludeList, WildcardAllExists))
          return;
        if (BuildIncludeList() == 0)
          includeList = new string[] { "*" };
        ProcessDirectory(new DirectoryInfo(s));
        manFile.CopyMethod = GetCopyMethod();
        manFile.UpdateLocation = textBox8.Text;
        SetPostUpdateCommand();
        manFile.Product = textBox11.Text;
        manFile.Publisher = textBox6.Text;
        manFile.Description = textBox12.Text;
        manFile.TargetApplicationVersion = GetVersionFromString(maskedTextBox1.Text);
        manFile.NewApplicationVersion = GetVersionFromString(maskedTextBox2.Text);
        manFile.Platform = (ApplicationPlatform)comboBox3.SelectedIndex;
      }
    }

    private bool SetPostUpdateCommand()
    {
      bool r = !string.IsNullOrEmpty(textBox9.Text);
      if (r)
        manFile.SetPostUpdateCommand(textBox9.Text, textBox5.Text, textBox10.Text, checkBox4.Checked);
      return r;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog fbd = new FolderBrowserDialog();
      fbd.ShowNewFolderButton = false;
      fbd.Description = "Select an update source folder:";
      if (!string.IsNullOrEmpty(textBox1.Text))
        fbd.SelectedPath = textBox1.Text;
      if (fbd.ShowDialog() == DialogResult.OK)
        textBox1.Text = fbd.SelectedPath;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      ToolTip maskToopTip = new ToolTip();
      maskToopTip.SetToolTip(textBox2, "File names and/or file masks separated by semicolon (;)");
      maskToopTip.SetToolTip(textBox3, "File names and/or file masks separated by semicolon (;)");
      comboBox2.SelectedIndex = comboBox3.SelectedIndex = 0;
      ReadSettings();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      string s1 = textBox1.Text;
      string s2 = textBox13.Text;
      if (!string.IsNullOrEmpty(s1) && !string.IsNullOrEmpty(s2))
      {
        button2.Enabled = false;
        Refresh();
        SaveSettings();
#if UseMyXmlBasedManifest
        manFile = new UpdateManifestFile1(s1, s2);
#else
        manFile = new UpdateManifestFile2(s1, s2);
#endif
        ProcessRootFolder();
        int r = manFile.CreateManifestDocument();
        button2.Enabled = true;
#if UseMyXmlBasedManifest
        if (r == UpdateManifestFile1.ERROR_SUCCESS)
#else
        if (r == UpdateManifestFile2.ERROR_SUCCESS)
#endif
          MessageBox.Show("Manifest file created successfully.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        else
          MessageBox.Show("Can't create manifest file.\nError: " + r.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}