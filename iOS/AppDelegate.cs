using System;
using Foundation;
using SplitAndMerge;
using UIKit;

namespace scripting.iOS
{
  // The UIApplicationDelegate for the application. This class is responsible for launching the
  // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
  [Register("AppDelegate")]
  public class AppDelegate : UIApplicationDelegate
  {
    static UIView m_view;
    static UIViewController m_viewController;

    public override UIWindow Window { get; set; }

    public static Action OnEnterBackgroundDelegate;

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
      Window = new UIWindow(UIScreen.MainScreen.Bounds);

      iOSApp controller = new iOSApp();
      Window.RootViewController = controller;
      Window.MakeKeyAndVisible();

      m_view = Window.RootViewController.View;
      m_viewController = Window.RootViewController;

      m_view.BackgroundColor = UIColor.White;

      // Will execute the CSCS script:
      controller.Run();

      m_view.AutoresizingMask = UIViewAutoresizing.All;
 
      return true;
    }
    public static UIView GetCurrentView()
    {
      return m_view;
    }
    public static UIViewController GetCurrentController()
    {
      return m_viewController;
    }

    public static void SetBgView(UIImage bg)
    {
      nint tag = 989898;
      var old = m_view.ViewWithTag(tag);
      if (old != null) {
        old.Hidden = true;
        old.RemoveFromSuperview();
      }

      var bgImageView = new UIImageView(m_view.Frame);
      bgImageView.Image = bg;
      bgImageView.Tag = tag;

      m_view.AddSubview(bgImageView);
      m_view.SendSubviewToBack(bgImageView);
    }

    public override void OnResignActivation(UIApplication application)
    {
      // Invoked when the application is about to move from active to inactive state.
      // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
      // or when the user quits the application and it begins the transition to the background state.
      // Games should use this method to pause the game.
    }

    public override void DidEnterBackground(UIApplication application)
    {
      // Use this method to release shared resources, save user data, invalidate timers and store the application state.
      // If your application supports background exection this method is called instead of WillTerminate when the user quits.
      OnEnterBackgroundDelegate?.Invoke();
    }

    public override void WillEnterForeground(UIApplication application)
    {
      // Called as part of the transiton from background to active state.
      // Here you can undo many of the changes made on entering the background.
    }

    public override void OnActivated(UIApplication application)
    {
      // Restart any tasks that were paused (or not yet started) while the application was inactive. 
      // If the application was previously in the background, optionally refresh the user interface.
    }

    public override void WillTerminate(UIApplication application)
    {
      // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
    }
  }
}

