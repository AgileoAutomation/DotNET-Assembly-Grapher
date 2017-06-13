namespace DotNETAssemblyGrapherModel
{
    public class Property
    {
        public string Name { get; internal set; }
        public object Value { get; set; }

        //public T GetValue<T>()
        //{
        //    return (T)Value;
        //}

        public Property(string name, object value)
        {
            Name = name;
            Value = value;
        } 
    }
}
