using System;

using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using Com.Syncfusion.Barcode;
using Com.Syncfusion.Barcode.Enums;
using Com.Syncfusion.Calendar;
using Com.Syncfusion.Calendar.Enums;
using Com.Syncfusion.Charts;
using Com.Syncfusion.Charts.Enums;
using Com.Syncfusion.SfPicker;
using Com.Syncfusion.Sfbusyindicator;
using Com.Syncfusion.Sfbusyindicator.Enums;
using Syncfusion.SfDataGrid;
using Com.Syncfusion.Gauges.SfCircularGauge;
using Com.Syncfusion.Gauges.SfCircularGauge.Enums;
using Com.Syncfusion.Gauges.SfDigitalGauge;
using Com.Syncfusion.Numericupdown;

using SplitAndMerge;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Collections.Generic;
using Android.Util;

namespace scripting.Droid
{
  public class SfWidget : DroidVariable
  {
    static int QRBARCODE_SIZE      = 400;
    static int CIRCULAR_GAUGE_SIZE = 500;
    static int STEPPER_SIZE        = 60;
    SyncFusionType  m_type;

    SfBarcode       m_barcode;

    SfBusyIndicator m_busyIndicator;

    SfCalendar      m_calendar;
    SfChart         m_chart;

    SfDataGrid      m_grid;
    int             m_rowIndex;
    DataModel       m_model;

    SfNumericUpDown m_stepper;

    SfDigitalGauge  m_digitalGauge;

    SfPicker        m_picker;

    SfCircularGauge m_circularGauge;
    NeedlePointer   m_needlePointer;
    RangePointer    m_rangePointer1;
    RangePointer    m_rangePointer2;
    CircularScale   m_circularScale;
    CircularScale   m_circularScale2;

    Tuple<GravityFlags, Android.Views.TextAlignment> m_alignment =
         new Tuple<GravityFlags, Android.Views.TextAlignment>(
                   GravityFlags.Left, Android.Views.TextAlignment.TextStart);
    Color m_bgColor   = Color.Transparent;
    Color m_fontColor = Color.Black;

    string          m_data;
    Context         m_context;
    int             m_width;
    int             m_height;
    List<string>    m_strings;
    List<int>       m_pics;

    List<View>      m_pickerViews;
    List<View>      m_textViews;
    List<View>      m_imageViews;

    int             m_linePic;
    bool            m_isLoaded;

    public enum SyncFusionType
    {
      NONE, QR_BARCODE, CODE39_BARCODE, CIRCULAR_GAUGE, DIGITAL_GAUGE, STEPPER, BUSY_INDICATOR,
      CALENDAR, PICKER, SPLINE_GRAPH, DOUGHNUT_GRAPH, DATA_GRID
    };

    public SyncFusionType SfType { get; set; }

    public SfWidget() {}

    public SfWidget(SyncFusionType type, string name, string text, Context context,
                   int width = 0, int height = 0) :
    base(UIType.CUSTOM, name)
    {
      m_type    = type;
      m_data    = text;
      m_context = context;
      m_width   = width;
      m_height  = height;
      SfType    = type;

      switch (type) {
        case SyncFusionType.BUSY_INDICATOR:
          CreateBusyIndicator();
          break;
        case SyncFusionType.CIRCULAR_GAUGE:
          CreateCircularGauge();
          break;
        case SyncFusionType.CODE39_BARCODE:
          CreateCode39Barcode();
          break;
        case SyncFusionType.CALENDAR:
          CreateCalendar();
          break;
        case SyncFusionType.DATA_GRID:
          CreateDataGrid();
          break;
        case SyncFusionType.DIGITAL_GAUGE:
          CreateDigitalGauge();
          break;
        case SyncFusionType.DOUGHNUT_GRAPH:
        case SyncFusionType.SPLINE_GRAPH:
          CreateGraph();
          break;
        case SyncFusionType.PICKER:
          CreatePicker();
          break;
        case SyncFusionType.STEPPER:
          CreateStepper();
          break;
        case SyncFusionType.QR_BARCODE:
          CreateQRBarcode();
          break;
      }

      if (ViewX != null) {
        ViewX.Id = ++m_currentTag;
      }
    }
    public override Variable Clone()
    {
      SfWidget newVar = (SfWidget)this.MemberwiseClone();
      return newVar;
    }

    public override DroidVariable GetWidget(string widgetType, string widgetName, string initArg,
                                           int width, int height)
    {
      switch (widgetType) {
        case "SfCalendar":
          return new SfWidget(SfWidget.SyncFusionType.CALENDAR, widgetName, initArg, MainActivity.TheView);
        case "SfQRBarcode":
          return new SfWidget(SfWidget.SyncFusionType.QR_BARCODE, widgetName, initArg, MainActivity.TheView);
        case "SfCode39Barcode":
          return new SfWidget(SfWidget.SyncFusionType.CODE39_BARCODE, widgetName, initArg, MainActivity.TheView);
        case "SfCircularGauge":
          return new SfWidget(SfWidget.SyncFusionType.CIRCULAR_GAUGE, widgetName, initArg, MainActivity.TheView);
        case "SfDigitalGauge":
          return new SfWidget(SfWidget.SyncFusionType.DIGITAL_GAUGE, widgetName, initArg, MainActivity.TheView);
        case "SfStepper":
          return new SfWidget(SfWidget.SyncFusionType.STEPPER, widgetName, initArg, MainActivity.TheView);
        case "SfBusyIndicator":
          return new SfWidget(SfWidget.SyncFusionType.BUSY_INDICATOR, widgetName, initArg, MainActivity.TheView);
        case "SfDataGrid":
          return new SfWidget(SfWidget.SyncFusionType.DATA_GRID, widgetName, initArg, MainActivity.TheView);
        case "SfDoughnutGraph":
          return new SfWidget(SfWidget.SyncFusionType.DOUGHNUT_GRAPH, widgetName, initArg, MainActivity.TheView);
        case "SfSplineGraph":
          return new SfWidget(SfWidget.SyncFusionType.SPLINE_GRAPH, widgetName, initArg, MainActivity.TheView);
        case "SfPicker":
          return new SfWidget(SfWidget.SyncFusionType.PICKER, widgetName, initArg, MainActivity.TheView);
        case "SfImageEditor":
          return new ImageEditor(widgetName, initArg, MainActivity.TheView);
      }
      return null;
    }
    public override bool SetFontSize(double fontSize)
    {
      if (m_stepper != null) {
        m_stepper.FontSize = fontSize;
        return true;
      }
      return false;
    }
    void CreateBusyIndicator()
    {
      m_busyIndicator = new SfBusyIndicator(m_context);

      m_busyIndicator.IsBusy = true;
      m_busyIndicator.ViewBoxWidth = 100;
      m_busyIndicator.ViewBoxHeight = 100;
      m_busyIndicator.TextSize = 60;
      m_busyIndicator.Title = "";
      m_busyIndicator.SetBackgroundColor(Color.Transparent);
      m_busyIndicator.TextColor = Color.Blue;
      m_busyIndicator.SecondaryColor = Color.Red;
      SetBusyIndicatorType(m_data);

      ViewX = m_busyIndicator;
    }

