using DotNETAssemblyGrapherModel;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.ObjectModel;

namespace DotNETAssemblyGrapher
{
    public class AssemblyPointerViewModel : BaseViewModel
    {
        internal static event EventHandler SelectionChanged;
        private AssemblyPointer model;

        public AssemblyPointerViewModel(AssemblyPointer model)
        {
            this.model = model;
        }

        public string Id => model.Id;

        public ObservableCollection<Property> Properties { get; }
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
                    _node.MarkedForDraggingEvent -= Node_MarkedForDraggingEvent;
                    _node.UnmarkedForDraggingEvent -= Node_UnmarkedForDraggingEvent;
                }

                _node = value;
                _node.MarkedForDraggingEvent += Node_MarkedForDraggingEvent;
                _node.UnmarkedForDraggingEvent += Node_UnmarkedForDraggingEvent;
            }
        }
        private IViewerNode _node;

        public bool IsSelected
        {
            get => Node.MarkedForDragging;
            set
            {
                Node.MarkedForDragging = value;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Node_MarkedForDraggingEvent(object sender, EventArgs e)
        {
            Node.Node.Attr.FillColor = Color.RoyalBlue;
            Node.Node.Label.FontColor = Color.White;
            OnPropertyChanged(nameof(IsSelected));
        }

        private void Node_UnmarkedForDraggingEvent(object sender, EventArgs e)
        {
            Node.Node.Label.FontColor = Color.Black;
            Node.Node.Attr.FillColor = Color.White;
            OnPropertyChanged(nameof(IsSelected));
        }
    }
}