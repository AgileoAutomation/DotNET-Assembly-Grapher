using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace DotNETAssemblyGrapherModel
{
    public class Model
    {
        ////////////////////////
        ///////PROPERTIES///////
        ////////////////////////

        public List<Regex> regexList = new List<Regex>();
        private HashSet<AssemblyPointer> Inputs { get; } = new HashSet<AssemblyPointer>();
        private HashSet<AssemblyPointer> AllAssemblies
        {
            get
            {
                HashSet<AssemblyPointer> result = new HashSet<AssemblyPointer>(Inputs);
                foreach (Dependency dependency in Dependencies)
                {
                    result.Add(dependency.From);
                    result.Add(dependency.To);
                }
                return result;
            }
        }

        public List<Dependency> Dependencies { get; internal set; } = new List<Dependency>();

        public List<AssemblyPointerGroup> AssemblyPointerGroups { get; internal set; } = new List<AssemblyPointerGroup>();

        public List<SoftwareComponent> SoftwareComponents { get; internal set; } = new List<SoftwareComponent>();

        /////////////////////////////
        /////////BUILD MODEL/////////
        /////////////////////////////

        public Model() { }

        private List<AssemblyPointer> listForBuild = new List<AssemblyPointer>();

        public void Build(DirectoryInfo dir)
        {

            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.Extension == ".exe" || file.Extension == ".dll")
                {
                    Inputs.Add(new AssemblyPointer(Assembly.LoadFrom(file.FullName)));
                }
            }
            foreach (AssemblyPointer pointer in Inputs)
            {
                LoadAllReferencedAssemblies(pointer);
            }

            AssemblyPointerGroups.Add(new AssemblyPointerGroup("Model Inputs", Inputs.ToList()));
            AssemblyPointerGroups.Add(new AssemblyPointerGroup("All Assemblies", AllAssemblies.ToList()));
        }

        private void LoadAllReferencedAssemblies(AssemblyPointer from)
        {
            // Already loaded ?
            if (listForBuild.Contains(from))
            {
                return;
            }
            listForBuild.Add(from);

            // Load and Build AssemblyPointers and Dependencies
            foreach (AssemblyName name in from.GetReferencedAssemblies().Where(x => !regexList.Any(y => y.Match(x.Name).Success)))
            {
                AssemblyPointer to = FindAssembly(name);
                if (to == null)
                    to = new AssemblyPointer(name);

                if (FindDependency(from, to) == null)
                {
                    Dependency dependency = new Dependency(from, to);
                    Dependencies.Add(dependency);
                }
            }

            foreach (AssemblyName name in from.GetReferencedAssemblies().Where(x => !regexList.Any(y => y.Match(x.Name).Success)))
            {
                AssemblyPointer to = GetAssembly(name);
                if (to.PhysicalyExists)
                    LoadAllReferencedAssemblies(to);
            }
        }

        /////////////////////////////
        ///////PRIVATE GETTERS///////
        /////////////////////////////

        private Dependency FindDependency(AssemblyPointer from, AssemblyPointer to)
        {
            return Dependencies.FirstOrDefault(x => x.From.GetName().Equals(from.GetName())
                                                    && x.To.GetName().Equals(to.GetName()));
        }

        private AssemblyPointer FindAssembly(AssemblyName name)
        {
            return AllAssemblies.FirstOrDefault(x => x.GetName().FullName == name.FullName);
        }

        private AssemblyPointer GetAssembly(AssemblyName name)
        {
            AssemblyPointer pointer = FindAssembly(name);
            if (pointer == null)
                throw new InvalidOperationException();
            return pointer;
        }

        ////////////////////////////
        ///////PUBLIC GETTERS///////
        ////////////////////////////

        public AssemblyPointer FindPointer(string name)
        {
            return AllAssemblies.FirstOrDefault(x => x.GetName().Name == name);
        }

        public AssemblyPointer FindPointerByPrettyName(string prettyname)
        {
            return AllAssemblies.FirstOrDefault(x => x.PrettyName == prettyname);
        }

        public AssemblyPointerGroup FindGroup(string name)
        {
            return AssemblyPointerGroups.FirstOrDefault(x => x.Name == name);
        }

        public SoftwareComponent FindComponent(string name)
        {
            SoftwareComponent result = SoftwareComponents.FirstOrDefault(x => x.Name == name);

            if (result == null)
            {
                foreach (SoftwareComponent component in SoftwareComponents)
                {
                    result = component.FindSubcomponent(name);
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
