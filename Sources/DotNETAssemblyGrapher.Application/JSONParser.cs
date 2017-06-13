using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;

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
            catch
            {
                MessageBox.Show("The specifications file is open in a other process");
                return null;
            }
            

            try
            {
                List<ComponentSpecification> specs = (List<ComponentSpecification>)serializer.Deserialize(reader, typeof(List<ComponentSpecification>));
                return specs;
            }
            catch (Exception e)
            {
                MessageBox.Show("Invalid specifications file :\nPlease you must give at least an assembly or a subcomponent by component");
                return null;
            }
        }
    }
}
