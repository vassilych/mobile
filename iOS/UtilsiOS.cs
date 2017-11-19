using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;

namespace scripting.iOS
{
  public class UtilsiOS
  {
    public const int ROOT_TOP_MIN           = 18;
    public const int ROOT_BOTTOM_MIN        = 10;
    public const int ROOT_BOTTOM_MIN_X      = 14;

    // Extending Combobox width with respect to Android:
    //public const double COMBOBOX_EXTENTION = 2.4;
    public const double COMBOBOX_EXTENTION  = 3.6;
    public const int MAX_COMBO_HEIGHT_SMALL = 144;
    public const int COMBOBOX_Y_MARGIN      = -20;

    static Dictionary<string, double> m_doubleCache = new Dictionary<string, double>();

    public delegate void AfterPause();
    static public void Pause(UIViewController controller, double interval,
                             AfterPause AfterPauseMethod)
    {
      System.Timers.Timer pauseTimer = new System.Timers.Timer(interval);
      pauseTimer.AutoReset = false;
      pauseTimer.Enabled = true;
      pauseTimer.Elapsed += (sender, e) => {
        controller.InvokeOnMainThread(() => {
          AfterPauseMethod();
        });
        pauseTimer.Close();
      };
    }

    public static void AdjustSizes(string widgetType, iOSVariable location,
                                   string text, ref int width, ref int height)
    {
      if (widgetType == "Combobox") {
        height = (int)(height * COMBOBOX_EXTENTION);
        //location.TranslationY += COMBOBOX_Y_MARGIN;
      } else if (widgetType == "AdMobBanner") {
        AdMob.GetAdSize(text, ref width, ref height);
      }
      if (widgetType == "Combobox" || widgetType == "TypePicker") {
        int screenWidth = (int)UtilsiOS.GetRealScreenWidth();
        if (screenWidth <= AutoScaleFunction.BASE_WIDTH) {
          // Check Combo height for smaller iPhones
          height = Math.Max(height, MAX_COMBO_HEIGHT_SMALL);
        }
      }
      location.SetSize(width, height);
    }

    public static void ShowToast(String message, UIColor fgColor = null,
                                 UIColor bgColor = null,
                                 double duration = 10.0f,
                                 float width = 320, float height = 60)
    {
      if (fgColor == null) {
        fgColor = UIColor.White;
      }
      if (bgColor == null) {
        bgColor = UIColor.Gray;
      }
      CGSize screen = UtilsiOS.GetScreenSize();
      var size = new CGSize(width, height);
      var x = (screen.Width - size.Width) / 2.0;
      var y = screen.Height - size.Height - 120 * WidthMultiplier();
      var point = new CGPoint(x, y);

      UIView view = AppDelegate.GetCurrentView();

      ShowToast(message, fgColor, bgColor, view, point, size, duration);
    }

