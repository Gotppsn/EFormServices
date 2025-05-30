using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace EFormServices.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Organizations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Subdomain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                TenantKey = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Settings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Organizations", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Permissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsSystemPermission = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Permissions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalWorkflows",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrganizationId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                WorkflowType = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                Settings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalWorkflows", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalWorkflows_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Departments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrganizationId = table.Column<int>(type: "int", nullable: false),
                ParentDepartmentId = table.Column<int>(type: "int", nullable: true),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Departments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Departments_Departments_ParentDepartmentId",
                    column: x => x.ParentDepartmentId,
                    principalTable: "Departments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Departments_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrganizationId = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.Id);
                table.ForeignKey(
                    name: "FK_Roles_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalSteps",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ApprovalWorkflowId = table.Column<int>(type: "int", nullable: false),
                StepName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                StepType = table.Column<int>(type: "int", nullable: false),
                StepOrder = table.Column<int>(type: "int", nullable: false),
                ApproverCriteria = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                RequireAllApprovers = table.Column<bool>(type: "bit", nullable: false),
                TimeoutHours = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalSteps", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalSteps_ApprovalWorkflows_ApprovalWorkflowId",
                    column: x => x.ApprovalWorkflowId,
                    principalTable: "ApprovalWorkflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrganizationId = table.Column<int>(type: "int", nullable: false),
                DepartmentId = table.Column<int>(type: "int", nullable: true),
                Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Salt = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.ForeignKey(
                    name: "FK_Users_Departments_DepartmentId",
                    column: x => x.DepartmentId,
                    principalTable: "Departments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Users_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<int>(type: "int", nullable: false),
                PermissionId = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_RolePermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RolePermissions_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Forms",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OrganizationId = table.Column<int>(type: "int", nullable: false),
                DepartmentId = table.Column<int>(type: "int", nullable: true),
                CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                ApprovalWorkflowId = table.Column<int>(type: "int", nullable: true),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                FormType = table.Column<int>(type: "int", nullable: false),
                IsTemplate = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                IsPublic = table.Column<bool>(type: "bit", nullable: false),
                Settings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                FormKey = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Forms", x => x.Id);
                table.ForeignKey(
                    name: "FK_Forms_ApprovalWorkflows_ApprovalWorkflowId",
                    column: x => x.ApprovalWorkflowId,
                    principalTable: "ApprovalWorkflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Forms_Departments_DepartmentId",
                    column: x => x.DepartmentId,
                    principalTable: "Departments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Forms_Organizations_OrganizationId",
                    column: x => x.OrganizationId,
                    principalTable: "Organizations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Forms_Users_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "UserRoles",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                RoleId = table.Column<int>(type: "int", nullable: false),
                AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRoles", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserRoles_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserRoles_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FormFields",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FormId = table.Column<int>(type: "int", nullable: false),
                FieldType = table.Column<int>(type: "int", nullable: false),
                Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                ValidationRules = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsRequired = table.Column<bool>(type: "bit", nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                Settings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FormFields", x => x.Id);
                table.ForeignKey(
                    name: "FK_FormFields_Forms_FormId",
                    column: x => x.FormId,
                    principalTable: "Forms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FormSubmissions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FormId = table.Column<int>(type: "int", nullable: false),
                SubmittedByUserId = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                TrackingNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FormSubmissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_FormSubmissions_Forms_FormId",
                    column: x => x.FormId,
                    principalTable: "Forms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_FormSubmissions_Users_SubmittedByUserId",
                    column: x => x.SubmittedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ConditionalLogics",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FormId = table.Column<int>(type: "int", nullable: false),
                TriggerFieldId = table.Column<int>(type: "int", nullable: false),
                TargetFieldId = table.Column<int>(type: "int", nullable: false),
                Condition = table.Column<int>(type: "int", nullable: false),
                TriggerValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Action = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ConditionalLogics", x => x.Id);
                table.ForeignKey(
                    name: "FK_ConditionalLogics_FormFields_TargetFieldId",
                    column: x => x.TargetFieldId,
                    principalTable: "FormFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ConditionalLogics_FormFields_TriggerFieldId",
                    column: x => x.TriggerFieldId,
                    principalTable: "FormFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ConditionalLogics_Forms_FormId",
                    column: x => x.FormId,
                    principalTable: "Forms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "FormFieldOptions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FormFieldId = table.Column<int>(type: "int", nullable: false),
                Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SortOrder = table.Column<int>(type: "int", nullable: false),
                IsDefault = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FormFieldOptions", x => x.Id);
                table.ForeignKey(
                    name: "FK_FormFieldOptions_FormFields_FormFieldId",
                    column: x => x.FormFieldId,
                    principalTable: "FormFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalProcesses",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FormSubmissionId = table.Column<int>(type: "int", nullable: false),
                ApprovalWorkflowId = table.Column<int>(type: "int", nullable: false),
                CurrentStepId = table.Column<int>(type: "int", nullable: true),
                Status = table.Column<int>(type: "int", nullable: false),
                StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalProcesses", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalProcesses_ApprovalSteps_CurrentStepId",
                    column: x => x.CurrentStepId,
                    principalTable: "ApprovalSteps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ApprovalProcesses_ApprovalWorkflows_ApprovalWorkflowId",
                    column: x => x.ApprovalWorkflowId,
                    principalTable: "ApprovalWorkflows",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ApprovalProcesses_FormSubmissions_FormSubmissionId",
                    column: x => x.FormSubmissionId,
                    principalTable: "FormSubmissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "FileAttachments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FormSubmissionId = table.Column<int>(type: "int", nullable: false),
                FormFieldId = table.Column<int>(type: "int", nullable: false),
                FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                FileSize = table.Column<long>(type: "bigint", nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                FileHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FileAttachments", x => x.Id);
                table.ForeignKey(
                    name: "FK_FileAttachments_FormFields_FormFieldId",
                    column: x => x.FormFieldId,
                    principalTable: "FormFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_FileAttachments_FormSubmissions_FormSubmissionId",
                    column: x => x.FormSubmissionId,
                    principalTable: "FormSubmissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SubmissionValues",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FormSubmissionId = table.Column<int>(type: "int", nullable: false),
                FormFieldId = table.Column<int>(type: "int", nullable: false),
                FieldName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ValueType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubmissionValues", x => x.Id);
                table.ForeignKey(
                    name: "FK_SubmissionValues_FormFields_FormFieldId",
                    column: x => x.FormFieldId,
                    principalTable: "FormFields",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_SubmissionValues_FormSubmissions_FormSubmissionId",
                    column: x => x.FormSubmissionId,
                    principalTable: "FormSubmissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ApprovalActions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ApprovalProcessId = table.Column<int>(type: "int", nullable: false),
                ApprovalStepId = table.Column<int>(type: "int", nullable: false),
                ActionByUserId = table.Column<int>(type: "int", nullable: false),
                Action = table.Column<int>(type: "int", nullable: false),
                Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                ActionAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ApprovalActions", x => x.Id);
                table.ForeignKey(
                    name: "FK_ApprovalActions_ApprovalProcesses_ApprovalProcessId",
                    column: x => x.ApprovalProcessId,
                    principalTable: "ApprovalProcesses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ApprovalActions_ApprovalSteps_ApprovalStepId",
                    column: x => x.ApprovalStepId,
                    principalTable: "ApprovalSteps",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ApprovalActions_Users_ActionByUserId",
                    column: x => x.ActionByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            table: "Permissions",
            columns: new[] { "Id", "Category", "CreatedAt", "Description", "IsSystemPermission", "Name", "UpdatedAt" },
            values: new object[,]
            {
                { 1, "Organization", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Manage organization settings", true, "manage_organization", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 2, "Users", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Create and manage users", true, "manage_users", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 3, "Users", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Create and manage roles", true, "manage_roles", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 4, "Forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Create new forms", true, "create_forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 5, "Forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Edit existing forms", true, "edit_forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 6, "Forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Delete forms", true, "delete_forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 7, "Forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "View forms", true, "view_forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 8, "Forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Submit form responses", true, "submit_forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 9, "Approvals", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Approve form submissions", true, "approve_forms", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                { 10, "Reports", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc), "View form reports and analytics", true, "view_reports", new DateTime(2025, 5, 30, 0, 0, 0, 0, DateTimeKind.Utc) }
            });

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalActions_ActionByUserId",
            table: "ApprovalActions",
            column: "ActionByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalActions_ApprovalProcessId_ActionAt",
            table: "ApprovalActions",
            columns: new[] { "ApprovalProcessId", "ActionAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalActions_ApprovalStepId",
            table: "ApprovalActions",
            column: "ApprovalStepId");

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalProcesses_ApprovalWorkflowId",
            table: "ApprovalProcesses",
            column: "ApprovalWorkflowId");

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalProcesses_CurrentStepId",
            table: "ApprovalProcesses",
            column: "CurrentStepId");

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalProcesses_FormSubmissionId",
            table: "ApprovalProcesses",
            column: "FormSubmissionId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalProcesses_Status_StartedAt",
            table: "ApprovalProcesses",
            columns: new[] { "Status", "StartedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalSteps_ApprovalWorkflowId_StepOrder",
            table: "ApprovalSteps",
            columns: new[] { "ApprovalWorkflowId", "StepOrder" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ApprovalWorkflows_OrganizationId_IsActive",
            table: "ApprovalWorkflows",
            columns: new[] { "OrganizationId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_ConditionalLogics_FormId_TriggerFieldId_TargetFieldId",
            table: "ConditionalLogics",
            columns: new[] { "FormId", "TriggerFieldId", "TargetFieldId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ConditionalLogics_TargetFieldId",
            table: "ConditionalLogics",
            column: "TargetFieldId");

        migrationBuilder.CreateIndex(
            name: "IX_ConditionalLogics_TriggerFieldId",
            table: "ConditionalLogics",
            column: "TriggerFieldId");

        migrationBuilder.CreateIndex(
            name: "IX_Departments_OrganizationId_Code",
            table: "Departments",
            columns: new[] { "OrganizationId", "Code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Departments_OrganizationId_IsActive",
            table: "Departments",
            columns: new[] { "OrganizationId", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Departments_ParentDepartmentId",
            table: "Departments",
            column: "ParentDepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_FileAttachments_FileHash",
            table: "FileAttachments",
            column: "FileHash");

        migrationBuilder.CreateIndex(
            name: "IX_FileAttachments_FormFieldId",
            table: "FileAttachments",
            column: "FormFieldId");

        migrationBuilder.CreateIndex(
            name: "IX_FileAttachments_FormSubmissionId_FormFieldId_FileName",
            table: "FileAttachments",
            columns: new[] { "FormSubmissionId", "FormFieldId", "FileName" });

        migrationBuilder.CreateIndex(
            name: "IX_FormFieldOptions_FormFieldId_SortOrder",
            table: "FormFieldOptions",
            columns: new[] { "FormFieldId", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_FormFieldOptions_FormFieldId_Value",
            table: "FormFieldOptions",
            columns: new[] { "FormFieldId", "Value" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_FormFields_FormId_Name",
            table: "FormFields",
            columns: new[] { "FormId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_FormFields_FormId_SortOrder",
            table: "FormFields",
            columns: new[] { "FormId", "SortOrder" });

        migrationBuilder.CreateIndex(
            name: "IX_Forms_ApprovalWorkflowId",
            table: "Forms",
            column: "ApprovalWorkflowId");

        migrationBuilder.CreateIndex(
            name: "IX_Forms_CreatedByUserId",
            table: "Forms",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Forms_DepartmentId",
            table: "Forms",
            column: "DepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_Forms_FormKey",
            table: "Forms",
            column: "FormKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Forms_OrganizationId_FormType_IsActive",
            table: "Forms",
            columns: new[] { "OrganizationId", "FormType", "IsActive" });

        migrationBuilder.CreateIndex(
            name: "IX_Forms_OrganizationId_IsActive_IsPublic",
            table: "Forms",
            columns: new[] { "OrganizationId", "IsActive", "IsPublic" });

        migrationBuilder.CreateIndex(
            name: "IX_FormSubmissions_FormId_Status_SubmittedAt",
            table: "FormSubmissions",
            columns: new[] { "FormId", "Status", "SubmittedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_FormSubmissions_SubmittedByUserId",
            table: "FormSubmissions",
            column: "SubmittedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_FormSubmissions_SubmittedByUserId_SubmittedAt",
            table: "FormSubmissions",
            columns: new[] { "SubmittedByUserId", "SubmittedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_FormSubmissions_TrackingNumber",
            table: "FormSubmissions",
            column: "TrackingNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Organizations_Subdomain",
            table: "Organizations",
            column: "Subdomain",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Organizations_TenantKey",
            table: "Organizations",
            column: "TenantKey",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_Name",
            table: "Permissions",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_PermissionId",
            table: "RolePermissions",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_RoleId_PermissionId",
            table: "RolePermissions",
            columns: new[] { "RoleId", "PermissionId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Roles_OrganizationId_Name",
            table: "Roles",
            columns: new[] { "OrganizationId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SubmissionValues_FormFieldId",
            table: "SubmissionValues",
            column: "FormFieldId");

        migrationBuilder.CreateIndex(
            name: "IX_SubmissionValues_FormSubmissionId_FormFieldId",
            table: "SubmissionValues",
            columns: new[] { "FormSubmissionId", "FormFieldId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_RoleId",
            table: "UserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_UserRoles_UserId_RoleId_IsActive",
            table: "UserRoles",
            columns: new[] { "UserId", "RoleId", "IsActive" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_DepartmentId",
            table: "Users",
            column: "DepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_OrganizationId",
            table: "Users",
            column: "OrganizationId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_OrganizationId_Email",
            table: "Users",
            columns: new[] { "OrganizationId", "Email" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ApprovalActions");
        migrationBuilder.DropTable(name: "ConditionalLogics");
        migrationBuilder.DropTable(name: "FileAttachments");
        migrationBuilder.DropTable(name: "FormFieldOptions");
        migrationBuilder.DropTable(name: "RolePermissions");
        migrationBuilder.DropTable(name: "SubmissionValues");
        migrationBuilder.DropTable(name: "UserRoles");
        migrationBuilder.DropTable(name: "ApprovalProcesses");
        migrationBuilder.DropTable(name: "Permissions");
        migrationBuilder.DropTable(name: "FormFields");
        migrationBuilder.DropTable(name: "Roles");
        migrationBuilder.DropTable(name: "ApprovalSteps");
        migrationBuilder.DropTable(name: "FormSubmissions");
        migrationBuilder.DropTable(name: "ApprovalWorkflows");
        migrationBuilder.DropTable(name: "Forms");
        migrationBuilder.DropTable(name: "Users");
        migrationBuilder.DropTable(name: "Departments");
        migrationBuilder.DropTable(name: "Organizations");
    }
}