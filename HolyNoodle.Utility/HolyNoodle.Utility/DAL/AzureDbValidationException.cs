using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.Common.DAL
{
    public class AzureDbValidationException : Exception
    {
        public Dictionary<string, string> Errors { get; set; }

        public AzureDbValidationException(Dictionary<string, string> errors) : base("Procedure validation failed")
        {
            Errors = errors;
        }
    }
}