    void SetBusyIndicatorType(string strType)
    {
      switch (strType) {
        case "ball":
          m_busyIndicator.AnimationType = AnimationTypes.Ball;
          break;
        case "battery":
          m_busyIndicator.AnimationType = AnimationTypes.Battery;
          break;
        case "box":
          m_busyIndicator.AnimationType = AnimationTypes.Box;
          break;
        case "doublecircle":
          m_busyIndicator.AnimationType = AnimationTypes.DoubleCircle;
          break;
        case "ecg":
          m_busyIndicator.AnimationType = AnimationTypes.Ecg;
          break;
        case "gear":
          m_busyIndicator.AnimationType = AnimationTypes.GearBox;
          break;
        case "globe":
          m_busyIndicator.AnimationType = AnimationTypes.Globe;
          break;
        case "pulsing":
          m_busyIndicator.AnimationType = AnimationTypes.HorizontalPulsingBox;
          break;
        case "movie":
          m_busyIndicator.AnimationType = AnimationTypes.MovieTimer;
          break;
        case "print":
          m_busyIndicator.AnimationType = AnimationTypes.Print;
          break;
        case "rectangle":
          m_busyIndicator.AnimationType = AnimationTypes.Rectangle;
          break;
        case "rollingball":
          m_busyIndicator.AnimationType = AnimationTypes.RollingBall;
          break;
        case "singlecircle":
          m_busyIndicator.AnimationType = AnimationTypes.SingleCircle;
          break;
        case "slicedcircle":
          m_busyIndicator.AnimationType = AnimationTypes.SlicedCircle;
          break;
        case "zooming":
          m_busyIndicator.AnimationType = AnimationTypes.ZoomingTarget;
          break;
      }
    }

    void CreatePicker()
    {
      m_picker = new SfPicker(m_context);

      string str1 = "", str2 = "", str3 = "", str4 = "";
      Utils.Extract(m_data, ref str1, ref str2, ref str3, ref str4);
      if (!string.IsNullOrEmpty(str1)) {
        double rowHeight = Utils.ConvertToDouble(str1);
        if (rowHeight > 0) {
          rowHeight = AutoScaleFunction.TransformSize((int)rowHeight,
                                                      UtilsDroid.GetScreenSize().Width);
          m_picker.ItemHeight = rowHeight;
        }
      }
      m_picker.ShowHeader = !string.IsNullOrEmpty(str2);
      if (m_picker.ShowHeader) {
        m_picker.HeaderHeight = 60;
        m_picker.HeaderText = str2;
        m_picker.HeaderBackgroundColor = Color.Transparent;
        m_picker.HeaderTextColor = Color.Black;
      }
      m_picker.ShowColumnHeader = !string.IsNullOrEmpty(str3);
      if (m_picker.ShowColumnHeader) {
        m_picker.ColumnHeaderHeight = 60;
        m_picker.ColumnHeaderText = str3;
        m_picker.ColumnHeaderBackgroundColor = Color.Transparent;
        m_picker.ColumnHeaderTextColor = Color.Black;
      }

      m_picker.ShowFooter = false;
      m_picker.SelectedIndex = 0;
      int lastSelected = 0;

      m_picker.PickerMode = PickerMode.Default;

      m_picker.BorderColor = Color.Black;

      m_picker.PickerWidth = m_width;
      m_picker.PickerHeight = m_height;

      m_picker.HeaderBackgroundColor = Color.Transparent;
      m_picker.HeaderTextColor = Color.Black;
      m_picker.UnSelectedItemTextColor = Color.Gray;
      m_picker.SelectedItemTextcolor = m_fontColor;

      m_pickerViews = new List<View>();
      m_textViews   = new List<View>();
      m_imageViews  = new List<View>();

      m_linePic = UtilsDroid.String2Pic("grayline");

      m_picker.OnSelectionChanged += (sender, e) => {
        if (m_textViews.Count == 0) {
          return;
        }
        int newSelection = (int)Utils.ConvertToDouble(m_picker.SelectedIndex);

        for (int row = 0; row < m_textViews.Count; row++) {
          AdjustPickerView(m_textViews[row],  row);
          AdjustPickerView(m_imageViews[row], row);
        }

        lastSelected = newSelection;
        ActionDelegate?.Invoke(WidgetName, newSelection.ToString());
      };

      m_picker.OnPickerItemLoaded += (sender, e) => {
        if (m_pics == null) {
          return;
        }
        LoadPickerViewsIfNeeded();
        e.CustomView = m_pickerViews[e.Row];
      };

      ViewX = m_picker;
    }

    void LoadPickerViewsIfNeeded()
    {
      if (m_pickerViews.Count > 0) {
        return;
      }
      m_imageViews.Clear();
      m_textViews.Clear();

      int stringRows = m_strings == null ? 0 : m_strings.Count;
      int picRows = m_pics == null ? 0 : m_pics.Count;
      int rows = Math.Max(stringRows, picRows);

      for (int row = 0; row < rows; row++) {
        LoadPickerView(row);
      }
    }

    void LoadPickerView(int row)
    {
      LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
        ViewGroup.LayoutParams.WrapContent,
        ViewGroup.LayoutParams.WrapContent
      );
      layoutParams.Height = (int)m_picker.ItemHeight;
      layoutParams.Width = (int)m_picker.Width;
      LinearLayout rowView = new LinearLayout(m_context) { Orientation = Orientation.Vertical };

