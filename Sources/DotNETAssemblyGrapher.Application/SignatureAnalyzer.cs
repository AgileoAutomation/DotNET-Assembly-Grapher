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

            foreach (AssemblyPointer pointer in model.AllAssemblies().Where(x => !x.IsSystemAssembly))
            {
                if (pointer.PhysicalyExists)
                {
                    if (pointer.GetName().GetPublicKeyToken().Count() != 0)
                    {
                        pointer.AddProperty("Signed", true);
                        pointer.AddProperty("PublicKeyToken", BitConverter.ToString(pointer.GetName().GetPublicKeyToken()));
                        signedAssemblies.Add(pointer);

                        SignatureSpecification spec = sigspecs.FirstOrDefault(x => x.Name == pointer.GetName().Name);
                        if (spec != null && !spec.IsSigned)
                            pointer.Errors.Add("This assembly is signed but it should not be signed");
                    }
                    else
                    {
                        pointer.AddProperty("Signed", false);
                        pointer.AddProperty("PublicKeyToken", BitConverter.ToString(pointer.GetName().GetPublicKeyToken()));
                        unsignedAssemblies.Add(pointer);

                        SignatureSpecification spec = sigspecs.FirstOrDefault(x => x.Name == pointer.GetName().Name);
                        if (spec != null && spec.IsSigned)
                            pointer.Errors.Add("This assembly is not signed but it should be signed");
                    }
                }
            }

            model.AssemblyPointerGroups.Add(new AssemblyPointerGroup(" Signed Assemblies", signedAssemblies));
            model.AssemblyPointerGroups.Add(new AssemblyPointerGroup(" Unsigned Assemblies", unsignedAssemblies));
        }
    }
}
