using System;
using System.Collections.Generic;
using System.Drawing;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using SplitAndMerge;
using UIKit;

namespace scripting.iOS
{
  public class GetLocationFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 4, m_name);

      string viewNameX = args[0].AsString();
      string ruleStrX = args[1].AsString();
      string viewNameY = args[2].AsString();
      string ruleStrY = args[3].AsString();

      int leftMargin = Utils.GetSafeInt(args, 4);
      int topMargin = Utils.GetSafeInt(args, 5);

      string autoSize = Utils.GetSafeString(args, 6);
      double multiplier = Utils.GetSafeDouble(args, 7);
      AutoScaleFunction.TransformSizes(ref leftMargin, ref topMargin,
                   UtilsiOS.GetRealScreenWidth(), autoSize, multiplier);

      Variable parentView = Utils.GetSafeVariable(args, 9, null);

      UIView referenceViewX = iOSVariable.GetView(viewNameX, script);
      UIView referenceViewY = iOSVariable.GetView(viewNameY, script);
      iOSVariable location = new iOSVariable(UIVariable.UIType.LOCATION,
                                             viewNameX + viewNameY, referenceViewX,
                                             referenceViewY);

      location.SetRules(ruleStrX, ruleStrY);
      location.ParentView = parentView as UIVariable;

      double screenRatio = UtilsiOS.GetScreenRatio();
      location.TranslationX = (int)(leftMargin / screenRatio);
      location.TranslationY = (int)(topMargin / screenRatio);

      return location;
    }
  }
  public class AddWidgetFunction : ParserFunction
  {
    // Extending Combobox width with respect to Android:
    const double COMBOBOX_EXTENTION = 3.6;
    public AddWidgetFunction(string widgetType = "", string extras = "")
    {
      m_widgetType = widgetType;
      m_extras = extras;
    }
    protected override Variable Evaluate(ParsingScript script)
    {
      string widgetType = m_widgetType;
      int start = string.IsNullOrEmpty(widgetType) ? 1 : 0;
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2 + start, m_name);

      if (start == 1) {
        widgetType = args[0].AsString();
        Utils.CheckNotEmpty(script, widgetType, m_name);
      }

      iOSVariable location = args[start] as iOSVariable;
      Utils.CheckNotNull(location, m_name);

      double screenRatio = UtilsiOS.GetScreenRatio();

      string varName = args[start + 1].AsString();
      string text    = Utils.GetSafeString(args, start + 2);
      int width      = (int)(Utils.GetSafeInt(args, start + 3) / screenRatio);
      int height     = (int)(Utils.GetSafeInt(args, start + 4) / screenRatio);

      string autoSize = Utils.GetSafeString(args, start + 5);
      double multiplier = Utils.GetSafeDouble(args, start + 6);
      AutoScaleFunction.TransformSizes(ref width, ref height,
            UtilsiOS.GetRealScreenWidth(), autoSize, multiplier);

      if (widgetType == "Combobox") {
        height = (int)(height * COMBOBOX_EXTENTION);
      } else if (widgetType == "AdMobBanner") {
        AdMob.GetAdSize(text, ref width, ref height);
      }
      if (widgetType == "Combobox" || widgetType == "TypePicker") {
        height = Math.Max(height, 162);
      }
      location.SetSize(width, height);
      CGSize parentSize = location.GetParentSize();

      location.X = UtilsiOS.String2Position(location.RuleX, location.ViewX, location, parentSize, true);
      location.Y = UtilsiOS.String2Position(location.RuleY, location.ViewY, location, parentSize, false);

      location.X += location.TranslationX;
      location.Y += location.TranslationY;

      CGRect rect = new CGRect(location.X, location.Y, width, height);

      iOSVariable widgetFunc = ExistingWidget(script, varName);
      bool existing = widgetFunc != null;
      if (!existing) {
        widgetFunc = GetWidget(widgetType, varName, text, rect);
      } else {
        widgetFunc.ViewX.Frame = rect;
      }
      Utils.CheckNotNull(widgetFunc, m_name);

      widgetFunc.CreateComplexView(rect, m_extras);

      var currView = location.GetParentView();
      currView.Add(widgetFunc.ViewX);

      iOSApp.AddView(widgetFunc);

      ParserFunction.AddGlobal(varName, new GetVarFunction(widgetFunc));
      return widgetFunc;
    }
    public static iOSVariable GetWidget(string widgetType, string widgetName, string initArg, CGRect rect)
    {
      UIVariable.UIType type = UIVariable.UIType.NONE;
      UIView widget = null;
      switch (widgetType) {
        case "View":
          type = UIVariable.UIType.VIEW;
          widget = new UIView(rect);
          break;
        case "Button":
          type = UIVariable.UIType.BUTTON;
          widget = new UIButton(rect);
          //widget = new UIButton(UIButtonType.System);
          ((UIButton)widget).SetTitleColor(UIColor.Black, UIControlState.Normal);
          ((UIButton)widget).SetTitle(initArg, UIControlState.Normal);
          AddBorderFunction.AddBorder(widget);
          break;
        case "Label":
          type = UIVariable.UIType.LABEL;
          widget = new UILabel(rect);
          ((UILabel)widget).TextColor = UIColor.Black;
          ((UILabel)widget).Text = initArg;
          break;
        case "TextEdit":
          type = UIVariable.UIType.TEXT_FIELD;
          widget = new UITextField(rect);
          ((UITextField)widget).TextColor = UIColor.Black;
          ((UITextField)widget).Placeholder = initArg;
          MakeBottomBorder(widget, (int)rect.Width, (int)rect.Height);
          break;
        case "TextView":
          type = UIVariable.UIType.TEXT_VIEW;
          widget = new UITextView(rect);
          ((UITextView)widget).TextColor = UIColor.Black;
          ((UITextView)widget).Text = initArg;
          AddBorderFunction.AddBorder(widget);
          break;
        case "ImageView":
          type = UIVariable.UIType.IMAGE_VIEW;
          widget = new UIImageView(rect);
          if (!string.IsNullOrWhiteSpace(initArg)) {
            UIImage img = UtilsiOS.LoadImage(initArg);
            ((UIImageView)widget).Image = img;
          }
          break;
        case "Combobox":
        case "TypePicker":
          type = UIVariable.UIType.PICKER_VIEW;
          widget = new UIPickerView(rect);
          ((UIPickerView)widget).AutosizesSubviews = true;
          break;
        case "ListView":
          type = UIVariable.UIType.LIST_VIEW;
          widget = new UITableView(rect);
          ((UITableView)widget).AutosizesSubviews = true;
          ((UITableView)widget).AutoresizingMask = UIViewAutoresizing.FlexibleBottomMargin;
          ((UITableView)widget).BackgroundColor = UIColor.Clear;
          break;
        case "Switch":
          type = UIVariable.UIType.SWITCH;
          widget = new UISwitch(rect);
          break;
        case "Slider":
          type = UIVariable.UIType.SLIDER;
          widget = new UISlider(rect);
          break;
        case "Stepper":
          type = UIVariable.UIType.STEPPER;
          //widget = new UIStepper(rect);
          break;
        case "SegmentedControl":
          type = UIVariable.UIType.SEGMENTED;
          widget = new UISegmentedControl(rect);
          break;
        case "AdMobBanner":
          type = UIVariable.UIType.ADMOB;
          var banView = new AdMob();
          widget = banView.AddBanner(AppDelegate.GetCurrentController(), rect, initArg);
          break;
        default:
          type = UIVariable.UIType.VIEW;
          widget = new UIView(rect);
          break;
      }
      SetOptionsFunction.SetMultiline(widget);

      iOSVariable widgetFunc = new iOSVariable(type, widgetName, widget);
      SetValues(widgetFunc, initArg);

      return widgetFunc;
    }
    public static void MakeBottomBorder(UIView view, int width, int height, float borderWidth = 1)
    {
      CALayer border = new CALayer();
      border.BorderColor = UIColor.DarkTextColor.CGColor;
      border.Frame = new CGRect(0, height - borderWidth, width, height);
      border.BorderWidth = borderWidth;
      view.Layer.AddSublayer(border);
      view.Layer.MasksToBounds = true;
    }
    public static void SetValues(iOSVariable widgetFunc, string valueStr)
    {
      if (string.IsNullOrWhiteSpace(valueStr)) {
        return;
      }
      widgetFunc.InitValue = new Variable(valueStr);

      // currValue:minValue:maxValue:step

      double minValue = 0, maxValue = 1, currValue = 0, step = 1.0;
      string[] vals = valueStr.Split(new char[] { ',', ':' });
      Double.TryParse(vals[0].Trim(), out currValue);

      if (vals.Length > 1) {
        Double.TryParse(vals[1].Trim(), out minValue);
        if (vals.Length > 2) {
          Double.TryParse(vals[2].Trim(), out maxValue);
        }
        if (vals.Length > 3) {
          Double.TryParse(vals[3].Trim(), out step);
        }
      } else {
        minValue = maxValue = currValue;
      }

      if (widgetFunc.ViewX is UISwitch) {
        ((UISwitch)widgetFunc.ViewX).On = (int)currValue == 1;
      } else if (widgetFunc.ViewX is UISlider) {
        ((UISlider)widgetFunc.ViewX).MinValue = (float)minValue;
        ((UISlider)widgetFunc.ViewX).MaxValue = (float)maxValue;
        ((UISlider)widgetFunc.ViewX).Value = (float)currValue;
      } else if (widgetFunc.ViewX is UISegmentedControl) {
        UISegmentedControl seg = widgetFunc.ViewX as UISegmentedControl;
        for (int i = 0; i < vals.Length; i++) {
          seg.InsertSegment(vals[i], i, false);
        }
        seg.SelectedSegment = 0;
      } else {
        widgetFunc.CurrVal = currValue;
        widgetFunc.MinVal  = minValue;
        widgetFunc.MaxVal  = maxValue;
        widgetFunc.Step    = step;
      }
    }
    public static iOSVariable ExistingWidget(ParsingScript script, string varName)
    {
      ParserFunction func = ParserFunction.GetFunction(varName);
      if (func == null) {
        return null;
      }
      iOSVariable viewVar = func.GetValue(script) as iOSVariable;
      RemoveViewFunction.RemoveView(viewVar);
      return viewVar;
    }

    string m_widgetType;
    string m_extras;
  }

  public class AddWidgetDataFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      string varName = args[0].AsString();
      ParserFunction func = ParserFunction.GetFunction(varName);
      Utils.CheckNotNull(func, varName);

      Variable data = Utils.GetSafeVariable(args, 1, null);
      Utils.CheckNotNull(data, m_name);
      Utils.CheckNotNull(data.Tuple, m_name);

      iOSVariable viewVar = func.GetValue(script) as iOSVariable;
      Utils.CheckNotNull(viewVar, m_name);

      List<string> types = new List<string>(data.Tuple.Count);
      for (int i = 0; i < data.Tuple.Count; i++) {
        types.Add(data.Tuple[i].AsString());
      }

      string strAction = Utils.GetSafeString(args, 2);
      string alignment = Utils.GetSafeString(args, 3);

      if (viewVar.ViewX is UIPickerView) {
        UIPickerView pickerView = viewVar.ViewX as UIPickerView;

        TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
        if (model == null) {
          model = new TypePickerViewModel(AppDelegate.GetCurrentController());
        }
        model.Data = types;

        if (!string.IsNullOrWhiteSpace(strAction)) {
          model.RowSelected += (row) => {
            UIVariable.GetAction(strAction, varName, row.ToString());
          };
        }
        if (!string.IsNullOrWhiteSpace(alignment)) {
          var al = AlignTitleFunction.GetAlignment(alignment);
          model.Alignment = al.Item2;
        }

        model.SetSize((int)pickerView.Bounds.Width, (int)pickerView.Bounds.Height / 4);
        pickerView.Model = model;
      } else if (viewVar.ViewX is UITableView) {
        UITableView tableView = viewVar.ViewX as UITableView;

        TableViewSource source = tableView.Source as TableViewSource;
        if (source == null) {
          source = new TableViewSource();
        }
        source.Data = types;
        tableView.Source = source;
        tableView.ReloadData();
      }

      return Variable.EmptyInstance;
    }
  }
  public class AddWidgetImagesFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      string varName = args[0].AsString();
      ParserFunction func = ParserFunction.GetFunction(varName);
      Utils.CheckNotNull(func, varName);

      Variable data = Utils.GetSafeVariable(args, 1, null);
      Utils.CheckNotNull(data, m_name);
      Utils.CheckNotNull(data.Tuple, m_name);

      iOSVariable viewVar = func.GetValue(script) as iOSVariable;
      Utils.CheckNotNull(viewVar, m_name);

      List<UIImage> images = new List<UIImage>(data.Tuple.Count);
      for (int i = 0; i < data.Tuple.Count; i++) {
        UIImage img = UtilsiOS.LoadImage(data.Tuple[i].AsString());
        images.Add(img);
      }

      string strAction = Utils.GetSafeString(args, 2);

      if (viewVar.ViewX is UIPickerView) {
        UIPickerView pickerView = viewVar.ViewX as UIPickerView;

        TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
        if (model == null) {
          model = new TypePickerViewModel(AppDelegate.GetCurrentController());
        }
        model.Images = images;

        if (!string.IsNullOrWhiteSpace(strAction)) {
          AddActionFunction.AddAction(viewVar, varName, strAction);
        }
        model.SetSize((int)pickerView.Bounds.Width, (int)pickerView.Bounds.Height / 4);
        pickerView.Model = model;
      } else if (viewVar.ViewX is UITableView) {
        UITableView tableView = viewVar.ViewX as UITableView;

        TableViewSource source = tableView.Source as TableViewSource;
        if (source == null) {
          source = new TableViewSource();
        }
        source.Images = images;
        tableView.Source = source;
      }

      return Variable.EmptyInstance;
    }
  }
  public class MoveViewFunction : ParserFunction
  {
    bool m_isAbsolute;
    public MoveViewFunction(bool absoluteMove)
    {
      m_isAbsolute = absoluteMove;
    }
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 3, m_name);

      string varName = args[0].AsString();
      ParserFunction func = ParserFunction.GetFunction(varName);
      Utils.CheckNotNull(func, varName);
      iOSVariable viewVar = func.GetValue(script) as iOSVariable;
      Utils.CheckNotNull(viewVar, m_name);

      int deltaX = args[1].AsInt();
      int deltaY = args[2].AsInt();

      string autoSize = Utils.GetSafeString(args, 3);
      double multiplier = Utils.GetSafeDouble(args, 4);
      AutoScaleFunction.TransformSizes(ref deltaX, ref deltaY,
                        (int)UtilsiOS.GetNativeScreenSize().Width, autoSize, multiplier);

      UIView view = viewVar.ViewX;
      Utils.CheckNotNull(view, m_name);

      CGRect frame = view.Frame;
      if (deltaX < 0) {
        deltaX = (int)frame.X;
      }
      if (deltaY < 0) {
        deltaY = (int)frame.Y;
      }
      if (!m_isAbsolute) {
        frame.Offset(deltaX, deltaY);
      } else {
        frame.Location = new CGPoint(deltaX, deltaY);
      }
      view.Frame = frame;

      return Variable.EmptyInstance;
    }
  }
  public class GetCoordinateFunction : ParserFunction
  {
    bool m_isX;
    public GetCoordinateFunction(bool isX)
    {
      m_isX = isX;
    }
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 3, m_name);

      string varName = args[0].AsString();
      ParserFunction func = ParserFunction.GetFunction(varName);
      Utils.CheckNotNull(func, varName);
      iOSVariable viewVar = func.GetValue(script) as iOSVariable;
      Utils.CheckNotNull(viewVar, m_name);

      UIView view = viewVar.ViewX;
      Utils.CheckNotNull(view, m_name);

      int coord = 0;
      CGRect frame = view.Frame;
      if (m_isX) {
        coord = (int)view.Frame.X;
      } else {
        coord = (int)view.Frame.X;
      }

      return new Variable(coord);
    }
  }
  public class SetTextFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);

      Variable title = Utils.GetItem(script);
      string strTitle = title.AsString();

      iOSVariable iosVar = Utils.GetVariable(varName, script) as iOSVariable;
      Utils.CheckNotNull(iosVar, m_name);

      iosVar.SetText(strTitle);

      return iosVar;
    }
    public static void SetText(iOSVariable iosVar, string text)
    {
      if (iosVar.ViewX is UIButton) {
        ((UIButton)iosVar.ViewX).SetTitle(text, UIControlState.Normal);
      } else if (iosVar.ViewX is UILabel) {
        ((UILabel)iosVar.ViewX).Text = text;
      } else if (iosVar.ViewX is UITextField) {
        ((UITextField)iosVar.ViewX).Text = text;
      } else if (iosVar.ViewX is UITextView) {
        ((UITextView)iosVar.ViewX).Text = text;
      } else if (iosVar.ViewX is UIPickerView) {
        UIPickerView picker = iosVar.ViewX as UIPickerView;
        TypePickerViewModel model = picker.Model as TypePickerViewModel;
        int row = model.StringToRow(text);
        SetValueFunction.SetValue(iosVar, row);
      }
    }
  }
  public class GetTextFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);

      iOSVariable iosVar = Utils.GetVariable(varName, script) as iOSVariable;
      Utils.CheckNotNull(iosVar, m_name);

      return new Variable(iosVar.GetText());
    }
    public static string GetText(iOSVariable iosVar)
    {
      string result = "";
      if (iosVar.ViewX is UIButton) {
        result = ((UIButton)iosVar.ViewX).Title(UIControlState.Normal);
      } else if (iosVar.ViewX is UILabel) {
        result = ((UILabel)iosVar.ViewX).Text;
      } else if (iosVar.ViewX is UITextField) {
        result = ((UITextField)iosVar.ViewX).Text;
      } else if (iosVar.ViewX is UITextView) {
        result = ((UITextView)iosVar.ViewX).Text;
      } else if (iosVar.ViewX is UIPickerView) {
        UIPickerView pickerView = iosVar.ViewX as UIPickerView;
        TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
        int row = (int)GetValueFunction.GetValue(iosVar);
        result = model.RowToString(row);
      }
      return result;
    }
  }
  public class SetValueFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);

      Variable arg = Utils.GetItem(script);

      iOSVariable iosVar = Utils.GetVariable(varName, script) as iOSVariable;
      Utils.CheckNotNull(iosVar, m_name);

      SetValue(iosVar, arg.Value);

      return iosVar;
    }
    public static void SetValue(iOSVariable iosVar, double val)
    {
      if (iosVar.ViewX is UISwitch) {
        ((UISwitch)iosVar.ViewX).On = (int)val == 1;
      } else if (iosVar.ViewX is UISlider) {
        ((UISlider)iosVar.ViewX).Value = (float)val;
      } else if (iosVar.ViewX is UIStepper) {
        ((UIStepper)iosVar.ViewX).Value = (float)val;
      } else if (iosVar.ViewX is UIPickerView) {
        UIPickerView picker = iosVar.ViewX as UIPickerView;
        picker.Select((int)val, 0, true);
        TypePickerViewModel model = picker.Model as TypePickerViewModel;
        model?.Selected(picker, (int)val, 0);
      } else if (iosVar.ViewX is UISegmentedControl) {
        ((UISegmentedControl)iosVar.ViewX).SelectedSegment = (nint)val;
      } else {
        iosVar.SetValue(val);
      }
    }
  }
  public class GetValueFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);

      iOSVariable iosVar = Utils.GetVariable(varName, script) as iOSVariable;
      Utils.CheckNotNull(iosVar, m_name);

      double result = GetValue(iosVar);

      return new Variable(result);
    }
    public static double GetValue(iOSVariable iosVar)
    {
      double result = 0;
      if (iosVar.ViewX is UISwitch) {
        result = ((UISwitch)iosVar.ViewX).On ? 1 : 0;
      } else if (iosVar.ViewX is UISlider) {
        result = ((UISlider)iosVar.ViewX).Value;
      } else if (iosVar.ViewX is UIStepper) {
        result = ((UIStepper)iosVar.ViewX).Value;
      } else if (iosVar.ViewX is UIPickerView) {
        result = ((TypePickerViewModel)(((UIPickerView)iosVar.ViewX).Model)).SelectedRow;
      } else if (iosVar.ViewX is UISegmentedControl) {
        result = ((UISegmentedControl)iosVar.ViewX).SelectedSegment;
      } else {
        result = iosVar.CurrVal;
      }
      return result;
    }
  }
  public class AlignTitleFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);

      iOSVariable iosVar = Utils.GetVariable(varName, script) as iOSVariable;
      Utils.CheckNotNull(iosVar, m_name);
      UIView view = iosVar.ViewX;
      Utils.CheckNotNull(view, m_name);

      string alignment = Utils.GetItem(script).AsString();
      alignment = alignment.ToLower();

      Tuple<UIControlContentHorizontalAlignment, UITextAlignment> al = GetAlignment(alignment);
      iosVar.SetText(GetTextFunction.GetText(iosVar), alignment);

      if (view is UIButton) {
        ((UIButton)view).HorizontalAlignment = al.Item1;
      } else if (view is UILabel) {
        ((UILabel)view).TextAlignment = al.Item2;
      } else if (view is UITextField) {
        ((UITextField)view).TextAlignment = al.Item2;
      } else if (view is UITextView) {
        ((UITextView)view).TextAlignment = al.Item2;
      } else if (view is UIPickerView) {
        TypePickerViewModel model = ((UIPickerView)view).Model as TypePickerViewModel;
        if (model != null) {
          model.Alignment = al.Item2;
        }
      }

      return iosVar;
    }
    public static Tuple<UIControlContentHorizontalAlignment, UITextAlignment>
                  GetAlignment(string alignment)
    {
      UIControlContentHorizontalAlignment al1 = UIControlContentHorizontalAlignment.Center;
      UITextAlignment al2 = UITextAlignment.Center;
      switch (alignment) {
        case "left":
          al1 = UIControlContentHorizontalAlignment.Left;
          al2 = UITextAlignment.Left;
          break;
        case "right":
          al1 = UIControlContentHorizontalAlignment.Right;
          al2 = UITextAlignment.Right;
          break;
        case "bottom": // there is no bottom for iOS, only for Android
        case "center":
          al1 = UIControlContentHorizontalAlignment.Center;
          al2 = UITextAlignment.Center;
          break;
        case "fill":
        case "natural":
          al1 = UIControlContentHorizontalAlignment.Fill;
          al2 = UITextAlignment.Natural;
          break;
        case "justified":
          al1 = UIControlContentHorizontalAlignment.Center;
          al2 = UITextAlignment.Justified;
          break;
      }
      return new Tuple<UIControlContentHorizontalAlignment, UITextAlignment>(al1, al2);
    }
  }
  public class ShowHideFunction : ParserFunction
  {
    bool m_show;
    public ShowHideFunction(bool show)
    {
      m_show = show;
    }
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 1, m_name);

      string varName = Utils.GetSafeString(args, 0);
      Utils.CheckNotEmpty(script, varName, m_name);

      bool  show     = Utils.GetSafeInt(args, 1, m_show ? 1 : 0) != 0;

      iOSVariable iosVar = Utils.GetVariable(varName, script) as iOSVariable;

      //AppDelegate.ShowView(view, show);
      iOSApp.ShowView(iosVar, show, false);

      if (iosVar.ViewX is UITextField) {
        UITextField findText = iosVar.ViewX as UITextField;
        findText.EndEditing(!m_show);
        if (show) {
          findText.BecomeFirstResponder();
        } else {
          findText.ResignFirstResponder();
        }
      }
      return Variable.EmptyInstance;
    }
  }

  public class RemoveViewFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);

      ParserFunction func = ParserFunction.GetFunction(varName);
      Utils.CheckNotNull(func, varName);
      iOSVariable viewVar = func.GetValue(script) as iOSVariable;
      Utils.CheckNotNull(viewVar, m_name);

      //RemoveView(viewVar);
      iOSApp.RemoveView(viewVar);

      return Variable.EmptyInstance;
    }
    public static void RemoveView(iOSVariable viewVar)
    {
      if (viewVar == null || viewVar.ViewX == null) {
        return;
      }
      viewVar.ViewX.RemoveFromSuperview();
    }
  }
  public class RemoveAllViewsFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      iOSApp.RemoveAll();

      return Variable.EmptyInstance;
    }
  }

  public class SetImageFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);

      Variable imageNameVar = Utils.GetItem(script);
      string imageStr = imageNameVar.AsString();

      string imageName = UIUtils.String2ImageName(imageStr);

      UIView view = iOSVariable.GetView(varName, script);
      UIImage img = UtilsiOS.LoadImage(imageName);

      if (view is UIButton) {
        ((UIButton)view).SetBackgroundImage(img, UIControlState.Normal);
      } else if (view is UIImageView) {
        ((UIImageView)view).Image = img;
      } else {
        view.BackgroundColor = new UIColor(img);
      }

      return img != null ? new Variable(imageName) : Variable.EmptyInstance;
    }
  }
  public class SetBackgroundColorFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 2, m_name);

      string varName  = Utils.GetSafeString(args, 0);
      string strColor = Utils.GetSafeString(args, 1);
      double alpha    = Utils.GetSafeDouble(args, 2, 1.0);

      UIView view = iOSVariable.GetView(varName, script);
      if (view == null) {
        view = AppDelegate.GetCurrentView();
      }

      if (strColor.Equals("transparent", StringComparison.OrdinalIgnoreCase)) {
        view.Opaque = true;
        alpha = 0.0;
      } else {
        view.Opaque = false;
      }

      view.BackgroundColor = UtilsiOS.String2Color(strColor, alpha);
      return Variable.EmptyInstance;
    }
  }
  public class SetBackgroundImageFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      Variable imgVar = Utils.GetItem(script);
      string imageName = imgVar.AsString();
      Utils.CheckNotEmpty(script, imageName, m_name);

      UIImage img = UtilsiOS.LoadImage(imageName);

      AppDelegate.SetBgView(img);
      return Variable.EmptyInstance;
    }
  }

  public class AddActionFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 2, m_name);

      string varName   = Utils.GetSafeString(args, 0);
      string strAction = Utils.GetSafeString(args, 1);
      string argument  = Utils.GetSafeString(args, 2);

      iOSVariable iosVar = Utils.GetVariable(varName, script) as iOSVariable;
      AddAction(iosVar, varName, strAction, argument);

      return Variable.EmptyInstance;
    }
    public static void AddAction(iOSVariable iosVar, string varName,
                                 string strAction, string argument = "")
    {
      if (!string.IsNullOrWhiteSpace(argument)) {
        if (argument.Equals("FINISHED")) {
          if (iosVar.ViewX is UITextField) {
            UITextField textField = iosVar.ViewX as UITextField;
            textField.EditingDidEnd += (sender, e) => {
              UIVariable.GetAction(strAction, varName, "\"" + textField.Text + "\"");
            };
          }
          return;
        }
      }
      if (iosVar.ViewX is UIButton) {
        UIButton button = iosVar.ViewX as UIButton;
        button.TouchUpInside += (sender, e) => {
          UIVariable.GetAction(strAction, varName, "\"" + argument + "\"");
        };
      } else if (iosVar.ViewX is UISwitch) {
        UISwitch sw = iosVar.ViewX as UISwitch;
        sw.ValueChanged += (sender, e) => {
          UIVariable.GetAction(strAction, varName, "\"" + sw.On + "\"");
        };
      } else if (iosVar.ViewX is UITextField) {
        UITextField textField = iosVar.ViewX as UITextField;
        textField.EditingChanged += (sender, e) => {
          UIVariable.GetAction(strAction, varName, "\"" + textField.Text + "\"");
        };
      } else if (iosVar.ViewX is UISlider) {
        UISlider slider = iosVar.ViewX as UISlider;
        slider.ValueChanged += (sender, e) => {
          UIVariable.GetAction(strAction, varName, "\"" + slider.Value + "\"");
        };
      } else if (iosVar.ViewX is UISegmentedControl) {
        UISegmentedControl seg = iosVar.ViewX as UISegmentedControl;
        seg.ValueChanged += (sender, e) => {
          UIVariable.GetAction(strAction, varName, "\"" + seg.SelectedSegment + "\"");
        };
      } else if (iosVar.ViewX is UIPickerView) {
        UIPickerView pickerView = iosVar.ViewX as UIPickerView;
        TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
        if (model == null) {
          model = new TypePickerViewModel(AppDelegate.GetCurrentController());
        }
        model.RowSelected += (row) => {
          UIVariable.GetAction(strAction, varName, row.ToString());
        };
        pickerView.Model = model;
      } else if (iosVar.ViewX is UITableView) {
        UITableView tableView = iosVar.ViewX as UITableView;
        TableViewSource source = tableView.Source as TableViewSource;
        if (source == null) {
          source = new TableViewSource();
        }
        source.RowSelectedDel += (row) => {
          UIVariable.GetAction(strAction, varName, "\"" + row + "\"");
        };
        tableView.Source = source;
      } else {/*if (iosVar.ViewX is UIStepper) {
                UIStepper stepper = iosVar.ViewX as UIStepper;
                stepper.ValueChanged += (sender, e) => {
                    UIVariable.GetAction(strAction, varName, "\"" + stepper.Value + "\"");
                };*/
        iosVar.ActionDelegate += (arg1, arg2) => {
          UIVariable.GetAction(strAction, varName, "\"" + iosVar.CurrVal + "\"");
        };

      }
    }
  }

