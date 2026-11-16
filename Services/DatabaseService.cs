using Microsoft.Data.SqlClient;
using System.Data;
using lms.android.Model;

namespace lms.android.Services;

public class DatabaseService
{
    private const string _connectionString =
    "Server=10.0.2.2,1433;Database=LMS;User Id=lms_user;Password=Sallu@31;" +
    "Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;Pooling=True;Min Pool Size=1;Max Pool Size=20;";

    // 1. Implementation for dbo.usp_GETUserForLogin
    public async Task<UserData?> GetUserForLoginAsync(string usernameOrEmail, string selectedRoleName)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_GETUserForLogin", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UsernameOrEmail", usernameOrEmail);
            command.Parameters.AddWithValue("@SelectedRoleName", selectedRoleName);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserData
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    Email = reader["Email"].ToString() ?? "",
                    Username = reader["Username"].ToString() ?? "",
                    FullName = reader["FullName"].ToString() ?? "",
                    Phone = reader["Phone"].ToString() ?? "",
                    PasswordHash = reader["PasswordHash"].ToString() ?? "",
                    Status = reader["Status"].ToString() ?? "",
                    RoleName = reader["RoleName"].ToString() ?? "",
                    VerificationStatus = reader["VerificationStatus"] is DBNull
                        ? "Pending"
                        : reader["VerificationStatus"].ToString() ?? "Pending",
                    LockedRoleName = reader["LockedRoleName"] is DBNull
                        ? ""
                        : reader["LockedRoleName"].ToString() ?? ""
                };
            }
            return null;
        }
        catch (SqlException ex) when (ex.Number == 50026 || ex.Number == 50028)
        {
            throw new UnauthorizedAccessException(ex.Message);
        }
        catch (SqlException ex) when (ex.Number == 50005)
        {
            throw new UnauthorizedAccessException("Access Denied: The selected role is not assigned to this account.");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string> GetVerificationStatusAsync(int userId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "SELECT VerificationStatus FROM dbo.UserAccounts WHERE UserID = @UserID", connection);
            command.Parameters.AddWithValue("@UserID", userId);

            var result = await command.ExecuteScalarAsync();
            return result == null || result is DBNull ? "Pending" : result.ToString() ?? "Pending";
        }
        catch
        {
            return "Pending";
        }
    }

    public async Task LockUserRoleAsync(int userId, string roleName)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand("dbo.usp_LockUserRole", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserID", userId);
        command.Parameters.AddWithValue("@RoleName", roleName);

        await command.ExecuteNonQueryAsync();
    }

    // 2. Implementation for dbo.usp_UpdateLastLogin
    public async Task UpdateLastLoginAsync(int userId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_UpdateLastLogin", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserID", userId);

            await command.ExecuteNonQueryAsync();
        }
        catch { /* Log error or ignore silent background update failure */ }
    }

    // 3. Implementation for dbo.usp_RegisterUser
    public async Task<(bool Success, string Message)> RegisterUserAsync(string username, string email, string phone, string plainPassword, string fullName)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_RegisterUser", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@PhoneNumber", phone);
            command.Parameters.AddWithValue("@PasswordHash", plainPassword);
            command.Parameters.AddWithValue("@FullName", fullName);

            var result = await command.ExecuteScalarAsync();
            return (true, "Registration Successful!");
        }
        catch (SqlException ex) when (ex.Number >= 50001 && ex.Number <= 50003)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, $"Database error: {ex.Message}");
        }
    }

    // 4. Implementation for dbo.usp_UpdatePasswordResetTokens
    public async Task<(int UserID, long ResetTokenID)?> UpdatePasswordResetTokensAsync(string email, string otpHash)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_UpdatePasswordResetTokens", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@OTPHash", otpHash);
            command.Parameters.AddWithValue("@ExpiryMinutes", 5);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (Convert.ToInt32(reader["UserID"]), Convert.ToInt64(reader["ResetTokenID"]));
            }
            return null;
        }
        catch { throw; }
    }

    // 5. Implementation for dbo.usp_GetActiveResetToken
    public async Task<(long ResetTokenID, int Attempts, string OTPHash, int UserID)?> GetActiveResetTokenAsync(string email)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_GetActiveResetToken", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Email", email);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (
                    Convert.ToInt64(reader["ResetTokenID"]),
                    Convert.ToInt32(reader["Attempts"]),
                    reader["OTPHash"].ToString() ?? "",
                    Convert.ToInt32(reader["UserID"])
                );
            }
            return null;
        }
        catch { throw; }
    }

    // 6. Implementation for dbo.usp_AddResetAttempt
    public async Task AddResetAttemptAsync(long resetTokenID)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_AddResetAttempt", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@ResetTokenID", resetTokenID);

            await command.ExecuteNonQueryAsync();
        }
        catch { throw; }
    }

    // 7. Implementation for dbo.usp_CompletePasswordReset
    public async Task<(bool Success, string Message)> CompletePasswordResetAsync(long resetTokenID, string newPasswordHash)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_CompletePasswordReset", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@ResetTokenID", resetTokenID);
            command.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);

            await command.ExecuteNonQueryAsync();
            return (true, "Password reset completed successfully!");
        }
        catch (SqlException ex) when (ex.Number == 50004)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    // 8. Implementation for dbo.usp_GetUserProfileByID (Dynamic Role Profile)
    public async Task<UserAccountProfile?> GetUserAccountProfileAsync(int userId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_GetUserProfileByID", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", userId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserAccountProfile
                {
                    UserID = Convert.ToInt32(reader["UserID"]),
                    Username = reader["Username"]?.ToString() ?? "",
                    FullName = reader["FullName"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? "",
                    AccountStatus = reader["AccountStatus"]?.ToString() ?? "",
                    VerificationStatus = reader["VerificationStatus"] is DBNull
                        ? "Pending"
                        : reader["VerificationStatus"]?.ToString() ?? "Pending",
                    RoleID = Convert.ToInt32(reader["RoleID"]),
                    RoleName = reader["RoleName"]?.ToString() ?? "",
                    RoleDescription = reader["RoleDescription"]?.ToString() ?? "",
                    LatestReviewComment = reader["LatestReviewComment"]?.ToString() ?? ""
                };
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // 9. Implementation for dbo.usp_GetPendingProfilesForReviewer
    public async Task<List<PendingVerificationItem>> GetPendingProfilesForReviewerAsync(string reviewerRole)
    {
        var list = new List<PendingVerificationItem>();
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_GetPendingProfilesForReviewer", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ReviewerActiveRole", reviewerRole);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new PendingVerificationItem
                {
                    TargetUserID = Convert.ToInt32(reader["TargetUserID"]),
                    FullName = reader["FullName"]?.ToString() ?? "",
                    Username = reader["Username"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    ProfileType = reader["ProfileType"]?.ToString() ?? "",
                    VerificationStatus = reader["VerificationStatus"]?.ToString() ?? "Pending"
                });
            }
        }
        catch
        {
            // Return empty list if table/procedure isn't configured yet
        }
        return list;
    }

    // 10. Implementation for dbo.ProcessProfileVerification
    public async Task<(bool Success, string Message)> ProcessProfileVerificationAsync(
        int targetUserId, string profileType, int reviewerUserId, string reviewerRole, string newStatus, string? comments = null)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.ProcessProfileVerification", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@TargetUserID", targetUserId);
            command.Parameters.AddWithValue("@ProfileType", profileType);
            command.Parameters.AddWithValue("@ReviewerUserID", reviewerUserId);
            command.Parameters.AddWithValue("@ReviewerActiveRole", reviewerRole);
            command.Parameters.AddWithValue("@NewStatus", newStatus);
            command.Parameters.AddWithValue("@Comments", string.IsNullOrWhiteSpace(comments) ? DBNull.Value : comments);

            await command.ExecuteNonQueryAsync();
            return (true, $"Profile {newStatus.ToLower()} successfully.");
        }
        catch (SqlException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }



    // Fetch Teacher Profile from dbo.usp_GetTeacherProfileByID
    public async Task<TeacherProfileModel?> GetTeacherProfileAsync(int userId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_GetTeacherProfileByID", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", userId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new TeacherProfileModel
                {
                    TeacherID = Convert.ToInt32(reader["TeacherID"]),
                    EmployeeNo = reader["EmployeeNo"]?.ToString() ?? "",
                    FirstName = reader["FirstName"]?.ToString() ?? "",
                    LastName = reader["LastName"]?.ToString() ?? "",
                    DateOfBirth = reader["DateOfBirth"] is DBNull ? new DateTime(1990, 1, 1) : Convert.ToDateTime(reader["DateOfBirth"]),
                    Gender = reader["Gender"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? "",
                    Status = reader["Status"]?.ToString() ?? "Active",

                    DepartmentID = reader["DepartmentID"] is DBNull ? 1 : Convert.ToInt32(reader["DepartmentID"]),
                    Designation = reader["Designation"]?.ToString() ?? "Lecturer",
                    JoiningDate = reader["JoiningDate"] is DBNull ? DateTime.Today : Convert.ToDateTime(reader["JoiningDate"]),
                    Specialization = reader["Specialization"]?.ToString() ?? "",
                    OfficeLocation = reader["OfficeLocation"]?.ToString() ?? "",
                    Qualification = reader["Qualification"]?.ToString() ?? "",
                    TeacherAcademicID = reader["TeacherAcademicID"] is DBNull ? 0 : Convert.ToInt32(reader["TeacherAcademicID"]),
                    ExperienceYears = reader["ExperienceYears"] is DBNull ? 0 : Convert.ToInt32(reader["ExperienceYears"]),

                    ExperienceID = reader["ExperienceID"] is DBNull ? 0 : Convert.ToInt32(reader["ExperienceID"]),
                    OrganizationName = reader["OrganizationName"]?.ToString() ?? "",
                    PreviousJobTitle = reader["PreviousJobTitle"]?.ToString() ?? "",
                    ExpStartDate = reader["ExpStartDate"] is DBNull ? DateTime.Today.AddYears(-1) : Convert.ToDateTime(reader["ExpStartDate"]),
                    ExpEndDate = reader["ExpEndDate"] is DBNull ? DateTime.Today : Convert.ToDateTime(reader["ExpEndDate"]),

                    AssignedCoursesCount = reader["AssignedCoursesCount"] is DBNull ? 0 : Convert.ToInt32(reader["AssignedCoursesCount"]),
                    AssignedCoursesSummary = reader["AssignedCoursesSummary"]?.ToString() ?? "No courses assigned yet"
                };
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // Save/Update Teacher Profile sections via dbo.usp_UpsertTeacherProfile
    public async Task<(bool Success, string Message)> UpdateTeacherProfileAsync(string action, TeacherProfileModel model)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_UpsertTeacherProfile", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Action", action);
            command.Parameters.AddWithValue("@TeacherID", model.TeacherID);
            command.Parameters.AddWithValue("@EmployeeNo", model.EmployeeNo ?? "");
            command.Parameters.AddWithValue("@FirstName", model.FirstName ?? "");
            command.Parameters.AddWithValue("@LastName", model.LastName ?? "");
            command.Parameters.AddWithValue("@DateOfBirth", model.DateOfBirth);
            command.Parameters.AddWithValue("@Gender", model.Gender ?? "");
            command.Parameters.AddWithValue("@Email", model.Email ?? "");
            command.Parameters.AddWithValue("@Phone", model.Phone ?? "");
            command.Parameters.AddWithValue("@Status", model.Status ?? "Active");

            command.Parameters.AddWithValue("@DepartmentID", model.DepartmentID <= 0 ? 1 : model.DepartmentID);
            command.Parameters.AddWithValue("@Designation", model.Designation ?? "Lecturer");
            command.Parameters.AddWithValue("@JoiningDate", model.JoiningDate);
            command.Parameters.AddWithValue("@Specialization", model.Specialization ?? "");
            command.Parameters.AddWithValue("@OfficeLocation", model.OfficeLocation ?? "");
            command.Parameters.AddWithValue("@Qualification", model.Qualification ?? "");
            command.Parameters.AddWithValue("@ExperienceYears", model.ExperienceYears);

            command.Parameters.AddWithValue("@OrganizationName", model.OrganizationName ?? "");
            command.Parameters.AddWithValue("@PreviousJobTitle", model.PreviousJobTitle ?? "");
            command.Parameters.AddWithValue("@ExpStartDate", model.ExpStartDate);
            command.Parameters.AddWithValue("@ExpEndDate", model.ExpEndDate);

            await command.ExecuteNonQueryAsync();
            return (true, "Teacher profile updated successfully!");
        }
        catch (Exception ex)
        {
            return (false, $"Update failed: {ex.Message}");
        }
    }


    // Fetch HOD Profile from dbo.usp_GetHODProfileByID
    public async Task<HODProfileModel?> GetHODProfileAsync(int userId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_GetHODProfileByID", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", userId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new HODProfileModel
                {
                    HODID = reader["HODID"] is DBNull ? userId : Convert.ToInt32(reader["HODID"]),
                    FullName = reader["FullName"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? "",
                    DepartmentID = reader["DepartmentID"] is DBNull ? 1 : Convert.ToInt32(reader["DepartmentID"]),
                    OfficeRoomNo = reader["OfficeRoomNo"]?.ToString() ?? "",
                    AppointedDate = reader["AppointedDate"] is DBNull ? DateTime.Today : Convert.ToDateTime(reader["AppointedDate"]),
                    TenureExpiryDate = reader["TenureExpiryDate"] is DBNull ? DateTime.Today.AddYears(3) : Convert.ToDateTime(reader["TenureExpiryDate"]),
                    Notes = reader["Notes"]?.ToString() ?? "",
                    DepartmentTeachersCount = reader["DepartmentTeachersCount"] is DBNull ? 0 : Convert.ToInt32(reader["DepartmentTeachersCount"]),
                    DepartmentCoursesCount = reader["DepartmentCoursesCount"] is DBNull ? 0 : Convert.ToInt32(reader["DepartmentCoursesCount"])
                };
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // Save/Update HOD Profile via dbo.usp_UpsertHODProfile
    public async Task<(bool Success, string Message)> UpdateHODProfileAsync(int userId, HODProfileModel model)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_UpsertHODProfile", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserID", userId);
            command.Parameters.AddWithValue("@HODID", model.HODID <= 0 ? userId : model.HODID);
            command.Parameters.AddWithValue("@FullName", model.FullName ?? "");
            command.Parameters.AddWithValue("@Email", model.Email ?? "");
            command.Parameters.AddWithValue("@Phone", model.Phone ?? "");
            command.Parameters.AddWithValue("@DepartmentID", model.DepartmentID <= 0 ? 1 : model.DepartmentID);
            command.Parameters.AddWithValue("@OfficeRoomNo", model.OfficeRoomNo ?? "");
            command.Parameters.AddWithValue("@AppointedDate", model.AppointedDate);
            command.Parameters.AddWithValue("@TenureExpiryDate", model.TenureExpiryDate);
            command.Parameters.AddWithValue("@Notes", model.Notes ?? "");

            await command.ExecuteNonQueryAsync();
            return (true, "HOD profile updated successfully!");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to update HOD profile: {ex.Message}");
        }
    }



    // Fetch Competent Authority Profile from dbo.usp_GetCompetentAuthorityProfileByID
    public async Task<CompetentAuthorityProfileModel?> GetCompetentAuthorityProfileAsync(int userId)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_GetCompetentAuthorityProfileByID", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", userId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CompetentAuthorityProfileModel
                {
                    AuthorityID = reader["AuthorityID"] is DBNull ? userId : Convert.ToInt32(reader["AuthorityID"]),
                    AuthorityCode = reader["AuthorityCode"]?.ToString() ?? "",
                    FullName = reader["FullName"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Phone = reader["Phone"]?.ToString() ?? "",
                    Designation = reader["Designation"]?.ToString() ?? "Dean / Competent Authority",
                    JurisdictionScope = reader["JurisdictionScope"]?.ToString() ?? "University-Wide",
                    SigningAuthorityLevel = reader["SigningAuthorityLevel"]?.ToString() ?? "Tier-1 Final Approver",
                    Qualification = reader["Qualification"]?.ToString() ?? "",
                    SecretariatOfficeLocation = reader["SecretariatOfficeLocation"]?.ToString() ?? "",
                    ProtocolExtension = reader["ProtocolExtension"]?.ToString() ?? "",
                    AppointedDate = reader["AppointedDate"] is DBNull ? DateTime.Today : Convert.ToDateTime(reader["AppointedDate"]),
                    TenureExpiryDate = reader["TenureExpiryDate"] is DBNull ? DateTime.Today.AddYears(3) : Convert.ToDateTime(reader["TenureExpiryDate"]),
                    CommitteeMemberships = reader["CommitteeMemberships"]?.ToString() ?? "",
                    MandateNotes = reader["MandateNotes"]?.ToString() ?? "",
                    Status = reader["Status"]?.ToString() ?? "Active",
                    TotalDepartmentsCount = reader["TotalDepartmentsCount"] is DBNull ? 0 : Convert.ToInt32(reader["TotalDepartmentsCount"]),
                    TotalHODsCount = reader["TotalHODsCount"] is DBNull ? 0 : Convert.ToInt32(reader["TotalHODsCount"]),
                    TotalApprovalsProcessed = reader["TotalApprovalsProcessed"] is DBNull ? 0 : Convert.ToInt32(reader["TotalApprovalsProcessed"])
                };
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // Save/Update Competent Authority Profile via dbo.usp_UpsertCompetentAuthorityProfile
    public async Task<(bool Success, string Message)> UpdateCompetentAuthorityProfileAsync(int userId, CompetentAuthorityProfileModel model)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("dbo.usp_UpsertCompetentAuthorityProfile", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@UserID", userId);
            command.Parameters.AddWithValue("@AuthorityCode", model.AuthorityCode ?? "");
            command.Parameters.AddWithValue("@FullName", model.FullName ?? "");
            command.Parameters.AddWithValue("@Email", model.Email ?? "");
            command.Parameters.AddWithValue("@Phone", model.Phone ?? "");
            command.Parameters.AddWithValue("@Designation", model.Designation ?? "");
            command.Parameters.AddWithValue("@JurisdictionScope", model.JurisdictionScope ?? "");
            command.Parameters.AddWithValue("@SigningAuthorityLevel", model.SigningAuthorityLevel ?? "");
            command.Parameters.AddWithValue("@Qualification", model.Qualification ?? "");
            command.Parameters.AddWithValue("@SecretariatOfficeLocation", model.SecretariatOfficeLocation ?? "");
            command.Parameters.AddWithValue("@ProtocolExtension", model.ProtocolExtension ?? "");
            command.Parameters.AddWithValue("@AppointedDate", model.AppointedDate);
            command.Parameters.AddWithValue("@TenureExpiryDate", model.TenureExpiryDate);
            command.Parameters.AddWithValue("@CommitteeMemberships", model.CommitteeMemberships ?? "");
            command.Parameters.AddWithValue("@MandateNotes", model.MandateNotes ?? "");
            command.Parameters.AddWithValue("@Status", model.Status ?? "Active");

            await command.ExecuteNonQueryAsync();
            return (true, "Competent Authority profile updated successfully!");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to update Competent Authority profile: {ex.Message}");
        }
    }



    // <--- Fetch Super Admin Profile from dbo.usp_GetSuperAdminProfileByID
        public async Task<SuperAdminProfileModel?> GetSuperAdminProfileAsync(int userId)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand("dbo.usp_GetSuperAdminProfileByID", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@UserID", userId);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new SuperAdminProfileModel
                    {
                        AdminID = reader["AdminID"] is DBNull ? userId : Convert.ToInt32(reader["AdminID"]),
                        AdminCode = reader["AdminCode"]?.ToString() ?? "",
                        FullName = reader["FullName"]?.ToString() ?? "",
                        Email = reader["Email"]?.ToString() ?? "",
                        Phone = reader["Phone"]?.ToString() ?? "",
                        AdminTitle = reader["AdminTitle"]?.ToString() ?? "Chief System Administrator",
                        AccessClearanceLevel = reader["AccessClearanceLevel"]?.ToString() ?? "Level-5 Root Access",
                        SecurityScope = reader["SecurityScope"]?.ToString() ?? "",
                        DataCenterLocation = reader["DataCenterLocation"]?.ToString() ?? "",
                        EmergencyEscalationContact = reader["EmergencyEscalationContact"]?.ToString() ?? "",
                        MaintenanceWindow = reader["MaintenanceWindow"]?.ToString() ?? "",
                        TwoFactorEnabled = reader["TwoFactorEnabled"] is not DBNull && Convert.ToBoolean(reader["TwoFactorEnabled"]),
                        SystemNotes = reader["SystemNotes"]?.ToString() ?? "",
                        Status = reader["Status"]?.ToString() ?? "Active",
                        LastLoginAt = reader["LastLoginAt"] is DBNull ? null : Convert.ToDateTime(reader["LastLoginAt"]),
                        TotalUsersCount = reader["TotalUsersCount"] is DBNull ? 0 : Convert.ToInt32(reader["TotalUsersCount"]),
                        PendingVerificationsCount = reader["PendingVerificationsCount"] is DBNull ? 0 : Convert.ToInt32(reader["PendingVerificationsCount"]),
                        TotalAuditLogsCount = reader["TotalAuditLogsCount"] is DBNull ? 0 : Convert.ToInt32(reader["TotalAuditLogsCount"]),
                        TotalCoursesCount = reader["TotalCoursesCount"] is DBNull ? 0 : Convert.ToInt32(reader["TotalCoursesCount"])
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // <--- Save/Update Super Admin Profile via dbo.usp_UpsertSuperAdminProfile
        public async Task<(bool Success, string Message)> UpdateSuperAdminProfileAsync(int userId, SuperAdminProfileModel model)
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand("dbo.usp_UpsertSuperAdminProfile", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UserID", userId);
                command.Parameters.AddWithValue("@AdminCode", model.AdminCode ?? "");
                command.Parameters.AddWithValue("@FullName", model.FullName ?? "");
                command.Parameters.AddWithValue("@Email", model.Email ?? "");
                command.Parameters.AddWithValue("@Phone", model.Phone ?? "");
                command.Parameters.AddWithValue("@AdminTitle", model.AdminTitle ?? "");
                command.Parameters.AddWithValue("@AccessClearanceLevel", model.AccessClearanceLevel ?? "");
                command.Parameters.AddWithValue("@SecurityScope", model.SecurityScope ?? "");
                command.Parameters.AddWithValue("@DataCenterLocation", model.DataCenterLocation ?? "");
                command.Parameters.AddWithValue("@EmergencyEscalationContact", model.EmergencyEscalationContact ?? "");
                command.Parameters.AddWithValue("@MaintenanceWindow", model.MaintenanceWindow ?? "");
                command.Parameters.AddWithValue("@TwoFactorEnabled", model.TwoFactorEnabled);
                command.Parameters.AddWithValue("@SystemNotes", model.SystemNotes ?? "");
                command.Parameters.AddWithValue("@Status", model.Status ?? "Active");

                await command.ExecuteNonQueryAsync();
                return (true, "Super Admin profile updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to update Super Admin profile: {ex.Message}");
            }
        }


}

public class UserData
{
    public int UserID { get; set; }
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Status { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string VerificationStatus { get; set; } = "Pending";
    public string LockedRoleName { get; set; } = "";
}