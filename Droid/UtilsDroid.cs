using System;
using System.Collections.Generic;
using System.IO;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using SplitAndMerge;
namespace scripting.Droid
{
  public class ScreenSize
  {
    public ScreenSize(int width, int height)
    {
      Width  = width;
      Height = height;
    }
    public int Width  { get; set; }
    public int Height { get; set; }
  }
  public class UtilsDroid
  {
    public const int SWITCH_MARGIN = -20;

    public static int ExtraMargin(DroidVariable widgetFunc, ScreenSize screenSize, double multiplier)
    {
      int offset = 0;
      if (widgetFunc.ViewX is Switch) {
        offset = (int)AutoScaleFunction.TransformSize(UtilsDroid.SWITCH_MARGIN, screenSize.Width, 3);
        if (screenSize.Width <= AutoScaleFunction.BASE_WIDTH) {
          offset = SWITCH_MARGIN; // from -45, 480
        }
        //offset = -112; // (before -168) // from 1200
        //offset = -135; // from 1440
      }
      return offset;
    }
    public static ScreenSize GetScreenSize()
    {
      DisplayMetrics displayMetrics = new DisplayMetrics();
      var winManager = MainActivity.TheView.WindowManager;
      winManager.DefaultDisplay.GetMetrics(displayMetrics);

      int width  = displayMetrics.WidthPixels < displayMetrics.HeightPixels ?
                   displayMetrics.WidthPixels : displayMetrics.HeightPixels;
      int height = displayMetrics.WidthPixels > displayMetrics.HeightPixels ?
                   displayMetrics.WidthPixels : displayMetrics.HeightPixels;

      return new ScreenSize(width, height);
    }

    public static void AddViewBorder(View view, Color borderColor = new Color(), int width = 1, int corner = 5)
    {
      GradientDrawable drawable = new GradientDrawable();
      drawable.SetShape(ShapeType.Rectangle);
      drawable.SetCornerRadius(corner);
      drawable.SetStroke(width, borderColor);
      //drawable.SetColor(borderColor);
      view.Background = drawable;
    }

    /*public static View ConvertFormsToNative(Xamarin.Forms.View view,
                                            Xamarin.Forms.Rectangle size)
    {
      var vRenderer = Platform.CreateRendererWithContext(view, MainActivity.TheView);
      var androidView = vRenderer.View;
      vRenderer.Tracker.UpdateLayout();
      var layoutParams = new ViewGroup.LayoutParams((int)size.Width, (int)size.Height);
      androidView.LayoutParameters = layoutParams;
      view.Layout(size);
      androidView.Layout(0, 0, (int)view.WidthRequest, (int)view.HeightRequest);
      return androidView;
    }*/

    public static LayoutRules String2LayoutParam(DroidVariable location, bool isX)
    {
      View referenceView = isX ? location.ViewX : location.ViewY;
      string param       = isX ? location.RuleX : location.RuleY;

      bool useRoot = referenceView == null;

      DroidVariable refLocation = isX ? location.RefViewX?.Location as DroidVariable :
                                        location.RefViewY?.Location as DroidVariable;

      // If the reference view has a margin, add it to the current view.
      // This is the iOS functionality.
      if (isX && refLocation != null && !location.IsAdjustedX) {
        location.TranslationX += refLocation.TranslationX;
        //} else if (!isX && refLocation != null && !location.IsAdjustedY) {
        //  location.TranslationY += refLocation.TranslationY - refLocation.ExtraY;
      } else if (!isX && refLocation != null) {
        location.TranslationY += refLocation.TranslationY;
      }

      switch (param) {
        case "LEFT":
          return useRoot ? LayoutRules.AlignParentLeft :
                           LayoutRules.LeftOf;
        case "RIGHT":
          return useRoot ? LayoutRules.AlignParentRight :
                           LayoutRules.RightOf;
        case "TOP":
          return useRoot ? LayoutRules.AlignParentTop :
                           LayoutRules.Above;
        case "BOTTOM":
          return useRoot ? LayoutRules.AlignParentBottom :
                           LayoutRules.Below;
        case "CENTER":
          if (useRoot) {
            return isX ? LayoutRules.CenterHorizontal :
                         LayoutRules.CenterVertical;
          } else {
            if (isX) {
              location.TranslationX += (refLocation.Width - location.Width) / 2;
              return LayoutRules.AlignLeft;
            } else {
              int delta = (refLocation.Height - location.Height) / 2;
              location.TranslationY += delta;
              return LayoutRules.AlignTop;// .AlignBaseline;
            }
          }
        case "ALIGN_LEFT":
          return LayoutRules.AlignLeft;
        case "ALIGN_RIGHT":
          return LayoutRules.AlignRight;
        case "ALIGN_TOP":
          return LayoutRules.AlignTop;
        case "ALIGN_BOTTOM":
          return LayoutRules.AlignBottom;
        case "ALIGN_PARENT_TOP":
          return LayoutRules.AlignParentTop;
        case "ALIGN_PARENT_BOTTOM":
          return LayoutRules.AlignParentBottom;
        default:
          return LayoutRules.AlignStart;
      }
    }

