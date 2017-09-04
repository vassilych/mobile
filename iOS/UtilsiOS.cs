﻿using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;

namespace scripting.iOS
{
    public class UtilsiOS
    {
        const int ROOT_TOP_MIN    = 18;
        const int ROOT_BOTTOM_MIN = 10;

        public delegate void AfterPause();

        static Dictionary<string, double> m_doubleCache = new Dictionary<string, double>();

        static public UIColor NeutralColor
        {
            get {
                return UIColor.FromRGBA((nfloat)(0.0 / 255.0), (nfloat)(191.0 / 255.0),
                                        (nfloat)(255.0 / 255.0), (nfloat)(1.0));
            }
        }

        static public void Pause(UIViewController controller, double interval,
                                 AfterPause AfterPauseMethod)
        {
            System.Timers.Timer pauseTimer = new System.Timers.Timer(interval);
            pauseTimer.AutoReset = false;
            pauseTimer.Enabled = true;
            pauseTimer.Elapsed += (sender, e) =>
            {
                controller.InvokeOnMainThread(() =>
                {
                    AfterPauseMethod();
                });
                pauseTimer.Close();
            };
        }

        public static void ShowToast(String message, UIColor color,
                                     double duration = 10.0f)
        {
            CGSize screen = UtilsiOS.GetScreenSize();
            var size = new CGSize(300, 60);
            var x = (screen.Height - size.Width) / 2.0;
            var y = screen.Width - size.Height - 150;
            var point = new CGPoint(x, y);

            UIView view = AppDelegate.GetCurrentView();

            ShowToast(message, color, view, point, size, duration);
        }

        public static void ShowToast(String message, UIColor color, UIView view,
                                     CGPoint point, CGSize size,
                                     double duration = 2.0f)
        {
            nint tag = 1989;

            UIView residualView = view.ViewWithTag(tag);
            if (residualView != null)
                residualView.RemoveFromSuperview();

            //var viewBack = new UIView(new CGRect(83, 0, 300, 100));
            var viewBack = new UIView(new CGRect(point, size));
            viewBack.BackgroundColor = color;
            viewBack.Tag = tag;
            UILabel lblMsg = new UILabel(new CGRect(0, 0, size.Width, size.Height));
            lblMsg.Lines = 2;
            lblMsg.Text = message;
            lblMsg.TextColor = UIColor.White;
            lblMsg.TextAlignment = UITextAlignment.Center;
            //viewBack.Center = view.Center;
            //viewBack.Frame.Location = point;
            viewBack.AddSubview(lblMsg);
            view.AddSubview(viewBack);
            UIView.BeginAnimations("Toast");
            UIView.SetAnimationDuration(duration);
            viewBack.Alpha = 0.0f;
            UIView.CommitAnimations();
        }

