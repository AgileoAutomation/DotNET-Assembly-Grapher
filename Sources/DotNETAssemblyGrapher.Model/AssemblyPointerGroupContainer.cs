using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNETAssemblyGrapherModel
{
    public class AssemblyPointerGroupContainer
    {
        public List<AssemblyPointerGroup> Groups { get; } = new List<AssemblyPointerGroup>();

        public void AddGroup(string groupName, IEnumerable<AssemblyPointer> inputs)
        {
            Groups.Add(new AssemblyPointerGroup(groupName, inputs));
        }
    }
}
