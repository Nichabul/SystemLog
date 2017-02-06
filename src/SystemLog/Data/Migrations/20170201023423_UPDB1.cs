using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SystemLog.Data.Migrations
{
    public partial class UPDB1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companys",
                columns: table => new
                {
                    CompanyId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyName = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companys", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "Details",
                columns: table => new
                {
                    DetailsID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DetailWork = table.Column<string>(maxLength: 50, nullable: true),
                    DetailsCreatedate = table.Column<DateTime>(nullable: false),
                    DetailsDueDate = table.Column<DateTime>(nullable: false),
                    DetailsName = table.Column<string>(maxLength: 50, nullable: true),
                    DetailsNoteProblem = table.Column<string>(maxLength: 255, nullable: true),
                    DetailsNoteSolve = table.Column<string>(maxLength: 255, nullable: true),
                    DetailsStatus = table.Column<int>(maxLength: 3, nullable: false),
                    DetailsUsersId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Details", x => x.DetailsID);
                    table.ForeignKey(
                        name: "FK_Details_AspNetUsers_DetailsUsersId",
                        column: x => x.DetailsUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DepartmentsName = table.Column<string>(maxLength: 50, nullable: true),
                    DeptCompanyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentsId);
                    table.ForeignKey(
                        name: "FK_Departments_Companys_DeptCompanyId",
                        column: x => x.DeptCompanyId,
                        principalTable: "Companys",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserDepartmentsId",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserDepartmentsId",
                table: "AspNetUsers",
                column: "UserDepartmentsId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DeptCompanyId",
                table: "Departments",
                column: "DeptCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Details_DetailsUsersId",
                table: "Details",
                column: "DetailsUsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Departments_UserDepartmentsId",
                table: "AspNetUsers",
                column: "UserDepartmentsId",
                principalTable: "Departments",
                principalColumn: "DepartmentsId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Departments_UserDepartmentsId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserDepartmentsId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserDepartmentsId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Details");

            migrationBuilder.DropTable(
                name: "Companys");
        }
    }
}
