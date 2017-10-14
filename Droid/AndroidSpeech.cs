using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Speech;


namespace scripting.Droid
{
  public delegate void OnVoiceRecognition(string status, string recognized);

  public class STT
  {
    public static int STT_REQUEST = 10;
    public static int STT_PERMISSIONS_REQUEST = 77;

    static SpeechRecognizer m_speechRecognizer;
    static Intent m_speechRecognizerIntent;

    static string m_voice = "en-US";
    public static string Voice {
      get {
        return m_voice;
      }
      set {
        m_voice = value.Replace('_', '-');
      }
    }

    static Intent InitSpeechIntent(string voice, string prompt = "")
    {
      var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
      voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

      // put a message on the modal dialog
      if (!string.IsNullOrWhiteSpace(prompt)) {
        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, prompt);
      }

      // if there is more then 1.5s of silence, consider the speech over
      //voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
      //voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
      //voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 2000);
      voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 10);

      voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
      //voiceIntent.PutExtra(RecognizerIntent.ExtraCallingPackage, PackageName);

      voiceIntent.PutExtra("android.speech.extra.EXTRA_ADDITIONAL_LANGUAGES", new string[] { });
      voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, voice);
      voiceIntent.PutExtra(RecognizerIntent.ExtraLanguagePreference, voice);
      voiceIntent.PutExtra(RecognizerIntent.ExtraOnlyReturnLanguagePreference, voice);

      return voiceIntent;
    }

    static bool m_microhoneChecked = false;
    static string CheckMicrophone()
    {
      if (m_microhoneChecked) {
        return null;
      }
      IList<ResolveInfo> activities = MainActivity.TheView.PackageManager.QueryIntentActivities(
          new Intent(RecognizerIntent.ActionRecognizeSpeech), 0);
      string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
      if (rec != "android.hardware.microphone" || activities == null ||
          activities.Count == 0) {
        return "No microphone found in the system";
      }
      m_microhoneChecked = true;
      return null;
    }

    public static string StartVoiceRecognition(string prompt = "")
    {
      string check = CheckMicrophone();
      if (check != null) {
        return check;
      }

      m_speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(MainActivity.TheView);
      m_speechRecognizerIntent = InitSpeechIntent(Voice, prompt);

      string serviceComponent = Android.Provider.Settings.Secure.GetString(MainActivity.TheView.ContentResolver,
                                      "voice_recognition_service");
      Console.WriteLine("--> serviceComponent: [{0}]", serviceComponent);
      //"com.google.android.googlequicksearchbox/com.google.android.voicesearch.serviceapi.GoogleRecognitionService";
      //if (false && !string.IsNullOrWhiteSpace(serviceComponent)) {
      //    m_speechRecognizer.StartListening(m_speechRecognizerIntent);
      //}
      MainActivity.TheView.StartActivityForResult(m_speechRecognizerIntent, STT_REQUEST);
      return null;
    }

    public static OnVoiceRecognition VoiceRecognitionDone;

    public static void SpeechRecognitionCompleted(Result resultCode, Intent data)
    {
      Console.WriteLine("--> SpeechRecognitionCompleted {0}", resultCode);

      if (resultCode == Result.Ok && data != null) {
        var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
        if (matches.Count != 0) {
          string recognized = matches[0];
          Console.WriteLine("--> Recognized: {0}", recognized);
          VoiceRecognitionDone?.Invoke("", recognized);
          return;
        }
      }
      VoiceRecognitionDone?.Invoke("No speech was recognized", "");
    }
  }
}
