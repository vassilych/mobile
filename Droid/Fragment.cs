using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace scripting.Droid
{
  public class ScriptingFragment : Fragment
  {
    List<DroidVariable> m_widgets = new List<DroidVariable>();

    int m_index;
    static List<View> m_hiddenViews = new List<View>();

    static List<ScriptingFragment> m_fragments = new List<ScriptingFragment>();
    static ScriptingFragment m_activeFragment = null;

    static Dictionary<DroidVariable, ViewGroup> m_allViews = new Dictionary<DroidVariable, ViewGroup>();

    public string OriginalText { get; private set; }
    public string OriginalText_Wide { get; private set; }
    public string Text { get; private set; }
    public int ActiveIcon { get; private set; }
    public int InactiveIcon { get; private set; }

    ScriptingFragment(string text, string imageNameActive, string imageNameInactive)
    {
      if (string.IsNullOrEmpty(imageNameInactive)) {
        imageNameInactive = imageNameActive;
      }
      ActiveIcon = MainActivity.String2Pic(imageNameActive);
      InactiveIcon = MainActivity.String2Pic(imageNameInactive);
      OriginalText = text;
      OriginalText_Wide = text + "_wide";
      Text = Localization.GetText(text);
      m_index = m_fragments.Count;
    }
    public string GetText()
    {
      if (MainActivity.IsLandscape) {
        return OriginalText_Wide;
      } else {
        return OriginalText;
      }
    }
    /*public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        m_view = inflater.Inflate(Resource.Layout.fragment, null);
        //m_view.FindViewById<TextView>(Resource.Id.textView1).SetText(Resource.String.quiz);
        m_view.FindViewById<ImageView>(Resource.Id.imageView1).SetImageResource(GetViewImage(false));
        return m_view;
    }*/
    public static ScriptingFragment AddFragment(string text, string imageNameActive, string imageNameInactive)
    {
      ScriptingFragment fragment = new ScriptingFragment(text, imageNameActive, imageNameInactive);
      m_fragments.Add(fragment);
      m_activeFragment = fragment;
      return fragment;
    }
    public static ScriptingFragment GetFragment(int index)
    {
      return index < m_fragments.Count ? m_fragments[index] : null;
    }
    public static int Count()
    {
      return m_fragments.Count;
    }

    public static void SetActive(int index)
    {
      m_activeFragment = index < m_fragments.Count ? m_fragments[index] : null;
    }
    public static void AddView(DroidVariable widget, ViewGroup parent = null)
    {
      var view = widget.ViewX;
      if (m_activeFragment != null) {
        m_activeFragment.m_widgets.Add(widget);
      }
      if (parent == null) {
        parent = MainActivity.TheLayout;
      }
      m_allViews.Add(widget, parent);
    }
    public static void RemoveAll()
    {
      foreach (KeyValuePair<DroidVariable, ViewGroup> entry in m_allViews) {
        ViewGroup parentView = entry.Value;
        View viewToRemove = entry.Key.ViewX;
        parentView.RemoveView(viewToRemove);
        //((ViewGroup)viewToRemove.Parent).RemoveAllViews();
      }
      m_allViews.Clear();
    }
    public static void RemoveView(DroidVariable widget, bool removeFromActive = true)
    {
      var view = widget.ViewX;
      if (removeFromActive && m_activeFragment != null) {
        m_activeFragment.m_widgets.Remove(widget);
      }
      m_allViews.Remove(widget);

      var parent = widget.Location?.ParentView as DroidVariable;

      ViewGroup parentView = parent != null ? parent.ViewLayout : MainActivity.TheLayout;
      View viewToRemove = widget.ViewX;

      parentView.RemoveView(viewToRemove);

      parentView = viewToRemove.Parent as ViewGroup;
      if (parentView != null && parentView.Parent != null) {
        parentView.RemoveView(viewToRemove);
      }
    }
    public static void ShowFragments(int activeIndex)
    {
      SetActive(activeIndex);
      foreach (ScriptingFragment fragment in m_fragments) {
        bool isActive = activeIndex == fragment.m_index;
        ShowFragment(fragment, isActive, true);
      }
    }
    public static void RemoveTabViews(int index)
    {
      var fragment = GetFragment(index);
      if (fragment == null) {
        return;
      }
      for (int i = 0; i < fragment.m_widgets.Count; i++) {
        var widget = fragment.m_widgets[i];
        RemoveView(widget, false);
      }
      fragment.m_widgets.Clear();
   }
    public static void ShowFragment(ScriptingFragment fragment, bool showIt, bool tabChange)
    {
      for (int i = 0; i < fragment.m_widgets.Count; i++) {
        var widget = fragment.m_widgets[i];
        var view   = widget.ViewX;
        widget.ShowView(showIt);
        ShowView(view, showIt, tabChange);
      }
    }
    public static void ShowView(View view, bool showIt, bool tabChange)
    {
      bool explicitlyHidden = m_hiddenViews.Contains(view);
      if (explicitlyHidden && tabChange) {
        return;
      }
      view.Visibility = showIt ? ViewStates.Visible : ViewStates.Gone;

      if (tabChange) {
        return;
      }
      if (!showIt && !explicitlyHidden) {
        m_hiddenViews.Add(view);
      } else if (showIt && explicitlyHidden) {
        m_hiddenViews.Remove(view);
      }
    }
    public int GetViewImage(bool active)
    {
      return active ? ActiveIcon : InactiveIcon;
    }
  }
}
