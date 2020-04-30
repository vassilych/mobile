using System;
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 4, m_name);

            string viewNameX = args[0].AsString();
            string ruleStrX = args[1].AsString();
            string viewNameY = args[2].AsString();
            string ruleStrY = args[3].AsString();

            int leftMargin = Utils.GetSafeInt(args, 4);
            int topMargin = Utils.GetSafeInt(args, 5);

            bool autoResize = Utils.GetSafeInt(args, 6, 1) == 1;
            if (autoResize)
            {
                double multiplier = Utils.GetSafeDouble(args, 7);
                AutoScaleFunction.TransformSizes(ref leftMargin, ref topMargin,
                                  UtilsDroid.GetScreenSize().Width, multiplier);
            }

            Variable parentView = Utils.GetSafeVariable(args, 8, null);

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

            // This adjusts Custom widgits (they must override AdjustTranslation)
            //((DroidVariable)location.RefViewX)?.AdjustTranslation(location, true, false);
            //((DroidVariable)location.RefViewY)?.AdjustTranslation(location, false, false);

            location.TranslationX += leftMargin;
            location.TranslationY += topMargin;

            return location;
        }
    }

    public class AddWidgetFunction : ParserFunction
    {
        public AddWidgetFunction(string widgetType = "", string extras = "")
        {
            m_widgetType = widgetType;
            m_extras = extras;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            string widgetType = m_widgetType;
            int start = string.IsNullOrEmpty(widgetType) ? 1 : 0;
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2 + start, m_name);

            if (start == 1)
            {
                widgetType = args[0].AsString();
                Utils.CheckNotEmpty(script, widgetType, m_name);
            }
            DroidVariable location = args[start] as DroidVariable;
            Utils.CheckNotNull(location, m_name, script);

            string varName = args[start + 1].AsString();
            string text = Utils.GetSafeString(args, start + 2);
            int width = Utils.GetSafeInt(args, start + 3);
            int height = Utils.GetSafeInt(args, start + 4);

            bool autoResize = Utils.GetSafeInt(args, start + 5, 1) == 1;
            double multiplier = Utils.GetSafeDouble(args, start + 6);
            ScreenSize screenSize = UtilsDroid.GetScreenSize();
            if (autoResize)
            {
                AutoScaleFunction.TransformSizes(ref width, ref height,
                                                 screenSize.Width, multiplier);
            }

            location.SetSize(width, height);
            location.LayoutRuleX = UtilsDroid.String2LayoutParam(location, true);
            location.LayoutRuleY = UtilsDroid.String2LayoutParam(location, false);

            RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent
            );

            DroidVariable widgetFunc = ExistingWidget(script, varName);
            bool alreadyExists = widgetFunc != null;
            if (!alreadyExists)
            {
                widgetFunc = GetWidget(widgetType, varName, text, width, height);

                if (widgetFunc.WidgetType == UIVariable.UIType.VIEW)
                {
                    widgetFunc.SetViewLayout(width, height);
                    layoutParams = widgetFunc.ViewLayout.LayoutParameters
                                             as RelativeLayout.LayoutParams;
                }
                else if (widgetFunc.WidgetType == UIVariable.UIType.STEPPER)
                {
                    widgetFunc.CreateStepper(width, height, m_extras);
                    layoutParams = widgetFunc.ViewLayout.LayoutParameters
                                             as RelativeLayout.LayoutParams;
                }
            }

            Utils.CheckNotNull(widgetFunc, m_name, script);
            View widget = widgetFunc.ViewX;

            widgetFunc.ProcessTranslationY(location);
            widgetFunc.Location = location;
            widgetFunc.SetSize(width, height);

            ((DroidVariable)location.RefViewX)?.AdjustTranslation(location, true, true);
            //((DroidVariable)location.RefViewX)?.AdjustTranslation(location, false, true);
            //widgetFunc.AdjustTranslation(location, true, true);
            //widgetFunc.AdjustTranslation(location, false, true);

            ApplyRule(layoutParams, location.LayoutRuleX, location.ViewX);
            ApplyRule(layoutParams, location.LayoutRuleY, location.ViewY);

            //layoutParams.SetMargins(location.LeftMargin,  location.TopMargin,
            //                        location.RightMargin, location.BottomMargin);
            if (location.Height > 0 || location.Width > 0)
            {
                layoutParams.Height = location.Height;
                layoutParams.Width = location.Width;
            }

            widget.LayoutParameters = widgetFunc.LayoutParams = layoutParams;
            widget.TranslationX = widgetFunc.TranslationX = location.TranslationX;
            widget.TranslationY = widgetFunc.TranslationY = location.TranslationY;

            MainActivity.AddView(widgetFunc, alreadyExists);

            RegisterWidget(widgetFunc);
            return widgetFunc;
        }

        public static DroidVariable GetWidget(string widgetType, string widgetName, string initArg,
                                             int width, int height)
        {
            for (int i = 0; i < UIVariable.WidgetTypes.Count; i++)
            {
                DroidVariable var = UIVariable.WidgetTypes[i] as DroidVariable;
                var widget = var.GetWidget(widgetType, widgetName, initArg, width, height);
                if (widget != null)
                {
                    return widget;
                }
            }
            return null;
        }

        public static void RegisterWidget(DroidVariable widget)
        {
            var location = widget.Location;
            var parent = location.ParentView;
            View view = widget.ViewX;
            string parentName = parent == null ? "" : parent.WidgetName;
            int tabId = MainActivity.CurrentTabId;
            UIUtils.Rect rect = new UIUtils.Rect((int)view.GetX(), (int)view.GetY(),
                                                  view.Width, view.Height);
            UIUtils.RegisterWidget(widget, rect, parentName, tabId);
        }
        public static void ApplyRule(RelativeLayout.LayoutParams layoutParams,
                                     LayoutRules rule, View view = null)
        {
            if (view != null)
            {
                layoutParams.AddRule(rule, view.Id);
            }
            else
            {
                layoutParams.AddRule(rule);
            }
        }
        public static DroidVariable ExistingWidget(ParsingScript script, string varName)
        {
            ParserFunction func = ParserFunction.GetFunction(varName, script);
            if (func == null)
            {
                return null;
            }
            DroidVariable viewVar = func.GetValue(script) as DroidVariable;
            MainActivity.RemoveView(viewVar);
            return viewVar;
        }

        string m_widgetType;
        string m_extras;
    }
    public class AddBorderFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);
            View view = widget.ViewX;

            Utils.CheckNotNull(view, m_name, script);

            int width = Utils.GetSafeInt(args, 1, 1);
            if (width <= 0)
            {
                return Variable.EmptyInstance;
            }
            int corner = Utils.GetSafeInt(args, 2, 5);
            string colorStr = Utils.GetSafeString(args, 3, "#000000");
            Color color = UtilsDroid.String2Color(colorStr);

            UtilsDroid.AddViewBorder(view, color, width, corner);
            return Variable.EmptyInstance;
        }
    }

    public class AddWidgetDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            Variable data = Utils.GetSafeVariable(args, 1, null);
            Utils.CheckNotNull(data, m_name, script);
            Utils.CheckNotNull(data.Tuple, m_name, script);

            List<string> types = new List<string>(data.Tuple.Count);
            for (int i = 0; i < data.Tuple.Count; i++)
            {
                types.Add(data.Tuple[i].AsString());
            }

            string strAction = Utils.GetSafeString(args, 2);
            string extra = Utils.GetSafeString(args, 3);

            widget.AddData(types, widget.Name, strAction, extra);

            return Variable.EmptyInstance;
        }
    }

    public class ClearWidgetDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            widget.Clear();
            return Variable.EmptyInstance;
        }
    }

    public class AddWidgetImagesFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            Variable data = Utils.GetSafeVariable(args, 1, null);
            Utils.CheckNotNull(data, m_name, script);
            Utils.CheckNotNull(data.Tuple, m_name, script);

            List<string> images = new List<string>(data.Tuple.Count);
            for (int i = 0; i < data.Tuple.Count; i++)
            {
                images.Add(data.Tuple[i].AsString());
            }

            string strAction = Utils.GetSafeString(args, 2);
            widget.AddImages(images, widget.Name, strAction);

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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            int deltaX = args[1].AsInt();
            int deltaY = args[2].AsInt();

            bool autoResize = Utils.GetSafeInt(args, 3, 1) == 1;
            if (autoResize)
            {
                double multiplier = Utils.GetSafeDouble(args, 4);
                AutoScaleFunction.TransformSizes(ref deltaX, ref deltaY,
                                  UtilsDroid.GetScreenSize().Width, multiplier);
            }

            View view = widget.ViewX;
            Utils.CheckNotNull(view, m_name, script);

            if (deltaX < 0)
            {
                deltaX = (int)view.GetX();
            }
            if (deltaY < 0)
            {
                deltaY = (int)view.GetY();
            }
            float x = m_isAbsolute ? deltaX : view.GetX() + deltaX;
            float y = m_isAbsolute ? deltaY : view.GetY() + deltaY;
            view.SetX(x);
            view.SetY(y);
            //view.TranslationX = deltaX;
            //view.TranslationY = deltaY;

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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            View view = widget.ViewX;
            Utils.CheckNotNull(view, m_name, script);

            int coord = 0;
            if (m_isX)
            {
                coord = (int)view.GetX();
            }
            else
            {
                coord = (int)view.GetY();
            }

            return new Variable(coord);
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            bool show = Utils.GetSafeInt(args, 1, m_show ? 1 : 0) != 0;

            // Special dealing if the user tries to show/hide the layout:
            View view = widget?.ViewX;
            if (view == null)
            {
                // Otherwise it's a a Main Root view.
                view = MainActivity.TheLayout.RootView;
            }

            widget?.ShowView(show);
            MainActivity.ShowView(view, show);

            return Variable.EmptyInstance;
        }
    }
    public class RemoveViewFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            MainActivity.RemoveView(widget);
            UIUtils.DeregisterWidget(widget.Name);

            return Variable.EmptyInstance;
        }
    }
    class RemoveViewIfExistsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string varName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            varName = Constants.ConvertName(varName);

            bool exists = ParserFunction.GetVariable(varName, script) != null;
            if (!exists)
            {
                return new Variable(false);
            }

            var widget = Utils.GetVariable(varName, script) as DroidVariable;
            if (widget == null)
            {
                return new Variable(false);
            }

            MainActivity.RemoveView(widget);
            UIUtils.DeregisterWidget(widget.Name);

            return new Variable(exists);
        }
    }

    public class RemoveAllViewsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();

            int tabId = Utils.GetSafeInt(args, 0, -1);
            if (tabId >= 0)
            {
                MainActivity.RemoveTabViews(tabId);
                UIUtils.DeregisterTab(tabId);
            }
            else
            {
                MainActivity.RemoveAll();
                UIUtils.DeregisterAll();
            }

            return Variable.EmptyInstance;
        }
    }
    public class GetSelectedTabFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            int tabId = MainActivity.CurrentTabId;
            script.MoveForwardIf(Constants.END_ARG);
            return new Variable(tabId);
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string text = Utils.GetSafeString(args, 0);
            string selectedImageName = Utils.GetSafeString(args, 1);
            string notSelectedImageName = Utils.GetSafeString(args, 2, selectedImageName);

            if (!m_forceCreate && MainActivity.SelectTab(text))
            {
                return Variable.EmptyInstance;
            }

            MainActivity.AddTab(text, selectedImageName, notSelectedImageName);

            return Variable.EmptyInstance;
        }
    }
    public class SelectTabFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            int tabId = Utils.GetSafeInt(args, 0);
            MainActivity.TheView.ChangeTab(tabId);
            return Variable.EmptyInstance;
        }
    }
    public class OnTabSelectedFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string action = args[0].AsString();

            MainActivity.TabSelectedDelegate += (tab) =>
            {
                UIVariable.GetAction(action, "ROOT", tab.ToString());
            };
            return Variable.EmptyInstance;
        }
    }
    public class SetSizeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            string varName = Utils.GetSafeString(args, 0);
            Variable widthVar = Utils.GetSafeVariable(args, 1);
            Utils.CheckNonNegativeInt(widthVar, script);
            int width = widthVar.AsInt();

            Variable heightVar = Utils.GetSafeVariable(args, 2);
            Utils.CheckNonNegativeInt(heightVar, script);
            int height = heightVar.AsInt();

            bool autoResize = Utils.GetSafeInt(args, 3, 1) == 1;
            if (autoResize)
            {
                double multiplier = Utils.GetSafeDouble(args, 4);
                AutoScaleFunction.TransformSizes(ref width, ref height,
                                  UtilsDroid.GetScreenSize().Width, multiplier);
            }
            View view = DroidVariable.GetView(varName, script);

            var layoutParams = view.LayoutParameters;
            if (width > 0)
            {
                layoutParams.Width = width;
            }
            if (height > 0)
            {
                layoutParams.Height = height;
            }
            view.LayoutParameters = layoutParams;

            return Variable.EmptyInstance;
        }
    }

    public class PickColorDialogFunction : ParserFunction
    {
        string m_action;
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();

            string title = Utils.GetSafeString(args, 0, "Pick a color");
            var initColor = UtilsDroid.String2Color(Utils.GetSafeString(args, 1, "white"));
            m_action = Utils.GetSafeString(args, 2, "ColorPicked");
            
            int colorInt = (initColor.A & 0xff) << 24 | (initColor.R & 0xff) << 16 | (initColor.G & 0xff) << 8 | (initColor.B & 0xff);

            Action<int, bool> callback1 = new Action<int, bool>(OnColorSelected);
            Action<int> callback2 = new Action<int>(OnColor);

            var observer = new Top.Defaults.ColorPickerLib.ColorPickerPopup.ColorPickerObserver(callback1, callback2);

            var picker = new Top.Defaults.ColorPickerLib.ColorPickerPopup.Builder(MainActivity.TheView);
            picker.InitialColor(colorInt);
            //picker.EnableAlpha(true);
            picker.OkTitle("OK");
            picker.CancelTitle("Cancel");
            picker.ShowIndicator(true);
            picker.ShowValue(true);
            picker.Build().Show(MainActivity.TheLayout, observer);

            return Variable.EmptyInstance;
        }

        void OnColorSelected(int colorInt, bool fromUser)
        {
            //Color color = new Color(colorInt);
            //var hex = "#" + string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
            //UIVariable.GetAction(m_action, "", hex);
        }
        void OnColor(int colorInt)
        {
            Color color = new Color(colorInt);
            var hex = "#" + string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
            UIVariable.GetAction(m_action, "", hex);
        }

    }
   
    public class ShowHideKeyboardFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            bool showKeyboard = Utils.GetSafeInt(args, 1, 1) == 1;

            MainActivity.TheView.ShowHideKeyboard(widget.ViewX, showKeyboard);
            return Variable.EmptyInstance;
        }
    }
    public class IsKeyboardFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            bool result = MainActivity.KeyboardVisible;

            return new Variable(result);
        }
    }
    public class SetImageFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string varName = args[0].AsString();
            string imageStr = args[1].AsString();

            string imageName = UIUtils.String2ImageName(imageStr);

            int resourceID = MainActivity.String2Pic(imageName);

            if (resourceID > 0)
            {
                View view = DroidVariable.GetView(varName, script);
                if (view is Button)
                {
                    Button but = view as Button;
                    Drawable dr = MainActivity.TheView.Resources.GetDrawable(resourceID);
                    but.Background = dr;
                }
                else
                {
                    view.SetBackgroundResource(resourceID);
                }
            }
            else
            {
                throw new ArgumentException("Couldn't load [" + imageName +
                            "] from disk.");
            }

            return resourceID > 0 ? new Variable(imageName) : Variable.EmptyInstance;
        }
    }
    public class SetBackgroundColorFunction : ParserFunction
    {
        public Color BGColor = Color.Transparent;
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            if (args.Count == 1)
            {
                string rootColor = Utils.GetSafeString(args, 0);
                MainActivity.TheLayout.RootView.SetBackgroundColor(UtilsDroid.String2Color(rootColor));
                return Variable.EmptyInstance;
            }

            string varName = args[0].AsString();
            string strColor = Utils.GetSafeString(args, 1);
            double alpha = Utils.GetSafeDouble(args, 2, 1.0);

            if (varName.Equals("ROOT"))
            {
                BGColor = UtilsDroid.String2Color(strColor);
                MainActivity.TheLayout.RootView.SetBackgroundColor(BGColor);
            }
            else
            {
                DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
                Utils.CheckNotNull(widget, m_name, script, 0);
                widget.SetBackgroundColor(strColor, alpha);
            }

            return Variable.EmptyInstance;
        }
    }

    public class SetBackgroundImageFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string imageName = Utils.GetSafeString(args, 0);

            int resourceID = MainActivity.String2Pic(imageName);

            if (resourceID <= 0)
            {
                throw new ArgumentException("Couldn't load [" + imageName +
                            "] from disk.");
            }

            View view = MainActivity.TheLayout.RootView;
            view.SetBackgroundResource(resourceID);

            return Variable.EmptyInstance;
        }
    }

    public class AddActionFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string strAction = Utils.GetSafeString(args, 1);
            string argument = Utils.GetSafeString(args, 2);

            widget.AddAction(widget.Name, strAction, argument);

            return Variable.EmptyInstance;
        }
    }
    public class AddLongClickFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string varName = args[0].AsString();
            string strAction = args[1].AsString();

            View view = DroidVariable.GetView(varName, script);

            view.LongClick += (sender, e) =>
            {
                UIVariable.GetAction(strAction, varName, e.ToString());
            };

            return Variable.EmptyInstance;
        }

        /*public class GestureListener : Java.Lang.Object, View.IOnLongClickListener
        {
           public bool OnLongClick(View v)
           {
                return true;
           }
        }*/
    }

    public class AddSwipeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string varName = Utils.GetSafeString(args, 0);
            string direction = Utils.GetSafeString(args, 1);
            string strAction = Utils.GetSafeString(args, 2);

            View view = DroidVariable.GetView(varName, script);
            Utils.CheckNotNull(view, m_name, script);

            OnTouchListener.Direction dir = OnTouchListener.Direction.Left;
            switch (direction)
            {
                case "Left": dir = OnTouchListener.Direction.Left; break;
                case "Right": dir = OnTouchListener.Direction.Right; break;
                case "Down": dir = OnTouchListener.Direction.Down; break;
                case "Up": dir = OnTouchListener.Direction.Up; break;
            }

            OnTouchListener listener = new OnTouchListener(view, dir, strAction, varName);
            view.Touch += listener.OnTouch;

            return Variable.EmptyInstance;
        }
        public class OnTouchListener
        {
            public enum Direction { Left, Right, Down, Up }
            Direction m_direction;
            View m_view;
            string m_action;
            string m_varName;

            float m_startX;
            float m_startY;

            const float TOUCH_DELTA = 10;

            public OnTouchListener(View view, Direction dir,
                                   string strAction = null, string varName = null)
            {
                m_direction = dir;
                m_view = view;
                m_action = strAction;
                m_varName = varName;
            }
            public void OnTouch(object sender, View.TouchEventArgs e)
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    m_startX = e.Event.GetX();
                    m_startY = e.Event.GetY();
                }
                else if (e.Event.Action == MotionEventActions.Up)
                {
                    float deltaX = e.Event.GetX() - m_startX;
                    float deltaY = e.Event.GetY() - m_startY;

                    if (m_direction == Direction.Left && deltaX < -1 * TOUCH_DELTA ||
                        m_direction == Direction.Right && deltaX > TOUCH_DELTA ||
                        m_direction == Direction.Up && deltaY < -1 * TOUCH_DELTA ||
                        m_direction == Direction.Down && deltaY > TOUCH_DELTA)
                    {
                        triggerAction(deltaX, deltaY);
                    }
                }
            }
            void triggerAction(float deltaX, float deltaY)
            {
                if (!string.IsNullOrWhiteSpace(m_action))
                {
                    UIVariable.GetAction(m_action, m_varName, m_direction.ToString());
                }
            }
        }
    }
    public class AddDragAndDropFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string strAction = Utils.GetSafeString(args, 1);

            OnTouchListener listener = new OnTouchListener(widget, strAction, script);
            widget.ViewX.Touch += listener.OnTouch;

            /*DragEventListener listener = new DragEventListener(view);
            view.SetOnDragListener(listener);
            view.LongClick += (sender, e) => {
                ClipData data = ClipData.NewPlainText("x", "X");
                View.DragShadowBuilder shadowBuilder = new View.DragShadowBuilder(view);
                view.StartDrag(data, shadowBuilder, null, 0);
            };*/

            return Variable.EmptyInstance;
        }
        public static void ReloadWidgets(ParsingScript script)
        {
            int tabId = MainActivity.CurrentTabId;
            UIUtils.Rect rect = new UIUtils.Rect(0, 0, 0, 0);
            string parentName = "";
            List<string> all = UIUtils.FindWidgets(rect, parentName, tabId, null);
            foreach (string widgetName in all)
            {
                ParserFunction func = ParserFunction.GetFunction(widgetName, script);
                DroidVariable widget = func.GetValue(script) as DroidVariable;
                Utils.CheckNotNull(widget, widgetName, script);
                View view = widget.ViewX;
                rect = new UIUtils.Rect((int)view.GetX(), (int)view.GetY(),
                                        view.Width, view.Height);
                UIUtils.RegisterWidget(widget.WidgetName, rect, parentName, tabId);
            }
        }
        protected class OnTouchListener
        {
            View m_view;
            DroidVariable m_variable;
            string m_widget;
            string m_action;
            ParsingScript m_script;
            float m_startX;
            float m_startY;

            const float TOUCH_DELTA = 5;

            public OnTouchListener(DroidVariable viewVar, string strAction, ParsingScript script)
            {
                m_view = viewVar.ViewX;
                m_widget = viewVar.WidgetName;
                m_variable = viewVar;
                m_action = strAction;
                m_script = script;
            }
            public void OnTouch(object sender, View.TouchEventArgs e)
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    m_startX = e.Event.GetX();
                    m_startY = e.Event.GetY();
                }
                else
                {
                    float deltaX = e.Event.GetX() - m_startX;
                    float deltaY = e.Event.GetY() - m_startY;
                    if (Math.Abs(deltaX) > TOUCH_DELTA || Math.Abs(deltaY) > TOUCH_DELTA)
                    {
                        //Console.WriteLine(e.Event.Action + ": " + deltaX + ", " + deltaY);
                        m_view.SetX(m_view.GetX() + deltaX);
                        m_view.SetY(m_view.GetY() + deltaY);

                        m_startX = e.Event.GetX();
                        m_startY = e.Event.GetY();

                        if (e.Event.Action == MotionEventActions.Up)
                        {
                            ReloadWidgets(m_script);
                            var parent = m_variable.Location.ParentView;
                            string parentName = parent == null ? "" : parent.WidgetName;
                            int tabId = MainActivity.CurrentTabId;
                            UIUtils.Rect rect = new UIUtils.Rect((int)m_view.GetX(), (int)m_view.GetY(),
                                                                 m_view.Width, m_view.Height);
                            UIUtils.RegisterWidget(m_widget, rect, parentName, tabId);
                            if (!string.IsNullOrWhiteSpace(m_action))
                            {

                                List<string> involved = UIUtils.FindWidgets(rect, parentName, tabId, m_widget);
                                string arg = string.Join(", ", involved);
                                UIVariable.GetAction(m_action, m_widget, arg);
                            }
                        }
                    }
                }
            }
        }
        // Not used, needs more tweaks.
        protected class DragEventListener : Java.Lang.Object, View.IOnDragListener
        {
            View m_view;
            float m_endX;
            float m_endY;
            bool m_started;

            public DragEventListener(View view)
            {
                m_view = view;
            }
            public bool OnDrag(View view, DragEvent ev)
            {
                DragAction action = ev.Action;
                bool handled = false;

                if (ev.GetX() != 0 || ev.GetY() != 0)
                {
                    m_endX = ev.GetX();
                    m_endY = ev.GetY();
                    Console.WriteLine("{0}: {1} {2} -- {3} {4}", action, m_endX, m_endY,
                                       m_view.GetX(), m_view.GetY());
                }
                m_view.Visibility = ViewStates.Visible;
                switch (action)
                {
                    case DragAction.Started:
                        if (ev.ClipDescription.HasMimeType(ClipDescription.MimetypeTextPlain))
                        {
                            handled = true;
                        }
                        break;
                    case DragAction.Entered:
                    case DragAction.Ended:
                        handled = true;
                        break;
                    case DragAction.Exited:
                        handled = true;
                        break;
                    case DragAction.Drop:
                        handled = true;
                        break;
                    case DragAction.Location:
                        handled = true;
                        break;
                }

                if (handled)
                {
                    m_view.Visibility = ViewStates.Visible;
                    m_view.SetX(m_endX);
                    m_view.SetY(m_endY);
                    view.Invalidate();
                }
                else if (!m_started)
                {
                    view.Visibility = ViewStates.Invisible;
                    m_started = true;
                }

                return handled;
            }
        }
    }
    public class SetTextFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string strTitle = args[1].AsString();
            string alignment = Utils.GetSafeString(args, 2, "left");

            bool isSet = widget.SetText(strTitle, alignment);

            return new Variable(isSet ? 1 : 0);
        }
    }
    public class GetTextFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string text = widget.GetText();

            return new Variable(text);
        }
    }
    public class SetValueFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            Variable arg1 = Utils.GetSafeVariable(args, 1);
            Variable arg2 = Utils.GetSafeVariable(args, 2);

            bool isSet = widget.SetValue(arg1.AsString(), arg2 == null ? "" : arg2.AsString());

            return new Variable(isSet ? 1 : 0);
        }
    }
    public class GetValueFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            double result = widget.GetValue();
            return new Variable(result);
        }
    }

    public class AlignTitleFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string alignment = args[1].AsString();
            bool aligned = widget.AlignText(alignment);

            return new Variable(aligned);
        }
    }
    public class SetOptionsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 0, m_name);
            // TODO: implement
            return Variable.EmptyInstance;
        }
    }
    public class SetFontSizeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            float fontSize = (float)args[1].AsDouble();

            bool autoResize = Utils.GetSafeInt(args, 2, 1) == 1;
            if (autoResize)
            {
                fontSize = AutoScaleFunction.ConvertFontSize(fontSize, UtilsDroid.GetScreenSize().Width);
            }

            widget.SetFontSize(fontSize);

            return Variable.EmptyInstance;
        }
    }
    public class SetFontFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string fontName = args[1].AsString();
            double fontSize = Utils.GetSafeDouble(args, 2);

            bool isSet = widget.SetFont(fontName, fontSize);

            return new Variable(isSet ? 1 : 0);
        }
    }
    public class SetFontTypeFunction : ParserFunction
    {
        public enum FontType { BOLD, ITALIC, NORMAL }
        FontType m_fontType;
        public SetFontTypeFunction(FontType fontType)
        {
            m_fontType = fontType;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            double fontSize = Utils.GetSafeDouble(args, 1);

            bool isSet = false;
            switch (m_fontType)
            {
                case FontType.NORMAL:
                    isSet = widget.SetNormalFont(fontSize);
                    break;
                case FontType.BOLD:
                    isSet = widget.SetBold(fontSize);
                    break;
                case FontType.ITALIC:
                    isSet = widget.SetItalic(fontSize);
                    break;
            }

            return new Variable(isSet ? 1 : 0);
        }
    }

    public class SetFontColorFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string colorStr = args[1].AsString();
            widget.SetFontColor(colorStr);

            return Variable.EmptyInstance;
        }
    }

    public class AddTextFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string text = Utils.ProcessString(args[1].AsString());
            string colorStr = Utils.GetSafeString(args, 2, "black").ToLower();
            double alpha = Utils.GetSafeDouble(args, 3, 1.0);
            widget.AddText(text, colorStr, alpha);

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

            DisplayMetrics bounds = new DisplayMetrics();
            var winManager = MainActivity.TheView.WindowManager;
            winManager.DefaultDisplay.GetMetrics(bounds);

            int width = bounds.WidthPixels < bounds.HeightPixels ?
                         bounds.WidthPixels : bounds.HeightPixels;
            int height = bounds.WidthPixels < bounds.HeightPixels ?
                         bounds.HeightPixels : bounds.WidthPixels;

            return new Variable(m_needWidth ? width : height);
        }
    }

    public class AllowedOrientationFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();

            string orientation = Utils.GetSafeString(args, 0).ToLower();

            ScreenOrientation orient = orientation == "landscape" ?
              ScreenOrientation.Landscape :
              ScreenOrientation.Portrait;

            MainActivity.TheView.RequestedOrientation = orient;
            return Variable.EmptyInstance;
        }
    }

    public class RegisterOrientationChangeFunction : ParserFunction
    {
        static string m_actionPortrait;
        static string m_actionLandscape;
        static string m_currentOrientation;

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            m_actionPortrait = Utils.GetSafeString(args, 0);
            m_actionLandscape = Utils.GetSafeString(args, 1);
            bool startNow = Utils.GetSafeInt(args, 2, 1) != 0;

            if (startNow)
            {
                PerformAction(MainActivity.Orientation, true);
            }

            MainActivity.OnOrientationChange += (newOrientation) =>
            {
                Console.WriteLine("New Orientation: {0} {1}", newOrientation, MainActivity.Orientation);
                DeviceRotated();
            };

            return Variable.EmptyInstance;
        }
        static void DeviceRotated()
        {
            string currentOrientation = MainActivity.Orientation;
            if (m_currentOrientation == currentOrientation)
            {
                Console.WriteLine("Same Orientation: {0}", currentOrientation);
                return;
            }

            PerformAction(currentOrientation);
        }
        static void PerformAction(string orientation, bool isInit = false)
        {
            m_currentOrientation = orientation;
            int currentTab = MainActivity.CurrentTabId;

            if (!isInit)
            {
                MainActivity.RemoveAll();
            }

            string action = orientation.Contains("Portrait") ?
                                        m_actionPortrait : m_actionLandscape;
            Console.WriteLine("PerformAction {0} Orient: {1} isInit={2}", action, m_currentOrientation, isInit);

            UIVariable.GetAction(action, "ROOT", (isInit ? "init" : m_currentOrientation));

            if (!isInit && currentTab >= 0)
            {
                MainActivity.TheView.ChangeTab(currentTab);
            }
        }
    }
    public class OrientationChangeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string strAction = args[0].AsString();

            MainActivity.OnOrientationChange += (newOrientation) =>
            {
                UIVariable.GetAction(strAction, "ROOT", newOrientation);
            };

            return Variable.EmptyInstance;
        }
    }

    public class OrientationFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            script.MoveForwardIf(Constants.END_ARG_ARRAY);

            var or = MainActivity.TheView.Resources.Configuration.Orientation.ToString();

            return new Variable(or);
        }
    }

    public class OnEnterBackgroundFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string strAction = args[0].AsString();

            MainActivity.OnEnterBackgroundDelegate += () =>
            {
                MainActivity.TheView.RunOnUiThread(() =>
                {
                    UIVariable.GetAction(strAction, "ROOT", "OnEnterBackground");
                });
            };

            return Variable.EmptyInstance;
        }
    }
    public class KillMeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            //System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());

            return Variable.EmptyInstance;
        }
    }
    public class ShowToastFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string msg = Utils.GetSafeString(args, 0);
            int duration = Utils.GetSafeInt(args, 1, 10);
            string fgColorStr = Utils.GetSafeString(args, 2);
            string bgColorStr = Utils.GetSafeString(args, 3);

            var ctx = Android.App.Application.Context;
            Toast toast = Toast.MakeText(ctx, msg,
                                         duration <= 3 ? ToastLength.Short : ToastLength.Long);

            if (args.Count > 2)
            {
                if (string.IsNullOrEmpty(fgColorStr))
                {
                    fgColorStr = "#FFFFFF";
                }
                if (string.IsNullOrEmpty(bgColorStr))
                {
                    bgColorStr = "#D3D3D3";
                }
                Color fgColor = UtilsDroid.String2Color(fgColorStr);
                Color bgColor = UtilsDroid.String2Color(bgColorStr);

                GradientDrawable shape = new GradientDrawable();
                shape.SetCornerRadius(20);
                shape.SetColor(bgColor);

                View toastView = toast.View;
                TextView toastText = (TextView)toastView.FindViewById(Android.Resource.Id.Message);
                toastText.SetTextColor(fgColor);
                toastView.Background = shape;
            }

            toast.Show();
 
            return Variable.EmptyInstance;
        }
    }
    public class ConvertColorFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            int r, g, b, a;

            string hex;
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);
            if (args.Count >= 3)
            {
                r = Utils.GetSafeInt(args, 0);
                g = Utils.GetSafeInt(args, 1);
                b = Utils.GetSafeInt(args, 2);
                hex = "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");

                return new Variable(hex);
            }
            hex = Utils.GetSafeString(args, 0);
            var color = UtilsDroid.String2Color(hex);

            r = (int)(color.R * 255);
            g = (int)(color.G * 255);
            b = (int)(color.B * 255);
            a = (int)(color.A * 255);

            Variable result = new Variable(Variable.VarType.ARRAY);
            result.AddVariable(new Variable(r));
            result.AddVariable(new Variable(g));
            result.AddVariable(new Variable(b));
            result.AddVariable(new Variable(a));

            return result;
        }
    }

    public class AlertDialogFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string title = Utils.GetSafeString(args, 0);
            string msg = Utils.GetSafeString(args, 1);
            string buttonOK = Utils.GetSafeString(args, 2, "Dismiss");
            string actionOK = Utils.GetSafeString(args, 3);
            string buttonCancel = Utils.GetSafeString(args, 4);
            string actionCancel = Utils.GetSafeString(args, 5);

            msg = msg.Replace(@"\\\\n", @"\n").Replace(@"\\\n", @"\n").Replace(@"\\n", @"\n").Replace(@"\\t", @"    ").Replace(@"\t", @"    ");

            var tokens = msg.Split("\\n");
            int maxSize = tokens[0].Trim().Length;
            for (int i = 1; i < tokens.Length; i++)
            {
                var currSize = tokens[i].Trim().Length;
                maxSize = currSize < maxSize ? maxSize : currSize;
            }
            string realMsg = "";
            for (int i = 0; i < tokens.Length; i++)
            {
                var current = tokens[i];
                //var missing = (maxSize - current.Length) / 2;
                var missing = maxSize - current.Length;
                var extra = missing > 0 ? new string(' ', missing) : "";
                realMsg += current + extra + (i < tokens.Length - 1 ? "\n" :"");
            }

            AlertDialog.Builder dialog = new AlertDialog.Builder(MainActivity.TheView);
            dialog.SetMessage(realMsg).
                   SetTitle(title);
            dialog.SetPositiveButton(buttonOK,
                (sender, e) =>
                {
                    dialog.Dispose();
                    if (!string.IsNullOrWhiteSpace(actionOK))
                    {
                        UIVariable.GetAction(actionOK, buttonOK, "1");
                    }
                });
            if (!string.IsNullOrWhiteSpace(buttonCancel))
            {
                dialog.SetNegativeButton(buttonCancel,
                    (sender, e) =>
                    {
                        dialog.Dispose();
                        if (!string.IsNullOrWhiteSpace(actionCancel))
                        {
                            UIVariable.GetAction(actionCancel, buttonCancel, "0");
                        }
                    });
            }

            dialog.Show();
            return Variable.EmptyInstance;
        }
    }
    public class SpeakFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string phrase = args[0].AsString();
            TTS.Voice = Utils.GetSafeString(args, 1, TTS.Voice).Replace("-", "_");
            bool force = Utils.GetSafeInt(args, 2) != 0;

            TTS tts = TTS.GetTTS(TTS.Voice);
            tts.Speak(phrase, force);

            return Variable.EmptyInstance;
        }
    }
    public class SpeechOptionsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            string option = args[0].AsString();
            Variable optionValue = Utils.GetSafeVariable(args, 1);
            Utils.CheckNotNull(optionValue, m_name, script);

            switch (option)
            {
                case "sound":
                    TTS.Sound = optionValue.AsInt() == 1;
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
        string m_action;
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            m_action = args[0].AsString();
            STT.Voice = Utils.GetSafeString(args, 1, STT.Voice).Replace("-", "_");
            string prompt = Utils.GetSafeString(args, 2);

            STT.VoiceRecognitionDone += OnVoiceResult;
            string init = STT.StartVoiceRecognition(prompt);
            if (init != null)
            {
                // An error in initializing:
                UIVariable.GetAction(m_action, init, "");
            }

            return Variable.EmptyInstance;
        }
        protected void OnVoiceResult(string status, string recognized)
        {
            UIVariable.GetAction(m_action, status, recognized);
        }
    }
    public class StopVoiceFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // No op at the moment.
            script.MoveForwardIf(Constants.END_ARG);
            return Variable.EmptyInstance;
        }
    }

    public class LocalizedFunction : ParserFunction
    {
        Dictionary<string, string> m_localizations = new Dictionary<string, string>();

        static Dictionary<string, Dictionary<string, string>> m_resources =
           new Dictionary<string, Dictionary<string, string>>();

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string currentCode = Localization.CurrentCode;

            string key = args[0].AsString();
            string langCode = Utils.GetSafeString(args, 1, currentCode);

            Dictionary<string, string> resourceCache;
            if (!m_resources.TryGetValue(langCode, out resourceCache))
            {
                resourceCache = new Dictionary<string, string>();
            }

            string localized;
            if (resourceCache.TryGetValue(key, out localized))
            {
                return new Variable(localized);
            }

            if (langCode != currentCode)
            {
                Localization.SetProgramLanguageCode(langCode);
            }

            localized = Localization.GetText(key);
            resourceCache[key] = localized;
            m_resources[langCode] = resourceCache;

            if (!string.IsNullOrWhiteSpace(langCode) &&
                !string.IsNullOrWhiteSpace(currentCode) &&
                langCode != currentCode)
            {
                Localization.SetProgramLanguageCode(currentCode);
            }
            return new Variable(localized);
        }
    }
    public class InitIAPFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            List<string> keyParts = new List<string>();
            for (int i = 0; i < args.Count; i++)
            {
                string keyPart = Utils.GetSafeString(args, i);
                keyParts.Add(keyPart);
            }

            //IAP.Init(keyParts);

            return Variable.EmptyInstance;
        }
    }
    public class InitTTSFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            TTS.Init(MainActivity.TheView);
            TTS.InitVoices();
            script.MoveForwardIf(Constants.END_ARG_ARRAY);
            return Variable.EmptyInstance;
        }
    }

    public class ReadFileFunction : ParserFunction
    {
        bool m_asString;
        public ReadFileFunction(bool asString = false)
        {
            m_asString = asString;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string filename = args[0].AsString();

            List<Variable> results = new List<Variable>();
            AssetManager assets = MainActivity.TheView.Assets;

            try
            {
                using (StreamReader sr = new StreamReader(assets.Open(filename)))
                {
                    if (m_asString)
                    {
                        string text = sr.ReadToEnd();
                        return new Variable(text);
                    }
                    while (!sr.EndOfStream)
                    {
                        Variable varLine = new Variable(sr.ReadLine());
                        results.Add(varLine);
                    }
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Couldn't read file [" + filename +
                                    "] from disk.");
            }
            return new Variable(results);
        }
    }
    class ImportFileFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string filename = args[0].AsString();

            string fileContents = "";
            try
            {
                fileContents = CommonFunctions.FileToString(filename);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception reading {0}: {1}", filename, exc.Message);
                AssetManager assets = MainActivity.TheView.Assets;
                using (StreamReader sr = new StreamReader(assets.Open(filename)))
                {
                    fileContents = sr.ReadToEnd();
                }
            }


            Variable result = RunScriptFunction.Execute(fileContents, filename);
            return result;
            }
    }
    public class PauseFunction : ParserFunction
    {
        static Dictionary<string, System.Timers.Timer> m_timers =
           new Dictionary<string, System.Timers.Timer>();

        bool m_startTimer;

        public PauseFunction(bool startTimer)
        {
            m_startTimer = startTimer;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();

            if (!m_startTimer)
            {
                Utils.CheckArgs(args.Count, 1, m_name);
                string cancelTimerId = Utils.GetSafeString(args, 0);
                System.Timers.Timer cancelTimer;
                if (m_timers.TryGetValue(cancelTimerId, out cancelTimer))
                {
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
            pauseTimer.Elapsed += (sender, e) =>
            {
                if (!autoReset)
                {
                    pauseTimer.Stop();
                    pauseTimer.Dispose();
                    m_timers.Remove(timerId);
                }
                //Console.WriteLine("QuizTimer_Elapsed {0:HH:mm:ss.fff}", e.SignalTime);
                MainActivity.TheView.RunOnUiThread(() =>
                {
                    UIVariable.GetAction(strAction, owner, timerId);
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
            script.MoveForwardIf(Constants.END_ARG);
            return new Variable(Localization.GetAppLanguageCode());
        }
    }
    public class SetAppLocale : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string code = args[0].AsString();

            bool found = Localization.SetProgramLanguageCode(code);
            return new Variable(found);
        }
    }
    public class OpenURLFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string urlStr = args[0].AsString();

            if (!urlStr.StartsWith("http"))
            {
                urlStr = "http://" + urlStr;
            }

            var url = Android.Net.Uri.Parse(urlStr);
            var intent = new Intent(Intent.ActionView, url);
            MainActivity.TheView.StartActivity(intent);

            return new Variable(urlStr);
        }
    }
    public class TranslateTabBar : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            MainActivity.TranslateTabs();
            script.MoveForwardIf(Constants.END_ARG);
            return Variable.EmptyInstance;
        }
    }
    public class GetSettingFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string settingName = args[0].AsString();
            string strType = Utils.GetSafeString(args, 1, "string");
            Variable defValue = Utils.GetSafeVariable(args, 2);

            switch (strType)
            {
                case "float":
                case "double":
                    float def = defValue == null ? -1 : (float)defValue.AsDouble();
                    float result = Settings.GetFloatSetting(settingName, def);
                    return new Variable(result);
                case "int":
                    int defInt = defValue == null ? -1 : defValue.AsInt();
                    int resultInt = Settings.GetIntSetting(settingName, defInt);
                    return new Variable(resultInt);
                case "long":
                    long defLong = defValue == null ? -1 : defValue.AsInt();
                    long resultLong = Settings.GetLongSetting(settingName, defLong);
                    return new Variable(resultLong);
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string settingName = args[0].AsString();
            Variable settingValue = Utils.GetSafeVariable(args, 1);
            string strType = Utils.GetSafeString(args, 2, "string");

            switch (strType)
            {
                case "float":
                case "double":
                    float setting = (float)settingValue.AsDouble();
                    Settings.SaveSetting(settingName, setting);
                    break;
                case "int":
                    int settingInt = settingValue.AsInt();
                    Settings.SaveSetting(settingName, settingInt);
                    break;
                case "long":
                    long settingLong = settingValue.AsLong();
                    Settings.SaveSetting(settingName, settingLong);
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
    public class SetStyleFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string strController = Utils.GetSafeString(args, 0);
            string styleName = Utils.GetSafeString(args, 1);
            string orientation = Utils.GetSafeString(args, 2);

            // todo: Main.SetController(strController, styleName, orientation);

            return Variable.EmptyInstance;
        }
    }
    public class RestoreFunction2 : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string strAction = args[0].AsString();

            /*for (int i = 1; i < args.Count; i++) {
              string productId = Utils.GetSafeString(args, i);
              //IAP.AddProductId(productId);
              InAppBilling.AddProductId(productId);
            }

            UnsubscribeFromAll();
            InAppBilling.OnIAPOK += (productIds) => {
              UIVariable.GetAction(strAction, "", "\"" + productIds + "\"");
            };
            InAppBilling.OnIAPError += (errorStr) => {
              UIVariable.GetAction(strAction, "\"" + errorStr + "\"", "");
              UnsubscribeFromAll();
            };
            //IAP.Restore();
            InAppBilling.Restore();*/

            return Variable.EmptyInstance;
        }
        public static void UnsubscribeFromAll()
        {
            /*Delegate[] clientList = InAppBilling.OnIAPOK?.GetInvocationList();
            if (clientList != null) {
              foreach (var d in clientList) {
                InAppBilling.OnIAPOK -= (System.Action<string>)d;
              }
            }
            clientList = InAppBilling.OnIAPError?.GetInvocationList();
            if (clientList != null) {
              foreach (var d in clientList) {
                InAppBilling.OnIAPError -= (System.Action<string>)d;
              }
            }*/
        }
    }
    public class PurchaseFunction2 : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            string strAction = args[0].AsString();
            string productId = args[1].AsString();

            RestoreFunction2.UnsubscribeFromAll();

            /*InAppBilling.OnIAPOK += (productIds) => {
              UIVariable.GetAction(strAction, "", "\"" + productIds + "\"");
            };
            InAppBilling.OnIAPError += (errorStr) => {
              UIVariable.GetAction(strAction, "\"" + errorStr + "\"", "");
              RestoreFunction.UnsubscribeFromAll();
            };
            InAppBilling.AddProductId(productId);
            InAppBilling.PurchaseItem(productId, productId);
      */
            return Variable.EmptyInstance;
        }
    }
    public class ProductIdDescriptionFunction2 : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string productId = args[0].AsString();

            //string description = IAP.GetDescription(productId);
            string description = "";//InAppBilling.GetDescription(productId);

            return new Variable(description);
        }
    }
    public class MakeSecureFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script);

            bool enable = Utils.GetSafeInt(args, 1, 1) == 1;
            bool isSet = widget.MakeSecure(enable);
            return new Variable(isSet);
        }
    }
    public class NumKeyboardFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            View view = widget.ViewX;
            Utils.CheckNotNull(view, m_name, script);

            bool isSet = false;

            if (view is TextView)
            {
                isSet = true;
                ((TextView)view).InputType = Android.Text.InputTypes.ClassNumber |
                    Android.Text.InputTypes.NumberFlagSigned |
                    Android.Text.InputTypes.NumberFlagDecimal;
            }

            return new Variable(isSet);
        }
    }

    public class EnableFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            DroidVariable widget = Utils.GetVariable(args[0].AsString(), script) as DroidVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            bool enable = Utils.GetSafeInt(args, 1, 1) == 1;
            bool isSet = widget.Enable(enable);

            return new Variable(isSet);
        }
    }
    public class SaveToPhotosFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 0, m_name, true);

            string productId = args[0].AsString();
            return Variable.EmptyInstance;
        }
    }
}
