using System;
using System.Collections.Generic;

using Foundation;
using UIKit;
using CoreFoundation;
using ImageIO;
using CoreGraphics;

namespace scripting.iOS
{
    public delegate void RowSelectedDel(int row);

    public class TypePickerViewModel : UIPickerViewModel
    {
        List<string> m_names = null;
        UIViewController m_controller;

        int m_width = 240, m_height = 40;
        float m_fontSize = 15f;

        public int SelectedRow { get; protected set; }
        public string SelectedText { get {
                return m_names[SelectedRow];
            } }

        public int StringToRow(string text)
        {
            int result = m_names.FindIndex((obj) => obj.Equals(text));
            return result < 0 ? 0 : result;
        }
        public string RowToString(int row)
        {
            return row >= 0 && row < m_names.Count ? m_names[row] : "";
        }

        public RowSelectedDel RowSelected;

        public TypePickerViewModel(UIViewController vc, List<string> names)
        {
            m_controller = vc;
            m_names = names;
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }
        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return m_names.Count;
        }

        public void SetSize(int width, int height)
        {
            m_width    = width;
            m_height   = height;
        }
        public void SetFontSize(int fontSize)
        {
            m_fontSize = fontSize;
        }

        /*public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return m_names[(int)row];
        }*/
        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            //if (view == null) {
                CGSize rowSize = pickerView.RowSizeForComponent(component);
                UILabel lbl = new UILabel(new CGRect(new CGPoint(0, 0), rowSize));

                lbl.TextColor = UIColor.Black;
                lbl.Font = UIFont.SystemFontOfSize(m_fontSize);
                lbl.TextAlignment = UITextAlignment.Center;
                lbl.Text = m_names[(int)row];
                return lbl;
            //}
            //return view;
        }

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            SelectedRow = (int)row;
            RowSelected?.Invoke((int)row);
        }

        public override nfloat GetComponentWidth(UIPickerView pickerView, nint component)
        {
            //if (component == 0)   {
            //    return m_width;
            //} else {  return 40f; }
            return m_width;
        }

        public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
        {
            return m_height;
        }
    }
}
