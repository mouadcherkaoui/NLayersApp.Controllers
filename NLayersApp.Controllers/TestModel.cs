using NLayersApp.Persistence.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NLayersApp.Controllers
{
    public class TestModel: IAuditable, ISoftDelete
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }
        public string Description { get; set; }
    }
}
