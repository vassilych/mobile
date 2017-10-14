using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using SplitAndMerge;

namespace scripting.Droid
{
  public class UtilsDroid
  {
    const int MIN_WIDTH = 640;
    const int MIN_HEIGHT = 960;

    public static Size GetScreenSize()
    {
      DisplayMetrics displayMetrics = new DisplayMetrics();
      var winManager = MainActivity.TheView.WindowManager;
      winManager.DefaultDisplay.GetMetrics(displayMetrics);

      return new Size(displayMetrics.WidthPixels, displayMetrics.HeightPixels);
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
    public static LayoutRules String2LayoutParam(DroidVariable location, bool isX)
    {
      View referenceView = isX ? location.ViewX : location.ViewY;
      string param = isX ? location.RuleX : location.RuleY;

      bool useRoot = referenceView == null;

      UIVariable refLocation = isX ? location.RefViewX?.Location :
                                     location.RefViewY?.Location;

      // If the reference view has a margin, add it to the current view.
      // This is the iOS functionality.
      if (isX && refLocation != null) {
        location.TranslationX += refLocation.TranslationX;
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
              if (referenceView is TextView) {
                delta -= 28;
              }
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

    static List<string> reserved = new List<string>() { "case", "class", "short", "switch", "turkey" };
    static public string String2ImageName(string name)
    {
      // Hooks for similar names:
      if (reserved.Contains(name)) {
        return "_" + name; // Only case difference with Turkey the country
      }
      string imagefileName = name.Replace("-", "_").Replace("(", "").
                                  Replace(")", "_").Replace("'", "_").
                                  Replace(" ", "_").Replace("é", "e").
                                  Replace(",", "").Replace("__", "_").
                                  Replace("\"", "").Replace(".png", "");
      return imagefileName;
    }

    static Dictionary<string, int> m_pics = new Dictionary<string, int>();
    public static int String2Pic(string name)
    {
      string imagefileName = String2ImageName(name);
      int resourceID = 0;
      if (m_pics.TryGetValue(imagefileName, out resourceID)) {
        return resourceID;
      }
      var fieldInfo = typeof(Resource.Drawable).GetField(imagefileName);
      if (fieldInfo == null) {
        // Try prepending _ for Android forbidden names like 'case'
        imagefileName = "_" + imagefileName;
        fieldInfo = typeof(Resource.Drawable).GetField(imagefileName);
        if (fieldInfo == null) {
          imagefileName = imagefileName.Replace("_", "");
          fieldInfo = typeof(Resource.Drawable).GetField(imagefileName);
        }
      }
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
  }
}
