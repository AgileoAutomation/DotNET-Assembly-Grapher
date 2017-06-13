using System;
using System.Collections.Generic;

namespace DotNETAssemblyGrapherModel
{
    public class AssemblyPointerGroup
    {
        public List<AssemblyPointer> Pointers { get; internal set; } = new List<AssemblyPointer>();
        public String Name { get; set; }

        public AssemblyPointerGroup(String name, List<AssemblyPointer> assemblies)
        {
            Name = name;
            Pointers.AddRange(assemblies);
        }
    }
}
