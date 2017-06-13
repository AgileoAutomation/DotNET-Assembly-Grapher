using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNETAssemblyGrapherModel
{
    public class AssemblyPointerGroupContainer
    {

        private List<AssemblyPointerGroup> assemblyPointerGroups = new List<AssemblyPointerGroup>();

        public ReadOnlyCollection<AssemblyPointerGroup> AssemblyPointerGroups { get { return assemblyPointerGroups.AsReadOnly(); } }

        public void AddAssemblyPointerGroup(string groupName, IEnumerable<AssemblyPointer> assemblyPointers)
        {
            assemblyPointerGroups.Add(new AssemblyPointerGroup(groupName, assemblyPointers));
        }

    }
}
