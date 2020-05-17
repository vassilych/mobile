using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System.Collections.Generic;
using Android.Graphics;

namespace scripting.Droid
{
    public class TextImageAdapter : BaseAdapter
    {
        Context m_context;
        //LayoutInflater m_inflater;
        List<string> m_names;
        List<int> m_pics;
        string m_first;
        Color m_fontColor;
        bool m_colorSet;
        bool m_prevColorSet;
        int m_prevSelection = -1;

        Dictionary<int, TextView> m_viewCache = new Dictionary<int, TextView>();

        public float TextSize { get; set; }
        public Typeface Typeface { get; set; }
        public TypefaceStyle TypefaceStyle { get; set; }
        public Color PrevFontColor { get; set; }
        public Color FontColor {
            get { return m_fontColor; }
            set { m_fontColor = value; m_colorSet = true; } }
        public Color BGColor { get; set; }
        public int SelectedIndex { get; set; }

        public TextImageAdapter(Context context)
        {
            m_context = context;
        }

        public void SetItems(List<string> items, string first = null)
        {
            m_first = first;
            m_names = items;
            if (m_first != null)
            {
                m_names.Insert(0, m_first);
            }
        }
        public int Text2Position(string text)
        {
            if (m_names == null)
            {
                return 0;
            }
            for (int i = 0; i < m_names.Count; i++)
            {
                if (text.Equals(m_names[i]))
                {
                    return i;
                }
            }
            for (int i = 0; i < m_names.Count; i++)
            {
                if (m_names[i].StartsWith(text, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return 0;
        }
        public string Position2Text(int pos)
        {
            if (m_names == null || pos >= m_names.Count || pos < 0)
            {
                return "";
            }
            return m_names[pos];
        }
        public void SetPics(List<string> pics)
        {
            m_pics = UtilsDroid.GetPicList(pics, m_first);
        }
        public override int Count
        {
            get
            {
                if (m_names != null)
                {
                    return m_names.Count;
                }
                if (m_pics != null)
                {
                    return m_pics.Count;
                }
                return 0;
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }
        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            return base.GetDropDownView(position, convertView, parent);
        }

        public void SetSelectedView(LinearLayout layout, int position)
        {
            SelectedIndex = position;
            if (!m_colorSet || layout == null || !m_prevColorSet)
            {
                return;
            }
            for (int i = 0; i < layout.ChildCount; i++)
            {
                var child = layout.GetChildAt(i);
                if (child is TextView)
                {
                    var textView = (TextView)child;
                    textView.SetTextColor(FontColor);
                    if (m_prevColorSet && m_prevSelection >= 0 && m_prevSelection != SelectedIndex &&
                        m_viewCache.TryGetValue(m_prevSelection, out TextView previous))
                    {
                        previous.SetTextColor(PrevFontColor);
                    }
                }
            }
            m_prevSelection = position;
        }
        public int SelectText(string text)
        {
            int position = Text2Position(text);
            SelectedIndex = position;

            if (!m_colorSet)
            {
                return position;
            }
            if (m_prevColorSet && m_prevSelection >= 0 && m_viewCache.TryGetValue(m_prevSelection, out TextView previous))
            {
                previous.SetTextColor(PrevFontColor);
            }
            if (m_viewCache.TryGetValue(position, out TextView current))
            {
                current.SetTextColor(FontColor);
            }
            m_prevSelection = SelectedIndex;
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
            {
                view = MainActivity.TheView.LayoutInflater.Inflate(Android.Resource.Layout.ActivityListItem, null);
            }
            if (m_names != null && position < m_names.Count)
            {
                TextView textView = view.FindViewById<TextView>(Android.Resource.Id.Text1);
                textView.Text = m_names[position];

                if (m_colorSet)
                {
                    if (!m_prevColorSet)
                    {
                        PrevFontColor = UtilsDroid.String2Color(Java.Lang.String.Format("#%06X", 0xFFFFFF & textView.CurrentTextColor));
                        m_prevColorSet = true;
                    }

                    m_viewCache[position] = textView;
                    textView.SetTextColor(position == SelectedIndex ? FontColor : PrevFontColor);
                    if (position == SelectedIndex)
                    {
                        textView.SetTextColor(FontColor);
                        if (FontColor != null)
                        {
                            Console.WriteLine(textView.Text);
                            textView.SetTextColor(FontColor);
                        }
                        if (BGColor != null)
                        {
                            textView.SetBackgroundColor(BGColor);
                        }
                    }
                    else if (position != 0)
                    {
                        textView.SetTextColor(PrevFontColor);
                    }
                    m_prevSelection = SelectedIndex;
                }
                if (TextSize > 0)
                {
                    textView.TextSize = TextSize;
                }
                if (Typeface != null)
                {
                    textView.SetTypeface(Typeface, TypefaceStyle);
                }
            }
            if (m_pics != null && position < m_pics.Count && m_pics[position] > 0)
            {
                view.FindViewById<ImageView>(Android.Resource.Id.Icon).SetImageResource(m_pics[position]);
            }
            return view;
        }
    }
}
