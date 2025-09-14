using AutoMapper;
using PatientsVIVABackend.Utility;
using System.Reflection;

namespace PatientsVIVABackend.Test.Common
{
    /// <summary>
    /// Clase base para las pruebas unitarias que proporciona configuraciones comunes
    /// </summary>
    public abstract class TestBase
    {
        protected readonly IMapper Mapper;

        protected TestBase()
        {
            // Configurar AutoMapper para las pruebas
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            Mapper = config.CreateMapper();
        }

        /// <summary>
        /// Crea un paciente de prueba con datos válidos
        /// </summary>
        protected static Model.Patient CreateTestPatient(int id = 1)
        {
            return new Model.Patient
            {
                PatientId = id,
                DocumentType = "CC",
                DocumentNumber = $"1234567{id}",
                FirstName = "Juan",
                LastName = "Pérez",
                BirthDate = new DateOnly(1990, 1, 1),
                Email = $"juan{id}@test.com",
                PhoneNumber = "3001234567",
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Crea un CreatePatientDTO de prueba con datos válidos
        /// </summary>
        protected static DTO.CreatePatientDTO CreateTestCreatePatientDTO()
        {
            return new DTO.CreatePatientDTO
            {
                DocumentType = "CC",
                DocumentNumber = "87654321",
                FirstName = "María",
                LastName = "García",
                BirthDate = new DateTime(1985, 5, 15),
                Email = "maria@test.com",
                PhoneNumber = "3001234567"
            };
        }

        /// <summary>
        /// Crea un UpdatePatientDTO de prueba con datos válidos
        /// </summary>
        protected static DTO.UpdatePatientDTO CreateTestUpdatePatientDTO()
        {
            return new DTO.UpdatePatientDTO
            {
                FirstName = "Juan Carlos",
                Email = "juancarlos@test.com",
                PhoneNumber = "3009876543"
            };
        }

        /// <summary>
        /// Crea un PatientDTO de prueba con datos válidos
        /// </summary>
        protected static DTO.PatientDTO CreateTestPatientDTO(int id = 1)
        {
            return new DTO.PatientDTO
            {
                PatientId = id,
                DocumentType = "CC",
                DocumentNumber = $"1234567{id}",
                FirstName = "Juan",
                LastName = "Pérez",
                BirthDate = new DateOnly(1990, 1, 1),
                Email = $"juan{id}@test.com",
                PhoneNumber = "3001234567",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}