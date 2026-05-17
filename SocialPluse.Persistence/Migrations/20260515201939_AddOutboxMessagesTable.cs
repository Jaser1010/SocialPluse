using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialPluse.Persistence.Migrations
{
	/// <inheritdoc />
	public partial class AddOutboxMessagesTable : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// COMMENTED OUT / REMOVED BOOKMARKS TO PREVENT COLLISION
			/*
            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => new { x.UserId, x.PostId });
                    table.ForeignKey(
                        name: "FK_Bookmarks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookmarks_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            */

			migrationBuilder.CreateTable(
				name: "IdempotentRecords",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					MessageType = table.Column<string>(type: "text", nullable: false),
					ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_IdempotentRecords", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "OutboxMessages",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Type = table.Column<string>(type: "text", nullable: false),
					Content = table.Column<string>(type: "text", nullable: false),
					OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					Error = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_OutboxMessages", x => x.Id);
				});

			// COMMENTED OUT POSTID INDEX BECAUSE THE BOOKMARKS TABLE ALREADY HAS IT
			/*
            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_PostId",
                table: "Bookmarks",
                column: "PostId");
            */
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// COMMENTED OUT TO MATCH THE UP CHANGES
			// migrationBuilder.DropTable(name: "Bookmarks");

			migrationBuilder.DropTable(
				name: "IdempotentRecords");

			migrationBuilder.DropTable(
				name: "OutboxMessages");
		}
	}
}