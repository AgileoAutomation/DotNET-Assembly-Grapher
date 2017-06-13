namespace DotNETAssemblyGrapherModel
{
    public class Dependency
    {
        public Dependency(AssemblyPointer from, AssemblyPointer to)
        {
            From = from;
            To = to;
        }

        /////////////////////
        ///////GETTERS///////
        /////////////////////

        public AssemblyPointer From { get; internal set; }

        public AssemblyPointer To { get; internal set; }
    }
}
