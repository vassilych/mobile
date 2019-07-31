using System;
using System.Collections.Generic;

using UIKit;
using CoreGraphics;

using SplitAndMerge;
using CoreAnimation;
using CoreText;
using Foundation;

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
            if (type != UIType.LOCATION && m_viewX != null)
            {
                m_viewX.Tag = ++m_currentTag;
            }
        }

        public override Variable Clone()
        {
            iOSVariable newVar = (iOSVariable)this.MemberwiseClone();
            return newVar;
        }

        public UIView ViewX
        {
            get { return m_viewX; }
            set { m_viewX = value; }
        }
        public UIView ViewY
        {
            get { return m_viewY; }
            set { m_viewY = value; }
        }

        public virtual iOSVariable GetWidget(string widgetType, string widgetName, string initArg, CGRect rect)
        {
            UIVariable.UIType type = UIVariable.UIType.NONE;
            iOSVariable widgetFunc = null;
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
                    UITextField textField = ((UITextField)widget);
                    textField.TextColor = UIColor.Black;
                    textField.Placeholder = initArg;
                    MakeBottomBorder(widget, (int)rect.Width, (int)rect.Height);

                    textField.ShouldReturn = (tf) => {
                        tf.ResignFirstResponder();
                        return true;
                    };
                    textField.TouchUpOutside += delegate
                    {
                        textField.ResignFirstResponder();
                    };
                    /*var parentView = GetParentView();
                    var g = new UITapGestureRecognizer(() => parentView.EndEditing(true));
                    parentView.AddGestureRecognizer(g);*/
                    break;
                case "TextEditView":
                    type = UIVariable.UIType.TEXT_VIEW;
                    widget = new UITextView(rect);
                    ((UITextView)widget).TextColor = UIColor.Black;
                    AddBorderFunction.AddBorder(widget);
                    break;
                case "TextView":
                    type = UIVariable.UIType.TEXT_VIEW;
                    widget = new UITextView(rect);
                    ((UITextView)widget).TextColor = UIColor.Black;
                    ((UITextView)widget).Editable = false;
                    AddBorderFunction.AddBorder(widget);
                    break;
                case "ImageView":
                    type = UIVariable.UIType.IMAGE_VIEW;
                    widget = new UIImageView(rect);
                    if (!string.IsNullOrWhiteSpace(initArg))
                    {
                        UIImage img = UtilsiOS.LoadImage(initArg);
                        ((UIImageView)widget).Image = img;
                    }
                    break;
                case "Combobox":
                    type = UIVariable.UIType.COMBOBOX;
                    widgetFunc = new iOSVariable(type, widgetName, widget);
                    widgetFunc.CreateCombobox(rect, initArg);
                    break;
                case "Indicator":
                    type = UIVariable.UIType.INDICATOR;
                    var ind = new UIActivityIndicatorView(rect);
                    ind.HidesWhenStopped = true;
                    ind.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray;
                    ind.StartAnimating();
                    widget = ind;
                    break;
                case "TypePicker":
                    type = UIVariable.UIType.PICKER_VIEW;
                    widget = new UIPickerView(rect);
                    ((UIPickerView)widget).AutosizesSubviews = true;
                    break;
                case "Picker":
                    type = UIVariable.UIType.PICKER_IMAGES;
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
                    widgetFunc = new iOSVariable(type, widgetName, widget);
                    widgetFunc.CreateStepper(rect, initArg);
                    break;
                case "SegmentedControl":
                    type = UIVariable.UIType.SEGMENTED;
                    widget = new UISegmentedControl(rect);
                    break;
                    //default:
                    //  type = UIVariable.UIType.VIEW;
                    //  widget = new UIView(rect);
                    //  break;
            }
            SetOptionsFunction.SetMultiline(widget);

            if (widgetFunc == null)
            {
                widgetFunc = new iOSVariable(type, widgetName, widget);
            }
            //iOSVariable widgetFunc = new iOSVariable(type, widgetName, widget);
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
            if (string.IsNullOrWhiteSpace(valueStr))
            {
                return;
            }
            widgetFunc.InitValue = new Variable(valueStr);

            // currValue:minValue:maxValue:step

            double minValue = 0, maxValue = 1, currValue = 0, step = 1.0;
            string[] vals = valueStr.Split(new char[] { ',', ':' });
            Double.TryParse(vals[0].Trim(), out currValue);

            if (vals.Length > 1)
            {
                Double.TryParse(vals[1].Trim(), out minValue);
                if (vals.Length > 2)
                {
                    Double.TryParse(vals[2].Trim(), out maxValue);
                }
                if (vals.Length > 3)
                {
                    Double.TryParse(vals[3].Trim(), out step);
                }
            }
            else
            {
                minValue = maxValue = currValue;
            }

            if (widgetFunc.ViewX is UISwitch)
            {
                ((UISwitch)widgetFunc.ViewX).On = (int)currValue == 1;
            }
            else if (widgetFunc.ViewX is UISlider)
            {
                ((UISlider)widgetFunc.ViewX).MinValue = (float)minValue;
                ((UISlider)widgetFunc.ViewX).MaxValue = (float)maxValue;
                ((UISlider)widgetFunc.ViewX).Value = (float)currValue;
            }
            else if (widgetFunc.ViewX is UISegmentedControl)
            {
                UISegmentedControl seg = widgetFunc.ViewX as UISegmentedControl;
                for (int i = 0; i < vals.Length; i++)
                {
                    seg.InsertSegment(vals[i], i, false);
                }
                seg.SelectedSegment = 0;
            }
            else
            {
                widgetFunc.CurrVal = currValue;
                widgetFunc.MinVal = minValue;
                widgetFunc.MaxVal = maxValue;
                widgetFunc.Step = step;
            }
        }
        public virtual bool MakeSecure(bool secure)
        {
            if (m_viewX is UITextField)
            {
                ((UITextField)m_viewX).SecureTextEntry = secure;
                return true;
            }
            return false;
        }
        public virtual double GetValue()
        {
            double result = 0;
            if (m_viewX is UISwitch)
            {
                result = ((UISwitch)m_viewX).On ? 1 : 0;
            }
            else if (m_viewX is UISlider)
            {
                result = ((UISlider)m_viewX).Value;
            }
            else if (m_viewX is UIStepper)
            {
                result = ((UIStepper)m_viewX).Value;
            }
            else if (m_viewX is UIPickerView)
            {
                result = ((TypePickerViewModel)(((UIPickerView)m_viewX).Model)).SelectedRow;
            }
            else if (m_viewX is UISegmentedControl)
            {
                result = ((UISegmentedControl)m_viewX).SelectedSegment;
            }
            else if (WidgetType == UIType.COMBOBOX)
            {
                TypePickerViewModel model = m_picker.Model as TypePickerViewModel;
                result = model.SelectedRow;
            }
            else
            {
                result = CurrVal;
            }
            return result;
        }
        public virtual bool AlignText(string alignment)
        {
            if (string.IsNullOrEmpty(alignment))
            {
                return false;
            }
            alignment = alignment.ToLower();

            Tuple<UIControlContentHorizontalAlignment, UITextAlignment> al = UtilsiOS.GetAlignment(alignment);

            if (WidgetType == UIType.COMBOBOX)
            {
                SetComboboxAlignment(al);
            }
            else if (ViewX is UIButton)
            {
                ((UIButton)ViewX).HorizontalAlignment = al.Item1;
            }
            else if (ViewX is UILabel)
            {
                ((UILabel)ViewX).TextAlignment = al.Item2;
            }
            else if (ViewX is UITextField)
            {
                ((UITextField)ViewX).TextAlignment = al.Item2;
            }
            else if (ViewX is UITextView)
            {
                ((UITextView)ViewX).TextAlignment = al.Item2;
            }
            else if (ViewX is UIPickerView)
            {
                UIPickerView picker = ViewX as UIPickerView;
                TypePickerViewModel model = picker.Model as TypePickerViewModel;
                if (model != null)
                {
                    model.Alignment = al.Item2;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public virtual bool Enable(bool enable)
        {
            if (WidgetType == UIType.COMBOBOX)
            {
                ViewX.UserInteractionEnabled = enable;
            }
            else if (ViewX is UIButton)
            {
                ((UIButton)ViewX).Enabled = enable;
            }
            else if (ViewX is UILabel)
            {
                ((UILabel)ViewX).Enabled = enable;
            }
            else if (ViewX is UITextField)
            {
                ((UITextField)ViewX).Enabled = enable;
            }
            else if (ViewX is UITextView)
            {
                ((UITextView)ViewX).UserInteractionEnabled = enable;
            }
            else if (ViewX is UIPickerView)
            {
                UIPickerView picker = ViewX as UIPickerView;
                picker.UserInteractionEnabled = enable;
            }
            else
            {
                return false;
            }
            return true;
        }

        public virtual bool SetText(string text, string alignment = null, bool tiggered = false)
        {
            m_originalText = text;

            m_alignment = string.IsNullOrEmpty(alignment) ? m_alignment : alignment;
            // Add extra space for buttons, otherwise they don't look good.
            if (m_viewX is UIButton)
            {
                if (m_alignment == "left")
                {
                    text = " " + text;
                }
                else if (m_alignment == "right")
                {
                    text = text + " ";
                }
            }
            AlignText(alignment);

            if (WidgetType == UIType.COMBOBOX && !tiggered)
            {
                SetComboboxText(text);
            }
            else if (ViewX is UIButton)
            {
                ((UIButton)ViewX).SetTitle(text, UIControlState.Normal);
            }
            else if (ViewX is UILabel)
            {
                ((UILabel)ViewX).Text = text;
            }
            else if (ViewX is UITextField)
            {
                ((UITextField)ViewX).Text = text;
            }
            else if (ViewX is UITextView)
            {
                ((UITextView)ViewX).Text = text;
            }
            else if (ViewX is UIPickerView && !tiggered)
            {
                UIPickerView picker = ViewX as UIPickerView;
                TypePickerViewModel model = picker.Model as TypePickerViewModel;
                int row = model.StringToRow(text);
                picker.Select((int)row, 0, true);
                model.Selected(picker, (int)row, 0);
            }
            else
            {
                return false;
            }

            return true;
        }

        public virtual string GetText()
        {
            if (m_viewX is UIButton)
            {
                return m_originalText != null ? m_originalText :
                  ((UIButton)m_viewX).Title(UIControlState.Normal);
            }
            string result = "";
            if (m_viewX is UIButton)
            {
                result = ((UIButton)m_viewX).Title(UIControlState.Normal);
            }
            else if (m_viewX is UILabel)
            {
                result = ((UILabel)m_viewX).Text;
            }
            else if (m_viewX is UITextField)
            {
                result = ((UITextField)m_viewX).Text;
            }
            else if (m_viewX is UITextView)
            {
                result = ((UITextView)m_viewX).Text;
            }
            else if (m_viewX is UIPickerView)
            {
                UIPickerView pickerView = m_viewX as UIPickerView;
                TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
                int row = (int)GetValueFunction.GetValue(this);
                result = model.RowToString(row);
            }
            return result;
        }

        public virtual void AddText(string text, string colorStr, double alpha = 1.0)
        {
            if (!(m_viewX is UITextView))
            {
                return;
            }
            var textView = m_viewX as UITextView;
            NSMutableAttributedString attrText = new NSMutableAttributedString(textView.AttributedText);

            var atts = new UIStringAttributes();
            atts.ForegroundColor = UtilsiOS.String2Color(colorStr, alpha);
            var attrTextAdd = new NSAttributedString(text, atts);
            //attributedString.BeginEditing();
            //attributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, atts.ForegroundColor, new NSRange(0, 10));
            //attributedString.AddAttribute(UIStringAttributeKey.Font, UIFont.GetPreferredFontForTextStyle(UIFontTextStyle.Headline), new NSRange(0, 10));
            //attributedString.EndEditing();

            if (attrText.Length > 0)
            {
                attrText.Append(new NSAttributedString("\n"));

            }
            attrText.Append(attrTextAdd);
            textView.AttributedText = attrText;
        }

        public CGSize GetParentSize()
        {
            if (ParentView != null)
            {
                return new CGSize(ParentView.Width, ParentView.Height);
            }
            return UtilsiOS.GetScreenSize();
        }
        public UIView GetParentView()
        {
            iOSVariable parent = ParentView as iOSVariable;
            if (parent != null)
            {
                return parent.ViewX;
            }
            return AppDelegate.GetCurrentView();
        }

        /*public void CreateComplexView(CGRect rect, string extras, string config)
        {
          if (WidgetType == UIType.STEPPER) {
            if (ViewX != null && ViewX.Subviews.Length > 1) {
              // Already created: just move it.
              ViewX.Frame = rect;
              return;
            }
            CreateStepper(rect, extras);
          } else if (WidgetType == UIType.COMBOBOX) {
            if (ViewX != null) {
              // Already created: just move it.
              ViewX.Frame = rect;
              return;
            }
            CreateCombobox(rect, config);
          }
          m_allComplex.Add(this);
        }*/

        static void ResetCombos()
        {
            for (int i = 0; i < m_allComplex.Count; i++)
            {
                iOSVariable complex = m_allComplex[i];
                if (complex.m_picker != null)
                {
                    complex.m_picker.Hidden = true;
                }
                if (complex.m_button2 != null)
                {
                    complex.m_button2.Hidden = true;
                }
            }
        }
        public void ShowHide(bool show)
        {
            /*if (show) {
              int i = 0;
            } else {
              int j = 0;
            }*/
            if (m_viewX != null)
            {
                m_viewX.Hidden = !show;
            }
            if (m_viewY != null)
            {
                m_viewY.Hidden = !show;
            }
            if (m_button != null)
            {
                m_button.Hidden = !show;
            }
            if (m_picker != null)
            {
                m_picker.Hidden = true;
            }
            if (m_button2 != null)
            {
                m_button2.Hidden = true;
            }
        }

        public virtual void AddData(List<string> data, string varName, string title, string extra)
        {
            if (ViewX is UIPickerView)
            {
                UIPickerView pickerView = ViewX as UIPickerView;

                TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
                if (model == null)
                {
                    model = new TypePickerViewModel(AppDelegate.GetCurrentController());
                }
                model.Data = data;

                if (!string.IsNullOrWhiteSpace(title))
                {
                    model.RowSelected += (row) =>
                    {
                        UIVariable.GetAction(title, varName, row.ToString());
                    };
                }
                if (!string.IsNullOrWhiteSpace(extra))
                {
                    var al = UtilsiOS.GetAlignment(extra);
                    model.Alignment = al.Item2;
                }

                model.SetSize((int)pickerView.Bounds.Width, (int)pickerView.Bounds.Height / 4);
                pickerView.Model = model;
            }
            else if (ViewX is UITableView)
            {
                UITableView tableView = ViewX as UITableView;

                TableViewSource source = tableView.Source as TableViewSource;
                if (source == null)
                {
                    source = new TableViewSource();
                }
                source.Data = data;
                tableView.Source = source;
                tableView.ReloadData();
            }
            else if (m_picker != null)
            {
                TypePickerViewModel model = m_picker.Model as TypePickerViewModel;
                model.Data = data;

                if (!string.IsNullOrEmpty(extra))
                {
                    Tuple<UIControlContentHorizontalAlignment, UITextAlignment> al =
                      UtilsiOS.GetAlignment(extra);
                    model.Alignment = al.Item2;
                }
                m_picker.Model = model;

                SetText(data[0], extra, true /* triggered */);
            }
        }
        public virtual void AddImages(List<UIImage> images, string varName, string title)
        {
            if (ViewX is UIPickerView)
            {
                UIPickerView pickerView = ViewX as UIPickerView;

                TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
                if (model == null)
                {
                    model = new TypePickerViewModel(AppDelegate.GetCurrentController());
                }
                model.Images = images;

                if (!string.IsNullOrWhiteSpace(title))
                {
                    AddAction(varName, title);
                }
                model.SetSize((int)pickerView.Bounds.Width, (int)pickerView.Bounds.Height / 4);
                pickerView.Model = model;
            }
            else if (ViewX is UITableView)
            {
                UITableView tableView = ViewX as UITableView;

                TableViewSource source = tableView.Source as TableViewSource;
                if (source == null)
                {
                    source = new TableViewSource();
                }
                source.Images = images;
                tableView.Source = source;
            }
        }
        public void CreateCombobox(CGRect rect, string argument)
        {
            int currentRow = 0;
            if (m_picker != null && m_picker.Model is TypePickerViewModel)
            {
                var currModel = m_picker.Model as TypePickerViewModel;
                string text = GetText();
                currentRow = currModel.StringToRow(text);
            }

            UIView parent = GetParentView();
            WidgetType = UIVariable.UIType.COMBOBOX;

            UIView mainView = AppDelegate.GetCurrentView();
            int mainHeight = (int)mainView.Frame.Size.Height;
            int mainWidth = (int)mainView.Frame.Size.Width;

            int pickerHeight = Math.Min(mainHeight / 4, 320);
            int pickerWidth = Math.Min(mainWidth, 640);
            int pickerY = mainHeight - pickerHeight;// - 6;

            m_picker = new UIPickerView();
            m_button2 = new UIButton();
            m_buttonCancel = new UIButton();

            m_extraView1 = new UIView();
            m_extraView1.BackgroundColor = UIColor.Cyan;
            m_extraView1.Hidden = true;

            m_extraView2 = new UIView();
            m_extraView2.BackgroundColor = UIColor.FromRGB(100, 100, 100);
            m_extraView2.Hidden = true;

            m_extraView1.Frame   = new CGRect(0, pickerY, pickerWidth, pickerHeight);
            m_picker.Frame       = new CGRect(0, 0, pickerWidth, pickerHeight);
            m_extraView2.Frame   = new CGRect(0, pickerY - 40, pickerWidth, 40);
            m_button2.Frame      = new CGRect(pickerWidth - 140, 0, 140, 40);
            m_buttonCancel.Frame = new CGRect(0, 0, 140, 40);

            string alignment = "", color1 = "", color2 = "", closeLabel = "", mode = "view";
            Utils.Extract(argument, ref alignment, ref color1, ref color2, ref closeLabel, ref mode);
            m_alignment = alignment;
            Tuple<UIControlContentHorizontalAlignment, UITextAlignment> al =
              UtilsiOS.GetAlignment(alignment);

            m_viewY = new UIView();
            m_viewY.Frame = new CGRect(0, 0, mainWidth, mainHeight);

            TypePickerViewModel model = new TypePickerViewModel(AppDelegate.GetCurrentController());
            m_picker.ShowSelectionIndicator = true;
            m_picker.Model = model;

            if (!string.IsNullOrEmpty(color1))
            {
                m_viewY.BackgroundColor = UtilsiOS.String2Color(color1);
                if (string.IsNullOrEmpty(color2))
                {
                    color2 = color1;
                }
                m_picker.BackgroundColor = UtilsiOS.String2Color(color2);
            }

            m_button = new UIButton();
            m_button.Frame = rect;
            //m_button.SetTitle(extraLabel, UIControlState.Normal);
            m_button.BackgroundColor = UIColor.Clear;
            m_button.SetTitleColor(UIColor.Black, UIControlState.Normal);
            m_button.Hidden = false;
            m_button.Layer.BorderWidth = 1;
            m_button.Layer.CornerRadius = 4;
            m_button.Layer.BorderColor = UIColor.LightGray.CGColor;
            UIImage img = UtilsiOS.CreateComboboxImage(rect);
            m_button.SetBackgroundImage(img, UIControlState.Normal);
            m_button.ImageView.ClipsToBounds = true;
            m_button.ContentMode = UIViewContentMode.Right;
            m_button.HorizontalAlignment = al.Item1;
            m_button.TouchUpInside += (sender, e) =>
            {
                ResetCombos();
                m_extraView1.Hidden   = false;
                m_extraView2.Hidden   = false;
                m_button2.Hidden      = false;
                m_buttonCancel.Hidden = false;
                m_picker.Hidden       = false;

                model = m_picker.Model as TypePickerViewModel;

                string text = GetText();
                int row = model.StringToRow(text);
                model.Selected(m_picker, row, 0);
                m_picker.Model = model;

                mainView.BecomeFirstResponder();
                mainView.AddSubview(m_viewY);
            };

            if (string.IsNullOrEmpty(closeLabel))
            {
                closeLabel = "Done";
            }
            m_button2.SetTitle(closeLabel + "\t", UIControlState.Normal);
            m_button2.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
            m_button2.BackgroundColor = UIColor.Clear; //FromRGB(100, 100, 100);
            m_button2.SetTitleColor(UIColor.White, UIControlState.Normal);
            m_button2.TouchUpInside += (sender, e) =>
            {
                m_extraView1.Hidden = true;
                m_extraView2.Hidden = true;

                string text = model.SelectedText;
                SetText(text, alignment, true /* triggered */);
                ActionDelegate?.Invoke(WidgetName, text);

                m_viewY.RemoveFromSuperview();
                mainView.BecomeFirstResponder();
            };

            m_buttonCancel.SetTitle("   X", UIControlState.Normal);
            m_buttonCancel.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
            m_buttonCancel.BackgroundColor = UIColor.Clear;// .FromRGB(100, 100, 100);
            m_buttonCancel.SetTitleColor(UIColor.White, UIControlState.Normal);
            m_buttonCancel.TouchUpInside += (sender, e) =>
            {
                m_extraView1.Hidden = true;
                m_extraView2.Hidden = true;

                model.Selected(m_picker, currentRow, 0);
                m_picker.Model = model;
                m_viewY.RemoveFromSuperview();
                mainView.BecomeFirstResponder();
            };

            m_viewX = m_button;
            mainView.AddSubview(m_viewX);

            m_viewY.AddSubview(m_extraView2);
            m_viewY.AddSubview(m_extraView1);

            m_extraView1.AddSubview(m_picker);
            m_extraView2.AddSubview(m_buttonCancel);
            m_extraView2.AddSubview(m_button2);
            //m_viewY.AddSubview(m_picker);
            //m_viewY.AddSubview(m_buttonCancel);
            //m_viewY.AddSubview(m_button2);

            m_viewX.Tag = ++m_currentTag;
        }

        public void SetComboboxText(string text, int row = -1)
        {
            if (m_picker == null || m_picker.Model == null)
            {
                return;
            }
            TypePickerViewModel model = m_picker.Model as TypePickerViewModel;
            if (row < 0)
            {
                row = model.StringToRow(text);
            }
            m_picker.Select((int)row, 0, true);
            //model?.Selected(m_picker, (int)row, 0);
            ActionDelegate?.Invoke(WidgetName, text);

            m_button?.SetTitle(text, UIControlState.Normal);
        }

        public void SetComboboxAlignment(Tuple<UIControlContentHorizontalAlignment, UITextAlignment> al)
        {
            if (m_picker == null || m_picker.Model == null)
            {
                return;
            }
            TypePickerViewModel model = m_picker.Model as TypePickerViewModel;
            model.Alignment = al.Item2;

            if (ViewX is UIButton)
            {
                ((UIButton)ViewX).HorizontalAlignment = al.Item1;
            }
            else if (ViewX is UITextField)
            {
                ((UITextField)ViewX).TextAlignment = al.Item2;
            }
        }

        public void CreateStepper(CGRect rect, string extraLabel)
        {
            UIView parent = GetParentView();
            WidgetType = UIVariable.UIType.STEPPER;
            UILabel label = null;

            int stepperX = (int)(10 * UtilsiOS.WidthMultiplier());
            int stepperY = (int)(4 * UtilsiOS.WidthMultiplier());
            int labelSize = (int)rect.Height;

            CGRect stepRect = new CGRect(stepperX, stepperY, rect.Width, rect.Height);
            UIStepper stepper = new UIStepper(stepRect);

            m_viewX = new UIView(rect);

            if (!string.IsNullOrWhiteSpace(extraLabel))
            {
                extraLabel = extraLabel.ToLower();
                nfloat labelWidth = rect.Width - stepper.Bounds.Width;
                nfloat labelHeight = stepper.Bounds.Height;
                if (extraLabel.EndsWith("left"))
                {
                    stepperX = 0;
                    CGRect labelRect = new CGRect(stepperX, stepperY, labelWidth, labelHeight);
                    label = new UILabel(labelRect);
                    label.TextAlignment = UITextAlignment.Left;
                    stepRect = new CGRect(stepperX + labelWidth, stepperY, stepper.Bounds.Width, stepper.Bounds.Height);
                    stepper = new UIStepper(stepRect);
                }
                else
                {
                    CGRect labelRect = new CGRect(stepperX + stepper.Bounds.Width, stepperY, labelWidth, labelHeight);
                    label = new UILabel(labelRect);
                    label.TextAlignment = UITextAlignment.Right;
                }
                label.Text = CurrVal.ToString();
                label.TextAlignment = UITextAlignment.Center;
                //label.SizeToFit();
                m_viewX.AddSubview(label);
            }
            // "5:3:50:1"
            double curr = 0, min = 0, max = 0, step = 0;
            Utils.Extract(extraLabel, ref curr, ref min, ref max, ref step);
            CurrVal = curr;
            MinVal = min;
            MaxVal = max;
            Step = step;

            stepper.MinimumValue = (float)MinVal;
            stepper.MaximumValue = (float)MaxVal;
            stepper.Value = (float)CurrVal;
            stepper.StepValue = (float)Step;

            m_viewX.AddSubview(stepper);
            //m_viewY = stepper;

            stepper.ValueChanged += (sender, e) =>
            {
                CurrVal = stepper.Value;
                if (label != null)
                {
                    label.Text = CurrVal.ToString();
                }
                ActionDelegate?.Invoke(WidgetName, CurrVal.ToString());
            };
        }

        public virtual void AddAction(string varName,
                                     string strAction, string argument = "")
        {
            if (!string.IsNullOrWhiteSpace(argument))
            {
                if (argument.Equals("FINISHED"))
                {
                    if (ViewX is UITextField)
                    {
                        UITextField textField = ViewX as UITextField;
                        textField.EditingDidEnd += (sender, e) =>
                        {
                            UIVariable.GetAction(strAction, varName, textField.Text);
                        };
                    }
                    return;
                }
            }
            if (WidgetType == UIVariable.UIType.COMBOBOX)
            {
                ActionDelegate += (arg1, arg2) =>
                {
                    UIVariable.GetAction(strAction, arg1, arg2);
                };
            }
            else if (ViewX is UIButton)
            {
                UIButton button = ViewX as UIButton;
                button.TouchUpInside += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, argument);
                };
            }
            else if (ViewX is UISwitch)
            {
                UISwitch sw = ViewX as UISwitch;
                sw.ValueChanged += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, sw.On.ToString());
                };
            }
            else if (ViewX is UITextField)
            {
                UITextField textField = ViewX as UITextField;
                textField.EditingChanged += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, textField.Text);
                };
            }
            else if (ViewX is UISlider)
            {
                UISlider slider = ViewX as UISlider;
                slider.ValueChanged += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, slider.Value.ToString());
                };
            }
            else if (ViewX is UISegmentedControl)
            {
                UISegmentedControl seg = ViewX as UISegmentedControl;
                seg.ValueChanged += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, seg.SelectedSegment.ToString());
                };
            }
            else if (ViewX is UIPickerView)
            {
                UIPickerView pickerView = ViewX as UIPickerView;
                TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
                if (model == null)
                {
                    model = new TypePickerViewModel(AppDelegate.GetCurrentController());
                }
                model.RowSelected += (row) =>
                {
                    UIVariable.GetAction(strAction, varName, row.ToString());
                };
                pickerView.Model = model;
            }
            else if (ViewX is UITableView)
            {
                UITableView tableView = ViewX as UITableView;
                TableViewSource source = tableView.Source as TableViewSource;
                if (source == null)
                {
                    source = new TableViewSource();
                }
                source.RowSelectedDel += (row) =>
                {
                    UIVariable.GetAction(strAction, varName, row.ToString());
                };
                tableView.Source = source;
            }
            else
            {
                ActionDelegate += (arg1, arg2) =>
                {
                    UIVariable.GetAction(strAction, varName, arg2);
                };
            }
        }
        public UIFont GetFont()
        {
            if (m_viewX is UIButton)
            {
                return ((UIButton)m_viewX).TitleLabel.Font;
            }
            else if (m_viewX is UILabel)
            {
                return ((UILabel)m_viewX).Font;
            }
            else if (m_viewX is UITextField)
            {
                return ((UITextField)m_viewX).Font;
            }
            else if (m_viewX is UITextView)
            {
                return ((UITextView)m_viewX).Font;
            }
            else if (m_viewX is UIPickerView)
            {
                UIPickerView picker = m_viewX as UIPickerView;
                TypePickerViewModel model = picker.Model as TypePickerViewModel;
                return model.GetFont();
            }
            return null;
        }
        public void SetFont(UIFont newFont)
        {
            if (m_viewX is UIButton)
            {
                ((UIButton)m_viewX).TitleLabel.Font = newFont; ;
            }
            else if (m_viewX is UILabel)
            {
                ((UILabel)m_viewX).Font = newFont;
            }
            else if (m_viewX is UITextField)
            {
                ((UITextField)m_viewX).Font = newFont;
            }
            else if (m_viewX is UITextView)
            {
                ((UITextView)m_viewX).Font = newFont;
            }
            else if (m_viewX is UIPickerView)
            {
                UIPickerView picker = m_viewX as UIPickerView;
                TypePickerViewModel model = picker.Model as TypePickerViewModel;
                model?.SetFont(newFont);
            }
        }

        public virtual bool SetFont(string name, double size = 0)
        {
            if (size == 0)
            {
                UIFont oldFont = GetFont();
                if (oldFont == null)
                {
                    return false;
                }
                size = oldFont.PointSize;
            }
            UIFont newFont = UIFont.FromName(name, (nfloat)size);
            if (newFont == null)
            {
                return false;
            }
            SetFont(newFont);

            return true;
        }
        public virtual bool SetFontSize(double size)
        {
            UIFont newFont = UIFont.SystemFontOfSize((nfloat)size);
            SetFont(newFont);
            return true;
        }
        bool SetBoldItalicNormal(double size = 0)
        {
            UIFont oldFont = GetFont();
            if (oldFont == null)
            {
                return false;
            }
            if (size == 0)
            {
                size = oldFont.PointSize;
            }
            if (!m_bold && !m_italic)
            {
                SetFont(UIFont.SystemFontOfSize((nfloat)size));
                return true;
            }
            var traits = m_bold && m_italic ?
              UIFontDescriptorSymbolicTraits.Bold | UIFontDescriptorSymbolicTraits.Italic :
                   m_bold ? UIFontDescriptorSymbolicTraits.Bold :
                            UIFontDescriptorSymbolicTraits.Italic;

            var descriptor = oldFont.FontDescriptor.CreateWithTraits(traits);
            UIFont newFont = UIFont.FromDescriptor(descriptor, (nfloat)size);
            SetFont(newFont);
            return true;
        }
        public virtual bool SetNormalFont(double size = 0)
        {
            m_bold = m_italic = false;
            return SetBoldItalicNormal(size);
        }
        public virtual bool SetBold(double size = 0)
        {
            m_bold = true;
            return SetBoldItalicNormal(size);
        }
        public virtual bool SetItalic(double size = 0)
        {
            m_italic = true;
            return SetBoldItalicNormal(size);
        }

        public virtual bool SetBackgroundColor(string colorStr, double alpha = 1.0)
        {
            if (ViewX == null)
            {
                return false;
            }
            if (colorStr.Equals("transparent", StringComparison.OrdinalIgnoreCase))
            {
                ViewX.Opaque = true;
                alpha = 0.0;
            }
            else
            {
                ViewX.Opaque = false;
            }

            var color = UtilsiOS.String2Color(colorStr, alpha);
            ViewX.BackgroundColor = color;

            return true;
        }
        public virtual bool SetFontColor(string colorStr)
        {
            if (ViewX == null)
            {
                return false;
            }

            UIColor color = UtilsiOS.String2Color(colorStr);

            if (ViewX is UIButton)
            {
                UIButton button = (UIButton)ViewX;
                button.SetTitleColor(color, UIControlState.Normal);
            }
            else if (ViewX is UILabel)
            {
                ((UILabel)ViewX).TextColor = color;
            }
            else if (ViewX is UITextField)
            {
                ((UITextField)ViewX).TextColor = color;
            }
            else if (ViewX is UITextView)
            {
                ((UITextView)ViewX).TextColor = color;
            }
            else
            {
                return false;
            }
            return true;
        }
        public virtual bool SetValue(string arg1, string arg2 = "")
        {
            double val = Utils.ConvertToDouble(arg1);
            CurrVal = val;
            if (m_viewY is UIStepper)
            {
                UIStepper stepper = m_viewY as UIStepper;
                stepper.Value = val;
                UIView[] subviews = m_viewX.Subviews;
                foreach (UIView view in subviews)
                {
                    if (view is UILabel)
                    {
                        UILabel label = view as UILabel;
                        label.Text = CurrVal.ToString();
                    }
                }
            }
            else if (WidgetType == UIType.COMBOBOX)
            {
                TypePickerViewModel model = m_picker.Model as TypePickerViewModel;
                if (model == null)
                {
                    return false;
                }

                switch (arg1)
                {
                    case "alignment":
                        Tuple<UIControlContentHorizontalAlignment, UITextAlignment> al =
                          UtilsiOS.GetAlignment(arg2);
                        m_button.HorizontalAlignment = al.Item1;
                        break;
                    case "backgroundcolorpicker":
                        m_picker.BackgroundColor = UtilsiOS.String2Color(arg2);
                        break;
                    case "backgroundcolorbutton2":
                        m_button2.BackgroundColor = UtilsiOS.String2Color(arg2);
                        break;
                    case "fontcolor":
                        var color = UtilsiOS.String2Color(arg2);
                        model.TextColor = color;
                        break;
                    case "fontcolor2":
                        m_button2.SetTitleColor(UtilsiOS.String2Color(arg2), UIControlState.Normal);
                        break;
                    case "text2":
                        m_button2.SetTitle(arg2 + "\t", UIControlState.Normal);
                        break;
                    default:
                        double row;
                        if (!Utils.CanConvertToDouble(arg1, out row) || row < 0)
                        {
                            row = model.StringToRow(arg1, -1);
                        }
                        if (row < 0)
                        {
                            return false;
                        }
                        model.SetColor((int)row, UtilsiOS.String2Color(arg2));
                        break;
                }
                if (string.IsNullOrEmpty(arg1) || arg1 == "value")
                {
                    SetComboboxText("", (int)Utils.ConvertToDouble(arg2));
                }
            }
            else if (m_viewX is UISwitch)
            {
                ((UISwitch)m_viewX).On = (int)val == 1;
            }
            else if (m_viewX is UISlider)
            {
                ((UISlider)m_viewX).Value = (float)val;
            }
            else if (m_viewX is UIStepper)
            {
                ((UIStepper)m_viewX).Value = (float)val;
            }
            else if (m_viewX is UIPickerView)
            {
                UIPickerView picker = m_viewX as UIPickerView;
                picker.Select((int)val, 0, true);
                TypePickerViewModel model = picker.Model as TypePickerViewModel;
                model?.Selected(picker, (int)val, 0);
            }
            else if (m_viewX is UISegmentedControl)
            {
                ((UISegmentedControl)m_viewX).SelectedSegment = (nint)val;
            }
            else
            {
                return false;
            }
            return true;
        }

        UIView m_viewX;
        UIView m_viewY;
        UIView m_extraView1;
        UIView m_extraView2;

        string m_originalText;
        string m_alignment;

        UIPickerView m_picker;
        UIButton m_button;
        UIButton m_buttonCancel;
        UIButton m_button2;
        UILabel m_label;

        bool m_bold;
        bool m_italic;

        static List<iOSVariable> m_allComplex = new List<iOSVariable>();

        public static UIView GetView(string viewName, ParsingScript script)
        {
            if (viewName.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            ParserFunction func = ParserFunction.GetVariable(viewName, script);
            Utils.CheckNotNull(viewName, func, script);
            Variable viewValue = func.GetValue(script);
            iOSVariable viewVar = viewValue as iOSVariable;
            return viewVar.ViewX;
        }
    }
}
