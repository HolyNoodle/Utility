using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Qbox.Common.DAL
{
    public class DalObjectValue
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public DateTime Date { get; internal set; }

        public DalObjectValue()
        {
            Date = DateTime.Now;
        }
    }
}