public class AddLongClickFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      //string varName = Utils.GetToken(script, Constants.NEXT_ARG_ARRAY);
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);
      script.MoveForwardIf(Constants.NEXT_ARG);

      Variable actionValue = Utils.GetItem(script);
      string strAction = actionValue.AsString();
      script.MoveForwardIf(Constants.NEXT_ARG);

      UIView view = iOSVariable.GetView(varName, script);

      var gr = new UILongPressGestureRecognizer();
      gr.AddTarget(() => this.ButtonLongPressed(gr, strAction, varName));

      view.AddGestureRecognizer(gr);

      return Variable.EmptyInstance;
    }

    void ButtonLongPressed(UILongPressGestureRecognizer gr,
                           string strAction, string varName)
    {
      if (gr.State != UIGestureRecognizerState.Began) {
        return;
      }
      UIVariable.GetAction(strAction, varName, "");
    }
  }
  public class AddSwipeFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 1, m_name);
      string varName = Utils.GetSafeString(args, 0);
      string direction = Utils.GetSafeString(args, 1);
      string strAction = Utils.GetSafeString(args, 2);

      UIView view = iOSVariable.GetView(varName, script);
      Utils.CheckNotNull(view, m_name);

      UISwipeGestureRecognizerDirection dir = UISwipeGestureRecognizerDirection.Left;
      switch (direction) {
        case "Left": dir = UISwipeGestureRecognizerDirection.Left; break;
        case "Right": dir = UISwipeGestureRecognizerDirection.Right; break;
        case "Down": dir = UISwipeGestureRecognizerDirection.Down; break;
        case "Up": dir = UISwipeGestureRecognizerDirection.Up; break;
      }
      UISwipeGestureRecognizer swiper = new UISwipeGestureRecognizer();
      swiper.Direction = dir;
      swiper.AddTarget(() => this.HandleSwipe(swiper, strAction, varName));
      view.AddGestureRecognizer(swiper);

      return Variable.EmptyInstance;
    }

    void HandleSwipe(UISwipeGestureRecognizer sender,
                                  string strAction, string varName)
    {
      Console.WriteLine("Swipe: " + sender.Direction);
      UIVariable.GetAction(strAction, varName, "\"" + sender.Direction + "\"");
    }
  }
  public class AddDragAndDropFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 1, m_name);
      string varName = Utils.GetSafeString(args, 0);

      UIView view = iOSVariable.GetView(varName, script);
      Utils.CheckNotNull(view, m_name);

      UIPanGestureRecognizer gesture = new UIPanGestureRecognizer();
      DragHandler handler = new DragHandler(view);
      // Wire up the event handler (have to use a selector)
      gesture.AddTarget(() => handler.HandleDrag(gesture));
      view.AddGestureRecognizer(gesture);

      return Variable.EmptyInstance;
    }

    public class DragHandler
    {
      UIView m_view;
      CGRect m_originalFrame;

      const float TOUCH_DELTA = 5;

      public DragHandler(UIView view)
      {
        m_view = view;
        m_originalFrame = m_view.Frame;
      }
      public void HandleDrag(UIPanGestureRecognizer recognizer)
      {
        if (recognizer.State == (UIGestureRecognizerState.Cancelled |
                                 UIGestureRecognizerState.Failed |
                                 UIGestureRecognizerState.Possible)) {
          return;
        }
        if (recognizer.State == UIGestureRecognizerState.Began) {
          m_originalFrame = m_view.Frame;
          return;
        }

        CGPoint offset = recognizer.TranslationInView(m_view);

        CGRect newFrame = m_originalFrame;
        newFrame.Offset(offset.X, offset.Y);

        //Console.WriteLine("Offsets: {0}, {1} -- {2}, {3}-- {4}, {5}", offset.X, offset.Y,
        //                  m_originalFrame.X, m_originalFrame.Y, newFrame.X, newFrame.Y);
        m_view.Frame = newFrame;
      }
    }
  }
  public class AddBorderFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 1, m_name);
      //UIView view = iOSVariable.GetView(args[0].AsString(), script);
      iOSVariable viewVar = args[0] as iOSVariable;
      UIView view = viewVar.ViewX;

      Utils.CheckNotNull(view, m_name);

      int width = Utils.GetSafeInt(args, 1, 1);
      int corner = Utils.GetSafeInt(args, 2, 5);
      string colorStr = Utils.GetSafeString(args, 3, "black");
      CGColor color = UtilsiOS.CGColorFromHex(colorStr);

      AddBorder(view, color, width, corner);

      return Variable.EmptyInstance;
    }
    public static void AddBorder(UIView view, CGColor borderColor = null, int width = 1, int corner = 5)
    {
      if (borderColor == null) {
        borderColor = UIColor.Black.CGColor;
      }
      view.Layer.BorderColor = borderColor;
      view.Layer.BorderWidth = width;
      view.Layer.CornerRadius = corner;
    }
  }
  public class AddTabFunction : ParserFunction
  {
    bool m_forceCreate;
    public AddTabFunction(bool forceCreate)
    {
      m_forceCreate = forceCreate;
    }
    protected override Variable Evaluate(ParsingScript script)
    {
      string text = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, text, m_name);

      string selectedImageName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, text, m_name);

      string notSelectedImageName = null;
      if (script.Current == Constants.NEXT_ARG) {
        notSelectedImageName = Utils.GetItem(script).AsString();
      }

      if (!m_forceCreate && iOSApp.SelectTab(text)) {
        return Variable.EmptyInstance;
      }

      iOSApp.AddTab(text, selectedImageName, notSelectedImageName);

      return Variable.EmptyInstance;
    }
  }
  public class GetSelectedTabFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      int tabId = iOSApp.Instance.SelectedTab;
      script.MoveForwardIf(Constants.END_ARG);
      return new Variable(tabId);
    }
  }
  public class SelectTabFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      int tabId = Utils.GetItem(script).AsInt();
      iOSApp.SelectTab(tabId);
      //iOSApp controller = AppDelegate.GetCurrentController() as iOSApp;
      //controller.SelectedIndex = tabId;
      return Variable.EmptyInstance;
    }
  }
  public class OnTabSelectedFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string action = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, action, m_name);

      iOSApp.TabSelectedDelegate += (tab) => {
        UIVariable.GetAction(action, "\"ROOT\"", "\"" + tab + "\"");
      };
      return Variable.EmptyInstance;
    }
  }
  public class ShowToastFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 1, m_name);
      string msg         = Utils.GetSafeString(args, 0);
      int duration       = Utils.GetSafeInt(args, 1, 10);
      string fgColorStr  = Utils.GetSafeString(args, 2);
      string bgColorStr  = Utils.GetSafeString(args, 3);

      double screenRatio = UtilsiOS.GetScreenRatio();
      double multiplier  = UtilsiOS.WidthMultiplier();

      int width          = (int)(Utils.GetSafeInt(args, 4, 560) / screenRatio * multiplier);
      int height         = (int)(Utils.GetSafeInt(args, 5, 120) / screenRatio * multiplier);

      if (string.IsNullOrEmpty(fgColorStr)) {
        fgColorStr = "#000000";
      }
      if (string.IsNullOrEmpty(bgColorStr)) {
        bgColorStr = "#D3D3D3";
      }

      UIColor fgColor = UtilsiOS.String2Color(fgColorStr);
      UIColor bgColor = UtilsiOS.String2Color(bgColorStr);

      UtilsiOS.ShowToast(msg, fgColor, bgColor, duration, width, height);

      return Variable.EmptyInstance;
    }
  }
  public class AlertDialogFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 2, m_name);
      string title = Utils.GetSafeString(args, 0);
      string msg = Utils.GetSafeString(args, 1);
      string buttonOK = Utils.GetSafeString(args, 2, "Dismiss");
      string actionOK = Utils.GetSafeString(args, 3);
      string buttonCancel = Utils.GetSafeString(args, 4);
      string actionCancel = Utils.GetSafeString(args, 5);

      UIViewController controller = AppDelegate.GetCurrentController();

      var okCancelAlertController = UIAlertController.Create(title, msg, UIAlertControllerStyle.Alert);
      var okAction = UIAlertAction.Create(buttonOK, UIAlertActionStyle.Default,
          alert => {
            if (!string.IsNullOrWhiteSpace(actionOK)) {
              UIVariable.GetAction(actionOK, "\"" + buttonOK + "\"", "1");
            }
          });
      okCancelAlertController.AddAction(okAction);

      if (!string.IsNullOrWhiteSpace(buttonCancel)) {
        var cancelAction = UIAlertAction.Create(buttonCancel, UIAlertActionStyle.Cancel,
            alert => {
              if (!string.IsNullOrWhiteSpace(actionCancel)) {
                UIVariable.GetAction(actionCancel, "\"" + buttonCancel + "\"", "0");
              }
            });
        okCancelAlertController.AddAction(cancelAction);
      }

      controller.PresentViewController(okCancelAlertController, true, null);
      return Variable.EmptyInstance;
    }
  }
  public class SetSizeFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 3, m_name);
      string varName = Utils.GetSafeString(args, 0);
      Utils.CheckNotEmpty(script, varName, m_name);

      Variable width = Utils.GetSafeVariable(args, 1);
      Utils.CheckNonNegativeInt(width);

      Variable height = Utils.GetSafeVariable(args, 2);
      Utils.CheckNonNegativeInt(height);

      UIView view = iOSVariable.GetView(varName, script);

      if (view is UIPickerView) {
        UIPickerView pickerView = view as UIPickerView;
        TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
        if (model != null) {
          model.SetSize(width.AsInt(), height.AsInt());
          pickerView.Model = model;
        }
      } else {
        CGRect newFrame = view.Frame;
        newFrame.Size = new CGSize((int)width.Value, (int)height.Value);
        view.Frame = newFrame;
      }

      return Variable.EmptyInstance;
    }
  }
  public class SetOptionsFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      Utils.CheckArgs(args.Count, 2, m_name);
      iOSVariable viewVar = args[0] as iOSVariable;
      Utils.CheckNotNull(viewVar, m_name);

      UIView view = viewVar.ViewX;

      string option = args[1].AsString().ToLower();
      switch (option) {
        case "multiline":
          SetMultiline(view);
          break;
      }
      return viewVar;
    }
    public static void SetMultiline(UIView view, bool multiline = true)
    {
      UILabel label = null;
      if (view is UIButton) {
        UIButton button = (UIButton)view;
        label = button.TitleLabel;
      } else if (view is UILabel) {
        label = (UILabel)view;
      }

      if (label == null) {
        return;
      }
      label.Lines = multiline ? 25 : 1;
    }
  }
  public class SetFontSizeFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      //string varName = Utils.GetToken(script, Constants.NEXT_ARG_ARRAY);
      string varName = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, varName, m_name);
      script.MoveForwardIf(Constants.NEXT_ARG);

      Variable fontSize = Utils.GetItem(script);
      Utils.CheckPosInt(fontSize);

      UIView view = iOSVariable.GetView(varName, script);

      UIFont newFont = UIFont.SystemFontOfSize((int)fontSize.Value);

      if (view is UIButton) {
        UIButton button = (UIButton)view;
        button.TitleLabel.Font = newFont; ;
      } else if (view is UILabel) {
        ((UILabel)view).Font = newFont;
      } else if (view is UITextField) {
        ((UITextField)view).Font = newFont;
      } else if (view is UITextView) {
        ((UITextView)view).Font = newFont;
      } else {
        return Variable.EmptyInstance;
      }

      return fontSize;
    }
  }
  public class SetFontColorFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      string varName = args[0].AsString();
      UIView view = iOSVariable.GetView(varName, script);
      Utils.CheckNotNull(view, m_name);

      string colorStr = args[1].AsString();
      UIColor color = UtilsiOS.String2Color(colorStr);

      if (view is UIButton) {
        UIButton button = (UIButton)view;
        button.SetTitleColor(color, UIControlState.Normal);
      } else if (view is UILabel) {
        ((UILabel)view).TextColor = color;
      } else if (view is UITextField) {
        ((UITextField)view).TextColor = color;
      } else if (view is UITextView) {
        ((UITextView)view).TextColor = color;
      }

      return Variable.EmptyInstance;
    }
  }

  public class GadgetSizeFunction : ParserFunction
  {
    bool m_needWidth;
    public GadgetSizeFunction(bool needWidth = true)
    {
      m_needWidth = needWidth;
    }
    protected override Variable Evaluate(ParsingScript script)
    {
      script.MoveForwardIf(Constants.END_ARG_ARRAY);

      var bounds = UIScreen.MainScreen.NativeBounds;
      return new Variable(m_needWidth ? bounds.Width : bounds.Height);
    }
  }

  public class AllowedOrientationFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
          Constants.START_ARG, Constants.END_ARG, out isList);

      string orientation = Utils.GetSafeString(args, 0).ToLower();

      UIInterfaceOrientationMask orientationMask = orientation == "landscape" ?
        UIInterfaceOrientationMask.Landscape :
        UIInterfaceOrientationMask.Portrait;

      iOSApp.OrientationMask = orientationMask;

      return Variable.EmptyInstance;
    }
  }
  public class RegisterOrientationChangeFunction : ParserFunction
  {
    const string DEFAULT_ORIENTATION = "Portrait";
    static string m_actionPortrait;
    static string m_actionLandscape;
    static string m_currentOrientation;

    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      m_actionPortrait  = Utils.GetSafeString(args, 0);
      m_actionLandscape = Utils.GetSafeString(args, 1);
      bool   startNow   = Utils.GetSafeInt(args, 2, 1) != 0;

      UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
      NSNotificationCenter.DefaultCenter.AddObserver(
          new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);

      if (startNow) {
        PerformAction(DEFAULT_ORIENTATION, true);
      }

      return Variable.EmptyInstance;
    }
    static void DeviceRotated(NSNotification notification)
    {
      string currentOrientation = iOSApp.Orientation;
      //Console.WriteLine("DeviceRotated {0} --> {1} before: {2}",
      //  UIDevice.CurrentDevice.Orientation, currentOrientation, m_currentOrientation);
      if (!iOSApp.ValidOrientation || m_currentOrientation == currentOrientation) {
        return;
      }

      PerformAction(currentOrientation);
    }
    static void PerformAction(string orientation, bool isInit = false)
    {
      m_currentOrientation = orientation;
      int currentTab = iOSApp.Instance.SelectedTab;

      if (!isInit) {
        iOSApp.RemoveAll();
      }

      string action = iOSApp.IsLandscape ? m_actionLandscape : m_actionPortrait;
      iOSApp.Instance.OffsetTabBar(false);

      UIVariable.GetAction(action, "\"ROOT\"", "\"" + (isInit ? "init" : m_currentOrientation) + "\"");

      if (!isInit && currentTab >= 0) {
        iOSApp.SelectTab(currentTab);
      }
    }
  }
  public class OrientationChangeFunction : ParserFunction
  {
    string m_action;
    static UIDeviceOrientation m_currentOrientation = UIDeviceOrientation.Unknown;
    protected override Variable Evaluate(ParsingScript script)
    {
      Variable actionValue = Utils.GetItem(script);
      m_action = actionValue.AsString();
      Utils.CheckNotEmpty(script, m_action, m_name);

      UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
      NSNotificationCenter.DefaultCenter.AddObserver(
          new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);

      return Variable.EmptyInstance;
    }
    void DeviceRotated(NSNotification notification)
    {
      UIDeviceOrientation currentOrientation = UIDevice.CurrentDevice.Orientation;
      if (m_currentOrientation == currentOrientation) {
        return;
      }
      m_currentOrientation = currentOrientation;
      iOSApp.Instance.OffsetTabBar(false);
      UIVariable.GetAction(m_action, "\"ROOT\"", "\"" + m_currentOrientation + "\"");
    }
  }
  public class OrientationFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      script.MoveForwardIf(Constants.END_ARG_ARRAY);

      UIInterfaceOrientation orientation = AppDelegate.GetCurrentController().InterfaceOrientation;
      orientation = UIApplication.SharedApplication.StatusBarOrientation;
      string or = orientation == UIInterfaceOrientation.LandscapeLeft ||
                  orientation == UIInterfaceOrientation.LandscapeRight ? "Landscape" : "Portrait";

      UIDeviceOrientation orientatation2 = UIDevice.CurrentDevice.Orientation;

      return new Variable(or);
    }
  }
  public class KillMeFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      UIApplication app = UIApplication.SharedApplication;
      app.PerformSelector(new ObjCRuntime.Selector("suspend"), null, 0.25);
      NSThread.SleepFor(0.5);

      System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
      NSThread.SleepFor(0.5);
      System.Threading.Thread.CurrentThread.Abort();

      return Variable.EmptyInstance;
    }
  }
  public class OnEnterBackgroundFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      string strAction = args[0].AsString();

      AppDelegate.OnEnterBackgroundDelegate += () => {
        AppDelegate.GetCurrentController().InvokeOnMainThread(() => {
          UIVariable.GetAction(strAction, "\"ROOT\"", "\"OnEnterBackground\"");
        });
      };

      return Variable.EmptyInstance;
    }
  }

  public class InitTTSFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      TTS.Init();

      script.MoveForwardIf(Constants.END_ARG_ARRAY);
      return Variable.EmptyInstance;
    }
  }
  public class SpeakFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      string phrase = args[0].AsString();
      TTS.Voice     = Utils.GetSafeString(args, 1, TTS.Voice);
      bool force    = Utils.GetSafeInt(args, 2) != 0;

      TTS.Speak(phrase, force);

      return Variable.EmptyInstance;
    }
  }
  public class SpeechOptionsFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      TTS.Init();

      string option = args[0].AsString();
      Variable optionValue = Utils.GetSafeVariable(args, 1);
      Utils.CheckNotNull(optionValue, m_name);

      switch(option) {
        case "sound": TTS.Sound = optionValue.AsInt() == 1;
          break;
        case "voice":
          TTS.Voice = optionValue.AsString();
          break;
        case "speechRate":
          TTS.SpeechRate = (float)optionValue.AsDouble();
          break;
        case "volume":
          TTS.Volume = (float)optionValue.AsDouble();
          break;
        case "pitch":
          TTS.PitchMultiplier = (float)optionValue.AsDouble();
          break;
      }

      return Variable.EmptyInstance;
    }
  }
  public class VoiceFunction : ParserFunction
  {
    static STT m_speech = null;
    public static  STT LastRecording { get { return m_speech; }}

    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      string strAction = args[0].AsString();
      STT.Voice = Utils.GetSafeString(args, 1, STT.Voice).Replace('_', '-');

      bool speechEnabled = UIDevice.CurrentDevice.CheckSystemVersion(10, 0);
      if (!speechEnabled) {
        UIVariable.GetAction(strAction, "\"" +
         string.Format("Speech recognition requires iOS 10.0 or higher. You have iOS {0}",
                       UIDevice.CurrentDevice.SystemVersion) + "\"", "");
        return Variable.EmptyInstance;
      }

      if (!STT.Init()) {
        // The user didn't authorize accessing the microphone.
        return Variable.EmptyInstance;
      }

      UIViewController controller = AppDelegate.GetCurrentController();
      m_speech = new STT(controller);
      m_speech.SpeechError += (errorStr) => {
        Console.WriteLine(errorStr);
        controller.InvokeOnMainThread(() => {
          UIVariable.GetAction(strAction, "\"" + errorStr + "\"", "");
        });
      };
      m_speech.SpeechOK += (recognized) => {
        Console.WriteLine("Recognized: " + recognized);
        controller.InvokeOnMainThread(() => {
          UIVariable.GetAction(strAction, "", "\"" + recognized + "\"");
        });
      };

      m_speech.StartRecording(STT.Voice);

      return Variable.EmptyInstance;
    }
  }
  public class StopVoiceFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      VoiceFunction.LastRecording?.StopRecording();
      script.MoveForwardIf(Constants.END_ARG);
      return Variable.EmptyInstance;
    }
  }

  public class LocalizedFunction : ParserFunction
  {
    static NSBundle m_bundle = NSBundle.MainBundle;

    Dictionary<string, string> m_localizations = new Dictionary<string, string>();

    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      string key = args[0].AsString();
      string langCode = Utils.GetSafeString(args, 1);
      string currentCode = Localization.CurrentCode;
      string localized;

      if (!string.IsNullOrWhiteSpace(langCode)) {
        if (m_localizations.TryGetValue(langCode + key, out localized)) {
          return new Variable(localized);
        }
        if (langCode != currentCode) {
          Localization.SetProgramLanguageCode(langCode);
        }
      }

      localized = Localization.GetText(key);

      if (!string.IsNullOrWhiteSpace(langCode)) {
        Localization.SetProgramLanguageCode(currentCode);
        m_localizations[langCode + key] = localized;
      }

      return new Variable(localized);
    }
  }
  public class InitAds : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      string appId = args[0].AsString();
      string interstId = Utils.GetSafeString(args, 1);
      string bannerId = Utils.GetSafeString(args, 1);

      AdMob.Init(appId, interstId, bannerId);

      return Variable.EmptyInstance;
    }
  }
  public class ShowInterstitial : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      AdMob.ShowInterstitialAd(AppDelegate.GetCurrentController());
      script.MoveForwardIf(Constants.END_ARG);

      return Variable.EmptyInstance;
    }
  }
  public class InitIAPFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      // Just consume the arguments: not needed for iOS:
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      IAP.Init();

      return Variable.EmptyInstance;
    }
  }
  public class RestoreFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);
      string strAction = args[0].AsString();

      for (int i = 1; i < args.Count; i++) {
        string productId = Utils.GetSafeString(args, i);
        IAP.AddProductId(productId);
      }

      IAP.IAPOK += (productIds) => {
        UIVariable.GetAction(strAction, "", "\"" + productIds + "\"");
      };
      IAP.IAPError += (errorStr) => {
        UIVariable.GetAction(strAction, "\"" + errorStr + "\"", "");
      };

      IAP.Restore();

      return Variable.EmptyInstance;
    }
  }
  public class PurchaseFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);
      string strAction = args[0].AsString();
      string productId = args[1].AsString();

      IAP.AddProductId(productId);

      IAP.IAPOK += (productIds) => {
        UIVariable.GetAction(strAction, "", "\"" + productIds + "\"");
      };
      IAP.IAPError += (errorStr) => {
        UIVariable.GetAction(strAction, "\"" + errorStr + "\"", "");
      };

      IAP.Purchase(productId);

      return Variable.EmptyInstance;
    }
  }
  public class ReadFileFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string filename = Utils.GetStringOrVarValue(script);
      string[] lines  = Utils.GetFileLines(filename);

      List<Variable> results = Utils.ConvertToResults(lines);
      return new Variable(results);
    }
  }
  class ImportFileFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string filename = Utils.GetItem(script).AsString();

      string[] lines = Utils.GetFileLines(filename);
      string includeFile = string.Join(Environment.NewLine, lines);

      Dictionary<int, int> char2Line;
      string includeScript = Utils.ConvertToScript(includeFile, out char2Line);
      ParsingScript tempScript = new ParsingScript(includeScript, 0, char2Line);
      tempScript.Filename = filename;
      tempScript.OriginalScript = string.Join(Constants.END_LINE.ToString(), lines);

      while (tempScript.Pointer < includeScript.Length) {
        tempScript.ExecuteTo();
        tempScript.GoToNextStatement();
      }
      return Variable.EmptyInstance;
    }
  }
  public class PauseFunction : ParserFunction
  {
    static Dictionary<string, System.Timers.Timer> m_timers =
       new Dictionary<string, System.Timers.Timer>();

    bool m_startTimer;

    public PauseFunction(bool startTimer) {
      m_startTimer = startTimer;
    }
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);

      if (!m_startTimer) {
        Utils.CheckArgs(args.Count, 1, m_name);
        string cancelTimerId = Utils.GetSafeString(args, 0);
        System.Timers.Timer cancelTimer;
        if (m_timers.TryGetValue(cancelTimerId, out cancelTimer)) {
          cancelTimer.Stop();
          cancelTimer.Dispose();
          m_timers.Remove(cancelTimerId);
        }
        return Variable.EmptyInstance;
      }

      Utils.CheckArgs(args.Count, 2, m_name);
      int timeout = args[0].AsInt();
      string strAction = args[1].AsString();
      string owner = Utils.GetSafeString(args, 2);
      string timerId = Utils.GetSafeString(args, 3);
      bool autoReset = Utils.GetSafeInt(args, 4, 0) != 0;

      System.Timers.Timer pauseTimer = new System.Timers.Timer(timeout);
      pauseTimer.Elapsed += (sender, e) => {
        if (!autoReset) {
          pauseTimer.Stop();
          pauseTimer.Dispose();
          m_timers.Remove(timerId);
        }
        //Console.WriteLine("QuizTimer_Elapsed {0:HH:mm:ss.fff}", e.SignalTime);
        AppDelegate.GetCurrentController().InvokeOnMainThread(() => {
          UIVariable.GetAction(strAction, owner, "\"" + timerId + "\"");
        });
      };
      pauseTimer.AutoReset = autoReset;
      m_timers[timerId] = pauseTimer;

      pauseTimer.Start();

      return Variable.EmptyInstance;
    }
  }

  public class GetDeviceLocale : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string code = Localization.GetDeviceLangCode();
      script.MoveForwardIf(Constants.END_ARG);
      return new Variable(code);
    }
  }
  public class SetAppLocale : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string code = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, code, m_name);

      bool found = Localization.SetProgramLanguageCode(code);

      return new Variable(found);
    }
  }
  public class OpenURLFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      string urlStr = Utils.GetItem(script).AsString();
      Utils.CheckNotEmpty(script, urlStr, m_name);

      if (!urlStr.StartsWith("http") && !urlStr.StartsWith("itms")) {
        urlStr = "http://" + urlStr;
      }

      NSUrl url = new NSUrl(urlStr);
      UIApplication.SharedApplication.OpenUrl(url);

      return new Variable(urlStr);
    }
  }
  public class TranslateTabBar : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      iOSApp.TranslateTabs();
      script.MoveForwardIf(Constants.END_ARG);
      return Variable.EmptyInstance;
    }
  }
  public class GetSettingFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);
      string settingName = args[0].AsString();
      string strType = Utils.GetSafeString(args, 1, "string");
      Variable defValue = Utils.GetSafeVariable(args, 2);

      switch (strType) {
        case "float":
        case "double":
          float def = defValue == null ? -1 : (float)defValue.AsDouble();
          float result = Settings.GetFloatSetting(settingName, def);
          return new Variable(result);
        case "int":
          int defInt = defValue == null ? -1 : defValue.AsInt();
          int resultInt = Settings.GetIntSetting(settingName, defInt);
          return new Variable(resultInt);
        case "bool":
          bool defBool = defValue == null ? false : defValue.AsInt() != 0;
          bool resultBool = Settings.GetBoolSetting(settingName, defBool);
          return new Variable(resultBool);
        default:
          string defStr = defValue == null ? null : defValue.AsString();
          string resultStr = Settings.GetSetting(settingName, defStr);
          return new Variable(resultStr);
      }
    }
  }
  public class SetSettingFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);
      string settingName = args[0].AsString();
      Variable settingValue = Utils.GetSafeVariable(args, 1);
      string strType = Utils.GetSafeString(args, 2, "string");

      switch (strType) {
        case "float":
        case "double":
          float setting = (float)settingValue.AsDouble();
          Settings.SaveSetting(settingName, setting);
          break;
        case "int":
          int settingInt = settingValue.AsInt();
          Settings.SaveSetting(settingName, settingInt);
          break;
        case "bool":
          bool settingBool = settingValue.AsInt() != 0;
          Settings.SaveSetting(settingName, settingBool);
          break;
        default:
          string settingStr = settingValue.AsString();
          Settings.SaveSetting(settingName, settingStr);
          break;
      }

      return Variable.EmptyInstance;
    }
  }
}
