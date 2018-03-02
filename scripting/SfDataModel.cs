using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using SplitAndMerge;
using Syncfusion.SfDataGrid;

namespace SplitAndMerge
{
  public class DataModel : INotifyPropertyChanged
  {
    public enum COL_TYPE {STRING, INT, DOUBLE, CURRENCY, DATE};

    public static List<string>   ColNames = new List<string>();
    public static List<COL_TYPE> ColTypes = new List<COL_TYPE>();

    ObservableCollection<DataPoint> m_data = new ObservableCollection<DataPoint>();
    SfDataGrid m_grid;

    public DataModel(SfDataGrid grid)
    {
      m_grid = grid;
    }

    public ObservableCollection<DataPoint> DataPoints {
      get { return m_data; }
      set { m_data = value; OnPropertyChanged("DataPoints"); }
    }

    public void SwapRows(int from, int to)
    {
      if (from == to) {
        return;
      }
      //if (from < to - 1) {
      //  to--;
      //}

      Console.WriteLine("Old: " + ToString());

      ObservableCollection<DataPoint> newData = new ObservableCollection<DataPoint>();
      int min = Math.Min(from, to);
      int max = Math.Max(from, to);
      for (int i = 0; i < min; i++) {
        newData.Add(new DataPoint(m_data[i]));
      }

      if (from < to) {
        for (int i = min; i < max; i++) {
          newData.Add(new DataPoint(m_data[i + 1]));
        }
        newData.Add(new DataPoint(m_data[from]));
      } else {
        newData.Add(new DataPoint(m_data[from]));
        for (int i = min; i < max; i++) {
          newData.Add(new DataPoint(m_data[i]));
        }
      }

      for (int i = max + 1; i < m_data.Count; i++) {
        DataPoint pt = new DataPoint(m_data[i]);
        newData.Add(new DataPoint(m_data[i]));
      }

      m_data = newData;

      Console.WriteLine("New: " + ToString());
    }

    public void Reload()
    {
      List<DataPoint> points = new List<DataPoint>(m_data);
      m_data = new ObservableCollection<DataPoint>(points);
      OnPropertyChanged("DataPoints");
    }
    public void Sort(string colName, bool ascending)
    {
      int colId = ColName2Id(colName);
      List<DataPoint> points = new List<DataPoint>(m_data);
      if (ColTypes[colId] == COL_TYPE.STRING) {
        points.Sort((a, b) => a.GetStringValue(colId).CompareTo(b.GetStringValue(colId)));
      } else {
        points.Sort((a, b) => a.GetNumValue(colId).CompareTo(b.GetNumValue(colId)));
      }

      if (!ascending) {
        points.Reverse();
      }

      m_data = new ObservableCollection<DataPoint>(points);
    }

    public int ColName2Id(string colName)
    {
      string idStr = colName.Substring(3);
      int id = (int)Utils.ConvertToDouble(idStr);
      return id;
    }

    public void AddColumns(List<string> data)
    {
      for (int i = 0; i < data.Count - 1; i += 2) {
        GridTextColumn column = new GridTextColumn();
        column.HeaderText = data[i];
        ColNames.Add(data[i]);
        switch(data[i + 1]) {
          case "string" :
            column.MappingName = "Str" + i/2;
            ColTypes.Add(COL_TYPE.STRING);
            break;
          case "currency":
            column.MappingName = "Num" + i / 2;
            ColTypes.Add(COL_TYPE.CURRENCY);
            column.Format = "C";
            break;
          case "number":
            column.MappingName = "Num" + i / 2;
            ColTypes.Add(COL_TYPE.DOUBLE);
            break;
          default:
            column.MappingName = "Str" + i / 2;
            ColTypes.Add(COL_TYPE.STRING);
            break;
        }
        m_grid.Columns.Add(column);
      }
    }

    public void AddPoint(List<string> values)
    {
      DataPoint dataPoint = new DataPoint(values);
      m_data.Add(dataPoint);
      dataPoint.PropertyChanged += (sender, e) => {
        OnPropertyChanged(e.PropertyName);
      };
      OnPropertyChanged("DataPoints");
    }

    public void RemovePoint(int rowIndex)
    {
      m_data.RemoveAt(rowIndex);
      OnPropertyChanged("DataPoints");
    }
    public DataPoint GetPoint(int rowIndex)
    {
       if (rowIndex < 0 || rowIndex >= m_data.Count) {
        return null;
      }
      return m_data[rowIndex];
    }

    public override string ToString()
    {
      StringBuilder result = new StringBuilder();
      for (int i = 0; i < m_data.Count; i++) {
        result.Append(m_data[i].ToString() + ", ");
      }
      return result.ToString();
    }
    void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public event PropertyChangedEventHandler PropertyChanged;
  }

  public class DataPoint : INotifyPropertyChanged
  {
    const int MAX_COLS = 10;
    IComparable xValue;
    List<string> m_str = new List<string>(new string[MAX_COLS]);
    List<int> m_int = new List<int>(new int[MAX_COLS]);
    List<double> m_num = new List<double>(new double[MAX_COLS]);

