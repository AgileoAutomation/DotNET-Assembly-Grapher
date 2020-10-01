using DotNETAssemblyGrapherModel;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using DotNETAssemblyGrapherApplication;
using Color = Microsoft.Msagl.Drawing.Color;

namespace DotNETAssemblyGrapher
{
    public class GrapherViewModel : BaseViewModel
    {
        private Model model;

        public GrapherViewModel()
        {
            Open = new UICommand(() => true, OpenExecute);
            Center = new UICommand(() => graph != null, CenterExecute);
            Errors = new UICommand(() => GroupViewModels.Any(g => g.ErrorsCount > 0), ErrorsExecute);
        }

        private DependencyGraphViewModel graph;
        private BaseViewModel _mainViewModel = new BaseViewModel();
        public BaseViewModel MainViewModel
        {
            get => _mainViewModel;
            set
            {
                _mainViewModel = value;
                OnPropertyChanged(nameof(MainViewModel));
            }
        }

        #region ToolBar

        public UICommand Open { get; }
        private void OpenExecute()
        {
            fileDialog = new OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = "exe",
                Filter = "Executable files (*.exe)|*.exe|DLL files (*.dll)|*.dll"
            };

            if (directoryInfo != null)
                fileDialog.InitialDirectory = directoryInfo.FullName;

            fileDialog.FileOk += FileOK_OrDropped;
            fileDialog.ShowDialog();
        }

        public UICommand Center { get; }
        private void CenterExecute() { } //graph.Graph.SetInitialTransform();

        private bool areErrorsHidden = false;
        public UICommand Errors { get; }
        private void ErrorsExecute()
        {
            if (areErrorsHidden)
            {
                graph.UpdateErrorNodes();
                areErrorsHidden = false;
            }
            else
            {
                foreach (Node node in graph.Graph.Nodes)
                {
                    if (node.Attr.Color == Color.Red)
                        node.Attr.Color = Color.Black;
                }
                foreach (Edge edge in graph.Graph.Edges)
                {
                    if (edge.Attr.Color == Color.Red)
                        edge.Attr.Color = Color.Black;
                }
                areErrorsHidden = true;
            }
        }

        #endregion

        #region Open New Folder

        FileDialog fileDialog;
        public DirectoryInfo directoryInfo = null;

        public void FileOK_OrDropped(object sender, EventArgs args)
        {
            if (args is DragEventArgs dragArgs)
            {
                string path = ((string[])dragArgs.Data.GetData(DataFormats.FileDrop))[0];
                try
                {
                    directoryInfo = new DirectoryInfo(path);
                }
                catch
                {
                    directoryInfo = new FileInfo(path).Directory;
                }
            }
            else if (args is CancelEventArgs cancelArgs)
            {
                fileDialog.FileOk -= FileOK_OrDropped;
                directoryInfo = new FileInfo(fileDialog.FileName).Directory;
            }
            else return;

            GroupViewModels.Clear();
            model = new Model(directoryInfo);
            MainViewModel = new ModelBuildingViewModel(model, this);
        }

        #endregion

        public bool AreControlsEnabled => model != null;
        public AssemblyPointerViewModel SelectedItem => graph?.assemblies.FirstOrDefault(a => a.IsSelected);

        public ObservableCollection<AssemblyGroupViewModel> GroupViewModels { get; private set; } = new ObservableCollection<AssemblyGroupViewModel>();

        public void BuildGraphAndTreeViewModels()
        {
            // Step 1 : Creating all Assemblies View Models and Binding with GraphViewer Nodes;
            graph = new DependencyGraphViewModel(model);

            // Step 2 : Grouping Assemblies View Models
            foreach (AssemblyPointerGroup group in model.Groups)
            {
                GroupViewModels.Add(new AssemblyGroupViewModel(group, graph.assemblies));
            }

            OnPropertyChanged(nameof(AreControlsEnabled));
            OnPropertyChanged(nameof(GroupViewModels));

            AssemblyPointerViewModel.SelectionChanged += AssemblyInfoViewModel_SelectionChanged;

            MainViewModel = graph;
            Center.OnCanExecuteChanged();
            Errors.OnCanExecuteChanged();
        }

        private void AssemblyInfoViewModel_SelectionChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(SelectedItem));
        }



        WorkbookPart workbookPart;

        internal void Analyze()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "SpreadSheets (*.xlsx) | *.xlsx",
                Multiselect = false
            };

            if (dialog.ShowDialog().HasValue)
            {
                try
                {
                    SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(dialog.FileName, false);
                    workbookPart = spreadsheetDocument.WorkbookPart;
                }
                catch
                {
                    MessageBox.Show("The specification file is open in a other process");
                }

                SheetData sheetData = null;

                try
                {
                    Sheet sheet = workbookPart.Workbook.Sheets.ChildElements.Cast<Sheet>().First(x => x.Name == "Assembly Specification");
                    int index = workbookPart.WorksheetParts.ToList().IndexOf(workbookPart.WorksheetParts.Last()) - workbookPart.Workbook.Sheets.ToList().IndexOf(sheet);
                    sheetData = workbookPart.WorksheetParts.ElementAt(index).Worksheet.Elements<SheetData>().First();
                }
                catch
                {
                    System.Windows.MessageBox.Show("Invalid specification file :\nCouldn't find the 'Assembly Specification' worksheet");
                }

                List<Row> rows = sheetData.Elements<Row>().ToList();
                List<Cell> headerRow = rows.First().Elements<Cell>().ToList();
                rows.RemoveAll(x => !x.Elements<Cell>().Any(y => !string.IsNullOrEmpty(TextInCell(y))));

                string assemblyName;

                if (headerRow.Any(x => TextInCell(x) == "Signed"))
                {
                    List<SignatureSpecification> sigspecs = new List<SignatureSpecification>();
                    int sigIndex = headerRow.IndexOf(headerRow.First(x => TextInCell(x) == "Signed"));

                    foreach (Row r in rows)
                    {
                        List<Cell> row = r.Elements<Cell>().ToList();
                        assemblyName = TextInCell(row.ElementAt(0));
                        sigspecs.Add(new SignatureSpecification(assemblyName, TextInCell(row.ElementAt(sigIndex)) == "x"));
                    }

                    SignatureAnalyzer sigAnalyzer = new SignatureAnalyzer();
                    sigAnalyzer.Analyze(model, sigspecs);
                }

                if (headerRow.Any(x => TextInCell(x) == "Obfuscated"))
                {
                    List<ObfuscationSpecification> obfuscationspecs = new List<ObfuscationSpecification>();
                    int obIndex = headerRow.IndexOf(headerRow.First(x => TextInCell(x) == "Obfuscated"));

                    foreach (Row r in rows)
                    {
                        List<Cell> row = r.Elements<Cell>().ToList();
                        assemblyName = TextInCell(row.ElementAt(0));
                        obfuscationspecs.Add(new ObfuscationSpecification(assemblyName, TextInCell(row.ElementAt(obIndex)) == "x"));
                    }

                    ObfuscationAnalyzer obfuscationAnalyzer = new ObfuscationAnalyzer();
                    obfuscationAnalyzer.Analyze(model, obfuscationspecs);
                }

                graph.UpdateErrorNodes();
            }
        }

        private string TextInCell(Cell c)
        {
            string text = c.InnerText;
            var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
            if (stringTable != null && !string.IsNullOrEmpty(text))
                return stringTable.SharedStringTable.ElementAt(int.Parse(text)).InnerText;
            else
                return null;
        }
    }
}
