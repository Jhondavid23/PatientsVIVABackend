using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using PatientsVIVABackend.BLL.PatientsService;
using PatientsVIVABackend.DAL.Repositories;
using PatientsVIVABackend.DTO;
using PatientsVIVABackend.Model;
using PatientsVIVABackend.Utility;
using System.Linq.Expressions;
using Xunit;

namespace PatientsVIVABackend.Test.Services
{
    public class PatientServiceTests
    {
        private readonly Mock<IGenericRepository<Patient>> _mockRepository;
        private readonly Mock<ILogger<PatientService>> _mockLogger;
        private readonly IMapper _mapper;
        private readonly PatientService _patientService;

        public PatientServiceTests()
        {
            _mockRepository = new Mock<IGenericRepository<Patient>>();
            _mockLogger = new Mock<ILogger<PatientService>>();

            // Configurar AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });
            _mapper = config.CreateMapper();

            _patientService = new PatientService(_mockRepository.Object, _mockLogger.Object, _mapper);
        }

        #region GetPatientById Tests

        [Fact]
        public async Task GetPatientById_ValidId_ReturnsPatientDTO()
        {
            // Arrange
            var patientId = 1;
            var patient = new Patient
            {
                PatientId = patientId,
                DocumentType = "CC",
                DocumentNumber = "12345678",
                FirstName = "Juan",
                LastName = "Pérez",
                BirthDate = new DateOnly(1990, 1, 1),
                Email = "juan@test.com",
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Patient, bool>>>()))
                          .ReturnsAsync(patient);

            // Act
            var result = await _patientService.GetPatientById(patientId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(patientId, result.PatientId);
            Assert.Equal("Juan", result.FirstName);
            Assert.Equal("Pérez", result.LastName);
            Assert.Equal("CC", result.DocumentType);
            Assert.Equal("12345678", result.DocumentNumber);
        }

        [Fact]
        public async Task GetPatientById_InvalidId_ThrowsArgumentException()
        {
            // Arrange
            var invalidId = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _patientService.GetPatientById(invalidId));
        }

