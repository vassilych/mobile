using System;
using UIKit;
using Speech;
using Foundation;
using AVFoundation;

namespace scripting.iOS
{
  public class STT
  {
    public delegate void OnSpeechError(string errorStr);
    public delegate void OnSpeechOK(string recognized);

    static public string Voice { set; get; }

    static UIViewController m_controller;
    static AVAudioEngine AudioEngine;
    static SFSpeechRecognizer SpeechRecognizer;
    static SFSpeechAudioBufferRecognitionRequest LiveSpeechRequest;
    static SFSpeechRecognitionTask RecognitionTask;

    static System.Timers.Timer m_speechTimer;

    const int m_wordTimeout    = 5000;
    const int m_phraseTimeout  = 10000;
    const int m_silenceTimeout = 1000;
    static int m_timeout;

    static bool m_cancelled;
    static public bool IsCancelled {
      get { return m_cancelled; }
      set { m_cancelled = value; }
    }
    static public bool IsRecording { get; set; }

    static DateTime m_lastSpeech;
    static DateTime m_startSpeech;

    public OnSpeechError SpeechError;
    public OnSpeechOK SpeechOK;

    public static bool SpeechEnabled { get; set; }
    public static bool SpeechPossible { get; set; }
    public string LastResult { get; set; }

    public STT(UIViewController controller)
    {
      m_controller = controller;
    }
    public static bool Init()
    {
      if (SpeechEnabled) {
        return true;
      }
      SpeechEnabled = SpeechPossible = UIDevice.CurrentDevice.CheckSystemVersion(10, 0);
      Console.WriteLine("Version {0}, SpeechEnabled: {1}",
                        UIDevice.CurrentDevice.SystemVersion, SpeechEnabled);

      if (!SpeechEnabled) {
        return false;
      }

      RequestAuthorization();
      if (string.IsNullOrWhiteSpace(Voice)) {
        Voice = "en-US";
      }
      return true;
    }

    public static void RequestAuthorization()
    {
      SFSpeechRecognizer.RequestAuthorization((SFSpeechRecognizerAuthorizationStatus status) => {
        // Take action based on status
        switch (status) {
          case SFSpeechRecognizerAuthorizationStatus.Authorized:
            // User has approved speech recognition
            SpeechEnabled = true;
            break;
          case SFSpeechRecognizerAuthorizationStatus.Denied:
            // User has declined speech recognition
            SpeechEnabled = false;
            break;
          case SFSpeechRecognizerAuthorizationStatus.NotDetermined:
            // Waiting on approval
            SpeechEnabled = false;
            break;
          case SFSpeechRecognizerAuthorizationStatus.Restricted:
            // The device is not permitted
            SpeechEnabled = true;
            break;
        }
      });

    }
    public void StartRecording(string voice, bool longTimeout = false)
    {
      if (!SpeechEnabled) {
        return;
      }
      // Setup audio session
      LastResult = "";
      AudioEngine = new AVAudioEngine();
      NSLocale voiceLocale = NSLocale.FromLocaleIdentifier(voice);
      SpeechRecognizer = new SFSpeechRecognizer(voiceLocale);
      LiveSpeechRequest = new SFSpeechAudioBufferRecognitionRequest();

      NSError error;
      var audioSession = AVAudioSession.SharedInstance();
      audioSession.SetCategory(AVAudioSessionCategory.Record);
      audioSession.SetMode(AVAudioSession.ModeMeasurement, out error);
      if (error != null) {
        SpeechError?.Invoke("Audio session error: " + error.ToString());
        return;
      }
      audioSession.SetActive(true, AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation);

      LiveSpeechRequest.ShouldReportPartialResults = true;

      var node = AudioEngine.InputNode;
      if (node == null) {
        SpeechError?.Invoke("Couldn't initialize Speech Input");
        return;
      }

      RecognitionTask = SpeechRecognizer.GetRecognitionTask(LiveSpeechRequest, (SFSpeechRecognitionResult result, NSError err) => {
        if (IsCancelled) {
          node.RemoveTapOnBus(0);
          return;
        }
        if (err != null) {
          SpeechError?.Invoke(err.ToString());
        } else if (result != null) {
          LastResult = result.BestTranscription.FormattedString;
          Console.WriteLine("You said: \"{0}\". Final: {1}",
                                LastResult, result.Final);
          m_lastSpeech = DateTime.Now;
          if (result.Final) {// || !IsRecording) {
            SpeechOK?.Invoke(LastResult);
          }
        }
        if ((result != null && result.Final) || err != null || !IsRecording) {
          IsRecording = false;
          //node.RemoveTapOnBus(0);
          AudioEngine.Stop();
          m_speechTimer.Close();
        }
      });

      var recordingFormat = node.GetBusOutputFormat(0);
      node.InstallTapOnBus(0, 1024, recordingFormat, (AVAudioPcmBuffer buffer, AVAudioTime when) => {
        //Console.WriteLine("--> {0}: {1} {2}.{3}", buffer.FrameLength, when.HostTime);
        // Append buffer to recognition request
        LiveSpeechRequest.Append(buffer);
      });

      // Start recording
      AudioEngine.Prepare();
      AudioEngine.StartAndReturnError(out error);

      if (error != null) {
        SpeechError?.Invoke("Speech init error: " + error.ToString());
        IsRecording = false;
        return;
      }
      IsRecording = true;
      IsCancelled = false;
      LastResult = "";
      m_lastSpeech = DateTime.MaxValue;
      m_startSpeech = DateTime.Now;
      m_timeout = longTimeout ? m_phraseTimeout : m_wordTimeout;

      m_speechTimer = new System.Timers.Timer(250);
      m_speechTimer.AutoReset = true;
      m_speechTimer.Elapsed += (sender, e) => {
        CheckRecording();
      };
      m_speechTimer.Start();
    }

    public void CheckRecording()
    {
      var now = DateTime.Now;
      var sinceStart = (now - m_startSpeech).TotalMilliseconds;
      var sinceLastSpeak = (now - m_lastSpeech).TotalMilliseconds;

      if (sinceStart >= m_timeout || sinceLastSpeak >= m_silenceTimeout) {
        CancelRecording();
        m_controller.InvokeOnMainThread(() => {
          if (string.IsNullOrEmpty(LastResult)) {
            SpeechError?.Invoke(Localization.GetText("No speech was recognized"));
          } else {
            SpeechOK?.Invoke(LastResult);
          }
        });
        return;
      }
    }
    public void StopRecording()
    {
      IsRecording = false;
      if (m_speechTimer != null) {
        m_speechTimer.Stop();
        m_speechTimer.Dispose();
      }
      if (AudioEngine != null) {
        AudioEngine.Stop();
        LiveSpeechRequest.EndAudio();
      }
      if (RecognitionTask != null) {
        RecognitionTask.Cancel();
      }
    }

    public void CancelRecording()
    {
      IsCancelled = true;
      StopRecording();
    }
  }
}
