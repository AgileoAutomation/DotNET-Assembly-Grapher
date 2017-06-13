namespace DotNETAssemblyGrapherModel
{
    public class Dependency
    {
        public AssemblyPointer From { get; }

        public AssemblyPointer To { get; }

        public Dependency(AssemblyPointer from, AssemblyPointer to)
        {
            From = from;
            To = to;
        }

    }
}
