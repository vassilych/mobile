using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using AdvancedColorPicker;
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
                int refWidth = UtilsiOS.GetAdjustedScreenWidth();
                AutoScaleFunction.TransformSizes(ref leftMargin, ref topMargin,
                                                 refWidth, multiplier);
            }

            Variable parentView = Utils.GetSafeVariable(args, 8, null);

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

            iOSVariable location = args[start] as iOSVariable;
            Utils.CheckNotNull(location, m_name, script);

            double screenRatio = UtilsiOS.GetScreenRatio();

            string varName = args[start + 1].AsString();
            varName = varName.Replace("\"", "");
            string config = Utils.GetSafeString(args, start + 2);
            int width = (int)(Utils.GetSafeInt(args, start + 3) / screenRatio);
            int height = (int)(Utils.GetSafeInt(args, start + 4) / screenRatio);

            bool autoResize = Utils.GetSafeInt(args, start + 5, 1) == 1;
            if (autoResize)
            {
                double multiplier = Utils.GetSafeDouble(args, start + 6);
                int refWidth = UtilsiOS.GetAdjustedScreenWidth();
                AutoScaleFunction.TransformSizes(ref width, ref height,
                                                 refWidth, multiplier);
            }

            UtilsiOS.AdjustSizes(widgetType, location, config, ref width, ref height);
            CGSize parentSize = location.GetParentSize();

            location.X = UtilsiOS.String2Position(location.RuleX, location.ViewX, location, parentSize, true);
            location.Y = UtilsiOS.String2Position(location.RuleY, location.ViewY, location, parentSize, false);

            location.X += location.TranslationX;
            location.Y += location.TranslationY;

            CGRect rect = new CGRect(location.X, location.Y, width, height);

            iOSVariable widgetFunc = ExistingWidget(script, varName);
            bool existing = widgetFunc != null;
            if (!existing)
            {
                widgetFunc = GetWidget(widgetType, varName, config, rect);
            }
            else
            {
                widgetFunc.ViewX.Frame = rect;
            }
            Utils.CheckNotNull(widgetFunc, m_name, script);

            //widgetFunc.CreateComplexView(rect, m_extras, config);

            var currView = location.GetParentView();
            currView.Add(widgetFunc.ViewX);

            widgetFunc.Location = location;

            iOSApp.AddView(widgetFunc);
            RegisterWidget(widgetFunc);

            return widgetFunc;
        }

        public static iOSVariable GetWidget(string widgetType, string widgetName, string initArg, CGRect rect)
        {
            for (int i = 0; i < UIVariable.WidgetTypes.Count; i++)
            {
                iOSVariable var = UIVariable.WidgetTypes[i] as iOSVariable;
                var widget = var.GetWidget(widgetType, widgetName, initArg, rect);
                if (widget != null)
                {
                    return widget;
                }
            }
            return null;
        }
        public static iOSVariable ExistingWidget(ParsingScript script, string varName)
        {
            ParserFunction func = ParserFunction.GetVariable(varName, script);
            if (func == null)
            {
                return null;
            }
            iOSVariable viewVar = func.GetValue(script) as iOSVariable;
            RemoveViewFunction.RemoveView(viewVar);
            return viewVar;
        }

        public static void RegisterWidget(iOSVariable widget)
        {
            var location = widget.Location;
            var parent = location.ParentView;
            string parentName = parent == null ? "" : parent.WidgetName;
            int tabId = iOSApp.Instance.SelectedTab;
            UIUtils.Rect rect = new UIUtils.Rect(location.X, location.Y, location.Width, location.Height);

            UIUtils.RegisterWidget(widget, rect, parentName, tabId);
        }

        string m_widgetType;
        string m_extras;
    }

    public class AddWidgetDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
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
            ParserFunction.UpdateFunction(widget.Name, new GetVarFunction(widget));

            return Variable.EmptyInstance;
        }
    }
    public class AddWidgetImagesFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            Variable data = Utils.GetSafeVariable(args, 1, null);
            Utils.CheckNotNull(data, m_name, script);
            Utils.CheckNotNull(data.Tuple, m_name, script);

            List<UIImage> images = new List<UIImage>(data.Tuple.Count);
            for (int i = 0; i < data.Tuple.Count; i++)
            {
                UIImage img = UtilsiOS.LoadImage(data.Tuple[i].AsString());
                images.Add(img);
            }

            string strAction = Utils.GetSafeString(args, 2);
            widget.AddImages(images, widget.Name, strAction);

            return Variable.EmptyInstance;
        }
    }

    public class ClearWidgetDataFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            widget.Clear();
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

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            int deltaX = args[1].AsInt();
            int deltaY = args[2].AsInt();

            bool autoResize = Utils.GetSafeInt(args, 3, 1) == 1;
            if (autoResize)
            {
                double multiplier = Utils.GetSafeDouble(args, 4);
                int refWidth = UtilsiOS.GetAdjustedScreenWidth();
                AutoScaleFunction.TransformSizes(ref deltaX, ref deltaY,
                                                 refWidth, multiplier);
            }

            UIView view = widget.ViewX;
            Utils.CheckNotNull(view, m_name, script);

            CGRect frame = view.Frame;
            if (deltaX < 0)
            {
                deltaX = (int)frame.X;
            }
            if (deltaY < 0)
            {
                deltaY = (int)frame.Y;
            }
            if (!m_isAbsolute)
            {
                frame.Offset(deltaX, deltaY);
            }
            else
            {
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            UIView view = widget.ViewX;
            Utils.CheckNotNull(view, m_name, script);

            int coord = 0;
            CGRect frame = view.Frame;
            if (m_isX)
            {
                coord = (int)view.Frame.X;
            }
            else
            {
                coord = (int)view.Frame.Y;
            }

            return new Variable(coord);
        }
    }
    public class SetTextFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            bool checkNull = Utils.GetSafeInt(args, 3, 1) == 1;

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            if (checkNull)
            {
                Utils.CheckNotNull(widget, m_name, script, 0);
            }
            else if (widget == null)
            {
                return Variable.EmptyInstance;
            }

            string strTitle = args[1].AsString();
            string alignment = Utils.GetSafeString(args, 2);

            bool isSet = widget.SetText(strTitle, alignment);

            return new Variable(isSet ? 1 : 0);
        }

        public static bool SetText(iOSVariable iosVar, string text)
        {
            return iosVar.SetText(text);
        }
    }
    public class GetTextFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            return new Variable(widget.GetText());
        }
        public static string GetText(iOSVariable iosVar)
        {
            return iosVar.GetText();
        }
    }
    public class SetValueFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
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

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            double result = GetValue(widget);

            return new Variable(result);
        }
        public static double GetValue(iOSVariable iosVar)
        {
            return iosVar.GetValue();
        }
    }
    public class AlignTitleFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            UIView view = widget.ViewX;
            Utils.CheckNotNull(view, m_name, script);

            string alignment = args[1].AsString();
            bool isSet = widget.AlignText(alignment);
            widget.SetText(GetTextFunction.GetText(widget), alignment);

            return new Variable(isSet);
        }
    }
    public class MakeSecureFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            UIView view = widget.ViewX;
            Utils.CheckNotNull(view, m_name, script);

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

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            UIView view = widget.ViewX;
            Utils.CheckNotNull(view, m_name, script);

            bool isSet = false;

            if (view is UITextField)
            {
                ((UITextField)view).KeyboardType = UIKeyboardType.NumberPad; ;
                isSet = true;
            }

            return new Variable(isSet);
        }
    }

    public class EnableFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            bool checkNull = Utils.GetSafeInt(args, 2, 1) == 1;

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            bool enable = Utils.GetSafeInt(args, 1, 1) == 1;
            bool isSet = widget.Enable(enable);

            return new Variable(isSet);
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

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            bool show = Utils.GetSafeInt(args, 1, m_show ? 1 : 0) != 0;
            iOSApp.ShowView(widget, show, false);

            if (widget.ViewX is UITextField)
            {
                UITextField findText = widget.ViewX as UITextField;
                findText.EndEditing(!m_show);
                if (show)
                {
                    findText.BecomeFirstResponder();
                }
                else
                {
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            iOSApp.RemoveView(widget);
            UIUtils.DeregisterWidget(widget.Name);

            return Variable.EmptyInstance;
        }
        public static void RemoveView(iOSVariable viewVar)
        {
            if (viewVar == null || viewVar.ViewX == null)
            {
                return;
            }
            viewVar.ViewX.RemoveFromSuperview();
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

            var widget = Utils.GetVariable(varName, script) as iOSVariable;
            if (widget == null)
            {
                return new Variable(false);
            }

            iOSApp.RemoveView(widget);
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
                iOSApp.RemoveTabViews(tabId);
                UIUtils.DeregisterTab(tabId);
            }
            else
            {
                iOSApp.RemoveAll();
                UIUtils.DeregisterAll();
            }

            return Variable.EmptyInstance;
        }
    }

    public class ShowHideKeyboardFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            bool showKeyboard = Utils.GetSafeInt(args, 1, 1) == 1;

            if (widget.ViewX is UITextView)
            {
                if (showKeyboard)
                {
                    ((UITextView)widget.ViewX).BecomeFirstResponder();
                }
                else
                {
                    ((UITextView)widget.ViewX).ResignFirstResponder();
                }
            }
            else if (widget.ViewX is UITextField)
            {
                if (showKeyboard)
                {
                    ((UITextField)widget.ViewX).BecomeFirstResponder();
                }
                else
                {
                    ((UITextField)widget.ViewX).ResignFirstResponder();
                }
            }
            else if (!showKeyboard)
            {
                widget.ViewX.EndEditing(true);
            }

            return Variable.EmptyInstance;
        }
    }
    public class IsKeyboardFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            bool result = false;

            if (widget.ViewX is UITextView)
            {
                result = ((UITextView)widget.ViewX).IsFirstResponder;
            }
            else if (widget.ViewX is UITextField)
            {
                result = ((UITextField)widget.ViewX).IsFirstResponder;
            }

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
            bool useVariable = Utils.GetSafeInt(args, 2, 0) == 1;

            UIImage img = null;
            string imageName = imageStr;
            if (useVariable)
            {
                Variable imageVar = Utils.GetVariable(imageStr, script);
                img = imageVar == null ? null : imageVar.Object as UIImage;
            }
            if (img == null)
            {
                imageName = UIUtils.String2ImageName(imageStr);
                img = UtilsiOS.LoadImage(imageName);
            }

            UIView view = iOSVariable.GetView(varName, script);
            if (view is UIButton)
            {
                ((UIButton)view).SetBackgroundImage(img, UIControlState.Normal);
            }
            else if (view is UIImageView)
            {
                ((UIImageView)view).Image = img;
            }
            else
            {
                view.BackgroundColor = new UIColor(img);
            }

            return img != null ? new Variable(imageName) : Variable.EmptyInstance;
        }
    }

    public class SaveToPhotosFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string imageStr = args[0].AsString();
            string filename = args[1].AsString();
            bool useVariable = Utils.GetSafeInt(args, 2, 1) == 1;

            UIImage image = null;
            string imageName = imageStr;
            if (useVariable)
            {
                Variable imageVar = Utils.GetVariable(imageStr, script);
                image = imageVar == null ? null : imageVar.Object as UIKit.UIImage;
            }
            if (image == null)
            {
                imageName = UIUtils.String2ImageName(imageStr);
                image = UtilsiOS.LoadImage(imageName);
            }

            ManualResetEvent mre = new ManualResetEvent(false);
            bool saved = true;

            image.SaveToPhotosAlbum((sender, error) => {
                if (error != null)
                {
                    Console.WriteLine("Saving pic error: " + error);
                }
                saved = (error == null);
                mre.Set();
            });

            mre.WaitOne(100);
            return new Variable(saved);
        }
    }

    public class SetBackgroundColorFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            if (args.Count == 1)
            {
                string rootColor = Utils.GetSafeString(args, 0);
                AppDelegate.SetBgColor(rootColor);
                return Variable.EmptyInstance;
            }

            string varName = args[0].AsString();
            string strColor = Utils.GetSafeString(args, 1);
            double alpha = Utils.GetSafeDouble(args, 2, 1.0);

            if (varName.Equals("ROOT"))
            {
                AppDelegate.SetBgColor(strColor, alpha);
            }
            else
            {
                iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
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

            string imageName = args[0].AsString();

            UIImage img = UtilsiOS.LoadImage(imageName);

            AppDelegate.SetBgView(img);
            return Variable.EmptyInstance;
        }
    }

    public class AddActionFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string strAction = Utils.GetSafeString(args, 1);
            string argument = Utils.GetSafeString(args, 2);

            widget.AddAction(widget.Name, strAction, argument);
            ParserFunction.AddGlobal(widget.Name, new GetVarFunction(widget));

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

            UIView view = iOSVariable.GetView(varName, script);
            Utils.CheckNotNull(view, m_name, script);

            var gr = new UILongPressGestureRecognizer();
            gr.AddTarget(() => this.ButtonLongPressed(gr, strAction, varName));

            view.AddGestureRecognizer(gr);

            return Variable.EmptyInstance;
        }

        void ButtonLongPressed(UILongPressGestureRecognizer gr,
                               string strAction, string varName)
        {
            if (gr.State != UIGestureRecognizerState.Began)
            {
                return;
            }
            UIVariable.GetAction(strAction, varName, "");
        }
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

            UIView view = iOSVariable.GetView(varName, script);
            Utils.CheckNotNull(view, m_name, script);

            UISwipeGestureRecognizerDirection dir = UISwipeGestureRecognizerDirection.Left;
            switch (direction)
            {
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
            UIVariable.GetAction(strAction, varName, sender.Direction.ToString());
        }
    }
    public class AddDragAndDropFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            string strAction = Utils.GetSafeString(args, 1);

            UIPanGestureRecognizer gesture = new UIPanGestureRecognizer();
            DragHandler handler = new DragHandler(widget, strAction);
            // Wire up the event handler (have to use a selector)
            gesture.AddTarget(() => handler.HandleDrag(gesture));
            widget.ViewX.AddGestureRecognizer(gesture);

            return Variable.EmptyInstance;
        }

        public class DragHandler
        {
            UIView m_view;
            CGRect m_originalFrame;
            iOSVariable m_variable;
            string m_widget;
            string m_action;

            const float TOUCH_DELTA = 5;

            public DragHandler(iOSVariable viewVar, string strAction)
            {
                m_view = viewVar.ViewX;
                m_originalFrame = m_view.Frame;
                m_widget = viewVar.WidgetName;
                m_variable = viewVar;
                m_action = strAction;
            }
            public void HandleDrag(UIPanGestureRecognizer recognizer)
            {
                if (recognizer.State == UIGestureRecognizerState.Cancelled ||
                    recognizer.State == UIGestureRecognizerState.Failed ||
                    recognizer.State == UIGestureRecognizerState.Possible)
                {
                    return;
                }
                if (recognizer.State == UIGestureRecognizerState.Began)
                {
                    m_originalFrame = m_view.Frame;
                    return;
                }

                CGPoint offset = recognizer.TranslationInView(m_view);

                CGRect newFrame = m_originalFrame;
                newFrame.Offset(offset.X, offset.Y);

                m_view.Frame = newFrame;

                if (recognizer.State == UIGestureRecognizerState.Ended)
                {
                    var parent = m_variable.Location.ParentView;
                    string parentName = parent == null ? "" : parent.WidgetName;
                    int tabId = iOSApp.Instance.SelectedTab;
                    UIUtils.Rect rect = new UIUtils.Rect((int)newFrame.X, (int)newFrame.Y,
                                                         (int)newFrame.Width, (int)newFrame.Height);
                    UIUtils.RegisterWidget(m_widget, rect, parentName, tabId);
                    if (!string.IsNullOrWhiteSpace(m_action))
                    {

                        List<string> involved = UIUtils.FindWidgets(rect, parentName, tabId, m_widget);
                        string arg = string.Join(", ", involved);
                        //Console.WriteLine("Offsets: {0}, {1} -- {2}, {3} -- {4}, {5}", offset.X, offset.Y,
                        //                  m_originalFrame.X, m_originalFrame.Y, newFrame.X, newFrame.Y);
                        UIVariable.GetAction(m_action, m_widget, arg);
                    }
                }
            }
        }
    }
    public class AddBorderFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);
            UIView view = widget.ViewX;

            Utils.CheckNotNull(view, m_name, script);

            int width = Utils.GetSafeInt(args, 1, 1);
            int corner = Utils.GetSafeInt(args, 2, 5);
            string colorStr = Utils.GetSafeString(args, 3, "black");
            CGColor color = UtilsiOS.CGColorFromHex(colorStr);

            AddBorder(view, color, width, corner);

            return Variable.EmptyInstance;
        }
        public static void AddBorder(UIView view, CGColor borderColor = null, int width = 1, int corner = 5)
        {
            if (borderColor == null)
            {
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string text = Utils.GetSafeString(args, 0);
            string selectedImageName = Utils.GetSafeString(args, 1);
            string notSelectedImageName = Utils.GetSafeString(args, 2, selectedImageName);

            if (!m_forceCreate && iOSApp.SelectTab(text))
            {
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            int tabId = Utils.GetSafeInt(args, 0);
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string action = args[0].AsString();

            iOSApp.TabSelectedDelegate += (tab) =>
            {
                UIVariable.GetAction(action, "ROOT", tab.ToString());
            };
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

            double screenRatio = UtilsiOS.GetScreenRatio();
            double multiplier = UtilsiOS.WidthMultiplier();

            int width = (int)(Utils.GetSafeInt(args, 4, 540) / screenRatio * multiplier);
            int height = (int)(Utils.GetSafeInt(args, 5, 120) / screenRatio * multiplier);

            if (string.IsNullOrEmpty(fgColorStr))
            {
                fgColorStr = "#000000";
            }
            if (string.IsNullOrEmpty(bgColorStr))
            {
                bgColorStr = "#D3D3D3";
            }

            UIColor fgColor = UtilsiOS.String2Color(fgColorStr);
            UIColor bgColor = UtilsiOS.String2Color(bgColorStr);

            UtilsiOS.ShowToast(msg, fgColor, bgColor, duration, width, height);

            return Variable.EmptyInstance;
        }
    }
    public class ConvertColorFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            nfloat r, g, b, a;
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
            var color = UtilsiOS.String2Color(hex);
            color.GetRGBA(out r, out g, out b, out a);

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

            msg = msg.Replace("\\n", "\n");
            msg = msg.Replace("\\t", "\t");

            UIViewController controller = AppDelegate.GetCurrentController();

            var okCancelAlertController = UIAlertController.Create(title, msg, UIAlertControllerStyle.Alert);
            var okAction = UIAlertAction.Create(buttonOK, UIAlertActionStyle.Default,
                alert =>
                {
                    if (!string.IsNullOrWhiteSpace(actionOK))
                    {
                        UIVariable.GetAction(actionOK, buttonOK, "1");
                    }
                });
            okCancelAlertController.AddAction(okAction);

            if (!string.IsNullOrWhiteSpace(buttonCancel))
            {
                var cancelAction = UIAlertAction.Create(buttonCancel, UIAlertActionStyle.Cancel,
                    alert =>
                    {
                        if (!string.IsNullOrWhiteSpace(actionCancel))
                        {
                            UIVariable.GetAction(actionCancel, buttonCancel, "0");
                        }
                    });
                okCancelAlertController.AddAction(cancelAction);
            }

            controller.PresentViewController(okCancelAlertController, true, null);
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
            var initColor = UtilsiOS.String2Color(Utils.GetSafeString(args, 1, "white"));
            m_action = Utils.GetSafeString(args, 2, "ColorPicked");

            UIViewController controller = AppDelegate.GetCurrentController();

            Action<UIColor> callback = new Action<UIColor>(OnColorSelected);
            ColorPickerViewController.Present(controller, title, initColor, callback);

            return Variable.EmptyInstance;
        }

        void OnColorSelected(UIColor color)
        {
            color.GetRGBA(out nfloat rfl, out nfloat gfl, out nfloat bfl, out _);
            int r = rfl <= 1 && rfl > 0 ? (int)(255 * rfl) : (int)rfl;
            int g = gfl <= 1 && gfl > 0 ? (int)(255 * gfl) : (int)gfl;
            int b = bfl <= 1 && bfl > 0 ? (int)(255 * bfl) : (int)bfl;
            var hex = "#" + string.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
            UIVariable.GetAction(m_action, "", hex);
        }
    }
    public class SetSizeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string varName = Utils.GetSafeString(args, 0);
            Utils.CheckNotEmpty(script, varName, m_name);

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
                int refWidth = UtilsiOS.GetAdjustedScreenWidth();
                AutoScaleFunction.TransformSizes(ref width, ref height,
                                                 refWidth, multiplier);
            }
            UIView view = iOSVariable.GetView(varName, script);

            if (view is UIPickerView)
            {
                UIPickerView pickerView = view as UIPickerView;
                TypePickerViewModel model = pickerView.Model as TypePickerViewModel;
                if (model != null)
                {
                    model.SetSize(width, height);
                    pickerView.Model = model;
                }
            }
            else
            {
                CGRect newFrame = view.Frame;
                newFrame.Size = new CGSize(width, height);
                view.Frame = newFrame;
            }

            return Variable.EmptyInstance;
        }
    }
    public class SetOptionsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            UIView view = widget.ViewX;

            string option = args[1].AsString().ToLower();
            string arg = Utils.GetSafeString(args, 2);
            switch (option)
            {
                case "multiline":
                    SetMultiline(view);
                    break;
                case "ontouch":
                    SetOnTouch(view, arg);
                    break;
                case "ontouchimage":
                    UIImage img = UtilsiOS.LoadImage(arg); 
                    SetOnTouch(view, img);
                    break;
            }
            return widget;
        }
        public static void SetMultiline(UIView view, bool multiline = true)
        {
            UILabel label = null;
            if (view is UIButton)
            {
                UIButton button = (UIButton)view;
                label = button.TitleLabel;
            }
            else if (view is UILabel)
            {
                label = (UILabel)view;
            }

            if (label == null)
            {
                return;
            }
            label.Lines = multiline ? 25 : 1;
        }
        public static void SetOnTouch(UIView view, string title)
        {
            if (!(view is UIButton))
            {
                return;
            }

            UIButton button = (UIButton)view;
            button.SetTitle(title, UIControlState.Highlighted);
        }
        public static void SetOnTouch(UIView view, UIImage img)
        {
            if (!(view is UIButton))
            {
                return;
            }

            UIButton button = (UIButton)view;
            button.SetImage(img, UIControlState.Highlighted);
        }
    }
    public class SetFontSizeFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            double fontSize = Utils.GetSafeDouble(args, 1);

            bool autoResize = Utils.GetSafeInt(args, 2, 1) == 1;
            if (autoResize)
            {
                int refWidth = UtilsiOS.GetAdjustedScreenWidth();
                fontSize = AutoScaleFunction.ConvertFontSize((float)fontSize,
                                                             refWidth);
            }

            bool isSet = widget.SetFontSize(fontSize);

            return new Variable(isSet ? 1 : 0);
        }
    }
    public class SetFontFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
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
            Utils.CheckArgs(args.Count, 1, m_name);

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
            Utils.CheckNotNull(widget, m_name, script, 0);

            double fontSize = Utils.GetSafeDouble(args, 1,0);

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

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
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

            iOSVariable widget = Utils.GetVariable(args[0].AsString(), script) as iOSVariable;
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

            var bounds = UtilsiOS.GetNativeScreenSize();
            return new Variable(m_needWidth ? bounds.Width : bounds.Height);
        }
    }

    public class AllowedOrientationFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            m_actionPortrait = Utils.GetSafeString(args, 0);
            m_actionLandscape = Utils.GetSafeString(args, 1);
            bool startNow = Utils.GetSafeInt(args, 2, 1) != 0;

            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
            NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);

            if (startNow)
            {
                PerformAction(iOSApp.Orientation, true);
            }

            return Variable.EmptyInstance;
        }
        static void DeviceRotated(NSNotification notification)
        {
            string currentOrientation = iOSApp.Orientation;
            //Console.WriteLine("DeviceRotated {0} --> {1} before: {2}",
            //  UIDevice.CurrentDevice.Orientation, currentOrientation, m_currentOrientation);
            if (!iOSApp.ValidOrientation || m_currentOrientation == currentOrientation)
            {
                return;
            }

            PerformAction(currentOrientation);
        }
        static void PerformAction(string orientation, bool isInit = false)
        {
            m_currentOrientation = orientation;
            int currentTab = iOSApp.Instance.SelectedTab;

            if (!isInit)
            {
                iOSApp.RemoveAll();
            }

            //string action = iOSApp.IsLandscape ? m_actionLandscape : m_actionPortrait;
            string action = orientation.Contains("Portrait") ?
                                        m_actionPortrait : m_actionLandscape;
            iOSApp.Instance.OffsetTabBar(false);

            UIVariable.GetAction(action, "ROOT", (isInit ? "init" : m_currentOrientation));

            if (!isInit && currentTab >= 0)
            {
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            m_action = args[0].AsString();

            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
            NSNotificationCenter.DefaultCenter.AddObserver(
                new NSString("UIDeviceOrientationDidChangeNotification"), DeviceRotated);

            return Variable.EmptyInstance;
        }
        void DeviceRotated(NSNotification notification)
        {
            UIDeviceOrientation currentOrientation = UIDevice.CurrentDevice.Orientation;
            if (m_currentOrientation == currentOrientation)
            {
                return;
            }
            m_currentOrientation = currentOrientation;
            iOSApp.Instance.OffsetTabBar(false);
            UIVariable.GetAction(m_action, "ROOT", m_currentOrientation.ToString());
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string strAction = args[0].AsString();

            AppDelegate.OnEnterBackgroundDelegate += () =>
            {
                AppDelegate.GetCurrentController().InvokeOnMainThread(() =>
                {
                    UIVariable.GetAction(strAction, "ROOT", "OnEnterBackground");
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            TTS.Init();

            string phrase = args[0].AsString();
            TTS.Voice = Utils.GetSafeString(args, 1, TTS.Voice);
            bool force = Utils.GetSafeInt(args, 2) != 0;

            TTS.Speak(phrase, force);

            return Variable.EmptyInstance;
        }
    }
    public class SpeechOptionsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            TTS.Init();

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
        static STT m_speech = null;
        public static STT LastRecording { get { return m_speech; } }

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string strAction = args[0].AsString();
            STT.Voice = Utils.GetSafeString(args, 1, STT.Voice).Replace('_', '-');

            bool speechEnabled = UIDevice.CurrentDevice.CheckSystemVersion(10, 0);
            if (!speechEnabled)
            {
                UIVariable.GetAction(strAction,
                 string.Format("Speech recognition requires iOS 10.0 or higher. You have iOS {0}",
                               UIDevice.CurrentDevice.SystemVersion), "");
                return Variable.EmptyInstance;
            }

            if (!STT.Init())
            {
                // The user didn't authorize accessing the microphone.
                return Variable.EmptyInstance;
            }

            UIViewController controller = AppDelegate.GetCurrentController();
            m_speech = new STT(controller);
            m_speech.OnSpeechError += (errorStr) =>
            {
                Console.WriteLine(errorStr);
                controller.InvokeOnMainThread(() =>
                {
                    UIVariable.GetAction(strAction, errorStr, "");
                });
            };
            m_speech.OnSpeechOK += (recognized) =>
            {
                Console.WriteLine("Recognized: " + recognized);
                controller.InvokeOnMainThread(() =>
                {
                    UIVariable.GetAction(strAction, "", recognized);
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string key = args[0].AsString();
            string langCode = Utils.GetSafeString(args, 1);
            string currentCode = Localization.CurrentCode;
            string localized;

            if (!string.IsNullOrWhiteSpace(langCode))
            {
                if (m_localizations.TryGetValue(langCode + key, out localized))
                {
                    return new Variable(localized);
                }
                if (langCode != currentCode)
                {
                    Localization.SetProgramLanguageCode(langCode);
                }
            }

            localized = Localization.GetText(key);
            m_localizations[langCode + key] = localized;

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
            // Just consume the arguments: not needed for iOS:
            List<Variable> args = script.GetFunctionArgs();
            IAP.Init();

            return Variable.EmptyInstance;
        }
    }
    public class RestoreFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);
            string strAction = args[0].AsString();

            for (int i = 1; i < args.Count; i++)
            {
                string productId = Utils.GetSafeString(args, i);
                IAP.AddProductId(productId);
            }

            IAP.IAPOK += (productIds) =>
            {
                UIVariable.GetAction(strAction, "", productIds);
            };
            IAP.IAPError += (errorStr) =>
            {
                UIVariable.GetAction(strAction, errorStr, "");
            };

            IAP.Restore();

            return Variable.EmptyInstance;
        }
    }
    public class PurchaseFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            string strAction = args[0].AsString();
            string productId = args[1].AsString();

            IAP.AddProductId(productId);

            IAP.IAPOK += (productIds) =>
            {
                UIVariable.GetAction(strAction, "", productIds);
            };
            IAP.IAPError += (errorStr) =>
            {
                UIVariable.GetAction(strAction, errorStr, "");
            };

            IAP.Purchase(productId);

            return Variable.EmptyInstance;
        }
    }
    public class ProductIdDescriptionFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string productId = args[0].AsString();

            string description = IAP.GetDescription(productId);

            return new Variable(description);
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

            if (m_asString)
            {
                string text = Utils.GetFileText(filename);
                return new Variable(text);
            }

            string[] lines = Utils.GetFileLines(filename);
            List<Variable> results = Utils.ConvertToResults(lines);
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

            string fileContents = Utils.GetFileText(filename);

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
                AppDelegate.GetCurrentController().InvokeOnMainThread(() =>
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
            string code = Localization.GetDeviceLangCode();
            script.MoveForwardIf(Constants.END_ARG);
            return new Variable(code);
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

            if (!urlStr.StartsWith("http") && !urlStr.StartsWith("itms"))
            {
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string settingName = args[0].AsString();
            string strType = Utils.GetSafeString(args, 1, "string");
            Variable defValue = Utils.GetSafeVariable(args, 2);

            switch (strType)
            {
                case "float":
                    float def = defValue == null ? -1 : (float)defValue.AsDouble();
                    float result = Settings.GetFloatSetting(settingName, def);
                    return new Variable(result);
                case "double":
                    double defD = defValue == null ? -1 : defValue.AsDouble();
                    double resultD = Settings.GetDoubleSetting(settingName, defD);
                    return new Variable(resultD);
                case "int":
                case "long":
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
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            string settingName = args[0].AsString();
            Variable settingValue = Utils.GetSafeVariable(args, 1);
            string strType = Utils.GetSafeString(args, 2, "string");

            switch (strType)
            {
                case "float":
                    float setting = (float)settingValue.AsDouble();
                    Settings.SaveSetting(settingName, setting);
                    break;
                case "double":
                    double settingD = settingValue.AsDouble();
                    Settings.SaveSetting(settingName, settingD);
                    break;
                case "int":
                case "long":
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

    public class SetStyleFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string styleStr = Utils.GetSafeString(args, 0);
            string typeName = Utils.GetSafeString(args, 1);
            string orientation = Utils.GetSafeString(args, 2);

            AppDelegate.Style style = AppDelegate.Style.TABS;
            switch (styleStr)
            {
                case "tabs":
                    style = AppDelegate.Style.TABS;
                    break;
                case "navi":
                    style = AppDelegate.Style.NAVI;
                    break;
                case "page":
                    style = AppDelegate.Style.PAGE;
                    break;
            }
            AppDelegate.SetController(style, typeName, orientation);

            return Variable.EmptyInstance;
        }
    }
}
