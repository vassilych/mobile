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
using Android.Views.InputMethods;

namespace scripting.Droid
{
    [Activity(//Theme = "@android:style/Theme.Holo.Light",
              Theme = "@style/CustomTheme",
              Icon = "@mipmap/icon",
              Label = "",
              //MainLauncher = true,
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.ScreenSize |
                                     ConfigChanges.Orientation | ConfigChanges.KeyboardHidden
              )]
    public partial class MainActivity : Activity,
                                        ActionBar.ITabListener
    {
        public static MainActivity TheView;
        public static RelativeLayout TheLayout;
        //public static ContentResolver ContentResolver;

        static List<ActionBar.Tab> m_actionBars = new List<ActionBar.Tab>();

        static Dictionary<string, int> m_allTabs = new Dictionary<string, int>();

        public static int CurrentTabId;

        public static Action<int> TabSelectedDelegate;
        public static Action OnEnterBackgroundDelegate;

        public static bool KeyboardVisible;

        bool m_scriptRun = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Console.WriteLine("Starting ActionBar: {0}", ActionBar != null);

            //ActionBar.NavigationMode = ActionBarNavigationMode.Standard;
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
            //ActionBar.Hide();

            TheView = this;
            TheLayout = relativelayout;

            InitTTS();

            // For Plugin.InAppBilling.
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
        }

        public Context GetContext()
        {
            return this.ApplicationContext;

        }

