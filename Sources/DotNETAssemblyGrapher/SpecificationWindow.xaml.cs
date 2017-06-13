using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.IO;

namespace DotNETAssemblyGrapher
{
    public partial class SpecificationWindow : Window
    {
        public bool OKClicked { get; private set; } = false;
        public bool ExcludeDefaultCSLibAssemblies { get; private set; } = false;
        public bool ExcludeDefaultCSGraphicLibAssemblies { get; private set; } = false;
        public string SpecFilePath { get; private set; }

        public SpecificationWindow()
        {
            InitializeComponent();

            AllowDrop = true;
            DragEnter += SpecWindow_DragEnter;
        }

        private void SpecWindow_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            string path = ((string[])e.Data.GetData(System.Windows.DataFormats.FileDrop))[0];
            FileInfo file = new FileInfo(path);
            if (file.Exists && (file.Extension == ".json" || file.Extension == ".xlsx"))
            {
                textBox.Text = file.FullName;
            }
            else
            {
                textBox.Text = "Invalid Specification File";
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All specifications files (*.json;*.xlsx)|*.json;*.xlsx|JSON files (*.json)|*.json|Excel files (*.xlsx)|*.xlsx";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox.Text = dialog.FileName;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            OKClicked = true;

            if (excludeDefaultCSLibAssemblies.IsChecked == true)
                ExcludeDefaultCSLibAssemblies = true;
            if (excludeDefaultCSGraphicLibAssemblies.IsChecked == true)
                ExcludeDefaultCSGraphicLibAssemblies = true;

            if (!string.IsNullOrEmpty(SpecFilePath))
            {
                FileInfo file = new FileInfo(SpecFilePath);
                if (!file.Exists || (file.Extension != ".json" && file.Extension != ".xlsx"))
                {
                    System.Windows.MessageBox.Show("Invalid Specification File");
                    SpecFilePath = "";
                    textBox.Text = "";
                }
                else
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SpecFilePath = textBox.Text;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OKClicked = false;
            Close();
        }
    }
}
