using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DotNETAssemblyGrapherModel
{
    public class AssemblyPointerGroup
    {
        public string name;
        private List<AssemblyPointer> assemblies = new List<AssemblyPointer>();

        public ReadOnlyCollection<AssemblyPointer> Assemblies
        {
            get { return assemblies.AsReadOnly() ; }
        }

        public AssemblyPointerGroup(string name, IEnumerable<AssemblyPointer> assemblies)
        {
            this.name = name;
            this.assemblies.AddRange(assemblies);
        }
    }
}