        [Fact]
        public async Task GetPatientById_PatientNotFound_ReturnsNull()
        {
            // Arrange
            var patientId = 999;
            _mockRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Patient, bool>>>()))
                          .ReturnsAsync((Patient)null);

            // Act
            var result = await _patientService.GetPatientById(patientId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region CreatePatient Tests

        [Fact]
        public async Task CreatePatient_ValidData_ReturnsCreatedPatientDTO()
        {
            // Arrange
            var createPatientDto = new CreatePatientDTO
            {
                DocumentType = "CC",
                DocumentNumber = "87654321",
                FirstName = "María",
                LastName = "García",
                BirthDate = new DateTime(1985, 5, 15),
                Email = "maria@test.com",
                PhoneNumber = "3001234567"
            };

            // Simular que no existe paciente duplicado
            _mockRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Patient, bool>>>()))
                          .ReturnsAsync((Patient)null);

            // Simular la creación del paciente
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Patient>()))
                          .ReturnsAsync((Patient p) =>
                          {
                              p.PatientId = 1;
                              p.CreatedAt = DateTime.UtcNow;
                              return p;
                          });

            // Act
            var result = await _patientService.CreatePatient(createPatientDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("María", result.FirstName);
            Assert.Equal("García", result.LastName);
            Assert.Equal("CC", result.DocumentType);
            Assert.Equal("87654321", result.DocumentNumber);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Once);
        }

        [Fact]
        public async Task CreatePatient_DuplicateDocument_ThrowsException()
        {
            // Arrange
            var createPatientDto = new CreatePatientDTO
            {
                DocumentType = "CC",
                DocumentNumber = "12345678",
                FirstName = "Pedro",
                LastName = "López",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var existingPatient = new Patient
            {
                PatientId = 1,
                DocumentType = "CC",
                DocumentNumber = "12345678",
                FirstName = "Juan",
                LastName = "Pérez"
            };

            _mockRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Patient, bool>>>()))
                          .ReturnsAsync(existingPatient);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _patientService.CreatePatient(createPatientDto));
            Assert.Contains("already exists", exception.Message);
        }

        #endregion

        #region UpdatePatient Tests

        [Fact]
        public async Task UpdatePatient_ValidData_ReturnsUpdatedPatientDTO()
        {
            // Arrange
            var patientId = 1;
            var existingPatient = new Patient
            {
                PatientId = patientId,
                DocumentType = "CC",
                DocumentNumber = "12345678",
                FirstName = "Juan",
                LastName = "Pérez",
                BirthDate = new DateOnly(1990, 1, 1),
                Email = "juan@test.com",
                CreatedAt = DateTime.UtcNow
            };

            var updatePatientDto = new UpdatePatientDTO
            {
                FirstName = "Juan Carlos",
                Email = "juancarlos@test.com"
            };

            _mockRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Patient, bool>>>()))
                          .ReturnsAsync(existingPatient);

            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                          .ReturnsAsync((Patient p) => p);

            // Act
            var result = await _patientService.UpdatePatient(patientId, updatePatientDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Juan Carlos", result.FirstName);
            Assert.Equal("juancarlos@test.com", result.Email);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Patient>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePatient_PatientNotFound_ThrowsException()
        {
            // Arrange
            var patientId = 999;
            var updatePatientDto = new UpdatePatientDTO
            {
                FirstName = "Nuevo Nombre"
            };

            _mockRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Patient, bool>>>()))
                          .ReturnsAsync((Patient)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _patientService.UpdatePatient(patientId, updatePatientDto));
            Assert.Contains("not found", exception.Message);
        }

        #endregion

        #region DeletePatient Tests

        [Fact]
        public async Task DeletePatient_ValidId_ReturnsTrue()
        {
            // Arrange
            var patientId = 1;
            var existingPatient = new Patient
            {
                PatientId = patientId,
                DocumentType = "CC",
                DocumentNumber = "12345678",
                FirstName = "Juan",
                LastName = "Pérez"
            };

            _mockRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Patient, bool>>>()))
                          .ReturnsAsync(existingPatient);

            _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Patient>()))
                          .ReturnsAsync(true);

            // Act
            var result = await _patientService.DeletePatient(patientId);

            // Assert
            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Patient>()), Times.Once);
        }

        [Fact]
        public async Task DeletePatient_PatientNotFound_ThrowsException()
        {
            // Arrange
            var patientId = 999;

            _mockRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Patient, bool>>>()))
                          .ReturnsAsync((Patient)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _patientService.DeletePatient(patientId));
            Assert.Contains("not found", exception.Message);
        }

        #endregion

        #region GetPatientsCreatedAfterAsync Tests

        [Fact]
        public async Task GetPatientsCreatedAfterAsync_ValidDate_ReturnsPatientList()
        {
            // Arrange
            var filterDate = DateTime.UtcNow.AddDays(-7);
            var patients = new List<Patient>
            {
                new Patient
                {
                    PatientId = 1,
                    DocumentType = "CC",
                    DocumentNumber = "12345678",
                    FirstName = "Juan",
                    LastName = "Pérez",
                    BirthDate = new DateOnly(1990, 1, 1),
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Patient
                {
                    PatientId = 2,
                    DocumentType = "TI",
                    DocumentNumber = "87654321",
                    FirstName = "María",
                    LastName = "García",
                    BirthDate = new DateOnly(1995, 5, 15),
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            _mockRepository.Setup(r => r.ExecuteStoredProcedureAsync<Patient>(
                It.IsAny<string>(),
                It.IsAny<SqlParameter[]>()))
                          .ReturnsAsync(patients);

            // Act
            var result = await _patientService.GetPatientsCreatedAfterAsync(filterDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.NotNull(p.FirstName));
            _mockRepository.Verify(r => r.ExecuteStoredProcedureAsync<Patient>(
                "GetPatientsCreatedAfter",
                It.IsAny<SqlParameter[]>()), Times.Once);
        }

        #endregion
    }
}