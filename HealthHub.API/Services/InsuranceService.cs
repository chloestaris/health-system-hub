using Microsoft.EntityFrameworkCore;
using HealthHub.API.Data;
using HealthHub.API.Models;
using HealthHub.API.Models.DTOs;

namespace HealthHub.API.Services
{
    public class InsuranceService : IInsuranceService
    {
        private readonly HealthHubContext _context;
        private readonly ILogger<InsuranceService> _logger;

        public InsuranceService(HealthHubContext context, ILogger<InsuranceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<InsuranceClaimDTO> GetClaimStatus(string claimNumber)
        {
            _logger.LogInformation("Searching for claim with number: {ClaimNumber}", claimNumber);

            var claim = await _context.InsuranceClaims
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);

            if (claim == null)
            {
                _logger.LogWarning("Claim with number {ClaimNumber} not found in database", claimNumber);
                throw new Exception($"Claim with number {claimNumber} not found.");
            }

            _logger.LogInformation("Found claim: ID={Id}, Status={Status}", claim.Id, claim.Status);

            return new InsuranceClaimDTO
            {
                Id = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                ClaimDate = claim.ClaimDate,
                InsuranceProvider = claim.InsuranceProvider,
                PolicyNumber = claim.PolicyNumber,
                Amount = claim.Amount,
                Status = claim.Status,
                Notes = claim.Notes
            };
        }

        public async Task<InsuranceClaimDTO> SubmitClaimAsync(InsuranceClaimDTO claimDto)
        {
            var claim = new InsuranceClaim
            {
                ClaimNumber = claimDto.ClaimNumber,
                ClaimDate = claimDto.ClaimDate,
                InsuranceProvider = claimDto.InsuranceProvider,
                PolicyNumber = claimDto.PolicyNumber,
                Amount = claimDto.Amount,
                Status = "Submitted",
                Notes = claimDto.Notes
            };

            _context.InsuranceClaims.Add(claim);
            await _context.SaveChangesAsync();

            claimDto.Id = claim.Id;
            claimDto.Status = claim.Status;
            return claimDto;
        }

        public async Task<List<InsuranceClaim>> GetPatientClaimsAsync(string nationalId)
        {
            try
            {
                var claims = await _context.InsuranceClaims
                    .Include(c => c.HealthRecord)
                    .ThenInclude(hr => hr.Patient)
                    .Where(c => c.HealthRecord.Patient.NationalId == nationalId)
                    .ToListAsync();

                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving claims for patient with National ID: {NationalId}", nationalId);
                throw;
            }
        }

        public async Task<bool> UpdateClaimStatusAsync(string claimNumber, string status)
        {
            var claim = await _context.InsuranceClaims
                .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);

            if (claim == null)
            {
                return false;
            }

            claim.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateClaimNumber()
        {
            return $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
    }
} 