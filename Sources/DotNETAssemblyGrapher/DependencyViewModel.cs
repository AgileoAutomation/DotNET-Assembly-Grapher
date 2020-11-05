using DotNETAssemblyGrapherModel;
using Microsoft.Msagl.Drawing;

namespace DotNETAssemblyGrapher
{
    public class DependencyViewModel : BaseViewModel
    {
        private readonly Dependency model;

        public DependencyViewModel(Dependency model)
        {
            this.model = model;

            From = AssemblyPointerViewModel.Create(model.from);
            To = AssemblyPointerViewModel.Create(model.to);
        }

        public AssemblyPointerViewModel From { get; }
        public AssemblyPointerViewModel To { get; }
    }
}