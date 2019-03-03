using System;
using System.ComponentModel.DataAnnotations;

namespace IDCardApp.Entity
{
    public class Building : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

    }
}
