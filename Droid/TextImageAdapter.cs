using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System.Collections.Generic;

namespace scripting.Droid
{
  public class TextImageAdapter : BaseAdapter
  {
    Context m_context;
    //LayoutInflater m_inflater;
    List<string> m_names;
    List<int> m_pics;
    string m_first;

    public TextImageAdapter(Context context)
    {
      m_context = context;
    }

    public void SetItems(List<string> items, string first = null)
    {
      m_first = first;
      m_names = items;
      if (m_first != null) {
        m_names.Insert(0, m_first);
      }
    }
    public int Text2Position(string text) {
      if (m_names == null) {
        return 0;
      }
      for (int i = 0; i < m_names.Count; i++) {
        if (text.Equals(m_names[i])) {
          return i;
        }
      }
      return 0;
    }
    public string Position2Text(int pos)
    {
      if (m_names == null || pos >= m_names.Count || pos < 0) {
        return "";
      }
      return m_names[pos];
    }
    public void SetPics(List<string> pics)
    {
      m_pics = UtilsDroid.GetPicList(pics, m_first);
    }
    public override int Count {
      get {
        if (m_names != null) {
          return m_names.Count;
        }
        if (m_pics != null) {
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

    public override View GetView(int position, View convertView, ViewGroup parent)
    {
      /*if (m_inflater == null) {
          m_inflater = (LayoutInflater)m_context.GetSystemService(Context.LayoutInflaterService);
      }
      if (convertView == null) {
          convertView = m_inflater.Inflate(m_resourceLayout, parent, false);
      }
      ImageView imageView = convertView.FindViewById<ImageView>(m_resourceImage);
      TextView textView = convertView.FindViewById<TextView>(m_resourceText);*/

      LinearLayout layout = new LinearLayout(m_context);
      //layout.SetBackgroundColor(m_bgcolor);//Android.Graphics.Color.ParseColor("#91F6FF"));
      layout.LayoutParameters = new LinearLayout.LayoutParams(
          ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
      layout.DescendantFocusability = DescendantFocusability.BlockDescendants;

      if (m_pics != null && position < m_pics.Count && m_pics[position] >= 0) {
        ImageView imageView = new ImageView(m_context);
        imageView.SetImageResource(m_pics[position]);
        layout.AddView(imageView);
      }
      if (m_names != null && position < m_names.Count) {
        TextView textView = new TextView(m_context);
        textView.Text = " " + m_names[position];
        if (m_pics != null && (position >= m_pics.Count || m_pics[position] < 0)) {
          textView.SetMinHeight(textView.Height + 10);
          textView.Gravity = GravityFlags.Center;
        } else {
          textView.TranslationY = 8;
        }

        layout.AddView(textView, ViewGroup.LayoutParams.MatchParent);
      }

      convertView = layout;
      return convertView;
    }
  }
}
