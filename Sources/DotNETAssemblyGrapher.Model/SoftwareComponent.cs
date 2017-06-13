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
                return AssemblyPointerGroups.Count != 0;
            }
        }
        public bool HasSubcomponents
        {
            get
            {
                return Subcomponents.Count != 0;
            }
        }

        //////////////////////////////
        /////////CONSTRUCTORS/////////
        //////////////////////////////

        //public SoftwareComponent(string name, List<AssemblyPointer> assemblies)
        //{
        //    if (assemblies == null || string.IsNullOrEmpty(name))
        //        throw new ArgumentNullException();
        //    else if (assemblies.Count == 0)
        //        throw new ArgumentException();

        //    Name = name;
        //    Color = Color.Black;

        //    foreach (AssemblyPointer pointer in assemblies)
        //    {
        //        Pointers.Add(pointer);
        //    }
        //}

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

        //public SoftwareComponent(string name, List<AssemblyPointer> assemblies, List<SoftwareComponent> subcomponents)
        //{
        //    if (assemblies == null || string.IsNullOrEmpty(name) || subcomponents == null)
        //        throw new ArgumentNullException();
        //    else if (assemblies.Count == 0 && subcomponents.Count == 0)
        //        throw new ArgumentException();

        //    Name = name;
        //    Color = Color.Black;

        //    foreach (AssemblyPointer pointer in assemblies)
        //    {
        //        Pointers.Add(pointer);
        //    }

        //    foreach (SoftwareComponent subcomponent in subcomponents)
        //    {
        //        AddSubcomponent(subcomponent);
        //    }
        //}

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

        /////////////////////////
        /////////SETTERS/////////
        /////////////////////////

        public void AddSubcomponent(SoftwareComponent subcomponent)
        {
            Subcomponents.Add(subcomponent);
            Pointers.AddRange(subcomponent.Pointers);
        }

        /////////////////////////
        /////////GETTERS/////////
        /////////////////////////

        public AssemblyPointer FindPointer(string name)
        {
            return Pointers.FirstOrDefault(x => x.GetName().Name == name);
        }

        public AssemblyPointer FindPointerByPrettyName(string prettyname)
        {
            return Pointers.FirstOrDefault(x => x.PrettyName == prettyname);
        }

        public AssemblyPointerGroup FindGroup(string name)
        {
            return AssemblyPointerGroups.FirstOrDefault(x => x.Name == name);
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
