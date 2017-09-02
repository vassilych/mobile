using System;
using System.Collections.Generic;

using Foundation;
using UIKit;

namespace scripting.iOS
{
    public partial class TabController2 : UITabBarController
    {
        UIViewController tab1, tab2, tab3;

        public List<string> images = new List<string>() { "learn.png", "test.png", "settings.png" };

        public TabController2() : base("TabController", null)
        {
            //          tab1.TabBarItem = new UITabBarItem (UITabBarSystemItem.History, 0); // sets image AND text
            //          tab2.TabBarItem = new UITabBarItem ("Orange", UIImage.FromFile("Images/first.png"), 1);
            //          tab3.TabBarItem = new UITabBarItem ();
            //          tab3.TabBarItem.Image = UIImage.FromFile("Images/second.png");
            //          tab3.TabBarItem.Title = "Rouge"; // this overrides tab3.Title set above
            //          tab3.TabBarItem.BadgeValue = "4";
            //          tab3.TabBarItem.Enabled = false;

            var tabs = new UIViewController[] {
                //tab1, tab2, tab3
            };

            ViewControllers = tabs;
            //OffsetTabBar(true);

            //SelectedViewController = tab2; // normally you would default to the left-most tab (ie. tab1)
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

        }

        public void OffsetTabBar(bool down = true)
        {
            var tabFrame = TabBar.Frame; //self.TabBar is IBOutlet of your TabBar
            int offset = down ? (int)tabFrame.Size.Height : -1 * (int)tabFrame.Size.Height;
            tabFrame.Y += offset; //.Offset(0, offset);
            TabBar.Frame = tabFrame;
        }

        public void AddTab(string text, string imageName)
        {
            UIViewController tab = new UIViewController();
            tab.Title = text;
            var image = UIImage.FromFile(imageName); ;
            image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            tab.TabBarItem.Image = image;
            List<UIViewController> ctr = ViewControllers == null ? 
                new List<UIViewController>() :
                new List<UIViewController>(ViewControllers);
            ctr.Add(tab);
            ViewControllers = ctr.ToArray();
            if (ViewControllers.Length == 1) {
                //OffsetTabBar(false);
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.
        }
    }
}

