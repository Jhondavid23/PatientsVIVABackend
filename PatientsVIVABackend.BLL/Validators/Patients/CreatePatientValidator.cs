using FluentValidation;
using PatientsVIVABackend.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PatientsVIVABackend.BLL.Validators.Patients
{
    public class CreatePatientValidator : AbstractValidator<CreatePatientDTO>
    {
        public CreatePatientValidator()
        {
            RuleFor(x => x.DocumentType)
                .NotEmpty().WithMessage("El tipo de documento es requerido")
                .Length(1, 10).WithMessage("El tipo de documento debe tener entre 1 y 10 caracteres")
                .Must(BeValidDocumentType).WithMessage("Tipo de documento inválido. Use: CC, TI, CE, PP, NIT, DNI");

            RuleFor(x => x.DocumentNumber)
                .NotEmpty().WithMessage("El número de documento es requerido")
                .Length(5, 20).WithMessage("El número de documento debe tener entre 5 y 20 caracteres")
                .Matches(@"^[0-9]+$").WithMessage("El número de documento solo puede contener números");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido")
                .Length(2, 80).WithMessage("El nombre debe tener entre 2 y 80 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras y espacios");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido")
                .Length(2, 80).WithMessage("El apellido debe tener entre 2 y 80 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El apellido solo puede contener letras y espacios");

            RuleFor(x => x.BirthDate)
                .NotEmpty().WithMessage("La fecha de nacimiento es requerida")
                .Must(BeValidAge).WithMessage("La edad debe estar entre 0 y 120 años")
                .Must(NotBeFutureDate).WithMessage("La fecha de nacimiento no puede ser futura");

            // VALIDACIÓN ACTUALIZADA DE TELÉFONO
            RuleFor(x => x.PhoneNumber)
                .Must(BeValidColombianPhoneNumber).WithMessage("Formato de teléfono inválido. Use: 3XXXXXXXXX (10 dígitos) o +573XXXXXXXXX (12 dígitos con código de país)")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Formato de email inválido")
                .MaximumLength(120).WithMessage("El email no puede exceder 120 caracteres")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }

        private static bool BeValidDocumentType(string documentType)
        {
            var validTypes = new[] { "CC", "TI", "CE", "PP", "NIT", "DNI" };
            return validTypes.Contains(documentType?.ToUpper());
        }

        private static bool BeValidAge(DateTime birthDate)
        {
            var age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now.DayOfYear < birthDate.DayOfYear)
                age--;

            return age >= 0 && age <= 120;
        }

        private static bool NotBeFutureDate(DateTime birthDate)
        {
            return birthDate <= DateTime.Now;
        }

        private static bool BeValidColombianPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return true; // Campo opcional

            // Remover espacios, guiones y paréntesis para validación
            var cleanPhone = Regex.Replace(phoneNumber, @"[\s\-\(\)]", "");

            // Formato 1: +573XXXXXXXXX (12 dígitos con código de país +57)
            if (Regex.IsMatch(cleanPhone, @"^\+573[0-9]{9}$"))
                return true;

            // Formato 2: 3XXXXXXXXX (10 dígitos)
            if (Regex.IsMatch(cleanPhone, @"^3[0-9]{9}$"))
                return true;

            // Formato 3: Teléfono fijo: 601XXXXXXX (10 dígitos)
            if (Regex.IsMatch(cleanPhone, @"^601[0-9]{7}$"))
                return true;

            // Formato 4: Teléfono fijo otras ciudades: 60[2-8]XXXXXXX (10 dígitos)
            if (Regex.IsMatch(cleanPhone, @"^60[2-8][0-9]{7}$"))
                return true;

            return false;
        }
    }
}
