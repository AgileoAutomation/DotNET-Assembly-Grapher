using DotNETAssemblyGrapherModel;
using System.Collections.Generic;

namespace DotNETAssemblyGrapherApplication
{
    public static class ExtendedGetters
    {
        public static List<AssemblyPointer> AllAssemblies(this Model model)
        {
            return model.FindGroup("All Assemblies").Pointers;
        }

        public static List<AssemblyPointer> MissingAssemblies(this Model model)
        {
            return model.FindGroup("Missing Assemblies").Pointers;
        }

        public static List<AssemblyPointer> NonReferencedAssemblies(this Model model)
        {
            return model.FindGroup("Non referenced Assemblies").Pointers;
        }

        public static List<AssemblyPointer> SimilarAssemblies(this Model model)
        {
            return model.FindGroup("Similar Assemblies").Pointers;
        }
    }

    public static class SoftwareComponentGetters
    {
        public static List<AssemblyPointer> Assemblies(this SoftwareComponent component)
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
