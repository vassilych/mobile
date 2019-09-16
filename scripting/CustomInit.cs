using System;
using SplitAndMerge;
using System.Collections.Generic;

#if __ANDROID__
using scripting.Droid;
#elif __IOS__
using scripting.iOS;
#endif

namespace scripting
{
    public class CustomInit
    {
        public static void InitAndRunScript()
        {
            UIVariable.WidgetTypes.Add(new AdMob());
            UIVariable.WidgetTypes.Add(new SfWidget());

            string fileName = "start.cscs";

            ParserFunction.RegisterFunction("InitAds", new InitAds());
            ParserFunction.RegisterFunction("ShowInterstitial", new ShowInterstitial());
            ParserFunction.RegisterFunction("AddAdMobBanner", new AddWidgetFunction("AdMobBanner"));

            ParserFunction.RegisterFunction("AddSfQRBarcode", new AddWidgetFunction("SfQRBarcode"));
            ParserFunction.RegisterFunction("AddSfCode39Barcode", new AddWidgetFunction("SfCode39Barcode"));
            ParserFunction.RegisterFunction("AddSfCircularGauge", new AddWidgetFunction("SfCircularGauge"));
            ParserFunction.RegisterFunction("AddSfDataGrid", new AddWidgetFunction("SfDataGrid"));
            ParserFunction.RegisterFunction("AddSfDigitalGauge", new AddWidgetFunction("SfDigitalGauge"));
            ParserFunction.RegisterFunction("AddSfPicker", new AddWidgetFunction("SfPicker"));
            ParserFunction.RegisterFunction("AddSfStepper", new AddWidgetFunction("SfStepper"));
            ParserFunction.RegisterFunction("AddSfBusyIndicator", new AddWidgetFunction("SfBusyIndicator"));
            ParserFunction.RegisterFunction("AddSfSplineGraph", new AddWidgetFunction("SfSplineGraph"));
            ParserFunction.RegisterFunction("AddSfColumnGraph", new AddWidgetFunction("SfColumnGraph"));
            ParserFunction.RegisterFunction("AddSfDoughnutGraph", new AddWidgetFunction("SfDoughnutGraph"));
            ParserFunction.RegisterFunction("AddSfCalendar", new AddWidgetFunction("SfCalendar"));
            ParserFunction.RegisterFunction("AddSfAppointment", new AddAppointmentFunction());
            ParserFunction.RegisterFunction("AddSfImageEditor", new AddWidgetFunction("SfImageEditor"));
            ParserFunction.RegisterFunction("StartSfImageEditor", new StartSfImageEditor());

            ParserFunction.RegisterFunction("SfExcelNew", new CreateExcel());
            ParserFunction.RegisterFunction("SfExcelOpen", new OpenExcel());
            ParserFunction.RegisterFunction("SfExcelSet", new SetExcelOption());
            ParserFunction.RegisterFunction("SfAddExcelWorksheet", new AddExcelWorksheet());
            ParserFunction.RegisterFunction("SfRenameExcelWorksheet", new RenameExcelWorksheet());
            ParserFunction.RegisterFunction("SfActivateExcelWorksheet", new ActivateExcelWorksheet());
            ParserFunction.RegisterFunction("SfAddExcelChart", new AddExcelChart());
            ParserFunction.RegisterFunction("SfSaveExcel", new SaveExcel());

            ParserFunction.RegisterFunction("SfWordNew", new CreateWord());
            ParserFunction.RegisterFunction("SfWordOpen", new OpenWord());
            ParserFunction.RegisterFunction("SfAddWordText", new AddWordText());
            ParserFunction.RegisterFunction("SfAddWordTextRange", new AddWordTextRange());
            ParserFunction.RegisterFunction("SfAddWordImage", new AddWordImage());
            ParserFunction.RegisterFunction("SfAddWordTable", new AddWordTable());
            ParserFunction.RegisterFunction("SfAddWordParagraph", new AddWordParagraph());
            ParserFunction.RegisterFunction("SfApplyWordStyle", new ApplyWordStyle());
            ParserFunction.RegisterFunction("SfSaveWord", new SaveWord());

            ParserFunction.RegisterFunction("SfPdfNew", new CreatePdf());
            ParserFunction.RegisterFunction("SfPdfOpen", new OpenPdf());
            ParserFunction.RegisterFunction("SfSetPdfText", new SetPdfText());
            ParserFunction.RegisterFunction("SfSetPdfImage", new SetPdfImage());
            ParserFunction.RegisterFunction("SfSetPdfLine", new SetPdfLine());
            ParserFunction.RegisterFunction("SfSetPdfRectangle", new SetPdfRectangle());
            ParserFunction.RegisterFunction("SfSetPdfPie", new SetPdfPie());
            ParserFunction.RegisterFunction("SfSetPdfFont", new SetPdfFont());
            ParserFunction.RegisterFunction("SfSavePdf", new SavePdf());

            ParserFunction.RegisterFunction("InitSyncfusion", new SyncfusionInitFunction());
            ParserFunction.RegisterFunction("Login", new Proxy.LoginFunction());
            ParserFunction.RegisterFunction("GetDataFromServer", new Proxy.GetServerDataFunction());
            ParserFunction.RegisterFunction("AddOrderedData", new Proxy.AddOrderedDataFunction());
            CommonFunctions.RunScript(fileName);
        }
    }
    internal class SyncfusionInitFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();

            string licenseKey = Utils.GetSafeString(args, 0, "OTU1OTRAMzEzNzJlMzEyZTMwQ29abC8wTnI5eVUvM3d2Skt1TmJSZGFzd1pxZ3RtK3NOWldNTU8vdU1ETT0=");
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);

            return Variable.EmptyInstance;
        }
    }
}
