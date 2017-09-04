using Android.App;
using Android.Widget;
using Android.Views;
using Android.OS;
using Android.Graphics;
using SplitAndMerge;
using System;
using Android.Content.Res;
using System.IO;
using System.Collections.Generic;

namespace scripting.Droid
{
    [Activity(Theme = "@android:style/Theme.Holo.Light",
              Label = "", MainLauncher = true)]
    public class MainActivity : Activity, ActionBar.ITabListener
    {
        public static MainActivity   TheView;
        public static RelativeLayout TheLayout;

        static List<ActionBar.Tab>     m_actionBars = new List<ActionBar.Tab>();


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

            ViewTreeObserver vto = relativelayout.ViewTreeObserver;
            vto.AddOnGlobalLayoutListener(new LayoutListener());
        }

        public void ChangeTab(int tab)
        {
            ActionBar.SelectTab(m_actionBars[tab]);
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

            for (int i = 0; i < m_actionBars.Count; i++)    {
                SetTabIcon(i, activeTab == i);
            }
        }

        public static void RunScript()
        {
            ParserFunction.RegisterFunction("_ANDROID_",          new CheckOSFunction(CheckOSFunction.OS.ANDROID));
            ParserFunction.RegisterFunction("_IOS_",              new CheckOSFunction(CheckOSFunction.OS.IOS));
            ParserFunction.RegisterFunction("_VERSION_",          new GetVersionFunction());

            ParserFunction.RegisterFunction("AddTab",             new AddTabFunction());
            ParserFunction.RegisterFunction("AddWidget",          new AddWidgetFunction());
            ParserFunction.RegisterFunction("AddView",            new AddWidgetFunction("View"));
            ParserFunction.RegisterFunction("AddButton",          new AddWidgetFunction("Button"));
            ParserFunction.RegisterFunction("AddLabel",           new AddWidgetFunction("Label"));
            ParserFunction.RegisterFunction("AddTextEdit",        new AddWidgetFunction("TextEdit"));
            ParserFunction.RegisterFunction("AddImageView",       new AddWidgetFunction("ImageView"));
            ParserFunction.RegisterFunction("AddTypePickerView",  new AddWidgetFunction("TypePicker"));
            ParserFunction.RegisterFunction("AddSwitch",          new AddWidgetFunction("Switch"));
            ParserFunction.RegisterFunction("AddSlider",          new AddWidgetFunction("Slider"));
            ParserFunction.RegisterFunction("AddCombobox",        new AddWidgetFunction("Combobox"));
            ParserFunction.RegisterFunction("AddWidgetData",      new AddWidgetDataFunction());
            ParserFunction.RegisterFunction("AddWidgetImages",    new AddWidgetImagesFunction());
            ParserFunction.RegisterFunction("AddBorder",          new AddBorderFunction());
            ParserFunction.RegisterFunction("AddAction",          new AddActionFunction());
            ParserFunction.RegisterFunction("GetLocation",        new GetLocationFunction());
            ParserFunction.RegisterFunction("ShowView",           new ShowHideFunction(true));
            ParserFunction.RegisterFunction("HideView",           new ShowHideFunction(false));
            ParserFunction.RegisterFunction("SelectTab",          new SelectTabFunction());
            ParserFunction.RegisterFunction("SetBackgroundColor", new SetBackgroundColorFunction());
            ParserFunction.RegisterFunction("SetBackground",      new SetBackgroundImageFunction());
            ParserFunction.RegisterFunction("SetText",            new SetTextFunction());
            ParserFunction.RegisterFunction("GetText",            new GetTextFunction());
            ParserFunction.RegisterFunction("SetValue",           new SetValueFunction());
            ParserFunction.RegisterFunction("GetValue",           new GetValueFunction());
            ParserFunction.RegisterFunction("SetImage",           new SetImageFunction());
            ParserFunction.RegisterFunction("SetFontSize",        new SetFontSizeFunction());
            ParserFunction.RegisterFunction("AlignText",          new AlignTitleFunction());
            ParserFunction.RegisterFunction("SetSize",            new SetSizeFunction());
            ParserFunction.RegisterFunction("ShowToast",          new ShowToastFunction());
            ParserFunction.RegisterFunction("AlertDialog",        new AlertDialogFunction());
            ParserFunction.RegisterFunction("DisplayWidth",       new GadgetSizeFunction(true));
            ParserFunction.RegisterFunction("DisplayHeight",      new GadgetSizeFunction(false));
            ParserFunction.RegisterFunction("CallNative",         new InvokeNativeFunction());
	        //ParserFunction.RegisterFunction("SetOptions", new SetOptionsFunction());
	        string script = "";
	        AssetManager assets = TheView.Assets;
	        using (StreamReader sr = new StreamReader(assets.Open("script.cscs"))) {
	            script = sr.ReadToEnd();
	        }

	        Variable result = null;
	        try {
	            result = Interpreter.Instance.Process(script);
	        }
	        catch (Exception exc) {
	            Console.WriteLine("Exception: " + exc.Message);
	            ParserFunction.InvalidateStacksAfterLevel(0);
	        }
	    }
        public static void AddView(View view, ViewGroup parent)
        {
            ScriptingFragment.AddView(view);
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
            )
            {
                Width  = width,
                Height = height
            };
            relativelayout.LayoutParameters = layoutParams;

            parent = parent == null ? TheLayout : parent;
            parent.AddView(relativelayout);

            return relativelayout;
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
