using System;
using CoreGraphics;
using UIKit;

namespace scripting.iOS
{
  public class CustomSwipeView : UIView
  {
    public override bool Hidden {
      get { return base.Hidden; }
      set { base.Hidden = value; }
    }

    public override void LayoutSubviews()
    {
      var start = 0;
      var childWidth = this.Frame.Width / 3;
      foreach (var v in this.Subviews) {
        v.Frame = new CGRect(start, 0, childWidth + start, this.Frame.Height);
        start = start + (int)childWidth;
      }
    }
  }

  public class CustomRowView : UIView
  {
    nfloat m_width;
    public CustomRowView(nfloat width = default(nfloat))
    {
      m_width = width;
    }
    public override void LayoutSubviews()
    {
      var start = 0;
      m_width = m_width == 0 ? Frame.Width : m_width;
      foreach (var v in Subviews) {
        var width = start > 0 ? m_width - start : v.Frame.Width;
        if (start > 0) {
          v.Frame = new CGRect(start, 0, width, Frame.Height);
        }
        start += (int)v.Frame.Width + 5;
      }
    }
  }
}
