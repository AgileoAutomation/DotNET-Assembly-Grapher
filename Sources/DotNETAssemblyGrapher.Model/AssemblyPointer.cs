using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotNETAssemblyGrapherModel
{
    public class AssemblyPointer
    {
        public bool physicalyExists;

        public AssemblyPointer(AssemblyName name)
        {
            AssemblyName = name;
            try
            {
                Assembly = Assembly.Load(AssemblyName);
                physicalyExists = true;
            }
            catch (Exception e)
            {
                // Impossible to load.
                Errors.Add(e.Message);
                physicalyExists = false;
            }
            finally
            {
                AddMainProperties();
            }
        }

        public AssemblyPointer(AssemblyName name, string loadingError)
        {
            AssemblyName = name;
            physicalyExists = false;
            Errors.Add(loadingError);
            AddMainProperties();
        }

        public AssemblyPointer(FileInfo file, string loadingError)
        {
            try
            {
                
            }
            catch (Exception e)
            {
                
            }
            
            physicalyExists = false;
            Properties.Add(new Property("Name", file.Name));
            Properties.Add(new Property("Windows DLL", loadingError));
        }

        public Assembly Assembly { get; private set; }

        public AssemblyName AssemblyName { get; private set; }

        public string Id => HasManifest ? AssemblyName.Name + " " + AssemblyName.Version : FindProperty("Name").value;

        public HashSet<Property> Properties { get; private set; } = new HashSet<Property>();

        public bool HasManifest => AssemblyName != null;

        public bool IsSystemAssembly => physicalyExists && Assembly.GlobalAssemblyCache
            && AssemblyName.Flags == AssemblyNameFlags.None
            && AssemblyName.HashAlgorithm == AssemblyHashAlgorithm.None
            && AssemblyName.ProcessorArchitecture == ProcessorArchitecture.None;

        public HashSet<string> Errors { get; private set; } = new HashSet<string>();
        public bool HasErrors => Errors.Count > 0;

        private void AddMainProperties()
        {
            AddProperty("Name", AssemblyName.Name);
            AddProperty("Version", AssemblyName.Version);
            AddProperty("Culture", AssemblyName.CultureName);

            if (physicalyExists)
            {
                AddProperty("Location", Assembly.Location);
                AddProperty("Is in the GAC", Assembly.GlobalAssemblyCache);

                if (Assembly.EntryPoint != null)
                    Properties.Add(new Property("Entry Point", Assembly.EntryPoint));
            }
        }

        public ReadOnlyCollection<AssemblyName> GetReferencedAssemblies()
        {
            if (Assembly == null)
                return new List<AssemblyName>().AsReadOnly();

            return Assembly.GetReferencedAssemblies().ToList().AsReadOnly();
        }

        public Property FindProperty(string propertyName)
        {
            return Properties.FirstOrDefault(x => x.name == propertyName);
        }

        public void AddProperty(string name, object value)
        {
            Properties.Add(new Property(name, value));
        }
    }
}
