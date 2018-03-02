using System;
using System.Collections.Generic;
using SplitAndMerge;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using COLOR = Syncfusion.Drawing;
using System.IO;
using System.Reflection;

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
  public class SfWord : BASE_VARIABLE
  {
    WordDocument m_document;
    WSection m_section;
    IWParagraph m_paragraph;

    IWTable m_table;
    WTableRow m_row;

    int m_id;
    static int m_counter;

    float m_margins;

    public SfWord()
    {
      m_id = ++m_counter;
    }
    public SfWord(int margins)
    {
      m_margins = margins;
      m_id = ++m_counter;
      Init();
    }
    public SfWord(SfWord other)
    {
      m_id = ++m_counter;
    }
    public override Variable Clone()
    {
      SfWord newVar = (SfWord)this.MemberwiseClone();
      newVar.m_id = ++m_counter;
      return newVar;
    }

    void Init()
    {
      if (m_document != null) {
        return;
      }
      //Fake(); return;
      m_document = new WordDocument();
      m_section = m_document.AddSection() as WSection;
      m_section.PageSetup.Margins.All = m_margins;

      AddStyles();
    }


    void CheckParagraph()
    {
      if (m_paragraph == null) {
        m_paragraph = m_section.AddParagraph();
      }
    }

    void AddStyles()
    {
      var style = m_document.AddParagraphStyle("Style_Normal") as WParagraphStyle;
      style.CharacterFormat.FontName = "Bitstream Vera Serif";
      style.CharacterFormat.FontSize = 10f;
      style.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Justify;
      style.CharacterFormat.TextColor = Syncfusion.Drawing.Color.FromArgb(0, 21, 84);

      style = m_document.AddParagraphStyle("Style_Low") as WParagraphStyle;
      style.CharacterFormat.FontName = "Times New Roman";
      style.CharacterFormat.FontSize = 16f;
      style.CharacterFormat.Bold = true;

      style = m_document.AddParagraphStyle("Style_Medium") as WParagraphStyle;
      style.CharacterFormat.FontName = "Monotype Corsiva";
      style.CharacterFormat.FontSize = 18f;
      style.CharacterFormat.Bold = true;
      style.CharacterFormat.TextColor = Syncfusion.Drawing.Color.FromArgb(51, 66, 125);

      style = m_document.AddParagraphStyle("Style_High") as WParagraphStyle;
      style.CharacterFormat.FontName = "Bitstream Vera Serif";
      style.CharacterFormat.FontSize = 20f;
      style.CharacterFormat.Bold = true;
      style.CharacterFormat.TextColor = Syncfusion.Drawing.Color.FromArgb(242, 151, 50);

      style = m_document.AddParagraphStyle("Normal2") as WParagraphStyle;
      style.CharacterFormat.FontName = "Calibri";
      style.CharacterFormat.FontSize = 11f;
      style.ParagraphFormat.BeforeSpacing = 0;
      style.ParagraphFormat.AfterSpacing = 8;
      style.ParagraphFormat.LineSpacing = 13.8f;

      style = m_document.AddParagraphStyle("Heading 1") as WParagraphStyle;
      style.ApplyBaseStyle("Normal");
      style.CharacterFormat.FontName = "Calibri Light";
      style.CharacterFormat.FontSize = 16f;
      style.CharacterFormat.TextColor = Syncfusion.Drawing.Color.FromArgb(46, 116, 181);
      style.ParagraphFormat.BeforeSpacing = 12;
      style.ParagraphFormat.AfterSpacing = 0;
      style.ParagraphFormat.Keep = true;
      style.ParagraphFormat.KeepFollow = true;
      style.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
    }

    void Fake()
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      // Creating a new document.
      // Creating a new document.
      WordDocument document = new WordDocument();
      //Adding a new section to the document.
      WSection section = document.AddSection() as WSection;
      //Set Margin of the section
      section.PageSetup.Margins.All = 72;
      //Set page size of the section
      section.PageSetup.PageSize = new Syncfusion.Drawing.SizeF(612, 792);

      //Create Paragraph styles
      WParagraphStyle style = document.AddParagraphStyle("Normal") as WParagraphStyle;
      style.CharacterFormat.FontName = "Calibri";
      style.CharacterFormat.FontSize = 11f;
      style.ParagraphFormat.BeforeSpacing = 0;
      style.ParagraphFormat.AfterSpacing = 8;
      style.ParagraphFormat.LineSpacing = 13.8f;

      style = document.AddParagraphStyle("Heading 1") as WParagraphStyle;
      style.ApplyBaseStyle("Normal");
      style.CharacterFormat.FontName = "Calibri Light";
      style.CharacterFormat.FontSize = 16f;
      style.CharacterFormat.TextColor = Syncfusion.Drawing.Color.FromArgb(46, 116, 181);
      style.ParagraphFormat.BeforeSpacing = 12;
      style.ParagraphFormat.AfterSpacing = 0;
      style.ParagraphFormat.Keep = true;
      style.ParagraphFormat.KeepFollow = true;
      style.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
      IWParagraph paragraph = section.HeadersFooters.Header.AddParagraph();

      Stream imageStream = assembly.GetManifestResourceStream("Resources.drawable.Adventure_Cycle.jpg");
      imageStream = UTILS.ImageToStream("AdventureCycle");
      WPicture picture = paragraph.AppendPicture(imageStream) as WPicture;
      picture.TextWrappingStyle = TextWrappingStyle.InFrontOfText;
      picture.VerticalOrigin = VerticalOrigin.Margin;
      picture.VerticalPosition = -24;
      picture.HorizontalOrigin = HorizontalOrigin.Column;
      picture.HorizontalPosition = 263.5f;
      picture.WidthScale = 20;
      picture.HeightScale = 15;

      paragraph.ApplyStyle("Normal");
      paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
      WTextRange textRange = paragraph.AppendText("Adventure Works Cycles") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Calibri";
      textRange.CharacterFormat.TextColor = Syncfusion.Drawing.Color.Red;

      //Appends paragraph.
      paragraph = section.AddParagraph();
      paragraph.ApplyStyle("Heading 1");
      paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;
      textRange = paragraph.AppendText("Adventure Works Cycles") as WTextRange;
      textRange.CharacterFormat.FontSize = 18f;
      textRange.CharacterFormat.FontName = "Calibri";


      //Appends paragraph.
      paragraph = section.AddParagraph();
      paragraph.ParagraphFormat.FirstLineIndent = 36;
      paragraph.BreakCharacterFormat.FontSize = 12f;
      textRange = paragraph.AppendText("Adventure Works Cycles, the fictitious company on which the AdventureWorks sample databases are based, is a large, multinational manufacturing company. The company manufactures and sells metal and composite bicycles to North American, European and Asian commercial markets. While its base operation is located in Bothell, Washington with 290 employees, several regional sales teams are located throughout their market base.") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;

      paragraph = section.AddParagraph();
      paragraph.ParagraphFormat.FirstLineIndent = 36;
      paragraph.BreakCharacterFormat.FontSize = 12f;
      textRange = paragraph.AppendText("In 2000, Adventure Works Cycles bought a small manufacturing plant, Importadores Neptuno, located in Mexico. Importadores Neptuno manufactures several critical subcomponents for the Adventure Works Cycles product line. These subcomponents are shipped to the Bothell location for final product assembly. In 2001, Importadores Neptuno, became the sole manufacturer and distributor of the touring bicycle product group.") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;

      paragraph = section.AddParagraph();
      paragraph.ApplyStyle("Heading 1");
      paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
      textRange = paragraph.AppendText("Product Overview") as WTextRange;
      textRange.CharacterFormat.FontSize = 16f;
      textRange.CharacterFormat.FontName = "Calibri";


      //Appends table.
      IWTable table = section.AddTable();
      table.ResetCells(3, 2);
      table.TableFormat.Borders.BorderType = Syncfusion.DocIO.DLS.BorderStyle.None;
      table.TableFormat.IsAutoResized = true;

      //Appends paragraph.
      paragraph = table[0, 0].AddParagraph();
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.BreakCharacterFormat.FontSize = 12f;
      imageStream = assembly.GetManifestResourceStream("Resources.drawable.Mountain_200.jpg");
      imageStream = UTILS.ImageToStream("Mountain200");
      //Appends picture to the paragraph.
      picture = paragraph.AppendPicture(imageStream) as WPicture;
      picture.TextWrappingStyle = TextWrappingStyle.TopAndBottom;
      picture.VerticalOrigin = VerticalOrigin.Paragraph;
      picture.VerticalPosition = 0;
      picture.HorizontalOrigin = HorizontalOrigin.Column;
      picture.HorizontalPosition = -5.15f;
      picture.WidthScale = 79;
      picture.HeightScale = 79;

      //Appends paragraph.
      paragraph = table[0, 1].AddParagraph();
      paragraph.ApplyStyle("Heading 1");
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.ParagraphFormat.LineSpacing = 12f;
      paragraph.AppendText("Mountain-200");
      //Appends paragraph.
      paragraph = table[0, 1].AddParagraph();
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.ParagraphFormat.LineSpacing = 12f;
      paragraph.BreakCharacterFormat.FontSize = 12f;
      paragraph.BreakCharacterFormat.FontName = "Times New Roman";

      textRange = paragraph.AppendText("Product No: BK-M68B-38\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Size: 38\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Weight: 25\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Price: $2,294.99\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";

      //Appends paragraph.
      paragraph = table[0, 1].AddParagraph();
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.ParagraphFormat.LineSpacing = 12f;
      paragraph.BreakCharacterFormat.FontSize = 12f;

      //Appends paragraph.
      paragraph = table[1, 0].AddParagraph();
      paragraph.ApplyStyle("Heading 1");
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.ParagraphFormat.LineSpacing = 12f;
      paragraph.AppendText("Mountain-300 ");
      //Appends paragraph.
      paragraph = table[1, 0].AddParagraph();
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.ParagraphFormat.LineSpacing = 12f;
      paragraph.BreakCharacterFormat.FontSize = 12f;
      paragraph.BreakCharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Product No: BK-M47B-38\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Size: 35\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Weight: 22\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Price: $1,079.99\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";

      //Appends paragraph.
      paragraph = table[1, 0].AddParagraph();
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.ParagraphFormat.LineSpacing = 12f;
      paragraph.BreakCharacterFormat.FontSize = 12f;

      //Appends paragraph.
      paragraph = table[1, 1].AddParagraph();
      paragraph.ApplyStyle("Heading 1");
      paragraph.ParagraphFormat.LineSpacing = 12f;
      imageStream = assembly.GetManifestResourceStream("Resources.drawable.Mountain_300.jpg");
      imageStream = UTILS.ImageToStream("Mountain300");
      //Appends picture to the paragraph.
      picture = paragraph.AppendPicture(imageStream) as WPicture;
      picture.TextWrappingStyle = TextWrappingStyle.TopAndBottom;
      picture.VerticalOrigin = VerticalOrigin.Paragraph;
      picture.VerticalPosition = 8.2f;
      picture.HorizontalOrigin = HorizontalOrigin.Column;
      picture.HorizontalPosition = -14.95f;
      picture.WidthScale = 75;
      picture.HeightScale = 75;

      //Appends paragraph.
      paragraph = table[2, 0].AddParagraph();
      paragraph.ApplyStyle("Heading 1");
      paragraph.ParagraphFormat.LineSpacing = 12f;
      imageStream = assembly.GetManifestResourceStream("Resources.drawable.Road_550_W.jpg");
      imageStream = UTILS.ImageToStream("Road550W");
      //Appends picture to the paragraph.
      picture = paragraph.AppendPicture(imageStream) as WPicture;
      picture.TextWrappingStyle = TextWrappingStyle.TopAndBottom;
      picture.VerticalOrigin = VerticalOrigin.Paragraph;
      picture.VerticalPosition = 0;
      picture.HorizontalOrigin = HorizontalOrigin.Column;
      picture.HorizontalPosition = -4.9f;
      picture.WidthScale = 92;
      picture.HeightScale = 92;

      //Appends paragraph.
      paragraph = table[2, 1].AddParagraph();
      paragraph.ApplyStyle("Heading 1");
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.ParagraphFormat.LineSpacing = 12f;
      paragraph.AppendText("Road-150 ");
      //Appends paragraph.
      paragraph = table[2, 1].AddParagraph();
      paragraph.ParagraphFormat.AfterSpacing = 0;
      paragraph.ParagraphFormat.LineSpacing = 12f;
      paragraph.BreakCharacterFormat.FontSize = 12f;
      paragraph.BreakCharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Product No: BK-R93R-44\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Size: 44\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Weight: 14\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      textRange = paragraph.AppendText("Price: $3,578.27\r") as WTextRange;
      textRange.CharacterFormat.FontSize = 12f;
      textRange.CharacterFormat.FontName = "Times New Roman";
      //Appends paragraph.
      paragraph = table[2, 1].AddParagraph();
      paragraph.ApplyStyle("Heading 1");
      paragraph.ParagraphFormat.LineSpacing = 12f;

      //Appends paragraph.
      section.AddParagraph();
      MemoryStream stream = new MemoryStream();
      document.Save(stream, FormatType.Word2013);
      document.Close();
      if (stream != null) {
#if __ANDROID__
        SaveAndroid.Save("GettingStarted.docx", "application/msword", stream, MainActivity.TheView);
#endif
      }
    }

    public void Open(string filename)
    {
      m_document = new WordDocument();

      Stream fileStream = Utils.OpenFile(filename);
      fileStream.Position = 0;

      m_document.Open(fileStream, FormatType.Word2013);

      if (m_document.Sections.Count == 0) {
        m_section = m_document.AddSection() as WSection;
        m_section.PageSetup.Margins.All = m_margins;
      } else {
        m_section = m_document.Sections[0];
        m_margins = m_section.PageSetup.Margins.All;
      }
      if (m_section.Paragraphs.Count == 0) {
        m_paragraph = m_section.AddParagraph();
      } else {
        m_paragraph = m_section.Paragraphs[0];
      }

      fileStream.Close();
    }
    public void AddTable(int rows, int cols, string styleStr)
    {
      m_table = m_section.AddTable();
      m_table.ResetCells(rows, cols);
      m_table.TableFormat.Borders.BorderType = SfUtils.String2BorderStyle(styleStr);
      m_table.TableFormat.IsAutoResized = true;
    }
    public void AddParagraph(string type = null, float indent = 0, int row = 0, int col = 0)
    {
      if (type == "header") {
        m_paragraph = m_section.HeadersFooters.Header.AddParagraph();
      } else if (type == "footer") {
        m_paragraph = m_section.HeadersFooters.Footer.AddParagraph();
      } else if (type == "table" && m_table != null) {
        m_paragraph = m_table[row, col].AddParagraph();
      } else {
        m_paragraph = m_section.AddParagraph();
      }

      m_paragraph.ParagraphFormat.FirstLineIndent = indent;
      m_paragraph.ParagraphFormat.AfterSpacing = 0;
      m_paragraph.ParagraphFormat.LineSpacing = 12f;
    }
    public void ApplyStyle(string styleStr, string alignmentStr)
    {
      CheckParagraph();
      var style = SfUtils.String2WordStyle(styleStr);
      if (style != BuiltinStyle.NoStyle) {
        m_paragraph.ApplyStyle(style);
      } else {
        m_paragraph.ApplyStyle(styleStr);
      }
      var alignment = SfUtils.String2HorizontalAlignment(alignmentStr);
      m_paragraph.ParagraphFormat.HorizontalAlignment = alignment;
    }
    public void AddText(string text)
    {
      CheckParagraph();
      m_paragraph.AppendText(text);
    }
    public void AddTextRange(string text, string font, float fontSize,
                             string fontColor = null,
                             string bgColor = null)
    {
      CheckParagraph();

      var textRange = m_paragraph.AppendText(text) as WTextRange;
      textRange.CharacterFormat.FontSize = fontSize;
      textRange.CharacterFormat.FontName = font;
      m_paragraph.BreakCharacterFormat.FontSize = fontSize;

      if (!string.IsNullOrEmpty(fontColor)) {
        textRange.CharacterFormat.TextColor = SfUtils.String2Color(fontColor);
      }
      if (!string.IsNullOrEmpty(bgColor)) {
        textRange.CharacterFormat.TextBackgroundColor = SfUtils.String2Color(bgColor);
      }
    }
    public void AddImage(string imagePath,
                         float horizontalPosition = 0f,
                         float verticalPosition = 0f,
                         string textWrap = "InFrontOfText",
                         string horizontalOrigin = "Margin",
                         string verticalOrigin = "Margin",
                         float widthScale = 20f,
                         float heightScale = 20f)
    {
      CheckParagraph();

      Stream imageStream = UTILS.ImageToStream(imagePath);
      WPicture picture = m_paragraph.AppendPicture(imageStream) as WPicture;
      picture.TextWrappingStyle = SfUtils.String2TextWrappingStyle(textWrap);
      picture.HorizontalOrigin = SfUtils.String2HorizontalOrigin(horizontalOrigin);
      picture.HorizontalPosition = horizontalPosition;
      picture.VerticalOrigin = SfUtils.String2VerticalOrigin(verticalOrigin);
      picture.VerticalPosition = verticalPosition;
      picture.WidthScale = widthScale;
      picture.HeightScale = heightScale;
    }
    public void Save(string filename)
    {
      m_section.AddParagraph();

      MemoryStream stream = new MemoryStream();
      m_document.Save(stream, FormatType.Word2013);
      m_document.Close();
      m_document.Dispose();

      if (stream != null) {
#if __ANDROID__
        SaveAndroid.Save(filename, "application/msword", stream, MainActivity.TheView);
#elif __IOS__
        PreviewController.Save(filename, "application/msword", stream);
#endif
      }
      Utils.SaveFile(filename, stream);
      m_document = null;
    }
  }
  public class CreateWord : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 0, m_name);

      int margins = Utils.GetSafeInt(args, 0, 72);

      SfWord word = new SfWord(margins);

      return word;
    }
  }
  public class OpenWord : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      string filename = Utils.GetSafeString(args, 0);

      SfWord word = new SfWord();
      word.Open(filename);

      return word;
    }
  }
  public class SaveWord : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      SfWord word = args[0] as SfWord;
      Utils.CheckNotNull(word, m_name);

      string filename = Utils.GetSafeString(args, 1);
      word.Save(filename);

      ParserFunction.UpdateFunction(word);
      return word;
    }
  }
  public class AddWordText : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      SfWord word = args[0] as SfWord;
      Utils.CheckNotNull(word, m_name);

      string text = Utils.GetSafeString(args, 1);
      text = text.Replace("\\n", "\n");
      text = text.Replace("\\t", "\t");

      word.AddText(text);

      ParserFunction.UpdateFunction(word);
      return word;
    }
  }
  public class AddWordTextRange : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      SfWord word = args[0] as SfWord;
      Utils.CheckNotNull(word, m_name);

      string text = Utils.GetSafeString(args, 1);
      string font = Utils.GetSafeString(args, 2, "Calibri");
      float fontSize = (float)Utils.GetSafeDouble(args, 3, 12);
      string fontColor = Utils.GetSafeString(args, 4);
      string bgColor = Utils.GetSafeString(args, 5);

      text = text.Replace("\\n", "\n");
      text = text.Replace("\\t", "\t");
      word.AddTextRange(text, font, fontSize, fontColor, bgColor);

      ParserFunction.UpdateFunction(word);
      return word;
    }
  }
  public class AddWordImage : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 2, m_name);

      SfWord word = args[0] as SfWord;
      Utils.CheckNotNull(word, m_name);

      string imagePath = Utils.GetSafeString(args, 1);
      float horizontalPosition = (float)Utils.GetSafeDouble(args, 2);
      float verticalPosition = (float)Utils.GetSafeDouble(args, 3);
      string textWrap = Utils.GetSafeString(args, 4, "InFrontOfText");
      string horizontalOrigin = Utils.GetSafeString(args, 5, "Margin");
      string verticalOrigin = Utils.GetSafeString(args, 6, "Margin");
      float widthScale = (float)Utils.GetSafeDouble(args, 7, 20f);
      float heightScale = (float)Utils.GetSafeDouble(args, 8, 20f);

      word.AddImage(imagePath, horizontalPosition, verticalPosition, textWrap,
                    horizontalOrigin, verticalOrigin, widthScale, heightScale);

      ParserFunction.UpdateFunction(word);
      return word;
    }
  }
  public class AddWordParagraph : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      SfWord word = args[0] as SfWord;
      Utils.CheckNotNull(word, m_name);

      string type = Utils.GetSafeString(args, 1);
      float indent = (float)Utils.GetSafeDouble(args, 2);
      int row = Utils.GetSafeInt(args, 3);
      int col = Utils.GetSafeInt(args, 4);

      word.AddParagraph(type, indent, row, col);

      ParserFunction.UpdateFunction(word);
      return word;
    }
  }
  public class AddWordTable : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      SfWord word = args[0] as SfWord;
      Utils.CheckNotNull(word, m_name);

      int rows = Utils.GetSafeInt(args, 1, 2);
      int cols = Utils.GetSafeInt(args, 2, 2);
      string styleStr = Utils.GetSafeString(args, 3);

      word.AddTable(rows, cols, styleStr);

      ParserFunction.UpdateFunction(word);
      return word;
    }
  }
  public class ApplyWordStyle : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      bool isList = false;
      List<Variable> args = Utils.GetArgs(script,
                            Constants.START_ARG, Constants.END_ARG, out isList);
      Utils.CheckArgs(args.Count, 1, m_name);

      SfWord word = args[0] as SfWord;
      Utils.CheckNotNull(word, m_name);

      string styleStr = Utils.GetSafeString(args, 1);
      string alignmentStr = Utils.GetSafeString(args, 2);

      word.ApplyStyle(styleStr, alignmentStr);

      ParserFunction.UpdateFunction(word);
      return word;
    }
  }
}