      LinearLayout rowData = new LinearLayout(m_context);
      rowData.Orientation = Orientation.Horizontal;

      ImageView rowImage = null;
      if (m_pics == null || m_pics.Count > row) {
        rowImage = new ImageView(m_context);
        rowImage.SetImageResource(m_pics[row]);
        AdjustPickerView(rowImage, row);
        rowData.AddView(rowImage, ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
      }
      TextView rowText = null;
      if (m_strings != null && m_strings.Count > row) {
        rowText = new TextView(m_context);
        rowText.Text = " " + m_strings[row];
        rowText.TextAlignment = m_alignment.Item2;
        rowText.SetBackgroundColor(m_bgColor);
        rowText.Gravity = GravityFlags.CenterVertical;
        AdjustPickerView(rowText, row);
        rowData.AddView(rowText, ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
      }

      ImageView line = new ImageView(m_context);
      line.SetImageResource(m_linePic);
      line.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

      rowView.AddView(rowData, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
      rowView.AddView(line, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
      rowView.LayoutParameters = layoutParams;

      m_imageViews.Add(rowImage);
      m_textViews.Add(rowText);
      m_pickerViews.Add(rowView);
    }

    void AdjustPickerView(View view, int row)
    {
      if (view == null) {
        return;
      }
      int selectedRow = (int)Utils.ConvertToDouble(m_picker.SelectedIndex);
      bool isSelected = row == selectedRow;

      if (view is TextView) {
        TextView rowText = view as TextView;
        rowText.SetTextColor(isSelected ? m_fontColor : Color.Gray);
      } else if (view is ImageView) {
        ImageView rowImage = view as ImageView;
        if (!isSelected) {
          rowImage.SetColorFilter(Color.Argb(200, 128, 128, 128));
        } else {
          rowImage.SetColorFilter(null);
        }
      }
    }

    void CreateDataGrid()
    {
      m_grid = new SfDataGrid(m_context);

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
        var elem = e.AddedItems.Count > 0 ? e.AddedItems[0] :
                   e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
        if (elem == null) {
          return;
        }
        m_model.Sort(elem.ColumnName, elem.SortDirection == System.ComponentModel.ListSortDirection.Ascending);
      };
      m_grid.QueryRowDragging += (sender, e) => {
        if (e.Reason == QueryRowDraggingReason.DragEnded) {
          m_model.SwapRows(e.From - 1, e.To - 1);
        }
      };

      m_model = new DataModel(m_grid);
      m_model.PropertyChanged += (sender, e) => {
        m_grid.ItemsSource = m_model.DataPoints;
      };

      m_grid.SwipeStarted += (sender, e) => {
        //m_grid.MaxSwipeOffset = m_grid.Width / 3;// Math.Max(10, (int)(m_grid.Width / 3));
      };
      m_grid.SwipeEnded += (sender, e) => {
        m_rowIndex = e.RowIndex - 1;
      };

      ViewX = m_grid;
    }
    void AddSwipeDelete()
    {
      SwipeView rightSwipeView = new SwipeView(m_context);

      ImageView deleteImage = new ImageView(m_context);
      deleteImage.SetImageResource(Resource.Drawable.Delete);
      deleteImage.SetBackgroundColor(Color.ParseColor("#DC595F"));

      TextView deleteText = new TextView(m_context);
      deleteText.Text = "DELETE";
      deleteText.SetTextColor(Color.White);
      deleteText.Gravity = GravityFlags.Center;
      deleteText.SetBackgroundColor(Color.ParseColor("#DC595F"));

      LinearLayout deleteView = new LinearLayout(m_context);
      deleteView.Orientation = Orientation.Horizontal;
      deleteView.Click += (sender, e) => {
        m_model.RemovePoint(m_rowIndex);
      };

      deleteView.AddView(deleteImage, ViewGroup.LayoutParams.WrapContent, (int)m_grid.RowHeight);
      deleteView.AddView(deleteText, ViewGroup.LayoutParams.MatchParent, (int)m_grid.RowHeight);

      rightSwipeView.AddView(deleteView, m_grid.MaxSwipeOffset, (int)m_grid.RowHeight);

      m_grid.RightSwipeView = rightSwipeView;
    }
    void AddSwipeEdit()
    {
      SwipeView leftSwipeView = new SwipeView(m_context);

      ImageView editImage = new ImageView(m_context);
      editImage.SetImageResource(Resource.Drawable.Edit);
      editImage.SetBackgroundColor(Color.ParseColor("#009EDA"));

      TextView editText = new TextView(m_context);
      editText.Text = "EDIT";
      editText.SetTextColor(Color.White);
      editText.Gravity = GravityFlags.Center;
      editText.SetBackgroundColor(Color.ParseColor("#009EDA"));

      LinearLayout editView = new LinearLayout(m_context);
      editView.Orientation = Orientation.Horizontal;
      editView.Click += EditView_Click;

      editView.AddView(editImage, ViewGroup.LayoutParams.WrapContent, (int)m_grid.RowHeight);
      editView.AddView(editText, ViewGroup.LayoutParams.MatchParent, (int)m_grid.RowHeight);

      leftSwipeView.AddView(editView, m_grid.MaxSwipeOffset, (int)m_grid.RowHeight);

      m_grid.LeftSwipeView = leftSwipeView;
    }

    void EditView_Click(object sender, EventArgs e)
    {
      DataPoint point = m_model.GetPoint(m_rowIndex);
      if (point == null) {
        return;
      }
      var parentLayout = MainActivity.TheLayout;
      var density = m_grid.Resources.DisplayMetrics.Density;

      LinearLayout optionView = new LinearLayout(m_context);
      optionView.SetBackgroundColor(Color.Transparent);

      LinearLayout editor = new LinearLayout(m_context);
      editor.SetBackgroundColor(Color.Snow);
      editor.Orientation = Orientation.Vertical;

      LinearLayout body = new LinearLayout(m_context);
      body.Orientation = Orientation.Vertical;
      body.SetGravity(GravityFlags.CenterHorizontal);

      TextView optionHeading = new TextView(m_context);
      optionHeading.Text = "EDIT DETAILS";
      optionHeading.SetTypeface(null, TypefaceStyle.Bold);
      optionHeading.Gravity = GravityFlags.Center;

      List<EditText> textViews = new List<EditText>(DataModel.ColNames.Count);
      for (int i = 0; i < DataModel.ColNames.Count; i++) {
        LinearLayout bodyRow = new LinearLayout(m_context);
        bodyRow.Orientation = Orientation.Horizontal;
        bodyRow.SetGravity(GravityFlags.CenterHorizontal);
        TextView coli = new TextView(m_context);
        EditText texti = new EditText(m_context);
        coli.Text = DataModel.ColNames[i];
        texti.Text = point.GetStringValue(i);

        texti.Gravity = GravityFlags.Start;
        textViews.Add(texti);

        bodyRow.AddView(coli, (int)(100 * density), (int)(50 * density));
        bodyRow.AddView(texti, (int)(150 * density), ViewGroup.LayoutParams.WrapContent);
        body.AddView(bodyRow);
      }

      Button save = new Button(m_context);
      Button cancel = new Button(m_context);
      save.Text = "Save";
      cancel.Text = "Cancel";
      save.Click += (sender2, e2) => {
        DataPoint dataPoint = m_model.GetPoint(m_rowIndex);
        for (int i = 0; i < DataModel.ColNames.Count; i++) {
          dataPoint.Set(i, textViews[i].Text);
        }
        m_model.Reload();
        parentLayout.RemoveView(optionView);
        m_grid.Visibility = ViewStates.Visible;
      };
      cancel.Click+= (sender2, e2) => {
        m_grid.ItemsSource = m_model.DataPoints;
        parentLayout.RemoveView(optionView);
        m_grid.Visibility = ViewStates.Visible;
      };

      LinearLayout bottom = new LinearLayout(m_context);
      bottom.Orientation = Orientation.Horizontal;
      bottom.AddView(save);
      bottom.AddView(cancel);
      bottom.SetGravity(GravityFlags.Center);

      editor.AddView(optionHeading);
      editor.AddView(body);
      editor.AddView(bottom);

      optionView.AddView(editor, ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

      LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                                                                             ViewGroup.LayoutParams.MatchParent);
      int delta = (int)AutoScaleFunction.TransformSize(20, AutoScaleFunction.GetRealScreenSize());
      layoutParams.TopMargin = (int)m_grid.GetY();
      layoutParams.LeftMargin = (int)(m_grid.GetX() + delta);
      optionView.LayoutParameters = layoutParams;

      //optionView.LayoutParameters = ViewX.LayoutParameters;

      parentLayout.AddView(optionView);
      m_grid.Visibility = ViewStates.Invisible;
    }

    void CreateCalendar()
    {
      m_calendar = new SfCalendar(m_context);

      m_calendar.SetBackgroundColor(Color.Transparent);
      m_calendar.HeaderHeight = 100;
      MonthViewLabelSetting labelSettings = new MonthViewLabelSetting();
      labelSettings.DateLabelSize = 14;
      MonthViewSettings monthViewSettings = new MonthViewSettings();
      monthViewSettings.MonthViewLabelSetting = labelSettings;
      monthViewSettings.TodayTextColor = Color.ParseColor("#1B79D6");
      monthViewSettings.InlineBackgroundColor = Color.ParseColor("#E4E8ED");
      monthViewSettings.WeekDayBackgroundColor = Color.ParseColor("#F7F7F7");
      m_calendar.MonthViewSettings = monthViewSettings;
      m_calendar.ShowEventsInline = true;

      ViewX = m_calendar;
    }

    Java.Util.Calendar GetCalDate(string dateStr, string dateFormat) 
    {
      Java.Text.SimpleDateFormat sdf = new Java.Text.SimpleDateFormat(dateFormat);
      var dt = sdf.Parse(dateStr);
      var cal = sdf.Calendar;
      return cal;
    }

    public bool AddAppointment(string subject, string startStr, string endStr, string colorStr,
                               string dateFormat)
    {
      if (m_calendar == null) {
        return false;
      }
      CalendarInlineEvent appointment = new CalendarInlineEvent();
      appointment.Subject = subject;
      appointment.StartTime = GetCalDate(startStr, dateFormat);
      appointment.EndTime = GetCalDate(endStr, dateFormat);
      appointment.Color   = UtilsDroid.String2Color(colorStr);

      var appointments = m_calendar.DataSource;
      if (appointments == null) {
        appointments = new CalendarEventCollection();
      }
      appointments.Add(appointment);
      m_calendar.DataSource = appointments;

      return true;
    }
    void CreateQRBarcode()
    {
      m_barcode = new SfBarcode(m_context);
      m_barcode.Text = m_data;
      Typeface fontFamily = Typeface.Create("helvetica", TypefaceStyle.Bold);
      m_barcode.TextFont = fontFamily;
      m_barcode.SetBackgroundColor(Color.White);
      m_barcode.Symbology = BarcodeSymbolType.QRBarcode;
      m_barcode.TextSize = 12;

      QRBarcodeSettings setting = new QRBarcodeSettings();
      setting.XDimension = 11;
      m_barcode.SymbologySettings = setting;

      ViewX = m_barcode;
    }
    void CreateCode39Barcode()
    {
      m_barcode = new SfBarcode(m_context);
      m_barcode.Text = m_data;
      m_barcode.TextSize = 20;
      Typeface fontFamily = Typeface.Create("helvetica", TypefaceStyle.Bold);
      m_barcode.TextFont = fontFamily;
      m_barcode.SetBackgroundColor(Color.ParseColor("#F2F2F2"));
      m_barcode.Symbology = BarcodeSymbolType.Code39Extended;
 
      Code39ExtendedSettings settings = new Code39ExtendedSettings();
      settings.BarHeight = 120;
      settings.NarrowBarWidth = 1;
      m_barcode.SymbologySettings = settings;

      ViewX = m_barcode;
    }
    void CreateStepper()
    {
      m_stepper = new SfNumericUpDown(m_context);

      double minValue = 0, maxValue = 1, currValue = 0, step = 1.0;
      Utils.Extract(m_data, ref currValue, ref minValue, ref maxValue, ref step);

      m_stepper.FontSize = 18;
      m_stepper.Minimum = minValue;
      m_stepper.Maximum = maxValue;
      m_stepper.Value = currValue;
      m_stepper.FormatString = "N";
      m_stepper.AutoReverse = false;
      m_stepper.StepValue = step;
      m_stepper.TextGravity = GravityFlags.CenterVertical;
      m_stepper.SpinButtonAlignment = SpinButtonAlignment.Right;
      m_stepper.TextAlignment = Android.Views.TextAlignment.TextStart;
      m_stepper.MaximumDecimalDigits = Utils.GetNumberOfDigits(m_data, 3);
      m_stepper.Focusable = false;
      m_stepper.IsEditable = false;

      m_stepper.ValueChanged += StepperCallback;

      ViewX = m_stepper;
    }

    void StepperCallback(object sender, ValueChangedEventArgs e) {
      ActionDelegate?.Invoke(WidgetName, e.Value.ToString());
      MainActivity.TheView.ShowHideKeyboard(m_stepper, false);
    }

    void CreateDigitalGauge()
    {
      m_digitalGauge = new SfDigitalGauge(m_context);

      m_digitalGauge.CharacterHeight = 25;
      m_digitalGauge.CharactersSpacing = 5;
      m_digitalGauge.CharacterWidth = 9;
      m_digitalGauge.SegmentStrokeWidth = 2;

      m_digitalGauge.CharacterType = CharacterTypes.EightCrossEightDotMatrix;
      m_digitalGauge.StrokeType = StrokeType.Triangle;
      m_digitalGauge.Value = DateTime.Now.ToString("HH:mm:ss");
      m_digitalGauge.SetBackgroundColor(Color.Transparent);
      m_digitalGauge.DimmedSegmentAlpha = 30;
      m_digitalGauge.DimmedSegmentColor = Color.Rgb(249, 66, 53);
      m_digitalGauge.CharacterStroke = Color.Rgb(249, 66, 53);
 
      float segmentMatrixHeight = TypedValue.ApplyDimension(ComplexUnitType.Pt, (float)m_digitalGauge.CharacterHeight, m_context.Resources.DisplayMetrics);
      float segmentMatrixWidth = TypedValue.ApplyDimension(ComplexUnitType.Pt, (float)(8 * m_digitalGauge.CharacterWidth + 8 * m_digitalGauge.CharactersSpacing), m_context.Resources.DisplayMetrics);
      m_digitalGauge.LayoutParameters = (new LinearLayout.LayoutParams((int)segmentMatrixWidth, (int)segmentMatrixHeight));


      ViewX = m_digitalGauge;
    }
    void CreateCircularGauge()
    {
      m_circularGauge = new SfCircularGauge(m_context);

      ObservableCollection<CircularScale> _circularScales = new ObservableCollection<CircularScale>();
      ObservableCollection<CircularPointer> _circularPointers = new ObservableCollection<CircularPointer>();
      ObservableCollection<Com.Syncfusion.Gauges.SfCircularGauge.Header> _gaugeHeaders =
                      new ObservableCollection<Com.Syncfusion.Gauges.SfCircularGauge.Header>();
      // adding  new CircularScale
      m_circularScale = new CircularScale();
      m_circularScale.StartValue = 0;
      m_circularScale.EndValue = 100;
      m_circularScale.StartAngle = 130;
      m_circularScale.SweepAngle = 280;
      m_circularScale.ShowRim = true;
      m_circularScale.MinorTicksPerInterval = 0;
      m_circularScale.LabelOffset = 0.75;
      m_circularScale.RadiusFactor = 1;
      m_circularScale.LabelTextSize = 18;

      //adding major ticks
      TickSetting majorTickstings = new TickSetting();
      majorTickstings.Color = Color.ParseColor("#444444");
      majorTickstings.Size = 15;
      majorTickstings.Offset = 0.97;
      m_circularScale.MajorTickSettings = majorTickstings;

      //adding minor ticks
      TickSetting minorTickstings = new TickSetting();
      minorTickstings.Color = Color.Gray;
      m_circularScale.MinorTickSettings = minorTickstings;

      // adding needle Pointer
      m_needlePointer = new NeedlePointer();
      m_needlePointer.Value = 60;
      m_needlePointer.KnobColor = Color.ParseColor("#2BBFB8");
      m_needlePointer.KnobRadius = 20;
      m_needlePointer.Type = NeedleType.Bar;
      m_needlePointer.LengthFactor = 0.8;
      m_needlePointer.Width = 3;
      m_needlePointer.Color = Color.Gray;
      _circularPointers.Add(m_needlePointer);

      // adding range Pointer
      m_rangePointer1 = new RangePointer();
      m_rangePointer1.Value = 60;
      m_rangePointer1.Offset = 1;
      m_rangePointer1.Color = Color.ParseColor("#2BBFB8");
      m_rangePointer1.Width = 10;
      m_rangePointer1.EnableAnimation = false;
      _circularPointers.Add(m_rangePointer1);

      m_rangePointer2 = new RangePointer();
      m_rangePointer2.RangeStart = 60;
      m_rangePointer2.Value = 100;
      m_rangePointer2.Offset = 1;
      m_rangePointer2.Color = Color.ParseColor("#D14646");
      m_rangePointer2.Width = 10;
      m_rangePointer2.EnableAnimation = false;
      _circularPointers.Add(m_rangePointer2);

      m_circularScale.CircularPointers = _circularPointers;
      _circularScales.Add(m_circularScale);

      //adding header
      Com.Syncfusion.Gauges.SfCircularGauge.Header circularGaugeHeader = new Com.Syncfusion.Gauges.SfCircularGauge.Header();
      circularGaugeHeader.Text = "Speedometer";
      circularGaugeHeader.TextColor = Color.Gray;
      circularGaugeHeader.Position = new PointF((float)0.5, (float)0.6);
      circularGaugeHeader.TextSize = 18;
      _gaugeHeaders.Add(circularGaugeHeader);
      m_circularGauge.Headers = _gaugeHeaders;
      m_circularGauge.CircularScales = _circularScales;
      m_circularGauge.SetBackgroundColor(Color.Transparent);

      ViewX = m_circularGauge;
    }
    void CreateScale2IfNeeded()
    {
      if (m_circularScale2 != null) {
        return;
      }
      m_circularScale2 = new CircularScale();
      m_circularScale2.StartValue = m_circularScale.StartValue;
      m_circularScale2.EndValue = m_circularScale.EndValue;
      m_circularScale2.Interval = m_circularScale.Interval;
      m_circularScale2.StartAngle = m_circularScale.StartAngle;
      m_circularScale2.SweepAngle = m_circularScale.SweepAngle;
      m_circularScale2.ShowTicks = false;
      m_circularScale2.ShowLabels = true;
      m_circularScale2.ShowRim = true;
      m_circularScale2.LabelColor = Color.Black;
      m_circularScale2.RimColor = Color.Blue;
      m_circularScale2.LabelOffset = 0.55f;
      //m_circularScale2.MinorTicksPerInterval = 0;
      m_circularScale2.ScaleStartOffset = 0.63f;
      m_circularScale2.ScaleEndOffset = 0.65f;

      var scales = m_circularGauge.CircularScales;
      scales.Add(m_circularScale2);
      m_circularGauge.CircularScales = scales;
    }
    void CreateGraph()
    {
      m_chart = new SfChart(m_context);

      m_chart.SetBackgroundColor(Color.White);
      m_chart.Title.Text = "Graph";
      m_chart.Title.TextSize = 15;
 
      CategoryAxis categoryaxis = new CategoryAxis();
      categoryaxis.LabelPlacement = LabelPlacement.BetweenTicks;
      categoryaxis.EdgeLabelsDrawingMode = EdgeLabelsDrawingMode.Shift;
      m_chart.PrimaryAxis = categoryaxis;

      NumericalAxis numericalaxis = new NumericalAxis();
      m_chart.SecondaryAxis = numericalaxis;

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
        var collection = new List<DataPoint>();
        for (int i = 0; i < data.Count - 1; i += 2) {
          string coord = data[i];
          double value = Utils.ConvertToDouble(data[i + 1]);
          collection.Add(new DataPoint(coord, value));
        }
        if (SfType == SyncFusionType.DOUGHNUT_GRAPH) {
          DoughnutSeries doughnutSeries = new DoughnutSeries();
          doughnutSeries.ItemsSource = collection;
          doughnutSeries.XBindingPath = "XValue";
          doughnutSeries.YBindingPath = "YValue";
          doughnutSeries.TooltipEnabled = true;
          doughnutSeries.EnableAnimation = true;
          if (!string.IsNullOrEmpty(title)) {
            //doughnutSeries.Label = title;
            m_chart.Title.Text = title;
          }
          doughnutSeries.DataMarker.ShowLabel = true;
          doughnutSeries.DataMarker.LabelContent = title == "values" ? LabelContent.YValue :
                                                                       LabelContent.Percentage;
          doughnutSeries.DataMarkerPosition = CircularSeriesDataMarkerPosition.Outside;
          doughnutSeries.LegendIcon = ChartLegendIcon.Circle;
          if (!string.IsNullOrEmpty(extra)) {
            var angles = extra.Split(new char[] { ',', ':' });
            doughnutSeries.StartAngle = Utils.ConvertToDouble(angles[0]);
            if (angles.Length > 1) {
              doughnutSeries.EndAngle = Utils.ConvertToDouble(angles[1]);
            }
          }
          m_chart.Series.Add(doughnutSeries);
        } else if (SfType == SyncFusionType.SPLINE_GRAPH) {
          SplineSeries series = new SplineSeries();
          series.ItemsSource = collection;
          series.XBindingPath = "XValue";
          series.YBindingPath = "YValue";
          series.Label = title;
          series.DataMarker.ShowMarker = true;
          series.LegendIcon = ChartLegendIcon.Rectangle;
          series.TooltipEnabled = true;
          series.StrokeWidth = 3;
          series.EnableAnimation = true;

          if (!string.IsNullOrEmpty(extra)) {
            var colors = extra.Split(new char[] { ',', ':' });
            series.Color = UtilsDroid.String2Color(colors[0]);
            if (colors.Length > 1) {
              var color = UtilsDroid.String2Color(colors[1]);
              series.DataMarker.MarkerColor = color;
              series.DataMarker.MarkerStrokeColor = color;
            }
          }

          m_chart.Series.Add(series);
        }

        if (!string.IsNullOrEmpty(title)) {
          m_chart.Legend.Visibility = Visibility.Visible;
          m_chart.Legend.ToggleSeriesVisibility = true;
          m_chart.Legend.DockPosition = ChartDock.Bottom;
        }
      } else if (m_picker != null) {
        m_strings = data;
        m_picker.ItemsSource = data;
        m_pickerViews.Clear();
      }
    }
    public override void AddImages(List<string> images, string varName, string title)
    {
      m_pics = UtilsDroid.GetPicList(images);
      if (m_picker != null && m_strings == null) {
        m_picker.ItemsSource = images;
        m_pickerViews.Clear();
      }
    }

    public override void ShowView(bool show)
    {
      if (!show || ViewX.Visibility == ViewStates.Visible || m_stepper == null) {
        return;
      }
      if (!m_isLoaded) {
        m_isLoaded = true;
        return;
      }

      MainActivity.RemoveView(this);

      if (m_stepper != null) {
        // There is a bug with the Stepper, some of its values are lost after it was hidden.
        // So we recreate it each time it's shown after it was hidden.
        m_stepper.ValueChanged -= StepperCallback;
        m_data = m_stepper.Value + ":" + m_stepper.Minimum + ":" + m_stepper.Maximum + ":" + m_stepper.StepValue;
        var fontSize = m_stepper.FontSize;

        //m_stepper.ValueChanged += StepperCallback;
        CreateStepper();
        m_stepper.FontSize = fontSize;
        MainActivity.TheView.ShowHideKeyboard(m_stepper, false);
      }

      ViewX.LayoutParameters = LayoutParams;
      ViewX.TranslationX = TranslationX;
      ViewX.TranslationY = TranslationY;

      MainActivity.AddView(this, true);
      ParserFunction.AddGlobal(WidgetName, new GetVarFunction(this));
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
            m_circularScale.StartValue = valueNum;
            break;
          case "step":
            m_circularScale.Interval = valueNum;
            break;
          case "to":
            m_rangePointer2.Value = valueNum;
            m_circularScale.EndValue = valueNum;
            break;
          case "value":
            m_needlePointer.Value = valueNum;
            break;
          case "start_range2":
            m_rangePointer1.Value = valueNum;
            m_rangePointer2.RangeStart = valueNum;
            break;
          case "start_angle":
            m_circularScale.StartAngle = 160 - valueNum;
            break;
          case "sweep_angle":
            m_circularScale.SweepAngle = valueNum - 50;
            break;
          case "color_needle":
            m_needlePointer.Color = UtilsDroid.String2Color(arg2);
            break;
          case "radius_knob":
            m_needlePointer.KnobRadius = valueNum;
            break;
          case "color_knob":
            m_needlePointer.KnobColor = UtilsDroid.String2Color(arg2);
            break;
          case "color_labels":
            m_circularScale.LabelColor = UtilsDroid.String2Color(arg2);
            var headers = m_circularGauge.Headers;
            headers[0].TextColor = UtilsDroid.String2Color(arg2);
            m_circularGauge.Headers= headers;
            break;
          case "color_range1":
            m_rangePointer1.Color = UtilsDroid.String2Color(arg2);
            break;
          case "color_range2":
            m_rangePointer2.Color = UtilsDroid.String2Color(arg2);
            break;
          case "color_minorticks":
            m_circularScale.MinorTickSettings.Color = UtilsDroid.String2Color(arg2);
            break;
          case "color_majorticks":
            m_circularScale.MajorTickSettings.Color = UtilsDroid.String2Color(arg2);
            break;
          case "scale2_from":
            CreateScale2IfNeeded();
            var scales = m_circularGauge.CircularScales;
            scales[1].StartValue = valueNum;
            m_circularGauge.CircularScales = scales;
            break;
          case "scale2_to":
            CreateScale2IfNeeded();
            scales = m_circularGauge.CircularScales;
            scales[1].EndValue = valueNum;
            m_circularGauge.CircularScales = scales;
            break;
          case "scale2_interval":
            CreateScale2IfNeeded();
            scales = m_circularGauge.CircularScales;
            scales[1].Interval = valueNum;
            m_circularGauge.CircularScales = scales;
            break;
          case "scale2_rimcolor":
            CreateScale2IfNeeded();
            scales = m_circularGauge.CircularScales;
            scales[1].RimColor = UtilsDroid.String2Color(arg2);
            m_circularGauge.CircularScales = scales;
            break;
          case "scale2_labelcolor":
            CreateScale2IfNeeded();
            scales = m_circularGauge.CircularScales;
            scales[1].LabelColor = UtilsDroid.String2Color(arg2);
            m_circularGauge.CircularScales = scales;
            break;
        }
      } else if (m_digitalGauge != null) {
        switch (arg1) {
          case "value":
            m_digitalGauge.Value = arg2;
            break;
        }
      } else if (m_picker != null) {
        switch (arg1) {
          case "alignment":
            m_alignment = UtilsDroid.GetAlignment(arg1);
            break;
          case "headerHeight":
            m_picker.HeaderHeight = valueNum;
            break;
          case "headerText":
            m_picker.ShowHeader = true;
            m_picker.HeaderText = arg2;
            break;
          case "colHeaderHeight":
            m_picker.ColumnHeaderHeight = valueNum;
            break;
          case "colHeaderText":
            m_picker.ShowColumnHeader = true;
            m_picker.ColumnHeaderText = arg2;
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
        var settings39 = m_barcode.SymbologySettings as Code39ExtendedSettings;
        if (settings39 != null) {
          switch (arg1) {
            case "bar_height":
              settings39.BarHeight = valueNum;
              break;
            case "bar_width":
              settings39.NarrowBarWidth = valueNum;
              break;
          }
        }
      } else if (m_chart != null) {
        NumericalAxis numericalaxis = m_chart.SecondaryAxis as NumericalAxis;
        switch (arg1) {
          case "primary_axis":
            m_chart.PrimaryAxis.Title.Text = arg2;
            break;
          case "secondary_axis":
            m_chart.SecondaryAxis.Title.Text = arg2;
            break;
          case "x_min":
            numericalaxis.Minimum = valueNum;
            break;
          case "x_max":
            numericalaxis.Maximum = valueNum;
            break;
          case "x_interval":
            numericalaxis.Interval = valueNum;
            break;
        }
      } else if (m_stepper != null) {
        switch (arg1) {
          case "min":
            m_stepper.Minimum   = valueNum;
            break;
          case "max":
            m_stepper.Maximum   = valueNum;
            break;
          case "step":
            m_stepper.StepValue = valueNum;
            m_stepper.MaximumDecimalDigits = Utils.GetNumberOfDigits(arg2);
            break;
          case "value":
            m_stepper.Value     = valueNum;
            break;
          case "buttons":
            switch (arg2) {
              case "left":
                m_stepper.SpinButtonAlignment = SpinButtonAlignment.Left;
                break;
              case "right":
                m_stepper.SpinButtonAlignment = SpinButtonAlignment.Right;
                break;
              default:
                m_stepper.SpinButtonAlignment = SpinButtonAlignment.Both;
                break;
            }
            break;
        }
      } else if (m_busyIndicator != null) {
        if (arg1 == "bg_color") {
          m_busyIndicator.SetBackgroundColor(UtilsDroid.String2Color(arg2));
        } else if (arg1 == "color") {
          m_busyIndicator.TextColor = UtilsDroid.String2Color(arg2);
        } else if (arg1 == "secondary_color") {
          m_busyIndicator.SecondaryColor = UtilsDroid.String2Color(arg2);
        } else if (arg1 == "duration") {
          m_busyIndicator.Duration = (int)valueNum * 1000;
        } else {
          SetBusyIndicatorType(arg2);
        }
      } else {
        return false;
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
      } else if (m_circularGauge != null && m_circularGauge.Headers.Count > 0) {
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
    public override bool SetText(string text, string alignment = null)
    {
      if (alignment != null) {
        m_alignment = UtilsDroid.GetAlignment(alignment);
      }
      if (m_barcode != null) {
        m_barcode.Text = text;
      } else if (m_chart != null) {
        m_chart.Title.Text = text;
      } else if (m_circularGauge != null) {
        var headers = m_circularGauge.Headers;
        headers[0].Text = text;
        m_circularGauge.Headers = headers;
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
    public override bool SetBackgroundColor(string colorStr, double alpha)
    {
      Color color = UtilsDroid.String2Color(colorStr);
      if (alpha < 1.0) {
        color = Color.Argb((int)(alpha * 255), color.R, color.G, color.B);
      }
      m_bgColor = color;

      if (m_picker != null) {
        m_picker.BackgroundColor = color;
      } else if (m_chart != null) {
        m_chart.SetBackgroundColor(color);
      } else if (m_stepper != null) {
        m_stepper.SetBackgroundColor(color);
      } else if (m_grid != null) {
        m_grid.SetBackgroundColor(color);
      } else if (m_barcode != null) {
        m_barcode.SetBackgroundColor(color);
      } else if (m_digitalGauge != null) {
        m_digitalGauge.SetBackgroundColor(color);
      } else if (m_circularGauge != null) {
        m_circularGauge.SetBackgroundColor(color);
        m_circularScale.LabelColor = color;
        //m_needlePointer.SetBackgroundColor(color);
        //m_rangePointer1.SetBackgroundColor(color);
        //m_rangePointer2.SetBackgroundColor(color);
      } else if (m_busyIndicator != null) {
        m_busyIndicator.SetBackgroundColor(color);
      } else {
        return false;
      }

      return true;
    }
    public override bool SetFontColor(string colorStr)
    {
      Color color = UtilsDroid.String2Color(colorStr);
      m_fontColor = color;

      if (m_picker != null) {
        m_picker.HeaderTextColor = color;
        m_picker.ColumnHeaderTextColor = color;
        m_picker.UnSelectedItemTextColor = color;
        m_picker.SelectedItemTextcolor = color;
      } else if (m_chart != null && m_chart.Title != null) {
        m_chart.Title.SetTextColor(color);
      } else if (m_stepper != null) {
        m_stepper.TextColor = color;
        m_stepper.BorderColor = color;
        m_stepper.UpDownButtonColor = color;
      } else if (m_barcode != null) {
        m_barcode.TextColor = color;
      } else if (m_digitalGauge != null) {
        //m_digitalGauge.DimmedSegmentAlpha = 30;
        m_digitalGauge.DimmedSegmentColor = color;
        m_digitalGauge.CharacterStroke = color;
      } else if (m_circularGauge != null && m_circularGauge.Headers.Count > 0) {
        m_circularGauge.Headers[0].TextColor = color;
      } else if (m_busyIndicator != null) {
        m_busyIndicator.TextColor = color;
      } else {
        return false;
      }

      return true;
    }

    public override void AddAction(string varName,
                                   string strAction, string argument = "")
    {
      ActionDelegate += (arg1, arg2) => {
        UIVariable.GetAction(strAction, varName, arg2);
      };
    }

    public override void AdjustTranslation(DroidVariable location, bool isX, bool sameWidget = true)
    {
      ScreenSize screenSize = UtilsDroid.GetScreenSize();
      int height = sameWidget ? 0 : m_barcode     != null ? QRBARCODE_SIZE :
                   m_circularGauge != null ? CIRCULAR_GAUGE_SIZE :
                   m_stepper       != null ? sameWidget ? 0 : STEPPER_SIZE :
                   0;
      int width  = m_barcode     != null ? QRBARCODE_SIZE :
                   m_circularGauge != null ? CIRCULAR_GAUGE_SIZE :
                   //m_stepper       != null ? STEPPER_SIZE :
                   0;
      AutoScaleFunction.TransformSizes(ref width, ref height,
                                       screenSize.Width);
      int[] loc = new int[2];
      if (!sameWidget && isX && this.Location.RefViewX != null) {
        location.RefViewX = this.Location.RefViewX;
        location.ViewX = ((DroidVariable)this.Location).ViewX;
        location.ViewX.GetLocationOnScreen(loc);
        //location.TranslationX += loc[0] + location.RefViewX.Width;
        int delta = 0;
        if (m_stepper != null) {
          //delta = AutoScaleFunction.TransformSize(100, screenSize.Width);
        }
        ExtraX = ((DroidVariable)location.RefViewX).ExtraX + location.RefViewX.TranslationX +
                                                   location.RefViewX.Width + delta;
        location.TranslationX += ExtraX;
        location.IsAdjustedX = true;
        return;
      }
      if (!sameWidget && !isX && this.Location.RefViewY != null) {
        location.RefViewY = this.Location.RefViewY;
        location.ViewY = ((DroidVariable)this.Location).ViewY;
        location.ViewY.GetLocationOnScreen(loc);
        int delta = (int)AutoScaleFunction.TransformSize(16, screenSize.Width);
        ExtraY = ((DroidVariable)location.RefViewY).ExtraY + location.RefViewY.Height - delta;
        location.TranslationY += ExtraY;
        location.IsAdjustedY = true;
        return;
      }

      if (isX && location.RuleX == "CENTER" && location.ViewX == null) {
        int delta = (int)AutoScaleFunction.TransformSize(14, screenSize.Width);
        location.TranslationX -= delta;
      }
      /*if (ViewX.LayoutParameters != null) {
        width = Math.Max(ViewX.LayoutParameters.Width, width);
        height = Math.Max(ViewX.LayoutParameters.Height, height);
      }
      int x = !sameWidget && isX && ViewX != null ? (int)ViewX.GetX() : 0;
      int y = !sameWidget && !isX && ViewX != null ? (int)ViewX.GetY() : 0;

      if (!isX && location.RuleY == "BOTTOM") {
        location.TranslationY += y + height;
      }*/ 
    }
  }
  public class AddAppointmentFunction : ParserFunction
  {
    protected override Variable Evaluate(ParsingScript script)
    {
      List<Variable> args = script.GetFunctionArgs();
      Utils.CheckArgs(args.Count, 4, m_name);

      SfWidget calendar = args[0] as SfWidget;
      string subject = Utils.GetSafeString(args, 1);
      string startStr = Utils.GetSafeString(args, 2);
      string endStr = Utils.GetSafeString(args, 3);
      string colorStr = Utils.GetSafeString(args, 4, "black");
      string dateFormat = Utils.GetSafeString(args, 5, "yyyy/MM/dd HH:mm");

      bool added = calendar.AddAppointment(subject, startStr, endStr, colorStr, dateFormat);

      return calendar;
    }
  }
}
