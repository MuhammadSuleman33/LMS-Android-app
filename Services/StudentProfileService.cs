using System.Data;
using Microsoft.Data.SqlClient;
using lms.android.Model;

namespace lms.android.Services;

public class StudentProfileService
{
    private const string _connectionString =
     "Server=10.0.2.2,1433;Database=LMS;User Id=lms_user;Password=Sallu@31; Encrypt=False;TrustServerCertificate=True;";

    // Fetch complete Student Profile context
    public async Task<StudentProfile?> GetStudentProfileAsync(int studentId)
    {
        
        return await Task.Run(async () =>
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand("dbo.usp_ManageStudentProfile", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Action", "GET");
                command.Parameters.AddWithValue("@StudentID", studentId);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new StudentProfile
                    {
                        StudentID = Convert.ToInt32(reader["StudentID"]),
                        RegistrationNo = reader["RegistrationNo"].ToString() ?? "",
                        FirstName = reader["FirstName"].ToString() ?? "", // Fixed: Changed from FullName to avoid database column confusion
                        LastName = reader["LastName"].ToString() ?? "",
                        DateofBirth = reader["DateofBirth"] != DBNull.Value ? Convert.ToDateTime(reader["DateofBirth"]) : null,
                        Gender = reader["Gender"].ToString() ?? "",
                        Email = reader["Email"].ToString() ?? "",
                        Phone = reader["Phone"].ToString() ?? "",

                        ContactID = reader["ContactID"] != DBNull.Value ? Convert.ToInt32(reader["ContactID"]) : null,
                        Address = reader["Address"].ToString() ?? "",
                        City = reader["City"].ToString() ?? "",
                        Country = reader["Country"].ToString() ?? "",
                        PostalCode = reader["PostalCode"].ToString() ?? "",
                        EmergencyContactName = reader["EmergencyContactName"].ToString() ?? "",
                        EmergencyContactPhone = reader["EmergencyContactPhone"].ToString() ?? "", // Fixed: Aligned to model specification variable

                        GuardianID = reader["GuardianID"] != DBNull.Value ? Convert.ToInt32(reader["GuardianID"]) : null,
                        GuardianName = reader["GuardianName"].ToString() ?? "",
                        Relationship = reader["Relationship"].ToString() ?? "",
                        GuardianPhone = reader["GuardianPhone"].ToString() ?? "",
                        GuardianEmail = reader["GuardianEmail"].ToString() ?? "",
                        GuardianAddress = reader["GuardianAddress"].ToString() ?? "",

                        StudentAcademicID = reader["StudentAcademicID"] != DBNull.Value ? Convert.ToInt32(reader["StudentAcademicID"]) : null,
                        ProgramID = reader["ProgramID"] != DBNull.Value ? Convert.ToInt32(reader["ProgramID"]) : null,
                        AdmissionDate = reader["AdmissionDate"] != DBNull.Value ? Convert.ToDateTime(reader["AdmissionDate"]) : null,
                        CurrentSemester = reader["CurrentSemester"].ToString() ?? "",
                        AcademicStatus = reader["AcademicStatus"].ToString() ?? "",
                        EnrollmentYear = reader["EnrollmentYear"] != DBNull.Value ? Convert.ToInt32(reader["EnrollmentYear"]) : null,
                        ExpectedGraduationDate = reader["ExpectedGraduationDate"] != DBNull.Value ? Convert.ToDateTime(reader["ExpectedGraduationDate"]) : null
                    };
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        });
    }

    // Allowed actions: 'UPDATE_CORE', 'UPSERT_CONTACT', 'UPSERT_GUARDIAN'
    public async Task<(bool Success, string Message)> UpdateStudentProfileAsync(string action, StudentProfile profile)
    {
        // FIX: Force network mutation writes off the UI thread
        return await Task.Run(async () =>
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Fixed: Added missing "usp_" prefix to point to your real database procedure definition
                await using var command = new SqlCommand("dbo.usp_ManageStudentProfile", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Action", action);
                command.Parameters.AddWithValue("@StudentID", profile.StudentID);

                // Core Profile Parameters
                command.Parameters.AddWithValue("@FirstName", (object)profile.FirstName ?? DBNull.Value);
                command.Parameters.AddWithValue("@LastName", (object)profile.LastName ?? DBNull.Value);
                command.Parameters.AddWithValue("@DateofBirth", (object)profile.DateofBirth ?? DBNull.Value);
                command.Parameters.AddWithValue("@Gender", (object)profile.Gender ?? DBNull.Value);
                command.Parameters.AddWithValue("@Email", (object)profile.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@Phone", (object)profile.Phone ?? DBNull.Value);

                // Contact Parameters
                command.Parameters.AddWithValue("@Address", (object)profile.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@City", (object)profile.City ?? DBNull.Value);
                command.Parameters.AddWithValue("@Country", (object)profile.Country ?? DBNull.Value);
                command.Parameters.AddWithValue("@PostalCode", (object)profile.PostalCode ?? DBNull.Value);
                command.Parameters.AddWithValue("@EmergencyContactName", (object)profile.EmergencyContactName ?? DBNull.Value);
                command.Parameters.AddWithValue("@EmergencyContactPhone", (object)profile.EmergencyContactPhone ?? DBNull.Value); // Fixed: Aligned syntax assignment

                // Guardian Parameters
                command.Parameters.AddWithValue("@GuardianName", (object)profile.GuardianName ?? DBNull.Value);
                command.Parameters.AddWithValue("@Relationship", (object)profile.Relationship ?? DBNull.Value);
                command.Parameters.AddWithValue("@GuardianPhone", (object)profile.GuardianPhone ?? DBNull.Value);
                command.Parameters.AddWithValue("@GuardianEmail", (object)profile.GuardianEmail ?? DBNull.Value);
                command.Parameters.AddWithValue("@GuardianAddress", (object)profile.GuardianAddress ?? DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return (true, "Profile section updated successfully!");
            }
            catch (SqlException ex)
            {
                return (false, $"Database error ({ex.Number}): {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        });
    }
}