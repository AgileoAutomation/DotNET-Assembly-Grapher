using DotNETAssemblyGrapherModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DotNETAssemblyGrapherApplication
{
    public static class ExtendedGetters
    {
        public static ReadOnlyCollection<AssemblyPointer> AllAssemblies(this Model model)
        {
            return model.FindGroupByName("All Assemblies").Pointers;
        }

        public static ReadOnlyCollection<AssemblyPointer> MissingAssemblies(this Model model)
        {
            return model.FindGroupByName("Missing Assemblies").Pointers;
        }

        public static ReadOnlyCollection<AssemblyPointer> NonReferencedAssemblies(this Model model)
        {
            return model.FindGroupByName("Non referenced Assemblies").Pointers;
        }
    }

    public static class SoftwareComponentGetters
    {
        public static ReadOnlyCollection<AssemblyPointer> Assemblies(this SoftwareComponent component)
        {
            return component.FindGroup("Assemblies").Pointers;
        }
    }

    public static class AssemblyPointerGetters
    {
        public static string Component(this AssemblyPointer pointer)
        {
            if (pointer.FindProperty("Component") != null)
                return (string)pointer.FindProperty("Component").Value;
            else
                return null;
        }
    }
}
