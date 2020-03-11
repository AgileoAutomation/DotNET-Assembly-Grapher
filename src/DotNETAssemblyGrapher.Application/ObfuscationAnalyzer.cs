using System.Collections.Generic;
using System.Linq;
using DotNETAssemblyGrapherModel;

namespace DotNETAssemblyGrapherApplication
{
    public class ObfuscationAnalyzer
    {
        public void Analyze(Model model, List<ObfuscationSpecification> obfuscationspecs)
        {
            List<AssemblyPointer> obfuscatedAssemblies = new List<AssemblyPointer>();
            List<AssemblyPointer> nonObfuscatedAssemblies = new List<AssemblyPointer>();

            foreach (AssemblyPointer pointer in model.AllAssemblies().Where(x => !x.IsSystemAssembly))
            {
                try
                {
                    if (pointer.Assembly.GetManifestResourceNames().Any(x => x.Contains("resources") && !x.Contains(pointer.GetName().Name) && !x.ToLower().Contains("agileo") && !x.ToLower().Contains("system") && !x.ToLower().Contains("microsoft") && !x.ToLower().Contains("windows") && !x.ToLower().Contains("exception")))
                    {
                        pointer.AddProperty("Obfuscated", true);
                        obfuscatedAssemblies.Add(pointer);

                        ObfuscationSpecification spec = obfuscationspecs.FirstOrDefault(x => x.Name == pointer.GetName().Name);
                        if (spec != null && !spec.IsObfuscated)
                            pointer.Errors.Add("This assembly is obfuscated but it should not be obfuscated");
                    }
                    else
                    {
                        pointer.AddProperty("Obfuscated", false);
                        nonObfuscatedAssemblies.Add(pointer);

                        ObfuscationSpecification spec = obfuscationspecs.FirstOrDefault(x => x.Name == pointer.GetName().Name);
                        if (spec != null && spec.IsObfuscated)
                            pointer.Errors.Add("This assembly is not obfuscated but it should be obfuscated");
                    }
                }
                catch
                {

                }
            }

            model.AddAssemblyPointerGroup(" Obfuscated Assemblies", obfuscatedAssemblies);
            model.AddAssemblyPointerGroup(" Non Obfuscated Assemblies", nonObfuscatedAssemblies);
        }
    }
}
