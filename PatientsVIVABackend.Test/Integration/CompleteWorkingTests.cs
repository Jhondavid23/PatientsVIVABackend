using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PatientsVIVABackend.API.Controllers;
using PatientsVIVABackend.BLL.PatientsService;
using PatientsVIVABackend.DAL.Repositories;
using PatientsVIVABackend.DTO;
using PatientsVIVABackend.Model;
using PatientsVIVABackend.Test.Common;
using PatientsVIVABackend.Utility;
using Xunit;

namespace PatientsVIVABackend.Test.Integration
{
    /// <summary>
    /// Pruebas de integración completas y funcionales que simulan el flujo real de la aplicación
    /// Estas pruebas NO usan WebApplicationFactory para evitar problemas de configuración
    /// </summary>
    public class CompleteWorkingTests : TestBase, IDisposable
    {
        private readonly VivapatientsContext _context;
        private readonly PatientsController _controller;
        private readonly PatientService _service;
        private readonly GenericRepository<Patient> _repository;

        public CompleteWorkingTests()
        {
            // Configurar DbContext en memoria con nombre único para cada test
            var options = new DbContextOptionsBuilder<VivapatientsContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new VivapatientsContext(options);
            _context.Database.EnsureCreated();

            // Configurar repositorio
            _repository = new GenericRepository<Patient>(_context);

            // Configurar loggers
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Warning);
            });
            var serviceLogger = loggerFactory.CreateLogger<PatientService>();
            var controllerLogger = loggerFactory.CreateLogger<PatientsController>();

            // Configurar servicio con AutoMapper
            _service = new PatientService(_repository, serviceLogger, Mapper);

            // Configurar controlador
            _controller = new PatientsController(_service, controllerLogger);
        }

        [Fact]
        public async Task Integration_CreatePatient_ValidData_Success()
        {
            // Arrange
            var createDto = CreateTestCreatePatientDTO();
            createDto.DocumentNumber = "TEST001";

            // Act
            var result = await _controller.CreatePatient(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(createdResult.Value);

            Assert.True(response.Success);
            Assert.Equal("Patient created successfully", response.Message);
            Assert.NotNull(response.Data);

            // Verificar que se guardó en la base de datos
            var savedPatients = await _context.Patients.ToListAsync();
            Assert.Single(savedPatients);
            Assert.Equal("TEST001", savedPatients[0].DocumentNumber);
        }

        [Fact]
        public async Task Integration_GetPatientById_ExistingPatient_ReturnsPatient()
        {
            // Arrange: Crear paciente primero
            var createDto = CreateTestCreatePatientDTO();
            createDto.DocumentNumber = "TEST002";

            var createResult = await _controller.CreatePatient(createDto);
            var createResponse = Assert.IsType<ResponseAPI>(((CreatedAtActionResult)createResult.Result).Value);
            var createdPatient = createResponse.Data as PatientDTO;
            var patientId = createdPatient.PatientId;

            // Act
            var getResult = await _controller.GetPatientById(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(getResult.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal("Patient retrieved successfully", response.Message);

            var retrievedPatient = response.Data as PatientDTO;
            Assert.NotNull(retrievedPatient);
            Assert.Equal(patientId, retrievedPatient.PatientId);
            Assert.Equal("TEST002", retrievedPatient.DocumentNumber);
        }

        [Fact]
        public async Task Integration_GetPatientById_NonExistentPatient_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetPatientById(99999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(notFoundResult.Value);

            Assert.False(response.Success);
            Assert.Equal("Patient not found", response.Message);
        }

        [Fact]
        public async Task Integration_UpdatePatient_ValidData_Success()
        {
            // Arrange: Crear paciente primero
            var createDto = CreateTestCreatePatientDTO();
            createDto.DocumentNumber = "TEST003";
            createDto.FirstName = "Original";
            createDto.Email = "original@test.com";

            var createResult = await _controller.CreatePatient(createDto);
            var createResponse = Assert.IsType<ResponseAPI>(((CreatedAtActionResult)createResult.Result).Value);
            var createdPatient = createResponse.Data as PatientDTO;
            var patientId = createdPatient.PatientId;

            // Act: Actualizar
            var updateDto = new UpdatePatientDTO
            {
                FirstName = "Updated",
                Email = "updated@test.com"
            };

            var updateResult = await _controller.UpdatePatient(patientId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(updateResult.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal("Patient updated successfully", response.Message);

            var updatedPatient = response.Data as PatientDTO;
            Assert.NotNull(updatedPatient);
            Assert.Equal("Updated", updatedPatient.FirstName);
            Assert.Equal("updated@test.com", updatedPatient.Email);

            // Verificar en base de datos
            var dbPatient = await _context.Patients.FindAsync(patientId);
            Assert.Equal("Updated", dbPatient.FirstName);
            Assert.Equal("updated@test.com", dbPatient.Email);
        }

        [Fact]
        public async Task Integration_DeletePatient_ExistingPatient_Success()
        {
            // Arrange: Crear paciente primero
            var createDto = CreateTestCreatePatientDTO();
            createDto.DocumentNumber = "TEST004";

            var createResult = await _controller.CreatePatient(createDto);
            var createResponse = Assert.IsType<ResponseAPI>(((CreatedAtActionResult)createResult.Result).Value);
            var createdPatient = createResponse.Data as PatientDTO;
            var patientId = createdPatient.PatientId;

            // Act
            var deleteResult = await _controller.DeletePatient(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(deleteResult.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal("Patient deleted successfully", response.Message);

            // Verificar que se eliminó de la base de datos
            var dbPatient = await _context.Patients.FindAsync(patientId);
            Assert.Null(dbPatient);
        }

        [Fact]
        public async Task Integration_CreatePatient_DuplicateDocument_ReturnsConflict()
        {
            // Arrange: Crear primer paciente
            var createDto1 = CreateTestCreatePatientDTO();
            createDto1.DocumentNumber = "DUPLICATE001";
            await _controller.CreatePatient(createDto1);

            // Intentar crear segundo paciente con mismo documento
            var createDto2 = CreateTestCreatePatientDTO();
            createDto2.DocumentNumber = "DUPLICATE001";
            createDto2.FirstName = "Different Name";

            // Act
            var result = await _controller.CreatePatient(createDto2);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(conflictResult.Value);

            Assert.False(response.Success);
            Assert.Contains("already exists", response.Message);

            // Verificar que solo hay un paciente en la base de datos
            var patientsCount = await _context.Patients.CountAsync();
            Assert.Equal(1, patientsCount);
        }

        [Fact]
        public async Task Integration_GetPatients_WithPagination_ReturnsPagedResults()
        {
            // Arrange: Crear múltiples pacientes
            for (int i = 1; i <= 15; i++)
            {
                var createDto = CreateTestCreatePatientDTO();
                createDto.DocumentNumber = $"PAGE{i:D3}";
                createDto.FirstName = $"Patient{i}";
                await _controller.CreatePatient(createDto);
            }

            // Act: Obtener primera página
            var result = await _controller.GetPatients(page: 1, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal("Patients retrieved successfully", response.Message);

            var pagedResult = response.Data as PagedResultDTO<PatientDTO>;
            Assert.NotNull(pagedResult);
            Assert.Equal(10, pagedResult.Data.Count);
            Assert.Equal(15, pagedResult.TotalRecords);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(10, pagedResult.PageSize);
            Assert.Equal(2, pagedResult.TotalPages); // 15 registros / 10 por página = 2 páginas
        }

        [Fact]
        public async Task Integration_GetPatients_WithNameFilter_ReturnsFilteredResults()
        {
            // Arrange: Crear pacientes con nombres específicos
            var patients = new[]
            {
                new { Doc = "FILTER001", First = "Juan", Last = "Pérez" },
                new { Doc = "FILTER002", First = "María", Last = "García" },
                new { Doc = "FILTER003", First = "Juan", Last = "López" },
                new { Doc = "FILTER004", First = "Pedro", Last = "Martínez" }
            };

            foreach (var p in patients)
            {
                var createDto = CreateTestCreatePatientDTO();
                createDto.DocumentNumber = p.Doc;
                createDto.FirstName = p.First;
                createDto.LastName = p.Last;
                await _controller.CreatePatient(createDto);
            }

            // Act: Filtrar por nombre "Juan"
            var result = await _controller.GetPatients(name: "Juan");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ResponseAPI>(okResult.Value);

            Assert.True(response.Success);

            var pagedResult = response.Data as PagedResultDTO<PatientDTO>;
            Assert.NotNull(pagedResult);
            Assert.Equal(2, pagedResult.Data.Count); // Solo Juan Pérez y Juan López
            Assert.All(pagedResult.Data, p => Assert.Contains("Juan", $"{p.FirstName} {p.LastName}"));
        }

        [Fact]
        public async Task Integration_CompleteWorkflow_CreateGetUpdateDelete_Success()
        {
            // Esta prueba simula un flujo completo de uso de la API

            // 1. CREATE
            var createDto = CreateTestCreatePatientDTO();
            createDto.DocumentNumber = "WORKFLOW001";
            createDto.FirstName = "WorkflowTest";
            createDto.Email = "workflow@test.com";

            var createResult = await _controller.CreatePatient(createDto);
            var createResponse = Assert.IsType<ResponseAPI>(((CreatedAtActionResult)createResult.Result).Value);
            var createdPatient = createResponse.Data as PatientDTO;
            var patientId = createdPatient.PatientId;

            Assert.NotNull(createdPatient);
            Assert.Equal("WorkflowTest", createdPatient.FirstName);

            // 2. GET (verificar que se puede recuperar)
            var getResult = await _controller.GetPatientById(patientId);
            var getResponse = Assert.IsType<ResponseAPI>(((OkObjectResult)getResult.Result).Value);
            var retrievedPatient = getResponse.Data as PatientDTO;

            Assert.NotNull(retrievedPatient);
            Assert.Equal(patientId, retrievedPatient.PatientId);
            Assert.Equal("WorkflowTest", retrievedPatient.FirstName);

            // 3. UPDATE
            var updateDto = new UpdatePatientDTO
            {
                FirstName = "UpdatedWorkflow",
                Email = "updated-workflow@test.com"
            };

            var updateResult = await _controller.UpdatePatient(patientId, updateDto);
            var updateResponse = Assert.IsType<ResponseAPI>(((OkObjectResult)updateResult.Result).Value);
            var updatedPatient = updateResponse.Data as PatientDTO;

            Assert.NotNull(updatedPatient);
            Assert.Equal("UpdatedWorkflow", updatedPatient.FirstName);
            Assert.Equal("updated-workflow@test.com", updatedPatient.Email);

            // 4. GET (verificar actualización)
            var getUpdatedResult = await _controller.GetPatientById(patientId);
            var getUpdatedResponse = Assert.IsType<ResponseAPI>(((OkObjectResult)getUpdatedResult.Result).Value);
            var verifyPatient = getUpdatedResponse.Data as PatientDTO;

            Assert.Equal("UpdatedWorkflow", verifyPatient.FirstName);
            Assert.Equal("updated-workflow@test.com", verifyPatient.Email);

            // 5. DELETE
            var deleteResult = await _controller.DeletePatient(patientId);
            var deleteResponse = Assert.IsType<ResponseAPI>(((OkObjectResult)deleteResult.Result).Value);

            Assert.True(deleteResponse.Success);

            // 6. GET (verificar eliminación)
            var getDeletedResult = await _controller.GetPatientById(patientId);
            var getDeletedResponse = Assert.IsType<ResponseAPI>(((NotFoundObjectResult)getDeletedResult.Result).Value);

            Assert.False(getDeletedResponse.Success);
            Assert.Equal("Patient not found", getDeletedResponse.Message);
        }

        [Fact]
        public async Task Integration_ValidationErrors_ReturnAppropriateResponses()
        {
            // Test ID inválido en GET
            var getInvalidResult = await _controller.GetPatientById(0);
            var getBadRequest = Assert.IsType<BadRequestObjectResult>(getInvalidResult.Result);
            var getBadResponse = Assert.IsType<ResponseAPI>(getBadRequest.Value);
            Assert.False(getBadResponse.Success);
            Assert.Equal("Invalid patient ID", getBadResponse.Message);

            // Test ID inválido en UPDATE
            var updateDto = new UpdatePatientDTO { FirstName = "Test" };
            var updateInvalidResult = await _controller.UpdatePatient(0, updateDto);
            var updateBadRequest = Assert.IsType<BadRequestObjectResult>(updateInvalidResult.Result);
            var updateBadResponse = Assert.IsType<ResponseAPI>(updateBadRequest.Value);
            Assert.False(updateBadResponse.Success);
            Assert.Equal("Invalid patient ID", updateBadResponse.Message);

            // Test ID inválido en DELETE
            var deleteInvalidResult = await _controller.DeletePatient(-1);
            var deleteBadRequest = Assert.IsType<BadRequestObjectResult>(deleteInvalidResult.Result);
            var deleteBadResponse = Assert.IsType<ResponseAPI>(deleteBadRequest.Value);
            Assert.False(deleteBadResponse.Success);
            Assert.Equal("Invalid patient ID", deleteBadResponse.Message);

            // Test email inválido en UPDATE
            var createDto = CreateTestCreatePatientDTO();
            createDto.DocumentNumber = "VALIDATION001";
            var createResult = await _controller.CreatePatient(createDto);
            var createResponse = Assert.IsType<ResponseAPI>(((CreatedAtActionResult)createResult.Result).Value);
            var createdPatient = createResponse.Data as PatientDTO;

            var invalidEmailDto = new UpdatePatientDTO { Email = "invalid-email-format" };
            var emailResult = await _controller.UpdatePatient(createdPatient.PatientId, invalidEmailDto);
            var emailBadRequest = Assert.IsType<BadRequestObjectResult>(emailResult.Result);
            var emailBadResponse = Assert.IsType<ResponseAPI>(emailBadRequest.Value);
            Assert.False(emailBadResponse.Success);
            Assert.Equal("Invalid email format", emailBadResponse.Message);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}