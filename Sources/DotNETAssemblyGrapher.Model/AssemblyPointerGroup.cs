using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DotNETAssemblyGrapherModel
{
    public class AssemblyPointerGroup
    {
        public string name;
        private List<AssemblyPointer> assemblies = new List<AssemblyPointer>();

        public AssemblyPointerGroup(string name, IEnumerable<AssemblyPointer> assemblies)
        {
            this.name = name;
            this.assemblies.AddRange(assemblies);
        }

        public ReadOnlyCollection<AssemblyPointer> Assemblies => assemblies.AsReadOnly();

        public int ErrorsCount
        {
            get
            {
                int count = 0;
                foreach (AssemblyPointer assembly in Assemblies)
                {
                    count = assembly.Errors.Count;
                }
                return count;
            }
        }

        public bool HasErrors => ErrorsCount > 0;
    }
}
