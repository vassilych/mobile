using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections.Generic;

using CoreGraphics;
using Foundation;
using UIKit;

using Syncfusion.SfBarcode.iOS;
using Syncfusion.SfBusyIndicator.iOS;
using Syncfusion.SfCalendar.iOS;
using Syncfusion.SfChart.iOS;
using Syncfusion.SfDataGrid;
using Syncfusion.SfGauge.iOS;
using Syncfusion.SfNumericUpDown.iOS;
//using Syncfusion.SfPicker.iOS;
using Syncfusion.SfPicker.XForms;
using Syncfusion.SfPicker.XForms.iOS;
using Syncfusion.GridCommon.ScrollAxis;

using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;

using SplitAndMerge;

// IMPORTANT - it works with Syncfusion 16.1.0.26.
// No garanties for other versions!!!
namespace scripting.iOS
{
  public class SfWidget : iOSVariable
  {
    SyncFusionType  m_type;

    SFBarcode       m_barcode;

    SFCalendar      m_calendar;
    SFDigitalGauge  m_digitalGauge;

    SFCircularGauge m_circularGauge;
    SFCircularScale m_circularScale;
    SFCircularScale m_circularScale2;
    SFNeedlePointer m_needlePointer;
    SFRangePointer  m_rangePointer1, m_rangePointer2;

    SFNumericUpDown m_stepper;

    SFBusyIndicator m_busyIndicator;

    SfPicker        m_picker;
    SFChart         m_chart;

    SfDataGrid      m_grid;
    int             m_rowIndex;
    DataModel       m_model;
    UIView          m_detailView;

    string          m_data;
    CGRect          m_rect;

    List<string>    m_strings;
    List<UIImage>   m_pics;

    UITextAlignment m_alignment = UITextAlignment.Left;
    UIColor m_bgColor           = UIColor.Clear;
    UIColor m_fontColor         = UIColor.Black;

    static bool     m_init;

    public enum SyncFusionType
    {
      NONE, QR_BARCODE, CODE39_BARCODE, CIRCULAR_GAUGE, DIGITAL_GAUGE, STEPPER, BUSY_INDICATOR,
      CALENDAR, PICKER, SPLINE_GRAPH, DOUGHNUT_GRAPH, DATA_GRID
    };

    public SyncFusionType SfType { get; set; }

    public SfWidget()
    {}
    public SfWidget(SyncFusionType type, string name, string text, CGRect rect) :
        base(UIType.CUSTOM, name)
    {
      m_type = type;
      m_data = text;
      m_rect = rect;
      SfType = type;
      switch (type) {
        case SyncFusionType.BUSY_INDICATOR:
          CreateBusyIndicator();
          break;
        case SyncFusionType.QR_BARCODE:
          CreateQRBarcode();
          break;
        case SyncFusionType.CODE39_BARCODE:
          CreateCode39Barcode();
          break;
        case SyncFusionType.PICKER:
          CreatePicker();
          break;
        case SyncFusionType.STEPPER:
          CreateStepper();
          break;
        case SyncFusionType.CIRCULAR_GAUGE:
          CreateCircularGauge();
          break;
        case SyncFusionType.DIGITAL_GAUGE:
          CreateDigitalGauge();
          break;
        case SyncFusionType.DATA_GRID:
          CreateDataGrid();
          break;
        case SyncFusionType.CALENDAR:
          CreateCalendar();
          break;
        case SyncFusionType.DOUGHNUT_GRAPH:
        case SyncFusionType.SPLINE_GRAPH:
          CreateGraph();
          break;
      }
      ViewX.Tag = ++m_currentTag;
    }
    public override Variable Clone()
    {
      SfWidget newVar = (SfWidget)this.MemberwiseClone();
      return newVar;
    }

    public override iOSVariable GetWidget(string widgetType, string widgetName, string initArg, CGRect rect)
    {
      switch (widgetType) {
        case "SfStepper":
          return new SfWidget(SfWidget.SyncFusionType.STEPPER, widgetName, initArg, rect);
        case "SfCalendar":
          return new SfWidget(SfWidget.SyncFusionType.CALENDAR, widgetName, initArg, rect);
        case "SfQRBarcode":
          return new SfWidget(SfWidget.SyncFusionType.QR_BARCODE, widgetName, initArg, rect);
        case "SfCode39Barcode":
          return new SfWidget(SfWidget.SyncFusionType.CODE39_BARCODE, widgetName, initArg, rect);
        case "SfCircularGauge":
          return new SfWidget(SfWidget.SyncFusionType.CIRCULAR_GAUGE, widgetName, initArg, rect);
        case "SfDigitalGauge":
          return new SfWidget(SfWidget.SyncFusionType.DIGITAL_GAUGE, widgetName, initArg, rect);
        case "SfBusyIndicator":
          return new SfWidget(SfWidget.SyncFusionType.BUSY_INDICATOR, widgetName, initArg, rect);
        case "SfDataGrid":
          return new SfWidget(SfWidget.SyncFusionType.DATA_GRID, widgetName, initArg, rect);
        case "SfDoughnutGraph":
          return new SfWidget(SfWidget.SyncFusionType.DOUGHNUT_GRAPH, widgetName, initArg, rect);
        case "SfSplineGraph":
          return new SfWidget(SfWidget.SyncFusionType.SPLINE_GRAPH, widgetName, initArg, rect);
        case "SfPicker":
          return new SfWidget(SfWidget.SyncFusionType.PICKER, widgetName, initArg, rect);
        case "SfImageEditor":
          return new ImageEditor(widgetName, initArg, rect);
      }
      return null;
    }
  
