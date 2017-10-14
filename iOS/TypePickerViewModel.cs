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
    List<UIImage> m_pics = null;
    UIViewController m_controller;

    int m_width = 240, m_height = 40;
    float m_fontSize = 15f;

    int m_picSize = 20;
    public int PicSize {
      set { m_picSize = value; }
      get { return m_picSize; }
    }

    UITextAlignment m_alignment = UITextAlignment.Center;
    public UITextAlignment Alignment {
      set { m_alignment = value; }
      get { return m_alignment; }
    }

    public int SelectedRow { get; protected set; }
    public string SelectedText {
      get {
        return m_names[SelectedRow];
      }
    }

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

    public TypePickerViewModel(UIViewController vc)
    {
      m_controller = vc;
    }

    public List<string> Data {
      set {
        m_names = value;
        for (int i = 0; i < m_names.Count; i++) {
          if (m_names[i].Length > 25) {
            m_fontSize = 10f;
            break;
          }
        }
      }
      get { return m_names; }
    }
    public List<UIImage> Images {
      set { m_pics = value; }
      get { return m_pics; }
    }

    public override nint GetComponentCount(UIPickerView pickerView)
    {
      return 1;
    }
    public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
    {
      if (m_names != null) {
        return m_names.Count;
      }
      if (m_pics != null) {
        return m_pics.Count;
      }
      return 0;
    }

    public void SetSize(int width, int height)
    {
      if (width > 0) {
        m_width = width;
      }
      if (m_height > 0) {
        m_height = height;
      }
    }
    public void SetFontSize(int fontSize)
    {
      m_fontSize = fontSize;
    }

    public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
    {
      if (view == null) {
        CGSize rowSize = pickerView.RowSizeForComponent(component);
        view = new UIView(new CGRect(0, 0, rowSize.Width, rowSize.Height - 10));
        int deltaX = -1;

        if (m_pics != null) {
          deltaX = m_picSize;
          var xamImageView = new UIImageView(new CGRect(0, 2, deltaX, deltaX));
          xamImageView.Image = m_pics[(int)row];
          view.AddSubview(xamImageView);
        }
        if (m_names != null) {
          var textWidth = deltaX > 0 ? rowSize.Width - deltaX + 5 : rowSize.Width;
          var textHeight = rowSize.Height;
          var textEdit = new UITextView(new CGRect(deltaX + 1, 0, textWidth, textHeight));
          textEdit.Editable = false;
          textEdit.TextColor = UIColor.Black;
          textEdit.BackgroundColor = UIColor.Clear;
          textEdit.Text = m_names[(int)row];
          textEdit.Font = UIFont.SystemFontOfSize(m_fontSize);
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
