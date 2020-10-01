using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace DotNETAssemblyGrapherApplication
{
    public class ComponentSpecification
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public List<string> Assemblies { get; set; } = new List<string>();

        [JsonProperty]
        public Color Color { get; set; }

        [JsonProperty]
        public List<ComponentSpecification> Subcomponents { get; set; } = new List<ComponentSpecification>();

        [JsonConstructor]
        public ComponentSpecification(string name, List<string> assemblies, string colorname, List<ComponentSpecification> subcomponents)
        {
            if (assemblies == null && subcomponents == null)
                throw new ArgumentNullException();

            Name = name;
            Color = GetColor(colorname);

            if (assemblies != null)
                Assemblies = assemblies;

            if (subcomponents != null)
                Subcomponents = subcomponents;
        }

        // Excel Constructor
        public ComponentSpecification(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Invalid Specification File :\nPlease respect the spreadsheet pattern, you didn't specified a component");
                throw new ArgumentNullException();
            }

            Name = name;
            Color = RandomColor();
        }

        private Color GetColor(string colorname)
        {
            Type type = typeof(Color);

            try
            {
                PropertyInfo property = type.GetProperty(colorname);
                return (Color)property.GetValue(null);
            }
            catch
            {
                return RandomColor();
            }
        }

        private List<Color> componentColors = new List<Color>() { Color.BlueViolet, Color.Brown, Color.Chocolate, Color.DarkOrange, Color.DarkGray, Color.ForestGreen, Color.Gold, Color.Goldenrod, Color.Gray, Color.Green, Color.Olive, Color.Orange, Color.SaddleBrown, Color.SeaGreen, Color.Sienna };
        private static Random rand = new Random();
        private Color RandomColor()
        {
            return componentColors[rand.Next(componentColors.Count)];
        }
    }
}
