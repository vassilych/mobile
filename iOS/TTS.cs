using System;
using AVFoundation;

namespace scripting.iOS
{
  public class TTS
  {
    static AVSpeechSynthesizer g_synthesizer = new AVSpeechSynthesizer();

    public static bool   Sound { set; get; }           = true;
    public static float  SpeechRate { set; get; }      = 1.0f;
    public static float  Volume { set; get; }          = 0.7f;
    public static float  PitchMultiplier { set; get; } = 1.0f;
    public static string Voice { set; get; }           = "en-US";

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
    }

    public static void Speak(string text, bool force = false)
    {
      if (!force && !Sound) {
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
