﻿using System;

using UIKit;

using SplitAndMerge;
using CoreGraphics;
using Foundation;

namespace scripting.iOS
{
  public partial class ViewController : UIViewController
  {
    public ViewController(IntPtr handle) : base(handle)
    {
    }

    public override void ViewDidLoad()
    {
      base.ViewDidLoad();
    }
    public override void DidReceiveMemoryWarning()
    {
      base.DidReceiveMemoryWarning();
      // Release any cached data, images, etc that aren't in use.        
    }

    public override void ViewWillTransitionToSize(CoreGraphics.CGSize toSize,
                                                  IUIViewControllerTransitionCoordinator coordinator)
    {
    }
  }
}
