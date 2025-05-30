using Microsoft.EntityFrameworkCore;
using HealthHub.API.Data;
using HealthHub.API.Models;
using HealthHub.API.Models.DTOs;

namespace HealthHub.API.Services
{
    public class EhrService : IEhrService
    {
        private readonly HealthHubContext _context;
        private readonly ILogger<EhrService> _logger;

        public EhrService(HealthHubContext context, ILogger<EhrService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PatientDTO> GetPatientById(string nationalId)
        {
            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.NationalId == nationalId);
                
                if (patient == null)
                {
                    throw new Exception($"Patient with National ID {nationalId} not found.");
                }

                return new PatientDTO
                {
                    Id = patient.Id,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    NationalId = patient.NationalId,
                    DateOfBirth = patient.DateOfBirth,
                    Address = patient.Address,
                    PhoneNumber = patient.PhoneNumber,
                    Email = patient.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient with National ID: {NationalId}", nationalId);
                throw;
            }
        }

        public async Task<List<HealthRecord>> GetPatientHealthRecordsAsync(string nationalId)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.HealthRecords)
                    .FirstOrDefaultAsync(p => p.NationalId == nationalId);

                if (patient == null)
                {
                    throw new Exception($"Patient with National ID {nationalId} not found.");
                }

                return patient.HealthRecords.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health records for patient with National ID: {NationalId}", nationalId);
                throw;
            }
        }

        public async Task<HealthRecord> AddHealthRecordAsync(HealthRecord healthRecord)
        {
            try
            {
                _context.HealthRecords.Add(healthRecord);
                await _context.SaveChangesAsync();
                return healthRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding health record for patient ID: {PatientId}", healthRecord.PatientId);
                throw;
            }
        }

        public async Task<bool> UpdateHealthRecordAsync(HealthRecord healthRecord)
        {
            try
            {
                var existingRecord = await _context.HealthRecords.FindAsync(healthRecord.Id);
                if (existingRecord == null)
                {
                    return false;
                }

                _context.Entry(existingRecord).CurrentValues.SetValues(healthRecord);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating health record ID: {Id}", healthRecord.Id);
                throw;
            }
        }

        public async Task<bool> DeleteHealthRecordAsync(int healthRecordId)
        {
            try
            {
                var healthRecord = await _context.HealthRecords.FindAsync(healthRecordId);
                if (healthRecord == null)
                {
                    return false;
                }

                _context.HealthRecords.Remove(healthRecord);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting health record ID: {Id}", healthRecordId);
                throw;
            }
        }
    }
} 