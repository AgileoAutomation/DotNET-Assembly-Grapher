using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Drawing;

namespace DotNETAssemblyGrapherModel
{
    public class Model : AssemblyPointerGroupContainer
    {
        private readonly DirectoryInfo directory;

        public Model(DirectoryInfo directory)
        {
            this.directory = directory;
        }

        #region Properties

        private ReadOnlyCollection<Regex> SystemCommonAssemblies
        {
            get
            {
                return new List<Regex>()
                {
                    new Regex("mscorlib"),
                    new Regex("netstandard"),
                    new Regex("System"),
                    new Regex("System.Core"),
                    new Regex("System.Data"),
                    new Regex("System.Data.DataSetExtension"),
                    new Regex("System.Net.Http"),
                    new Regex("System.Xml"),
                    new Regex("System.Xml.Linq")
                }.AsReadOnly();
            }
        }
        public bool excludeSystemCommonAssemblies = false;

        private ReadOnlyCollection<Regex> SystemGUIAssemblies
        {
            get
            {
                return new List<Regex>(SystemCommonAssemblies)
                {
                    new Regex("PresentationCore"),
                    new Regex("PresentationFramework"),
                    new Regex("WindowsBase"),
                    new Regex("System.Deployment"),
                    new Regex("System.Drawing"),
                    new Regex("System.Windows.Forms")
                }.AsReadOnly();
            }
        }
        public bool excludeSystemGUIAssemblies = false;

        public HashSet<AssemblyPointer> Inputs { get; set; } = new HashSet<AssemblyPointer>();

        public List<Dependency> Dependencies { get; } = new List<Dependency>();

        public HashSet<AssemblyPointer> AllAssemblies
        {
            get
            {
                HashSet<AssemblyPointer> result = new HashSet<AssemblyPointer>(Inputs);
                foreach (Dependency dependency in Dependencies)
                {
                    result.Add(dependency.from);
                    result.Add(dependency.to);
                }
                return result;
            }
        }

        public List<SoftwareComponent> SoftwareComponents { get; } = new List<SoftwareComponent>();

        #endregion

        public void Build()
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                if (file.Extension == ".exe" || file.Extension == ".dll")
                {
                    try
                    {
                        AssemblyName name = AssemblyName.GetAssemblyName(file.FullName);
                        Inputs.Add(new AssemblyPointer(name));
                    }
                    catch (Exception e)
                    {
                        Inputs.Add(new AssemblyPointer(file.Name.Replace(file.Extension, ""), e.Message));
                    }
                }
            }
            foreach (AssemblyPointer pointer in Inputs)
            {
                LoadAllReferencedAssemblies(pointer);
            }

