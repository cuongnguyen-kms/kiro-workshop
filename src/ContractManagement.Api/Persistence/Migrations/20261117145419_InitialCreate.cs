using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractManagement.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expense_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    expense_start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expense_end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expense_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.contract_id);
                });

            migrationBuilder.CreateTable(
                name: "expense_schedule_entries",
                columns: table => new
                {
                    expense_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_schedule_entries", x => x.expense_entry_id);
                    table.ForeignKey(
                        name: "FK_expense_schedule_entries_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "contract_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_streams",
                columns: table => new
                {
                    stream_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_streams", x => x.stream_id);
                    table.ForeignKey(
                        name: "FK_payment_streams_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "contract_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_schedule_entries",
                columns: table => new
                {
                    payment_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stream_id = table.Column<Guid>(type: "uuid", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_schedule_entries", x => x.payment_entry_id);
                    table.ForeignKey(
                        name: "FK_payment_schedule_entries_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "contract_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_schedule_entries_payment_streams_stream_id",
                        column: x => x.stream_id,
                        principalTable: "payment_streams",
                        principalColumn: "stream_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contracts_start_date_name_id",
                table: "contracts",
                columns: new[] { "start_date", "name", "contract_id" },
                descending: new[] { true, false, false });

            migrationBuilder.CreateIndex(
                name: "ix_expense_schedule_entries_contract_id_period_start",
                table: "expense_schedule_entries",
                columns: new[] { "contract_id", "period_start" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_schedule_entries_contract_id_due_date",
                table: "payment_schedule_entries",
                columns: new[] { "contract_id", "due_date" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_schedule_entries_stream_id",
                table: "payment_schedule_entries",
                column: "stream_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_streams_contract_id",
                table: "payment_streams",
                column: "contract_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense_schedule_entries");

            migrationBuilder.DropTable(
                name: "payment_schedule_entries");

            migrationBuilder.DropTable(
                name: "payment_streams");

            migrationBuilder.DropTable(
                name: "contracts");
        }
    }
}
