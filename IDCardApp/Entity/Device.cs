using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDCardApp.Entity
{
    public class Device : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string SerialNum { get; set; }
        public string Building { get; set; }
    }
}