    static Dictionary<string, int> m_pics = new Dictionary<string, int>();
    public static int String2Pic(string name)
    {
      string imagefileName = UIUtils.String2ImageName(name);
      int resourceID = 0;
      if (m_pics.TryGetValue(imagefileName, out resourceID)) {
        return resourceID;
      }
      var fieldInfo = typeof(Resource.Drawable).GetField(imagefileName);
      /*if (fieldInfo == null) {
        imagefileName = imagefileName.Replace("_", "");
        fieldInfo = typeof(Resource.Drawable).GetField(imagefileName);
      }*/
      if (fieldInfo == null) {
        Console.WriteLine("Couldn't find pic [{0}] for [{1}]", imagefileName, name);
        return -999;
      }
      resourceID = (int)fieldInfo.GetValue(null);
      m_pics[imagefileName] = resourceID;
      return resourceID;
    }
    public static JavaList<string> GetJavaStringList(List<string> items, string first = null)
    {
      JavaList<string> javaObjects = new JavaList<string>();
      if (first != null) {
        javaObjects.Add(first);
      }
      for (int index = 0; index < items.Count; index++) {
        string item = items[index];
        javaObjects.Add(item);
      }

      return javaObjects;
    }
    public static JavaList<int> GetJavaPicList(List<string> items, string first = null)
    {
      JavaList<int> javaObjects = new JavaList<int>();
      if (first != null) {
        javaObjects.Add(-1);
      }
      for (int index = 0; index < items.Count; index++) {
        string item = items[index];
        int picId = UtilsDroid.String2Pic(item);
        javaObjects.Add(item);
      }

      return javaObjects;
    }
    public static List<int> GetPicList(List<string> items, string first = null)
    {
      List<int> pics = new List<int>();
      if (first != null) {
        pics.Add(-1);
      }
      for (int index = 0; index < items.Count; index++) {
        string item = items[index];
        int picId = UtilsDroid.String2Pic(item);
        pics.Add(picId);
      }

      return pics;
    }

