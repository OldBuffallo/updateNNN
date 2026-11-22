using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IRM.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServerV101 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    IDUser = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Permission = table.Column<int>(type: "int", nullable: false),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.IDUser);
                });

            migrationBuilder.CreateTable(
                name: "AdministrativeUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    PredecessorId = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LegacyDistrictId = table.Column<int>(type: "int", nullable: true),
                    LegacyWardId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdministrativeUnits_AdministrativeUnits_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AdministrativeUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdministrativeUnits_AdministrativeUnits_PredecessorId",
                        column: x => x.PredecessorId,
                        principalTable: "AdministrativeUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchivedEmployees",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalId = table.Column<int>(type: "int", nullable: false),
                    StaffName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Passport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDCareer = table.Column<int>(type: "int", nullable: true),
                    WorkPermit = table.Column<int>(type: "int", nullable: false),
                    WorkPermitNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisaNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemporaryStay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettlementResults = table.Column<int>(type: "int", nullable: false),
                    SettlementResultsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDUser = table.Column<int>(type: "int", nullable: false),
                    IDCompany = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardCreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkingStatus = table.Column<int>(type: "int", nullable: false),
                    DateOfJoin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfLeave = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FamilyVisit = table.Column<int>(type: "int", nullable: false),
                    FamilyVisitRelativeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyVisitRelationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyVisitRelativeIdCard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyVisitStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FamilyVisitEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FamilyVisitNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CareerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ArchivedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivedEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CareerGroups",
                columns: table => new
                {
                    IDCG = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CareerGroupName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareerGroups", x => x.IDCG);
                });

            migrationBuilder.CreateTable(
                name: "ColumnMappingTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    MappingJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnMappingTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    IDDistrict = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisTrictName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.IDDistrict);
                });

            migrationBuilder.CreateTable(
                name: "EconomicZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomicZones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    IDField = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.IDField);
                });

            migrationBuilder.CreateTable(
                name: "ImportBackups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportSessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    OldData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportBackups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    AddedRows = table.Column<int>(type: "int", nullable: false),
                    UpdatedRows = table.Column<int>(type: "int", nullable: false),
                    ErrorRows = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImportDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LegacyChangeEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Operation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BeforeXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyChangeEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MigrationIssues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationIssues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nationality",
                columns: table => new
                {
                    IDNationality = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationalityCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NationalityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationality", x => x.IDNationality);
                    table.UniqueConstraint("AK_Nationality_NationalityCode", x => x.NationalityCode);
                });

            migrationBuilder.CreateTable(
                name: "SchemaVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Checksum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StorageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScanStatusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByAccountId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    IDWard = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WardName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.IDWard);
                });

            migrationBuilder.CreateTable(
                name: "WebCredentials",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebCredentials", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_WebCredentials_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "IDUser",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationsV010",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdministrativeUnitId = table.Column<int>(type: "int", nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationsV010", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationsV010_AdministrativeUnits_AdministrativeUnitId",
                        column: x => x.AdministrativeUnitId,
                        principalTable: "AdministrativeUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    RoleCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdministrativeUnitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebRoleAssignments_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "IDUser",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WebRoleAssignments_AdministrativeUnits_AdministrativeUnitId",
                        column: x => x.AdministrativeUnitId,
                        principalTable: "AdministrativeUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Careers",
                columns: table => new
                {
                    IDCareer = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CareerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDCG = table.Column<int>(type: "int", nullable: false),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Careers", x => x.IDCareer);
                    table.ForeignKey(
                        name: "FK_Careers_CareerGroups_IDCG",
                        column: x => x.IDCG,
                        principalTable: "CareerGroups",
                        principalColumn: "IDCG",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    IDCompany = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeOfBusiniess = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDField = table.Column<int>(type: "int", nullable: false),
                    LegalRepresentative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<int>(type: "int", nullable: false),
                    AmountOfExemption = table.Column<int>(type: "int", nullable: false),
                    QuantityAvailable = table.Column<int>(type: "int", nullable: false),
                    QuantityNotYet = table.Column<int>(type: "int", nullable: false),
                    NumberOfPersonalities = table.Column<int>(type: "int", nullable: false),
                    RegistrationProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationProfileIndex = table.Column<int>(type: "int", nullable: false),
                    DescriptionOfActivities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackerID = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.IDCompany);
                    table.ForeignKey(
                        name: "FK_Companies_Accounts_TrackerID",
                        column: x => x.TrackerID,
                        principalTable: "Accounts",
                        principalColumn: "IDUser",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Companies_Fields_IDField",
                        column: x => x.IDField,
                        principalTable: "Fields",
                        principalColumn: "IDField",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForeignPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NationalityCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PassportSearchKey = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDataIncomplete = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForeignPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForeignPersons_Nationality_NationalityCode",
                        column: x => x.NationalityCode,
                        principalTable: "Nationality",
                        principalColumn: "NationalityCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    IDStudent = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Passport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Major = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EducationLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedGraduation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VisaNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisaExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TemporaryStay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScholarshipType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDUser = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Hidden_flag = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.IDStudent);
                    table.ForeignKey(
                        name: "FK_Students_Nationality_Nationality",
                        column: x => x.Nationality,
                        principalTable: "Nationality",
                        principalColumn: "NationalityCode");
                });

            migrationBuilder.CreateTable(
                name: "Attach",
                columns: table => new
                {
                    IDAttach = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IDCompany = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Folder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attach", x => x.IDAttach);
                    table.ForeignKey(
                        name: "FK_Attach_Companies_IDCompany",
                        column: x => x.IDCompany,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAccommodationAgreements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    AccommodationId = table.Column<int>(type: "int", nullable: false),
                    ScopeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAccommodationAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyAccommodationAgreements_AccommodationsV010_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "AccommodationsV010",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyAccommodationAgreements_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyProfiles",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    OwnershipTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProfiles", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_CompanyProfiles_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyRepresentatives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRepresentatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyRepresentatives_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdministrativeUnitId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySites_AdministrativeUnits_AdministrativeUnitId",
                        column: x => x.AdministrativeUnitId,
                        principalTable: "AdministrativeUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySites_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    IDEmail = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDCompany = table.Column<int>(type: "int", nullable: false),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.IDEmail);
                    table.ForeignKey(
                        name: "FK_Emails_Companies_IDCompany",
                        column: x => x.IDCompany,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    IDEmployee = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Passport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDCareer = table.Column<int>(type: "int", nullable: true),
                    WorkPermit = table.Column<int>(type: "int", nullable: false),
                    WorkPermitNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisaNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemporaryStay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettlementResults = table.Column<int>(type: "int", nullable: false),
                    SettlementResultsString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDUser = table.Column<int>(type: "int", nullable: false),
                    IDCompany = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CardCreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkingStatus = table.Column<int>(type: "int", nullable: false),
                    DateOfJoin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfLeave = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Hidden_flag = table.Column<int>(type: "int", nullable: false),
                    FamilyVisit = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FamilyVisitRelativeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyVisitRelationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyVisitRelativeIdCard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FamilyVisitStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FamilyVisitEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FamilyVisitNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.IDEmployee);
                    table.ForeignKey(
                        name: "FK_Employees_Careers_IDCareer",
                        column: x => x.IDCareer,
                        principalTable: "Careers",
                        principalColumn: "IDCareer");
                    table.ForeignKey(
                        name: "FK_Employees_Companies_IDCompany",
                        column: x => x.IDCompany,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Employees_Nationality_Nationality",
                        column: x => x.Nationality,
                        principalTable: "Nationality",
                        principalColumn: "NationalityCode");
                });

            migrationBuilder.CreateTable(
                name: "Investment",
                columns: table => new
                {
                    IDInvestment = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmountOfMoney = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IDCompany = table.Column<int>(type: "int", nullable: false),
                    Passport = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investment", x => x.IDInvestment);
                    table.ForeignKey(
                        name: "FK_Investment_Companies_IDCompany",
                        column: x => x.IDCompany,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhoneNumbers",
                columns: table => new
                {
                    IDPhoneNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDCompany = table.Column<int>(type: "int", nullable: false),
                    Delete_flag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneNumbers", x => x.IDPhoneNumber);
                    table.ForeignKey(
                        name: "FK_PhoneNumbers_Companies_IDCompany",
                        column: x => x.IDCompany,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ElectronicIdentities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForeignPersonId = table.Column<int>(type: "int", nullable: false),
                    IdentityNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentitySearchKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectronicIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ElectronicIdentities_ForeignPersons_ForeignPersonId",
                        column: x => x.ForeignPersonId,
                        principalTable: "ForeignPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForeignPersonSourceLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForeignPersonId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceFingerprint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSynchronizedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForeignPersonSourceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForeignPersonSourceLinks_ForeignPersons_ForeignPersonId",
                        column: x => x.ForeignPersonId,
                        principalTable: "ForeignPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImmigrationDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForeignPersonId = table.Column<int>(type: "int", nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImmigrationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImmigrationDocuments_ForeignPersons_ForeignPersonId",
                        column: x => x.ForeignPersonId,
                        principalTable: "ForeignPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResidencePeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForeignPersonId = table.Column<int>(type: "int", nullable: false),
                    AccommodationId = table.Column<int>(type: "int", nullable: true),
                    AdministrativeUnitId = table.Column<int>(type: "int", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArrangementCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponsibleCompanyId = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidencePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidencePeriods_AccommodationsV010_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "AccommodationsV010",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResidencePeriods_AdministrativeUnits_AdministrativeUnitId",
                        column: x => x.AdministrativeUnitId,
                        principalTable: "AdministrativeUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResidencePeriods_Companies_ResponsibleCompanyId",
                        column: x => x.ResponsibleCompanyId,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResidencePeriods_ForeignPersons_ForeignPersonId",
                        column: x => x.ForeignPersonId,
                        principalTable: "ForeignPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StayCases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForeignPersonId = table.Column<int>(type: "int", nullable: false),
                    PurposeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SponsorCompanyId = table.Column<int>(type: "int", nullable: true),
                    SponsorForeignPersonId = table.Column<int>(type: "int", nullable: true),
                    StatusCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StayCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StayCases_Companies_SponsorCompanyId",
                        column: x => x.SponsorCompanyId,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StayCases_ForeignPersons_ForeignPersonId",
                        column: x => x.ForeignPersonId,
                        principalTable: "ForeignPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StayCases_ForeignPersons_SponsorForeignPersonId",
                        column: x => x.SponsorForeignPersonId,
                        principalTable: "ForeignPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyLegalDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoredFileId = table.Column<int>(type: "int", nullable: true),
                    LegacyAttachId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyLegalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyLegalDocuments_Attach_LegacyAttachId",
                        column: x => x.LegacyAttachId,
                        principalTable: "Attach",
                        principalColumn: "IDAttach",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyLegalDocuments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "IDCompany",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyLegalDocuments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InspectionsV010",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdministrativeUnitId = table.Column<int>(type: "int", nullable: true),
                    AccommodationId = table.Column<int>(type: "int", nullable: true),
                    CompanySiteId = table.Column<int>(type: "int", nullable: true),
                    InspectorNames = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByAccountId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionsV010", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionsV010_AccommodationsV010_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "AccommodationsV010",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionsV010_AdministrativeUnits_AdministrativeUnitId",
                        column: x => x.AdministrativeUnitId,
                        principalTable: "AdministrativeUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionsV010_CompanySites_CompanySiteId",
                        column: x => x.CompanySiteId,
                        principalTable: "CompanySites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SiteZoneMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanySiteId = table.Column<int>(type: "int", nullable: false),
                    EconomicZoneId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteZoneMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteZoneMemberships_CompanySites_CompanySiteId",
                        column: x => x.CompanySiteId,
                        principalTable: "CompanySites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SiteZoneMemberships_EconomicZones_EconomicZoneId",
                        column: x => x.EconomicZoneId,
                        principalTable: "EconomicZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyVisitDetails",
                columns: table => new
                {
                    StayCaseId = table.Column<int>(type: "int", nullable: false),
                    RelativeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelativeIdNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelativePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelativeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelativeTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelativeNationalityCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyVisitDetails", x => x.StayCaseId);
                    table.ForeignKey(
                        name: "FK_FamilyVisitDetails_StayCases_StayCaseId",
                        column: x => x.StayCaseId,
                        principalTable: "StayCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InspectionSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    ForeignPersonId = table.Column<int>(type: "int", nullable: false),
                    PurposeCodeSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentsSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkDescriptionSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResultCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViolationDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionTaken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionSubjects_ForeignPersons_ForeignPersonId",
                        column: x => x.ForeignPersonId,
                        principalTable: "ForeignPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionSubjects_InspectionsV010_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "InspectionsV010",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationsV010_AdministrativeUnitId",
                table: "AccommodationsV010",
                column: "AdministrativeUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeUnits_Code_ValidFrom",
                table: "AdministrativeUnits",
                columns: new[] { "Code", "ValidFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeUnits_ParentId",
                table: "AdministrativeUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdministrativeUnits_PredecessorId",
                table: "AdministrativeUnits",
                column: "PredecessorId");

            migrationBuilder.CreateIndex(
                name: "IX_AU_LegacyDistrictId",
                table: "AdministrativeUnits",
                column: "LegacyDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_AU_LegacyWardId",
                table: "AdministrativeUnits",
                column: "LegacyWardId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedEmployees_ArchiveReason",
                table: "ArchivedEmployees",
                column: "ArchiveReason");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivedEmployees_OriginalId",
                table: "ArchivedEmployees",
                column: "OriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_Attach_IDCompany",
                table: "Attach",
                column: "IDCompany");

            migrationBuilder.CreateIndex(
                name: "IX_Careers_IDCG",
                table: "Careers",
                column: "IDCG");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_DeleteFlag",
                table: "Companies",
                column: "Delete_flag");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_IDField",
                table: "Companies",
                column: "IDField");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TrackerID",
                table: "Companies",
                column: "TrackerID");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAccommodationAgreements_AccommodationId",
                table: "CompanyAccommodationAgreements",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAccommodationAgreements_CompanyId_AccommodationId_ValidFrom",
                table: "CompanyAccommodationAgreements",
                columns: new[] { "CompanyId", "AccommodationId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLegalDocuments_CompanyId_TypeCode_DocumentNumber",
                table: "CompanyLegalDocuments",
                columns: new[] { "CompanyId", "TypeCode", "DocumentNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLegalDocuments_LegacyAttachId",
                table: "CompanyLegalDocuments",
                column: "LegacyAttachId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyLegalDocuments_StoredFileId",
                table: "CompanyLegalDocuments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRepresentatives_CompanyId",
                table: "CompanyRepresentatives",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySites_AdministrativeUnitId",
                table: "CompanySites",
                column: "AdministrativeUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySites_CompanyId_ValidFrom_ValidTo",
                table: "CompanySites",
                columns: new[] { "CompanyId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_EconomicZones_Code_ValidFrom",
                table: "EconomicZones",
                columns: new[] { "Code", "ValidFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicIdentities_ForeignPersonId",
                table: "ElectronicIdentities",
                column: "ForeignPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicIdentities_IdentitySearchKey",
                table: "ElectronicIdentities",
                column: "IdentitySearchKey");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_IDCompany",
                table: "Emails",
                column: "IDCompany");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_FamilyVisit",
                table: "Employees",
                column: "FamilyVisit");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IDCareer",
                table: "Employees",
                column: "IDCareer");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IDCompany",
                table: "Employees",
                column: "IDCompany");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Nationality",
                table: "Employees",
                column: "Nationality");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_TemporaryStay",
                table: "Employees",
                column: "TemporaryStay");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_WorkStatus",
                table: "Employees",
                columns: new[] { "WorkingStatus", "Hidden_flag" });

            migrationBuilder.CreateIndex(
                name: "IX_ForeignPersons_NationalityCode",
                table: "ForeignPersons",
                column: "NationalityCode");

            migrationBuilder.CreateIndex(
                name: "IX_ForeignPersons_PassportSearchKey",
                table: "ForeignPersons",
                column: "PassportSearchKey");

            migrationBuilder.CreateIndex(
                name: "IX_ForeignPersonSourceLinks_ForeignPersonId",
                table: "ForeignPersonSourceLinks",
                column: "ForeignPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ForeignPersonSourceLinks_SourceType_SourceId",
                table: "ForeignPersonSourceLinks",
                columns: new[] { "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImmigrationDocuments_ForeignPersonId_TypeCode_ValidTo",
                table: "ImmigrationDocuments",
                columns: new[] { "ForeignPersonId", "TypeCode", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBackups_EntityTypeId",
                table: "ImportBackups",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportBackups_ImportSessionId",
                table: "ImportBackups",
                column: "ImportSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportHistories_SessionId",
                table: "ImportHistories",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSubjects_ForeignPersonId_InspectionId",
                table: "InspectionSubjects",
                columns: new[] { "ForeignPersonId", "InspectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionSubjects_InspectionId",
                table: "InspectionSubjects",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionsV010_AccommodationId",
                table: "InspectionsV010",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionsV010_AdministrativeUnitId",
                table: "InspectionsV010",
                column: "AdministrativeUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionsV010_CompanySiteId",
                table: "InspectionsV010",
                column: "CompanySiteId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionsV010_InspectedAt",
                table: "InspectionsV010",
                column: "InspectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Investment_IDCompany",
                table: "Investment",
                column: "IDCompany");

            migrationBuilder.CreateIndex(
                name: "IX_LegacyChangeEvents_StatusCode_OccurredAt",
                table: "LegacyChangeEvents",
                columns: new[] { "StatusCode", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MigrationIssues_StatusCode_SourceType",
                table: "MigrationIssues",
                columns: new[] { "StatusCode", "SourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumbers_IDCompany",
                table: "PhoneNumbers",
                column: "IDCompany");

            migrationBuilder.CreateIndex(
                name: "IX_ResidencePeriods_AccommodationId",
                table: "ResidencePeriods",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidencePeriods_AdministrativeUnitId_ValidFrom_ValidTo",
                table: "ResidencePeriods",
                columns: new[] { "AdministrativeUnitId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_ResidencePeriods_ForeignPersonId",
                table: "ResidencePeriods",
                column: "ForeignPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidencePeriods_ResponsibleCompanyId",
                table: "ResidencePeriods",
                column: "ResponsibleCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaVersions_Version",
                table: "SchemaVersions",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteZoneMemberships_CompanySiteId_EconomicZoneId_ValidFrom",
                table: "SiteZoneMemberships",
                columns: new[] { "CompanySiteId", "EconomicZoneId", "ValidFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteZoneMemberships_EconomicZoneId",
                table: "SiteZoneMemberships",
                column: "EconomicZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_StayCases_ForeignPersonId_ValidFrom_ValidTo",
                table: "StayCases",
                columns: new[] { "ForeignPersonId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_StayCases_SponsorCompanyId",
                table: "StayCases",
                column: "SponsorCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_StayCases_SponsorForeignPersonId",
                table: "StayCases",
                column: "SponsorForeignPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_Sha256",
                table: "StoredFiles",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Nationality",
                table: "Students",
                column: "Nationality");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StatusHidden",
                table: "Students",
                columns: new[] { "Status", "Hidden_flag" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_VisaExpiry",
                table: "Students",
                column: "VisaExpiry");

            migrationBuilder.CreateIndex(
                name: "IX_WebRoleAssignments_AccountId_RoleCode_AdministrativeUnitId",
                table: "WebRoleAssignments",
                columns: new[] { "AccountId", "RoleCode", "AdministrativeUnitId" },
                unique: true,
                filter: "[AdministrativeUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WebRoleAssignments_AdministrativeUnitId",
                table: "WebRoleAssignments",
                column: "AdministrativeUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivedEmployees");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ColumnMappingTemplates");

            migrationBuilder.DropTable(
                name: "CompanyAccommodationAgreements");

            migrationBuilder.DropTable(
                name: "CompanyLegalDocuments");

            migrationBuilder.DropTable(
                name: "CompanyProfiles");

            migrationBuilder.DropTable(
                name: "CompanyRepresentatives");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "ElectronicIdentities");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "FamilyVisitDetails");

            migrationBuilder.DropTable(
                name: "ForeignPersonSourceLinks");

            migrationBuilder.DropTable(
                name: "ImmigrationDocuments");

            migrationBuilder.DropTable(
                name: "ImportBackups");

            migrationBuilder.DropTable(
                name: "ImportHistories");

            migrationBuilder.DropTable(
                name: "InspectionSubjects");

            migrationBuilder.DropTable(
                name: "Investment");

            migrationBuilder.DropTable(
                name: "LegacyChangeEvents");

            migrationBuilder.DropTable(
                name: "MigrationIssues");

            migrationBuilder.DropTable(
                name: "PhoneNumbers");

            migrationBuilder.DropTable(
                name: "ResidencePeriods");

            migrationBuilder.DropTable(
                name: "SchemaVersions");

            migrationBuilder.DropTable(
                name: "SiteZoneMemberships");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropTable(
                name: "WebCredentials");

            migrationBuilder.DropTable(
                name: "WebRoleAssignments");

            migrationBuilder.DropTable(
                name: "Attach");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropTable(
                name: "Careers");

            migrationBuilder.DropTable(
                name: "StayCases");

            migrationBuilder.DropTable(
                name: "InspectionsV010");

            migrationBuilder.DropTable(
                name: "EconomicZones");

            migrationBuilder.DropTable(
                name: "CareerGroups");

            migrationBuilder.DropTable(
                name: "ForeignPersons");

            migrationBuilder.DropTable(
                name: "AccommodationsV010");

            migrationBuilder.DropTable(
                name: "CompanySites");

            migrationBuilder.DropTable(
                name: "Nationality");

            migrationBuilder.DropTable(
                name: "AdministrativeUnits");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Fields");
        }
    }
}
