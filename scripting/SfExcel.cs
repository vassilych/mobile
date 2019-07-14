using System;
using System.Collections.Generic;
using SplitAndMerge;
using Syncfusion.XlsIO;
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
    public class SfExcel : BASE_VARIABLE
    {
        ExcelEngine m_excelEngine;
        IApplication m_application;
        IWorkbook m_workbook;
        IWorksheet m_sheet;
        int m_numberSheets;

        public SfExcel()
        { }

        public SfExcel(int numberSheets)
        {
            m_numberSheets = numberSheets;
            Init();
        }

        public override Variable Clone()
        {
            SfExcel newVar = (SfExcel)this.MemberwiseClone();
            return newVar;
        }

        public void Init()
        {
            if (m_excelEngine != null)
            {
                return;
            }
            m_excelEngine = new ExcelEngine();
            m_application = m_excelEngine.Excel;
            m_application.DefaultVersion = ExcelVersion.Excel2013;

            m_workbook = m_application.Workbooks.Create(m_numberSheets);
            m_workbook.Version = ExcelVersion.Excel2013;

            m_sheet = m_workbook.Worksheets[0];

            m_sheet.EnableSheetCalculations();

        }
        public void Open(string filename)
        {
            m_excelEngine = new ExcelEngine();
            m_application = m_excelEngine.Excel;
            m_application.DefaultVersion = ExcelVersion.Excel2013;

            Stream fileStream = Utils.OpenFile(filename);
            fileStream.Position = 0;

            m_workbook = m_application.Workbooks.Open(fileStream);
            m_workbook.Version = ExcelVersion.Excel2013;

            m_sheet = m_workbook.Worksheets[0];

            m_numberSheets = m_workbook.Worksheets.Count;

            m_sheet.EnableSheetCalculations();
            fileStream.Close();
        }
        public void AddChart(string range, string title, int top, int bottom, int left, int right)
        {
            IChartShape chart = m_sheet.Charts.Add();

            chart.DataRange = m_sheet[range];
            chart.ChartTitle = title;
            chart.HasLegend = false;
            chart.TopRow = top;
            chart.LeftColumn = left;
            chart.RightColumn = right;
            chart.BottomRow = bottom;
        }
        public void AddWorksheet(string title)
        {
            m_sheet = m_workbook.Worksheets.Create(title);
        }
        public void SetWorksheetName(string title)
        {
            m_sheet.Name = title;
        }
        public void ActivateWorksheet(string title)
        {
            m_sheet = m_workbook.Worksheets[title];
        }
        public void ActivateWorksheet(int index)
        {
            m_sheet = m_workbook.Worksheets[index];
        }
        public void SetValue(string cell, string option, string value)
        {
            Init();

            switch (option)
            {
                case "text":
                    m_sheet.Range[cell].Text = value;
                    break;
                case "number":
                    m_sheet.Range[cell].Number = Utils.ConvertToDouble(value);
                    break;
                case "number_format":
                    m_sheet.Range[cell].NumberFormat = value;
                    break;
                case "date_time":
                    m_sheet.Range[cell].DateTime = DateTime.Parse(value);
                    break;
                case "merge":
                    m_sheet.Range[cell].Merge(Utils.ConvertToBool(value));
                    break;
                case "formula":
                    m_sheet.Range[cell].Formula = value;
                    break;
                case "col_width":
                    m_sheet.Range[cell].ColumnWidth = Utils.ConvertToDouble(value);
                    break;
                case "row_height":
                    m_sheet.Range[cell].RowHeight = Utils.ConvertToDouble(value);
                    break;
                case "include_border":
                    m_sheet.Range[cell].CellStyle.IncludeBorder = Utils.ConvertToBool(value);
                    break;
                case "wrap_text":
                    m_sheet.Range[cell].WrapText = Utils.ConvertToBool(value);
                    break;
                case "horizontal_alignment":
                    switch (value)
                    {
                        case "center":
                            m_sheet.Range[cell].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            break;
                        case "left":
                            m_sheet.Range[cell].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            break;
                        case "right":
                            m_sheet.Range[cell].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            break;
                        case "fill":
                            m_sheet.Range[cell].HorizontalAlignment = ExcelHAlign.HAlignFill;
                            break;
                        case "justify":
                            m_sheet.Range[cell].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            break;
                    }
                    break;
                case "vertical_alignment":
                    switch (value)
                    {
                        case "center":
                            m_sheet.Range[cell].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            break;
                        case "top":
                            m_sheet.Range[cell].VerticalAlignment = ExcelVAlign.VAlignTop;
                            break;
                        case "bottom":
                            m_sheet.Range[cell].VerticalAlignment = ExcelVAlign.VAlignBottom;
                            break;
                        case "fill":
                            m_sheet.Range[cell].VerticalAlignment = ExcelVAlign.VAlignDistributed;
                            break;
                        case "justify":
                            m_sheet.Range[cell].VerticalAlignment = ExcelVAlign.VAlignJustify;
                            break;
                    }
                    break;
                case "bold":
                    m_sheet.Range[cell].CellStyle.Font.Bold = Utils.ConvertToBool(value);
                    break;
                case "font_name":
                    m_sheet.Range[cell].CellStyle.Font.FontName = value;
                    break;
                case "font_size":
                    m_sheet.Range[cell].CellStyle.Font.Size = Utils.ConvertToDouble(value);
                    break;
                case "font_color":
                    var color = SfUtils.ConvertToColor(value);
                    if (color != COLOR.Color.Empty)
                    {
                        m_sheet.Range[cell].CellStyle.Font.RGBColor = color;
                    }
                    else
                    {
                        m_sheet.Range[cell].CellStyle.Font.Color = SfUtils.GetColor(value);
                    }
                    break;
                case "color":
                    color = SfUtils.ConvertToColor(value);
                    if (color != COLOR.Color.Empty)
                    {
                        m_sheet.Range[cell].CellStyle.Color = color;
                    }
                    else
                    {
                        m_sheet.Range[cell].CellStyle.Color =
                                 COLOR.Color.FromName(SfUtils.GetColor(value).ToString());
                    }
                    break;
                default:
                    Console.WriteLine("No action for {0}", option);
                    break;
            }
        }
        public void Save(string filename)
        {
            MemoryStream stream = new MemoryStream();
            m_workbook.SaveAs(stream);
            m_workbook.Close();
            m_excelEngine.Dispose();

            if (stream != null)
            {
#if __ANDROID__
        SaveAndroid.Save(filename, "application/msexcel", stream, MainActivity.TheView);
#elif __IOS__
                PreviewController.Save(filename, "application/msexcel", stream);
#endif
            }
            Utils.SaveFile(filename, stream);
            m_excelEngine = null;
        }
    }
    public class CreateExcel : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();

            int numberSheets = Utils.GetSafeInt(args, 0, 1);

            SfExcel excel = new SfExcel(numberSheets);

            return excel;
        }
    }
    public class OpenExcel : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string filename = Utils.GetSafeString(args, 0);

            SfExcel excel = new SfExcel();
            excel.Open(filename);

            return excel;
        }
    }
    public class SaveExcel : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            SfExcel excel = args[0] as SfExcel;
            Utils.CheckNotNull(excel, m_name, script);

            string filename = Utils.GetSafeString(args, 1);
            excel.Save(filename);

            ParserFunction.UpdateFunction(excel);
            return excel;
        }
    }
    public class SetExcelOption : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            SfExcel excel = args[0] as SfExcel;
            Utils.CheckNotNull(excel, m_name, script);

            string cell = Utils.GetSafeString(args, 1);
            string option = Utils.GetSafeString(args, 2);
            string value = Utils.GetSafeString(args, 3, "");

            excel.SetValue(cell, option, value);

            ParserFunction.UpdateFunction(excel);
            return excel;
        }
    }
    public class AddExcelWorksheet : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            SfExcel excel = args[0] as SfExcel;
            Utils.CheckNotNull(excel, m_name, script);

            string sheetName = Utils.GetSafeString(args, 1);
            excel.AddWorksheet(sheetName);

            ParserFunction.UpdateFunction(excel);
            return excel;
        }
    }
    public class RenameExcelWorksheet : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            SfExcel excel = args[0] as SfExcel;
            Utils.CheckNotNull(excel, m_name, script);

            string sheetName = Utils.GetSafeString(args, 1);
            excel.SetWorksheetName(sheetName);

            ParserFunction.UpdateFunction(excel);
            return excel;
        }
    }
    public class ActivateExcelWorksheet : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            SfExcel excel = args[0] as SfExcel;
            Utils.CheckNotNull(excel, m_name, script);

            Variable sheet = Utils.GetSafeVariable(args, 1);
            if (sheet.Type == Variable.VarType.NUMBER)
            {
                int index = sheet.AsInt();
                excel.ActivateWorksheet(index);
            }
            else
            {
                string name = sheet.AsString();
                excel.ActivateWorksheet(name);
            }

            ParserFunction.UpdateFunction(excel);
            return excel;
        }
    }
    public class AddExcelChart : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            SfExcel excel = args[0] as SfExcel;
            Utils.CheckNotNull(excel, m_name, script);

            string range = Utils.GetSafeString(args, 1);
            string title = Utils.GetSafeString(args, 2);
            int top = Utils.GetSafeInt(args, 3);
            int bottom = Utils.GetSafeInt(args, 4);
            int left = Utils.GetSafeInt(args, 5);
            int right = Utils.GetSafeInt(args, 6);

            excel.AddChart(range, title, top, bottom, left, right);

            ParserFunction.UpdateFunction(excel);
            return excel;
        }
    }
}
