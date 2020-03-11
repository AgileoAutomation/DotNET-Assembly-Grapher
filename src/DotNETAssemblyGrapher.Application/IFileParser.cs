using System.Collections.Generic;

namespace DotNETAssemblyGrapherApplication
{
    public interface IFileParser
    {
        List<ComponentSpecification> BuildSpecifications(string filepath);
    }
}
