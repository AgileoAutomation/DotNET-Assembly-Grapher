using DotNETAssemblyGrapherModel;

namespace DotNETAssemblyGrapherApplication
{
    public class ObfuscationSpecification : ISpecification
    {
        public string Name { get; private set; }
        public bool IsObfuscated { get; private set; }

        public ObfuscationSpecification(string name, bool isObfuscated)
        {
            Name = name;
            IsObfuscated = isObfuscated;
        }
    }
}
