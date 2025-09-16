using AutoMapper;
using PatientsVIVABackend.DTO;
using PatientsVIVABackend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientsVIVABackend.Utility
{
    public class AutoMapperProfile : Profile
    {
        // Constructor to define the mapping profiles
        // Constructor para definir los perfiles de mapeo
        public AutoMapperProfile()
        {
            // Mapeo entre Patient y PatientDTO (bidireccional)
            CreateMap<Patient, PatientDTO>().ReverseMap();

            // Mapeo de CreatePatientDTO a Patient
            CreateMap<CreatePatientDTO, Patient>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore()) // Se genera automáticamente
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Se asigna en el servicio
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(src.BirthDate)));

            // Mapeo de UpdatePatientDTO a Patient (solo propiedades no nulas)
            CreateMap<UpdatePatientDTO, Patient>()
                .ForMember(dest => dest.PatientId, opt => opt.Ignore()) // No se actualiza
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // No se actualiza
                .ForMember(dest => dest.DocumentType, opt => opt.Condition(src => !string.IsNullOrEmpty(src.DocumentType)))
                .ForMember(dest => dest.DocumentNumber, opt => opt.Condition(src => !string.IsNullOrEmpty(src.DocumentNumber)))
                .ForMember(dest => dest.FirstName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.FirstName)))
                .ForMember(dest => dest.LastName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.LastName)))
                .ForMember(dest => dest.BirthDate, opt => opt.Condition(src => src.BirthDate.HasValue))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate.HasValue ? new DateOnly(src.BirthDate.Value.Year, src.BirthDate.Value.Month, src.BirthDate.Value.Day) : default(DateOnly)))
                .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => src.PhoneNumber != null))
                .ForMember(dest => dest.Email, opt => opt.Condition(src => src.Email != null));

            // Mapeo para Patient a CreatePatientDTO (útil para casos de prueba o conversiones inversas)
            CreateMap<Patient, CreatePatientDTO>()
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate.ToDateTime(TimeOnly.MinValue)));

            // Mapeo para Patient a UpdatePatientDTO (útil para casos de prueba o conversiones inversas)
            CreateMap<Patient, UpdatePatientDTO>()
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => (DateTime?)src.BirthDate.ToDateTime(TimeOnly.MinValue)));

           
        }
    }
}