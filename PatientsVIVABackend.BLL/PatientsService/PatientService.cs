using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientsVIVABackend.BLL.PatientsService.Contract;
using PatientsVIVABackend.DAL.Repositories;
using PatientsVIVABackend.DTO;
using PatientsVIVABackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PatientsVIVABackend.BLL.PatientsService
{
    public class PatientService : IPatientService
    {
        private readonly IGenericRepository<Patient> _patientRepository;
        private readonly ILogger<PatientService> _logger;
        private readonly IMapper _mapper;

        public PatientService(
            IGenericRepository<Patient> patientRepository,
            ILogger<PatientService> logger,
            IMapper mapper
        )
        {
            _patientRepository = patientRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves a paginated list of patients with optional filtering.
        /// Obtiene una lista paginada de pacientes con filtrado opcional.
        /// </summary>
        public async Task<PagedResultDTO<PatientDTO>> GetPatientsPaginated(int page, int pageSize, string? name = null, string? documentNumber = null)
        {
            PagedResultDTO<PatientDTO> result = new PagedResultDTO<PatientDTO>();
            try
            {
                // Validate parameters 
                if (page <= 0) page = 1;
                if (pageSize <= 0) pageSize = 10;
                // Limit page size to 100 max
                if (pageSize > 100) pageSize = 100;

                // Get all patients from DB with filters if any
                IQueryable<Patient> queryAllPatients = await _patientRepository.Query();

                // Validate if name filter is provided
                if (!string.IsNullOrEmpty(name))
                {
                    queryAllPatients = queryAllPatients.Where(p => (p.FirstName.ToLower() + " " + p.LastName.ToLower()).Contains(name.ToLower()));
                }

                // Validate if document number filter is provided
                if (!string.IsNullOrEmpty(documentNumber))
                {
                    queryAllPatients = queryAllPatients.Where(p => p.DocumentNumber.Contains(documentNumber));
                }

                // Get total count of patients
                int totalPatients = queryAllPatients.Count();

                // Apply pagination
                var patients = await queryAllPatients
                    .OrderBy(p => p.PatientId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map entities to DTOs
                var patientDTOs = _mapper.Map<List<PatientDTO>>(patients);

                // Prepare paged result
                result.Data = patientDTOs;
                result.TotalRecords = totalPatients;
                result.Page = page;
                result.PageSize = pageSize;


                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific patient by their unique identifier.
        /// Obtiene un paciente específico por su identificador único.
        /// </summary>
        public async Task<PatientDTO?> GetPatientById(int id)
        {
            try
            {
                // Validate id
                if (id <= 0) throw new ArgumentException("Invalid patient ID.");

                // Get patient by id from DB
                var patient = await _patientRepository.Get(p => p.PatientId == id);

                // Map entity to DTO and return
                return patient == null ? null : _mapper.Map<PatientDTO>(patient);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a new patient in the system.
        /// Crea un nuevo paciente en el sistema.
        /// </summary>
        public async Task<PatientDTO> CreatePatient(CreatePatientDTO patient)
        {
            try
            {
                // Validate if patient with same document type and number already exists
                // Validar si ya existe un paciente con el mismo tipo y número de documento
                _logger.LogInformation("Checking for existing patient with DocumentType: {DocumentType} and DocumentNumber: {DocumentNumber}", patient.DocumentType, patient.DocumentNumber);
                var existingPatient = await _patientRepository.Get(p => p.DocumentNumber == patient.DocumentNumber);
                if (existingPatient != null)
                {
                    throw new Exception("A patient with the same document type and number already exists.");
                }

                // Map DTO to entity
                var patientEntity = _mapper.Map<Patient>(patient);
                patientEntity.CreatedAt = DateTime.UtcNow;

                //Log the creation time
                _logger.LogInformation("Creating new patient with DocumentType: {DocumentType}, DocumentNumber: {DocumentNumber} at {CreatedAt}", patientEntity.DocumentType, patientEntity.DocumentNumber, patientEntity.CreatedAt);

                // Save to database
                var createdPatient = await _patientRepository.AddAsync(patientEntity);

                // Map entity back to DTO and return 
                return _mapper.Map<PatientDTO>(createdPatient);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates an existing patient's information with partial or complete data.
        /// Actualiza la información de un paciente existente con datos parciales o completos.
        /// </summary>
        public async Task<PatientDTO> UpdatePatient(int id, UpdatePatientDTO patient)
        {
            try
            {
                _logger.LogInformation("Updating patient with ID: {PatientId}", id);
                // Validate id
                if (id <= 0) throw new ArgumentException("Invalid patient ID.");

                // Get existing patient from DB
                var existingPatient = await _patientRepository.Get(p => p.PatientId == id) ?? throw new Exception("Patient not found.");

                _logger.LogInformation("Existing patient found: {ExistingPatient}", existingPatient);
                // Check for duplicate document type and number if they are being updated
                if (!string.IsNullOrEmpty(patient.DocumentType) || !string.IsNullOrEmpty(patient.DocumentNumber))
                {
                    var newDocType = patient.DocumentType ?? existingPatient.DocumentType;
                    var newDocNumber = patient.DocumentNumber ?? existingPatient.DocumentNumber;

                    var duplicatedPatient = await _patientRepository.Get(p => p.PatientId != id && p.DocumentType == newDocType && p.DocumentNumber == newDocNumber);
                    if (duplicatedPatient != null)
                    {
                        throw new Exception("Another patient with the same document type and number already exists.");
                    }
                }
                _logger.LogInformation("No duplicate patient found with the same DocumentType and DocumentNumber.");

                // Map updated fields from DTO to entity
                _mapper.Map(patient, existingPatient);
                // Save changes to database
                var updatedPatient = await _patientRepository.UpdateAsync(existingPatient);
                _logger.LogInformation("Patient with ID: {PatientId} updated successfully.", id);
                return _mapper.Map<PatientDTO>(updatedPatient);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes a patient from the system.
        /// Elimina un paciente del sistema.
        /// </summary>
        public async Task<bool> DeletePatient(int id)
        {
            try
            {
                // Validate id
                if (id <= 0) throw new ArgumentException("Invalid patient ID.");
                // Validate if patient exists
                _logger.LogInformation("Checking for existing patient with ID: {PatientId}", id);
                Patient existingPatient = await _patientRepository.Get(p => p.PatientId == id) ?? throw new Exception("Patient not found.");
                // Delete patient
                _logger.LogInformation("Deleting patient with ID: {PatientId}", id);
                return await _patientRepository.DeleteAsync(existingPatient);
            }
            catch
            {
                throw;
            }
        }

        // <summary>
        /// Retrieves patients created after a specific date using a stored procedure.
        /// Obtiene pacientes creados después de una fecha específica usando un procedimiento almacenado.
        /// </summary>
        public async Task<List<PatientDTO>> GetPatientsCreatedAfterAsync(DateTime date)
        {
            try
            {
                // Validate date
                var parameters = new[]
                {
                    new SqlParameter("@CreatedAfter", date)  // El nombre original no importa, se renombrará automáticamente
                };
                // Execute stored procedure
                var patients = await _patientRepository.ExecuteStoredProcedureAsync<Patient>("GetPatientsCreatedAfter", parameters);

                // Map entities to DTOs
                var patientDTOs = _mapper.Map<List<PatientDTO>>(patients);
                return patientDTOs;
            }
            catch
            {
                throw;
            }
        }

    }
}