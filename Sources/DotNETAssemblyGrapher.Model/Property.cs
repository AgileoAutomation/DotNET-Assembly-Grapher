namespace DotNETAssemblyGrapherModel
{
    public class Property
    {
        public string Name { get; private set; }

        public object Value { get; set; }

        public Property(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
