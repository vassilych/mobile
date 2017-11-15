using System;
using Android.App;
using Android.Content;

namespace scripting.Droid
{
  public class Settings
  {
    static ISharedPreferences g_prefs = Application.Context.GetSharedPreferences(
    Application.Context.ApplicationInfo.PackageName, FileCreationMode.Private);

    static public void SaveSetting(string name, string value)
    {
      var prefEditor = g_prefs.Edit();
      prefEditor.PutString(name, value);
      prefEditor.Commit();
      //Console.WriteLine("Saved {0} --> {1}", name, value);
    }
    static public void SaveSetting(string name, int value)
    {
      var prefEditor = g_prefs.Edit();
      prefEditor.PutInt(name, value);
      prefEditor.Commit();
    }
    static public void SaveSetting(string name, bool value)
    {
      var prefEditor = g_prefs.Edit();
      prefEditor.PutBoolean(name, value);
      prefEditor.Commit();
    }
    static public void SaveSetting(string name, float value)
    {
      var prefEditor = g_prefs.Edit();
      prefEditor.PutFloat(name, value);
      prefEditor.Commit();
    }

    static public string GetSetting(string name, string defValue = null)
    {
      string res = g_prefs.GetString(name, defValue);
      Console.WriteLine("GetSetting {0}: {1} --> {2}",
                Application.Context.ApplicationInfo.PackageName, name, res);
      return res;
    }
    static public int GetIntSetting(string name, int defValue = -1)
    {
      int res = g_prefs.GetInt(name, defValue);
      Console.WriteLine("GetIntSetting {0}: {1} --> {2}",
                Application.Context.ApplicationInfo.PackageName, name, res);
      return res;
    }
    static public bool GetBoolSetting(string name, bool defValue = false)
    {
      bool res = g_prefs.GetBoolean(name, defValue);
      Console.WriteLine("GetBoolSetting {0}: {1} --> {2}",
                Application.Context.ApplicationInfo.PackageName, name, res);
      return res;
    }
    static public float GetFloatSetting(string name, float defValue = -1)
    {
      var res = g_prefs.GetFloat(name, defValue);
      Console.WriteLine("GetFloatSetting {0}: {1} --> {2}",
                Application.Context.ApplicationInfo.PackageName, name, res);
      return res;
    }
  }
}
