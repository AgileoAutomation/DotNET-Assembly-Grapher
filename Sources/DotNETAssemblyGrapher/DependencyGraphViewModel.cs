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

        public DependencyGraphViewModel(Model model)
        {
            this.model = model;
            Graph = new Graph("Dependency Graph");

            // Draw edges and nodes
            foreach (Dependency dependency in model.Dependencies)
            {
                Graph.AddEdge(dependency.from.Id, dependency.to.Id);
            }

            //Set subgraphs
            foreach (SoftwareComponent component in model.SoftwareComponents)
            {
                Graph.RootSubgraph.AddSubgraph(SetSubgraph(Graph, component));
            }

            // Creating View Models and adding alone nodes
            foreach (AssemblyPointer assembly in model.AllAssemblies)
            {
                assemblies.Add(new AssemblyPointerViewModel(assembly));

                Node node = Graph.FindNode(assembly.Id);
                if (node == null)
                    node = Graph.AddNode(assembly.Id);
                node.Attr.LabelMargin = 3;
            }

            GraphPanel = new DockPanel
            {
                ClipToBounds = true
            };
            Viewer = new GraphViewer();
            Viewer.BindToPanel(GraphPanel);
            Viewer.GraphChanged += Viewer_GraphChanged;
            AssemblyPointerViewModel.SelectionChanged += AssemblyPointerViewModel_SelectionChanged;
            Viewer.Graph = Graph;
        }

        private void AssemblyPointerViewModel_SelectionChanged(object sender, EventArgs e)
        {
            Viewer.Invalidate((sender as AssemblyPointerViewModel).Node);

        }

        private Subgraph SetSubgraph(Graph graph, SoftwareComponent component)
        {
            Subgraph subgraph = new Subgraph(component.Name);

            // Before build children subgraphs
            foreach (SoftwareComponent subcomponent in component.Subcomponents)
            {
                subgraph.AddSubgraph(SetSubgraph(graph, subcomponent));
            }

            // Add correct nodes in subgraph
            foreach (AssemblyPointer pointer in component.AllAssemblies)
            {
                Property comp = pointer.FindProperty("Component");
                if (comp != null
                    && comp.value == component.Name)
                {
                    Node node = graph.FindNode(pointer.Id);
                    if (node != null)
                        subgraph.AddNode(node);
                }
            }

            subgraph.Attr.Color = ConvertSystemDrawingToMsaglColor(component);
            return subgraph;
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

        private void Viewer_GraphChanged(object sender, EventArgs e)
        {
            if (Viewer.Graph.Nodes.Count() != 0)
            {
                UpdateErrorNodes();

                // Reset events listening for all nodes
                foreach (var obj in Viewer.Entities)
                {
                    if (obj is IViewerNode node)
                        assemblies.First(a => a.Id == node.Node.Id).Node = node;
                }
            }
        }
    }
}
