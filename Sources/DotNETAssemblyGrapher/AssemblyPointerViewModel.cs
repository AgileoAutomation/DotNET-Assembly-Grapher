using DotNETAssemblyGrapherModel;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.ObjectModel;

namespace DotNETAssemblyGrapher
{
    public class AssemblyPointerViewModel
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

        public IViewerNode Node { get; internal set; }
        public bool IsSelected
        {
            get => Node.MarkedForDragging;
            internal set
            {
                Node.MarkedForDragging = value;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}