    public static Java.Util.Locale LocaleFromString(string voice, bool display = true)
    {
      string voice_ = voice.Replace("-", "_");
      int index = voice_.IndexOf('_');
      if (index <= 0) {
        return new Java.Util.Locale(voice_);
      }

      string language = voice_.Substring(0, index).ToLower();
      string country = voice_.Substring(index + 1).ToUpper();
      if (language.Equals("en")) {
        if (country == "US" || display) {
          return Java.Util.Locale.Us;
        }
        return Java.Util.Locale.Uk;
      }
      if (language.Equals("de")) {
        if (country == "DE" || display) {
          return display ? Java.Util.Locale.Germany : Java.Util.Locale.German;
        } else {
          return new Java.Util.Locale(language, country);
        }
      }
      if (language.Equals("fr")) {
        return display ? Java.Util.Locale.France : Java.Util.Locale.French;
      }
      if (language.Equals("it")) {
        return display ? Java.Util.Locale.Italy : Java.Util.Locale.Italian;
      }
      if (language.Equals("zh")) {
        return display ? Java.Util.Locale.China : Java.Util.Locale.Chinese;
      }
      if (language.Equals("ja")) {
        return display ? Java.Util.Locale.Japan : Java.Util.Locale.Japanese;
      }

      if (display || language.Equals("ar")) {
        return new Java.Util.Locale(language);
      } else {
        return new Java.Util.Locale(language, country);
      }
    }
    public static Tuple<GravityFlags, TextAlignment> GetAlignment(string alignment)
    {
      GravityFlags al1 = GravityFlags.Left;
      TextAlignment al2 = TextAlignment.TextStart;

      switch (alignment) {
        case "left":
          al1 = GravityFlags.Left;
          al2 = TextAlignment.TextStart;
          break;
        case "right":
          al1 = GravityFlags.Right;
          al2 = TextAlignment.TextEnd;
          break;
        case "fill":
        case "natural":
          al1 = GravityFlags.Fill;
          al2 = TextAlignment.Gravity;
          break;
        case "top":
          al1 = GravityFlags.Top;
          al2 = TextAlignment.Gravity;
          break;
        case "bottom":
          al1 = GravityFlags.Bottom;
          al2 = TextAlignment.Gravity;
          break;
        case "center":
          al1 = GravityFlags.Center;
          al2 = TextAlignment.Center;
          break;
        case "justified":
          al1 = GravityFlags.ClipHorizontal;
          al2 = TextAlignment.Gravity;
          break;
      }
      return new Tuple<GravityFlags, TextAlignment>(al1, al2);
    }
    public static Color NeutralColor {
      get => Color.Rgb(0, 191, 255);
    }
    public static Color String2Color(string colorStr)
    {
      colorStr = colorStr.ToLower();
      switch (colorStr) {
        case "black": return Color.Black;
        case "blue": return Color.Blue;
        case "brown": return Color.Brown;
        case "clear": return Color.Transparent;
        case "cyan": return Color.Cyan;
        case "dark_gray": return Color.DarkGray;
        case "dark_green": return Color.DarkGreen;
        case "dark_red": return Color.DarkRed;
        case "deep_pink": return Color.DeepPink;
        case "deep_sky_blue": return Color.DeepSkyBlue;
        case "gainsboro": return Color.Gainsboro;
        case "gray": return Color.Gray;
        case "green": return Color.Green;
        case "light_blue": return Color.LightBlue;
        case "light_cyan": return Color.LightCyan;
        case "light_gray": return Color.LightGray;
        case "light_green": return Color.LightGreen;
        case "light_yellow": return Color.LightYellow;
        case "magenta": return Color.Magenta;
        case "neutral": return NeutralColor;
        case "orange": return Color.Orange;
        case "pink": return Color.Pink;
        case "purple": return Color.Purple;
        case "rose": return Color.RosyBrown;
        case "red": return Color.Red;
        case "sky_blue": return Color.SkyBlue;
        case "silver": return Color.Silver;
        case "snow": return Color.Snow;
        case "transparent": return Color.Transparent;
        case "white": return Color.White;
        case "white_smoke": return Color.WhiteSmoke;
        case "yellow": return Color.Yellow;

        default: return Color.ParseColor(colorStr);
      }
    }
    public static Stream ImageToStream(string imagePath)
    {
      int resourceID = MainActivity.String2Pic(imagePath);

      Stream pngImageStream = new MemoryStream();
      var data = BitmapFactory.DecodeResource(MainActivity.TheView.Resources, resourceID);
      data.Compress(Bitmap.CompressFormat.Png, 0, pngImageStream);

      return pngImageStream;
    }
    public static string FileToString(string filename)
    {
      string contents = "";
      Android.Content.Res.AssetManager assets = MainActivity.TheView.Assets;
      using (StreamReader sr = new StreamReader(assets.Open(filename))) {
        contents = sr.ReadToEnd();
      }
      return contents;
    }
  }

  public class KeyboardListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
  {
    DroidVariable m_widget;
    public void OnGlobalLayout()
    {
      var vto = MainActivity.TheLayout.ViewTreeObserver;
      Rect r = new Rect();
      m_widget.ViewX.GetWindowVisibleDisplayFrame(r);
      int screenHeight = MainActivity.TheLayout.RootView.Height;

      // r.bottom is the position above soft keypad or device button.
      // if keypad is shown, the r.bottom is smaller than that before.
      int keypadHeight = screenHeight - r.Bottom;

      m_widget.KeyboardVisible = keypadHeight > screenHeight * 0.15;
    }
  }
}
