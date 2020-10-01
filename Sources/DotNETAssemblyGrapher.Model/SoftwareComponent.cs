using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace DotNETAssemblyGrapherModel
{
    public class SoftwareComponent : AssemblyPointerGroupContainer
    {
        public string Name { get; set; }
        private List<AssemblyPointer> Pointers { get; } = new List<AssemblyPointer>();
        public Color Color { get; set; }
        public List<SoftwareComponent> Subcomponents { get; } = new List<SoftwareComponent>();

        public bool HasGroups
        {
            get
            {
                return Groups.Count != 0;
            }
        }
        public bool HasSubcomponents
        {
            get
            {
                return Subcomponents.Count != 0;
            }
        }

        public IEnumerable<AssemblyPointer> AllAssemblies
        {
            get
            {
                HashSet<AssemblyPointer> result = new HashSet<AssemblyPointer>(Pointers);
                foreach (SoftwareComponent component in Subcomponents)
                {
                    result.Concat(component.Pointers);
                }
                return result;
            }
        }

        //////////////////////////////
        /////////CONSTRUCTORS/////////
        //////////////////////////////

        public SoftwareComponent(string name, IEnumerable<AssemblyPointer> assemblies, Color color)
        {
            if (assemblies == null || string.IsNullOrEmpty(name))
                throw new ArgumentNullException();
            else if (assemblies.Count() == 0)
                throw new ArgumentException();

            Name = name;
            Color = color;

            foreach (AssemblyPointer pointer in assemblies)
            {
                Pointers.Add(pointer);
            }
        }

        public SoftwareComponent(string name, List<AssemblyPointer> assemblies, Color color, List<SoftwareComponent> subcomponents)
        {
            if (assemblies == null || string.IsNullOrEmpty(name) || subcomponents == null)
                throw new ArgumentNullException();
            else if (assemblies.Count == 0 && subcomponents.Count == 0)
                throw new ArgumentException();

            Name = name;
            Color = color;

            foreach (AssemblyPointer pointer in assemblies)
            {
                Pointers.Add(pointer);
            }

            foreach (SoftwareComponent subcomponent in subcomponents)
            {
                AddSubcomponent(subcomponent);
            }
        }


        public void AddSubcomponent(SoftwareComponent subcomponent)
        {
            Subcomponents.Add(subcomponent);
            Pointers.AddRange(subcomponent.Pointers);
        }

        public AssemblyPointer FindPointer(string name)
        {
            return Pointers.FirstOrDefault(x => x.AssemblyName.Name == name);
        }

        public AssemblyPointer FindAssemblyById(string prettyname)
        {
            return Pointers.FirstOrDefault(x => x.Id == prettyname);
        }

        public AssemblyPointerGroup FindGroup(string name)
        {
            return Groups.FirstOrDefault(x => x.name == name);
        }

        public SoftwareComponent FindSubcomponent(string name)
        {
            SoftwareComponent result = Subcomponents.FirstOrDefault(x => x.Name == name);

            if (result == null)
            {
                foreach (SoftwareComponent subcomponent in Subcomponents)
                {
                    result = subcomponent.FindSubcomponent(name);
                    if (result != null)
                        return result;
                }
                return null;
            }
            else
                return result;
        }
    }
}
