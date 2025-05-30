using System.ServiceModel;
using HealthHub.API.Models;
using HealthHub.API.Models.DTOs;

namespace HealthHub.API.Services
{
    [ServiceContract(Namespace = "http://healthhub.api.services")]
    public interface IEhrService
    {
        [OperationContract]
        Task<PatientDTO> GetPatientById(string nationalId);

        [OperationContract]
        Task<List<HealthRecord>> GetPatientHealthRecordsAsync(string nationalId);

        [OperationContract]
        Task<HealthRecord> AddHealthRecordAsync(HealthRecord healthRecord);

        [OperationContract]
        Task<bool> UpdateHealthRecordAsync(HealthRecord healthRecord);

        [OperationContract]
        Task<bool> DeleteHealthRecordAsync(int healthRecordId);
    }
} 