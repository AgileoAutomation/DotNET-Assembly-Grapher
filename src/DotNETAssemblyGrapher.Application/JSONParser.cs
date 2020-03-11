using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace DotNETAssemblyGrapherApplication
{
    public class JSONParser : IFileParser
    {
        StreamReader reader;
        JsonSerializer serializer;
        public List<ComponentSpecification> BuildSpecifications(string filepath)
        {
            try
            {
                reader = File.OpenText(filepath);
                serializer = new JsonSerializer();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The specification file is open in a other process", ex);
            }
            

            try
            {
                List<ComponentSpecification> specs = (List<ComponentSpecification>)serializer.Deserialize(reader, typeof(List<ComponentSpecification>));
                return specs;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Invalid specifications file :\nPlease you must give at least an assembly or a subcomponent by component", ex);
            }
        }
    }
}
