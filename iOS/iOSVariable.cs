using System;
using System.Collections.Generic;

using UIKit;
using CoreGraphics;

using SplitAndMerge;

namespace scripting.iOS
{
  public class iOSVariable : UIVariable
  {
    public iOSVariable() { }

    public iOSVariable(UIType type, string name,
                       UIView viewx = null, UIView viewy = null) :
                       base(type, name)
    {
      m_viewX = viewx;
      m_viewY = viewy;
      if (type != UIType.LOCATION && m_viewX != null) {
        m_viewX.Tag = ++m_currentTag;
      }
    }

    public override Variable Clone()
    {
      iOSVariable newVar = new iOSVariable();
      newVar.Copy(this);
      newVar.m_viewX = m_viewX;
      newVar.m_viewY = m_viewY;
      newVar.m_originalText = m_originalText;
      newVar.m_alignment = m_alignment;
      return newVar;
    }

    public UIView ViewX {
      get { return m_viewX; }
      set { m_viewX = value; }
    }
    public UIView ViewY {
      get { return m_viewY; }
      set { m_viewY = value; }
    }

    public void SetText(string text, string alignment = null)
    {
      m_originalText = text;
      m_alignment = string.IsNullOrEmpty(alignment) ? m_alignment : alignment;
      // Add extra space for buttons, otherwise they don't look good.
      if (m_viewX is UIButton) {
        if (m_alignment == "left") {
          text = " " + text;
        } else if (m_alignment == "right") {
          text = text + " ";
        }
      }

      SetTextFunction.SetText(this, text);
    }
    public string GetText()
    {
      if (m_viewX is UIButton) {
        return m_originalText;
      }
      return GetTextFunction.GetText(this);
    }

    public CGSize GetParentSize()
    {
      if (ParentView != null) {
        return new CGSize(ParentView.Width, ParentView.Height);
      }
      return UtilsiOS.GetScreenSize();
    }
    public UIView GetParentView()
    {
      iOSVariable parent = ParentView as iOSVariable;
      if (parent != null) {
        return parent.ViewX;
      }
      return AppDelegate.GetCurrentView();
    }

    public UIView CreateStepper(CGRect rect, string extraLabel)
    {
      UIView parent = GetParentView();
      WidgetType = UIVariable.UIType.STEPPER;
      UILabel label = null;

      int stepperX = (int)(10 * UtilsiOS.WidthMultiplier());
      int stepperY = (int)( 4 * UtilsiOS.WidthMultiplier());
      int labelSize = (int)rect.Height;

      CGRect stepRect = new CGRect(stepperX, stepperY, rect.Width, rect.Height);
      UIStepper stepper = new UIStepper(stepRect);

      m_viewX = new UIView(rect);

      if (!string.IsNullOrWhiteSpace(extraLabel)) {
        nfloat labelWidth = rect.Width - stepper.Bounds.Width;
        nfloat labelHeight = stepper.Bounds.Height;
        if (extraLabel == "left") {
          stepperX = 0;
          CGRect labelRect = new CGRect(stepperX, stepperY, labelWidth, labelHeight);
          label = new UILabel(labelRect);
          label.TextAlignment = UITextAlignment.Left;
          stepRect = new CGRect(stepperX + labelWidth, stepperY, stepper.Bounds.Width, stepper.Bounds.Height);
          stepper = new UIStepper(stepRect);
        } else {
          CGRect labelRect = new CGRect(stepperX + stepper.Bounds.Width, stepperY, labelWidth, labelHeight);
          label = new UILabel(labelRect);
          label.TextAlignment = UITextAlignment.Right;
        }
        label.Text = CurrVal.ToString();
        label.TextAlignment = UITextAlignment.Center;
        //label.SizeToFit();
        m_viewX.AddSubview(label);
      }

      stepper.MinimumValue = (float)MinVal;
      stepper.MaximumValue = (float)MaxVal;
      stepper.Value = (float)CurrVal;
      stepper.StepValue = (float)Step;

      m_viewX.AddSubview(stepper);
      m_viewY = stepper;

      stepper.ValueChanged += (sender, e) => {
        CurrVal = stepper.Value;
        if (label != null) {
          label.Text = CurrVal.ToString();
        }
        ActionDelegate?.Invoke(WidgetName, CurrVal.ToString());
      };

      return m_viewX;
    }

    public void SetValue(double val) {
      CurrVal = val;
      if (m_viewY is UIStepper) {
        UIStepper stepper = m_viewY as UIStepper;
        stepper.Value = val;
        UIView[] subviews = m_viewX.Subviews;
        foreach (UIView view in subviews) {
          if (view is UILabel) {
            UILabel label = view as UILabel;
            label.Text = CurrVal.ToString();
          }
        }
      }
    }

    UIView m_viewX;
    UIView m_viewY;
    string m_originalText;
    string m_alignment;

    public static UIView GetView(string viewName, ParsingScript script)
    {
      if (viewName.Equals("root", StringComparison.OrdinalIgnoreCase)) {
        return null;
      }
      ParserFunction func = ParserFunction.GetFunction(viewName);
      Utils.CheckNotNull(viewName, func);
      Variable viewValue = func.GetValue(script);
      iOSVariable viewVar = viewValue as iOSVariable;
      return viewVar.ViewX;
    }
  }
}
