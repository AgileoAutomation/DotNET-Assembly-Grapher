using System;
using System.Collections.Generic;
using System.Linq;
using DotNETAssemblyGrapherModel;

namespace DotNETAssemblyGrapherApplication
{
    public class SignatureAnalyzer
    {
        public void Analyze(Model model, List<SignatureSpecification> sigspecs)
        {
            List<AssemblyPointer> signedAssemblies = new List<AssemblyPointer>();
            List<AssemblyPointer> unsignedAssemblies = new List<AssemblyPointer>();

            foreach (AssemblyPointer pointer in model.AllAssemblies.Where(x => !x.IsSystemAssembly))
            {
                if (pointer.physicalyExists)
                {
                    if (pointer.AssemblyName.GetPublicKeyToken().Count() != 0)
                    {
                        pointer.AddProperty("Signed", true);
                        pointer.AddProperty("PublicKeyToken", BitConverter.ToString(pointer.AssemblyName.GetPublicKeyToken()));
                        signedAssemblies.Add(pointer);

                        SignatureSpecification spec = sigspecs.FirstOrDefault(x => x.Name == pointer.AssemblyName.Name);
                        if (spec != null && !spec.IsSigned)
                            pointer.Errors.Add("This assembly is signed but it should not be signed");
                    }
                    else
                    {
                        pointer.AddProperty("Signed", false);
                        pointer.AddProperty("PublicKeyToken", BitConverter.ToString(pointer.AssemblyName.GetPublicKeyToken()));
                        unsignedAssemblies.Add(pointer);

                        SignatureSpecification spec = sigspecs.FirstOrDefault(x => x.Name == pointer.AssemblyName.Name);
                        if (spec != null && spec.IsSigned)
                            pointer.Errors.Add("This assembly is not signed but it should be signed");
                    }
                }
            }

            model.AddGroup("Signed Assemblies", signedAssemblies);
            model.AddGroup("Unsigned Assemblies", unsignedAssemblies);
        }
    }
}
