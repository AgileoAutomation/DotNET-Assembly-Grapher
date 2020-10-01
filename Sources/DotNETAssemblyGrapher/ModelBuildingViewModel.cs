using DotNETAssemblyGrapherModel;
using System;
using System.Windows;

namespace DotNETAssemblyGrapher
{
    public class ModelBuildingViewModel : BaseViewModel
    {
        private readonly Model model;
        private readonly GrapherViewModel parent;

        public ModelBuildingViewModel(Model model, GrapherViewModel parent)
        {
            this.model = model;
            this.parent = parent;
            BuildWithAllAssemblies = new UICommand(() => true, BuildModel);
            ExcludeSystemAssemblies = new UICommand
            (
                () => true,
                () =>
                {
                    model.excludeSystemGUIAssemblies = true;
                    BuildModel();
                }
            );
            ExcludeSystemBaseAssemblies = new UICommand
            (
                () => true,
                () =>
                {
                    model.excludeSystemCommonAssemblies = true;
                    BuildModel();
                }
            );
        }

        public UICommand BuildWithAllAssemblies { get; }
        public UICommand ExcludeSystemAssemblies { get; }
        public UICommand ExcludeSystemBaseAssemblies { get; }

        public void BuildModel()
        {
            try
            {
                model.Build();
                model.Classify();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n" + e.StackTrace);
            }

            parent.BuildGraphAndTreeViewModels();
        }
    }
}