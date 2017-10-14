using System;
using Foundation;

namespace scripting.iOS
{
  public class Settings
  {
    static NSUserDefaults g_prefs = NSUserDefaults.StandardUserDefaults;

    static public void Init()
    {
      /*var defaults = NSDictionary.FromObjectsAndKeys(
                new object[] { true, 0.50, "en-US", true, false, true },
          new object[] { "sound", "ttsSpeed", "voice", "showAds", "speech", "wordsOrPhrases" }
      );
      NSUserDefaults.StandardUserDefaults.RegisterDefaults(defaults);*/
    }
    static public void SaveSetting(string name, string val)
    {
      g_prefs.SetString(val, name);
      g_prefs.Synchronize();
      //Console.WriteLine("Saved {0} --> {1}", name, value);
    }
    static public void SaveSetting(string name, int val)
    {
      g_prefs.SetInt(val, name);
      g_prefs.Synchronize();
    }
    static public void SaveSetting(string name, bool val)
    {
      g_prefs.SetBool(val, name);
      g_prefs.Synchronize();
    }
    static public void SaveSetting(string name, float val)
    {
      g_prefs.SetFloat(val, name);
      g_prefs.Synchronize();
    }

    static public string GetSetting(string name, string defValue = null)
    {
      string res = g_prefs.StringForKey(name);
      if (string.IsNullOrEmpty(res)) {
        res = defValue;
      }
      return res;
    }
    static public bool GetBoolSetting(string name, bool defValue = false)
    {
      var isSet = g_prefs[name];
      if (isSet == null) {
        return defValue;
      }
      bool res = g_prefs.BoolForKey(name);
      return res;
    }
    static public int GetIntSetting(string name, int defValue = 0)
    {
      int res = (int)g_prefs.IntForKey(name);
      if (res == 0 && defValue != 0) {
        res = defValue;
      }
      return res;
    }
    static public float GetFloatSetting(string name, float defValue = 0)
    {
      float res = g_prefs.FloatForKey(name);
      if (res == 0.0 && defValue != 0.0) {
        res = defValue;
      }
      return res;
    }
  }
}
