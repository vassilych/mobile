using System;
using System.IO;
using Syncfusion.Drawing;
/*using Syncfusion.Pdf;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Interactive;

using SplitAndMerge;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if __ANDROID__
using scripting.Droid;
using BASE_VARIABLE = scripting.Droid.DroidVariable;
using UTILS = scripting.Droid.UtilsDroid;
#elif __IOS__
using scripting.iOS;
using BASE_VARIABLE = scripting.iOS.iOSVariable;
using UTILS = scripting.iOS.UtilsiOS;
#endif


namespace scripting
{
  public class SfPdf : BASE_VARIABLE
  {
    PdfDocument m_document;
    PdfLoadedDocument m_loadedDoc;
    PdfPage m_page;
    PdfGraphics m_grapics;
    PdfStandardFont m_font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);

    public SfPdf(bool doInit = false)
    {
      if (doInit) {
        Init();
      }
    }
    public override Variable Clone()
    {
      SfPdf newVar = (SfPdf)this.MemberwiseClone();
      return newVar;
    }
    public void Init()
    {
      if (m_document != null) {
        return;
      }
      m_document = new PdfDocument();
      m_page = m_document.Pages.Add();
      m_grapics = m_page.Graphics;
    }
    public void Open(string filename)
    {
      bool fileExists = File.Exists(filename);

      Stream fileStream = Utils.OpenFile(filename);
      fileStream.Position = 0;
      m_loadedDoc = new PdfLoadedDocument(fileStream);
    }

    public void SetFontOptions(string fontName, float newSize = 0, bool bold = false, bool italic = false)
    {
      fontName = fontName.ToLower();
      PdfFontFamily family = m_font.FontFamily;
      switch (fontName) {
        case "helvetica":
          family = PdfFontFamily.Helvetica; break;
        case "courier":
          family = PdfFontFamily.Courier; break;
        case "timesroman":
          family = PdfFontFamily.TimesRoman; break;
        case "symbol":
          family = PdfFontFamily.Symbol; break;
        case "zapfdingbats":
          family = PdfFontFamily.ZapfDingbats; break;
      }

      if (newSize == 0) {
        newSize = m_font.Size;
      }

      if (bold) {
        m_font = new PdfStandardFont(family, newSize, PdfFontStyle.Bold);
      } else if (italic) {
        m_font = new PdfStandardFont(family, newSize, PdfFontStyle.Italic);
      } else {
        m_font = new PdfStandardFont(family, newSize);
      }
    }
    public void AddText(string text, int x, int y, PdfBrush color)
    {
      m_grapics.DrawString(text, m_font, color, x, y);
    }

    public void AddImage(string imagePath, int x, int y, int w, int h)
    {
      Stream pngImageStream = UTILS.ImageToStream(imagePath);
      PdfImage pngImage = new PdfBitmap(pngImageStream);
      m_grapics.DrawImage(pngImage, x, y, w, h);
    }
    public void AddLine(int x1, int y1, int x2, int y2, PdfBrush color, float width)
    {
      PdfPen pen = new PdfPen(color, width);
      m_grapics.DrawLine(pen, x1, y1, x2, y2);
    }
    public void AddRectangle(int x, int y, int w, int h, PdfBrush color)
    {
      m_grapics.DrawRectangle(color, x, y, w, h);
    }
    public void AddPie(int x, int y, int w, int h, float startAngle,
                       float sweepAngle, PdfBrush color, float width)
    {
      PdfPen pen = new PdfPen(color, width);
      m_grapics.DrawPie(pen, x, y, w, h, startAngle, sweepAngle);
    }
    public void Save(string filename)
    {
      MemoryStream stream = new MemoryStream();
      if (m_loadedDoc != null) {
        m_loadedDoc.Save(stream);
      } else {
        Init();
        m_document.Save(stream);
        m_document.Close(true);
      }
      stream.Position = 0;

#if __ANDROID__
      SaveAndroid.Save(filename, "application/pdf", stream, MainActivity.TheView);
#elif __IOS__
      PreviewController.Save(filename, "application/pdf", stream);
#endif
      m_document = null;

      Utils.SaveFile(filename, stream);
    }
  }
  public class CreatePdf : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();

      SfPdf pdf = new SfPdf(true);

      return pdf;
    }
  }
  public class OpenPdf : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 1, m_name);

      string filename = Utils.GetSafeString(args, 0);

      SfPdf pdf = new SfPdf(true);
      pdf.Open(filename);

      return pdf;
    }
  }
  public class SavePdf : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 2, m_name);

      SfPdf pdf = args[0] as SfPdf;
      Utils.CheckNotNull(pdf, m_name);

      string filename = Utils.GetSafeString(args, 1);
      pdf.Save(filename);

      ParserFunction.UpdateFunction(pdf);
      return pdf;
    }
  }
  public class SetPdfText : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 3, m_name);

      SfPdf pdf = args[0] as SfPdf;
      Utils.CheckNotNull(pdf, m_name);

      string text = Utils.GetSafeString(args, 1);
      int x = Utils.GetSafeInt(args, 2);
      int y = Utils.GetSafeInt(args, 3);

      string colorStr = Utils.GetSafeString(args, 4, "black");
      PdfBrush color = SfUtils.String2PdfColor(colorStr);

      pdf.Init();
      pdf.AddText(text, x, y, color);

      ParserFunction.UpdateFunction(pdf);
      return pdf;
    }
  }
  public class SetPdfImage : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 3, m_name);

      SfPdf pdf = args[0] as SfPdf;
      Utils.CheckNotNull(pdf, m_name);

      string image = Utils.GetSafeString(args, 1);
      int x = Utils.GetSafeInt(args, 2);
      int y = Utils.GetSafeInt(args, 3);
      int w = Utils.GetSafeInt(args, 4);
      int h = Utils.GetSafeInt(args, 5);

      pdf.Init();
      pdf.AddImage(image, x, y, w, h);
      return pdf;
    }
  }
  public class SetPdfLine : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 3, m_name);

      SfPdf pdf = args[0] as SfPdf;
      Utils.CheckNotNull(pdf, m_name);

      //m_grapics.DrawLine(pen, x1, y1, x2, y2);
      //m_grapics.DrawRectangle(color, x, y, w, h);
      //m_grapics.DrawPie(pen, x, y, w, h, startAngle, swipeAngle);

      int x1 = Utils.GetSafeInt(args, 1);
      int y1 = Utils.GetSafeInt(args, 2);
      int x2 = Utils.GetSafeInt(args, 3);
      int y2 = Utils.GetSafeInt(args, 4);

      string colorStr = Utils.GetSafeString(args, 5, "black");
      PdfBrush color = SfUtils.String2PdfColor(colorStr);

      float lineWidth = (float)Utils.GetSafeDouble(args, 6, 1.0);

      pdf.Init();
      pdf.AddLine(x1, y1, x2, y2, color, lineWidth);

      ParserFunction.UpdateFunction(pdf);
      return pdf;
    }
  }
  public class SetPdfRectangle : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 3, m_name);

      SfPdf pdf = args[0] as SfPdf;
      Utils.CheckNotNull(pdf, m_name);

      //m_grapics.DrawLine(pen, x1, y1, x2, y2);
      //m_grapics.DrawRectangle(color, x, y, w, h);
      //m_grapics.DrawPie(pen, x, y, w, h, startAngle, swipeAngle);

      int x = Utils.GetSafeInt(args, 1);
      int y = Utils.GetSafeInt(args, 2);
      int w = Utils.GetSafeInt(args, 3);
      int h = Utils.GetSafeInt(args, 4);

      string colorStr = Utils.GetSafeString(args, 5, "black");
      PdfBrush color = SfUtils.String2PdfColor(colorStr);

      pdf.Init();
      pdf.AddRectangle(x, y, w, h, color);

      ParserFunction.UpdateFunction(pdf);
      return pdf;
    }
  }
  public class SetPdfPie : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 3, m_name);

      SfPdf pdf = args[0] as SfPdf;
      Utils.CheckNotNull(pdf, m_name);

      //m_grapics.DrawLine(pen, x1, y1, x2, y2);
      //m_grapics.DrawRectangle(color, x, y, w, h);
      //m_grapics.DrawPie(pen, x, y, w, h, startAngle, swipeAngle);

      int x = Utils.GetSafeInt(args, 1);
      int y = Utils.GetSafeInt(args, 2);
      int w = Utils.GetSafeInt(args, 3);
      int h = Utils.GetSafeInt(args, 4);

      string colorStr = Utils.GetSafeString(args, 5, "black");
      PdfBrush color = SfUtils.String2PdfColor(colorStr);

      float startAngle = (float)Utils.GetSafeDouble(args, 6);
      float sweepAngle = (float)Utils.GetSafeDouble(args, 7);
      float lineWidth  = (float)Utils.GetSafeDouble(args, 8);

      pdf.Init();
      pdf.AddPie(x, y, w, h, startAngle, sweepAngle, color, lineWidth);

      ParserFunction.UpdateFunction(pdf);
      return pdf;
    }
  }
  public class SetPdfFont : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 3, m_name);

      SfPdf pdf = args[0] as SfPdf;
      Utils.CheckNotNull(pdf, m_name);

      string fontName = Utils.GetSafeString(args, 1);
      double newSize = Utils.GetSafeDouble(args, 2);
      bool bold = Utils.GetSafeInt(args, 3) == 1;
      bool italic = Utils.GetSafeInt(args, 4) == 1;

      pdf.Init();
      pdf.SetFontOptions(fontName, (float)newSize, bold, italic);

      ParserFunction.UpdateFunction(pdf);
      return pdf;
    }
  }
}
*/