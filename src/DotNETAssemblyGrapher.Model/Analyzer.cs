using System.Collections.Generic;

namespace DotNETAssemblyGrapherModel
{
    public interface Analyzer
    {
        void Analyze(Model model, List<ISpecification> specifications);
    } 

    public interface ISpecification
    {
        string Name { get; }
    }
}
