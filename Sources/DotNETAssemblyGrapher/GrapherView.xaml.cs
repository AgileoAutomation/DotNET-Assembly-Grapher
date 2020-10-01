using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;

namespace DotNETAssemblyGrapher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class GrapherView : Window
    {
        GrapherViewModel grapher;

        public GrapherView()
        {
            DataContext = grapher = new GrapherViewModel();

            InitializeComponent();

            DragEnter += OnDragEnter;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            Drop += grapher.FileOK_OrDropped;
            Drop += FileDropped;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Move;
            else
                e.Effects = DragDropEffects.None;
        }

        private void FileDropped(object sender, DragEventArgs e)
        {
            Drop -= grapher.FileOK_OrDropped;
            Drop -= FileDropped;
        }
        
        private void Analysis_Click(object sender, RoutedEventArgs e)
        {
            grapher.Analyze();
        }
    }

    public class LeftMarginMultiplierConverter : IValueConverter
    {
        public double Length { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TreeViewItem item))
                return new Thickness(0);
            
            return new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        { 
            TreeViewItem parent;
            while ((parent = GetParent(item)) != null)        
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

    private static TreeViewItem GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);        while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
    }
}