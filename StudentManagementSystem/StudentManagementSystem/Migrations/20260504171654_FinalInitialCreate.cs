using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudentManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class FinalInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    FacultyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FacultyName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculties", x => x.FacultyId);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Credits = table.Column<int>(type: "int", nullable: false),
                    FacultyId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectCode);
                    table.ForeignKey(
                        name: "FK_Subjects_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "FacultyId");
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    TeacherId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FacultyId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.TeacherId);
                    table.ForeignKey(
                        name: "FK_Teachers_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "FacultyId");
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ClassId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacultyId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TeacherId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.ClassId);
                    table.ForeignKey(
                        name: "FK_Classes_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "FacultyId");
                    table.ForeignKey(
                        name: "FK_Classes_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId");
                });

            migrationBuilder.CreateTable(
                name: "CourseClasses",
                columns: table => new
                {
                    CourseClassId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubjectCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeacherId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Semester = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxStudents = table.Column<int>(type: "int", nullable: false),
                    CurrentStudents = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Schedule = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseClasses", x => x.CourseClassId);
                    table.ForeignKey(
                        name: "FK_CourseClasses_Subjects_SubjectCode",
                        column: x => x.SubjectCode,
                        principalTable: "Subjects",
                        principalColumn: "SubjectCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseClasses_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "TeacherId");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    MSSV = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hometown = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClassId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FacultyId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.MSSV);
                    table.ForeignKey(
                        name: "FK_Students_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId");
                    table.ForeignKey(
                        name: "FK_Students_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "FacultyId");
                });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SubjectCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Midterm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Final = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LetterGrade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Semester = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grades_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "MSSV");
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    RegistrationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CourseClassId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Semester = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.RegistrationId);
                    table.ForeignKey(
                        name: "FK_Registrations_CourseClasses_CourseClassId",
                        column: x => x.CourseClassId,
                        principalTable: "CourseClasses",
                        principalColumn: "CourseClassId");
                    table.ForeignKey(
                        name: "FK_Registrations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "MSSV");
                });

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "FacultyId", "FacultyName" },
                values: new object[,]
                {
                    { "CB", "Khoa Cơ bản" },
                    { "CK", "Cơ khí" },
                    { "CNTT", "Công nghệ Thông tin" },
                    { "KT", "Kinh tế" }
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "ClassId", "ClassName", "FacultyId", "TeacherId" },
                values: new object[,]
                {
                    { "CNTT18-06", "CNTT 18-06", "CNTT", null },
                    { "CNTT18-07", "CNTT 18-07", "CNTT", null }
                });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "SubjectCode", "Credits", "FacultyId", "SubjectName" },
                values: new object[,]
                {
                    { "CNPM_01", 3, "CNTT", "Công nghệ phần mềm" },
                    { "CSDL_01", 3, "CNTT", "Cơ sở dữ liệu" },
                    { "LTW_01", 3, "CNTT", "Lập trình Web" },
                    { "TRR_01", 2, "CB", "Toán rời rạc" }
                });

            migrationBuilder.InsertData(
                table: "Teachers",
                columns: new[] { "TeacherId", "Email", "FacultyId", "FullName", "Phone", "Status" },
                values: new object[,]
                {
                    { "GV_001", "ptnga@dainam.edu.vn", "CNTT", "Phạm Thị Tố Nga", null, "Active" },
                    { "GV_002", "nva@dainam.edu.vn", "CNTT", "Nguyễn Văn A", null, "Active" },
                    { "GV_003", "tbc@dainam.edu.vn", "CNTT", "Trần Bảo Châu", null, "Active" }
                });

            migrationBuilder.InsertData(
                table: "CourseClasses",
                columns: new[] { "CourseClassId", "CurrentStudents", "MaxStudents", "Schedule", "Semester", "Status", "SubjectCode", "TeacherId" },
                values: new object[,]
                {
                    { "LHP_CNPM_01", 45, 50, "Thứ 3 (Ca 1)", "Học kỳ 1 - 2024", "Mở đăng ký", "CNPM_01", "GV_001" },
                    { "LHP_CSDL_02", 50, 50, "Thứ 5 (Ca 2)", "Học kỳ 1 - 2024", "Đã đầy", "CSDL_01", "GV_002" },
                    { "LHP_LTW_01", 0, 40, "Thứ 2 (Ca 3)", "Học kỳ 1 - 2024", "Chờ duyệt", "LTW_01", "GV_003" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "MSSV", "ClassId", "DateOfBirth", "Email", "FacultyId", "FullName", "Gender", "Hometown", "Phone", "Status" },
                values: new object[,]
                {
                    { "18710204", "CNTT18-06", new DateTime(2000, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "nam@xxx.edu", "CNTT", "Đào Ngọc Nam", "Nam", "Hà Nội", null, "Đang học" },
                    { "18710205", "CNTT18-06", new DateTime(2000, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "trung@xxx.edu", "CNTT", "Nguyễn Quang Trung", "Nam", "Hà Nội", null, "Đang học" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_FacultyId",
                table: "Classes",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_TeacherId",
                table: "Classes",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseClasses_SubjectCode",
                table: "CourseClasses",
                column: "SubjectCode");

            migrationBuilder.CreateIndex(
                name: "IX_CourseClasses_TeacherId",
                table: "CourseClasses",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_StudentId",
                table: "Grades",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_CourseClassId",
                table: "Registrations",
                column: "CourseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_StudentId",
                table: "Registrations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassId",
                table: "Students",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_FacultyId",
                table: "Students",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_FacultyId",
                table: "Subjects",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_FacultyId",
                table: "Teachers",
                column: "FacultyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropTable(
                name: "CourseClasses");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Faculties");
        }
    }
}
