using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Speech.Tts;

namespace scripting.Droid
{
  public class TTS : Java.Lang.Object, TextToSpeech.IOnInitListener, IDisposable
  {
    public static readonly int TTS_INSTALLED_DATA = 101;
    public static readonly int TTS_CHECK_DATA     = 102;
    public static readonly int TTS_REQUEST_DATA   = 103;
    static Context m_context;

    static public bool   Sound { set; get; }           = true;
    static public float  SpeechRate { set; get; }      = 1.0f;
    static public float  Volume { set; get; }          = 0.7f;
    static public float  PitchMultiplier { set; get; } = 1.0f;
    static public string Voice { set; get; }           = "en-US";

    static bool m_initDone;

    static List<string> m_initVoices = new List<string>() {
        "en_US", "es_MX" };

    public static void Init(Context context)
    {
      if (m_initDone) {
        return;
      }
      m_initDone = true;
      m_context = context;
    }

    TextToSpeech m_textToSpeech;
    Java.Util.Locale m_language = Java.Util.Locale.English;
    string m_text = "";
    bool m_force;

    static Dictionary<string, TTS> g_tts = new Dictionary<string, TTS>();

    public static TTS GetTTS(string voice)
    {
      TTS tts;
      if (g_tts.TryGetValue(voice, out tts))
      {
        return tts;
      }
      tts = new TTS(voice);
      g_tts[voice] = tts;
      return tts;
    }
    public static void InitVoices()
    {
      foreach (string voice in m_initVoices)
      {
        g_tts[voice] = new TTS(voice);
      }
    }

    TTS(string voice)
    {
      m_language = UtilsDroid.LocaleFromString(voice, false);

      m_textToSpeech = new TextToSpeech(Application.Context, this, "com.google.android.tts");
      m_textToSpeech.SetLanguage(m_language);

      Console.WriteLine("Initializing TTS {0} --> {1}", voice, m_language);
    }

    public ICollection<Java.Util.Locale> LanguagesAvailable()
    {
      return m_textToSpeech.AvailableLanguages;
    }

    /*public static void Speak(string text, bool force = false)
    {
      if (!force && !Sound) {
        return;
      }
    }*/

    public void Speak(string text, bool force = false)
    {
      m_text = text;

      m_textToSpeech.Stop();

      m_force = force;
      if (!Sound && !force) {
        return;
      }

      Console.WriteLine("Speak TTS {0} {1} {2} --> {3}. Available: {4}",
                        m_language, SpeechRate, m_textToSpeech.Language, text,
                        m_textToSpeech.IsLanguageAvailable(m_language));
      m_textToSpeech.SetLanguage(m_language);
      m_textToSpeech.SetSpeechRate(SpeechRate);
      m_textToSpeech.SetPitch(PitchMultiplier);

      if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
      { // 5.0, API 21
        string utteranceId = m_text.GetHashCode() + "";
        m_textToSpeech.Speak(m_text, QueueMode.Flush, null, utteranceId);
      }
      else
      {
        m_textToSpeech.Speak(m_text, QueueMode.Flush, null);
      }
    }

    void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
    {
      if (!Sound && !m_force)
      {
        return;
      }
      // if we get an error, default to the default language
      if (status == OperationResult.Error)
      {
        m_textToSpeech.SetLanguage(Java.Util.Locale.Default);
        Console.WriteLine("TTS ERROR [{0}] for [{1}]: Default Language: {2}",
                          m_language, m_text, m_textToSpeech.Language);
      }
      // if the listener is ok, set the lang
      if (status == OperationResult.Success)
      {
        m_textToSpeech.SetLanguage(m_language);
      }
      Console.WriteLine("TTS [{0}]: {1}, {2}, {3} ({4})", m_text, m_language,
         m_language.Language, m_language.Country, status == OperationResult.Success);

      m_force = false;
    }

    public static void OnTTSResult(int req, Result res, Intent data)
    {
      if (data == null)
      {
        TTS.InitVoices();
        return;
      }

      if (req == TTS_CHECK_DATA || req == TTS_INSTALLED_DATA)
      {
        IList<string> availableLanguages = data.GetStringArrayListExtra(TextToSpeech.Engine.ExtraAvailableVoices);
        IList<string> unavailableLanguages = data.GetStringArrayListExtra(TextToSpeech.Engine.ExtraUnavailableVoices);

        Console.WriteLine("TTS {0} available: ", availableLanguages.Count);
        Array.ForEach<string>(availableLanguages.ToArray(), (av) => Console.Write("{0} ", av));
        Console.WriteLine("\nTTS {0} unavailable: ", unavailableLanguages.Count);
        Array.ForEach<string>(unavailableLanguages.ToArray(), (un) => Console.Write("{0} ", un));

        TTS.InitVoices();

        if (availableLanguages.Count <= 1 && m_context != null)
        {
          Console.WriteLine("---> Performing TTS Install");
          var installTTS = new Intent();
          installTTS.SetAction(TextToSpeech.Engine.ActionInstallTtsData);
          m_context.StartActivity(installTTS);
        }
      }
      if (req == TTS_REQUEST_DATA)
      {
        Console.WriteLine("---> TTS INSTALL Result {0} {1}", res, data);
      }
    }
  }
}
