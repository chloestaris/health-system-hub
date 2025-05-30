using Microsoft.EntityFrameworkCore;
using HealthHub.API.Data;
using HealthHub.API.Models;

namespace HealthHub.API.Services
{
    public class GovernmentHealthService : IGovernmentHealthService
    {
        private readonly HealthHubContext _context;
        private readonly ILogger<GovernmentHealthService> _logger;

        public GovernmentHealthService(HealthHubContext context, ILogger<GovernmentHealthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ValidatePatientIdentity(string nationalId, DateTime dateOfBirth)
        {
            try
            {
                // Convert incoming date to UTC
                var utcDateOfBirth = DateTime.SpecifyKind(dateOfBirth.Date, DateTimeKind.Utc);
                
                _logger.LogInformation("Validating patient identity: NationalId={NationalId}, DOB={DateOfBirth}", 
                    nationalId, utcDateOfBirth);

                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.NationalId == nationalId && 
                                            p.DateOfBirth.Date == utcDateOfBirth.Date);

                return patient != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating patient identity with government system");
                throw;
            }
        }

        public async Task<bool> ReportHealthRecordAsync(HealthRecord healthRecord)
        {
            try
            {
                // In a real implementation, this would integrate with government health systems
                // For now, we'll just log the report and store it
                _logger.LogInformation("Health record reported to government system: Patient ID {PatientId}, Provider {Provider}",
                    healthRecord.PatientId, healthRecord.ProviderName);

                // Simulate async operation and store the report
                await Task.Delay(100); // Simulating external API call
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting health record to government system");
                throw;
            }
        }

        public async Task<List<HealthRecord>> GetPatientHealthHistoryAsync(string nationalId)
        {
            try
            {
                var records = await _context.HealthRecords
                    .Include(hr => hr.Patient)
                    .Where(hr => hr.Patient.NationalId == nationalId)
                    .OrderByDescending(hr => hr.RecordDate)
                    .ToListAsync();

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient health history from government system");
                throw;
            }
        }

        public async Task<bool> ReportInfectiousDiseaseAsync(HealthRecord healthRecord, string diseaseCode)
        {
            try
            {
                // In a real implementation, this would report to CDC or equivalent government health agency
                _logger.LogInformation(
                    "Infectious disease report submitted: Disease Code {DiseaseCode}, Patient ID {PatientId}",
                    diseaseCode, healthRecord.PatientId);

                // Store the report in the health record notes
                healthRecord.Notes += $"\nInfectious Disease Report - Code: {diseaseCode}, Date: {DateTime.UtcNow}";
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting infectious disease to government system");
                throw;
            }
        }
    }
} 