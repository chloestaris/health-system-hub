using System.ServiceModel;
using System.Runtime.Serialization;
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
        Task<bool> ValidatePatientIdentity(
            [MessageParameter(Name = "nationalId")] string nationalId,
            [MessageParameter(Name = "dateOfBirth")] DateTime dateOfBirth);

        [OperationContract]
        Task<bool> ReportInfectiousDiseaseAsync(HealthRecord healthRecord, string diseaseCode);
    }
} 