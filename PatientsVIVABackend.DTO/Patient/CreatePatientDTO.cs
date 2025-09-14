using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsVIVABackend.DTO
{
    public class CreatePatientDTO
    {
        [Required(ErrorMessage = "El tipo de documento es requerido")]
        [StringLength(10, ErrorMessage = "El tipo de documento no puede exceder 10 caracteres")]
        public string DocumentType { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de documento es requerido")]
        [StringLength(20, ErrorMessage = "El número de documento no puede exceder 20 caracteres")]
        public string DocumentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(80, ErrorMessage = "El nombre no puede exceder 80 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(80, ErrorMessage = "El apellido no puede exceder 80 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime BirthDate { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "El email debe tener un formato válido")]
        [StringLength(120, ErrorMessage = "El email no puede exceder 120 caracteres")]
        public string? Email { get; set; }
    }
}
