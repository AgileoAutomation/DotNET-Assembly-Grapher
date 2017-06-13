using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DotNETAssemblyGrapherModel;
using DotNETAssemblyGrapherApplication;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;

namespace DotNETAssemblyGrapher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        Model Model;
        SpecificationWindow SpecWindow;
        GraphViewer GraphViewer = new GraphViewer();
        TreeViewItem SelectedItem;
        TreeViewItem ParentItem;

        public MainWindow()
        {
            InitializeComponent();

            AllowDrop = true;
            DragEnter += MainWindow_DragEnter;
            Drop += MainWindow_Drop;

            // Initialize Views
            GraphViewer.BindToPanel(GraphViewerContainer);
            GraphViewer.GraphChanged += Viewer_GraphChanged;
            treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
        }


        ///////////////////////////////////////////
        /////////BUILD EVENTS AND SETTINGS/////////
        ///////////////////////////////////////////

        private void MainWindow_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                e.Effects = System.Windows.DragDropEffects.Move;
            else
                e.Effects = System.Windows.DragDropEffects.None;
        }

        private void MainWindow_Drop(object sender, System.Windows.DragEventArgs e)
        {
            Topmost = true;
            string path = ((string[])e.Data.GetData(System.Windows.DataFormats.FileDrop))[0];
            Topmost = false;

            if (Directory.Exists(path))
            {
                AllowDrop = false;
                SpecWindow = new SpecificationWindow();
                SpecWindow.ShowDialog();

                if (SpecWindow.OKClicked)
                {
                    while (SpecWindow.OKClicked && !BuildAndDisplay(new DirectoryInfo(path)))
                    {
                        SpecWindow = new SpecificationWindow();
                        SpecWindow.ShowDialog();
                    }
                }
            }
            else
                System.Windows.MessageBox.Show("Invalid Directory");

            AllowDrop = true;
        }

        private void open_Click(object sender, RoutedEventArgs e)
        {
            AllowDrop = false;
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo directory = new DirectoryInfo(dialog.SelectedPath);
                if (directory.Exists
                    && directory.GetFiles().Any(x => x.Extension == ".dll" || x.Extension == ".exe"))
                {
                    SpecWindow = new SpecificationWindow();
                    SpecWindow.ShowDialog();

                    if (SpecWindow.OKClicked)
                    {
                        while (SpecWindow.OKClicked && !BuildAndDisplay(directory))
                        {
                            SpecWindow = new SpecificationWindow();
                            SpecWindow.ShowDialog();
                        }
                    }
                }
                else
                    System.Windows.MessageBox.Show("Invalid Directory");
            }
            AllowDrop = true;
        }

        private bool BuildAndDisplay(DirectoryInfo directory)
        {
            Topmost = true;
            Topmost = false;

            //// Step 1 : Model creation
            Model = new Model();

            if (SpecWindow.ExcludeDefaultCSGraphicLibAssemblies)
            {
                ExcludeDotNETFrameworkAssemblies();
                Model.AddRegex("PresentationCore");
                Model.AddRegex("PresentationFramework");
                Model.AddRegex("WindowsBase");
                Model.AddRegex("System.Deployment");
                Model.AddRegex("System.Drawing");
                Model.AddRegex("System.Windows.Forms");
            }
            else if (SpecWindow.ExcludeDefaultCSLibAssemblies)
            {
                ExcludeDotNETFrameworkAssemblies();
            }

            try
            {
                Model.Build(directory);
                ModelCommonDataOrganizer.Organize(Model);
            }
            catch
            {
                System.Windows.MessageBox.Show("Grapher can analyze only DLLs build with .NET assemblies");
            }

            //// Step 2 : Specifications adding

            if (!string.IsNullOrEmpty(SpecWindow.SpecFilePath))
            {
                try
                {
                    ComponentsBuilder builder = new ComponentsBuilder(SpecWindow.SpecFilePath);
                    builder.Build(Model);
                    Model
                        .SoftwareComponents
                        .Where(x => x.Name != "System Assemblies")
                        .ToList()
                        .ForEach(x => ModelCommonDataOrganizer.UpdateGroups(Model, x));
                }
                catch
                {
                    return false; // an exception is raised in JSONParser or in a SoftwareComponent constructor
                }
            }

            //// Step 3 : Model representation
            ResetViews();
            DrawGraph();
            UpdateTreeView();
            analysis.Visibility = Visibility.Visible;
            hideErrors.Visibility = Visibility.Visible;
            startLabel.Visibility = Visibility.Hidden;

            return true;
        }

        private void ExcludeDotNETFrameworkAssemblies()
        {
            Model.AddRegex("mscorlib");
            Model.AddRegex("System");
            Model.AddRegex("System.Core");
            Model.AddRegex("System.Data");
            Model.AddRegex("System.Data.DataSetExtension");
            Model.AddRegex("System.Net.Http");
            Model.AddRegex("System.Xml");
            Model.AddRegex("System.Xml.Linq");
        }

        private void ResetViews()
        {
            // Reset Treeview
            if (SelectedItem != null)
            {
                SoftwareComponent component = Model.FindComponentByName((string)SelectedItem.Header);
                if (component != null)
                    SelectedItem.Foreground = new SolidColorBrush(ConvertSystemDrawingToWindowsMediaColor(component.Color));
                else if (ParentItem != null
                        && (string)ParentItem.Header == "Groups"
                        && SelectedItem.Items.Cast<TreeViewItem>().Any(x => Model.FindPointerByPrettyName((string)x.Header).HasErrors))
                {
                    SelectedItem.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255,0,0));
                }
                else
                    SelectedItem.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
            }
            SelectedItem = null;

            // Clean TabControl
            tabControl.Opacity = 25;
            propertiesGrid.Visibility = Visibility.Hidden;
            errorsGrid.Visibility = Visibility.Hidden;
            errorsGrid.Items.Clear();
            errorsGrid.Columns.Clear();

            TabItem itemErrors = (TabItem)tabControl.Items.GetItemAt(1);
            itemErrors.Visibility = Visibility.Hidden;

            // Reset nodes to non highlighted mode
            foreach (VNode node in GraphViewer.Entities.Where(x => x is VNode).Cast<VNode>())
            {
                node.Node.Label.FontColor = Microsoft.Msagl.Drawing.Color.Black;
                node.Node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
            }

            foreach (var edge in GraphViewer.Entities.Where(x => x is IViewerEdge).Cast<IViewerEdge>())
            {
                if (edge.Edge.Attr.Color != Microsoft.Msagl.Drawing.Color.Red)
                    edge.Edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
            }
        }

        ////////////////////////////////
        /////////SPECIFICATIONS/////////
        ////////////////////////////////

        WorkbookPart workbookPart;

        private string TextInCell(Cell c)
        {
            string text = c.InnerText;
            var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
            if (stringTable != null && !string.IsNullOrEmpty(text))
                return stringTable.SharedStringTable.ElementAt(int.Parse(text)).InnerText;
            else
                return null;
        }

        private void analysis_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "SpreadSheets (*.xlsx) | *.xlsx";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(dialog.FileName, false);
                    workbookPart = spreadsheetDocument.WorkbookPart;
                }
                catch
                {
                    System.Windows.MessageBox.Show("The specifications file is open in a other process");
                }

                SheetData sheetData = null;

                try
                {
                    Sheet sheet = workbookPart.Workbook.Sheets.ChildElements.Cast<Sheet>().First(x => x.Name == "Assemblies Specifications");
                    int index = workbookPart.WorksheetParts.ToList().IndexOf(workbookPart.WorksheetParts.Last()) - workbookPart.Workbook.Sheets.ToList().IndexOf(sheet);
                    sheetData = workbookPart.WorksheetParts.ElementAt(index).Worksheet.Elements<SheetData>().First();
                }
                catch
                {
                    System.Windows.MessageBox.Show("Invalid specifications file :\nCouldn't find the 'Assemblies Specifications' worksheet");
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
                    sigAnalyzer.Analyze(Model, sigspecs);
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
                    obfuscationAnalyzer.Analyze(Model, obfuscationspecs);
                }

                Model.SoftwareComponents.Where(x => x.Name != "System Assemblies").ToList().ForEach(x => ModelCommonDataOrganizer.UpdateGroups(Model, x));
                UpdateTreeView();
                UpdateErrorNodes(GraphViewer.Graph);
                analysis.Visibility = Visibility.Hidden;
            }
        }

        ///////////////////////////////////////////
        /////////GRAPH SETTINGS AND EVENTS/////////
        ///////////////////////////////////////////

        public void DrawGraph()
        {
            Graph graph = new Graph("Dependencies Graph");

            // Draw edges and nodes
            foreach (Dependency dependency in Model.Dependencies)
            {
                if (!dependency.To.PhysicalyExists)
                {
                    graph.AddEdge(dependency.From.PrettyName, dependency.To.PrettyName).Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                }
                else
                    graph.AddEdge(dependency.From.PrettyName, dependency.To.PrettyName);
            }

            //Set subgraphs
            foreach (SoftwareComponent component in Model.SoftwareComponents)
            {
                graph.RootSubgraph.AddSubgraph(setSubgraph(graph, component));
            }

            // Alone nodes
            foreach (AssemblyPointer pointer in Model.AllAssemblies())
            {
                if (graph.FindNode(pointer.PrettyName) == null)
                {
                    pointer.Errors.Add("This assembly is not referenced and it reference only some hidden system assemblies");
                    Node node = new Node(pointer.PrettyName);
                    graph.AddNode(node);
                }
            }

            GraphViewer.Graph = graph;
        }

        private Subgraph setSubgraph(Graph graph, SoftwareComponent component)
        {
            Subgraph subgraph = new Subgraph(component.Name);

            // Before build children subgraphs
            foreach (SoftwareComponent subcomponent in component.Subcomponents)
            {
                subgraph.AddSubgraph(setSubgraph(graph, subcomponent));
            }

            // Add correct nodes in subgraph
            foreach (AssemblyPointer pointer in component.Assemblies().Where(x => (string)x.Component() == component.Name))
            {
                Node node = graph.FindNode(pointer.PrettyName);
                if (node != null)
                    subgraph.AddNode(node);
            }

            subgraph.Attr.Color = ConvertSystemDrawingToMsaglColor(component);
            return subgraph;
        }

        private Microsoft.Msagl.Drawing.Color ConvertSystemDrawingToMsaglColor(SoftwareComponent component)
        {
            string colorName = component.Color.Name;
            Type type = typeof(Microsoft.Msagl.Drawing.Color);
            PropertyInfo property = type.GetProperty(colorName);
            Microsoft.Msagl.Drawing.Color adaptedColor = (Microsoft.Msagl.Drawing.Color)property.GetValue(null);
            return adaptedColor;
        }

        private void UpdateErrorNodes(Graph graph)
        {
            foreach (AssemblyPointer pointer in Model.AllAssemblies().Where(x => x.HasErrors))
            {
                Node node = graph.FindNode(pointer.PrettyName);

                // Reset red color
                node.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                if (Model.MissingAssemblies().Contains(pointer))
                {
                    foreach (Edge edge in node.InEdges)
                        edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Red;
                }

                // Reset node tooltip
                TextBlock nodeLabel = GraphViewer.GraphCanvas.Children.OfType<TextBlock>().First(x => x.Text == node.LabelText);
                foreach (string error in pointer.Errors)
                {
                    nodeLabel.ToolTip = node.Id;
                    nodeLabel.ToolTip += "\n" + error;
                }
            }
        }

        //////EVENTS//////

        private void Viewer_GraphChanged(object sender, EventArgs e)
        {
            if (GraphViewer.Graph.Nodes.Count() != 0)
            {
                UpdateErrorNodes(GraphViewer.Graph);

                // Reset events listening for all nodes
                foreach (VNode node in GraphViewer.Entities.Where(x => x is VNode))
                {
                    node.MarkedForDraggingEvent += Node_MarkedForDragging;
                    node.UnmarkedForDraggingEvent += Node_UnmarkedForDragging;
                }
            }
        }


        private void Node_MarkedForDragging(object sender, EventArgs e)
        {
            ResetViews();

            if (GraphViewer.Entities.Where(x => x is VNode && x.MarkedForDragging).Count() == 1)
            {
                VNode node = (VNode)sender;
                if (!(node.Node is Subgraph))
                {
                    AssemblyPointer pointer = Model.FindPointerByPrettyName(node.Node.LabelText);
                    ShowPropertiesAndErrors(pointer);

                    // Set referenced nodes to highlighted mode
                    foreach (var edge in node.OutEdges)
                    {
                        if (edge.Edge.Attr.Color != Microsoft.Msagl.Drawing.Color.Red)
                            edge.Edge.Attr.Color = Microsoft.Msagl.Drawing.Color.CadetBlue;

                        edge.Edge.TargetNode.Label.FontColor = Microsoft.Msagl.Drawing.Color.White;
                        edge.Edge.TargetNode.Attr.FillColor = Microsoft.Msagl.Drawing.Color.CadetBlue;
                    }

                    // Set referencers nodes to highlighted mode
                    foreach (var edge in node.InEdges)
                    {
                        edge.Edge.SourceNode.Label.FontColor = Microsoft.Msagl.Drawing.Color.White;
                        edge.Edge.SourceNode.Attr.FillColor = Microsoft.Msagl.Drawing.Color.DarkBlue;
                    }
                }
            }
        }

        private void Node_UnmarkedForDragging(object sender, EventArgs e)
        {
            if (!treeView.IsFocused && !tabControl.IsFocused)
            {
                ResetViews();
            }
        }

        bool errorsHidden = false;
        private void hideErrors_Click(object sender, RoutedEventArgs e)
        {
            if (errorsHidden)
            {// show errors
                UpdateErrorNodes(GraphViewer.Graph);
                errorsHidden = false;
                hideErrors.Content = "Hide Errors";
            }
            else
            {// hide errors
                foreach (Node node in GraphViewer.Graph.Nodes.Where(x => x.Attr.Color == Microsoft.Msagl.Drawing.Color.Red))
                {
                    node.Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
                }
                foreach (Edge edge in GraphViewer.Graph.Edges.Where(x => x.Attr.Color == Microsoft.Msagl.Drawing.Color.Red))
                {
                    edge.Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
                }
                errorsHidden = true;
                hideErrors.Content = "Show Errors";
            }
        }

        /////////////////////////////////////
        /////////TABCONTROL SETTINGS/////////
        /////////////////////////////////////

        private void ShowPropertiesAndErrors(AssemblyPointer pointer)
        {
            tabControl.Opacity = 100;

            if (pointer.HasErrors)
            {// Set Errors and Properties in TabControl
                DataGridTextColumn c = new DataGridTextColumn();
                System.Windows.Style style = new System.Windows.Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                c.ElementStyle = style;
                c.Header = "Errors";
                c.Binding = new System.Windows.Data.Binding();
                c.Width = 336;
                errorsGrid.Columns.Add(c);

                pointer.Errors.ForEach(x => { errorsGrid.Items.Add(x); });
                errorsGrid.Visibility = Visibility.Visible;
                TabItem itemErrors = (TabItem)tabControl.Items.GetItemAt(1);
                itemErrors.Visibility = Visibility.Visible;

                SetPropetiesGrid(pointer);
            }
            else
            {
                SetPropetiesGrid(pointer);
            }
        }

        private void SetPropetiesGrid(AssemblyPointer pointer)
        {
            propertiesGrid.ItemsSource = pointer.Properties;
            propertiesGrid.Visibility = Visibility.Visible;
        }

        ///////////////////////////////////
        /////////TREEVIEW SETTINGS/////////
        ///////////////////////////////////

        private void UpdateTreeView()
        {
            treeView.Items.Clear();

            TreeViewItem modelItem = new TreeViewItem();
            modelItem.Header = "Model";
            TreeViewItem groupsItem = new TreeViewItem();
            groupsItem.Header = "Groups";
            TreeViewItem componentsItem = new TreeViewItem();
            componentsItem.Header = "Components";

            // Set TreeviewItems
            foreach (AssemblyPointerGroup group in Model.AssemblyPointerGroups)
            {
                groupsItem.Items.Add(SetTreeViewGroupItem(group));
            }

            foreach (SoftwareComponent component in Model.SoftwareComponents)
            {
                componentsItem.Items.Add(SetTreeViewComponentItem(component));
            }

            //Treeview Build
            modelItem.Items.Add(groupsItem);
            modelItem.Items.Add(componentsItem);

            modelItem.IsExpanded = true;
            treeView.Items.Add(modelItem);
            foreach (TreeViewItem item in treeView.Items)
                OpenTreeView(item);

            treeView.Opacity = 100;
        }

        private void OpenTreeView(TreeViewItem item)
        {
            foreach (TreeViewItem subItem in item.Items)
            {
                if ((string)subItem.Header == "Groups")
                {
                    subItem.IsExpanded = true;
                }
                if ((string)subItem.Header == "Subcomponents"
                    || (string)subItem.Header == "Components")
                {
                    subItem.IsExpanded = true;
                    foreach (TreeViewItem subSubItem in subItem.Items)
                    {
                        subSubItem.IsExpanded = true;
                        OpenTreeView(subSubItem);
                    }
                }
            }
        }

        private TreeViewItem SetTreeViewComponentItem(SoftwareComponent component)
        {
            TreeViewItem componentItem = new TreeViewItem();
            componentItem.Header = component.Name;

            //Color Management
            if (component.Color.Name != "Black")
            {
                componentItem.Foreground = new SolidColorBrush(ConvertSystemDrawingToWindowsMediaColor(component.Color));
            }

           // Groups build
            if (component.HasGroups)
            {
                TreeViewItem groupsItem = new TreeViewItem();
                groupsItem.Header = "Groups";

                foreach (AssemblyPointerGroup group in component.AssemblyPointerGroups)
                {
                    groupsItem.Items.Add(SetTreeViewGroupItem(group));
                }

                componentItem.Items.Add(groupsItem);
            }
            // Subcomponents build
            if (component.HasSubcomponents)
            {
                TreeViewItem subcomponentsItem = new TreeViewItem();
                subcomponentsItem.Header = "Subcomponents";

                foreach (SoftwareComponent subcomponent in component.Subcomponents)
                {
                    subcomponentsItem.Items.Add(SetTreeViewComponentItem(subcomponent));
                }

                componentItem.Items.Add(subcomponentsItem);
            }

            return componentItem;
        }

        private TreeViewItem SetTreeViewGroupItem(AssemblyPointerGroup group)
        {
            TreeViewItem groupItem = new TreeViewItem();
            groupItem.Header = group.Name;

            // Reported Errors
            if (group.Pointers.Any(x => x.HasErrors))
                ColorizeErrorItem(groupItem);

            foreach (AssemblyPointer pointer in group.Pointers)
            {
                TreeViewItem pointerItem = new TreeViewItem();
                pointerItem.Header = pointer.PrettyName;
                if (pointer.HasErrors)
                    ColorizeErrorItem(pointerItem);

                groupItem.Items.Add(pointerItem);
            }

            return groupItem;
        }

        private void ColorizeErrorItem(TreeViewItem item)
        {
            SolidColorBrush brush = new SolidColorBrush(ConvertSystemDrawingToWindowsMediaColor(System.Drawing.Color.Red));
            item.Foreground = brush;
        }

        private System.Windows.Media.Color ConvertSystemDrawingToWindowsMediaColor(System.Drawing.Color color)
        {
            string colorName = color.Name;
            Type type = typeof(System.Windows.Media.Colors);
            PropertyInfo property = type.GetProperty(colorName);
            System.Windows.Media.Color adaptedColor = (System.Windows.Media.Color)property.GetValue(null);
            return adaptedColor;
        }

        //////EVENTS//////

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ResetViews();

            SelectedItem = (TreeViewItem)treeView.SelectedItem;


            if (SelectedItem != null)
            {
                SelectedItem.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));

                if ((string)SelectedItem.Header != "Model"
                    && SelectedItem.Parent != null)
                    ParentItem = (TreeViewItem)SelectedItem.Parent;

                if (SelectedItem.HasItems
                    && ParentItem != null
                    && (string)ParentItem.Header == "Groups")
                {// Set all assemblies of a selected group to highlighted mode
                    foreach (TreeViewItem assembly in SelectedItem.Items)
                    {
                        List<VNode> nodes = GraphViewer.Entities.Where(x => x is VNode).Cast<VNode>().ToList();
                        VNode node = nodes.FirstOrDefault(x => x.Node.Id == (string)assembly.Header);

                        if (node != null)
                        {
                            node.Node.Label.FontColor = Microsoft.Msagl.Drawing.Color.White;
                            node.Node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.DodgerBlue;
                        }
                    }
                }
                else if (SelectedItem.HasItems
                        && ((string)SelectedItem.Header == "Components" || (string)SelectedItem.Header == "Subcomponents"))
                {// Set all components of a selected components group to highlighted mode
                    foreach (TreeViewItem component in SelectedItem.Items)
                    {
                        VNode node = GraphViewer.Entities.Where(x => x is VNode).Cast<VNode>().FirstOrDefault(x => x.Node.Id == (string)SelectedItem.Header);

                        if (node != null)
                        {
                            node.Node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.DodgerBlue;
                        }
                    }
                }
                else if (SelectedItem.HasItems
                        && ParentItem != null
                        && ((string)ParentItem.Header == "Components" || (string)ParentItem.Header == "Subcomponents"))
                {// Set a selected component to highlighted mode
                    VNode node = GraphViewer.Entities.Where(x => x is VNode).Cast<VNode>().FirstOrDefault(x => x.Node.Id == (string)SelectedItem.Header);

                    if (node != null)
                    {
                        node.Node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.DodgerBlue;
                    }
                }
                else if (!SelectedItem.HasItems)
                {// Set a selected assembly to highlighted mode
                    AssemblyPointer pointer = Model.FindPointerByPrettyName((string)SelectedItem.Header);
                    if (pointer != null)
                        ShowPropertiesAndErrors(pointer);

                    List<VNode> nodes = GraphViewer.Entities.Where(x => x is VNode).Cast<VNode>().ToList();
                    VNode node = nodes.FirstOrDefault(x => x.Node.Id == (string)SelectedItem.Header);

                    if (node != null)
                    {
                        node.Node.Label.FontColor = Microsoft.Msagl.Drawing.Color.White;
                        node.Node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.DodgerBlue;
                    }
                }
            }
        }
    }
}
