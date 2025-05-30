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
            if (context.Patients.Any())
            {
                return; // Database has been seeded
            }

            // Add test patients
            var patient1 = new Patient
            {
                FirstName = "John",
                LastName = "Doe",
                NationalId = "123456789",
                DateOfBirth = new DateTime(1990, 1, 1),
                Address = "123 Main St, City, State 12345",
                PhoneNumber = "555-0123",
                Email = "john.doe@example.com"
            };

            var patient2 = new Patient
            {
                FirstName = "Jane",
                LastName = "Smith",
                NationalId = "987654321",
                DateOfBirth = new DateTime(1985, 6, 15),
                Address = "456 Oak Ave, City, State 12345",
                PhoneNumber = "555-0124",
                Email = "jane.smith@example.com"
            };

            context.Patients.AddRange(patient1, patient2);
            await context.SaveChangesAsync();

            // Add health records
            var healthRecord1 = new HealthRecord
            {
                PatientId = patient1.Id,
                RecordDate = DateTime.UtcNow.AddDays(-30),
                Diagnosis = "Hypertension",
                Treatment = "Prescribed ACE inhibitors",
                Medications = "Lisinopril 10mg daily",
                Notes = "Patient responding well to treatment",
                ProviderId = "DR001",
                ProviderName = "Dr. Robert Wilson"
            };

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

            context.HealthRecords.AddRange(healthRecord1, healthRecord2);
            await context.SaveChangesAsync();

            // Add insurance claims
            var claim1 = new InsuranceClaim
            {
                HealthRecordId = healthRecord1.Id,
                ClaimNumber = "CLM-20240101-12345678",
                ClaimDate = DateTime.UtcNow.AddDays(-29),
                InsuranceProvider = "HealthCare Plus",
                PolicyNumber = "POL123456",
                Amount = 150.00M,
                Status = "Approved"
            };

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

            context.InsuranceClaims.AddRange(claim1, claim2);
            await context.SaveChangesAsync();
        }
    }
} 