        internal void HideBarIfNeeded()
        {
            if (m_actionBars.Count > 0)
            {
                return;
            }
            ActionBar.Hide();
            TheLayout.ForceLayout();
            TheLayout.RequestLayout();
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
        }
        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }
        public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            OnTabSelected(tab, ft);
        }

        public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            //ft.Replace(Resource.Id.frameLayout1, frag);
            SelectTab(tab.Position);
            TabSelectedDelegate?.Invoke(tab.Position);
        }

        public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
        }

        public delegate void OrientationChange(string newOrientation);
        public static OrientationChange OnOrientationChange;
        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            OnOrientationChange?.Invoke(newConfig.Orientation == Android.Content.Res.Orientation.Portrait ?
                                        "Portrait" : "Landscape");
            base.OnConfigurationChanged(newConfig);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }
        protected override void OnStop()
        {
            base.OnStop();
            OnEnterBackgroundDelegate?.Invoke();
        }
        protected override void OnResume()
        {
            base.OnResume();
            if (!m_scriptRun)
            {
                ViewTreeObserver vto = TheLayout.ViewTreeObserver;
                vto.AddOnGlobalLayoutListener(new LayoutListener());
                m_scriptRun = true;
            }
        }
        public static string Orientation
        {
            get
            {
                return IsLandscape ? "Landscape" : "Portrait";
            }
        }
        public static bool IsLandscape
        {
            get
            {
                return TheView.Resources.Configuration.Orientation ==
                              Android.Content.Res.Orientation.Landscape;
            }
        }
        public void ChangeTab(int tabId)
        {
            if (m_actionBars.Count <= tabId)
            {
                return;
            }
            CurrentTabId = tabId;
            ActionBar.Tab tab = m_actionBars[tabId];
            //tab.Select();

            Handler h = new Handler();
            Action myAction = () =>
            {
                tab.Select();
            };

            h.PostDelayed(myAction, 0);

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
        public static bool TabExists(string text)
        {
            return m_allTabs.ContainsKey(text);
        }
        public static bool SelectTab(string text)
        {
            int tabId = 0;
            if (m_allTabs.TryGetValue(text, out tabId))
            {
                SelectTab(tabId);
                return true;
            }
            return false;
        }
        public static void AddTab(string text, string selectedImageName, string notSelectedImageName = null)
        {
            ScriptingFragment fragment = ScriptingFragment.AddFragment(text, selectedImageName, notSelectedImageName);
            ActionBar.Tab tab = MainActivity.TheView.AddTabToActionBar(fragment, ScriptingFragment.Count() == 1);

            m_actionBars.Add(tab);
            m_allTabs[text] = m_actionBars.Count - 1;
            SelectTab(m_actionBars.Count - 1);
        }
        public static void TranslateTabs()
        {
            Localization.CheckCode();
            for (int i = 0; i < m_actionBars.Count; i++)
            {
                ActionBar.Tab tab = m_actionBars[i];
                ScriptingFragment fragment = ScriptingFragment.GetFragment(i);
                string translated = Localization.GetText(fragment.GetText());
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
            if (m_actionBars.Count > tab)
            {
                ActionBar.Tab curr = m_actionBars[tab];
                ScriptingFragment fragment = ScriptingFragment.GetFragment(tab);
                int resourceId = fragment.GetViewImage(active);
                curr.SetIcon(resourceId);
            }
        }

        public static void SelectTab(int activeTab)
        {
            ScriptingFragment.ShowFragments(activeTab);

            for (int i = 0; i < m_actionBars.Count; i++)
            {
                SetTabIcon(i, activeTab == i);
            }
            CurrentTabId = activeTab;
        }
        public static void RemoveAll()
        {
            ScriptingFragment.RemoveAll();
        }
        public static void RemoveTabViews(int tabId)
        {
            ScriptingFragment.RemoveTabViews(tabId);
        }

        public static void AddView(DroidVariable widget, bool alreadyExists)
        {
            var location = widget.Location;
            var view = widget.ViewX;
            var parentView = location.ParentView as DroidVariable;
            var parent = parentView?.ViewLayout;
            //Console.WriteLine("--ADDING {0} {1}, text: {2}, parent: {3}, exists: {4}",
            //                  varName, widgetType, text, parentView == null, alreadyExists);
            //MainActivity.AddView(widget, parentView?.ViewLayout);
            ScriptingFragment.AddView(widget, parent);
            if (parent != null)
            {
                parent.AddView(view);
            }
            else
            {
                TheLayout.AddView(view);
            }
            if (alreadyExists)
            {
                MainActivity.TheLayout.Invalidate();
                MainActivity.TheLayout.RefreshDrawableState();
                view.Invalidate();
                view.RefreshDrawableState();
                //if (parentView != null) {
                parentView?.ViewLayout.Invalidate();
                //}
            }
        }
        public static void RemoveView(DroidVariable viewVar)
        {
            if (viewVar == null || viewVar.ViewX == null)
            {
                return;
            }
            ScriptingFragment.RemoveView(viewVar);
        }

        public static ViewGroup CreateViewLayout(int width, int height, ViewGroup parent = null)
        {
            RelativeLayout relativelayout = new RelativeLayout(MainActivity.TheView);
            RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent
            )
            {
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
            )
            {
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
            if (view is TextView || view is EditText)
            {
                TheView.ShowHideKeyboard(view, showIt);
            }
        }
        public static string ProcessClick(string arg)
        {
            var now = DateTime.Now.ToString("T");
            return "Clicks: " + arg + "\n" + now;
        }
        static public int String2Pic(string name)
        {
            //string imagefileName = UIUtils.String2ImageName(name);
            string imagefileName = name.Replace("-", "_").Replace(".png", "");
            var fieldInfo = typeof(Resource.Drawable).GetField(imagefileName);
            if (fieldInfo == null)
            {
                Console.WriteLine("Couldn't find pic for [{0}]", imagefileName);
                return -999;
            }
            int resourceID = (int)fieldInfo.GetValue(null);
            return resourceID;
        }

        public void ShowHideKeyboard(View widget, bool show)
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            if (!show)
            { // Hide!
                imm.HideSoftInputFromWindow(widget.WindowToken, 0);
            }
            else
            { // Show!
                if (widget is TextView)
                {
                    ((TextView)widget).SetCursorVisible(show);
                }
                else if (widget is EditText)
                {
                    ((EditText)widget).SetCursorVisible(show);
                }
                widget.RequestFocus();
                imm.ShowSoftInput(widget, ShowFlags.Implicit);
            }
        }

        static bool m_keyboardVisible;
        public bool IsKeyboardVisible(View widget)
        {
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);

            bool keyboardMaybePresent = imm.IsActive || imm.IsAcceptingText;
            if (keyboardMaybePresent)
            { // if it's true doesn't mean much...
                keyboardMaybePresent = !m_keyboardVisible;
            }
            m_keyboardVisible = keyboardMaybePresent;
            return m_keyboardVisible;
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
                requestCode == TTS.TTS_CHECK_DATA)
            {
                TTS.OnTTSResult(requestCode, resultCode, data);
            }
            else if (requestCode == STT.STT_REQUEST)
            {
                STT.SpeechRecognitionCompleted(resultCode, data);
                //} else if (requestCode == InAppBilling.IAP_REQUEST) {
                //  InAppBilling.OnIAPCallback(requestCode, resultCode, data);
            }
            else if (requestCode == ImageEditor.SELECT_FROM_GALLERY ||
                     requestCode == ImageEditor.SELECT_FROM_CAMERA)
            {
                ImageEditor.OnActivityResult(requestCode, resultCode, data);
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }
        
        public static long GetTotalMemory()
        {
            var ctx = Android.App.Application.Context;
            ActivityManager actManager = (ActivityManager)ctx.GetSystemService(Context.ActivityService);
            ActivityManager.MemoryInfo memInfo = new ActivityManager.MemoryInfo();
            actManager.GetMemoryInfo(memInfo);
            return memInfo.TotalMem;
        }
    }

    public class LayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
    {
        bool m_scriptRun;
        public void OnGlobalLayout()
        {
            //var vto = MainActivity.TheLayout.ViewTreeObserver;
            //vto.RemoveOnGlobalLayoutListener(this);

            if (!m_scriptRun)
            {
                CustomInit.InitAndRunScript();
                m_scriptRun = true;
                MainActivity.TheView.HideBarIfNeeded();
                return;
            }
            MainActivity.KeyboardVisible = KeyboardPresent();
        }
        bool KeyboardPresent()
        {
            Rect r = new Rect();
            MainActivity.TheLayout.GetWindowVisibleDisplayFrame(r);
            int screenHeight = MainActivity.TheLayout.RootView.Height;

            // r.bottom is the position above soft keypad or device button.
            // if keypad is shown, the r.bottom is smaller than that before.
            int keypadHeight = screenHeight - r.Bottom;

            return keypadHeight > screenHeight * 0.15;
        }
    }
}
