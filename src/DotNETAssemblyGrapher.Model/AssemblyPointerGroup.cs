using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DotNETAssemblyGrapherModel
{
    public class AssemblyPointerGroup
    {

        private List<AssemblyPointer> pointers = new List<AssemblyPointer>();

        public ReadOnlyCollection<AssemblyPointer> Pointers
        {
            get { return pointers.AsReadOnly() ; }
        }

        public string Name { get; }

        public AssemblyPointerGroup(string name, IEnumerable<AssemblyPointer> assemblies)
        {
            Name = name;
            pointers.AddRange(assemblies);
        }
    }
}
