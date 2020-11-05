using DotNETAssemblyGrapherModel;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotNETAssemblyGrapher
{
    public class AssemblyPointerViewModel : BaseViewModel
    {
        static readonly List<AssemblyPointerViewModel> createdViewModels = new List<AssemblyPointerViewModel>();
        public static AssemblyPointerViewModel Create(AssemblyPointer model)
        {
            AssemblyPointerViewModel result;

            result = createdViewModels.FirstOrDefault(a => a.Id == model.Id);
            if (result == null)
            {
                result = new AssemblyPointerViewModel(model);
                createdViewModels.Add(result);
            }

            return result;
        }

        public static AssemblyPointerViewModel Get(AssemblyPointer model)
            => createdViewModels.FirstOrDefault(a => a.Id == model.Id);

        // Instance members
        
        private AssemblyPointer model;

        private AssemblyPointerViewModel(AssemblyPointer model)
        {
            this.model = model;
            Properties = new ObservableCollection<PropertyViewModel>();
            foreach (Property property in model.Properties)
                Properties.Add(new PropertyViewModel(property));
            foreach (string error in model.Errors)
                Properties.Add(new PropertyViewModel(new Property("Error", error)));
        }

        public string Id => model.Id;

        public ObservableCollection<PropertyViewModel> Properties { get; }
        public int PropertiesCount => model.Properties.Count;

        public int ErrorsCount => model.Errors.Count;

        public bool HasErrors => model.HasErrors;

        public IViewerNode Node
        {
            get => _node;
            set
            {
                if (_node != null)
                {
                    _node.MarkedForDraggingEvent -= Node_MarkedForDragging;
                    _node.UnmarkedForDraggingEvent -= Node_UnmarkedForDragging;
                }

                _node = value;
                _node.MarkedForDraggingEvent += Node_MarkedForDragging;
            }
        }
        private IViewerNode _node;

        // Selected by the graph
        private void Node_MarkedForDragging(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsSelected));
        }

        // Unselected by the graph
        private void Node_UnmarkedForDragging(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsSelected));
        }

        // Selected / Unselected by the treeview
        public bool IsSelected
        {
            get => Node.MarkedForDragging;
            set
            {
                if (_node.MarkedForDragging != value)
                {
                    _node.MarkedForDragging = value;
                }
            }
        }

        public bool IsHidden
        {
            get => !Node.Node.IsVisible;
            set
            {
                Node.Node.IsVisible = !value;
                if (_node.Node.IsVisible)
                {
                    foreach (var inEdge in _node.Node.InEdges)
                    {
                        inEdge.IsVisible = inEdge.SourceNode.IsVisible;
                    }
                    foreach (var outEdge in _node.Node.OutEdges)
                    {
                        outEdge.IsVisible = outEdge.TargetNode.IsVisible;
                    }
                }
                else
                {
                    foreach (var edge in Node.Node.Edges)
                    {
                        edge.IsVisible = false;
                    }
                }

                OnPropertyChanged(nameof(IsHidden));
            }
        }
    }
}