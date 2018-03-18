using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using CoreGraphics;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace scripting.iOS
{
  public class UtilsiOS
  {
    public const int ROOT_TOP_MIN           = 18;
    public const int ROOT_BOTTOM_MIN        = 10;
    public const int ROOT_BOTTOM_MIN_X      = 14;

    // Extending Combobox width with respect to Android:
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
      if (widgetType == "Picker") {
        height = (int)(height * COMBOBOX_EXTENTION);
        //location.TranslationY += COMBOBOX_Y_MARGIN;
      } else if (widgetType == "AdMobBanner") {
        AdMob.GetAdSize(text, ref width, ref height);
      }
      if (widgetType == "TypePicker" || widgetType == "Picker") {
        int screenWidth = (int)UtilsiOS.GetRealScreenWidth();
        if (screenWidth <= AutoScaleFunction.BASE_WIDTH) {
          // Check Combo height for smaller iPhones
          height = Math.Max(height, MAX_COMBO_HEIGHT_SMALL);
        }
      }
      location.SetSize(width, height);
    }

    public static UIView ConvertFormsToNative(Xamarin.Forms.View view, CGRect size)
    {
      var renderer = Platform.CreateRenderer(view);

      renderer.NativeView.Frame = size;

      renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
      renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;

      renderer.Element.Layout(size.ToRectangle());

      var nativeView = renderer.NativeView;

      nativeView.SetNeedsLayout();

      return nativeView;
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

    public static Tuple<UIControlContentHorizontalAlignment, UITextAlignment>
                  GetAlignment(string alignment)
    {
      UIControlContentHorizontalAlignment al1 = UIControlContentHorizontalAlignment.Center;
      UITextAlignment al2 = UITextAlignment.Center;
      switch (alignment) {
        case "left":
          al1 = UIControlContentHorizontalAlignment.Left;
          al2 = UITextAlignment.Left;
          break;
        case "right":
          al1 = UIControlContentHorizontalAlignment.Right;
          al2 = UITextAlignment.Right;
          break;
        case "top":
        case "bottom": // there is no top & bottom for iOS, only for Android
        case "center":
          al1 = UIControlContentHorizontalAlignment.Center;
          al2 = UITextAlignment.Center;
          break;
        case "fill":
        case "natural":
          al1 = UIControlContentHorizontalAlignment.Fill;
          al2 = UITextAlignment.Natural;
          break;
        case "justified":
          al1 = UIControlContentHorizontalAlignment.Center;
          al2 = UITextAlignment.Justified;
          break;
      }
      return new Tuple<UIControlContentHorizontalAlignment, UITextAlignment>(al1, al2);
    }
    public static CGColor CGColorFromHex(string hexString, double alpha = 1.0)
    {
      UIColor uicolor = String2Color(hexString, alpha);
      return uicolor.CGColor;
    }
    public static UIColor UIColorFromHex(string hexString, double alpha = 1.0)
    {
      //int rgbValue = (int)new System.ComponentModel.Int32Converter().ConvertFromString(hexString);
      try {
        hexString = hexString.Replace("#", "");
        nfloat red = Convert.ToInt32(hexString.Substring(0, 2), 16) / 255f;
        nfloat green = Convert.ToInt32(hexString.Substring(2, 2), 16) / 255f;
        nfloat blue = Convert.ToInt32(hexString.Substring(4, 2), 16) / 255f;

        UIColor uicolor = new UIColor(red, green, blue, (nfloat)alpha);
        return uicolor;
      } catch (Exception exc) {
        throw new ArgumentException("Couldn't parse color [" + hexString + "]");
      }
    }
    public static UIColor String2Color(string colorStr, double alpha)
    {
      UIColor color = String2Color(colorStr);
      if (alpha < 1.0) {
        nfloat r, g, b, a;
        color.GetRGBA(out r, out g, out b, out a);
        color = UIColor.FromRGBA(r, g, b, (nfloat)alpha);
      }

      return color;
    }
    public static UIColor String2Color(string colorStr)
    {
      colorStr = colorStr.ToLower();
      switch (colorStr) {
        case "black": return UIColor.Black;
        case "blue": return UIColor.Blue;
        case "brown": return UIColor.Brown;
        case "clear": return UIColor.Clear;
        case "cyan": return UIColor.Cyan;
        case "dark_gray": return UIColor.DarkGray;
        case "dark_green": return UIColorFromHex("#006400");
        case "dark_red": return UIColorFromHex("#8B0000");
        case "deep_sky_blue": return UIColorFromHex("#00BFFF");
        case "deep_pink": return UIColorFromHex("#FF1493");
        case "gainsboro": return UIColorFromHex("#DCDCDC");;
        case "gray": return UIColor.Gray;
        case "green": return UIColor.Green;
        case "light_blue": return UIColorFromHex("#ADD8E6");
        case "light_cyan": return UIColorFromHex("#E0FFFF");
        case "light_gray": return UIColor.LightGray;
        case "light_green": return UIColorFromHex("#90EE90");
        case "light_yellow": return UIColorFromHex("#FFFFE0");
        case "magenta": return UIColor.Magenta;
        case "neutral": return NeutralColor;
        case "orange": return UIColor.Orange;
        case "pink": return UIColorFromHex("#FFC0CB");
        case "purple": return UIColor.Purple;
        case "red": return UIColor.Red;
        case "rose": return UIColorFromHex("#FF007F");
        case "sky_blue": return UIColorFromHex("#ADD8E6");
        case "silver": return UIColorFromHex("#C0C0C0");
        case "snow": return UIColorFromHex("#FFFAFA");
        case "transparent": return UIColor.Clear;
        case "white": return UIColor.White;
        case "white_smoke": return UIColorFromHex("#F5F5F5");
        case "yellow": return UIColor.Yellow;
        default: return UIColorFromHex(colorStr);
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
    public static UIImage AddMarge(UIImage img, CGSize destinationSize)
    {
      var delta = destinationSize.Width - img.Size.Width;

      if (delta <= 0) {
        return img;
      }
      var height = img.Size.Height;
      var width = img.Size.Width;

      UIImage transpImg = UtilsiOS.ImageOfSize("transparent", new CGSize(delta, height));
      UIImage newImage  = UtilsiOS.MergeImages(transpImg, img);

      return newImage;
    }
    public static UIImage ImageOfSize(string imageName, CGSize destinationSize)
    {
      UIImage img = UtilsiOS.LoadImage(imageName);
      UIGraphics.BeginImageContext(destinationSize);

      CGRect newRect = new CGRect(0, 0, destinationSize.Width, destinationSize.Height);
      img.Draw(newRect);
      UIImage newImage = UIGraphics.GetImageFromCurrentImageContext();

      UIGraphics.EndImageContext();

      return newImage;
    }
    public static UIImage MergeImages(UIImage image1, UIImage image2)
    {
      double newHeight = Math.Max(image1.Size.Height, image2.Size.Height);
      CGSize destinationSize = new CGSize(image1.Size.Width + image2.Size.Width, newHeight);

      UIGraphics.BeginImageContext(destinationSize);

      CGRect newRect = new CGRect(0, 0, image1.Size.Width, image1.Size.Height);
      image1.Draw(new CGRect(0, 0, image1.Size.Width, image1.Size.Height));
      image2.Draw(new CGRect(image1.Size.Width, 0, image2.Size.Width, image2.Size.Height));
      UIImage newImage = UIGraphics.GetImageFromCurrentImageContext();

      UIGraphics.EndImageContext();

      return newImage;
    }

    public static Stream ImageToStream(string imagePath)
    {
      var image = UtilsiOS.LoadImage(imagePath);
      Byte[] byteArray = null;
      using (var imageData = image.AsPNG()) {
        byteArray = new Byte[imageData.Length];
        Marshal.Copy(imageData.Bytes, byteArray, 0, Convert.ToInt32(imageData.Length));
      }
      var pngImageStream = new MemoryStream(byteArray);

      return pngImageStream;
    }

    public static UIImage CreateComboboxImage(CGRect rect)
    {
      UIImage comboImage = UtilsiOS.ImageOfSize("combobox", new CGSize(rect.Height, rect.Height));

      UIImage img = UtilsiOS.AddMarge(comboImage, new CGSize(rect.Width, rect.Height));
      return img;
    }

    [DllImport(ObjCRuntime.Constants.SystemLibrary)] 
    internal static extern int sysctlbyname( 
        [MarshalAs(UnmanagedType.LPStr)] string property, 
        IntPtr output, 
        IntPtr oldLen, 
        IntPtr newp, 
        uint newlen); 
    
    public static string GetSystemProperty(string property)
    {
        var pLen = Marshal.AllocHGlobal(sizeof(int));
        sysctlbyname(property, IntPtr.Zero, pLen, IntPtr.Zero, 0);
        var length = Marshal.ReadInt32(pLen);
        var pStr = Marshal.AllocHGlobal(length);
        sysctlbyname(property, pStr, pLen, IntPtr.Zero, 0);
        return Marshal.PtrToStringAnsi(pStr);
    } 
    public static string GetDeviceName()
    {
      string deviceName = GetSystemProperty("hw.machine");

      switch(deviceName) {
        case "x86_x64" : 
        case "i386" :      return "Simulator";
        case "iPhone3,1":
        case "iPhone3,2":
        case "iPhone3,3":  return "iPhone 4";
        case "iPhone4,1":  return "iPhone 4s";
        case "iPhone5,1":
        case "iPhone5,2":  return "iPhone 5";
        case "iPhone5,3":
        case "iPhone5,4":  return "iPhone 5c";
        case "iPhone6,1":
        case "iPhone6,2":  return "iPhone 5s";
        case "iPhone7,1":  return "iPhone 6 Plus";
        case "iPhone7,2":  return "iPhone 6";
        case "iPhone8,1":  return "iPhone 6s Plus";
        case "iPhone8,2":  return "iPhone 6s";
        case "iPhone8,4":  return "iPhone SE";
        case "iPhone9,1":
        case "iPhone9,3":  return "iPhone 7";
        case "iPhone9,2":
        case "iPhone9,4":  return "iPhone 7 Plus";
        case "iPhone10,1":
        case "iPhone10,4": return "iPhone 8";
        case "iPhone10,2":
        case "iPhone10,5": return "iPhone 8 Plus";
        case "iPhone10,3":
        case "iPhone10,6": return "iPhone X";
        case "iPad2,1":    return "iPad 2 Wi-Fi";
        case "iPad2,2":    return "iPad 2 GSM";
        case "iPad2,3":    return "iPad 2 CDMA";
        case "iPad2,4":    return "iPad 2 Wi-Fi Rev";
        case "iPad2,5":    return "iPad mini Wi-Fi";
        case "iPad2,6":
        case "iPad2,7":    return "iPad mini";
        case "iPad3,1":    return "iPad 3 Wi-Fi";
        case "iPad3,2":    return "iPad 3 Wi-Fi LTE";
        case "iPad3,3":    return "iPad 3 Wi-Fi LTE AT&T";
        case "iPad3,4":    return "iPad 4 Wi-Fi";
        case "iPad3,5":    
        case "iPad3,6":    return "iPad 4";
        case "iPad4,1":    return "iPad Air Wi-Fi";
        case "iPad4,2":    return "iPad Air Wi-Fi LTE";
        case "iPad4,3":    return "iPad Air Rev";
        case "iPad4,4":    return "iPad mini 2 Wi-Fi";
        case "iPad4,5":    return "iPad mini 2 Wi-Fi LTE";
        case "iPad4,6":    return "iPad mini 2 Rev";
        case "iPad4,7":    return "iPad mini 3 Wi-Fi";
        case "iPad4,8":
        case "iPad4,9":    return "iPad mini 3";
        case "iPad5,1":    return "iPad mini 4 Wi-Fi";
        case "iPad5,2":    return "iPad mini 4 Wi-Fi LTE";
        case "iPad5,3":    return "iPad Air 2 Wi-Fi";
        case "iPad5,4":    return "iPad Air 2 Wi-Fi LTE";
        case "iPad6,3":    return "iPad Pro 9.7 Wi-Fi";
        case "iPad6,4":    return "iPad Pro 9.7 Wi-Fi LTE";
        case "iPad6,7":    return "iPad Pro 12.9 Wi-Fi";
        case "iPad6,8":    return "iPad Pro 12.9 Wi-Fi LTE";
        case "iPad6,11":   return "iPad 5 9.7 Wi-Fi";
        case "iPad6,12":   return "iPad 5 9.7 Wi-Fi LTE";
        case "iPad7,3":
        case "iPad7,4":    return "iPad Pro 10.5";
        case "iPod1,1":    return "iPod Touch";
        case "iPod2,1":    return "iPod Touch 1 Gen";
        case "iPod3,1":    return "iPod Touch 2 Gen";
        case "iPod4,1":    return "iPod Touch 3 Gen";
        case "iPod5,1":    return "iPod Touch 4 Gen";
        case "iPod6,1":    return "iPod Touch 5 Gen";
        case "iPod7,1":    return "iPod Touch 6 Gen";
      }
      return deviceName;
    }
  }
}
