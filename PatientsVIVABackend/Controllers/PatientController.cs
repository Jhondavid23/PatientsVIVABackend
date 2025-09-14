using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PatientsVIVABackend.BLL.PatientsService.Contract;
using PatientsVIVABackend.DTO;
using PatientsVIVABackend.Model;
using PatientsVIVABackend.Utility;
using System.ComponentModel.DataAnnotations;

namespace PatientsVIVABackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientsController> _logger;
        private readonly IValidator<CreatePatientDTO> _createValidator;
        private readonly IValidator<UpdatePatientDTO> _updateValidator;

        public PatientsController(
            IPatientService patientService, 
            ILogger<PatientsController> logger,
            IValidator<CreatePatientDTO> createValidator,
            IValidator<UpdatePatientDTO> updateValidator
        )
        {
            _patientService = patientService;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        /// <summary>
        /// Retrieves a paginated list of patients with optional filtering
        /// Obtiene una lista paginada de pacientes con filtrado opcional
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of records per page (default: 10, max: 100)</param>
        /// <param name="name">Filter by name (partial match)</param>
        /// <param name="documentNumber">Filter by document number (partial match)</param>
        /// <returns>Paginated list of patients</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseAPI>> GetPatients(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? documentNumber = null)
        {
            try
            {
                _logger.LogInformation("Getting patients with pagination - Page: {Page}, PageSize: {PageSize}, Name: {Name}, DocumentNumber: {DocumentNumber}",
                    page, pageSize, name, documentNumber);

                var pagedResult = await _patientService.GetPatientsPaginated(page, pageSize, name, documentNumber);


                return Ok(new ResponseAPI
                {
                    Success = true,
                    Message = "Patients retrieved successfully",
                    Data = pagedResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseAPI
                {
                    Success = false,
                    Message = "An error occurred while retrieving patients",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Retrieves a specific patient by ID
        /// Obtiene un paciente específico por ID
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <returns>Patient details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseAPI>> GetPatientById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ResponseAPI
                    {
                        Success = false,
                        Message = "Invalid patient ID",
                        Data = null
                    });
                }

                _logger.LogInformation("Getting patient by ID: {PatientId}", id);

                var patient = await _patientService.GetPatientById(id);

                if (patient == null)
                {
                    return NotFound(new ResponseAPI
                    {
                        Success = false,
                        Message = "Patient not found",
                        Data = null
                    });
                }

                return Ok(new ResponseAPI
                {
                    Success = true,
                    Message = "Patient retrieved successfully",
                    Data = patient
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when getting patient by ID: {PatientId}", id);
                return BadRequest(new ResponseAPI
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient by ID: {PatientId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseAPI
                {
                    Success = false,
                    Message = "An error occurred while retrieving the patient",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Creates a new patient
        /// Crea un nuevo paciente
        /// </summary>
        /// <param name="createPatientDto">Patient data</param>
        /// <returns>Created patient</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseAPI>> CreatePatient([FromBody] CreatePatientDTO createPatientDto)
        {
            try
            {
                // Validators FluentValidation
                var validationResult = await _createValidator.ValidateAsync(createPatientDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new
                    {
                        Property = e.PropertyName,
                        Error = e.ErrorMessage,
                        AttemptedValue = e.AttemptedValue
                    }).ToList();

                    return BadRequest(new ResponseAPI
                    {
                        Success = false,
                        Message = "Errores de validación",
                        Data = errors
                    });
                }


                _logger.LogInformation("Creating new patient with DocumentType: {DocumentType} and DocumentNumber: {DocumentNumber}",
                    createPatientDto.DocumentType, createPatientDto.DocumentNumber);

                var createdPatient = await _patientService.CreatePatient(createPatientDto);

                return CreatedAtAction(
                    nameof(GetPatientById),
                    new { id = createdPatient.PatientId },
                    new ResponseAPI
                    {
                        Success = true,
                        Message = "Patient created successfully",
                        Data = createdPatient
                    });
            }
            catch (Exception ex) when (ex.Message.Contains("already exists"))
            {
                _logger.LogWarning(ex, "Attempt to create duplicate patient");
                return Conflict(new ResponseAPI
                {
                    Success = false,
                    Message = "A patient with the same document type and number already exists",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient");
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseAPI
                {
                    Success = false,
                    Message = "An error occurred while creating the patient",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Updates an existing patient (partial or complete update)
        /// Actualiza un paciente existente (actualización parcial o completa)
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="updatePatientDto">Updated patient data</param>
        /// <returns>Updated patient</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseAPI>> UpdatePatient(int id, [FromBody] UpdatePatientDTO updatePatientDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ResponseAPI
                    {
                        Success = false,
                        Message = "Invalid patient ID",
                        Data = null
                    });
                }

                // Validate email format if provided
                if (!string.IsNullOrEmpty(updatePatientDto.Email))
                {
                    var emailAttribute = new EmailAddressAttribute();
                    if (!emailAttribute.IsValid(updatePatientDto.Email))
                    {
                        return BadRequest(new ResponseAPI
                        {
                            Success = false,
                            Message = "Invalid email format",
                            Data = null
                        });
                    }
                }

                // Validation with FluentValidation for Update
                var validationResult = await _updateValidator.ValidateAsync(updatePatientDto);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new
                    {
                        Property = e.PropertyName,
                        Error = e.ErrorMessage,
                        AttemptedValue = e.AttemptedValue
                    }).ToList();

                    return BadRequest(new ResponseAPI
                    {
                        Success = false,
                        Message = "Errores de validación",
                        Data = errors
                    });
                }

                _logger.LogInformation("Updating patient with ID: {PatientId}", id);

                var updatedPatient = await _patientService.UpdatePatient(id, updatePatientDto);

                return Ok(new ResponseAPI
                {
                    Success = true,
                    Message = "Patient updated successfully",
                    Data = updatedPatient
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating patient with ID: {PatientId}", id);
                return BadRequest(new ResponseAPI
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Patient not found when updating with ID: {PatientId}", id);
                return NotFound(new ResponseAPI
                {
                    Success = false,
                    Message = "Patient not found",
                    Data = null
                });
            }
            catch (Exception ex) when (ex.Message.Contains("already exists"))
            {
                _logger.LogWarning(ex, "Duplicate patient when updating with ID: {PatientId}", id);
                return Conflict(new ResponseAPI
                {
                    Success = false,
                    Message = "Another patient with the same document type and number already exists",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with ID: {PatientId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseAPI
                {
                    Success = false,
                    Message = "An error occurred while updating the patient",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Deletes a patient
        /// Elimina un paciente
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <returns>Deletion confirmation</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseAPI>> DeletePatient(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ResponseAPI
                    {
                        Success = false,
                        Message = "Invalid patient ID",
                        Data = null
                    });
                }

                _logger.LogInformation("Deleting patient with ID: {PatientId}", id);

                var result = await _patientService.DeletePatient(id);

                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseAPI
                    {
                        Success = false,
                        Message = "Failed to delete patient",
                        Data = null
                    });
                }

                return Ok(new ResponseAPI
                {
                    Success = true,
                    Message = "Patient deleted successfully",
                    Data = null
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when deleting patient with ID: {PatientId}", id);
                return BadRequest(new ResponseAPI
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning(ex, "Patient not found when deleting with ID: {PatientId}", id);
                return NotFound(new ResponseAPI
                {
                    Success = false,
                    Message = "Patient not found",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient with ID: {PatientId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseAPI
                {
                    Success = false,
                    Message = "An error occurred while deleting the patient",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Gets patients created after a specific date using stored procedure
        /// Obtiene pacientes creados después de una fecha específica usando procedimiento almacenado
        /// </summary>
        /// <param name="createdAfter">Date filter</param>
        /// <returns>List of patients created after the specified date</returns>
        [HttpGet("created-after")]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseAPI), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseAPI>> GetPatientsCreatedAfter([FromQuery] DateTime createdAfter)
        {
            try
            {
                if (createdAfter == default(DateTime))
                {
                    return BadRequest(new ResponseAPI
                    {
                        Success = false,
                        Message = "Invalid date parameter",
                        Data = null
                    });
                }

                _logger.LogInformation("Getting patients created after: {CreatedAfter}", createdAfter);

                var patients = await _patientService.GetPatientsCreatedAfterAsync(createdAfter);

                return Ok(new ResponseAPI
                {
                    Success = true,
                    Message = $"Found {patients.Count} patients created after {createdAfter:yyyy-MM-dd}",
                    Data = patients
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patients created after: {CreatedAfter}", createdAfter);
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseAPI
                {
                    Success = false,
                    Message = "An error occurred while retrieving patients",
                    Data = null
                });
            }
        }
    }
}