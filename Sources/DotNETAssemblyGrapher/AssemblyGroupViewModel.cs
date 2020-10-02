using DotNETAssemblyGrapherModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DotNETAssemblyGrapher
{
    public class AssemblyGroupViewModel
    {
        private AssemblyPointerGroup model;

        public AssemblyGroupViewModel(AssemblyPointerGroup model, HashSet<AssemblyPointerViewModel> assemblies)
        {
            this.model = model;

            AssemblyPointerViewModels = new ObservableCollection<AssemblyPointerViewModel>();
            foreach (AssemblyPointer assembly in model.Assemblies)
            {
                AssemblyPointerViewModels.Add(assemblies.First(a => a.Id == assembly.Id));
            }
        }

        public string Name => model.name;

        public ObservableCollection<AssemblyPointerViewModel> AssemblyPointerViewModels { get; }

        public int Count => AssemblyPointerViewModels.Count;

        public int ErrorsCount => model.ErrorsCount;

        public bool HasErrors => model.HasErrors;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                foreach (AssemblyPointerViewModel assembly in AssemblyPointerViewModels)
                {
                    assembly.Node.MarkedForDragging = _isSelected;
                }
            }
        }
    }
}