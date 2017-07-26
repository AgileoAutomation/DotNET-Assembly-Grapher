using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace DotNETAssemblyGrapherModel
{
    public class Model : AssemblyPointerGroupContainer
    {
        ////////////////////////
        ///////PROPERTIES///////
        ////////////////////////

        private List<Regex> filterRegexList = new List<Regex>();
        public void AddRegex(string regex)
        {
            filterRegexList.Add(new Regex(regex));
        }


        private HashSet<AssemblyPointer> inputs = new HashSet<AssemblyPointer>();

        private HashSet<AssemblyPointer> AllAssemblies
        {
            get
            {
                HashSet<AssemblyPointer> result = new HashSet<AssemblyPointer>(inputs);
                foreach (Dependency dependency in Dependencies)
                {
                    result.Add(dependency.From);
                    result.Add(dependency.To);
                }
                return result;
            }
        }

        public List<Dependency> Dependencies { get; } = new List<Dependency>();

        public List<SoftwareComponent> SoftwareComponents { get; } = new List<SoftwareComponent>();

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
                    try
                    {
                        inputs.Add(new AssemblyPointer(Assembly.LoadFrom(file.FullName)));
                    }
                    catch (BadImageFormatException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            foreach (AssemblyPointer pointer in inputs)
            {
                LoadAllReferencedAssemblies(pointer);
            }

            AddAssemblyPointerGroup("Model Inputs", inputs);
            AddAssemblyPointerGroup("All Assemblies", AllAssemblies);
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
            foreach (AssemblyName name in GetFilteredReferencedAssemblies(from))
            {
                AssemblyPointer to = FindAssemblyByName(name);
                if (to == null)
                    to = new AssemblyPointer(name);

                if (FindDependency(from, to) == null)
                {
                    Dependency dependency = new Dependency(from, to);
                    Dependencies.Add(dependency);
                }
            }

            foreach (AssemblyName name in GetFilteredReferencedAssemblies(from))
            {
                AssemblyPointer to = GetAssemblyByName(name);
                if (to.PhysicalyExists)
                    LoadAllReferencedAssemblies(to);
            }
        }

        private IEnumerable<AssemblyName> GetFilteredReferencedAssemblies(AssemblyPointer from)
        {
            return from
                .GetReferencedAssemblies()
                .Where(x => !filterRegexList
                                .Any(y => y.Match(x.Name).Success));
        }

        /////////////////////////////
        ///////PRIVATE GETTERS///////
        /////////////////////////////

        private Dependency FindDependency(AssemblyPointer from, AssemblyPointer to)
        {
            return Dependencies.FirstOrDefault(x => x.From.GetName().Equals(from.GetName())
                                                    && x.To.GetName().Equals(to.GetName()));
        }

        private AssemblyPointer FindAssemblyByName(AssemblyName name)
        {
            return AllAssemblies.FirstOrDefault(x => x.GetName().FullName == name.FullName);
        }

        private AssemblyPointer GetAssemblyByName(AssemblyName name)
        {
            AssemblyPointer pointer = FindAssemblyByName(name);
            if (pointer == null)
                throw new InvalidOperationException();
            return pointer;
        }

        ////////////////////////////
        ///////PUBLIC GETTERS///////
        ////////////////////////////

        public AssemblyPointer FindPointerByName(string name)
        {
            return AllAssemblies.FirstOrDefault(x => x.GetName().Name == name);
        }

        public AssemblyPointer FindPointerByPrettyName(string prettyName)
        {
            return AllAssemblies.FirstOrDefault(x => x.PrettyName == prettyName);
        }

        public AssemblyPointerGroup FindGroupByName(string name)
        {
            return AssemblyPointerGroups.FirstOrDefault(x => x.Name == name);
        }

        public SoftwareComponent FindComponentByName(string name)
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
