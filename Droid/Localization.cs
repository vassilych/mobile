using System;
using Android.Content.Res;
using Java.Util;

namespace scripting.Droid
{
  public class Localization
  {
    public static string CurrentCode;

    public static Java.Util.Locale LocaleFromString(string voice, bool display = true)
    {
      string voice_ = voice.Replace("-", "_");
      int index = voice_.IndexOf('_');
      if (index <= 0) {
        return new Java.Util.Locale(voice_);
      }

      string language = voice_.Substring(0, index).ToLower();
      string country = voice_.Substring(index + 1).ToUpper();
      if (language.Equals("en")) {
        if (country == "US" || display) {
          return Java.Util.Locale.Us;
        }
        return Java.Util.Locale.Uk;
      }
      if (language.Equals("de")) {
        if (country == "DE" || display) {
          return display ? Java.Util.Locale.Germany : Java.Util.Locale.German;
        } else {
          return new Java.Util.Locale(language, country);
        }
      }
      if (language.Equals("fr")) {
        return display ? Java.Util.Locale.France : Java.Util.Locale.French;
      }
      if (language.Equals("it")) {
        return display ? Java.Util.Locale.Italy : Java.Util.Locale.Italian;
      }
      if (language.Equals("zh")) {
        return display ? Java.Util.Locale.China : Java.Util.Locale.Chinese;
      }
      if (language.Equals("ja")) {
        return display ? Java.Util.Locale.Japan : Java.Util.Locale.Japanese;
      }

      if (display || language.Equals("ar")) {
        return new Java.Util.Locale(language);
      } else {
        return new Java.Util.Locale(language, country);
      }
    }

    public static string VoiceFromLocale(Java.Util.Locale locale)
    {
      string voice = "";
      if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop) { // 5.0
        voice = locale.ToLanguageTag();
      } else { // Older Android version (< 5.0)
        voice = locale.ToString();
      }

      voice = AdjustVoiceString(voice);

      Console.WriteLine("--> VoiceFromLocale: {0} --> {1}", locale, voice);
      return voice.Replace('-', '_');
    }

    public static string CodeFromLocale(Java.Util.Locale locale)
    {
      string voice = VoiceFromLocale(locale);
      return voice.Substring(0, 2);
    }

    public static string AdjustVoiceString(string voice)
    {
      voice = voice.Replace('-', '_').Replace("__", "_");
      if (voice.Length == 2) {
        if (voice == "en") {
          voice = "en_US";
        } else if (voice == "es") {
          voice = "es_MX";
        } else if (voice == "zh") {
          voice = "zh_CN";
        } else if (voice == "ja") {
          voice = "ja_JP";
        } else if (voice == "pt") {
          voice = "pt_BR";
        } else if (voice == "ar") {
          voice = "ar_SA";
        } else {
          voice = voice + "_" + voice.ToUpper();
        }
      }
      if (voice.Length > 5) {
        voice = voice.Substring(0, 5);
      }

      return voice;
    }

    public static bool CheckCode()
    {
      string deviceCode = Localization.GetAppLanguageCode();
      string currentCode = Localization.CurrentCode;
      if (deviceCode != currentCode) {
        Localization.SetProgramLanguageCode(currentCode);
        return true;
      }
      return false;
    }
    public static string GetAppLanguageCode()
    {
      Configuration conf = MainActivity.TheView.Resources.Configuration;
      string code =  CodeFromLocale(conf.Locale);
      return code;
    }

    public static bool SetProgramLanguageCode(string voice = "")
    {
      Configuration conf = MainActivity.TheView.Resources.Configuration;

      if (string.IsNullOrWhiteSpace(voice)) {
        voice = VoiceFromLocale(conf.Locale);
      }

      Locale newLocale = LocaleFromString(voice);
      conf.Locale = newLocale;
      if (newLocale == null) {
        return false;
      }
      CurrentCode = voice;

      Console.WriteLine("SetUILanguage {0} from {1}", conf.Locale, voice);

      MainActivity.TheView.Resources.UpdateConfiguration(conf,
                   MainActivity.TheView.Resources.DisplayMetrics);
      return true;
    }

    public static string GetText(string key)
    {
      string stringId = key.Replace(" ", "_").Replace("'", "" ).Replace("!", "")
                           .Replace("?", "" ).Replace(";", "" ).Replace(":", "").Replace("-", "_")
                           .Replace(".", "_").Replace(",", "_").Replace("&", "and");

      int resId = MainActivity.TheView.Resources.GetIdentifier(
                  stringId, "string", MainActivity.TheView.PackageName);

      string localized = resId > 0 ? MainActivity.TheView.Resources.GetText(resId) : key;
      return localized;
    }
  }
}
