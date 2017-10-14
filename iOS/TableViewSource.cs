using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace scripting.iOS
{
    public class TableViewSource : UITableViewSource
    {
        List<string> m_names = null;
        List<UIImage> m_pics = null;

        public nfloat FontSize { set; get; }

        public List<string> Data {
            set { m_names = value; }
            get { return m_names; }
        }
        public List<UIImage> Images {
            set { m_pics = value; }
            get { return m_pics; }
        }
        string CellIdentifier = "TableCell";
        public RowSelectedDel RowSelectedDel;

        public TableViewSource(List<string> names = null, List<UIImage> pics = null)
        {
            m_names = names;
            m_pics = pics;
            FontSize = 14f;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (m_names != null) {
                return m_names.Count;
            }
            if (m_pics != null) {
                return m_pics.Count;
            }
            return 0;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);

            string item = (m_names == null || m_names.Count < indexPath.Row + 1) ? "" : m_names[indexPath.Row];
            UIImage img = (m_pics == null || m_pics.Count < indexPath.Row + 1) ? null : m_pics[indexPath.Row];

            //---- if there are no cells to reuse, create a new one
            cell = cell ?? new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);

            cell.BackgroundColor = UIColor.Clear;
            cell.TextLabel.Text = item;
            cell.TextLabel.Font = cell.TextLabel.Font.WithSize(FontSize);
            cell.TextLabel.Lines = 3;
            cell.TextLabel.BackgroundColor = UIColor.Clear;
            cell.ImageView.Image = img;

            return cell;
        }

        /*public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            //base.WillDisplay(tableView, cell, indexPath);
            cell.BackgroundColor = UIColor.Clear;
            //cell.BackgroundView = null;
            //UIImageView line = new UIImageView(new CoreGraphics.CGRect(1, 25, 260, 1));
            //line.BackgroundColor = UIColor.LightGray;
            //cell.AddSubview(line);
            //cell.textLabel.font = [cell.textLabel.font fontWithSize:16];
        }*/

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            int row = indexPath.Row;
            if (row >= m_names.Count) {
                return;
            }

            // Remove the selected background from the cell:
            UITableViewCell cell = tableView.CellAt(indexPath);
            UIView bgView = new UIView();
            bgView.BackgroundColor = UIColor.Clear;
            cell.SelectedBackgroundView = bgView;

            RowSelectedDel?.Invoke(row);
        }
    }
}
