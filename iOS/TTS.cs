using System;
using AVFoundation;

namespace scripting.iOS
{
  public class TTS
  {
    static AVSpeechSynthesizer g_synthesizer = new AVSpeechSynthesizer();

    static bool m_sound = true;
    static public bool Sound {
      set {
        m_sound = value;
      }
      get {
        return m_sound;
      }
    }
    static public float SpeechRate { set; get; }
    static public float Volume { set; get; }
    static public float PitchMultiplier { set; get; }
    static public string Voice { set; get; }

    static bool m_initDone;

    public static void Init()
    {
      if (m_initDone) {
        return;
      }
      m_initDone = true;
      // set audio session category, so app will speak even when mute switch is on
      AVAudioSession.SharedInstance().Init();
      AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback,
                                      AVAudioSessionCategoryOptions.DefaultToSpeaker);
      SpeechRate = 0.5f;
      Volume = 0.7f;
      PitchMultiplier = 1.0f;
      Voice = "en-US";
    }

    public static void Speak(string text, bool force = false)
    {
      if (!force && !m_sound) {
        return;
      }
      if (g_synthesizer.Speaking) {
        g_synthesizer.StopSpeaking(AVSpeechBoundary.Immediate);
      }

      var speechUtterance = new AVSpeechUtterance(text) {
        Rate = SpeechRate * AVSpeechUtterance.MaximumSpeechRate,
        Voice = AVSpeechSynthesisVoice.FromLanguage(Voice),
        Volume = Volume,
        PitchMultiplier = PitchMultiplier
      };

      g_synthesizer.SpeakUtterance(speechUtterance);
    }
  }
}
