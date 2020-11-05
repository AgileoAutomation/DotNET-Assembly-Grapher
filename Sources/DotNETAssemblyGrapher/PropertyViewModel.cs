using DotNETAssemblyGrapherModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNETAssemblyGrapher
{
    public class PropertyViewModel
    {
        private Property model;

        public PropertyViewModel(Property model)
        {
            this.model = model;
        }

        public string Name => model?.name;

        public string Value => model?.value;
    }
}
