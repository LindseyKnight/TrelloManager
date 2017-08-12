using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Trello.Wpf.ViewModels;

namespace Trello.Wpf.Utility
{
    public sealed class GridViewSort
    {
        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand) obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(GridViewSort), new UIPropertyMetadata(null, AddCommandHandler));

        public static bool GetAutoSort(DependencyObject obj)
        {
            return (bool) obj.GetValue(AutoSortProperty);
        }

        public static void SetAutoSort(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSortProperty, value);
        }

        public static readonly DependencyProperty AutoSortProperty =
            DependencyProperty.RegisterAttached("AutoSort", typeof(bool), typeof(GridViewSort), new UIPropertyMetadata(false, AddAutoSortHandler));

        public static string GetPropertyName(DependencyObject obj)
        {
            return (string) obj.GetValue(PropertyNameProperty);
        }

        public static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(GridViewSort), new UIPropertyMetadata(null));

        private static GridViewColumnHeader GetSortedColumnHeader(DependencyObject obj)
        {
            return (GridViewColumnHeader) obj.GetValue(SortedColumnHeaderProperty);
        }

        private static void SetSortedColumnHeader(DependencyObject obj, GridViewColumnHeader value)
        {
            obj.SetValue(SortedColumnHeaderProperty, value);
        }

        private static readonly DependencyProperty SortedColumnHeaderProperty =
            DependencyProperty.RegisterAttached("SortedColumnHeader", typeof(GridViewColumnHeader), typeof(GridViewSort), new UIPropertyMetadata(null));

        private static void AddCommandHandler(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl listView = o as ItemsControl;
            if (listView != null)
            {
                if (!GetAutoSort(listView))
                {
                    if (e.OldValue != null && e.NewValue == null)
                        listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                    if (e.OldValue == null && e.NewValue != null)
                        listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                }
            }
        }

        private static void AddAutoSortHandler(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ListView listView = o as ListView;
            if (listView != null)
            {
                // Don't change click handler if a command is set
                if (GetCommand(listView) == null)
                {
                    bool oldValue = (bool) e.OldValue;
                    bool newValue = (bool) e.NewValue;
                    if (oldValue && !newValue)
                        listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                    if (!oldValue && newValue)
                        listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                }
            }
        }

        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                string propertyName = GetPropertyName(headerClicked.Column);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    ListView listView = GetAncestor<ListView>(headerClicked);
                    if (listView != null)
                    {
                        ICommand command = GetCommand(listView);
                        if (command != null)
                        {
                            if (command.CanExecute(propertyName))
                                command.Execute(propertyName);
                        }
                        else if (GetAutoSort(listView))
                        {
                            ApplySort(listView.Items, propertyName, listView, headerClicked);
                        }
                    }
                }
            }
        }

        private static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
                parent = VisualTreeHelper.GetParent(parent);

            return (T) parent;
        }

        private static void ApplySort(ICollectionView view, string propertyName, ListView listView, GridViewColumnHeader sortedColumnHeader)
        {
            ListSortDirection direction = ListSortDirection.Ascending;
            SortDescription groupKeySort = view.SortDescriptions.FirstOrDefault(x => x.PropertyName == "GroupKey");
            SortDescription secondarySort = view.SortDescriptions.FirstOrDefault(x => x.PropertyName != "GroupKey");
            if (secondarySort.PropertyName == propertyName)
                direction = secondarySort.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

            GridViewColumnHeader currentSortedColumnHeader = GetSortedColumnHeader(listView);
            if (currentSortedColumnHeader != null)
                RemoveSortGlyph(currentSortedColumnHeader);

            view.SortDescriptions.Clear();
            if (groupKeySort.PropertyName == "GroupKey")
                view.SortDescriptions.Add(groupKeySort);

            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
                AddSortGlyph(sortedColumnHeader, direction);
                SetSortedColumnHeader(listView, sortedColumnHeader);
            }

            MainWindowViewModel viewModel = ((MainWindow) listView.DataContext).MainWindowViewModel;
            viewModel.CardsSortPropertyName = propertyName;
            viewModel.CardsSortDirection = direction;
        }

        private static void AddSortGlyph(GridViewColumnHeader columnHeader, ListSortDirection direction)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            adornerLayer.Add(new SortGlyphAdorner(columnHeader, direction));
        }

        private static void RemoveSortGlyph(GridViewColumnHeader columnHeader)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
            Adorner[] adorners = adornerLayer.GetAdorners(columnHeader);
            if (adorners != null)
            {
                foreach (Adorner adorner in adorners)
                {
                    if (adorner is SortGlyphAdorner)
                        adornerLayer.Remove(adorner);
                }
            }
        }

        private class SortGlyphAdorner : Adorner
        {
            public SortGlyphAdorner(GridViewColumnHeader columnHeader, ListSortDirection direction)
                : base(columnHeader)
            {
                m_columnHeader = columnHeader;
                m_direction = direction;
            }

            private Geometry GetDefaultGlyph()
            {
                double x1 = m_columnHeader.ActualWidth - 13;
                double x2 = x1 + 10;
                double x3 = x1 + 5;
                double y1 = m_columnHeader.ActualHeight / 2 - 3;
                double y2 = y1 + 5;

                if (m_direction == ListSortDirection.Ascending)
                {
                    double tmp = y1;
                    y1 = y2;
                    y2 = tmp;
                }

                PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
                pathSegmentCollection.Add(new LineSegment(new Point(x2, y1), true));
                pathSegmentCollection.Add(new LineSegment(new Point(x3, y2), true));

                PathFigure pathFigure = new PathFigure(
                    new Point(x1, y1),
                    pathSegmentCollection,
                    true);

                PathFigureCollection pathFigureCollection = new PathFigureCollection { pathFigure };
                PathGeometry pathGeometry = new PathGeometry(pathFigureCollection);
                return pathGeometry;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                drawingContext.DrawGeometry(Brushes.LightGray, new Pen(Brushes.Gray, 1.0), GetDefaultGlyph());
            }

            readonly GridViewColumnHeader m_columnHeader;
            readonly ListSortDirection m_direction;
        }
    }
}
