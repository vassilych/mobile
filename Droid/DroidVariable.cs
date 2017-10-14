using System;
using Android.Util;
using Android.Views;
using Android.Widget;

using SplitAndMerge;


namespace scripting.Droid
{
  public class DroidVariable : UIVariable
  {
    public DroidVariable() { }

    public DroidVariable(UIType type, string name, View viewx = null, View viewy = null) :
                         base(type, name)
    {
      m_viewX = viewx;
      m_viewY = viewy;
      if (type != UIType.LOCATION && m_viewX != null) {
        m_viewX.Tag = ++m_currentTag;
        m_viewX.Id = m_currentTag;
      }
    }
    public DroidVariable(UIType type, string name = "",
                         UIVariable refViewX = null, UIVariable refViewY = null) :
                         base(type, name, refViewX, refViewY)
    {
    }

    public override Variable Clone()
    {
      DroidVariable newVar = new DroidVariable();
      newVar.Copy(this);
      newVar.m_viewX = m_viewX;
      newVar.m_viewY = m_viewY;
      newVar.m_layoutRuleX = m_layoutRuleX;
      newVar.m_layoutRuleY = m_layoutRuleY;
      return newVar;
    }

    public View ViewX {
      get { return m_viewX; }
      set { m_viewX = value; }
    }
    public View ViewY {
      get { return m_viewY; }
      set { m_viewY = value; }
    }
    public LayoutRules LayoutRuleX {
      get { return m_layoutRuleX; }
      set { m_layoutRuleX = value; }
    }
    public LayoutRules LayoutRuleY {
      get { return m_layoutRuleY; }
      set { m_layoutRuleY = value; }
    }
    public ViewGroup ViewLayout {
      get { return m_viewX as ViewGroup; }
    }

    public View SetViewLayout(int width, int height)
    {
      //DroidVariable refView = RefViewX as DroidVariable;
      DroidVariable refView = Location?.RefViewX as DroidVariable;
      m_viewX = MainActivity.CreateViewLayout(width, height, refView?.ViewLayout);
      return m_viewX;
    }

    public View CreateStepper(int width, int height, string extraLabel)
    {
      DroidVariable refView = Location?.RefViewX as DroidVariable;
      ViewGroup layout = MainActivity.CreateViewLayout(width, height, refView?.ViewLayout);

      TextView label = null;

      Button btn1 = new Button(MainActivity.TheView);
      btn1.Text = "-";
      btn1.Id = ++m_currentTag;
      Button btn2 = new Button(MainActivity.TheView);
      btn2.Text = "+";
      btn2.Id = ++m_currentTag;

      layout.AddView(btn1);
      layout.AddView(btn2);

      RelativeLayout.LayoutParams layoutParams1 = new RelativeLayout.LayoutParams(
          ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent
      ) {
        Width = height, Height = height
      };

      RelativeLayout.LayoutParams layoutParams2 = new RelativeLayout.LayoutParams(
          ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent
      ) {
        Width = height, Height = height
      };
      layoutParams2.AddRule(LayoutRules.RightOf, btn1.Id);

      if (!string.IsNullOrWhiteSpace(extraLabel)) {
        label = new TextView(MainActivity.TheView);
        label.Text = CurrVal.ToString();
        label.Id = ++m_currentTag;
        label.Gravity = GravityFlags.Center;

        RelativeLayout.LayoutParams layoutParams3 = new RelativeLayout.LayoutParams(
            ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent
        ) {
          Width = height, Height = height
        };
        if (extraLabel == "left") {
          layoutParams1.AddRule(LayoutRules.RightOf, label.Id);
          //label.Gravity = GravityFlags.Right | GravityFlags.CenterVertical;
        } else {
          layoutParams3.AddRule(LayoutRules.RightOf, btn2.Id);
          //label.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
        }
        label.LayoutParameters = layoutParams3;
        layout.AddView(label);
      }

      btn1.Touch += (sender, e) => {
        if (e.Event.Action == MotionEventActions.Up) {
          CurrVal -= Step;
          CurrVal = CurrVal < MinVal ? MinVal : CurrVal;
          CurrVal = CurrVal > MaxVal ? MaxVal : CurrVal;
          btn1.Enabled = CurrVal > MinVal;
          btn2.Enabled = CurrVal < MaxVal;
          if (label != null) {
            label.Text = CurrVal.ToString();
          }
          ActionDelegate?.Invoke(WidgetName, CurrVal.ToString());
        }
      };
      btn2.Touch += (sender, e) => {
        if (e.Event.Action == MotionEventActions.Up) {
          CurrVal += Step;
          CurrVal = CurrVal < MinVal ? MinVal : CurrVal;
          CurrVal = CurrVal > MaxVal ? MaxVal : CurrVal;
          btn1.Enabled = CurrVal > MinVal;
          btn2.Enabled = CurrVal < MaxVal;
          if (label != null) {
            label.Text = CurrVal.ToString();
          }
          ActionDelegate?.Invoke(WidgetName, CurrVal.ToString());
        }
      };

      btn1.LayoutParameters = layoutParams1;
      btn2.LayoutParameters = layoutParams2;

      m_viewX = layout;
      return m_viewX;
    }

    View m_viewX;
    View m_viewY;
    LayoutRules m_layoutRuleX;
    LayoutRules m_layoutRuleY;

    public static Size GetLocation(View view)
    {
      if (view == null) {
        return null;
      }
      int[] outArr = new int[2];
      view.GetLocationOnScreen(outArr);
      return new Size(outArr[0], outArr[1]);
    }

    public static View GetView(string viewName, ParsingScript script)
    {
      if (viewName.Equals("root", StringComparison.OrdinalIgnoreCase)) {
        return null;
      }
      ParserFunction func = ParserFunction.GetFunction(viewName);
      Utils.CheckNotNull(func, viewName);
      Variable viewValue = func.GetValue(script);
      DroidVariable viewVar = viewValue as DroidVariable;
      return viewVar.ViewX;
    }
  }
}
