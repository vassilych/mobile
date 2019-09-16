using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
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
            if (type != UIType.LOCATION && m_viewX != null)
            {
                m_viewX.Tag = ++m_currentTag;
                m_viewX.Id = m_currentTag;
            }
        }
        public DroidVariable(UIType type, string name,
                             UIVariable refViewX, UIVariable refViewY = null) :
                             base(type, name, refViewX, refViewY)
        {
        }

        public override Variable Clone()
        {
            DroidVariable newVar = (DroidVariable)this.MemberwiseClone();
            return newVar;
        }

        public View ViewX
        {
            get { return m_viewX; }
            set { m_viewX = value; }
        }
        public View ViewY
        {
            get { return m_viewY; }
            set { m_viewY = value; }
        }
        public LayoutRules LayoutRuleX
        {
            get { return m_layoutRuleX; }
            set { m_layoutRuleX = value; }
        }
        public LayoutRules LayoutRuleY
        {
            get { return m_layoutRuleY; }
            set { m_layoutRuleY = value; }
        }
        public ViewGroup ViewLayout
        {
            get { return m_viewX as ViewGroup; }
        }
        public ViewGroup.LayoutParams LayoutParams { get; set; }

        public int ExtraX { get; set; }
        public int ExtraY { get; set; }
        public bool IsAdjustedX { get; set; }
        public bool IsAdjustedY { get; set; }
        public bool KeyboardVisible { get; set; }

        public string FontName { get; set; }
        bool m_bold;
        bool m_italic;

        public void SetViewLayout(int width, int height)
        {
            DroidVariable refView = Location?.RefViewX as DroidVariable;
            m_viewX = MainActivity.CreateViewLayout(width, height, refView?.ViewLayout);
        }

        public virtual DroidVariable GetWidget(string widgetType, string widgetName, string initArg,
                                               int width, int height)
        {
            UIVariable.UIType type = UIVariable.UIType.NONE;
            View widget = null;
            switch (widgetType)
            {
                case "View":
                    type = UIVariable.UIType.VIEW;
                    widget = new View(MainActivity.TheView);
                    break;
                case "Button":
                    type = UIVariable.UIType.BUTTON;
                    widget = new Button(MainActivity.TheView);
                    ((Button)widget).SetTextColor(Color.Black);
                    ((Button)widget).Text = initArg;
                    UtilsDroid.AddViewBorder(widget, Color.Black);
                    break;
                case "TextView":
                    type = UIVariable.UIType.TEXT_VIEW;
                    widget = new TextView(MainActivity.TheView);
                    ((TextView)widget).SetTextColor(Color.Black);
                    ((TextView)widget).Text = initArg;
                    ((TextView)widget).Gravity = GravityFlags.Top | GravityFlags.Left;
                    ((TextView)widget).TextAlignment = TextAlignment.TextStart;
                    ((TextView)widget).MovementMethod = new ScrollingMovementMethod();
                    ((TextView)widget).VerticalScrollBarEnabled = true;
                    ((TextView)widget).HorizontalScrollBarEnabled = true;
                    //((TextView)widget).SetMaxLines(40);
                    //((TextView)widget).ScrollBarStyle = ScrollbarStyles.OutsideOverlay;
                    //((TextView)widget).ScrollBarSize = 2;
                    break;
                case "Label":
                    type = UIVariable.UIType.LABEL;
                    widget = new TextView(MainActivity.TheView);
                    ((TextView)widget).SetTextColor(Color.Black);
                    ((TextView)widget).Text = initArg;
                    ((TextView)widget).Gravity = GravityFlags.CenterVertical | GravityFlags.Left;
                    break;
                case "TextEdit":
                    type = UIVariable.UIType.TEXT_FIELD;
                    widget = new EditText(MainActivity.TheView);
                    ((EditText)widget).SetTextColor(Color.Black);
                    ((EditText)widget).Hint = initArg;
                    break;
                case "TextEditView":
                    type = UIVariable.UIType.EDIT_VIEW;
                    widget = new EditText(MainActivity.TheView);
                    ((EditText)widget).SetTextColor(Color.Black);
                    ((EditText)widget).Hint = initArg;
                    ((EditText)widget).Gravity = GravityFlags.Top | GravityFlags.Left;
                    ((EditText)widget).TextAlignment = TextAlignment.TextStart;
                    ((EditText)widget).MovementMethod = new ScrollingMovementMethod();
                    ((EditText)widget).VerticalScrollBarEnabled = true;
                    ((EditText)widget).HorizontalScrollBarEnabled = true;
                    break;
                case "ImageView":
                    type = UIVariable.UIType.IMAGE_VIEW;
                    widget = new ImageView(MainActivity.TheView);
                    if (!string.IsNullOrWhiteSpace(initArg))
                    {
                        int resourceID = MainActivity.String2Pic(initArg);
                        widget.SetBackgroundResource(resourceID);
                    }
                    break;
                case "Combobox":
                    type = UIVariable.UIType.COMBOBOX;
                    widget = new Spinner(MainActivity.TheView);
                    ((Spinner)widget).DescendantFocusability = DescendantFocusability.BlockDescendants;
                    break;
                case "TypePicker":
                    type = UIVariable.UIType.PICKER_VIEW;
                    widget = new NumberPicker(MainActivity.TheView);
                    // Don't show the cursor on the picker:
                    ((NumberPicker)widget).DescendantFocusability = DescendantFocusability.BlockDescendants;
                    break;
                case "Picker":
                    type = UIVariable.UIType.PICKER_IMAGES;
                    widget = new Spinner(MainActivity.TheView);
                    // Don't show the cursor on the picker:
                    ((Spinner)widget).DescendantFocusability = DescendantFocusability.BlockDescendants;
                    break;
                case "ListView":
                    type = UIVariable.UIType.LIST_VIEW;
                    widget = new ListView(MainActivity.TheView);
                    // Don't show the cursor on the list view:
                    ((ListView)widget).DescendantFocusability = DescendantFocusability.BlockDescendants;
                    break;
                case "Switch":
                    type = UIVariable.UIType.SWITCH;
                    widget = new Switch(MainActivity.TheView);
                    break;
                case "SegmentedControl":
                    type = UIVariable.UIType.SEGMENTED;
                    widget = new Switch(MainActivity.TheView);
                    break;
                case "Slider":
                    type = UIVariable.UIType.SLIDER;
                    widget = new SeekBar(MainActivity.TheView);
                    break;
                case "Stepper":
                    type = UIVariable.UIType.STEPPER;
                    widget = new View(MainActivity.TheView);
                    break;
            }

            DroidVariable widgetFunc = new DroidVariable(type, widgetName, widget);
            widgetFunc.AddAction(widgetName, widgetName + "_click");

            SetValues(widgetFunc, initArg);
            return widgetFunc;
        }

        public virtual void AdjustTranslation(DroidVariable location, bool isX, bool sameWidget = true)
        {
            int offset = 0;
            ScreenSize screenSize = UtilsDroid.GetScreenSize();
            if (isX && sameWidget && ViewX is Switch)
            {
                offset = (int)AutoScaleFunction.TransformSize(UtilsDroid.SWITCH_MARGIN, screenSize.Width, 3);
                if (screenSize.Width <= AutoScaleFunction.BASE_WIDTH)
                {
                    offset = UtilsDroid.SWITCH_MARGIN; // from -45, 480
                }
                //offset = -112; // (before -168) // from 1200
                //offset = -135; // from 1440
            }
            location.TranslationX += offset;
        }

        public static void SetValues(DroidVariable widgetFunc, string valueStr)
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

            if (widgetFunc.WidgetType == UIVariable.UIType.SEGMENTED)
            {
                Switch seg = widgetFunc.ViewX as Switch;
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                {
                    seg.ShowText = true;
                }
                seg.TextOn = vals[vals.Length - 1];
                seg.TextOff = vals[0];
                seg.Checked = false;
            }
            else if (widgetFunc.ViewX is Switch)
            {
                Switch sw = widgetFunc.ViewX as Switch;
                sw.Checked = (int)currValue == 1;
            }
            else if (widgetFunc.ViewX is SeekBar)
            {
                SeekBar slider = widgetFunc.ViewX as SeekBar;
                slider.Max = (int)maxValue - (int)minValue;
                slider.Progress = (int)currValue;
                widgetFunc.MinVal = minValue;
                widgetFunc.MaxVal = maxValue;
                widgetFunc.CurrVal = currValue;
            }
            else
            {
                widgetFunc.MinVal = minValue;
                widgetFunc.MaxVal = maxValue;
                widgetFunc.CurrVal = currValue;
                widgetFunc.Step = step;
            }
        }

        public virtual bool AlignText(string alignment)
        {
            if (string.IsNullOrEmpty(alignment))
            {
                return false;
            }
            alignment = alignment.ToLower();

            //m_viewX.TextAlignment = TextAlignment.Gravity;
            var al = UtilsDroid.GetAlignment(alignment);

            if (ViewX is Button)
            {
                ((Button)ViewX).Gravity = al.Item1;
                ((Button)ViewX).TextAlignment = al.Item2;
            }
            else if (ViewX is TextView)
            {
                ((TextView)ViewX).Gravity = al.Item1;
                ((TextView)ViewX).TextAlignment = al.Item2;
            }
            else if (ViewX is EditText)
            {
                ((EditText)ViewX).Gravity = al.Item1;
                ((EditText)ViewX).TextAlignment = al.Item2;
            }
            else
            {
                return false;
            }

            /*var layoutParams = ViewX.LayoutParameters as RelativeLayout.LayoutParams;
            if (alignment == "center") {
              layoutParams.AddRule(LayoutRules.CenterHorizontal);
            }
            ViewX.LayoutParameters = layoutParams;*/

            return true;
        }
        public virtual bool SetText(string text, string alignment = null)
        {
            AlignText(alignment);

            if (ViewX is Button)
            {
                ((Button)ViewX).Text = text;
            }
            else if (ViewX is TextView)
            {
                ((TextView)ViewX).Text = text;
            }
            else if (ViewX is EditText)
            {
                ((EditText)ViewX).Text = text;
            }
            else if (ViewX is Spinner)
            {
                Spinner spinner = ViewX as Spinner;
                TextImageAdapter adapter = spinner.Adapter as TextImageAdapter;
                if (adapter != null)
                {
                    int pos = adapter.Text2Position(text);
                    spinner.SetSelection(pos);
                }
            }
            else if (ViewX is NumberPicker)
            {
                NumberPicker picker = ViewX as NumberPicker;
                string[] all = picker.GetDisplayedValues();
                List<string> names = new List<string>(all);
                int row = names.FindIndex((obj) => obj.Equals(text));
                picker.Value = (int)row;
                ExecuteAction(row.ToString());
            }
            else
            {
                return false;
            }

            return true;
        }
        public virtual string GetText()
        {
            string text = "";
            if (ViewX is Button)
            {
                text = ((Button)ViewX).Text;
            }
            else if (ViewX is TextView)
            {
                text = ((TextView)ViewX).Text;
            }
            else if (ViewX is EditText)
            {
                text = ((EditText)ViewX).Text;
            }
            else if (ViewX is Spinner)
            {
                Spinner spinner = ViewX as Spinner;
                TextImageAdapter adapter = spinner.Adapter as TextImageAdapter;
                if (adapter != null)
                {
                    int pos = spinner.SelectedItemPosition;
                    text = adapter.Position2Text(pos);
                }
            }
            else if (ViewX is NumberPicker)
            {
                NumberPicker picker = ViewX as NumberPicker;
                string[] all = picker.GetDisplayedValues();
                int row = picker.Value;
                if (all.Length > row && row >= 0)
                {
                    text = all[row];
                }
            }
            return text;
        }

        public virtual void AddText(string text, string colorStr, double alpha = 1.0)
        {
            var color = UtilsDroid.String2Color(colorStr);
            SpannableString newText = new SpannableString(text);
            newText.SetSpan(new ForegroundColorSpan(color), 0, text.Length,
                            SpanTypes.InclusiveExclusive);
            if (m_viewX is EditText)
            {
                var editTextView = m_viewX as EditText;
                if (editTextView.Text.Length > 0)
                {
                    editTextView.Append("\n");
                }
                editTextView.Append(newText);
            }
            else if (m_viewX is TextView)
            {
                var textView = m_viewX as TextView;
                if (textView.Text.Length > 0)
                {
                    textView.Append("\n");
                }
                textView.Append(newText);
            }
        }

        public virtual bool MakeSecure(bool secure)
        {
            if (ViewX is TextView)
            {
                var textView = ViewX as TextView;
                textView.InputType = secure ? InputTypes.ClassText | InputTypes.TextVariationPassword :
                                              InputTypes.ClassText | InputTypes.TextVariationNormal;
                return true;
            }
            return false;
        }

        public virtual bool SetValue(string arg1, string arg2 = "")
        {
            double val = Utils.ConvertToDouble(arg1);
            if (ViewX is Switch)
            {
                ((Switch)ViewX).Checked = (int)val == 1;
            }
            else if (ViewX is SeekBar)
            {
                ((SeekBar)ViewX).Progress = (int)val;
            }
            else if (WidgetType == UIVariable.UIType.STEPPER)
            {
                CurrVal = val;
            }
            else if (ViewX is NumberPicker)
            {
                NumberPicker picker = ViewX as NumberPicker;
                picker.Value = (int)val;
                ExecuteAction(val.ToString());
            }
            else if (WidgetType == UIVariable.UIType.SEGMENTED)
            {
                Switch sw = ((Switch)ViewX);
                sw.Checked = val == 0;
            }
            else if (ViewX is Spinner)
            {
                Spinner spinner = ((Spinner)ViewX);
                spinner.SetSelection((int)val);
            }
            else
            {
                return false;
            }
            return true;
        }
        public virtual double GetValue()
        {
            double result = 0;
            if (ViewX is Switch)
            {
                result = ((Switch)ViewX).Checked ? 1 : 0;
            }
            else if (ViewX is SeekBar)
            {
                result = ((SeekBar)ViewX).Progress;
            }
            else if (WidgetType == UIVariable.UIType.STEPPER)
            {
                result = CurrVal;
            }
            else if (ViewX is NumberPicker)
            {
                result = ((NumberPicker)ViewX).Value;
            }
            else if (WidgetType == UIVariable.UIType.SEGMENTED)
            {
                Switch sw = ((Switch)ViewX);
                result = sw.Checked ? 0 : 1;
            }
            else if (ViewX is Spinner)
            {
                Spinner spinner = ((Spinner)ViewX);
                result = spinner.SelectedItemPosition;
            }
            return result;
        }

        public virtual void ShowView(bool show)
        {
        }

        static Dictionary<string, Tuple<string, string>> m_actions =
           new Dictionary<string, Tuple<string, string>>();

        public virtual void AddAction(string varName,
                                     string strAction, string argument = "")
        {
            if (!string.IsNullOrWhiteSpace(argument))
            {
                if (argument.Equals("FINISHED"))
                {
                    if (ViewX is ListView)
                    {
                        ListView listView = ViewX as ListView;
                        listView.NothingSelected += (sender, e) =>
                        {
                            UIVariable.GetAction(strAction, varName, "");
                        };
                    }
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(strAction))
            {
                return;
            }
            if (ViewX is Button)
            {
                Button button = ViewX as Button;
                button.Click += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, argument);
                };
            }
            else if (ViewX is EditText)
            {
                if (argument.Equals("FINISHED"))
                {
                }
                else
                {
                    EditText editText = ViewX as EditText;
                    editText.TextChanged += (sender, e) =>
                    {
                        UIVariable.GetAction(strAction, varName, e.Text.ToString());
                    };
                }
            }
            else if (ViewX is Switch)
            {
                Switch sw = ViewX as Switch;
                sw.CheckedChange += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, e.ToString());
                };
            }
            else if (ViewX is SeekBar)
            {
                SeekBar slider = ViewX as SeekBar;
                slider.ProgressChanged += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, e.ToString());
                };
            }
            else if (ViewX is NumberPicker)
            {
                NumberPicker pickerView = ViewX as NumberPicker;
                pickerView.ValueChanged += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, e.NewVal.ToString());
                };
            }
            else if (ViewX is Spinner)
            {
                Spinner spinner = ViewX as Spinner;
                spinner.ItemSelected += (sender, e) =>
                {
                    var adapter = spinner.Adapter as TextImageAdapter;
                    var item = adapter != null ? adapter.Position2Text(e.Position) : "";
                    UIVariable.GetAction(strAction, varName, item);
                };
            }
            else if (ViewX is ListView)
            {
                ListView listView = ViewX as ListView;
                listView.ItemClick += (sender, e) =>
                {
                    UIVariable.GetAction(strAction, varName, e.Position.ToString());
                };
            }
            else
            {
                ActionDelegate += (arg1, arg2) =>
                {
                    UIVariable.GetAction(strAction, varName, arg2);
                };
            }

            m_actions[Name] = new Tuple<string, string>(strAction, varName);
        }

        public void ExecuteAction(string arg)
        {
            Tuple<string, string> action;
            if (m_actions.TryGetValue(Name, out action))
            {
                UIVariable.GetAction(action.Item1, action.Item2, arg);
            }
        }
        public bool SetFontSize(View view, float fontSize)
        {
            if (view is Button)
            {
                ((Button)view).TextSize = fontSize;
            }
            else if (view is TextView)
            {
                ((TextView)view).TextSize = fontSize;
            }
            else if (view is EditText)
            {
                ((EditText)view).TextSize = fontSize;
            }
            else if (view is Switch)
            {
                ((Switch)ViewX).TextSize = fontSize;
            }
            else if (view is Spinner)
            {
                Spinner spinner = (Spinner)view;
                TextImageAdapter adapter = spinner.Adapter as TextImageAdapter;
                if (adapter != null)
                {
                    adapter.TextSize = fontSize;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public virtual bool SetFontSize(double fontSize)
        {
            if (SetFontSize(ViewX, (float)fontSize))
            {
                return true;
            }
            ViewGroup layout = m_viewX as ViewGroup;
            if (layout == null || layout.ChildCount == 0)
            {
                return false;
            }
            for (int i = 0; i < layout.ChildCount; i++)
            {
                View view = layout.GetChildAt(i);
                SetFontSize(view, (float)fontSize);
            }
            return true;
        }
        public bool SetTypeface(Typeface typeface, TypefaceStyle style = TypefaceStyle.Normal)
        {
            if (m_viewX is Button)
            {
                ((Button)m_viewX).SetTypeface(typeface, style);
            }
            else if (m_viewX is TextView)
            {
                ((TextView)m_viewX).SetTypeface(typeface, style);
            }
            else if (m_viewX is EditText)
            {
                ((EditText)m_viewX).SetTypeface(typeface, style);
            }
            else if (m_viewX is Switch)
            {
                ((Switch)m_viewX).SetTypeface(typeface, style);
            }
            else if (m_viewX is Spinner)
            {
                Spinner spinner = (Spinner)m_viewX;
                TextImageAdapter adapter = spinner.Adapter as TextImageAdapter;
                if (adapter != null)
                {
                    adapter.Typeface = typeface;
                    adapter.TypefaceStyle = style;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public virtual bool SetFont(string name, double size = 0)
        {
            var typeface = Typeface.Create(name, TypefaceStyle.Normal);
            FontName = name;
            return SetTypeface(typeface);
        }

        bool SetBoldItalicNormal(double size = 0)
        {
            var style = !m_bold && !m_italic ? TypefaceStyle.Normal :
                          m_bold && m_italic ? TypefaceStyle.BoldItalic :
                          m_bold ? TypefaceStyle.Bold : TypefaceStyle.Italic;
            SetTypeface(null, style);
            if (size != 0)
            {
                SetFontSize(size);
            }
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
        public void ProcessTranslationY(DroidVariable location)
        {
            if (location.RuleY == "CENTER" && WidgetType == UIType.LABEL)
            {
                int deltaY = (int)AutoScaleFunction.TransformSize(20, UtilsDroid.GetScreenSize().Width); ;
                location.TranslationY += deltaY;
            }

            if (location.TranslationY < 0 && location.LayoutRuleY == LayoutRules.AlignParentTop)
            {
                location.TranslationY = 0;
            }
            else if (location.ViewY is Spinner)
            {
                if (location.LayoutRuleY == LayoutRules.AlignParentBottom ||
                    location.LayoutRuleY == LayoutRules.Below)
                {
                    location.ExtraY = (int)AutoScaleFunction.TransformSize(10, UtilsDroid.GetScreenSize().Width);
                    location.TranslationY -= location.ExtraY;
                }
            }
        }

        public void CreateStepper(int width, int height, string extraLabel)
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
            )
            {
                Width = height,
                Height = height
            };

            RelativeLayout.LayoutParams layoutParams2 = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent
            )
            {
                Width = height,
                Height = height
            };
            layoutParams2.AddRule(LayoutRules.RightOf, btn1.Id);

            if (!string.IsNullOrWhiteSpace(extraLabel))
            {
                label = new TextView(MainActivity.TheView);
                label.Text = CurrVal.ToString();
                label.Id = ++m_currentTag;
                label.Gravity = GravityFlags.Center;

                RelativeLayout.LayoutParams layoutParams3 = new RelativeLayout.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent
                )
                {
                    Width = height,
                    Height = height
                };
                if (extraLabel == "left")
                {
                    layoutParams1.AddRule(LayoutRules.RightOf, label.Id);
                    //label.Gravity = GravityFlags.Right | GravityFlags.CenterVertical;
                }
                else
                {
                    layoutParams3.AddRule(LayoutRules.RightOf, btn2.Id);
                    //label.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
                }
                label.LayoutParameters = layoutParams3;
                layout.AddView(label);
            }

            btn1.Touch += (sender, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    CurrVal -= Step;
                    CurrVal = CurrVal < MinVal ? MinVal : CurrVal;
                    CurrVal = CurrVal > MaxVal ? MaxVal : CurrVal;
                    btn1.Enabled = CurrVal > MinVal;
                    btn2.Enabled = CurrVal < MaxVal;
                    if (label != null)
                    {
                        label.Text = CurrVal.ToString();
                    }
                    ActionDelegate?.Invoke(WidgetName, CurrVal.ToString());
                }
            };
            btn2.Touch += (sender, e) =>
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    CurrVal += Step;
                    CurrVal = CurrVal < MinVal ? MinVal : CurrVal;
                    CurrVal = CurrVal > MaxVal ? MaxVal : CurrVal;
                    btn1.Enabled = CurrVal > MinVal;
                    btn2.Enabled = CurrVal < MaxVal;
                    if (label != null)
                    {
                        label.Text = CurrVal.ToString();
                    }
                    ActionDelegate?.Invoke(WidgetName, CurrVal.ToString());
                }
            };

            btn1.LayoutParameters = layoutParams1;
            btn2.LayoutParameters = layoutParams2;

            m_viewX = layout;
        }

        View m_viewX;
        View m_viewY;
        LayoutRules m_layoutRuleX;
        LayoutRules m_layoutRuleY;

        public static Size GetLocation(View view)
        {
            if (view == null)
            {
                return null;
            }
            int[] outArr = new int[2];
            view.GetLocationOnScreen(outArr);
            return new Size(outArr[0], outArr[1]);
        }

        public static View GetView(string viewName, ParsingScript script)
        {
            if (viewName.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            ParserFunction func = ParserFunction.GetVariable(viewName, script);
            Utils.CheckNotNull(func, viewName, script);
            Variable viewValue = func.GetValue(script);
            DroidVariable viewVar = viewValue as DroidVariable;
            return viewVar.ViewX;
        }
        public virtual void AddData(List<string> data, string varName, string title, string extra)
        {
            if (ViewX is NumberPicker)
            {
                NumberPicker pickerView = ViewX as NumberPicker;

                pickerView.SaveFromParentEnabled = false;
                pickerView.SaveEnabled = true;

                pickerView.SetDisplayedValues(data.ToArray());
                pickerView.MinValue = 0;
                pickerView.MaxValue = data.Count - 1;
                pickerView.Value = 0;
                pickerView.WrapSelectorWheel = false;

                AddAction(varName, title);
            }
            else if (ViewX is Spinner)
            {
                Spinner spinner = ViewX as Spinner;
                var adapter = spinner.Adapter as TextImageAdapter;
                if (adapter == null)
                {
                    adapter = new TextImageAdapter(MainActivity.TheView);
                }
                string first = null;//InitValue == null ? null : InitValue.AsString();
                adapter.SetItems(data, first);
                spinner.Adapter = adapter;
                AddAction(varName, title);
            }
            else if (ViewX is ListView)
            {
                ListView listView = ViewX as ListView;
                var adapter = listView.Adapter as TextImageAdapter;
                if (adapter == null)
                {
                    adapter = new TextImageAdapter(MainActivity.TheView);
                }
                adapter.SetItems(data);
                listView.Adapter = adapter;
                AddAction(varName, title);
            }
        }
        public virtual void AddImages(List<string> images, string varName, string title)
        {
            if (ViewX is Spinner)
            {
                Spinner spinner = ViewX as Spinner;
                var adapter = spinner.Adapter as TextImageAdapter;
                if (adapter == null)
                {
                    adapter = new TextImageAdapter(MainActivity.TheView);
                }
                adapter.SetPics(images);
                spinner.Adapter = adapter;
                if (!string.IsNullOrEmpty(title))
                {
                    AddAction(varName, title);
                }
            }
            else if (ViewX is ListView)
            {
                ListView listView = ViewX as ListView;
                var adapter = listView.Adapter as TextImageAdapter;
                if (adapter == null)
                {
                    adapter = new TextImageAdapter(MainActivity.TheView);
                }
                adapter.SetPics(images);
                listView.Adapter = adapter;
                if (!string.IsNullOrEmpty(title))
                {
                    AddAction(varName, title);
                }
            }
        }
        public virtual bool SetBackgroundColor(string colorStr, double alpha = 1.0)
        {
            if (ViewX == null)
            {
                return false;
            }

            var color = UtilsDroid.String2Color(colorStr);
            if (alpha < 1.0)
            {
                color = Color.Argb((int)(alpha * 255), color.R, color.G, color.B);
            }
            ViewX.SetBackgroundColor(color);

            return true;
        }
        public virtual bool SetFontColor(string colorStr)
        {
            if (ViewX == null)
            {
                return false;
            }

            Color color = UtilsDroid.String2Color(colorStr);

            if (ViewX is Button)
            {
                ((Button)ViewX).SetTextColor(color);
            }
            else if (ViewX is TextView)
            {
                ((TextView)ViewX).SetTextColor(color);
            }
            else if (ViewX is EditText)
            {
                ((EditText)ViewX).SetTextColor(color);
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
