namespace DotNETAssemblyGrapherModel
{
    public class Property
    {
        public readonly string name;
        public readonly string value;

        public Property(string name, object value)
        {
            this.name = name;
            this.value = value.ToString();
        }
    }
}
