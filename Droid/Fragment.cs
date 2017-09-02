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
        List<View> m_views = new List<View>();
        int m_index;
        static List<View> m_hiddenViews = new List<View>();

        static List<ScriptingFragment> m_fragments = new List<ScriptingFragment>();
        static ScriptingFragment m_activeFragment = null;

        public string Text      { get; private set; }
        public int ActiveIcon   { get; private set; }
        public int InactiveIcon { get; private set; }

        ScriptingFragment(string text, string imageNameActive, string imageNameInactive)
        {
            if (string.IsNullOrEmpty(imageNameInactive)) {
                imageNameInactive = imageNameActive;
            }
            ActiveIcon   = MainActivity.String2Pic(imageNameActive);
            InactiveIcon = MainActivity.String2Pic(imageNameInactive);
            Text = text;
            m_index = m_fragments.Count;
        }
        /*public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            m_view = inflater.Inflate(Resource.Layout.fragment, null);
            //m_view.FindViewById<TextView>(Resource.Id.textView1).SetText(Resource.String.quiz);
            m_view.FindViewById<ImageView>(Resource.Id.imageView1).SetImageResource(GetViewImage(false));
            return m_view;
        }*/
        public static ScriptingFragment AddFragment(string text, string imageName, string selectedImageName = null)
        {
            ScriptingFragment fragment = new ScriptingFragment(text, imageName, selectedImageName);
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
        public static void AddView(View view)
        {
            if (m_activeFragment != null) {
                m_activeFragment.m_views.Add(view);
            }
        }
        public static void RemoveView(View view)
        {
            if (m_activeFragment != null) {
                m_activeFragment.m_views.Remove(view);
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
        public static void ShowFragment(ScriptingFragment fragment, bool showIt, bool tabChange)
        {
            foreach (View view in fragment.m_views) {
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
            if (!showIt && !explicitlyHidden)    {
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
