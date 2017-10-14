using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace scripting.iOS
{
  public class Localization
  {
    static NSBundle m_bundle = NSBundle.MainBundle;

    static Dictionary<string, Dictionary<string, string>> m_announce =
        new Dictionary<string, Dictionary<string, string>>();
    static string m_voice;
    static string m_voiceNative;
    static string m_voice_;
    static string m_voiceNative_;
    static string m_deviceLanguage;

    public static string Voice {
      set {
        m_voice = value.Replace('_', '-');
        m_voice_ = value.Replace('-', '_');
      }
      get { return m_voice; }
    }
    public static string VoiceNative {
      set {
        m_voiceNative = value.Replace('_', '-');
        m_voiceNative_ = value.Replace('-', '_');
      }
      get { return m_voiceNative; }
    }
    public static string Voice_ {
      get { return m_voice_; }
    }
    public static string VoiceNative_ {
      get { return m_voiceNative_; }
    }
    public static string DeviceLanguageCode {
      set {
        m_deviceLanguage = value;
        if (m_deviceLanguage == "zh") {
          m_deviceLanguage = "zh-Hans";
        }
      }
      get { return m_deviceLanguage; }
    }

    public static string GetText(string key)
    {
      string text = m_bundle.LocalizedString(key, key);
      if (text == key && DeviceLanguageCode != "en" && !key.Contains(":")) {
        text = m_bundle.LocalizedString(key + ":", key).Replace(":", "");
      }
      return text;
    }

    public static string GetDeviceLangCode()
    {
      string deviceLangCode = "";//Settings.GetSetting("menuCode", "");
      if (!string.IsNullOrEmpty(deviceLangCode)) {
        return deviceLangCode;
      }

      var languages = NSBundle.MainBundle.PreferredLocalizations;
      if (languages == null || languages.Length == 0) {
        languages = NSLocale.PreferredLanguages;
      }

      deviceLangCode = languages != null && languages.Length > 0 ?
                       languages[0].Substring(0, 2) : "en";
      return deviceLangCode;
    }
    public static bool SetProgramLanguageCode(string langCode)
    {
      var path = NSBundle.MainBundle.PathForResource(langCode, "lproj");
      if (string.IsNullOrEmpty(path)) {
        // Translations not found.. no change
        Console.WriteLine("Translations not found for {0}", langCode);
        return false;
      }
      m_bundle = NSBundle.FromPath(path);
      return true;
    }
  }
}
