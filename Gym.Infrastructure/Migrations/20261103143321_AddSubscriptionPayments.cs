using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE EXTENSION IF NOT EXISTS btree_gist;

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "Subscriptions" a
                        JOIN "Subscriptions" b
                          ON a."MemberId" = b."MemberId"
                         AND a."Id" < b."Id"
                         AND tstzrange(a."StartDate", a."EndDate") && tstzrange(b."StartDate", b."EndDate")
                    ) THEN
                        RAISE EXCEPTION 'Cannot add EX_Subscriptions_MemberId_Period: overlapping subscription periods exist. Resolve them manually; no data was deleted.';
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    AmountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberId",
                table: "Payments",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaidAt",
                table: "Payments",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.Sql(
                """
                ALTER TABLE "Subscriptions"
                    ADD CONSTRAINT "EX_Subscriptions_MemberId_Period"
                    EXCLUDE USING gist ("MemberId" WITH =, tstzrange("StartDate", "EndDate") WITH &&);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Subscriptions"
                    DROP CONSTRAINT IF EXISTS "EX_Subscriptions_MemberId_Period";
                """);

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
