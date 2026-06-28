using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudentManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "ClassId", "ClassName", "FacultyId", "TeacherId" },
                values: new object[,]
                {
                    { "CK18-01", "Cơ khí 18-01", "CK", null },
                    { "KT18-01", "Kinh tế 18-01", "KT", null }
                });

            migrationBuilder.InsertData(
                table: "Grades",
                columns: new[] { "Id", "Attendance", "Final", "LetterGrade", "Midterm", "Semester", "StudentId", "SubjectCode", "SubjectName", "TotalScore", "Year" },
                values: new object[,]
                {
                    { 1, 9m, 8.5m, "B+", 8m, "Học kỳ 2 - 2023", "18710205", "CNPM_01", "Công nghệ phần mềm", 8.4m, 2023 },
                    { 2, 10m, 9m, "A", 9m, "Học kỳ 2 - 2023", "18710205", "CSDL_01", "Cơ sở dữ liệu", 9.1m, 2023 },
                    { 3, 8m, 6m, "C+", 7m, "Học kỳ 2 - 2023", "18710204", "CNPM_01", "Công nghệ phần mềm", 6.5m, 2023 }
                });

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710204",
                column: "PasswordHash",
                value: "$2a$11$0v6jC.8f2d.1v.Q7L0.8o.9Q5i.4E1v2d6s.4f2d.1v.Q7L0.8o");

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710205",
                column: "PasswordHash",
                value: "$2a$11$0v6jC.8f2d.1v.Q7L0.8o.9Q5i.4E1v2d6s.4f2d.1v.Q7L0.8o");

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "MSSV", "ClassId", "DateOfBirth", "Email", "FacultyId", "FullName", "Gender", "Hometown", "PasswordHash", "Phone", "Status" },
                values: new object[,]
                {
                    { "18710206", "CNTT18-07", new DateTime(2001, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "lan@xxx.edu", "CNTT", "Trần Thị Lan", "Nữ", "Hải Phòng", "$2a$11$0v6jC.8f2d.1v.Q7L0.8o.9Q5i.4E1v2d6s.4f2d.1v.Q7L0.8o", null, "Đang học" },
                    { "18710208", "CNTT18-06", new DateTime(2000, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "huy@xxx.edu", "CNTT", "Phạm Quang Huy", "Nam", "Hà Nội", "$2a$11$0v6jC.8f2d.1v.Q7L0.8o.9Q5i.4E1v2d6s.4f2d.1v.Q7L0.8o", null, "Đang học" }
                });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "SubjectCode", "Credits", "FacultyId", "SubjectName" },
                values: new object[,]
                {
                    { "KTCT_01", 2, "KT", "Kinh tế chính trị" },
                    { "KTLT_01", 3, "CNTT", "Kỹ thuật lập trình" },
                    { "MMT_01", 3, "CNTT", "Mạng máy tính" },
                    { "NLCB_01", 3, "KT", "Nguyên lý cơ bản" },
                    { "VLDC_01", 2, "CB", "Vật lý đại cương" }
                });

            migrationBuilder.InsertData(
                table: "Teachers",
                columns: new[] { "TeacherId", "Email", "FacultyId", "FullName", "PasswordHash", "Phone", "Status" },
                values: new object[,]
                {
                    { "GV_004", "lma@dainam.edu.vn", "KT", "Lê Mai Anh", null, null, "Active" },
                    { "GV_005", "ht@dainam.edu.vn", "CB", "Hoàng Tuấn", null, null, "Active" },
                    { "GV_006", "dqm@dainam.edu.vn", "CK", "Đinh Quang Minh", null, null, "Active" }
                });

            migrationBuilder.InsertData(
                table: "CourseClasses",
                columns: new[] { "CourseClassId", "CurrentStudents", "MaxStudents", "Schedule", "Semester", "Status", "SubjectCode", "TeacherId" },
                values: new object[,]
                {
                    { "LHP_KTCT_01", 0, 60, "Thứ 7 (Ca 2)", "Học kỳ 1 - 2024", "Mở đăng ký", "KTCT_01", "GV_004" },
                    { "LHP_KTLT_01", 2, 50, "Thứ 6 (Ca 1)", "Học kỳ 1 - 2024", "Mở đăng ký", "KTLT_01", "GV_001" },
                    { "LHP_MMT_01", 1, 45, "Thứ 4 (Ca 3)", "Học kỳ 1 - 2024", "Mở đăng ký", "MMT_01", "GV_002" }
                });

            migrationBuilder.InsertData(
                table: "Grades",
                columns: new[] { "Id", "Attendance", "Final", "LetterGrade", "Midterm", "Semester", "StudentId", "SubjectCode", "SubjectName", "TotalScore", "Year" },
                values: new object[] { 4, 10m, 7.5m, "B+", 8.5m, "Học kỳ 2 - 2023", "18710206", "TRR_01", "Toán rời rạc", 8.05m, 2023 });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "MSSV", "ClassId", "DateOfBirth", "Email", "FacultyId", "FullName", "Gender", "Hometown", "PasswordHash", "Phone", "Status" },
                values: new object[,]
                {
                    { "18710207", "KT18-01", new DateTime(2000, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "bach@xxx.edu", "KT", "Lê Văn Bách", "Nam", "Nam Định", "$2a$11$0v6jC.8f2d.1v.Q7L0.8o.9Q5i.4E1v2d6s.4f2d.1v.Q7L0.8o", null, "Đang học" },
                    { "18710209", "KT18-01", new DateTime(2001, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "ngoc@xxx.edu", "KT", "Nguyễn Bích Ngọc", "Nữ", "Thái Bình", "$2a$11$0v6jC.8f2d.1v.Q7L0.8o.9Q5i.4E1v2d6s.4f2d.1v.Q7L0.8o", null, "Đang học" }
                });

            migrationBuilder.InsertData(
                table: "Registrations",
                columns: new[] { "RegistrationId", "CourseClassId", "RegistrationDate", "Semester", "Status", "StudentId", "Year" },
                values: new object[,]
                {
                    { 1, "LHP_KTLT_01", new DateTime(2024, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Học kỳ 1 - 2024", "Đã đăng ký", "18710205", 2024 },
                    { 2, "LHP_KTLT_01", new DateTime(2024, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Học kỳ 1 - 2024", "Đã đăng ký", "18710206", 2024 },
                    { 3, "LHP_MMT_01", new DateTime(2024, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Học kỳ 1 - 2024", "Đã đăng ký", "18710205", 2024 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "ClassId",
                keyValue: "CK18-01");

            migrationBuilder.DeleteData(
                table: "CourseClasses",
                keyColumn: "CourseClassId",
                keyValue: "LHP_KTCT_01");

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Grades",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Registrations",
                keyColumn: "RegistrationId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Registrations",
                keyColumn: "RegistrationId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Registrations",
                keyColumn: "RegistrationId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710207");

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710208");

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710209");

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "SubjectCode",
                keyValue: "NLCB_01");

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "SubjectCode",
                keyValue: "VLDC_01");

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "TeacherId",
                keyValue: "GV_005");

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "TeacherId",
                keyValue: "GV_006");

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "ClassId",
                keyValue: "KT18-01");

            migrationBuilder.DeleteData(
                table: "CourseClasses",
                keyColumn: "CourseClassId",
                keyValue: "LHP_KTLT_01");

            migrationBuilder.DeleteData(
                table: "CourseClasses",
                keyColumn: "CourseClassId",
                keyValue: "LHP_MMT_01");

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710206");

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "SubjectCode",
                keyValue: "KTCT_01");

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "TeacherId",
                keyValue: "GV_004");

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "SubjectCode",
                keyValue: "KTLT_01");

            migrationBuilder.DeleteData(
                table: "Subjects",
                keyColumn: "SubjectCode",
                keyValue: "MMT_01");

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710204",
                column: "PasswordHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "Students",
                keyColumn: "MSSV",
                keyValue: "18710205",
                column: "PasswordHash",
                value: null);
        }
    }
}
