using PatientsVIVABackend.DTO;
using PatientsVIVABackend.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsVIVABackend.BLL.PatientsService.Contract
{
    public interface IPatientService
    {
        // Function to create patients on DB
        public Task<PatientDTO> CreatePatient(CreatePatientDTO patient);

        // Function to get all patients from DB
        public Task<PagedResultDTO<PatientDTO>> GetPatientsPaginated(int page, int pageSize, string? name = null, string? documentNumber = null);

        // Function to get patient by id from DB
        public Task<PatientDTO?> GetPatientById(int id);

        // Function to update patient by id from DB
        public Task<PatientDTO> UpdatePatient(int id, UpdatePatientDTO patient);

        // Function to delete patient by id from DB
        public Task<bool> DeletePatient(int id);

        public Task<List<PatientDTO>> GetPatientsCreatedAfterAsync(DateTime date);
    }
}
