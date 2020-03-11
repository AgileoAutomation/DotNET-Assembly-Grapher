using System.Collections.Generic;

namespace DotNETAssemblyGrapherModel
{
    public class ModelCommonDataOrganizer
    {

        /////////////////////////////////
        /////////ANALYZE METHODS/////////
        /////////////////////////////////

        public static void Organize(Model model)
        {
            List<AssemblyPointer> systemAssemblies = new List<AssemblyPointer>();
            List<AssemblyPointer> referencedAssemblies = new List<AssemblyPointer>();
            List<AssemblyPointer> nonReferencedAssemblies = new List<AssemblyPointer>();
            List<AssemblyPointer> missingAssemblies = new List<AssemblyPointer>();
            List<AssemblyPointer> versionConfilcts = new List<AssemblyPointer>();

            foreach (Dependency dependency in model.Dependencies)
            {
                if (!referencedAssemblies.Contains(dependency.To))
                    referencedAssemblies.Add(dependency.To);
            }

            foreach (AssemblyPointer pointer in model.FindGroupByName("All Assemblies").Pointers)
            {
                if (pointer.IsSystemAssembly)
                {
                    pointer.AddProperty("Component", "System Assemblies");
                    systemAssemblies.Add(pointer);
                }

                if (!referencedAssemblies.Contains(pointer))
                {
                    nonReferencedAssemblies.Add(pointer);
                }
                else if (!pointer.PhysicalyExists
                    && !missingAssemblies.Contains(pointer))
                {
                    missingAssemblies.Add(pointer);
                }

                if (!versionConfilcts.Contains(pointer))
                    versionConfilcts.AddRange(FindVersionConflicts(pointer, model));
            }

            if (systemAssemblies.Count != 0)
            {
                SoftwareComponent SystemAssemblies = new SoftwareComponent("System Assemblies", systemAssemblies, System.Drawing.Color.Blue);
                SystemAssemblies.AddAssemblyPointerGroup("Assemblies", systemAssemblies);
                model.SoftwareComponents.Add(SystemAssemblies);
            }

            model.AddAssemblyPointerGroup("Referenced Assemblies", referencedAssemblies);
            model.AddAssemblyPointerGroup("Non Referenced Assemblies", nonReferencedAssemblies);
            model.AddAssemblyPointerGroup("Missing Assemblies", missingAssemblies);
            model.AddAssemblyPointerGroup("Version Conflicts", versionConfilcts);
        }

        private static List<AssemblyPointer> FindVersionConflicts(AssemblyPointer pointer, Model model)
        {
            List<AssemblyPointer> versionConfilcts = new List<AssemblyPointer>();

            foreach (AssemblyPointer similar in model.FindGroupByName("All Assemblies").Pointers)
            {
                if (pointer.GetName().Name.Equals(similar.GetName().Name)
                    && !pointer.GetName().Version.Equals(similar.GetName().Version))
                {
                    versionConfilcts.Add(similar);
                    versionConfilcts.Add(pointer);

                    pointer.Errors.Add("The model contains other version of this assembly");
                    similar.Errors.Add("The model contains other version of this assembly");
                }
            }

            return versionConfilcts;
        }

        public static void UpdateGroups(Model model, SoftwareComponent component)
        {
            foreach (AssemblyPointerGroup group in model.AssemblyPointerGroups)
            {
                List<AssemblyPointer> pointers = new List<AssemblyPointer>();
                foreach (AssemblyPointer pointer in group.Pointers)
                {
                    if (component.FindPointerByPrettyName(pointer.PrettyName) != null)
                    {
                        pointers.Add(pointer);
                    }
                }

                if (group.Name != "All Assemblies")
                    component.AddAssemblyPointerGroup(group.Name, pointers);
                else
                    component.AddAssemblyPointerGroup("Assemblies", pointers);
            }

            foreach (SoftwareComponent subcomponent in component.Subcomponents)
            {
                UpdateGroups(model, subcomponent);
            }
        }
    }
}