    public override bool SetFontSize(double val)
    {
      if (m_stepper != null) {
        m_stepper.FontSize = (nfloat)val;
      } else {
        return false;
      }
      return true;
    }

    void CreateBusyIndicator()
    {
      m_busyIndicator = new SFBusyIndicator();
      m_busyIndicator.Frame = m_rect;

      m_busyIndicator.BackgroundColor = UIColor.Clear;
      m_busyIndicator.Foreground = UIColor.Blue;
      m_busyIndicator.ViewBoxWidth = 100;
      m_busyIndicator.ViewBoxHeight = 100;
      SetBusyIndicatorType(m_data);

      ViewX = m_busyIndicator;
    }

    void SetBusyIndicatorType(string strType)
    {
      switch (strType) {
        case "ball":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeBall;
          break;
        case "battery":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeBattery;
          break;
        case "box":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeBox;
          break;
        case "doublecircle":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeDoubleCircle;
          break;
        case "ecg":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeECG;
          break;
        case "gear":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeGear;
          break;
        case "globe":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeGlobe;
          break;
        case "pulsing":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeHorizontalPulsingBox;
          break;
        case "movie":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeMovieTimer;
          break;
        case "print":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypePrint;
          break;
        case "rectangle":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeRectangle;
          break;
        case "rollingball":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeRollingBall;
          break;
        case "singlecircle":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeSingleCircle;
          break;
        case "slicedcircle":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeSlicedCircle;
          break;
        case "zooming":
          m_busyIndicator.AnimationType = SFBusyIndicatorAnimationType.SFBusyIndicatorAnimationTypeZoomingTarget;
          break;
      }
    }

    void CreatePicker()
    {
      if (!m_init) {
        Forms.Init();
        SfPickerRenderer.Init();
        m_init = true;
      }

      m_picker = new SfPicker();
      ViewX = UtilsiOS.ConvertFormsToNative(m_picker, m_rect);

      string str1 = "", str2 = "", str3 = "", str4 = "", str5 = "";
      Utils.Extract(m_data, ref str1, ref str2, ref str3, ref str4, ref str5);
      if (!string.IsNullOrEmpty(str1)) {
        double rowHeight = Utils.ConvertToDouble(str1);
        if (rowHeight > 0) {
          rowHeight = AutoScaleFunction.TransformSize((int)rowHeight,
                                                      (int)UtilsiOS.GetScreenSize().Width);
          m_picker.ItemHeight = rowHeight;
        }
      }

      m_picker.ShowHeader = !string.IsNullOrEmpty(str2);
      if (m_picker.ShowHeader) {
        m_picker.HeaderHeight = 40;
        m_picker.HeaderText = str2;
      }
      m_picker.ShowColumnHeader = !string.IsNullOrEmpty(str3);
      if (m_picker.ShowColumnHeader) {
        m_picker.ColumnHeaderHeight = 40;
        m_picker.ColumnHeaderText = str3;
      }

      m_picker.ShowFooter = false;
      m_picker.SelectedIndex = 0;

      m_picker.PickerMode = PickerMode.Default;
      m_picker.PickerWidth = m_rect.Width;
      m_picker.PickerHeight = m_rect.Height;

      m_picker.BackgroundColor = Color.Transparent;
      m_picker.HeaderBackgroundColor = Color.Transparent;
      m_picker.ColumnHeaderBackgroundColor = Color.Transparent;
      m_picker.HeaderTextColor = Color.Black;
      m_picker.UnSelectedItemTextColor = Color.LightGray;
      m_picker.SelectedItemTextColor = Color.Black;

      m_picker.SelectionChanged += (sender, e) => {
        ActionDelegate?.Invoke(WidgetName, e.NewValue.ToString());
      };
       
      m_picker.OnPickerItemLoaded += (sender, e) => {
        if (m_pics == null) {
          return;
        }
        int row = e.Row;

        CustomRowView view = new CustomRowView(m_rect.Width);

        if (m_pics.Count > row) {
          UIImageView rowPic = new UIImageView(m_pics[row]);
          view.AddSubview(rowPic);
        }

        if (m_strings != null && m_strings.Count > row) {
          UILabel rowText = new UILabel();
          rowText.Text = m_strings[row];
          rowText.TextAlignment = m_alignment;
          rowText.BackgroundColor = m_bgColor;
          //rowText.BackgroundColor = UIColor.Clear;
          rowText.TextColor = m_fontColor;
          view.AddSubview(rowText);
        }

        e.View = view.ToView();
      };

      m_picker.HeaderFontSize = m_picker.ColumnHeaderFontSize =
        m_picker.SelectedItemFontSize = m_picker.UnSelectedItemFontSize = 15;
    }
    void CreateDataGrid()
    {
      m_grid = new SfDataGrid();
      m_grid.Frame = m_rect;

      m_grid.AutoGenerateColumns = false;
      m_grid.ShowRowHeader = false;
      m_grid.HeaderRowHeight = 45;
      m_grid.RowHeight = 45;
      m_grid.ColumnSizer = ColumnSizer.Star;
      m_grid.AllowDraggingRow = true;
      m_grid.AllowSwiping = true;
      m_grid.AllowResizingColumn = true;
      m_grid.AllowSorting = true;
      //m_grid.AllowTriStateSorting = true;
      m_grid.SortColumnsChanged += (sender, e) => {
        var elem = e.AddedItems.Count   > 0 ? e.AddedItems[0] :
                   e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
        if (elem == null) {
          return;
        }
        m_model.Sort(elem.ColumnName, elem.SortDirection == System.ComponentModel.ListSortDirection.Ascending);
      };
      m_grid.QueryRowDragging += (sender, e) => {
        if (e.Reason == QueryRowDraggingReason.DragEnded) {
          var from = e.From - 1;
          var to   = e.To - 2;
          var totalHeight = m_grid.RowColumnIndexToPoint(
            new RowColumnIndex(this.LastIndex, 0)).Y + m_grid.RowHeight;
          if (e.To == LastIndex &&
              Math.Ceiling(e.Position.Y + (m_grid.RowHeight * 0.45)) > totalHeight) {
            to++;
          }
          var init = e.RowData;
          Console.WriteLine("Drag {0} {1} -- > {2} {3}",
                            from, e.RowData, to, e.CurrentRowData);
          m_model.SwapRows(from, to);
        }
      };

      m_grid.SwipeStarted += (sender, e) => {
        m_grid.MaxSwipeOffset = (int)(m_grid.Frame.Width / 3);
      };
      m_grid.SwipeEnded += (sender, e) => {
        m_rowIndex = e.RowIndex - 1;
        //Console.WriteLine("SwipeEnded {0}: {1}", m_rowIndex, m_model.ToString());
      };

      m_model = new DataModel(m_grid);
      m_model.PropertyChanged += (sender, e) => {
        m_grid.ItemsSource = m_model.DataPoints;
      };

      ViewX = m_grid;
    }

