using DotNETAssemblyGrapherModel;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace DotNETAssemblyGrapher
{
    public class DependencyGraphViewModel : BaseViewModel
    {
        private Model model;
        public Graph Graph { get; }
        public GraphViewer Viewer { get; }
        public DockPanel GraphPanel { get; }

        public HashSet<AssemblyPointerViewModel> assemblies = new HashSet<AssemblyPointerViewModel>();
        public HashSet<DependencyViewModel> dependencies = new HashSet<DependencyViewModel>();

        public DependencyGraphViewModel(Model model)
        {
            this.model = model;
            Graph = new Graph("Dependency Graph");

            // Draw edges and nodes
            foreach (Dependency dependency in model.Dependencies)
            {
                Graph.AddEdge(dependency.from.Id, dependency.to.Id);
                DependencyViewModel dpvm = new DependencyViewModel(dependency);
                dependencies.Add(dpvm);
                assemblies.Add(dpvm.From);
                assemblies.Add(dpvm.To);
            }

            // Adding alone nodes and creating corresponding View Models
            foreach (AssemblyPointer assembly in model.AllAssemblies)
            {
                Node node = Graph.FindNode(assembly.Id);
                if (node == null)
                {
                    node = Graph.AddNode(assembly.Id);
                    assemblies.Add(AssemblyPointerViewModel.Create(assembly));
                }

                node.Attr.LabelMargin = 4;
            }

            // Init Viewer
            GraphPanel = new DockPanel
            {
                ClipToBounds = true
            };

            Viewer = new GraphViewer();
            Viewer.BindToPanel(GraphPanel);
            Viewer.Graph = Graph;

            if (Viewer.Graph.Nodes.Count() != 0)
            {
                UpdateErrorNodes();

                // Reset events listening for all nodes
                foreach (var obj in Viewer.Entities)
                {
                    if (obj is IViewerNode node)
                    {
                        assemblies.First(a => a.Id == node.Node.Id).Node = node;
                        node.MarkedForDraggingEvent += Node_MarkedForDragging;
                    }
                }
            }
        }

        private Color ConvertSystemDrawingToMsaglColor(SoftwareComponent component)
        {
            Type type = typeof(Color);
            PropertyInfo property = type.GetProperty(component.Color.Name);
            Color adaptedColor = (Color)property.GetValue(null);
            return adaptedColor;
        }

        public void UpdateErrorNodes()
        {
            foreach (AssemblyPointer assembly in model.AllAssemblies)
            {
                if (assembly.HasErrors)
                {
                    Node node = Viewer.Graph.FindNode(assembly.Id);

                    node.Attr.Color = Color.Red;
                    // missing assemblies have in edges color set to red
                    if (!assembly.physicalyExists)
                    {
                        foreach (Edge edge in node.InEdges)
                            edge.Attr.Color = Color.Red;
                    }
                }
            }
        }

        private void Node_MarkedForDragging(object sender, EventArgs e)
        {
            IViewerNode node = sender as IViewerNode;
            node.MarkedForDraggingEvent -= Node_MarkedForDragging;

            node.Node.Attr.FillColor = Color.RoyalBlue;
            node.Node.Label.FontColor = Color.White;
            Viewer.Invalidate(node);

            foreach (var edge in node.OutEdges)
            {
                edge.Edge.Attr.Color = Color.RoyalBlue;
                Viewer.Invalidate(edge);
            }

            node.UnmarkedForDraggingEvent += Node_UnmarkedForDragging;
        }

        private void Node_UnmarkedForDragging(object sender, EventArgs e)
        {
            IViewerNode node = sender as IViewerNode;
            node.UnmarkedForDraggingEvent -= Node_UnmarkedForDragging;

            node.Node.Label.FontColor = Color.Black;
            node.Node.Attr.FillColor = Color.White;
            Viewer.Invalidate(node);

            foreach (var edge in node.OutEdges)
            {
                edge.Edge.Attr.Color = Color.Black;
                Viewer.Invalidate(edge);
            }

            node.MarkedForDraggingEvent += Node_MarkedForDragging;
        }
    }
}
