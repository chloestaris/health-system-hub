using System.ServiceModel;
using HealthHub.API.Models;

namespace HealthHub.API.Services
{
    [ServiceContract(Namespace = "http://healthhub.api.services")]
    public interface IGovernmentHealthService
    {
        [OperationContract]
        Task<bool> ReportHealthRecordAsync(HealthRecord healthRecord);

        [OperationContract]
        Task<List<HealthRecord>> GetPatientHealthHistoryAsync(string nationalId);

        [OperationContract]
        Task<bool> ValidatePatientIdentityAsync(string nationalId, DateTime dateOfBirth);

        [OperationContract]
        Task<bool> ReportInfectiousDiseaseAsync(HealthRecord healthRecord, string diseaseCode);
    }
} 