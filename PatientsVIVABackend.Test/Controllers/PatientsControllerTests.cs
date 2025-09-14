using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PatientsVIVABackend.API.Controllers;
using PatientsVIVABackend.BLL.PatientsService.Contract;
using PatientsVIVABackend.DTO;
using PatientsVIVABackend.Utility;
using Xunit;

namespace PatientsVIVABackend.Test.Controllers
{
    public class PatientsControllerTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly Mock<ILogger<PatientsController>> _mockLogger;
        private readonly PatientsController _controller;

        public PatientsControllerTests()
        {
            _mockPatientService = new Mock<IPatientService>();
            _mockLogger = new Mock<ILogger<PatientsController>>();
            _controller = new PatientsController(_mockPatientService.Object, _mockLogger.Object);
        }

        #region GetPatients Tests

        [Fact]
        public async Task GetPatients_ValidParameters_ReturnsOkWithPagedResult()
        {
            // Arrange
            var pagedResult = new PagedResultDTO<PatientDTO>
            {
                Data = new List<PatientDTO>
                {
                    new PatientDTO
                    {
                        PatientId = 1,
                        DocumentType = "CC",
                        DocumentNumber = "12345678",
                        FirstName = "Juan",
                        LastName = "Pérez",
                        BirthDate = new DateOnly(1990, 1, 1),
                        Email = "juan@test.com",
                        CreatedAt = DateTime.UtcNow
                    }
                },
                TotalRecords = 1,
                Page = 1,
                PageSize = 10
            };

            _mockPatientService.Setup(s => s.GetPatientsPaginated(1, 10, null, null))
                              .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetPatients(1, 10, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Patients retrieved successfully", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetPatients_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockPatientService.Setup(s => s.GetPatientsPaginated(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                              .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.GetPatients();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            var response = Assert.IsType<ResponseAPI>(statusResult.Value);
            Assert.False(response.Success);
            Assert.Contains("error occurred", response.Message);
        }

        #endregion

        #region GetPatientById Tests

        [Fact]
        public async Task GetPatientById_ValidId_ReturnsOkWithPatient()
        {
            // Arrange
            var patientId = 1;
            var patientDto = new PatientDTO
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

            _mockPatientService.Setup(s => s.GetPatientById(patientId))
                              .ReturnsAsync(patientDto);

            // Act
            var result = await _controller.GetPatientById(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Patient retrieved successfully", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetPatientById_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _controller.GetPatientById(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid patient ID", response.Message);
        }

        [Fact]
        public async Task GetPatientById_PatientNotFound_ReturnsNotFound()
        {
            // Arrange
            var patientId = 999;
            _mockPatientService.Setup(s => s.GetPatientById(patientId))
                              .ReturnsAsync((PatientDTO)null);

            // Act
            var result = await _controller.GetPatientById(patientId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(notFoundResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Patient not found", response.Message);
        }

        [Fact]
        public async Task GetPatientById_ServiceThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var patientId = 1;
            _mockPatientService.Setup(s => s.GetPatientById(patientId))
                              .ThrowsAsync(new ArgumentException("Invalid argument"));

            // Act
            var result = await _controller.GetPatientById(patientId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid argument", response.Message);
        }

        #endregion

        #region CreatePatient Tests

        [Fact]
        public async Task CreatePatient_ValidData_ReturnsCreatedResult()
        {
            // Arrange
            var createPatientDto = new CreatePatientDTO
            {
                DocumentType = "CC",
                DocumentNumber = "87654321",
                FirstName = "María",
                LastName = "García",
                BirthDate = new DateTime(1985, 5, 15),
                Email = "maria@test.com"
            };

            var createdPatientDto = new PatientDTO
            {
                PatientId = 1,
                DocumentType = "CC",
                DocumentNumber = "87654321",
                FirstName = "María",
                LastName = "García",
                BirthDate = new DateOnly(1985, 5, 15),
                Email = "maria@test.com",
                CreatedAt = DateTime.UtcNow
            };

            _mockPatientService.Setup(s => s.CreatePatient(createPatientDto))
                              .ReturnsAsync(createdPatientDto);

            // Act
            var result = await _controller.CreatePatient(createPatientDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(createdResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Patient created successfully", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task CreatePatient_DuplicatePatient_ReturnsConflict()
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

            _mockPatientService.Setup(s => s.CreatePatient(createPatientDto))
                              .ThrowsAsync(new Exception("A patient with the same document already exists"));

            // Act
            var result = await _controller.CreatePatient(createPatientDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(conflictResult.Value);
            Assert.False(response.Success);
            Assert.Contains("already exists", response.Message);
        }

        [Fact]
        public async Task CreatePatient_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var createPatientDto = new CreatePatientDTO
            {
                DocumentType = "CC",
                DocumentNumber = "87654321",
                FirstName = "María",
                LastName = "García",
                BirthDate = new DateTime(1985, 5, 15)
            };

            _mockPatientService.Setup(s => s.CreatePatient(createPatientDto))
                              .ThrowsAsync(new Exception("Database connection error"));

            // Act
            var result = await _controller.CreatePatient(createPatientDto);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            var response = Assert.IsType<ResponseAPI>(statusResult.Value);
            Assert.False(response.Success);
            Assert.Contains("error occurred", response.Message);
        }

        #endregion

        #region UpdatePatient Tests

        [Fact]
        public async Task UpdatePatient_ValidData_ReturnsOkWithUpdatedPatient()
        {
            // Arrange
            var patientId = 1;
            var updatePatientDto = new UpdatePatientDTO
            {
                FirstName = "Juan Carlos",
                Email = "juancarlos@test.com"
            };

            var updatedPatientDto = new PatientDTO
            {
                PatientId = patientId,
                DocumentType = "CC",
                DocumentNumber = "12345678",
                FirstName = "Juan Carlos",
                LastName = "Pérez",
                BirthDate = new DateOnly(1990, 1, 1),
                Email = "juancarlos@test.com",
                CreatedAt = DateTime.UtcNow
            };

            _mockPatientService.Setup(s => s.UpdatePatient(patientId, updatePatientDto))
                              .ReturnsAsync(updatedPatientDto);

            // Act
            var result = await _controller.UpdatePatient(patientId, updatePatientDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Patient updated successfully", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task UpdatePatient_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0;
            var updatePatientDto = new UpdatePatientDTO { FirstName = "Test" };

            // Act
            var result = await _controller.UpdatePatient(invalidId, updatePatientDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid patient ID", response.Message);
        }

        [Fact]
        public async Task UpdatePatient_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var patientId = 1;
            var updatePatientDto = new UpdatePatientDTO
            {
                Email = "invalid-email"
            };

            // Act
            var result = await _controller.UpdatePatient(patientId, updatePatientDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid email format", response.Message);
        }

        [Fact]
        public async Task UpdatePatient_PatientNotFound_ReturnsNotFound()
        {
            // Arrange
            var patientId = 999;
            var updatePatientDto = new UpdatePatientDTO { FirstName = "Test" };

            _mockPatientService.Setup(s => s.UpdatePatient(patientId, updatePatientDto))
                              .ThrowsAsync(new Exception("Patient not found"));

            // Act
            var result = await _controller.UpdatePatient(patientId, updatePatientDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(notFoundResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Patient not found", response.Message);
        }

        #endregion

        #region DeletePatient Tests

        [Fact]
        public async Task DeletePatient_ValidId_ReturnsOk()
        {
            // Arrange
            var patientId = 1;
            _mockPatientService.Setup(s => s.DeletePatient(patientId))
                              .ReturnsAsync(true);

            // Act
            var result = await _controller.DeletePatient(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("Patient deleted successfully", response.Message);
        }

        [Fact]
        public async Task DeletePatient_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var invalidId = 0;

            // Act
            var result = await _controller.DeletePatient(invalidId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid patient ID", response.Message);
        }

        [Fact]
        public async Task DeletePatient_PatientNotFound_ReturnsNotFound()
        {
            // Arrange
            var patientId = 999;
            _mockPatientService.Setup(s => s.DeletePatient(patientId))
                              .ThrowsAsync(new Exception("Patient not found"));

            // Act
            var result = await _controller.DeletePatient(patientId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(notFoundResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Patient not found", response.Message);
        }

        [Fact]
        public async Task DeletePatient_ServiceReturnsFalse_ReturnsInternalServerError()
        {
            // Arrange
            var patientId = 1;
            _mockPatientService.Setup(s => s.DeletePatient(patientId))
                              .ReturnsAsync(false);

            // Act
            var result = await _controller.DeletePatient(patientId);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
            var response = Assert.IsType<ResponseAPI>(statusResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Failed to delete patient", response.Message);
        }

        #endregion

        #region GetPatientsCreatedAfter Tests

        [Fact]
        public async Task GetPatientsCreatedAfter_ValidDate_ReturnsOkWithPatients()
        {
            // Arrange
            var filterDate = DateTime.UtcNow.AddDays(-7);
            var patients = new List<PatientDTO>
            {
                new PatientDTO
                {
                    PatientId = 1,
                    DocumentType = "CC",
                    DocumentNumber = "12345678",
                    FirstName = "Juan",
                    LastName = "Pérez",
                    BirthDate = new DateOnly(1990, 1, 1),
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            _mockPatientService.Setup(s => s.GetPatientsCreatedAfterAsync(filterDate))
                              .ReturnsAsync(patients);

            // Act
            var result = await _controller.GetPatientsCreatedAfter(filterDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);
            Assert.True(response.Success);
            Assert.Contains("Found 1 patients", response.Message);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetPatientsCreatedAfter_DefaultDate_ReturnsBadRequest()
        {
            // Arrange
            var defaultDate = default(DateTime);

            // Act
            var result = await _controller.GetPatientsCreatedAfter(defaultDate);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(badRequestResult.Value);
            Assert.False(response.Success);
            Assert.Equal("Invalid date parameter", response.Message);
        }

        #endregion
    }
}