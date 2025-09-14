using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsVIVABackend.DTO
{
    // In this case all the properties are the same as in the Patient model, if the program is extended in the future with more properties, 
    // The DTO can be modified to include only the necessary properties for data transfer.
    public class PatientDTO
    {
        public int PatientId { get; set; }

        public string DocumentType { get; set; } = null!;

        public string DocumentNumber { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public DateOnly BirthDate { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
