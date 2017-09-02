using System;
using System.Collections.Generic;

using UIKit;
using CoreGraphics;

using SplitAndMerge;

namespace scripting.iOS
{
    public class iOSVariable : UIVariable
    {
        public iOSVariable() {}

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
            newVar.m_viewX        = m_viewX;
            newVar.m_viewY        = m_viewY;
            newVar.m_originalText = m_originalText;
            newVar.m_alignment    = m_alignment;
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

            SetTextFunction.SetText(m_viewX, text);
        }
        public string GetText()
        {
            if (m_viewX is UIButton) {
                return m_originalText;
            }
            return GetTextFunction.GetText(m_viewX);
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
