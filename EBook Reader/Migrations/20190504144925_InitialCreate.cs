using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EBook_Reader.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicDocuments",
                columns: table => new
                {
                    PublicDocumentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    userName = table.Column<string>(nullable: true),
                    DocumentName = table.Column<string>(nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    documentType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicDocuments", x => x.PublicDocumentId);
                });

            migrationBuilder.CreateTable(
                name: "PublicComments",
                columns: table => new
                {
                    PublicCommentsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CommentDate = table.Column<DateTime>(nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    userName = table.Column<string>(nullable: true),
                    PublicDocumentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicComments", x => x.PublicCommentsId);
                    table.ForeignKey(
                        name: "FK_PublicComments_PublicDocuments_PublicDocumentId",
                        column: x => x.PublicDocumentId,
                        principalTable: "PublicDocuments",
                        principalColumn: "PublicDocumentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicComments_PublicDocumentId",
                table: "PublicComments",
                column: "PublicDocumentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicComments");

            migrationBuilder.DropTable(
                name: "PublicDocuments");
        }
    }
}
