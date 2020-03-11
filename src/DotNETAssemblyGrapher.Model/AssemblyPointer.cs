using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace DotNETAssemblyGrapherModel
{
    public class AssemblyPointer
    {
        private AssemblyName name;
        public bool HasErrors
        {
            get
            {
                return Errors.Count != 0;
            }
        }
        public Assembly Assembly { get; private set; }
        public List<Property> Properties { get; private set; } = new List<Property>();
        public List<string> Errors { get; private set; } = new List<string>();

        ///////////////////
        ///////BUILD///////
        ///////////////////

        public AssemblyPointer(AssemblyName name)
        {
            this.name = name;
            try
            {
                Load();
            }
            catch (Exception)
            {
                // Impossible to load, the assembly does not exist.
                Errors.Add("This assembly is referenced but physically missing");
            }
            finally
            {
                BuildProperties();
            }
        }

        public AssemblyPointer(Assembly assembly)
        {
            name = assembly.GetName();
            Assembly = assembly;
            BuildProperties();
        }

        private void Load()
        {
            //Assembly = Assembly.ReflectionOnlyLoad(name.FullName);
            Assembly = Assembly.Load(name);
        }

        private void BuildProperties()
        {
            AddProperty("Name", GetName().Name);
            AddProperty("Version", GetName().Version);
            AddProperty("Culture", GetName().CultureName);

            if (PhysicalyExists)
            {
                AddProperty("Location", Assembly.Location);
                AddProperty("Entry Point", Assembly.EntryPoint);
                AddProperty("Is in the GAC", Assembly.GlobalAssemblyCache);
            }
        }

        public void AddProperty(string name, object value)
        {
            Property property = new Property(name, value);
            if (FindProperty(name) != null)
            {
                throw new InvalidOperationException($"Property {name} already exists");
            }
            Properties.Add(property);
        }

        public AssemblyName GetName()
        {
            return name;
        }

        public string PrettyName
        {
            get
            {
                return GetName().Name + " " + GetName().Version;
            }
        }

        public ReadOnlyCollection<AssemblyName> GetReferencedAssemblies()
        {
            return Assembly?.GetReferencedAssemblies().ToList().AsReadOnly();
        }

        public bool PhysicalyExists
        {
            get
            {
                return Assembly != null;
            }
        }

        public bool IsSystemAssembly
        {
            get
            {
                if (PhysicalyExists)
                    return Assembly.Location.ToLower().Contains("microsoft.net");
                else
                    return false;
            }
        }

        public Property FindProperty(string propertyName)
        {
            return Properties.FirstOrDefault(x => x.Name == propertyName);
        }
    }
}
