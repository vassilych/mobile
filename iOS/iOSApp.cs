using System;
using System.Collections.Generic;
using SplitAndMerge;
using UIKit;

namespace scripting.iOS
{
    public class iOSTab
    {
        UIViewController m_tab;
        List<UIView>     m_views = new List<UIView>();
        UIImage          m_image;

        public iOSTab(UIViewController tab)
        {
            m_tab = tab;
        }
        public string Text {
            get { return m_tab.Title; }
            set { m_tab.Title = value; }
        }
        public UIImage Image {
            get { return m_tab.TabBarItem.Image; }
            set {
                m_image = value;
                if (m_image != null) {
                    m_image = m_image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
                }
                m_tab.TabBarItem.Image = m_image;
            }
        }
        public void AddView(UIView view)
        {
            m_views.Add(view);
        }
        public void RemoveView(UIView view)
        {
            m_views.Remove(view);
        }
        public void ShowTab(bool showIt = true)
        {
            for (int i = 0; i < m_views.Count; i++) {
                iOSApp.ShowView(m_views[i], showIt, true);
            }
        }

        public UIColor BackgroundColor {
            set {
                m_tab.View.BackgroundColor =value;
            }
        }
        //public UIColor BackgroundImage {
        //    set {    //m_tab.bac =value;    }
        //}
    }

    public class iOSApp : UITabBarController
    {
        static List<iOSTab> m_tabs = new List<iOSTab>();
        static iOSTab m_activeTab;
        static List<UIView> m_hiddenViews = new List<UIView>();

        int m_selectedTab = -1;

        public static iOSApp Instance { set; protected get; }

        public iOSApp()
        {
            Instance = this;
        }

        public static int CurrentOffset { get; set; }

        public void OffsetTabBar(bool down = true)
        {
            var tabFrame = TabBar.Frame; //self.TabBar is IBOutlet of your TabBar
            int offset = down ? (int)tabFrame.Size.Height : -1 * (int)tabFrame.Size.Height;
            tabFrame.Y += offset; //.Offset(0, offset);
            TabBar.Frame = tabFrame;

            CurrentOffset = down ? 0 : (int)tabFrame.Size.Height;
        }

        public void OnTabSelected(object sender, UITabBarSelectionEventArgs e)
        {
            m_selectedTab = (int)e.ViewController.TabBarController.SelectedIndex;
            SelectTab(m_selectedTab);
        }
        public static void SelectTab(int selectedTab)
        {
            Instance.m_selectedTab = selectedTab;
            Instance.SelectedIndex = Instance.m_selectedTab;

            m_activeTab = m_tabs[selectedTab];
            for (int i = 0; i < m_tabs.Count; i++) {
                m_tabs[i].ShowTab(i == selectedTab);
            }
        }

        public void Run()
        {
            // If there is no tabbbar, move the tabs view down:
            OffsetTabBar(true);

            this.ViewControllerSelected += OnTabSelected;

            //iosTabController.SelectedIndex = 0;

            RunScript();

            if (m_selectedTab >= 0) {
                SelectTab(m_selectedTab);
            }
        }

        public void RunScript()
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
            ParserFunction.RegisterFunction("SetOptions",         new SetOptionsFunction());

            string[] lines = System.IO.File.ReadAllLines("script.cscs");
            string script = string.Join("\n", lines);

            Variable result = null;
            try    {
                result = Interpreter.Instance.Process(script);
            }
            catch (Exception exc) {
                Console.WriteLine("Exception: " + exc.Message);
                ParserFunction.InvalidateStacksAfterLevel(0);
            }
        }
        public static void AddView(UIView view)
        {
            if (m_activeTab == null) {
                return;
            }
            m_activeTab.AddView(view);
        }
        public static void RemoveView(UIView view)
        {
            if (m_activeTab == null) {
                return;
            }
            m_activeTab.RemoveView(view);
        }
        public static void AddTab(string text, string selectedImageName, string notSelectedImageName = null)
        {
            var selImage = UIImage.FromFile(selectedImageName);
            selImage = selImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

            UIViewController tab = new UIViewController();
            tab.Title = text;
            tab.TabBarItem.SelectedImage = selImage;
            tab.TabBarItem.Image = selImage;

            if (notSelectedImageName != null) {
                var image = UIImage.FromFile(notSelectedImageName);
                image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
                tab.TabBarItem.Image = image;
            }

            List<UIViewController> controllers = Instance.ViewControllers == null ?
                new List<UIViewController>() :
                new List<UIViewController>(Instance.ViewControllers);
            controllers.Add(tab);
            Instance.ViewControllers = controllers.ToArray();

            m_activeTab = new iOSTab(tab);
            m_tabs.Add(m_activeTab);
            if (controllers.Count == 1)    {
                // Lift the tabbar back up:
                Instance.OffsetTabBar(false);
            }
            if (m_tabs.Count > 1) {
                m_tabs[m_tabs.Count - 2].ShowTab(false);
            }
        }

        public static void ShowView(UIView view, bool showIt, bool tabChange)
        {
            bool explicitlyHidden = m_hiddenViews.Contains(view);
            if (explicitlyHidden && tabChange) {
                return;
            }

            view.Hidden = !showIt;

            if (tabChange) {
                return;
            }
            if (!showIt && !explicitlyHidden)   {
                m_hiddenViews.Add(view);
            } else if (showIt && explicitlyHidden) {
                m_hiddenViews.Remove(view);
            }
        }

        public static string ProcessClick(string arg)
        {
            var now = DateTime.Now.ToString("T");
            return "Clicks: " + arg + "\n" + now;
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }
    }
}
