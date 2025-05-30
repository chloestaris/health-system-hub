using System.ServiceModel;
using HealthHub.API.Models;
using HealthHub.API.Models.DTOs;

namespace HealthHub.API.Services
{
    [ServiceContract(Namespace = "http://healthhub.api.services")]
    public interface IInsuranceService
    {
        [OperationContract]
        Task<InsuranceClaimDTO> GetClaimStatus(string claimNumber);

        [OperationContract]
        Task<InsuranceClaimDTO> SubmitClaimAsync(InsuranceClaimDTO claim);

        [OperationContract]
        Task<List<InsuranceClaim>> GetPatientClaimsAsync(string nationalId);

        [OperationContract]
        Task<bool> UpdateClaimStatusAsync(string claimNumber, string status);
    }
} 