    void AddSwipeDelete()
    {
      UIButton rightSwipeViewText = new UIButton();
      rightSwipeViewText.SetTitle("Delete", UIControlState.Normal);
      rightSwipeViewText.SetTitleColor(UIColor.White, UIControlState.Normal);
      rightSwipeViewText.VerticalAlignment = UIControlContentVerticalAlignment.Center;
      rightSwipeViewText.BackgroundColor = UIColor.FromRGB(220, 89, 95);
      rightSwipeViewText.TouchDown += (sender, e) => {
        m_model.RemovePoint(m_rowIndex);
      };

      UIButton rightSwipeViewButton = new UIButton();
      rightSwipeViewButton.SetImage(UtilsiOS.LoadImage("Delete"), UIControlState.Normal);
      rightSwipeViewButton.BackgroundColor = UIColor.FromRGB(220, 89, 95);
      rightSwipeViewButton.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
      rightSwipeViewButton.TouchDown += (sender, e) => {
        m_model.RemovePoint(m_rowIndex);
      };

      CustomSwipeView rightSwipeView = new CustomSwipeView();
      rightSwipeView.AddSubview(rightSwipeViewButton);
      rightSwipeView.AddSubview(rightSwipeViewText);

      m_grid.RightSwipeView = rightSwipeView;
    }
    void AddSwipeEdit()
    {

      UIButton leftSwipeViewText = new UIButton();
      leftSwipeViewText.SetTitle("Edit", UIControlState.Normal);
      leftSwipeViewText.SetTitleColor(UIColor.White, UIControlState.Normal);
      leftSwipeViewText.VerticalAlignment = UIControlContentVerticalAlignment.Center;
      leftSwipeViewText.BackgroundColor = UIColor.FromRGB(0, 158, 218);
      leftSwipeViewText.TouchDown += EditDataGrid;

      UIButton leftSwipeViewButton = new UIButton();
      leftSwipeViewButton.SetImage(UtilsiOS.LoadImage("Edit"), UIControlState.Normal);
      leftSwipeViewButton.BackgroundColor = UIColor.FromRGB(0, 158, 218);
      leftSwipeViewButton.TouchDown += EditDataGrid;

      CustomSwipeView leftSwipeView = new CustomSwipeView();
      leftSwipeView.AddSubview(leftSwipeViewButton);
      leftSwipeView.AddSubview(leftSwipeViewText);

      m_grid.LeftSwipeView = leftSwipeView;
    }

