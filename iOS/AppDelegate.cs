using System;
using Foundation;
using UIKit;

using SplitAndMerge;

namespace scripting.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public enum Style
        {
            TABS, NAVI, PAGE
        };
        static Style m_style = Style.TABS;
        static UIView m_view;
        static iOSApp m_viewController;
        static UIWindow MainWindow { get; set; }

        public override UIWindow Window { get; set; }

        public static Action OnEnterBackgroundDelegate;

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            MainWindow = Window = new UIWindow(UIScreen.MainScreen.Bounds);

            m_viewController = new iOSApp();
            Window.RootViewController = m_viewController;
            Window.MakeKeyAndVisible();

            m_view = Window.RootViewController.View;
            m_view.BackgroundColor = UIColor.White;

            // 16.2.0.41
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjEzMjM3QDMxMzYyZTMyMmUzMG8zdVhtRjZlYzJmZGNaNzkwTlRNeVFMZkxyVld5QjBuRFBMYUZhdEV1QlU9");
            // 17.4.0.39
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjEzMjM4QDMxMzcyZTM0MmUzMFpUdERwTWxzdGJwbm43Mit3dk1qZXpqcTZCcUdMaDM4YTlidktSY2Q1M2s9");

            // Will execute the CSCS script:
            m_viewController.Run();

            /*var segmented = CreateSegmentedControl();
            segmented.Frame = new CoreGraphics.CGRect(10, 10, 600, 50);
            var view1 = new UIView(new CoreGraphics.CGRect(0, 340, 620, 50));
            m_view.AddSubview(view1);
            view1.AddSubview(segmented);*/

            m_view.AutoresizingMask = UIViewAutoresizing.All;
            return true;
        }

        Syncfusion.iOS.Buttons.SfSegmentedControl CreateSegmentedControl()
        {
            Syncfusion.iOS.Buttons.SfSegmentedControl m_segmented = new Syncfusion.iOS.Buttons.SfSegmentedControl();

            var SizeCollection = new System.Collections.ObjectModel.ObservableCollection<Syncfusion.iOS.Buttons.SfSegmentItem>
            {
                new Syncfusion.iOS.Buttons.SfSegmentItem(){Text = "A", IconFont="XS"},
                 new Syncfusion.iOS.Buttons.SfSegmentItem(){Text = "A", IconFont="S"},
                  new Syncfusion.iOS.Buttons.SfSegmentItem(){Text = "A", IconFont="M"},
                   new Syncfusion.iOS.Buttons.SfSegmentItem(){Text = "A", IconFont="L"},
                    new Syncfusion.iOS.Buttons.SfSegmentItem(){Text = "A", IconFont="XL"},
            };

            m_segmented.ItemsSource = SizeCollection;
            m_segmented.CornerRadius = 25;
            m_segmented.BorderColor = UIColor.Black;
            m_segmented.SelectionTextColor = UIColor.White;
            m_segmented.DisplayMode = Syncfusion.iOS.Buttons.SegmentDisplayMode.Image;
            m_segmented.Font = UIFont.SystemFontOfSize(16);
            m_segmented.FontIconFontColor = UIColor.Black;
            m_segmented.VisibleSegmentsCount = 5;
            m_segmented.SegmentHeight = 50;
            m_segmented.BorderThickness = 1;
            m_segmented.SegmentWidth = 20;

            m_segmented.SelectionIndicatorSettings = new Syncfusion.iOS.Buttons.SelectionIndicatorSettings()
            { Color = UIColor.FromRGB(44, 123, 188), Position = Syncfusion.iOS.Buttons.SelectionIndicatorPosition.Fill };

            //m_segmented.Frame = m_rect;
            return m_segmented;
        }

        public static void SetController(Style style, string type, string orient)
        {
            m_style = style;
            UIViewController controller = null;
            switch (m_style)
            {
                case Style.TABS:
                    controller = new iOSApp(); // This is a UITabBarController.
                    break;
                case Style.NAVI:
                    controller = new UINavigationController();
                    break;
                case Style.PAGE:
                    controller = new UIPageViewController(type == "scroll" ?
                      UIPageViewControllerTransitionStyle.Scroll :
                      UIPageViewControllerTransitionStyle.PageCurl,
                                                          orient == "horizontal" ?
                      UIPageViewControllerNavigationOrientation.Horizontal :
                      UIPageViewControllerNavigationOrientation.Vertical);
                    break;
            }

            MainWindow.RootViewController = controller;
            MainWindow.MakeKeyAndVisible();
            m_view = MainWindow.RootViewController.View;
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
            if (old != null)
            {
                old.Hidden = true;
                old.RemoveFromSuperview();
            }

            var bgImageView = new UIImageView(m_view.Frame);
            bgImageView.Image = bg;
            bgImageView.Tag = tag;

            m_view.AddSubview(bgImageView);
            m_view.SendSubviewToBack(bgImageView);
        }
        public static void SetBgColor(string colorStr, double alpha = 1.0)
        {
            var color = UtilsiOS.String2Color(colorStr, alpha);
            m_view.BackgroundColor = color;
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