    public static void ShowToast(String message, UIColor fgColor,
                                 UIColor bgColor,
                                 UIView view,
                                 CGPoint point, CGSize size,
                                 double duration)
    {
      nint tag = 1917;

      UIView residualView = view.ViewWithTag(tag);
      if (residualView != null) {
        residualView.RemoveFromSuperview();
      }

      var viewBack = new UIView(new CGRect(point, size));
      viewBack.BackgroundColor = bgColor;
      viewBack.Tag = tag;
      viewBack.Layer.CornerRadius = 20;
      viewBack.Layer.MasksToBounds = true;

      UILabel lblMsg = new UILabel(new CGRect(0, 0, size.Width, size.Height));
      lblMsg.Lines = 4;
      lblMsg.Text = message;
      lblMsg.TextColor = fgColor;
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

    public static CGSize GetNativeScreenSize()
    {
      var bounds = UIScreen.MainScreen.NativeBounds;
      var width = bounds.Width;
      var height = bounds.Height;
      return new CGSize(width, height);
    }
    public static CGSize GetScreenSize()
    {
      //var bounds = UIScreen.MainScreen.NativeBounds;
      var bounds = UIScreen.MainScreen.Bounds;
      var width = bounds.Width;
      var height = bounds.Height;
      return new CGSize(width, height);
    }
    public static int GetRealScreenWidth()
    {
      var bounds = UIScreen.MainScreen.NativeBounds;
      var width  = bounds.Width < bounds.Height ?
                   bounds.Width : bounds.Height;
      return (int)width;
    }
    public static int GetRealScreenHeight()
    {
      var bounds = UIScreen.MainScreen.NativeBounds;
      var height = bounds.Width > bounds.Height ?
                   bounds.Width : bounds.Height;
      return (int)height;
    }
    public static double WidthMultiplier()
    {
      var size = GetNativeScreenSize();
      return size.Width / AutoScaleFunction.BASE_WIDTH;
    }
    public static double GetScreenRatio()
    {
      double xdiff = 0.0;
      if (m_doubleCache.TryGetValue("screenRatio", out xdiff)) {
        return xdiff;
      }
      var native = UIScreen.MainScreen.NativeBounds;
      var bounds = UIScreen.MainScreen.Bounds;
      var nativeWidth   = Math.Min(native.Width, native.Height);
      var currWidth = Math.Min(bounds.Width, bounds.Height);
      xdiff = currWidth > 0 && nativeWidth > 0 ? nativeWidth / currWidth : 1.0;

      m_doubleCache["screenRatio"] = xdiff;
      return xdiff;
    }
    public static int String2Position(string param, UIView referenceView,
                                      iOSVariable location, CGSize parentSize, bool isX)
    {
      bool useRoot = referenceView == null;

      int refX = useRoot ? 0 : (int)referenceView.Frame.Location.X;
      int refY = useRoot ? 0 : (int)referenceView.Frame.Location.Y;
      int refWidth = useRoot ? (int)parentSize.Width : (int)referenceView.Frame.Size.Width;
      int refHeight = useRoot ? (int)parentSize.Height : (int)referenceView.Frame.Size.Height;

      int parentWidth = (int)parentSize.Width;
      int parentHeight = (int)parentSize.Height;

      int widgetWidth = (int)location.Width;
      int widgetHeight = (int)location.Height;

      switch (param) {
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
          return useRoot ? parentWidth - widgetWidth :
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
          int offset1 = useRoot ? parentHeight - widgetHeight :
                                  refY + refHeight - widgetHeight;
          // if there is a tabbar, move the bottom part up:
          if (useRoot && !isX) {
            offset1 -= iOSApp.GetVerticalOffset();
          }
          return offset1;
        case "BOTTOM":
          int offset2 = useRoot ? parentHeight - widgetHeight :
                                 refY + refHeight;
          // if there is a tabbar, move the bottom part up:
          if (useRoot && !isX) {
            offset2 -= iOSApp.GetVerticalOffset();
          }
          return offset2;
        case "CENTER":
          if (useRoot) {
            return isX ? (parentWidth - widgetWidth) / 2 :
                         (parentHeight - widgetHeight) / 2;
          } else {
            return isX ? refX + (refWidth - widgetWidth) / 2 :
                         refY + (refHeight - widgetHeight) / 2;
          }
        default:
          return 0;
      }
    }

    public static CGColor CGColorFromHex(string hexString, double alpha = 1.0)
    {
      UIColor uicolor = String2Color(hexString, alpha);
      return uicolor.CGColor;
    }
    public static UIColor UIColorFromHex(string hexString, double alpha = 1.0)
    {
      int rgbValue = (int)new System.ComponentModel.Int32Converter().ConvertFromString(hexString);

      var red = (((rgbValue >> 16) & 0xFF0000) / 255.0);
      var green = (((rgbValue >> 8) & 0x00FF00) / 255.0);
      var blue = (((rgbValue >> 0) & 0x0000FF) / 255.0);

      UIColor uicolor = new UIColor((nfloat)red, (nfloat)green, (nfloat)blue, (nfloat)alpha);
      return uicolor;
    }
    public static UIColor String2Color(string colorStr, double alpha = 1.0)
    {
      switch (colorStr) {
        case "black": return UIColor.Black;
        case "blue": return UIColor.Blue;
        case "brown": return UIColor.Brown;
        case "clear": return UIColor.Clear;
        case "cyan": return UIColor.Cyan;
        case "dark_gray": return UIColor.DarkGray;
        case "gray": return UIColor.Gray;
        case "green": return UIColor.Green;
        case "light_gray": return UIColor.LightGray;
        case "magenta": return UIColor.Magenta;
        case "orange": return UIColor.Orange;
        case "purple": return UIColor.Purple;
        case "red": return UIColor.Red;
        case "white": return UIColor.White;
        case "yellow": return UIColor.Yellow;
        case "transparent": return UIColor.Clear;
        case "neutral": return NeutralColor;
        default: return UIColorFromHex(colorStr, alpha);
      }
    }
    public static UIColor NeutralColor {
      get {
        return UIColor.FromRGBA((nfloat)(0.0 / 255.0), (nfloat)(191.0 / 255.0),
                                (nfloat)(255.0 / 255.0), (nfloat)(1.0));
      }
    }

    public static UIImage LoadImage(string imageName)
    {
      string imageNameConverted = UIUtils.String2ImageName(imageName);
      // This also caches the result:
      UIImage img = UIImage.FromBundle("drawable/" + imageNameConverted);
      return img;
    }
  }
}
