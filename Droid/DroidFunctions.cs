using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using SplitAndMerge;

namespace scripting.Droid
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

            DroidVariable refViewX = viewNameX == "ROOT" ? null :
                Utils.GetVariable(viewNameX, script) as DroidVariable;

            DroidVariable refViewY = viewNameY == "ROOT" ? null : 
                Utils.GetVariable(viewNameY, script) as DroidVariable;

            DroidVariable location = new DroidVariable(UIVariable.UIType.LOCATION, viewNameX + viewNameY,
                                                     refViewX, refViewY);

            location.ViewX = refViewX == null ? null : refViewX.ViewX;
            location.ViewY = refViewY == null ? null : refViewY.ViewX;

            location.SetRules(ruleStrX, ruleStrY);
            location.ParentView = parentView as DroidVariable;

            location.TranslationX = leftMargin;
            location.TranslationY = topMargin;
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

            if (start == 1)    {
                widgetType = args[0].AsString();
                Utils.CheckNotEmpty(script, widgetType, m_name);
            }
            DroidVariable location = args[start] as DroidVariable;
            Utils.CheckNotNull(location, m_name);
            
            string varName = args[start + 1].AsString();
            string text    = Utils.GetSafeString(args, start + 2);
            int width      = Utils.GetSafeInt(args, start + 3);
            int height     = Utils.GetSafeInt(args, start + 4);

            location.SetSize(width, height);
            location.LayoutRuleX = UtilsDroid.String2LayoutParam(location, true);
            location.LayoutRuleY = UtilsDroid.String2LayoutParam(location, false);

            DroidVariable widgetFunc = GetWidget(widgetType, varName, text);
            Utils.CheckNotNull(widgetFunc, m_name);

            widgetFunc.Location = location;
            View widget = widgetFunc.ViewX;

            RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );

            if (widgetFunc.WidgetType == UIVariable.UIType.VIEW) {
                widgetFunc.SetViewLayout(width, height);
                layoutParams = widgetFunc.ViewLayout.LayoutParameters
                                         as RelativeLayout.LayoutParams;}


            ApplyRule(layoutParams, location.LayoutRuleX, location.ViewX);
            ApplyRule(layoutParams, location.LayoutRuleY, location.ViewY);

            //layoutParams.SetMargins(location.LeftMargin,  location.TopMargin,
            //                        location.RightMargin, location.BottomMargin);
            if (location.Height > 0 || location.Width > 0) {
                layoutParams.Height = location.Height;
                layoutParams.Width  = location.Width;
            }
            widget.LayoutParameters = layoutParams;

            widget.TranslationX = location.TranslationX;
            widget.TranslationY = location.TranslationY;

            var parentView = location.ParentView as DroidVariable;
            MainActivity.AddView(widget, parentView?.ViewLayout);

            ParserFunction.AddGlobal(varName, new GetVarFunction(widgetFunc));
            return widgetFunc;
        }

        public static DroidVariable GetWidget(string widgetType, string widgetName, string initArg)
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
                case "Label":
                    type = UIVariable.UIType.LABEL;
                    widget = new TextView(MainActivity.TheView);
                    ((TextView)widget).SetTextColor(Color.Black);
                    ((TextView)widget).Text = initArg;
                    break;
                case "TextView":
                case "TextEdit":
                    type = UIVariable.UIType.TEXT_FIELD;
                    widget = new EditText(MainActivity.TheView);
                    ((EditText)widget).SetTextColor(Color.Black);
                    ((EditText)widget).Hint = initArg;
                    break;
                case "ImageView":
                    type = UIVariable.UIType.IMAGE_VIEW;
                    widget = new ImageView(MainActivity.TheView);
                    if (!string.IsNullOrWhiteSpace(initArg)) {
                        int resourceID = MainActivity.String2Pic(initArg);
                        widget.SetBackgroundResource(resourceID);
                    }
                    break;
                case "TypePicker":
                    type = UIVariable.UIType.PICKER_VIEW;
                    widget = new NumberPicker(MainActivity.TheView);
                    // Don't show the cursor on the picker:
                    ((NumberPicker)widget).DescendantFocusability = DescendantFocusability.BlockDescendants;
                    break;
                case "Switch":
                    type = UIVariable.UIType.SWITCH;
                    widget = new Switch(MainActivity.TheView);
                    SetValues(widget, initArg);
                    break;
                case "Slider":
                    type = UIVariable.UIType.SLIDER;
                    widget = new SeekBar(MainActivity.TheView);
                    SetValues(widget, initArg);
                    break;
                default:
                    type = UIVariable.UIType.VIEW;
                    widget = new View(MainActivity.TheView);
                    break;
            }

            DroidVariable widgetFunc = new DroidVariable(type, widgetName, widget);
            return widgetFunc;
        }

        public static void SetValues(View view, string valueStr)
        {
            double value1, value2 = 0;
            string[] vals = valueStr.Split(new char[] { ',', ':' });
            Double.TryParse(vals[0].Trim(), out value1);
            Double.TryParse(vals[vals.Length - 1].Trim(), out value2);

            if (view is Switch) {
                Switch sw = view as Switch;
                sw.Checked = (int)value1 == 1;
            } else if (view is SeekBar) {
                SeekBar slider = view as SeekBar;
                slider.Max = (int)value2 - (int)value1;
                slider.Progress = (int)(value1 + value2) / 2;
            }
        }
        public static void ApplyRule(RelativeLayout.LayoutParams layoutParams,
                                     LayoutRules rule, View view = null)
        {
            if (view != null) {
                layoutParams.AddRule(rule, view.Id);
            } else {
                layoutParams.AddRule(rule);
            }
        }
        string m_widgetType;
    }
    public class AddBorderFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            bool isList = false;
            List<Variable> args = Utils.GetArgs(script,
                Constants.START_ARG, Constants.END_ARG, out isList);

            Utils.CheckArgs(args.Count, 1, m_name);
            DroidVariable viewVar = args[0] as DroidVariable;
            View view = viewVar.ViewX;

            Utils.CheckNotNull(view, m_name);

            int width       = Utils.GetSafeInt(args, 1, 1);
            int corner      = Utils.GetSafeInt(args, 2, 5);
            string colorStr = Utils.GetSafeString(args, 3, "#000000");
            Color color = Color.ParseColor(colorStr);

            UtilsDroid.AddViewBorder(view, color, width, corner);
            return Variable.EmptyInstance;
        }
    }

    public class AddTabFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string text = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, text, m_name);

            string imageName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, text, m_name);

            string selectedImageName = null;
            if (script.Current == Constants.NEXT_ARG) {
                selectedImageName = Utils.GetItem(script).AsString();
            }

            MainActivity.AddTab(text, imageName, selectedImageName);

            return Variable.EmptyInstance;
        }
    }
    public class AddWidgetDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            ParserFunction func = ParserFunction.GetFunction(varName);
            Utils.CheckNotNull(func, varName);
            DroidVariable viewVar = func.GetValue(script) as DroidVariable;
            Utils.CheckNotNull(viewVar, m_name);

            NumberPicker pickerView = viewVar.ViewX as NumberPicker;
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

            pickerView.SaveFromParentEnabled = false;
            pickerView.SaveEnabled = true;

            pickerView.SetDisplayedValues(types.ToArray());
            pickerView.MinValue = 0;
            pickerView.MaxValue = types.Count - 1;
            pickerView.Value = 0;
            pickerView.WrapSelectorWheel = false;

            pickerView.ValueChanged += (sender, e) => {
                UIVariable.GetAction(strAction, e.NewVal.ToString());
            };

            return Variable.EmptyInstance;
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

            ParserFunction func = ParserFunction.GetFunction(varName);
            Utils.CheckNotNull(func, varName);
            DroidVariable viewVar = func.GetValue(script) as DroidVariable;
            Utils.CheckNotNull(viewVar, m_name);

            // Special dealing if the user tries to show/hide the layout:
            View view = viewVar.ViewLayout;
            if (view == null) {
                // Otherwise it's a subview.
                view = viewVar.ViewX; 
                if (view == null) {
                    // Otherwise it's a a Main Root view.
                    view = MainActivity.TheLayout.RootView;
                }
            }

            MainActivity.ShowView(view, m_show);

            return Variable.EmptyInstance;
        }
    }
    public class SelectTabFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            int tabId = Utils.GetItem(script).AsInt();
            MainActivity.TheView.ChangeTab(tabId);
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

            View view = DroidVariable.GetView(varName, script);

            var layoutParams = view.LayoutParameters;
            layoutParams.Width  = width.AsInt();
            layoutParams.Height = height.AsInt();
            view.LayoutParameters = layoutParams;

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

            int resourceID = MainActivity.String2Pic(imageName);

            View view = DroidVariable.GetView(varName, script);
            view.SetBackgroundResource(resourceID);

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

            View view = DroidVariable.GetView(varName, script);
            if (view == null){
                view = MainActivity.TheLayout.RootView;
            }
            view.SetBackgroundColor(String2Color(strColor));

            return Variable.EmptyInstance;
        }
        public static Color String2Color(string colorStr)
        {
            switch(colorStr) {
                case "blue":        return Color.Blue;
                case "cyan":        return Color.Cyan;
                case "green":       return Color.Green;
                case "yellow":      return Color.Yellow;
                case "white":       return Color.White;
                case "red":         return Color.Red;
                case "brown":       return Color.Brown;
                case "orange":      return Color.Orange;
                case "rose":        return Color.Magenta;
                case "gray":        return Color.LightGray;
                case "purple":      return Color.Purple;
                default:            return Color.Transparent;
            }
        }
    }

    public class SetBackgroundImageFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            Variable imageNameVar = Utils.GetItem(script);
            string imageName = imageNameVar.AsString();

            int resourceID = MainActivity.String2Pic(imageName);

            View view = MainActivity.TheLayout.RootView;
            view.SetBackgroundResource(resourceID);

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

            View view = DroidVariable.GetView(varName, script);
            AddAction(view, varName, strAction);

            return Variable.EmptyInstance;
        }
        public static void AddAction(View view, string varName, string strAction)
        {
            if (view is Button) {
                Button button = view as Button;
                button.Click += (sender, e) => {
                    UIVariable.GetAction(strAction, varName, "\"" + e + "\"");
                };
            } else if (view is Switch) {
                Switch sw = view as Switch;
                sw.CheckedChange += (sender, e) => {
                    UIVariable.GetAction(strAction, varName, "\"" + e + "\"");
                };
            } else if (view is SeekBar) {
                SeekBar slider = view as SeekBar;
                slider.ProgressChanged += (sender, e) => {
                    UIVariable.GetAction(strAction, varName, "\"" + e + "\"");
                };
            }
        }
    }
    public class SetTextFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            Variable title = Utils.GetItem(script);
            string text = title.AsString();

            View view = DroidVariable.GetView(varName, script);
            Utils.CheckNotNull(view, m_name);

            SetText(view, text);

            return title;
        }
        public static void SetText(View view, string text)
        {
            if (view is Button) {
                ((Button)view).Text = text;
            } else if (view is TextView) {
                ((TextView)view).Text = text;
            } else if (view is EditText) {
                ((EditText)view).Text = text;
            } else if (view is NumberPicker) {
                NumberPicker picker = view as NumberPicker;
                string[] all = picker.GetDisplayedValues();
                List<string> names = new List<string>(all);
                int row = names.FindIndex((obj) => obj.Equals(text));
                row = row < 0 ? 0 : row;
                picker.Value = row;
            }
        }
    }
    public class GetTextFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            View view = DroidVariable.GetView(varName, script);
            Utils.CheckNotNull(view, m_name);

            string text = GetText(view);
 
            return new Variable(text);
        }
        public static string GetText(View view)
        {
            string text = "";
            if (view is Button) {
                text = ((Button)view).Text;
            } else if (view is TextView) {
                text = ((TextView)view).Text;
            } else if (view is EditText) {
                text = ((EditText)view).Text;
            } else if (view is NumberPicker) {
                NumberPicker picker = view as NumberPicker;
                string[] all = picker.GetDisplayedValues();
                int row = (int)GetValueFunction.GetValue(picker);
                if (all.Length > row && row >= 0) {
                    picker.Value = row;
                }
            }
            return text;
        }
    }
    public class SetValueFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            Variable arg = Utils.GetItem(script);

            DroidVariable droidVar = Utils.GetVariable(varName, script) as DroidVariable;
            Utils.CheckNotNull(droidVar, m_name);

            SetValue(droidVar.ViewX, arg.Value);

            return droidVar;
        }
        public static void SetValue(View view, double val)
        {
            if (view is Switch) {
                ((Switch)view).Checked = (int)val == 1;
            } else if (view is SeekBar) {
                ((SeekBar)view).Progress = (int)val;
            } else if (view is NumberPicker) {
                ((NumberPicker)view).Value = (int)val;
            }
        }
    }
    public class GetValueFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);

            DroidVariable droidVar = Utils.GetVariable(varName, script) as DroidVariable;
            Utils.CheckNotNull(droidVar, m_name);

            double result = GetValue(droidVar.ViewX);

            return new Variable(result);
        }
        public static double GetValue(View view)
        {
            double result = 0;
            if (view is Switch) {
                result = ((Switch)view).Checked ? 1 : 0;
            } else if (view is SeekBar) {
                result = ((SeekBar)view).Progress;
            } else if (view is NumberPicker) {
                result = ((NumberPicker)view).Value;
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

            View view = DroidVariable.GetView(varName, script);
            Utils.CheckNotNull(view, m_name);

            string alignment = Utils.GetItem(script).AsString();
            alignment = alignment.ToLower();

            view.TextAlignment = TextAlignment.Gravity;
            GravityFlags gravity = GravityFlags.Center;

            switch(alignment) {
                case "left":
                    gravity = GravityFlags.Left;
                    break;
                case "right":
                    gravity = GravityFlags.Right;
                    break;
                case "fill":
                case "natural":
                    gravity = GravityFlags.Fill;
                    break;
            }                   

            if (view is Button) {
                ((Button)view).Gravity = gravity;
            } else if (view is TextView) {
                ((TextView)view).Gravity = gravity;
            }

            return Variable.EmptyInstance;
        }
    }
    public class SetFontSizeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetItem(script).AsString();
            Utils.CheckNotEmpty(script, varName, m_name);
            script.MoveForwardIf(Constants.NEXT_ARG);

            Variable fontSize = Utils.GetItem(script);
            Utils.CheckPosInt(fontSize);

            View view = DroidVariable.GetView(varName, script);
            Utils.CheckNotNull(view, m_name);

            if (view is Button)    {
                ((Button)view).TextSize   = (int)fontSize.Value;
            }
            else if (view is TextView) {
                ((TextView)view).TextSize = (int)fontSize.Value;
            }
            else if (view is EditText) {
                ((EditText)view).TextSize = (int)fontSize.Value;
            }
            else {
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
            DisplayMetrics bounds = new DisplayMetrics();
            var winManager = MainActivity.TheView.WindowManager;
            winManager.DefaultDisplay.GetMetrics(bounds);

            return new Variable(m_needWidth ? bounds.WidthPixels :
                                              bounds.HeightPixels);
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
