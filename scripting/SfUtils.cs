using System;
using Syncfusion.Pdf.Graphics;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using COLOR = Syncfusion.Drawing;

namespace scripting
{
    public class SfUtils
    {
        public static ExcelKnownColors GetColor(string colorStr)
        {
            colorStr = colorStr.ToLower();
            switch (colorStr)
            {
                case "black": return ExcelKnownColors.Black;
                case "blue": return ExcelKnownColors.Blue;
                case "brown": return ExcelKnownColors.Brown;
                case "clear": return ExcelKnownColors.Custom0;
                case "cyan": return ExcelKnownColors.Cyan;
                case "dark_gray": return ExcelKnownColors.Grey_80_percent;
                case "dark_green": return ExcelKnownColors.Dark_green;
                case "dark_red": return ExcelKnownColors.Dark_red;
                case "deep_sky_blue": return ExcelKnownColors.Sky_blue;
                case "deep_pink": return ExcelKnownColors.Pink;
                case "gray": return ExcelKnownColors.Grey_50_percent;
                case "green": return ExcelKnownColors.Green;
                case "light_blue": return ExcelKnownColors.Light_blue;
                case "light_cyan": return ExcelKnownColors.Cyan;
                case "light_gray": return ExcelKnownColors.Grey_25_percent;
                case "light_green": return ExcelKnownColors.Light_green;
                case "light_yellow": return ExcelKnownColors.Light_yellow;
                case "magenta": return ExcelKnownColors.Magenta;
                case "neutral": return ExcelKnownColors.Custom0;
                case "orange": return ExcelKnownColors.Orange;
                case "pink": return ExcelKnownColors.Pink;
                case "purple": return ExcelKnownColors.Violet;
                case "red": return ExcelKnownColors.Red;
                case "rose": return ExcelKnownColors.Rose;
                case "sky_blue": return ExcelKnownColors.Sky_blue;
                case "snow": return ExcelKnownColors.WhiteCustom;
                case "transparent": return ExcelKnownColors.Custom0;
                case "white": return ExcelKnownColors.White;
                case "yellow": return ExcelKnownColors.Yellow;
            }

            return ExcelKnownColors.Black;
        }
        public static PdfBrush String2PdfColor(string colorStr)
        {
            colorStr = colorStr.ToLower();
            switch (colorStr)
            {
                case "black": return PdfBrushes.Black;
                case "blue": return PdfBrushes.Blue;
                case "brown": return PdfBrushes.Brown;
                case "clear": return PdfBrushes.Transparent;
                case "cyan": return PdfBrushes.Cyan;
                case "dark_gray": return PdfBrushes.DarkGray;
                case "dark_green": return PdfBrushes.DarkGreen;
                case "dark_red": return PdfBrushes.DarkRed;
                case "deep_sky_blue": return PdfBrushes.DeepSkyBlue;
                case "deep_pink": return PdfBrushes.DeepPink;
                case "gray": return PdfBrushes.Gray;
                case "green": return PdfBrushes.Green;
                case "light_blue": return PdfBrushes.LightBlue;
                case "light_cyan": return PdfBrushes.LightCyan;
                case "light_gray": return PdfBrushes.LightGray;
                case "light_green": return PdfBrushes.LightGreen;
                case "light_yellow": return PdfBrushes.LightYellow;
                case "magenta": return PdfBrushes.Magenta;
                case "neutral": return PdfBrushes.Transparent;
                case "orange": return PdfBrushes.Orange;
                case "pink": return PdfBrushes.Pink;
                case "purple": return PdfBrushes.Purple;
                case "red": return PdfBrushes.Red;
                case "rose": return PdfBrushes.MistyRose;
                case "sky_blue": return PdfBrushes.SkyBlue;
                case "snow": return PdfBrushes.Snow;
                case "transparent": return PdfBrushes.Transparent;
                case "white": return PdfBrushes.White;
                case "yellow": return PdfBrushes.Yellow;
            }

            return PdfBrushes.Black;
        }
        public static COLOR.Color String2Color(string colorStr)
        {
            colorStr = colorStr.ToLower();
            switch (colorStr)
            {
                case "black": return COLOR.Color.Black;
                case "blue": return COLOR.Color.Blue;
                case "brown": return COLOR.Color.Brown;
                case "clear": return COLOR.Color.Transparent;
                case "cyan": return COLOR.Color.Cyan;
                case "dark_gray": return COLOR.Color.DarkGray;
                case "dark_green": return COLOR.Color.DarkGreen;
                case "dark_red": return COLOR.Color.DarkRed;
                case "deep_sky_blue": return COLOR.Color.DeepSkyBlue;
                case "deep_pink": return COLOR.Color.DeepPink;
                case "gray": return COLOR.Color.Gray;
                case "green": return COLOR.Color.Green;
                case "light_blue": return COLOR.Color.LightBlue;
                case "light_cyan": return COLOR.Color.LightCyan;
                case "light_gray": return COLOR.Color.LightGray;
                case "light_green": return COLOR.Color.LightGreen;
                case "light_yellow": return COLOR.Color.LightYellow;
                case "magenta": return COLOR.Color.Magenta;
                case "neutral": return COLOR.Color.Transparent;
                case "orange": return COLOR.Color.Orange;
                case "pink": return COLOR.Color.Pink;
                case "purple": return COLOR.Color.Purple;
                case "red": return COLOR.Color.Red;
                case "rose": return COLOR.Color.MistyRose;
                case "sky_blue": return COLOR.Color.SkyBlue;
                case "snow": return COLOR.Color.Snow;
                case "transparent": return COLOR.Color.Transparent;
                case "white": return COLOR.Color.White;
                case "yellow": return COLOR.Color.Yellow;
            }
            return COLOR.Color.Black;
        }
        public static COLOR.Color ConvertToColor(string colorStr)
        {
            COLOR.Color result = COLOR.Color.Black;
            var tokens = colorStr.Split(new char[] { ',' });
            int r, g, b;
            if (tokens.Length > 2 && Int32.TryParse(tokens[0], out r) && Int32.TryParse(tokens[1], out g) &&
                Int32.TryParse(tokens[2], out b))
            {
                return COLOR.Color.FromArgb(r, g, b);
            }
            return COLOR.Color.Empty;
        }
        public static BorderStyle String2BorderStyle(string styleStr)
        {
            styleStr = styleStr.ToLower();
            BorderStyle style = BorderStyle.None;
            switch (styleStr)
            {
                case "dashdotstroker": return BorderStyle.DashDotStroker;
                case "dashlargegap": return BorderStyle.DashLargeGap;
                case "dashsmallgap": return BorderStyle.DashSmallGap;
                case "dot": return BorderStyle.Dot;
                case "dotdash": return BorderStyle.DotDash;
                case "dotdotdash": return BorderStyle.DotDotDash;
                case "double": return BorderStyle.Double;
                case "doublewave": return BorderStyle.DoubleWave;
                case "hairline": return BorderStyle.Hairline;
                case "single": return BorderStyle.Single;
                case "thick": return BorderStyle.Thick;
                case "triple": return BorderStyle.Triple;
                case "wave": return BorderStyle.Wave;
            }
            return style;
        }
        public static TextWrappingStyle String2TextWrappingStyle(string styleStr)
        {
            styleStr = styleStr.ToLower();
            TextWrappingStyle style = TextWrappingStyle.InFrontOfText;
            switch (styleStr)
            {
                case "behind": return TextWrappingStyle.Behind;
                case "infrontoftext": return TextWrappingStyle.InFrontOfText;
                case "inline": return TextWrappingStyle.Inline;
                case "right": return TextWrappingStyle.Square;
                case "through": return TextWrappingStyle.Through;
                case "tight": return TextWrappingStyle.Tight;
                case "topandbottom": return TextWrappingStyle.TopAndBottom;
            }
            return style;
        }
        public static VerticalOrigin String2VerticalOrigin(string styleStr)
        {
            styleStr = styleStr.ToLower();
            VerticalOrigin style = VerticalOrigin.Margin;
            switch (styleStr)
            {
                case "bottommargin": return VerticalOrigin.BottomMargin;
                case "insidemargin": return VerticalOrigin.InsideMargin;
                case "line": return VerticalOrigin.Line;
                case "margin": return VerticalOrigin.Margin;
                case "outsidemargin": return VerticalOrigin.OutsideMargin;
                case "page": return VerticalOrigin.Page;
                case "paragraph": return VerticalOrigin.Paragraph;
                case "topmargin": return VerticalOrigin.TopMargin;
            }
            return style;
        }
        public static HorizontalOrigin String2HorizontalOrigin(string styleStr)
        {
            styleStr = styleStr.ToLower();
            HorizontalOrigin style = HorizontalOrigin.Margin;
            switch (styleStr)
            {
                case "character": return HorizontalOrigin.Character;
                case "column": return HorizontalOrigin.Column;
                case "insidemargin": return HorizontalOrigin.InsideMargin;
                case "leftmargin": return HorizontalOrigin.LeftMargin;
                case "margin": return HorizontalOrigin.Margin;
                case "outsidemargin": return HorizontalOrigin.OutsideMargin;
                case "page": return HorizontalOrigin.Page;
                case "rightmargin": return HorizontalOrigin.RightMargin;
            }
            return style;
        }
        public static HorizontalAlignment String2HorizontalAlignment(string styleStr)
        {
            styleStr = styleStr.ToLower();
            HorizontalAlignment style = HorizontalAlignment.Center;
            switch (styleStr)
            {
                case "center": return HorizontalAlignment.Center;
                case "distribute": return HorizontalAlignment.Distribute;
                case "justify": return HorizontalAlignment.Justify;
                case "justifylow": return HorizontalAlignment.JustifyLow;
                case "justifyhigh": return HorizontalAlignment.JustifyHigh;
                case "justifymedium": return HorizontalAlignment.JustifyMedium;
                case "left": return HorizontalAlignment.Left;
                case "right": return HorizontalAlignment.Right;
            }
            return style;
        }
        public static BuiltinStyle String2WordStyle(string styleStr)
        {
            styleStr = styleStr.ToLower();
            BuiltinStyle style = BuiltinStyle.NoStyle;
            switch (styleStr)
            {
                case "ballontext": return BuiltinStyle.BalloonText;
                case "bodytext": return BuiltinStyle.BodyText;
                case "bodytextind": return BuiltinStyle.BodyTextInd;
                case "bodytext2": return BuiltinStyle.BodyText2;
                case "bodytext3": return BuiltinStyle.BodyText3;
                case "blocktext": return BuiltinStyle.BlockText;
                case "caption": return BuiltinStyle.Caption;
                case "closing": return BuiltinStyle.Closing;
                case "commentreference": return BuiltinStyle.CommentReference;
                case "commentsubject": return BuiltinStyle.CommentSubject;
                case "commenttext": return BuiltinStyle.CommentText;
                case "date": return BuiltinStyle.Date;
                case "defaultparagraph": return BuiltinStyle.DefaultParagraphFont;
                case "documentmap": return BuiltinStyle.DocumentMap;
                case "emailsignature": return BuiltinStyle.EmailSignature;
                case "emphasis": return BuiltinStyle.Emphasis;
                case "endnotereference": return BuiltinStyle.EndnoteReference;
                case "footer": return BuiltinStyle.Footer;
                case "footnotetext": return BuiltinStyle.FootnoteText;
                case "footnotereference": return BuiltinStyle.FootnoteReference;
                case "header": return BuiltinStyle.Header;
                case "heading1": return BuiltinStyle.Heading1;
                case "heading2": return BuiltinStyle.Heading2;
                case "heading3": return BuiltinStyle.Heading3;
                case "heading4": return BuiltinStyle.Heading4;
                case "heading5": return BuiltinStyle.Heading5;
                case "heading6": return BuiltinStyle.Heading6;
                case "heading7": return BuiltinStyle.Heading7;
                case "heading8": return BuiltinStyle.Heading8;
                case "heading9": return BuiltinStyle.Heading9;
                case "htmlacronym": return BuiltinStyle.HtmlAcronym;
                case "htmladdress": return BuiltinStyle.HtmlAddress;
                case "htmlcode": return BuiltinStyle.HtmlCode;
                case "htmldefinition": return BuiltinStyle.HtmlDefinition;
                case "htmlpreformated": return BuiltinStyle.HtmlPreformatted;
                case "htmlsample": return BuiltinStyle.HtmlSample;
                case "htmltypewriter": return BuiltinStyle.HtmlTypewriter;
                case "htmlvariable": return BuiltinStyle.HtmlVariable;
                case "hyperlink": return BuiltinStyle.Hyperlink;
                case "index1": return BuiltinStyle.Index1;
                case "index2": return BuiltinStyle.Index2;
                case "index3": return BuiltinStyle.Index3;
                case "index4": return BuiltinStyle.Index4;
                case "index5": return BuiltinStyle.Index5;
                case "index6": return BuiltinStyle.Index6;
                case "index7": return BuiltinStyle.Index7;
                case "index8": return BuiltinStyle.Index8;
                case "index9": return BuiltinStyle.Index9;
                case "indexheading": return BuiltinStyle.IndexHeading;
                case "linenumber": return BuiltinStyle.LineNumber;
                case "list": return BuiltinStyle.List;
                case "list2": return BuiltinStyle.List2;
                case "list3": return BuiltinStyle.List3;
                case "list4": return BuiltinStyle.List4;
                case "list5": return BuiltinStyle.List5;
                case "listbullet": return BuiltinStyle.ListBullet;
                case "listbullet2": return BuiltinStyle.ListBullet2;
                case "listbullet3": return BuiltinStyle.ListBullet3;
                case "listbullet4": return BuiltinStyle.ListBullet4;
                case "listbullet5": return BuiltinStyle.ListBullet5;
                case "listcontinue": return BuiltinStyle.ListContinue;
                case "listcontinue2": return BuiltinStyle.ListContinue2;
                case "listcontinue3": return BuiltinStyle.ListContinue3;
                case "listcontinue4": return BuiltinStyle.ListContinue4;
                case "listcontinue5": return BuiltinStyle.ListContinue5;
                case "listnumber": return BuiltinStyle.ListNumber;
                case "listnumber2": return BuiltinStyle.ListNumber2;
                case "listnumber3": return BuiltinStyle.ListNumber3;
                case "listnumber4": return BuiltinStyle.ListNumber4;
                case "listnumber5": return BuiltinStyle.ListNumber5;
                case "macrotext": return BuiltinStyle.MacroText;
                case "messageheader": return BuiltinStyle.MessageHeader;
                case "normal": return BuiltinStyle.Normal;
                case "nolist": return BuiltinStyle.NoList;
                case "normalweb": return BuiltinStyle.NormalWeb;
                case "normalindent": return BuiltinStyle.NormalIndent;
                case "pagenumber": return BuiltinStyle.PageNumber;
                case "plaintext": return BuiltinStyle.PlainText;
                case "salutation": return BuiltinStyle.Salutation;
                case "signature": return BuiltinStyle.Signature;
                case "strong": return BuiltinStyle.Strong;
                case "subtitle": return BuiltinStyle.Subtitle;
                case "tableoffigures": return BuiltinStyle.TableOfFigures;
                case "title": return BuiltinStyle.Title;
                case "toc1": return BuiltinStyle.Toc1;
                case "toc2": return BuiltinStyle.Toc2;
                case "toc3": return BuiltinStyle.Toc3;
                case "toc4": return BuiltinStyle.Toc4;
                case "toc5": return BuiltinStyle.Toc5;
                case "toc6": return BuiltinStyle.Toc6;
                case "toc7": return BuiltinStyle.Toc7;
                case "toc8": return BuiltinStyle.Toc8;
                case "toc9": return BuiltinStyle.Toc9;
                case "user": return BuiltinStyle.User;
            }

            return style;
        }
    }
}
