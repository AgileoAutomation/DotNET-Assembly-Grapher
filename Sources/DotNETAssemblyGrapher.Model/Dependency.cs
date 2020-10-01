namespace DotNETAssemblyGrapherModel
{
    public class Dependency
    {
        public AssemblyPointer from;
        public AssemblyPointer to;

        public Dependency(AssemblyPointer from, AssemblyPointer to)
        {
            this.from = from;
            this.to = to;
        }
    }
}