        public delegate void AlertOKCancelDelegate(bool OK);
        public static void OKCancelDialog(UIViewController controller, string title, string msg, AlertOKCancelDelegate action = null)
        {
            var okCancelAlertController = UIAlertController.Create(title, msg, UIAlertControllerStyle.Alert);
            okCancelAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default,
                                              alert => {
                                                  if (action != null) {
                                                      action(true);
                                                  }
                                              }));
            if (action != null) {
                okCancelAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel,
                                                  alert => action(false)));
            }

            controller.PresentViewController(okCancelAlertController, true, null);
        }

        public static CGSize GetScreenSize()
        {
            //var bounds = UIScreen.MainScreen.NativeBounds;
            var bounds = UIScreen.MainScreen.Bounds;
            return new CGSize(bounds.Width, bounds.Height);
        }
        public static CGSize GetNativeScreenSize()
        {
            //var bounds = UIScreen.MainScreen.NativeBounds;
            var bounds = UIScreen.MainScreen.NativeBounds;
            return new CGSize(bounds.Width, bounds.Height);
        }
        public static double GetScreenRatio()
        {
            double xdiff = 0.0;
            if (m_doubleCache.TryGetValue("screenRatio", out xdiff)) {
                return xdiff;
            }
            var native = UIScreen.MainScreen.NativeBounds;
            var bounds = UIScreen.MainScreen.Bounds;
            xdiff = native.Width  / bounds.Width;
            double ydiff = native.Height / bounds.Height;

            xdiff = xdiff == 0.0 ? 1.0 : xdiff;
            m_doubleCache["screenRatio"] = xdiff;
            return xdiff;
        }
        public static CGSize GetMiddlePoint()
        {
            var size = GetScreenSize();
            return new CGSize((int)(size.Width/2), (int)(size.Height/2));
        }

        public static int String2Position(string param, UIView referenceView,
                                          iOSVariable location, CGSize parentSize, bool isX)
        {
            bool useRoot = referenceView == null;
            
            int refX      = useRoot ? 0 : (int)referenceView.Frame.Location.X;
            int refY      = useRoot ? 0 : (int)referenceView.Frame.Location.Y;
            int refWidth  = useRoot ? (int)parentSize.Width : (int)referenceView.Frame.Size.Width;
            int refHeight = useRoot ? (int)parentSize.Height : (int)referenceView.Frame.Size.Height;

            int parentWidth  = (int)parentSize.Width;
            int parentHeight = (int)parentSize.Height;

            int widgetWidth  = (int)location.Width;
            int widgetHeight = (int)location.Height;

            switch (param)
            {
                case "ALIGN_LEFT": // X
                    return useRoot ? 0 :
                                     refX;
                case "LEFT": // X
                    return useRoot ? 0 :
                                     refX - widgetWidth;
                case "ALIGN_RIGHT": // X
                    return useRoot ? parentWidth - widgetWidth :
                                     refX + refWidth - widgetWidth;
                case "RIGHT": // X
                    return useRoot ? parentWidth -  widgetWidth :
                                     refX + refWidth;
                case "ALIGN_PARENT_TOP":
                case "ALIGN_TOP": // Y
                    return useRoot ? ROOT_TOP_MIN :
                                     refY;
                case "TOP":
                    return useRoot ? ROOT_TOP_MIN :
                                     refY - widgetHeight;
                case "ALIGN_PARENT_BOTTOM":
                case "ALIGN_BOTTOM":
                    int offset1 = useRoot ? parentHeight - widgetHeight - ROOT_BOTTOM_MIN :
                                            refY + refHeight - widgetHeight;
                    // if there is a tabbar, move the bottom part up:
                    if (useRoot && !isX) {
                        offset1 -= (int)(iOSApp.CurrentOffset * 0.8);
                    }
                    return offset1;
                case "BOTTOM":
                    int offset2 = useRoot ? parentHeight - widgetHeight - ROOT_BOTTOM_MIN :
                                           refY + refHeight;
                    // if there is a tabbar, move the bottom part up:
                    if (useRoot && !isX) {
                        offset2 -= (int)(iOSApp.CurrentOffset * 0.8);
                    }
                    return offset2;
                case "CENTER":
                    if (useRoot) {
                        return isX ? (parentWidth  - widgetWidth ) / 2 :
                                     (parentHeight - widgetHeight) / 2 ;
                    } else {
                        return isX ? refX + (refWidth  - widgetWidth ) / 2 :
                                     refY + (refHeight - widgetHeight) / 2;
                    }
                default:
                    return 0;
            }
        }

        public static CGColor CGColorFromHex(string hexString)
        {
            int rgbValue = (int)new System.ComponentModel.Int32Converter().ConvertFromString(hexString);

            var red   = (((rgbValue >> 16) & 0xFF0000) / 255.0);
            var green = (((rgbValue >> 8)  & 0x00FF00) / 255.0);
            var blue  = (((rgbValue >> 0)  & 0x0000FF) / 255.0);

            UIColor uicolor = new UIColor((nfloat)red, (nfloat)green, (nfloat)blue, (nfloat)1.0);
            return uicolor.CGColor;
        }
    }
}