    public IComparable XValue {
      get { return xValue; }
      set { xValue = value; OnPropertyChanged("XValue"); }
    }
    double yValue;
    public double YValue {
      get { return yValue; }
      set { yValue = value; OnPropertyChanged("YValue"); }
    }
    public string Str0 { get { return m_str[0]; } set { m_str[0] = value; OnPropertyChanged("Str0"); } }
    public string Str1 { get { return m_str[1]; } set { m_str[1] = value; OnPropertyChanged("Str1"); } }
    public string Str2 { get { return m_str[2]; } set { m_str[2] = value; OnPropertyChanged("Str2"); } }
    public string Str3 { get { return m_str[3]; } set { m_str[3] = value; OnPropertyChanged("Str3"); } }
    public string Str4 { get { return m_str[4]; } set { m_str[4] = value; OnPropertyChanged("Str4"); } }
    public string Str5 { get { return m_str[5]; } set { m_str[5] = value; OnPropertyChanged("Str5"); } }
    public string Str6 { get { return m_str[6]; } set { m_str[6] = value; OnPropertyChanged("Str6"); } }
    public string Str7 { get { return m_str[7]; } set { m_str[7] = value; OnPropertyChanged("Str7"); } }
    public string Str8 { get { return m_str[8]; } set { m_str[8] = value; OnPropertyChanged("Str8"); } }
    public string Str9 { get { return m_str[9]; } set { m_str[9] = value; OnPropertyChanged("Str9"); } }
    public double Num0 { get { return m_num[0]; } set { m_num[0] = value; OnPropertyChanged("Num0"); } }
    public double Num1 { get { return m_num[1]; } set { m_num[1] = value; OnPropertyChanged("Num1"); } }
    public double Num2 { get { return m_num[2]; } set { m_num[2] = value; OnPropertyChanged("Num2"); } }
    public double Num3 { get { return m_num[3]; } set { m_num[3] = value; OnPropertyChanged("Num3"); } }
    public double Num4 { get { return m_num[4]; } set { m_num[4] = value; OnPropertyChanged("Num4"); } }
    public double Num5 { get { return m_num[5]; } set { m_num[5] = value; OnPropertyChanged("Num5"); } }
    public double Num6 { get { return m_num[6]; } set { m_num[6] = value; OnPropertyChanged("Num6"); } }
    public double Num7 { get { return m_num[7]; } set { m_num[7] = value; OnPropertyChanged("Num7"); } }
    public double Num8 { get { return m_num[8]; } set { m_num[8] = value; OnPropertyChanged("Num8"); } }
    public double Num9 { get { return m_num[9]; } set { m_num[9] = value; OnPropertyChanged("Num9"); } }

    public DataPoint()
    {
    }

    public DataPoint(IComparable xValue, double yValue)
    {
      XValue = xValue;
      YValue = yValue;
    }

    public DataPoint(List<string> values)
    {
      for (int i = 0; i < values.Count; i++) {
        Set(i, values[i]);
      }
    }
    public DataPoint(DataPoint other)
    {
      SetDataPoint(other);
    }
    public void SetDataPoint(DataPoint other)
    {
      for (int i = 0; i < DataModel.ColNames.Count; i++) {
        Set(i, other.GetStringValue(i));
      }
    }
    public void Set(int index, string value)
    {
      if (DataModel.ColTypes[index] == DataModel.COL_TYPE.STRING) {
        m_str[index] = value;
      } else {
        m_num[index] = Utils.ConvertToDouble(value);
      } 
    }
    public void Assign(int index, string value)
    {
      if (index == 0) { Str0 = value; return; }
      if (index == 1) { Str1 = value; return; }
      if (index == 2) { Str2 = value; return; }
      if (index == 3) { Str3 = value; return; }
      if (index == 4) { Str4 = value; return; }
      if (index == 5) { Str5 = value; return; }
      if (index == 6) { Str6 = value; return; }
      if (index == 7) { Str7 = value; return; }
      if (index == 8) { Str8 = value; return; }
      if (index == 9) { Str9 = value; return; }
    }
    public void Assign(int index, double value)
    {
      if (index == 0) { Num0 = value; return; }
      if (index == 1) { Num1 = value; return; }
      if (index == 2) { Num2 = value; return; }
      if (index == 3) { Num3 = value; return; }
      if (index == 4) { Num4 = value; return; }
      if (index == 5) { Num5 = value; return; }
      if (index == 6) { Num6 = value; return; }
      if (index == 7) { Num7 = value; return; }
      if (index == 8) { Num8 = value; return; }
      if (index == 9) { Num9 = value; return; }
    }
    public string GetStringValue(int index)
    {
      if (DataModel.ColTypes.Count <= index) {
        return "";
      } else if (DataModel.ColTypes[index] == DataModel.COL_TYPE.STRING) {
        return m_str[index];
      } else {
        return "" + m_num[index];
      }
    }
    public double GetNumValue(int index)
    {
      return m_num[index];
    }

    public override string ToString()
    {
      StringBuilder result = new StringBuilder();
      for (int i = 0; i < MAX_COLS; i++) {
        var part = GetStringValue(i);
        if (string.IsNullOrEmpty(part)) {
          break;
        }
        result.Append(GetStringValue(i) + " "); break;
      }
      return result.ToString();
    }

		void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public event PropertyChangedEventHandler PropertyChanged;

	}
}
