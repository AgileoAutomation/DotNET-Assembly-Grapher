using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DependenciesModel
{
    public class Error
    {
        public string Message { get; }

        public Error(string message)
        {
            this.Message = message;
        }
    }
}