    public int LastIndex {
      get {
        return (m_grid.GroupColumnDescriptions.Count > 0 ?
                m_grid.View.TopLevelGroup.DisplayElements.Count :
                m_grid.View.Records.Count);
      }
    }
    void EditDataGrid(object sender, EventArgs e)
    {
      DataPoint point = m_model.GetPoint(m_rowIndex);
      Console.WriteLine("EditDataGrid {0}: {1} ({2})", m_rowIndex, point, m_model);
      if (point == null) {
        return;
      }
      m_detailView = new UIView();
      m_detailView.BackgroundColor = UIColor.LightGray;
      m_detailView.Frame = new CGRect(10, m_grid.Frame.Top, m_grid.Frame.Width - 10, 240);

      AppDelegate.GetCurrentView().AddSubview(m_detailView);
      List<UITextField> textViews = new List<UITextField>(DataModel.ColNames.Count);

      nfloat y = 0;
      for (int i = 0; i < DataModel.ColNames.Count; i++) {
        UILabel coli      = new UILabel();
        coli.Frame = new CGRect(10, y, m_detailView.Frame.Right / 2, 20);

        UITextField texti = new UITextField();
        textViews.Add(texti);
        texti.Frame = new CGRect(coli.Frame.Right, y, m_detailView.Frame.Right, 20);
        coli.Text         = DataModel.ColNames[i];
        texti.Text        = point.GetStringValue(i);

        m_detailView.AddSubview(coli);
        m_detailView.AddSubview(texti);

        y = coli.Frame.Bottom + 10;
      }

      UIButton save = new UIButton();
      save.SetTitle("Save", UIControlState.Normal);
      save.BackgroundColor = UIColor.DarkGray;
      save.Font = UIFont.FromName("Helvetica-Bold", 12f);
      save.Frame = new CGRect(m_detailView.Frame.Right / 4, m_detailView.Frame.Bottom - 220, 50, 20);
      save.TouchDown += (sender2, e2) => {
        m_detailView.RemoveFromSuperview();
        DataPoint dataPoint = m_model.GetPoint(m_rowIndex);
        for (int i = 0; i < DataModel.ColNames.Count; i++) {
          dataPoint.Set(i, textViews[i].Text);
        }
        m_model.Reload();
      };

      UIButton cancel = new UIButton();
      cancel.SetTitle("Cancel", UIControlState.Normal);
      cancel.TouchDown += (sender2, e2) => {
        m_detailView.RemoveFromSuperview();
        m_grid.ItemsSource = m_model.DataPoints;
      };
      cancel.BackgroundColor = UIColor.DarkGray;
      cancel.Font = UIFont.FromName("Helvetica-Bold", 12f);
      cancel.Frame = new CGRect(save.Frame.Right + 10, save.Frame.Top, 80, 20);

      m_detailView.AddSubview(save);
      m_detailView.AddSubview(cancel);
    }

    void CreateCalendar()
    {
      m_calendar = new SFCalendar();
      m_calendar.Frame = m_rect;

      m_calendar.BackgroundColor = UIColor.Clear;
      m_calendar.ViewMode = SFCalendarViewMode.SFCalendarViewModeMonth;
      m_calendar.HeaderHeight = 40;

      ViewX = m_calendar;
    }

    public bool AddAppointment(string subject, string startStr, string endStr, string colorStr,
                              string dateFormat)
    {
      if (m_calendar == null) {
        return false;
      }
      SFAppointment appointment = new SFAppointment();
      appointment.Subject = (NSString)subject;
      DateTime dt = DateTime.ParseExact(startStr, dateFormat, null);
      appointment.StartTime = (NSDate)DateTime.SpecifyKind(dt, DateTimeKind.Local);
      dt = DateTime.ParseExact(endStr, dateFormat, null);
      appointment.EndTime = (NSDate)DateTime.SpecifyKind(dt, DateTimeKind.Local);
      appointment.AppointmentBackground = UtilsiOS.String2Color(colorStr);

      var appointments = m_calendar.Appointments;
      if (appointments == null) {
        appointments = new NSMutableArray();
      }
      appointments.Add(appointment);
      m_calendar.Appointments = appointments;

      return true;
    }

