﻿using Android.App;
using Android.Content;
using Android.Widget;
using Android.Views;
using Android.OS;
using Android.Speech.Tts;
using Android.Graphics;
using SplitAndMerge;
using System;
using Android.Content.Res;
using System.IO;
using System.Collections.Generic;
using Android.Content.PM;

namespace scripting.Droid
{
  [Activity(Theme = "@android:style/Theme.Holo.Light",
            Label = "", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
  public partial class MainActivity : Activity, ActionBar.ITabListener
  {
    public static MainActivity TheView;
    public static RelativeLayout TheLayout;

    static List<ActionBar.Tab> m_actionBars = new List<ActionBar.Tab>();

    public static int CurrentTabId;

    protected override void OnCreate(Bundle savedInstanceState)
    {
      base.OnCreate(savedInstanceState);

      Console.WriteLine("Starting ActionBar: {0}", ActionBar != null);

      ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
      ActionBar.SetDisplayShowTitleEnabled(false);
      ActionBar.SetDisplayShowHomeEnabled(false);

      RelativeLayout relativelayout = new RelativeLayout(this);
      RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(
          ViewGroup.LayoutParams.MatchParent,
          ViewGroup.LayoutParams.MatchParent
      );


      relativelayout.LayoutParameters = layoutParams;

      //relativelayout.SetBackgroundColor(Color.ParseColor("#fa0303"));
      SetContentView(relativelayout);

      TheView = this;
      TheLayout = relativelayout;

      InitTTS();

      ViewTreeObserver vto = relativelayout.ViewTreeObserver;
      vto.AddOnGlobalLayoutListener(new LayoutListener());
    }

    public void ChangeTab(int tabId)
    {
      CurrentTabId = tabId;
      ActionBar.Tab tab = m_actionBars[tabId];
      tab.Select();
      //ActionBar.SelectTab(m_actionBars[tabId]);
    }

    public ActionBar.Tab AddTabToActionBar(ScriptingFragment fragment, bool setActive = false)
    {
      ActionBar.Tab tab = ActionBar.NewTab()
                                   .SetText(fragment.Text)
                                   .SetIcon(setActive ? fragment.ActiveIcon : fragment.InactiveIcon)
                                   .SetTag(fragment.Text)
                                   .SetTabListener(this);
      ActionBar.AddTab(tab);
  
      //tv.All .setAllCaps(false);
      return tab;
    }
    public static void AddTab(string text, string imageName, string selectedImageName = null)
    {
      ScriptingFragment fragment = ScriptingFragment.AddFragment(text, imageName, selectedImageName);
      ActionBar.Tab tab = MainActivity.TheView.AddTabToActionBar(fragment, ScriptingFragment.Count() == 1);

      m_actionBars.Add(tab);
      SelectTab(m_actionBars.Count - 1);
    }
    public static void TranslateTabs()
    {
      for (int i = 0; i < m_actionBars.Count; i++) {
        ActionBar.Tab tab = m_actionBars[i];
        ScriptingFragment fragment = ScriptingFragment.GetFragment(i);
        string translated = Localization.GetText(fragment.OriginalText);
        tab.SetText(translated);
      }
    }
    public static void EditTabText(int tabId, string text)
    {
      ActionBar.Tab tab = m_actionBars[tabId];
      tab.SetText(text);
      /*for (int j = 0; j < TabWidget.get; j++) {
        TextView tv = (TextView)TabWidget.GetChildAt(j).FindViewById(Android.Resource.Id.Title);
        tv.SetTextColor(Android.Graphics.Color.ParseColor("#F9F5AD"));
        tv.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
      }*/
    }

    public static void SetTabIcon(int tab, bool active = true)
    {
      if (m_actionBars.Count > tab) {
        ActionBar.Tab curr = m_actionBars[tab];
        ScriptingFragment fragment = ScriptingFragment.GetFragment(tab);
        int resourceId = fragment.GetViewImage(active);
        curr.SetIcon(resourceId);
      }
    }

    public static void SelectTab(int activeTab)
    {
      ScriptingFragment.ShowFragments(activeTab);

      for (int i = 0; i < m_actionBars.Count; i++) {
        SetTabIcon(i, activeTab == i);
      }
      CurrentTabId = activeTab;
    }
    public static void RemoveAll()
    {
      ScriptingFragment.RemoveAll();
    }

    public static void RunScript()
    {


      ParserFunction.RegisterFunction("_ANDROID_", new CheckOSFunction(CheckOSFunction.OS.ANDROID));
      ParserFunction.RegisterFunction("_IOS_", new CheckOSFunction(CheckOSFunction.OS.IOS));
      ParserFunction.RegisterFunction("_VERSION_", new GetVersionFunction());
      ParserFunction.RegisterFunction("DisplayWidth", new GadgetSizeFunction(true));
      ParserFunction.RegisterFunction("DisplayHeight", new GadgetSizeFunction(false));
      ParserFunction.RegisterFunction("Orientation", new OrientationFunction());

      ParserFunction.RegisterFunction("AddTab", new AddTabFunction());
      ParserFunction.RegisterFunction("GetSelectedTab", new GetSelectedTabFunction());
      ParserFunction.RegisterFunction("AddWidget", new AddWidgetFunction());
      ParserFunction.RegisterFunction("AddView", new AddWidgetFunction("View"));
      ParserFunction.RegisterFunction("AddButton", new AddWidgetFunction("Button"));
      ParserFunction.RegisterFunction("AddLabel", new AddWidgetFunction("Label"));
      ParserFunction.RegisterFunction("AddTextEdit", new AddWidgetFunction("TextEdit"));
      ParserFunction.RegisterFunction("AddTextView", new AddWidgetFunction("TextView"));
      ParserFunction.RegisterFunction("AddImageView", new AddWidgetFunction("ImageView"));
      ParserFunction.RegisterFunction("AddTypePickerView", new AddWidgetFunction("TypePicker"));
      ParserFunction.RegisterFunction("AddSwitch", new AddWidgetFunction("Switch"));
      ParserFunction.RegisterFunction("AddSlider", new AddWidgetFunction("Slider"));
      ParserFunction.RegisterFunction("AddStepper", new AddWidgetFunction("Stepper"));
      ParserFunction.RegisterFunction("AddStepperLeft", new AddWidgetFunction("Stepper", "left"));
      ParserFunction.RegisterFunction("AddStepperRight", new AddWidgetFunction("Stepper", "right"));
      ParserFunction.RegisterFunction("AddListView", new AddWidgetFunction("ListView"));
      ParserFunction.RegisterFunction("AddCombobox", new AddWidgetFunction("Combobox"));
      ParserFunction.RegisterFunction("AddSegmentedControl", new AddWidgetFunction("SegmentedControl"));
      ParserFunction.RegisterFunction("AddBanner", new AddWidgetFunction("AdMobBanner"));
      ParserFunction.RegisterFunction("AddWidgetData", new AddWidgetDataFunction());
      ParserFunction.RegisterFunction("AddWidgetImages", new AddWidgetImagesFunction());
      ParserFunction.RegisterFunction("AddBorder", new AddBorderFunction());
      ParserFunction.RegisterFunction("AutoScale", new AutoScaleFunction());
      ParserFunction.RegisterFunction("AddAction", new AddActionFunction());
      ParserFunction.RegisterFunction("AddLongClick", new AddLongClickFunction());
      ParserFunction.RegisterFunction("AddSwipe", new AddSwipeFunction());
      ParserFunction.RegisterFunction("AddDragAndDrop", new AddDragAndDropFunction());
      ParserFunction.RegisterFunction("GetLocation", new GetLocationFunction());
      ParserFunction.RegisterFunction("ShowView", new ShowHideFunction(true));
      ParserFunction.RegisterFunction("HideView", new ShowHideFunction(false));
      ParserFunction.RegisterFunction("SetVisible", new ShowHideFunction(true));
      ParserFunction.RegisterFunction("RemoveView", new RemoveViewFunction());
      ParserFunction.RegisterFunction("RemoveAllViews", new RemoveAllViewsFunction());
      ParserFunction.RegisterFunction("Move", new MoveFunction());
      ParserFunction.RegisterFunction("SelectTab", new SelectTabFunction());
      ParserFunction.RegisterFunction("OnOrientationChange", new OrientationChangeFunction());
      ParserFunction.RegisterFunction("SetBackgroundColor", new SetBackgroundColorFunction());
      ParserFunction.RegisterFunction("SetBackground", new SetBackgroundImageFunction());
      ParserFunction.RegisterFunction("SetText", new SetTextFunction());
      ParserFunction.RegisterFunction("GetText", new GetTextFunction());
      ParserFunction.RegisterFunction("SetValue", new SetValueFunction());
      ParserFunction.RegisterFunction("GetValue", new GetValueFunction());
      ParserFunction.RegisterFunction("SetImage", new SetImageFunction());
      ParserFunction.RegisterFunction("SetFontSize", new SetFontSizeFunction());
      ParserFunction.RegisterFunction("SetFontColor", new SetFontColorFunction());
      ParserFunction.RegisterFunction("AlignText", new AlignTitleFunction());
      ParserFunction.RegisterFunction("SetSize", new SetSizeFunction());
      ParserFunction.RegisterFunction("ShowToast", new ShowToastFunction());
      ParserFunction.RegisterFunction("AlertDialog", new AlertDialogFunction());
      ParserFunction.RegisterFunction("CallNative", new InvokeNativeFunction());
      ParserFunction.RegisterFunction("Speak", new SpeakFunction());
      ParserFunction.RegisterFunction("SetupSpeech", new SpeechOptionsFunction());
      ParserFunction.RegisterFunction("VoiceRecognition", new VoiceFunction());
      ParserFunction.RegisterFunction("StopVoiceRecognition", new StopVoiceFunction());
      ParserFunction.RegisterFunction("Localize", new LocalizedFunction());
      ParserFunction.RegisterFunction("TranslateTabBar", new TranslateTabBar());
      ParserFunction.RegisterFunction("InitAds", new InitAds());
      ParserFunction.RegisterFunction("ShowInterstitial", new ShowInterstitial());
      ParserFunction.RegisterFunction("InitIAP", new InitIAPFunction());
      ParserFunction.RegisterFunction("Purchase", new PurchaseFunction());
      ParserFunction.RegisterFunction("Restore", new RestoreFunction());
      ParserFunction.RegisterFunction("ReadFile", new ReadFileFunction());
      ParserFunction.RegisterFunction("Schedule", new PauseFunction(true));
      ParserFunction.RegisterFunction("CancelSchedule", new PauseFunction(false));
      ParserFunction.RegisterFunction("GetDeviceLocale", new GetDeviceLocale());
      ParserFunction.RegisterFunction("SetAppLocale", new SetAppLocale());
      ParserFunction.RegisterFunction("GetSetting", new GetSettingFunction());
      ParserFunction.RegisterFunction("SetSetting", new SetSettingFunction());
      ParserFunction.RegisterFunction("GetTrie", new CreateTrieFunction());
      ParserFunction.RegisterFunction("SearchTrie", new SearchTrieFunction());
      //ParserFunction.RegisterFunction("SetOptions", new SetOptionsFunction());
      string script = "";
      AssetManager assets = TheView.Assets;
      using (StreamReader sr = new StreamReader(assets.Open("script.cscs"))) {
        script = sr.ReadToEnd();
      }

      Variable result = null;
      try {
        result = Interpreter.Instance.Process(script);
      } catch (Exception exc) {
        Console.WriteLine("Exception: " + exc.Message);
        ParserFunction.InvalidateStacksAfterLevel(0);
      }
    }
    public static void AddView(View view, ViewGroup parent)
    {
      ScriptingFragment.AddView(view, parent);
      if (parent != null) {
        parent.AddView(view);
      } else {
        TheLayout.AddView(view);
      }
    }
    public static ViewGroup CreateViewLayout(int width, int height, ViewGroup parent = null)
    {
      RelativeLayout relativelayout = new RelativeLayout(MainActivity.TheView);
      RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(
          ViewGroup.LayoutParams.MatchParent,
          ViewGroup.LayoutParams.MatchParent
      ) {
        Width = width,
        Height = height
      };
      relativelayout.LayoutParameters = layoutParams;

      //parent = parent == null ? TheLayout : parent;
      //parent.AddView(relativelayout);

      return relativelayout;
    }
    public static ViewGroup CreateLinearLayout(int width, int height, ViewGroup parent = null)
    {
      LinearLayout linlayout = new LinearLayout(MainActivity.TheView);
      LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
          ViewGroup.LayoutParams.WrapContent,
          ViewGroup.LayoutParams.WrapContent
      ) {
        Width = width,
        Height = height
      };
      linlayout.LayoutParameters = layoutParams;
      linlayout.Orientation = Android.Widget.Orientation.Horizontal;

      parent = parent == null ? TheLayout : parent;
      parent.AddView(linlayout);

      return linlayout;
    }
    public static void ShowView(View view, bool showIt)
    {
      ScriptingFragment.ShowView(view, showIt, false);
    }
    public static string ProcessClick(string arg)
    {
      var now = DateTime.Now.ToString("T");
      return "Clicks: " + arg + "\n" + now;
    }
    static public int String2Pic(string name)
    {
      string imagefileName = name.Replace("-", "_").Replace(".png", "");
      var fieldInfo = typeof(Resource.Drawable).GetField(imagefileName);
      if (fieldInfo == null) {
        Console.WriteLine("Couldn't find pic for [{0}]", imagefileName);
        return -999;
      }
      int resourceID = (int)fieldInfo.GetValue(null);
      return resourceID;
    }

    public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
    {
      SelectTab(tab.Position);
    }

    public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
    {
      //ft.Replace(Resource.Id.frameLayout1, frag);
      SelectTab(tab.Position);
    }

    public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
    {
      //throw new NotImplementedException();
    }

    public delegate void OrientationChange(string newOrientation);
    public static OrientationChange OnOrientationChange;
    public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
    {
      OnOrientationChange?.Invoke(newConfig.Orientation == Android.Content.Res.Orientation.Portrait ?
                                  "Portrait" : "Landscape");
      base.OnConfigurationChanged(newConfig);
    }

    void InitTTS()
    {
      //Intent installedIntent = new Intent();
      //installedIntent.SetAction(TextToSpeech.Engine.ActionTtsDataInstalled);
      //StartActivityForResult(installedIntent, TTS_INSTALLED_DATA);

      Intent checkIntent = new Intent();
      checkIntent.SetAction(TextToSpeech.Engine.ActionCheckTtsData);
      StartActivityForResult(checkIntent, TTS.TTS_CHECK_DATA);

      TTS.Init(this);
    }
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
      Console.WriteLine("OnActivityResult {0}: {1}, {2}, {3}",
                        requestCode, resultCode, (int)resultCode, data);
      if (requestCode == TTS.TTS_REQUEST_DATA ||
          requestCode == TTS.TTS_INSTALLED_DATA ||
          requestCode == TTS.TTS_CHECK_DATA) {
        TTS.OnTTSResult(requestCode, resultCode, data);
      } else if (requestCode == STT_REQUEST) {
        STT.SpeechRecognitionCompleted(resultCode, data);
      } else if (requestCode == IAP.IAP_REQUEST) {
        IAP.OnIAPCallback(requestCode, resultCode, data);
      }

      base.OnActivityResult(requestCode, resultCode, data);
    }
  }

  public class LayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
  {
    public void OnGlobalLayout()
    {
      var vto = MainActivity.TheLayout.ViewTreeObserver;
      vto.RemoveOnGlobalLayoutListener(this);

      MainActivity.RunScript();
      // TODO: set tab if not set in the script:
      //MainActivity.SelectTab(0);
    }
  }
}
