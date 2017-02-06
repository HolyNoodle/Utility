using System;
using System.Collections.Generic;

namespace HolyNoodle.Utility.DAL
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