    void CreateQRBarcode()
    {
      m_barcode = new SFBarcode();
      m_barcode.Frame = m_rect;

      m_barcode.BackgroundColor = UIColor.Clear;
      m_barcode.Text = (NSString)m_data;
      m_barcode.Symbology = SFBarcodeSymbolType.SFBarcodeSymbolTypeQRCode;
      SFQRBarcodeSettings settings = new SFQRBarcodeSettings();
      settings.XDimension = 5;
      m_barcode.SymbologySettings = settings;

      ViewX = m_barcode;
    }
    void CreateCode39Barcode()
    {
      m_barcode = new SFBarcode();
      m_barcode.Frame = m_rect;

      m_barcode.BackgroundColor = UIColor.Clear;
      m_barcode.Text = (NSString)m_data;
      m_barcode.Symbology = SFBarcodeSymbolType.SFBarcodeSymbolTypeCode39;
      SFCode39Settings settings = new SFCode39Settings();
      settings.BarHeight = 80;
      settings.NarrowBarWidth = 1f;
      m_barcode.SymbologySettings = settings;

      ViewX = m_barcode;
    }
    void CreateStepper()
    {
      m_stepper = new SFNumericUpDown();
      m_stepper.Frame = m_rect;

      double minValue = 0, maxValue = 1, currValue = 0, step = 1.0;
      Utils.Extract(m_data, ref currValue, ref minValue, ref maxValue, ref step);
      m_stepper.AllowNull = true;
      m_stepper.PercentDisplayMode = SFNumericUpDownPercentDisplayMode.Value;
      m_stepper.ValueChangeMode = SFNumericUpDownValueChangeMode.OnLostFocus;
      m_stepper.Value = (nfloat)currValue;
      m_stepper.StepValue = (nfloat)step;
      m_stepper.Minimum = (nfloat)minValue;
      m_stepper.Maximum = (nfloat)maxValue;
      m_stepper.Culture = NSLocale.CurrentLocale; //new NSLocale("en_US");
      m_stepper.MaximumDecimalDigits = Utils.GetNumberOfDigits(m_data, 3);

      //m_stepper.BackgroundColor = UIColor.White;
      //m_stepper.UpdownButtonColor = UIColor.Clear;

      m_stepper.ValueChanged += (sender, e) =>  {
        ActionDelegate?.Invoke(WidgetName, e.Value.ToString());
      };

      ViewX = m_stepper;
    }
    void CreateDigitalGauge()
    {
      m_digitalGauge = new SFDigitalGauge();
      m_digitalGauge.Frame = m_rect;

      m_digitalGauge.CharacterHeight = 36;
      m_digitalGauge.CharacterWidth = 17;
      m_digitalGauge.VerticalPadding = 10;
      m_digitalGauge.SegmentWidth = 3;

      m_digitalGauge.CharacterType = SFDigitalGaugeCharacterType.SFDigitalGaugeCharacterTypeEightCrossEightDotMatrix;
      m_digitalGauge.StrokeType = SFDigitalGaugeStrokeType.SFDigitalGaugeStrokeTypeTriangleEdge;
      m_digitalGauge.Value = (NSString)NSDate.Now.ToString();
      m_digitalGauge.DimmedSegmentAlpha = 0.11f;
      m_digitalGauge.BackgroundColor = UIColor.White;
      m_digitalGauge.CharacterColor = UIColor.Blue;
      m_digitalGauge.DimmedSegmentColor = UIColor.LightGray;

      ViewX = m_digitalGauge;
    }
    void CreateCircularGauge()
    {
      m_circularGauge = new SFCircularGauge();
      m_circularGauge.Frame = m_rect;

      ObservableCollection<SFCircularScale> scales = new ObservableCollection<SFCircularScale>();

      m_circularScale = new SFCircularScale();
      m_circularScale.StartValue = 0;
      m_circularScale.EndValue = 100;
      m_circularScale.Interval = 10;
      m_circularScale.StartAngle = 34;
      m_circularScale.SweepAngle = 325;
      m_circularScale.ShowTicks = true;
      m_circularScale.ShowLabels = true;
      m_circularScale.ShowRim = true;
      m_circularScale.LabelColor = UIColor.Gray;
      m_circularScale.LabelOffset = 0.8f;
      m_circularScale.MinorTicksPerInterval = 0;
      ObservableCollection<SFCircularPointer> pointers = new ObservableCollection<SFCircularPointer>();

      m_needlePointer = new SFNeedlePointer();
      m_needlePointer.Value = 60;
      m_needlePointer.Color = UIColor.Gray;
      m_needlePointer.KnobRadius = 12;
      m_needlePointer.KnobColor = UIColor.FromRGB(43, 191, 184);
      m_needlePointer.Width = 3;
      m_needlePointer.LengthFactor = 0.8f;
      //  needlePointer.PointerType = SFCiruclarGaugePointerType.SFCiruclarGaugePointerTypeBar;

      m_rangePointer1 = new SFRangePointer();
      m_rangePointer1.Value = 60;
      m_rangePointer1.Color = UIColor.FromRGB(43, 191, 184);
      m_rangePointer1.Width = 5;
      m_rangePointer1.EnableAnimation = true;

      m_rangePointer2 = new SFRangePointer();
      m_rangePointer2.RangeStart = 60;
      m_rangePointer2.Value = 100;
      m_rangePointer2.Color = UIColor.FromRGB((byte)209, (byte)70, (byte)70);
      m_rangePointer2.Width = 5;
      m_rangePointer2.EnableAnimation = true;

      pointers.Add(m_needlePointer);
      pointers.Add(m_rangePointer1);
      pointers.Add(m_rangePointer2);

      SFTickSettings minor = new SFTickSettings();
      minor.Size = 4;
      minor.Color = UIColor.FromRGB(68, 68, 68);
      minor.Width = 3;
      m_circularScale.MinorTickSettings = minor;

      SFTickSettings major = new SFTickSettings();
      major.Size = 12;
      //major.Offset = 0.01f;
      major.Color = UIColor.FromRGB(68, 68, 68);
      major.Width = 2;
      m_circularScale.MajorTickSettings = major;
      m_circularScale.Pointers = pointers;

      SFGaugeHeader header = new SFGaugeHeader();
      header.Text = (NSString)"Speedometer";
      header. Position = new CGPoint(0.5f, 0.6f);
      header.TextColor = UIColor.Gray;

      scales.Add(m_circularScale);
      m_circularGauge.Scales = scales;
      m_circularGauge.Headers.Add(header);

      ViewX = m_circularGauge;
    }
    void CreateScale2IfNeeded()
    {
      if (m_circularScale2 != null) {
        return;
      }
      m_circularScale2 = new SFCircularScale();
      m_circularScale2.StartValue = m_circularScale.StartValue;
      m_circularScale2.EndValue = m_circularScale.EndValue;
      m_circularScale2.Interval = m_circularScale.Interval;
      m_circularScale2.StartAngle = m_circularScale.StartAngle;
      m_circularScale2.SweepAngle = m_circularScale.SweepAngle;
      m_circularScale2.ShowTicks = false;
      m_circularScale2.ShowLabels = true;
      m_circularScale2.ShowRim = true;
      m_circularScale2.LabelColor = UIColor.Black;
      m_circularScale2.RimColor = UIColor.Blue;
      m_circularScale2.LabelOffset = 0.55f;
      //m_circularScale2.MinorTicksPerInterval = 0;
      m_circularScale2.ScaleStartOffset = 0.63f;
      m_circularScale2.ScaleEndOffSet = 0.65f;

      m_circularGauge.Scales.Add(m_circularScale2);
    }
    void CreateGraph()
    {
      m_chart = new SFChart();
      m_chart.Frame = m_rect;

      m_chart.Title.Text = new NSString("Graph");

      SFCategoryAxis primaryAxis = new SFCategoryAxis();
      m_chart.PrimaryAxis = primaryAxis;
      primaryAxis.LabelPlacement = SFChartLabelPlacement.BetweenTicks;
      m_chart.SecondaryAxis = new SFNumericalAxis();

      ViewX = m_chart;
    }
    public override void AddData(List<string> data, string varName, string title, string extra)
    {
      if (m_grid != null) {
        if (title == "columns") {
          m_model.AddColumns(data);
        } else if (title == "item") {
          m_model.AddPoint(data);
        }
      } else if (m_chart != null) {
        var collection = new ObservableCollection<DataPoint>();
        for (int i = 0; i < data.Count - 1; i+=2) {
          string coord = data[i];
          double value = Utils.ConvertToDouble(data[i + 1]);
          collection.Add(new DataPoint(coord, value));
        }
        if (SfType == SyncFusionType.DOUGHNUT_GRAPH) {
          SFDoughnutSeries doughnutSeries = new SFDoughnutSeries();
          doughnutSeries.ItemsSource = collection;
          doughnutSeries.XBindingPath = "XValue";
          doughnutSeries.YBindingPath = "YValue";
          doughnutSeries.EnableTooltip = true;
          doughnutSeries.EnableAnimation = true;
          if (!string.IsNullOrEmpty(title)) {
            //doughnutSeries.Label = title;
            m_chart.Title.Text = (NSString)title;
          }
          doughnutSeries.DataMarker.ShowLabel = true;
          doughnutSeries.DataMarker.LabelContent = title == "values" ? SFChartLabelContent.YValue :
                                                                       SFChartLabelContent.Percentage;
          doughnutSeries.DataMarkerPosition = SFChartCircularSeriesLabelPosition.OutsideExtended;
          doughnutSeries.LegendIcon = SFChartLegendIcon.Circle;
          if (!string.IsNullOrEmpty(extra)) {
            var angles = extra.Split(new char[] { ',', ':' });
            doughnutSeries.StartAngle = Utils.ConvertToDouble(angles[0]);
            if (angles.Length > 1) {
              doughnutSeries.EndAngle = Utils.ConvertToDouble(angles[1]);
            }
          }
          m_chart.Series.Add(doughnutSeries);
        } else if (SfType == SyncFusionType.SPLINE_GRAPH) {
          SFSplineSeries splineSeries = new SFSplineSeries();
          splineSeries.ItemsSource = collection;
          splineSeries.XBindingPath = "XValue";
          splineSeries.YBindingPath = "YValue";
          splineSeries.EnableTooltip = true;
          splineSeries.EnableAnimation = true;
          splineSeries.Label = title;
          splineSeries.DataMarker.MarkerHeight = 5;
          splineSeries.DataMarker.MarkerWidth = 5;
          splineSeries.DataMarker.ShowMarker = true;
          splineSeries.LegendIcon = SFChartLegendIcon.Rectangle;
          if (!string.IsNullOrEmpty(extra)) {
            var colors = extra.Split(new char[] { ',', ':' });
            splineSeries.Color = UtilsiOS.String2Color(colors[0]);
            if (colors.Length > 1) {
              var color = UtilsiOS.String2Color(colors[1]);
              splineSeries.DataMarker.MarkerColor = color;
            }
          }
          m_chart.Series.Add(splineSeries);
        }

        if (!string.IsNullOrEmpty(title)) {
          m_chart.Legend.Visible = true;
          m_chart.Legend.DockPosition = SFChartLegendPosition.Top;
          //m_chart.Legend.
          m_chart.Legend.ToggleSeriesVisibility = true;
        }
      } else if (m_picker != null) {
        m_strings = data;
        m_picker.ItemsSource = data;
      }
    }

