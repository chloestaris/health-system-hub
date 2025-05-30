using HealthHub.API.Models;

namespace HealthHub.API.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(HealthHubContext context)
        {
            // Create database if it doesn't exist
            await context.Database.EnsureCreatedAsync();

            // Check if there's any data
            if (!context.Patients.Any())
            {
                // Add test patient
                var patient = new Patient
                {
                    FirstName = "John",
                    LastName = "Doe",
                    NationalId = "123456789",
                    DateOfBirth = DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc),
                    Address = "123 Main St, City, State 12345",
                    PhoneNumber = "555-0123",
                    Email = "john.doe@example.com"
                };

                context.Patients.Add(patient);
                await context.SaveChangesAsync();

                // Add health record
                var healthRecord = new HealthRecord
                {
                    PatientId = patient.Id,
                    RecordDate = DateTime.UtcNow,
                    Diagnosis = "Common Cold",
                    Treatment = "Rest and fluids",
                    Medications = "Over-the-counter cold medicine",
                    Notes = "Patient reported mild symptoms",
                    ProviderId = "DR123",
                    ProviderName = "Dr. Jane Smith"
                };

                context.HealthRecords.Add(healthRecord);
                await context.SaveChangesAsync();

                // Add insurance claim
                var claim = new InsuranceClaim
                {
                    HealthRecordId = healthRecord.Id,
                    ClaimNumber = "CLM123456",
                    ClaimDate = DateTime.UtcNow,
                    InsuranceProvider = "Health Insurance Co",
                    PolicyNumber = "POL987654",
                    Amount = 150.00M,
                    Status = "Pending",
                    Notes = "Initial claim submission"
                };

                context.InsuranceClaims.Add(claim);
                await context.SaveChangesAsync();

                // Add additional test data
                var patient2 = new Patient
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    NationalId = "987654321",
                    DateOfBirth = DateTime.SpecifyKind(new DateTime(1985, 6, 15), DateTimeKind.Utc),
                    Address = "456 Oak Ave, City, State 12345",
                    PhoneNumber = "555-0124",
                    Email = "jane.smith@example.com"
                };

                context.Patients.Add(patient2);
                await context.SaveChangesAsync();

                var healthRecord2 = new HealthRecord
                {
                    PatientId = patient2.Id,
                    RecordDate = DateTime.UtcNow.AddDays(-15),
                    Diagnosis = "Type 2 Diabetes",
                    Treatment = "Diet modification and medication",
                    Medications = "Metformin 500mg twice daily",
                    Notes = "Regular monitoring required",
                    ProviderId = "DR002",
                    ProviderName = "Dr. Sarah Johnson"
                };

                context.HealthRecords.Add(healthRecord2);
                await context.SaveChangesAsync();

                var claim2 = new InsuranceClaim
                {
                    HealthRecordId = healthRecord2.Id,
                    ClaimNumber = "CLM-20240115-87654321",
                    ClaimDate = DateTime.UtcNow.AddDays(-14),
                    InsuranceProvider = "MediCover",
                    PolicyNumber = "POL987654",
                    Amount = 200.00M,
                    Status = "Pending"
                };

                context.InsuranceClaims.Add(claim2);
                await context.SaveChangesAsync();
            }
        }
    }
} 