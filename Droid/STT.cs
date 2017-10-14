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
    //public delegate void OnVoiceRecognition(string status, string recognized);

    public partial class MainActivity
    {
        private readonly int STT_REQUEST = 10;
        private readonly int STT_PERMISSIONS_REQUEST = 77;

        SpeechRecognizer m_speechRecognizer;
        Intent m_speechRecognizerIntent;
        
        static string m_voice = "en-US";
        public static string Voice {
            get {
                return m_voice;
            }
            set {
                m_voice = value.Replace('_', '-');
            }
        }

        void InitSpeechRecognizer(string voice)
        {
            m_speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(this);
            m_speechRecognizerIntent = InitSpeechIntent(voice);

            SpeechRecognitionListener listener = new SpeechRecognitionListener(m_speechRecognizer,
                                                                               m_speechRecognizerIntent);
            m_speechRecognizer.SetRecognitionListener(listener);
        }

        Intent InitSpeechIntent(string voice, string prompt = "")
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

            voiceIntent.PutExtra("android.speech.extra.EXTRA_ADDITIONAL_LANGUAGES", new string[] {});
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, voice);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguagePreference, voice);
            voiceIntent.PutExtra(RecognizerIntent.ExtraOnlyReturnLanguagePreference, voice);

            return voiceIntent;
        }

        static bool m_microhoneChecked = false;
        string CheckMicrophone()
        {
            if (m_microhoneChecked) {
                return null;
            }
            IList<ResolveInfo> activities = PackageManager.QueryIntentActivities(
                new Intent(RecognizerIntent.ActionRecognizeSpeech), 0);
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone" || activities == null ||
                activities.Count == 0) {
                return "No microhone found in the system";
            }
            m_microhoneChecked = true;
            return null;
        }

        public string PrepareSpeech(string voice)
        {
            string check = CheckMicrophone();
            if (check != null) {
                return check;
            }

            InitSpeechRecognizer(voice);
            return null;
        }

        bool GetPermission()
        {
            if (CheckCallingOrSelfPermission(Android.Manifest.Permission.RecordAudio)
                == Permission.Granted) {
                return true;
            }
            RequestPermissions(new string[] { Android.Manifest.Permission.RecordAudio  },
                                              STT_PERMISSIONS_REQUEST);
            return false;
        }

        public void StartVoiceRecognition(string prompt = "")
        {
            m_speechRecognizerIntent = InitSpeechIntent(Voice, prompt);

            string serviceComponent = Android.Provider.Settings.Secure.GetString(ContentResolver,
                                            "voice_recognition_service");
            Console.WriteLine("--> serviceComponent: [{0}]", serviceComponent);
            //"com.google.android.googlequicksearchbox/com.google.android.voicesearch.serviceapi.GoogleRecognitionService";
            //if (false && !string.IsNullOrWhiteSpace(serviceComponent)) {
            //    m_speechRecognizer.StartListening(m_speechRecognizerIntent);
            //}
            StartActivityForResult(m_speechRecognizerIntent, STT_REQUEST);
        }
        public OnVoiceRecognition VoiceRecognitionDone;

        void SpeechRecognitionCompleted(Result resultCode, Intent data)
        {
            Console.WriteLine("--> SpeechRecognitionCompleted {0}", resultCode);

            if (resultCode == Result.Ok && data != null) {
                var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                if (matches.Count != 0) {
                    string recognized = matches[0];
                    VoiceRecognitionDone?.Invoke("", recognized);
                    return;
                }
            }
            VoiceRecognitionDone?.Invoke("No speech was recognized", "");
        }

        class SpeechRecognitionListener : Java.Lang.Object, IRecognitionListener
        {
            SpeechRecognizer m_speechRecognizer;
            Intent m_speechRecognizerIntent;

            public SpeechRecognitionListener(SpeechRecognizer speechRecognizer,
                                             Intent speechRecognizerIntent) {
                m_speechRecognizer = speechRecognizer;
                m_speechRecognizerIntent = speechRecognizerIntent;
            }
            public void OnBeginningOfSpeech() {
                Console.WriteLine("--> OnBeginningOfSpeech");
            }
            public void OnBufferReceived(byte[] buffer) {
            }
            public void OnEndOfSpeech() {
                Console.WriteLine("--> OnEndOfSpeech");
            }
            public void OnError(SpeechRecognizerError error) {
                Console.WriteLine("--> SpeechRecognizerError {0}", error);
                MainActivity.TheView.VoiceRecognitionDone?.Invoke(error.ToString(), "");
            }
            public void OnEvent(int eventType, Bundle par) {
                Console.WriteLine("--> OnEvent {0}", eventType);
            }
            public void OnPartialResults(Bundle partialResults) {
            }
            public void OnReadyForSpeech(Bundle par) {
                Console.WriteLine("--> ReadyForSpeech!");
            }
            public void OnResults(Bundle results) {
                Console.WriteLine("--> OnResults!");
                IList<string> matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
                if (matches.Count != 0) {
                    foreach (var match in matches) {
                        Console.WriteLine("   Result {0}", match);
                    }
                }
            }
            public void OnRmsChanged(float rmsdB) {
            }
        }
    }
}
