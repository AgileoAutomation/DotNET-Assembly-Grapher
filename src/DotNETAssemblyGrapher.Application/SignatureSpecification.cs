using DotNETAssemblyGrapherModel;

namespace DotNETAssemblyGrapherApplication
{
    public class SignatureSpecification : ISpecification
    {
        public string Name { get; private set; }
        public bool IsSigned { get; private set; }

        public SignatureSpecification(string name, bool isSigned)
        {
            Name = name;
            IsSigned = isSigned;
        }
    }
}
