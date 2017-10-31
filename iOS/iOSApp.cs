using System;
using System.Collections.Generic;
using CoreGraphics;
using SplitAndMerge;
using UIKit;

namespace scripting.iOS
{
  public class iOSTab
  {
    UIViewController m_tab;
    List<UIView> m_views = new List<UIView>();
    UIImage m_image;

    public iOSTab(UIViewController tab, string text)
    {
      m_tab = tab;
      OriginalText = text;
      Text = text;
    }
    public string OriginalText;
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
    public void RemoveAll()
    {
      for (int i = 0; i < m_views.Count; i++) {
        m_views[i].RemoveFromSuperview();
      }
    }

    public UIColor BackgroundColor {
      set {
        m_tab.View.BackgroundColor = value;
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

    public static Action<int> TabSelectedDelegate;

    int m_selectedTab = -1;

    public static iOSApp Instance { set; get; }
    public int SelectedTab { get { return m_selectedTab; } }

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

    public static int GetVerticalOffset()
    {
      if (iOSApp.CurrentOffset == 0) {
        return 0;
      }
      int offset = (int)(iOSApp.CurrentOffset * 0.8);
      CGSize screen = UtilsiOS.GetNativeScreenSize();
      // Special dealing with iPhone X:
      if (screen.Width == 1125) {
        offset += 6;
      }

      return offset;
    }

    public void OnTabSelected(object sender, UITabBarSelectionEventArgs e)
    {
      m_selectedTab = (int)e.ViewController.TabBarController.SelectedIndex;
      SelectTab(m_selectedTab);
      TabSelectedDelegate?.Invoke(m_selectedTab);
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
    public static void RemoveAll()
    {
      for (int i = 0; i < m_tabs.Count; i++) {
        m_tabs[i].RemoveAll();
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
      CommonFunctions.RegisterFunctions();

      //string[] lines = System.IO.File.ReadAllLines("script.cscs");
      string[] lines = System.IO.File.ReadAllLines("script.cscs");
      string script = string.Join("\n", lines);

      Variable result = null;
      try {
        result = Interpreter.Instance.Process(script);
      } catch (Exception exc) {
        Console.WriteLine("Exception: " + exc.Message);
        Console.WriteLine(exc.StackTrace);
        ParserFunction.InvalidateStacksAfterLevel(0);
        throw;
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
      var selImage = UtilsiOS.LoadImage(selectedImageName);
      selImage = selImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

      UIViewController tab = new UIViewController();
      tab.Title = text;
      tab.TabBarItem.SelectedImage = selImage;
      tab.TabBarItem.Image = selImage;

      if (notSelectedImageName != null) {
        var image = UtilsiOS.LoadImage(notSelectedImageName);
        image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        tab.TabBarItem.Image = image;
      }

      List<UIViewController> controllers = Instance.ViewControllers == null ?
          new List<UIViewController>() :
          new List<UIViewController>(Instance.ViewControllers);
      controllers.Add(tab);
      Instance.ViewControllers = controllers.ToArray();

      m_activeTab = new iOSTab(tab, text);
      m_tabs.Add(m_activeTab);
      if (controllers.Count == 1) {
        // Lift the tabbar back up:
        Instance.OffsetTabBar(false);
      }

      SelectTab(m_tabs.Count - 1);
    }

    public static void TranslateTabs()
    {
      foreach (var tab in m_tabs) {
        string translated = Localization.GetText(tab.OriginalText);
        tab.Text = translated;
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
      if (!showIt && !explicitlyHidden) {
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
      if (this.InterfaceOrientation == UIInterfaceOrientation.Portrait
          || this.InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown) {
        // portrait
      } else {
        // landsacpe
      }
      base.ViewDidLoad();
    }
  }
}
