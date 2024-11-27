using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    SNo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, computedColumnSql: "(CONVERT([varchar](50),concat(CONVERT([varchar](10),left(lower([RealmName]),(10))),CONVERT([varchar](1),'-'),CONVERT([varchar](3),right(lower(CONVERT([varchar](50),[TUId])),(3))),CONVERT([varchar](2),right('00'+CONVERT([varchar],[SNo]),(2))))))", stored: true),
                    RealmName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    TUId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    SaverId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.SNo);
                    table.UniqueConstraint("AK_Tenant_TenantId", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceHierarchy",
                columns: table => new
                {
                    Sno = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ParentAccId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    SaverId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceHierarchy_Sno", x => x.Sno)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_ServiceHierarchy_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "TenantId");
                });

            migrationBuilder.CreateIndex(
                name: "IDX_unique_ServiceHierarchy_TenantId_ParentAccId_AccountId",
                table: "ServiceHierarchy",
                columns: new[] { "TenantId", "ParentAccId", "AccountId" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IDX_Tenant_RealmName_OwnerId",
                table: "Tenant",
                columns: new[] { "RealmName", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_Tenant_TenantId",
                table: "Tenant",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceHierarchy");

            migrationBuilder.DropTable(
                name: "Tenant");
        }
    }
}
