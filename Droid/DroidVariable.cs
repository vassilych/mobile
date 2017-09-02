using System;
using Android.Util;
using Android.Views;
using Android.Widget;

using SplitAndMerge;


namespace scripting.Droid
{
    public class DroidVariable : UIVariable
    {
        public DroidVariable() {}

        public DroidVariable(UIType type, string name, View viewx = null, View viewy = null) :
                             base(type, name)
        {
            m_viewX = viewx;
            m_viewY = viewy;
            if (type != UIType.LOCATION && m_viewX != null) {
                m_viewX.Tag = ++m_currentTag;
                m_viewX.Id  = m_currentTag;
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
            newVar.m_viewX       = m_viewX;
            newVar.m_viewY       = m_viewY;
            newVar.m_layoutRuleX = m_layoutRuleX;
            newVar.m_layoutRuleY = m_layoutRuleY;
            newVar.m_viewLayout  = m_viewLayout;
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
            get { return m_viewLayout;  }
        }

        public void SetViewLayout(int width, int height)
        {
            DroidVariable refView = RefViewX as DroidVariable;
            m_viewLayout = MainActivity.CreateViewLayout(width, height, refView?.ViewLayout);
        }

        View        m_viewX;
        View        m_viewY;
        LayoutRules m_layoutRuleX;
        LayoutRules m_layoutRuleY;
        ViewGroup   m_viewLayout; // If this is a parent itself.

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
