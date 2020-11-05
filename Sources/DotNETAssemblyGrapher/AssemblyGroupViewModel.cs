using DotNETAssemblyGrapherModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotNETAssemblyGrapher
{
    public class AssemblyGroupViewModel : BaseViewModel
    {
        private AssemblyPointerGroup model;

        public AssemblyGroupViewModel(AssemblyPointerGroup model)
        {
            this.model = model;

            AssemblyPointerViewModels = new ObservableCollection<AssemblyPointerViewModel>();
            foreach (AssemblyPointer assembly in model.Assemblies)
            {
                AssemblyPointerViewModel avm = AssemblyPointerViewModel.Get(assembly);
                if (avm != null)
                {
                    AssemblyPointerViewModels.Add(avm);
                    avm.Node.MarkedForDraggingEvent += Node_MarkedForDraggingEvent;
                }
            }
        }

        public string Name => model.name;

        public ObservableCollection<AssemblyPointerViewModel> AssemblyPointerViewModels { get; }

        public int Count => AssemblyPointerViewModels.Count;

        public int ErrorsCount => model.ErrorsCount;

        public bool HasErrors => model.HasErrors;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    foreach (AssemblyPointerViewModel assembly in AssemblyPointerViewModels)
                    {
                        assembly.IsSelected = _isSelected;
                    }
                    foreach (AssemblyPointerViewModel assembly in AssemblyPointerViewModels)
                    {
                        assembly.Node.UnmarkedForDraggingEvent += Node_UnmarkedForDraggingEvent;
                    }
                }

                OnPropertyChanged(nameof(IsSelected));
            }
        }
        private bool _isSelected;

        private void Node_UnmarkedForDraggingEvent(object sender, EventArgs e)
        {
            IsSelected = false;
            foreach (AssemblyPointerViewModel assembly in AssemblyPointerViewModels)
            {
                assembly.Node.UnmarkedForDraggingEvent -= Node_UnmarkedForDraggingEvent;
                assembly.Node.MarkedForDraggingEvent += Node_MarkedForDraggingEvent;
            }
        }

        private void Node_MarkedForDraggingEvent(object sender, EventArgs e)
        {
            foreach (AssemblyPointerViewModel assembly in AssemblyPointerViewModels)
            {
                assembly.Node.MarkedForDraggingEvent -= Node_MarkedForDraggingEvent;
            }
            IsExpanded = true;
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }
        private bool isExpanded;
    }
}