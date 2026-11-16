using System;

namespace lms.android.Model
{
        public class UserAccountProfile
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = "Pending";
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RoleDescription { get; set; } = string.Empty;
        public string LatestReviewComment { get; set; } = string.Empty;
    }

    public class StudentProfile
    {
        // Core Identity
        public int StudentID { get; set; }
        public string RegistrationNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateofBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Contact Details
        public int? ContactID { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;

        // Guardian Details
        public int? GuardianID { get; set; }
        public string GuardianName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string GuardianPhone { get; set; } = string.Empty;
        public string GuardianEmail { get; set; } = string.Empty;
        public string GuardianAddress { get; set; } = string.Empty;

        // Academic Details
        public int? StudentAcademicID { get; set; }
        public int? ProgramID { get; set; }
        public DateTime? AdmissionDate { get; set; }
        public string CurrentSemester { get; set; } = string.Empty;
        public string AcademicStatus { get; set; } = string.Empty;
        public int? EnrollmentYear { get; set; }
        public DateTime? ExpectedGraduationDate { get; set; }

        // Helper property for full name display in UI (supports both FirstName + LastName or direct fallback assignment)
        private string _fallbackFullName = string.Empty;
        public string FullName
        {
            get
            {
                var combined = $"{FirstName} {LastName}".Trim();
                return !string.IsNullOrWhiteSpace(combined) ? combined : _fallbackFullName;
            }
            set => _fallbackFullName = value;
        }
    }


    public class TeacherProfileModel
    {
        // 1. dbo.Teachers (Core Identity & Contact)
        public int TeacherID { get; set; }
        public string EmployeeNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } = new DateTime(1990, 1, 1);
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        // 2. dbo.Teacher & dbo.TeacherAcademic (Academic, Rank & Office)
        public int DepartmentID { get; set; } = 1;
        public string Designation { get; set; } = "Lecturer";
        public DateTime JoiningDate { get; set; } = DateTime.Today;
        public string Specialization { get; set; } = string.Empty;
        public string OfficeLocation { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public int TeacherAcademicID { get; set; }
        public int ExperienceYears { get; set; }

        // 3. dbo.TeacherExperience (Previous Experience)
        public int ExperienceID { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string PreviousJobTitle { get; set; } = string.Empty;
        public DateTime ExpStartDate { get; set; } = DateTime.Today.AddYears(-2);
        public DateTime ExpEndDate { get; set; } = DateTime.Today;

        // 4. dbo.TeacherCourseAssignments (Assigned Courses Summary)
        public int AssignedCoursesCount { get; set; }
        public string AssignedCoursesSummary { get; set; } = "No courses assigned yet";

        public string FullName => $"{FirstName} {LastName}".Trim();
    }


    public class HODProfileModel
    {
        public int HODID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // dbo.HeadOfDepartment columns
        public int DepartmentID { get; set; } = 1;
        public string OfficeRoomNo { get; set; } = string.Empty;
        public DateTime AppointedDate { get; set; } = DateTime.Today;
        public DateTime TenureExpiryDate { get; set; } = DateTime.Today.AddYears(3);
        public string Notes { get; set; } = string.Empty;

        // Department overview counts
        public int DepartmentTeachersCount { get; set; }
        public int DepartmentCoursesCount { get; set; }

        public string TenureStatus => TenureExpiryDate >= DateTime.Today ? "Active Tenure" : "Tenure Expired";
    }



    public class CompetentAuthorityProfileModel
    {
        public int AuthorityID { get; set; }
        public string AuthorityCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Executive & Governance Details
        public string Designation { get; set; } = "Dean / Competent Authority";
        public string JurisdictionScope { get; set; } = "University-Wide";
        public string SigningAuthorityLevel { get; set; } = "Tier-1 Final Approver";
        public string Qualification { get; set; } = string.Empty;

        // Secretariat & Tenure Details
        public string SecretariatOfficeLocation { get; set; } = string.Empty;
        public string ProtocolExtension { get; set; } = string.Empty;
        public DateTime AppointedDate { get; set; } = DateTime.Today;
        public DateTime TenureExpiryDate { get; set; } = DateTime.Today.AddYears(3);
        public string CommitteeMemberships { get; set; } = string.Empty;
        public string MandateNotes { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";

        // Live Institutional Metrics
        public int TotalDepartmentsCount { get; set; }
        public int TotalHODsCount { get; set; }
        public int TotalApprovalsProcessed { get; set; }
    }



    //  Super Admin Profile Model
    public class SuperAdminProfileModel
    {
        public int AdminID { get; set; }
        public string AdminCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Security & Governance
        public string AdminTitle { get; set; } = "Chief System Administrator";
        public string AccessClearanceLevel { get; set; } = "Level-5 Root Access";
        public string SecurityScope { get; set; } = "Full System, Roles & Workflow Control";
        public string DataCenterLocation { get; set; } = string.Empty;
        public string EmergencyEscalationContact { get; set; } = string.Empty;
        public string MaintenanceWindow { get; set; } = "Sunday 02:00 AM - 04:00 AM";
        public bool TwoFactorEnabled { get; set; } = true;
        public string SystemNotes { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime? LastLoginAt { get; set; }

        // Live System Telemetry
        public int TotalUsersCount { get; set; }
        public int PendingVerificationsCount { get; set; }
        public int TotalAuditLogsCount { get; set; }
        public int TotalCoursesCount { get; set; }
    }

    public class PendingVerificationItem
    {
        public int TargetUserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfileType { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
    }
}