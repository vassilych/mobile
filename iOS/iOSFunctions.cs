using System;
using System.Collections.Generic;
using CoreAnimation;
using CoreGraphics;
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

            string viewNameX    = args[0].AsString();
            string ruleStrX     = args[1].AsString();
            string viewNameY    = args[2].AsString();
            string ruleStrY     = args[3].AsString();

            int leftMargin      = Utils.GetSafeInt(args, 4);
            int topMargin       = Utils.GetSafeInt(args, 5);
            Variable parentView = Utils.GetSafeVariable(args, 6, null);

            UIView referenceViewX = iOSVariable.GetView(viewNameX, script);
            UIView referenceViewY = iOSVariable.GetView(viewNameY, script);
            iOSVariable location = new iOSVariable(UIVariable.UIType.LOCATION,
                                                   viewNameX + viewNameY, referenceViewX,
                                                   referenceViewY);

            location.SetRules(ruleStrX, ruleStrY);
            location.ParentView = parentView as UIVariable;

            double screenRatio    = UtilsiOS.GetScreenRatio();
            location.TranslationX = (int)(leftMargin / screenRatio);
            location.TranslationY = (int)(topMargin  / screenRatio);

            return location;
        }
    }
    public class AddWidgetFunction : ParserFunction
    {
        public AddWidgetFunction(string widgetType = "")
        {
            m_widgetType = widgetType;
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

            if (widgetType == "Combobox") {
                height *= 2;
            }

            location.SetSize(width, height);
            CGSize parentSize = location.GetParentSize();

            location.X = UtilsiOS.String2Position(location.RuleX, location.ViewX, location, parentSize, true);
            location.Y = UtilsiOS.String2Position(location.RuleY, location.ViewY, location, parentSize, false);

            location.X += location.TranslationX;
            location.Y += location.TranslationY;

            CGRect rect = new CGRect(location.X, location.Y, width, height);

            iOSVariable widgetFunc = GetWidget(widgetType, varName, text, rect);
            Utils.CheckNotNull(widgetFunc, m_name);

            var currView = location.GetParentView();
            currView.Add(widgetFunc.ViewX);

            iOSApp.AddView(widgetFunc.ViewX);

            ParserFunction.AddGlobal(varName, new GetVarFunction(widgetFunc));
            return widgetFunc;
        }
        public static iOSVariable GetWidget(string widgetType, string widgetName, string initArg, CGRect rect)
        {
            UIVariable.UIType type = UIVariable.UIType.NONE;
            UIView widget = null;
            switch (widgetType)
            {
                case "View":
                    type = UIVariable.UIType.VIEW;
                    widget = new UIView(rect);
                    break;
                case "Button":
                    type = UIVariable.UIType.BUTTON;
                    widget = new UIButton(rect);
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
                        UIImage img = UIImage.FromBundle(initArg);
                        ((UIImageView)widget).Image = img;
                    }
                    break;
                case "Combobox":
                case "TypePicker":
                    type = UIVariable.UIType.PICKER_VIEW;
                    widget = new UIPickerView(rect);
                    ((UIPickerView)widget).AutosizesSubviews = true;
                    SetValues(widget, initArg);
                    break;
                case "Switch":
                    type = UIVariable.UIType.SWITCH;
                    widget = new UISwitch(rect);
                    SetValues(widget, initArg);
                    break;
                case "Slider":
                    type = UIVariable.UIType.SLIDER;
                    widget = new UISlider(rect);
                    SetValues(widget, initArg);
                    break;
                default:
                    type = UIVariable.UIType.VIEW;
                    widget = new UIView(rect);
                    break;
            }
            SetOptionsFunction.SetMultiline(widget);

            iOSVariable widgetFunc = new iOSVariable(type, widgetName, widget);
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
        public static void SetValues(UIView view, string valueStr)
        {
            double value1, value2 = 0;
            string[] vals = valueStr.Split(new char[] { ',', ':' });
            Double.TryParse(vals[0].Trim(), out value1);
            Double.TryParse(vals[vals.Length - 1].Trim(), out value2);

            if (view is UISwitch) {
                ((UISwitch)view).On = (int)value1 == 1;
            } else if (view is UISlider) {
                ((UISlider)view).MinValue = (float)value1;
                ((UISlider)view).MaxValue = (float)value2;
                ((UISlider)view).Value = (float)(value1 + value2)/2;
            } else if (view is UIPickerView) {
                TypePickerViewModel model = new TypePickerViewModel(AppDelegate.GetCurrentController());
                ((UIPickerView)view).Model = model;
            }
        }

        string m_widgetType;
    }

    public class AddWidgetDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            UIPickerView pickerView = iOSVariable.GetView(varName, script) as UIPickerView;
            Utils.CheckNotNull(pickerView, m_name);

            Variable data = Utils.GetItem(script);
            Utils.CheckNotNull(data.Tuple, m_name);         

            List<string> types = new List<string>(data.Tuple.Count);
            for (int i = 0; i < data.Tuple.Count; i++) {
                types.Add(data.Tuple[i].AsString());
            }

            Variable actionValue = Utils.GetItem(script);
            string strAction = actionValue.AsString();
            script.MoveForwardIf(Constants.NEXT_ARG);

            TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
            if (model == null) {
                model = new TypePickerViewModel(AppDelegate.GetCurrentController());
            }
            model.Data = types;

            string alignment = Utils.GetItem(script).AsString();
            if (!string.IsNullOrWhiteSpace(alignment)) {
                var al = AlignTitleFunction.GetAlignment(alignment);
                model.Alignment = al.Item2;
            }

            model.RowSelected += (row) => {
                UIVariable.GetAction(strAction, varName, row.ToString());
            };
            model.SetSize((int)pickerView.Bounds.Width, (int)pickerView.Bounds.Height / 4);
            pickerView.Model = model;

            return Variable.EmptyInstance;
        }
    }
    public class AddWidgetImagesFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            UIPickerView pickerView = iOSVariable.GetView(varName, script) as UIPickerView;
            Utils.CheckNotNull(pickerView, m_name);

            Variable data = Utils.GetItem(script);
            Utils.CheckNotNull(data.Tuple, m_name);

            List<UIImage> images = new List<UIImage>(data.Tuple.Count);
            for (int i = 0; i < data.Tuple.Count; i++) {
                UIImage img = UIImage.FromBundle(data.Tuple[i].AsString());
                images.Add(img);
            }

            Variable actionValue = Utils.GetItem(script);
            string strAction = actionValue.AsString();
            script.MoveForwardIf(Constants.NEXT_ARG);

            TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
            if (model == null) {
                model = new TypePickerViewModel(AppDelegate.GetCurrentController());
            }
            model.Images = images;

            if (!string.IsNullOrWhiteSpace(strAction)) {
                model.RowSelected += (row) => {
                    UIVariable.GetAction(strAction, varName, row.ToString());
                };
            }
            model.SetSize((int)pickerView.Bounds.Width, (int)pickerView.Bounds.Height / 4);
            pickerView.Model = model;

            return Variable.EmptyInstance;
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
        public static void SetText(UIView view, string text)
        {
            if (view is UIButton) {
                ((UIButton)view).SetTitle(text, UIControlState.Normal);
            } else if (view is UILabel) {
                ((UILabel)view).Text = text;
            } else if (view is UITextField) {
                ((UITextField)view).Text = text;
            } else if (view is UITextView) {
                ((UITextView)view).Text = text;
            } else if (view is UIPickerView) {
                UIPickerView picker = view as UIPickerView;
                TypePickerViewModel model = picker.Model as TypePickerViewModel;
                int row = model.StringToRow(text);
                SetValueFunction.SetValue(view, row);
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
        public static string GetText(UIView view)
        {
            string result = "";
            if (view is UIButton) {
                result = ((UIButton)view).Title(UIControlState.Normal);
            } else if (view is UILabel) {
                result = ((UILabel)view).Text;
            } else if (view is UITextField) {
                result = ((UITextField)view).Text;
            } else if (view is UITextView) {
                result = ((UITextView)view).Text;
            } else if (view is UIPickerView) {
                UIPickerView pickerView = view as UIPickerView;
                TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
                int row = (int)GetValueFunction.GetValue(pickerView);
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
            UIView view = iosVar.ViewX;

            SetValue(view, arg.Value);

            return iosVar;
        }
        public static void SetValue(UIView view, double val)
        {
            if (view is UISwitch) {
                ((UISwitch)view).On = (int)val == 1;
            } else if (view is UISlider) {
                ((UISlider)view).Value = (float)val;
            } else if (view is UIPickerView) {
                ((UIPickerView)view).Select((int)val, 0, true);
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

            double result = GetValue(iosVar.ViewX);

            return new Variable(result);
        }
        public static double GetValue(UIView view)
        {
            double result = 0;
            if (view is UISwitch) {
                result = ((UISwitch)view).On ? 1 : 0;
            } else if (view is UISlider) {
                result = ((UISlider)view).Value;
            } else if (view is UIPickerView) {
                result = ((TypePickerViewModel)(((UIPickerView)view).Model)).SelectedRow;
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
            iosVar.SetText(GetTextFunction.GetText(view), alignment);

            if (view is UIButton) {
                ((UIButton)view).HorizontalAlignment = al.Item1;
            } else if (view is UILabel) {
                ((UILabel)view).TextAlignment = al.Item2;
            } else if (view is UITextField) {
                ((UITextField)view).TextAlignment = al.Item2;
            } else if (view is UITextView) {
                ((UITextView)view).TextAlignment = al.Item2;
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
                case "center":
                    al1 = UIControlContentHorizontalAlignment.Center;
                    al2 = UITextAlignment.Center;
                    break;
                case "fill":
                case "natural":
                    al1 = UIControlContentHorizontalAlignment.Fill;
                    al2 = UITextAlignment.Natural;
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
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            UIView view = iOSVariable.GetView(varName, script);
            if (view == null) {
                view = AppDelegate.GetCurrentView();
            }

            AppDelegate.ShowView(view, m_show);
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
            string imageName = imageNameVar.AsString();

            UIView view = iOSVariable.GetView(varName, script);
            UIImage img = UIImage.FromBundle(imageName);

            if (view is UIButton) {
                ((UIButton)view).SetBackgroundImage(img, UIControlState.Normal);
            } else if (view is UIImageView) {
                ((UIImageView)view).Image = img;
            } else {
                view.BackgroundColor = new UIColor(img);
            }

            return Variable.EmptyInstance;
        }
    }
    public class SetBackgroundColorFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            Variable color = Utils.GetItem(script);
            string strColor = color.AsString();

            UIView view = iOSVariable.GetView(varName, script);
            if (view == null) {
                view = AppDelegate.GetCurrentView();
            }
            view.BackgroundColor = UtilsiOS.String2Color(strColor);

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

            UIImage img = UIImage.FromBundle(imageName);

            AppDelegate.SetBgView(img);
            return Variable.EmptyInstance;
        }
    }
    
    public class AddActionFunction : ParserFunction
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
            AddAction(view, varName, strAction);

            return Variable.EmptyInstance;
        }
        public static void AddAction(UIView view, string varName, string strAction)
        {
            if (view is UIButton) {
                UIButton button = view as UIButton;
                button.TouchUpInside += (sender, e) => {
                    UIVariable.GetAction(strAction, varName, "\"" + e + "\"");
                };
            } else if (view is UISwitch) {
                UISwitch sw = view as UISwitch;
                sw.ValueChanged += (sender, e) => {
                    UIVariable.GetAction(strAction, varName, "\"" + e + "\"");
                };
            } else if (view is UISlider) {
                UISlider slider = view as UISlider;
                slider.ValueChanged += (sender, e) => {
                    UIVariable.GetAction(strAction, varName, "\"" + e + "\"");
                };
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

            int width       = Utils.GetSafeInt(args, 1, 1);
            int corner      = Utils.GetSafeInt(args, 2, 5);
            string colorStr = Utils.GetSafeString(args, 3, "black");
            CGColor color   = UtilsiOS.CGColorFromHex(colorStr);
  
            AddBorder(view, color, width, corner);

            return Variable.EmptyInstance;
        }
        public static void AddBorder(UIView view, CGColor borderColor = null, int width = 1, int corner = 5)
        {
            if (borderColor == null) {
                borderColor = UIColor.Black.CGColor;
            }
            view.Layer.BorderColor  = borderColor;
            view.Layer.BorderWidth  = width;
            view.Layer.CornerRadius = corner;
        }
    }
    public class AddTabFunction : ParserFunction
    {
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

            iOSApp.AddTab(text, selectedImageName, notSelectedImageName);

            return Variable.EmptyInstance;
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
    public class ShowToastFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            bool isList = false;
            List<Variable> args = Utils.GetArgs(script,
                Constants.START_ARG, Constants.END_ARG, out isList);

            Utils.CheckArgs(args.Count, 1, m_name);
            string msg        = Utils.GetSafeString(args, 0);
            int duration      = Utils.GetSafeInt   (args, 1, 10);
            string fgColorStr = Utils.GetSafeString(args, 2);
            string bgColorStr = Utils.GetSafeString(args, 3);

            if (string.IsNullOrEmpty(fgColorStr)) {
                fgColorStr = "#FFFFFF";
            }
            if (string.IsNullOrEmpty(bgColorStr)) {
                bgColorStr = "#D3D3D3";
            }

            UIColor fgColor = UtilsiOS.String2Color(fgColorStr);
            UIColor bgColor = UtilsiOS.String2Color(bgColorStr);

            UtilsiOS.ShowToast(msg, fgColor, bgColor, duration);

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
            string title        = Utils.GetSafeString(args, 0);
            string msg          = Utils.GetSafeString(args, 1);
            string buttonOK     = Utils.GetSafeString(args, 2, "Dismiss");
            string actionOK     = Utils.GetSafeString(args, 3);
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
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);
            script.MoveForwardIf(Constants.NEXT_ARG);

            Variable width = Utils.GetItem(script);
            Utils.CheckPosInt(width);
            script.MoveForwardIf(Constants.NEXT_ARG);

            Variable height = Utils.GetItem(script);
            Utils.CheckPosInt(height);
            script.MoveForwardIf(Constants.NEXT_ARG);

            UIView view = iOSVariable.GetView(varName, script);

            CGRect newFrame = view.Frame;
            newFrame.Size = new CGSize((int)width.Value, (int)height.Value);
            view.Frame = newFrame;

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
            switch(option) {
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

    public class GadgetSizeFunction : ParserFunction
    {
        bool m_needWidth;
        public GadgetSizeFunction(bool needWidth = true)
        {
            m_needWidth = needWidth;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            var bounds = UIScreen.MainScreen.NativeBounds;
            return new Variable(m_needWidth ? bounds.Width : bounds.Height);
        }
    }

    public class InvokeNativeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string methodName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, methodName, m_name);
            script.MoveForwardIf(Constants.NEXT_ARG);

            string paramName = Utils.GetToken(script, Constants.NEXT_ARG_ARRAY);
            Utils.CheckNotEmpty(script, paramName, m_name);
            script.MoveForwardIf(Constants.NEXT_ARG);

            Variable paramValueVar = Utils.GetItem(script);
            string paramValue = paramValueVar.AsString();

            var result = Utils.InvokeCall(typeof(Statics),
                                          methodName, paramName, paramValue);
            return result;
        }
    }
}
