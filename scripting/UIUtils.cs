using System;
using System.Collections.Generic;
using SplitAndMerge;

namespace scripting
{
  public class UIUtils
  {
    public static string RemovePrefix(string text)
    {
      string candidate = text.Trim().ToLower();
      if (candidate.Length > 2 && candidate.StartsWith("l'",
                    StringComparison.OrdinalIgnoreCase)) {
        return candidate.Substring(2).Trim();
      }

      int firstSpace = candidate.IndexOf(' ');
      if (firstSpace <= 0) {
        return candidate;
      }

      string prefix = candidate.Substring(0, firstSpace);
      if (prefix.Length == 3 && candidate.Length > 4 &&
         (prefix == "der" || prefix == "die" || prefix == "das" ||
          prefix == "los" || prefix == "las" || prefix == "les")) {
        return candidate.Substring(firstSpace + 1);
      }
      if (prefix.Length == 2 && candidate.Length > 3 &&
         (prefix == "el" || prefix == "la" || prefix == "le" ||
          prefix == "il" || prefix == "lo")) {
        return candidate.Substring(firstSpace + 1);
      }
      return candidate;
    }

    static List<string> reserved = new List<string>() { "case", "catch", "class", "double", "else", "for",
      "if", "int", "internal", "long", "private", "public", "return", "short", "static", "switch", "try",
      "turkey", "while" };
    public static string String2ImageName(string name)
    {
      // Hooks for similar names:
      if (reserved.Contains(name)) {
        return "_" + name; // Only case difference with Turkey the country
      }
      string imagefileName = name.Replace("-", "_").Replace("(", "").
              Replace(")", "_").Replace("'", "_").
              Replace(" ", "_").Replace("é", "e").
              Replace("ñ", "n").Replace("í", "i").
              Replace(",", "" ).Replace("__", "_").
              Replace("\"", "").Replace(".png", "");
      return imagefileName;
    }

    public struct Rect
    {
      public int X, Y, W, H;
      public Rect(int x, int y, int w, int h)
      {
        X = x;
        Y = y;
        W = w;
        H = h;
      }
    }
    static Dictionary<string, List<Tuple<string, Rect>>> m_widgetLocations =
       new Dictionary<string, List<Tuple<string, Rect>>>();
    static Dictionary<string, Tuple<string, Rect>> m_widget2Location =
       new Dictionary<string, Tuple<string, Rect>>();
    
    public static void RegisterWidget(UIVariable widget, Rect rect,
                                      string parent = "", int tabId = 0)
    {
      RegisterWidget(widget.Name, rect, parent, tabId);
      ParserFunction.AddGlobal(widget.Name, new GetVarFunction(widget));
    }

    public static void RegisterWidget(string widgetName, Rect rect,
                                      string parent = "", int tabId = 0)
    {
      string key = parent + "_" + tabId;
      List<Tuple<string, Rect>> locations;
      if (!m_widgetLocations.TryGetValue(key, out locations)) {
        locations = new List<Tuple<string, Rect>>();
      } else {
        Tuple<string, Rect> existing;
        if (m_widget2Location.TryGetValue(widgetName, out existing)) {
          locations.Remove(existing);
        }
      }
      Tuple<string, Rect> location = new Tuple<string, Rect>(widgetName, rect);
      locations.Add(location);
      m_widgetLocations[key] = locations;
      m_widget2Location[widgetName] = location;
    }

    public static void DeregisterTab(int tabId, string parent = "")
    {
      string key = parent + "_" + tabId;
      List<Tuple<string, Rect>> locations;
      if (!m_widgetLocations.TryGetValue(key, out locations)) {
        return;
      }

      List<string> toDeregister = new List<string>();
      foreach (var location in locations) {
        toDeregister.Add(location.Item1);
      }
      foreach (var widgetName in toDeregister) {
        DeregisterWidget(widgetName);
      }
    }
    public static void DeregisterAll()
    {
      foreach(string widgetName in m_widget2Location.Keys) {
        ParserFunction.RemoveGlobal(widgetName);
      }

      m_widgetLocations.Clear();
      m_widget2Location.Clear();
    }

    public static void DeregisterWidget(string widgetName)
    {
      Tuple<string, Rect> location;
      if (!m_widget2Location.TryGetValue(widgetName, out location)) {
        return;
      }
      m_widget2Location.Remove(widgetName);
      foreach (var locations in m_widgetLocations.Values) {
        locations.Remove(location);
      }

      ParserFunction.RemoveGlobal(widgetName);
    }

    public static List<string> GetAllWidgets(string widgetName)
    {
      return new List<string>(m_widget2Location.Keys);
    }

    public static List<string> FindWidgets(Rect location, string parent = "", int tabId = 0,
                                           string exceptWidget = "")
    {
      List<string> results = new List<string>();
      string key = parent + "_" + tabId;
      List<Tuple<string, Rect>> locations;
      if (!m_widgetLocations.TryGetValue(key, out locations)) {
        return results;
      }

      foreach (Tuple<string, Rect> tuple in locations) {
        if (exceptWidget == null) {
          results.Add(tuple.Item1);
          continue;
        }
        if (exceptWidget == tuple.Item1) {
          continue;
        }
        var loc = tuple.Item2;
        var horizLeft  = loc.X >= location.X && loc.X <= location.X + location.W;
        var horizRight = loc.X + loc.W >= location.X && loc.X + loc.W <= location.X + location.W;
        if (!horizLeft && !horizRight) {
          continue;
        }
        var vertTop    = loc.Y >= location.Y && loc.Y <= location.Y + location.H;
        var vertButtom = loc.Y + loc.H >= location.Y && loc.Y + loc.H <= location.Y + location.H;
        if (!vertTop && !vertButtom) {
          continue;
        }
        results.Add(tuple.Item1);
      }

      return results;
    }
  }
}