    public override void AddImages(List<UIImage> images, string varName, string title)
    {
      m_pics = images;
      if (m_picker != null && m_strings == null) {
        m_picker.ItemsSource = images;
      }
    }
    public override bool SetValue(string arg1, string arg2 = "")
    {
      if (string.IsNullOrEmpty(arg2)) {
        arg2 = arg1;
        arg1 = "value";
      }
      double valueNum = Utils.ConvertToDouble(arg2);
      if (m_circularGauge != null) {
        switch (arg1) {
          case "from":
            m_circularScale.StartValue = (nfloat)valueNum;
            break;
          case "to":
            m_rangePointer2.Value = (nfloat)valueNum;
            m_circularScale.EndValue = (nfloat)valueNum;
            break;
          case "step":
            m_circularScale.Interval = (nfloat)valueNum;
            break;
          case "value":
            m_needlePointer.Value = (nfloat)valueNum;
            break;
          case "start_range2":
            m_rangePointer1.Value = (nfloat)valueNum;
            m_rangePointer2.RangeStart = (nfloat)valueNum;
            break;
          case "start_angle":
            m_circularScale.StartAngle = (nfloat)valueNum;
            break;
          case "sweep_angle":
            m_circularScale.SweepAngle = (nfloat)valueNum;
            break;
          case "color_needle":
            m_needlePointer.Color = UtilsiOS.String2Color(arg2);
            break;
          case "radius_knob":
            m_needlePointer.KnobRadius = (nfloat)valueNum;
            break;
          case "color_knob":
            m_needlePointer.KnobColor = UtilsiOS.String2Color(arg2);
            break;
          case "color_labels":
            m_circularScale.LabelColor = UtilsiOS.String2Color(arg2);
            m_circularGauge.Headers[0].TextColor = UtilsiOS.String2Color(arg2);
            break;
          case "color_range1":
            m_rangePointer1.Color = UtilsiOS.String2Color(arg2);
            break;
          case "color_range2":
            m_rangePointer2.Color = UtilsiOS.String2Color(arg2);
            break;
          case "color_minorticks":
            m_circularScale.MinorTickSettings.Color = UtilsiOS.String2Color(arg2);
            break;
          case "color_majorticks":
            m_circularScale.MajorTickSettings.Color = UtilsiOS.String2Color(arg2);
            break;
          case "scale2_from":
            CreateScale2IfNeeded();
            m_circularScale2.StartValue = (nfloat)valueNum;
            break;
          case "scale2_to":
            CreateScale2IfNeeded();
            m_circularScale2.EndValue = (nfloat)valueNum;
            break;
          case "scale2_interval":
            CreateScale2IfNeeded();
            m_circularScale2.Interval = (nfloat)valueNum;
            break;
          case "scale2_rimcolor":
            CreateScale2IfNeeded();
            m_circularScale2.RimColor = UtilsiOS.String2Color(arg2);
            break;
          case "scale2_labelcolor":
            CreateScale2IfNeeded();
            m_circularScale2.LabelColor = UtilsiOS.String2Color(arg2);
            break;
        }
      } else if (m_digitalGauge != null) {
        switch (arg1) {
          case "value":
            m_digitalGauge.Value = (NSString)arg2;
            break;
        }
      } else if (m_picker != null) {
        switch (arg1) {
          case "alignment":
            Tuple<UIControlContentHorizontalAlignment, UITextAlignment> al =
              UtilsiOS.GetAlignment(arg2);
            m_alignment = al.Item2;
            break;
          case "headerHeight":
            m_picker.HeaderHeight = (nfloat)valueNum;
            break;
          case "headerText":
            m_picker.ShowHeader = true;
            m_picker.HeaderText = (NSString)arg2;
            break;
          case "colHeaderHeight":
            m_picker.ColumnHeaderHeight = (nfloat)valueNum;
            break;
          case "colHeaderText":
            m_picker.ShowColumnHeader = true;
            m_picker.ColumnHeaderText = (NSString)arg2;
            break;
          case "index":
            m_picker.SelectedIndex = (int)valueNum;
             break;
          case "pickerMode":
            m_picker.PickerMode = arg2.Equals("Default", StringComparison.OrdinalIgnoreCase) ?
              PickerMode.Default : PickerMode.Dialog;
            break;
        }
      } else if (m_grid != null) {
        switch (arg1) {
          case "swipe_delete":
            AddSwipeDelete();
            break;
          case "swipe_edit":
            AddSwipeEdit();
            break;
          case "allow_drag":
            m_grid.AllowDraggingRow = valueNum > 0;
            break;
          case "allow_swipe":
            m_grid.AllowSwiping = valueNum > 0;
            break;
          case "allow_resize":
            m_grid.AllowResizingColumn = valueNum > 0;
            break;
          case "allow_sort":
            m_grid.AllowSorting = valueNum > 0;
            break;
        }
     } else if (m_barcode != null) {
        var settings39 = m_barcode.SymbologySettings as SFCode39Settings;
        if (settings39 != null) {
          switch (arg1) {
            case "bar_height":
              settings39.BarHeight = (nfloat)valueNum;
              break;
            case "bar_width":
              settings39.NarrowBarWidth = (nfloat)valueNum;
              break;
          }
        }
      } else if (m_chart != null) {
        SFNumericalAxis numericalaxis = m_chart.SecondaryAxis as SFNumericalAxis;
        switch (arg1) {
          case "primary_axis":
            m_chart.PrimaryAxis.Title.Text = (NSString)arg2;
            break;
          case "secondary_axis":
            m_chart.SecondaryAxis.Title.Text = (NSString)arg2;
            break;
          case "x_min":
            numericalaxis.Minimum = new NSNumber(valueNum);
            break;
          case "x_max":
            numericalaxis.Maximum = new NSNumber(valueNum);
            break;
          case "x_interval":
            numericalaxis.Interval = new NSNumber(valueNum);
            break;
        }
      } else if (m_stepper != null) {
        switch (arg1) {
          case "min":
            m_stepper.Minimum   = (nfloat)valueNum;
            break;
          case "max":
            m_stepper.Maximum   = (nfloat)valueNum;
            break;
          case "step":
            m_stepper.StepValue = (nfloat)valueNum;
            m_stepper.MaximumDecimalDigits = Utils.GetNumberOfDigits(arg2);
            break;
          case "value":
            m_stepper.Value = (nfloat)valueNum;
            break;
          case "buttons":
            switch(arg2) {
              case "left": m_stepper.SpinButtonAlignment = SFNumericUpDownSpinButtonAlignment.Left;
                break;
              case "right":
                m_stepper.SpinButtonAlignment = SFNumericUpDownSpinButtonAlignment.Right;
                break;
              default: m_stepper.SpinButtonAlignment = SFNumericUpDownSpinButtonAlignment.Both;
                break;
            }
            break;
        }
      } else if (m_busyIndicator != null) {
        if (arg1 == "bg_color") {
          m_busyIndicator.BackgroundColor = UtilsiOS.String2Color(arg2);
        } else if (arg1 == "color") {
          m_busyIndicator.Foreground = UtilsiOS.String2Color(arg2);
        } else if (arg1 == "secondary_color") {
          m_busyIndicator.SecondaryColor = UtilsiOS.String2Color(arg2);
        } else if (arg1 == "duration") {
          m_busyIndicator.Duration = (nfloat)valueNum;
        } else {
          SetBusyIndicatorType(arg2);
        }
      }
      return true;
    }
    public override double GetValue()
    {
      double result = 0;
      if (m_stepper != null) {
        result = Utils.ConvertToDouble(m_stepper.Value);
      } else if (m_needlePointer != null) {
        result = m_needlePointer.Value;
      } else if (m_picker != null) {
        result = Utils.ConvertToDouble(m_picker.SelectedIndex);
      }

      return result;
    }
    public override string GetText()
    {
      string text = "";
      if (m_barcode != null) {
        text = m_barcode.Text;
      } else if (m_circularGauge != null) {
        text = m_circularGauge.Headers[0].Text;
      } else if (m_picker != null) {
        if (m_picker.SelectedItem != null) {
          text = m_picker.SelectedItem.ToString();
        } else {
          var list = m_picker.ItemsSource as List<string>;
          if (list != null) {
            int index = (int)m_picker.SelectedIndex;
            text = list[index];
          }
        }
      }
      return text;
    }
    public override bool SetText(string text, string alignment = null, bool tiggered = false)
    {
      if (m_barcode != null) {
        m_barcode.Text = (NSString)text;
      } else if (m_chart != null) {
        m_chart.Title.Text = (NSString)text;
      } else if (m_circularGauge != null) {
        m_circularGauge.Headers[0].Text = (NSString)text;
      } else if (m_picker != null) {
        var list = m_picker.ItemsSource as List<string>;
        if (list != null) {
          int index = list.FindIndex(item => item == text);
          if (index >= 0) {
            m_picker.SelectedIndex = index;
          }
        }
      } else {
        return false;
      }
      return true;
    }
    public override bool SetBackgroundColor(string colorStr, double alpha = 1.0)
    {
      var color = UtilsiOS.String2Color(colorStr, alpha);
      m_bgColor = color;

      if (m_picker != null) {
        m_picker.BackgroundColor = color.ToColor();
      } else if (m_chart != null) {
        m_chart.BackgroundColor = color;
      } else if (m_stepper != null) {
        m_stepper.BackgroundColor = color;
      } else if (m_grid != null) {
        m_grid.BackgroundColor = color;
      } else if (m_barcode != null) {
        m_barcode.BackgroundColor = color;
      } else if (m_digitalGauge != null) {
        m_digitalGauge.BackgroundColor = color;
      } else if (m_circularGauge != null) {
        m_circularGauge.BackgroundColor = color;
        m_circularScale.BackgroundColor = color;
        //m_needlePointer.BackgroundColor = color;
        //m_rangePointer1.BackgroundColor = color;
        //m_rangePointer2.BackgroundColor = color;
      } else if (m_busyIndicator != null) {
        m_busyIndicator.BackgroundColor = color;
      } else {
        ViewX.BackgroundColor = color;
      }

      return true;
    }
    public override bool SetFontColor(string colorStr)
    {
      UIColor color = UtilsiOS.String2Color(colorStr);
      m_fontColor = color;

      if (m_picker != null) {
        m_picker.HeaderTextColor = color.ToColor();
        m_picker.ColumnHeaderTextColor = color.ToColor();
        m_picker.UnSelectedItemTextColor = color.ToColor();
        m_picker.SelectedItemTextColor = color.ToColor();
      } else if (m_chart != null && m_chart.Title != null) {
        m_chart.Title.TextColor = color;
      } else if (m_stepper != null) {
        m_stepper.TextColor = color;
        m_stepper.BorderColor = color;
        m_stepper.UpdownButtonColor = color;

        var settingsUp = m_stepper.IncrementButtonSettings;
        settingsUp.ButtonFontColor = color;
        m_stepper.IncrementButtonSettings = settingsUp;
        var settingsDown = m_stepper.DecrementButtonSettings;
        settingsDown.ButtonFontColor = color;
        m_stepper.DecrementButtonSettings = settingsDown;
      } else if (m_barcode != null) {
        m_barcode.TextColor = color;
      } else if (m_digitalGauge != null) {
        //m_digitalGauge.DimmedSegmentAlpha = 30;
        m_digitalGauge.DimmedSegmentColor = color;
        m_digitalGauge.CharacterColor = color;
      } else if (m_circularGauge != null && m_circularGauge.Headers.Count > 0) {
        m_circularGauge.Headers[0].TextColor = color;
      } else {
        return false;
      }

      return true;
    }
    public override void AddAction(string varName,
                                   string strAction, string argument = "")
    {
      ActionDelegate += (arg1, arg2) => {
        UIVariable.GetAction(strAction, varName, "\"" + arg2 + "\"");
      };
    }
  }
  public class AddAppointmentFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 4, m_name);

      SfWidget calendar = args[0] as SfWidget;
      string subject    = Utils.GetSafeString(args, 1);
      string startStr   = Utils.GetSafeString(args, 2);
      string endStr     = Utils.GetSafeString(args, 3);
      string colorStr   = Utils.GetSafeString(args, 4, "black");
      string dateFormat = Utils.GetSafeString(args, 5, "yyyy/MM/dd HH:mm");

      bool added = calendar.AddAppointment(subject, startStr, endStr, colorStr, dateFormat);

      return calendar;
    }
  }
}