            AddGroup("Model Inputs", (IEnumerable<AssemblyPointer>)Inputs);
            AddGroup("All Assemblies", (IEnumerable<AssemblyPointer>)AllAssemblies);
        }

        private HashSet<AssemblyPointer> loadedAssemblies = new HashSet<AssemblyPointer>();
        private void LoadAllReferencedAssemblies(AssemblyPointer from)
        {
            // Already loaded ?
            if (loadedAssemblies.Contains(from))
            {
                return;
            }
            loadedAssemblies.Add(from);

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
                AssemblyPointer to = FindAssemblyByName(name);
                if (to.physicalyExists)
                    LoadAllReferencedAssemblies(to);
            }
        }

        #region Classification

        public void Classify()
        {
            HashSet<AssemblyPointer> referencedAssemblies = new HashSet<AssemblyPointer>();
            foreach (Dependency dependency in Dependencies)
            {
                referencedAssemblies.Add(dependency.to);
            }

            HashSet<AssemblyPointer> systemAssemblies = new HashSet<AssemblyPointer>();
            HashSet<AssemblyPointer> nonReferencedAssemblies = new HashSet<AssemblyPointer>();
            HashSet<AssemblyPointer> missingAssemblies = new HashSet<AssemblyPointer>();
            HashSet<AssemblyPointer> nonDotNetAssemblies = new HashSet<AssemblyPointer>();

            foreach (AssemblyPointer assembly in AllAssemblies)
            {
                if (assembly.IsSystemAssembly)
                {
                    assembly.AddProperty("Component", "System Assemblies");
                    systemAssemblies.Add(assembly);
                }

                if (!referencedAssemblies.Contains(assembly))
                    nonReferencedAssemblies.Add(assembly);
                else if (!assembly.physicalyExists)
                    missingAssemblies.Add(assembly);

                if (!assembly.HasManifest)
                    nonDotNetAssemblies.Add(assembly);
            }

            AddGroup("Referenced Assemblies", referencedAssemblies);
            AddGroup("Non Referenced Assemblies", nonReferencedAssemblies);
            AddGroup("Missing Assemblies", missingAssemblies);
            AddGroup("Whitout Manifest Assemblies", nonDotNetAssemblies);
            AddGroup("Version Conflicts", FindVersionConflicts());

            if (systemAssemblies.Count != 0)
            {
                SoftwareComponent SystemAssemblies = new SoftwareComponent("System Assemblies", systemAssemblies, Color.Blue);
                SystemAssemblies.AddGroup("Assemblies", systemAssemblies);
                AddGroup("System Assemblies", systemAssemblies);
            }
        }

        private HashSet<AssemblyPointer> FindVersionConflicts()
        {
            HashSet<AssemblyPointer> versionConfilcts = new HashSet<AssemblyPointer>();
            HashSet<AssemblyPointer> all = new HashSet<AssemblyPointer>(AllAssemblies.Except(FindGroupByName("Whitout Manifest Assemblies").Assemblies));

            foreach (AssemblyPointer assembly in all)
            {
                if (all.Any(
                        p => p.AssemblyName.Name.Equals(assembly.AssemblyName.Name)
                        && !assembly.AssemblyName.Version.Equals(p.AssemblyName.Version)))
                {
                    versionConfilcts.Add(assembly);
                    assembly.Errors.Add("The model contains other version of this assembly");
                }
            }

            return versionConfilcts;
        }

        #endregion

        private IEnumerable<AssemblyName> GetFilteredReferencedAssemblies(AssemblyPointer from)
        {
            if (excludeSystemGUIAssemblies)
                return from.GetReferencedAssemblies().Where(x => !SystemGUIAssemblies.Any(y => y.Match(x.Name).Success));
            else if (excludeSystemCommonAssemblies)
                return from.GetReferencedAssemblies().Where(x => !SystemCommonAssemblies.Any(y => y.Match(x.Name).Success));

            return from.GetReferencedAssemblies();
        }



        #region Accessors

        private void AddGroup(string groupName, HashSet<AssemblyPointer> inputs)
        {
            Groups.Add(new AssemblyPointerGroup(groupName, inputs));
        }

        public Dependency FindDependency(AssemblyPointer from, AssemblyPointer to)
        {
            return Dependencies.FirstOrDefault(x => x.from.AssemblyName.Equals(from.AssemblyName)
                                                && x.to.AssemblyName.Equals(to.AssemblyName));
        }

        public AssemblyPointer FindAssemblyByName(AssemblyName name)
        {
            return AllAssemblies.Where(a => a.AssemblyName != null).FirstOrDefault(x => x.AssemblyName.FullName == name.FullName);
        }

        public AssemblyPointer FindAssemblyByFileName(string filename)
        {
            return AllAssemblies.FirstOrDefault(x => x.FindProperty("Name").value == filename);
        }

        public AssemblyPointer FindAssemblyById(string id)
        {
            return AllAssemblies.FirstOrDefault(x => x.Id == id);
        }

        public AssemblyPointerGroup FindGroupByName(string name)
        {
            return Groups.FirstOrDefault(x => x.name == name);
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

        #endregion
    }
}
