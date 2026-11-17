# 📚 LMS Android Portal (`lms.android`)

A multi-role, enterprise-grade **Learning Management System (LMS)** mobile application built using **.NET MAUI (C#)** and backed by **Microsoft SQL Server**.

The application follows the **MVVM architectural pattern** and implements Role-Based Access Control (RBAC), dynamic profile management across five institutional roles, and an automated hierarchical account verification workflow.

---

## ✨ Key Features

### 1. Five-Tier Role-Based Access Control (RBAC)

Supports five distinct institutional roles with role-based navigation and permissions:

* **Student:** Access academic profiles, enrolled courses, and LMS features.
* **Teacher:** Manage faculty profiles, academic information, teaching assignments, and student verification.
* **HOD (Head of Department):** Manage departmental information and verify teacher profiles.
* **Competent Authority:** Manage executive profiles and verify HOD accounts.
* **Super Admin:** Maintain system-wide oversight and manage top-level account verification.

Implements role locking through `LockedRoleName` and `UserRoleMapping` to maintain consistent role assignments.

### 2. Hierarchical Account Verification Workflow

Newly registered accounts enter a `Pending` verification state with restricted application access until approved by their designated reviewing authority.

The verification hierarchy is configurable through `dbo.ProfileWorkflowConfig`:

* **Teacher** ➔ Verifies Student profiles.
* **HOD** ➔ Verifies Teacher profiles.
* **Competent Authority** ➔ Verifies HOD profiles.
* **Super Admin** ➔ Verifies Competent Authority profiles and maintains system-wide oversight.

Additional functionality:

* Profile approval and rejection.
* Verification status tracking.
* Verification history through `dbo.ProfileVerificationHistory`.
* Dynamic reviewer approval queues.
* Restricted navigation for unverified accounts.

### 3. Dynamic Role-Based Profile Management

A unified `ProfilePage.xaml` dynamically adapts its interface, icons, visibility, and data bindings according to the authenticated user's role.

**Student Profile**
* Academic information and enrollment status.
* Personal and contact information.
* Guardian records.

**Teacher Profile**
* Personal and contact information.
* Academic qualifications and professional rank.
* Teaching experience and work history.
* Course and teacher assignments.

**HOD Profile**
* Department leadership information.
* Faculty statistics and department details.
* Office room and tenure information.
* Leadership mandate notes.

**Competent Authority Profile**
* Executive information and institutional jurisdiction.
* Signing mandate and administrative responsibilities.
* Institutional overview and metrics.

**Reviewer Approval Queue**
* Displays pending staff and administrator profiles.
* Supports profile approval and rejection.
* Updates verification status through database procedures.

### 4. Non-Blocking Asynchronous Database Architecture

* Uses asynchronous SQL Server operations to prevent UI freezing.
* Offloads database connection operations from the main UI thread.
* Implements `BindableLayout` within scrollable views.
* Improves responsiveness during profile loading and verification operations.
* Supports SQL Server communication through `Microsoft.Data.SqlClient`.

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Frontend / UI** | .NET MAUI, XAML |
| **Programming Language** | C# |
| **Architecture** | MVVM |
| **MVVM Implementation** | `INotifyPropertyChanged`, `ICommand`, Compiled Bindings |
| **Target Platform** | Android |
| **UI Navigation** | .NET MAUI Shell |
| **Database** | Microsoft SQL Server |
| **Data Access** | `Microsoft.Data.SqlClient` |
| **Database Operations** | SQL Server Stored Procedures |
| **Development Environment** | Visual Studio 2022 / 2026 |
| **Testing Environment** | Android Emulator (API 36) and Physical Devices |

---

## 📂 Project Structure

```text
lms.android/
├── Model/
│   ├── StudentProfile.cs
│   │   # Student, Teacher, HOD, Competent Authority
│   │   # and Pending Queue models
│   └── UserAccount.cs
│       # Authentication and session models
│
├── Services/
│   ├── DatabaseService.cs
│   │   # Authentication, RBAC, Teacher, HOD
│   │   # and Competent Authority database operations
│   └── StudentProfileService.cs
│       # Student profile CRUD operations
│
├── ViewModel/
│   ├── BaseViewModel.cs
│   │   # Base INotifyPropertyChanged implementation
│   ├── LoginViewModel.cs
│   │   # Authentication and role-locking logic
│   └── ProfileViewModel.cs
│       # Unified profile state and verification queue
│
├── View/
│   ├── LoginPage.xaml
│   │   # Role-selectable login screen
│   ├── SignUpPage.xaml
│   │   # User registration
│   ├── ForgotPasswordPage.xaml
│   │   # Password recovery
│   ├── MainPage.xaml
│   │   # LMS courses dashboard
│   └── ProfilePage.xaml
│       # Dynamic role-based profile and approval portal
│
├── AppShell.xaml
├── AppShell.xaml.cs
│   # Shell navigation and session management
│
├── MauiProgram.cs
│   # Application configuration and dependency injection
│
└── GlobalUsings.cs
    # Project-wide namespaces
```

---

## 🗄️ Database Schema Overview

The LMS uses a normalized Microsoft SQL Server database named `LMS`, organized into identity, academic, administrative, and learning management domains.

### 1. Identity & Role Management

* `dbo.UserAccounts`
* `dbo.UserRoles`
* `dbo.UserRoleMapping`
* `dbo.PasswordResetTokens`

### 2. Account Verification Workflow

* `dbo.ProfileWorkflowConfig`
* `dbo.ProfileVerificationHistory`

### 3. Student Management

* `dbo.Student`
* `dbo.StudentAcademic`
* `dbo.StudentContactNumber`
* `dbo.StudentGuardian`

### 4. Teacher Management

* `dbo.Teachers`
* `dbo.Teacher`
* `dbo.TeacherAcademic`
* `dbo.TeacherExperience`
* `dbo.TeacherCourseAssignments`

### 5. Department & Administrative Management

* `dbo.HeadOfDepartment`
* `dbo.CompetentAuthority`
* `dbo.Departments`
* `dbo.Programs`

### 6. Learning Management System

* `dbo.Courses`
* `dbo.Semesters`
* `dbo.Sections`
* `dbo.Enrollments`
* `dbo.Lessons`
* `dbo.LearningMaterials`
* `dbo.Assignments`
* `dbo.Submissions`
* `dbo.ExamsQuizzes`
* `dbo.Results`
* `dbo.Attendance`
* `dbo.CourseFeedback`
* `dbo.DiscussionForums`
* `dbo.DiscussionPosts`

---

## ⚙️ Core Stored Procedures

The application uses SQL Server stored procedures to handle authentication, profile management, role assignments, and verification workflows.

| Stored Procedure | Responsibility |
|---|---|
| `dbo.usp_GETUserForLogin` | Authenticates users and validates role assignments. |
| `dbo.usp_LockUserRole` | Locks the selected user role during initial login. |
| `dbo.ProcessProfileVerification` | Processes approval, rejection, and verification status transitions. |
| `dbo.usp_GetPendingProfilesForReviewer` | Retrieves pending profiles for the active reviewer. |
| `dbo.usp_GetTeacherProfileByID` | Retrieves Teacher profile information. |
| `dbo.usp_UpsertTeacherProfile` | Inserts or updates Teacher profile information. |
| `dbo.usp_GetHODProfileByID` | Retrieves HOD profile information. |
| `dbo.usp_UpsertHODProfile` | Inserts or updates HOD profile information. |
| `dbo.usp_GetCompetentAuthorityProfileByID` | Retrieves Competent Authority information. |
| `dbo.usp_UpsertCompetentAuthorityProfile` | Inserts or updates Competent Authority information. |

---

## 🚀 Getting Started & Configuration

### 1. Prerequisites

Before running the application, install the following:

* Visual Studio 2022 / 2026.
* .NET Multi-platform App UI (MAUI) development workload.
* .NET 10 SDK.
* Microsoft SQL Server / SQL Server Express.
* SQL Server Management Studio (SSMS).
* Android SDK (API 34+ / API 36).
* Android Emulator or a physical Android device.

### 2. Database Setup

1. Open SQL Server Management Studio.
2. Create a database named `LMS`.
3. Execute the database schema creation scripts.
4. Execute the stored procedure scripts.
5. Ensure all required tables, relationships, constraints, and stored procedures are created successfully.

Disable database auto-close to prevent unnecessary connection delays:

```sql
ALTER DATABASE LMS
SET AUTO_CLOSE OFF WITH NO_WAIT;
GO
```

### 3. SQL Server Network Configuration

For Android Emulator connectivity, configure SQL Server to accept TCP/IP connections.

1. Open SQL Server Configuration Manager.
2. Navigate to SQL Server Network Configuration.
3. Select Protocols for MSSQLSERVER or SQLEXPRESS.
4. Enable TCP/IP.
5. Open TCP/IP Properties.
6. Navigate to IP Addresses.
7. Under IPAll, configure TCP Port as `1433`.
8. Restart the SQL Server service.
9. Enable inbound TCP traffic on port `1433` in Windows Defender Firewall.
10. Enable SQL Server and Windows Authentication mode.

### 4. Connection String Configuration

Open the following files:

* `Services/DatabaseService.cs`
* `Services/StudentProfileService.cs`

Configure the SQL Server connection string according to your environment.

**Android Emulator:**

```csharp
private const string _connectionString =
    "Server=10.0.2.2,1433;Database=LMS;User Id=lms_user;Password=YOUR_PASSWORD;" +
    "Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;Pooling=True;";
```

**Windows / Local Environment:**

```csharp
private const string _connectionString =
    "Server=localhost,1433;Database=LMS;User Id=lms_user;Password=YOUR_PASSWORD;" +
    "Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;Pooling=True;";
```

**Important:** Replace the placeholder credentials with your actual SQL Server login details. Do not commit database passwords to GitHub.

### 5. Build & Run

1. Open the `lms.android` solution in Visual Studio.
2. Restore the required NuGet packages.
3. Ensure the SQL Server database is running.
4. Verify the database connection string.
5. Select an Android Emulator (e.g., Pixel 7 - API 36) or a physical Android device.
6. Press **F5** to build, deploy, and launch the application.

---

## 🔐 Default Test Accounts

The following accounts are provided for application testing:

| Username | Role | Verification Status | Access Level |
|---|---|---|---|
| `Suleman_31` | Student | Verified | Full LMS and Student Profile access |
| `Sallu_31` | Teacher | Verified | Teacher Profile and Student Approval Queue |
| `Sameer_31` | Student / Any | Pending | Restricted to Verification Pending screen |
| `superadmin` | Super Admin | Verified | System-wide oversight and top-level approval queue |

*Note: Use the passwords configured in your development database. The accounts above represent sample test data.*

---

## 🔒 Security & Architecture Considerations

* Implements centralized role-based access control.
* Enforces hierarchical profile verification through database-driven configuration.
* Uses stored procedures for structured database operations.
* Maintains verification history for administrative accountability.
* Restricts application navigation for pending accounts.
* Uses asynchronous database operations to improve UI responsiveness.
* Separates Models, Views, ViewModels, and Services following MVVM principles.
* Uses parameterized stored procedure calls to reduce SQL injection risks.

---

## 👨‍💻 Author

**Muhammad Suleman Riaz**

* **GitHub:** [MuhammadSuleman33](https://github.com/MuhammadSuleman33)
* **LinkedIn:** [Muhammad Suleman Riaz](https://www.linkedin.com/in/muhammad-suleman-riaz-095201355/)

---

## 📄 License

This project is developed for educational and institutional Learning Management System purposes. Add an appropriate open-source license before distributing the source code publicly.
