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
        List<string> m_names;
        List<UIImage> m_pics;
        UIViewController m_controller;
        Dictionary<int, UIColor> m_row2color;

        int m_width = 240, m_height = 40;
        UIFont m_font = UIFont.SystemFontOfSize(15f);

        public int PicSize { get; set; } = 20;

        public UITextAlignment Alignment { get; set; } = UITextAlignment.Center;

        public UIColor TextColor { get; set; } = UIColor.Black;

        public int SelectedRow { get; protected set; }
        public string SelectedText
        {
            get
            {
                return SelectedRow >= 0 && m_names != null && SelectedRow < m_names.Count ? m_names[SelectedRow] : "";
            }
        }

        public int StringToRow(string text, int defaultRow = 0)
        {
            if (m_names == null)
            {
                return defaultRow;
            }
            int result = m_names.FindIndex((obj) => obj.Equals(text));
            if (result < 0)
            {
                text = text.Trim();
                result = m_names.FindIndex((obj) => obj.Equals(text));
            }
            return result < 0 ? defaultRow : result;
        }
        public string RowToString(int row)
        {
            return m_names != null && row >= 0 && row < m_names.Count ? m_names[row] : "";
        }

        public RowSelectedDel RowSelected;

        public TypePickerViewModel(UIViewController vc)
        {
            m_controller = vc;
        }

        public void SetColor(int row, UIColor color)
        {
            if (m_row2color == null)
            {
                m_row2color = new Dictionary<int, UIColor>();
            }
            m_row2color[row] = color;
        }

        public List<string> Data
        {
            set
            {
                m_names = value;
                nfloat maxSize = 0;
                for (int i = 0; i < m_names.Count; i++)
                {
                    if (m_names[i].Length > maxSize)
                    {
                        maxSize = m_names[i].Length;
                    }
                }
                var fontSize = CalculateFontSize(maxSize);
                SetFontSize(fontSize);
            }
            get { return m_names; }
        }
        public List<UIImage> Images
        {
            set { m_pics = value; }
            get { return m_pics; }
        }

        public static nfloat CalculateFontSize(nfloat size)
        {
            nfloat fontSize = 15f;
            if (size > 60)
            {
                fontSize = 10f;
            }
            else if (size > 50)
            {
                fontSize = 11f;
            }
            else if (size > 40)
            {
                fontSize = 12f;
            }
            else if (size > 30)
            {
                fontSize = 13f;
            }
            else if (size > 25)
            {
                fontSize = 14f;
            }
            return fontSize;
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }
        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            if (m_names != null)
            {
                return m_names.Count;
            }
            if (m_pics != null)
            {
                return m_pics.Count;
            }
            return 0;
        }

        public void SetSize(int width, int height)
        {
            if (width > 0)
            {
                m_width = width;
            }
            if (m_height > 0)
            {
                m_height = height;
            }
        }
        public UIFont GetFont()
        {
            return m_font;
        }
        public void SetFont(UIFont font)
        {
            m_font = font;
        }
        public void SetFontSize(nfloat fontSize)
        {
            m_font = m_font.WithSize(fontSize);
        }

        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            if (view == null)
            {
                CGSize rowSize = pickerView.RowSizeForComponent(component);
                view = new UIView(new CGRect(0, 0, rowSize.Width, rowSize.Height - 10));
                int deltaX = -1;

                if (m_pics != null)
                {
                    deltaX = PicSize;
                    var xamImageView = new UIImageView(new CGRect(0, 2, deltaX, deltaX));
                    xamImageView.Image = m_pics[(int)row];
                    view.AddSubview(xamImageView);
                }
                if (m_names != null)
                {
                    var textWidth = deltaX > 0 ? rowSize.Width - deltaX + 5 : rowSize.Width;
                    var textHeight = rowSize.Height;
                    var textEdit = new UITextView(new CGRect(deltaX + 1, 0, textWidth, textHeight));
                    textEdit.Editable = false;

                    UIColor color;
                    if (m_row2color == null || !m_row2color.TryGetValue((int)row, out color))
                    {
                        color = TextColor;
                    }
                    textEdit.TextColor = color;
                    textEdit.BackgroundColor = UIColor.Clear;
                    textEdit.Text = m_names[(int)row];
                    textEdit.Font = m_font;
                    textEdit.TextAlignment = Alignment;
                    textEdit.ContentInset = new UIEdgeInsets(-5, 0, 0, 0);
                    view.AddSubview(textEdit);
                }
            }
            return view;
        }
        /*public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
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
        }*/

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            SelectedRow = (int)row;
            pickerView.Select(row, component, false);

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
