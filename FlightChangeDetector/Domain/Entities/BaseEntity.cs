using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightChangeDetector.Domain.Entities
{
    public class BaseEntity 
    {
        [Key]
        public int Id { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [NotMapped]
        public DateTime UpdatedDate { get; set; }

        [NotMapped]
        public DateTime CreatedDate { get; set; }
    }
}
