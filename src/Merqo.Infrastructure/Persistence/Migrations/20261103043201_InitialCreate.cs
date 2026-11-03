using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Merqo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activation_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    license_record_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    activated_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    machine_fingerprint_hash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    app_version = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    window_user_name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activation_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "application_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    key = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_application_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ordinal = table.Column<long>(type: "INTEGER", nullable: false),
                    occurred_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    username = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    action = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    entity_type = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    entity_id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    details = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    success = table.Column<bool>(type: "INTEGER", nullable: false),
                    correlation_id = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    previous_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    row_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "backup_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    file_path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    size_bytes = table.Column<long>(type: "INTEGER", nullable: false),
                    sha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    kind = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    is_verified = table.Column<bool>(type: "INTEGER", nullable: false),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_backup_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name_bn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "business_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    business_name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    owner_name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    address_line = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    mobile = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    logo_path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    currency_code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    currency_symbol = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    decimal_places = table.Column<int>(type: "INTEGER", nullable: false),
                    date_format = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    invoice_number_format = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    invoice_header = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    invoice_footer = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    default_vat_percent = table.Column<long>(type: "INTEGER", nullable: false),
                    allow_negative_stock = table.Column<bool>(type: "INTEGER", nullable: false),
                    round_grand_total_to_taka = table.Column<bool>(type: "INTEGER", nullable: false),
                    receipt_paper_width_mm = table.Column<int>(type: "INTEGER", nullable: false),
                    receipt_copies = table.Column<int>(type: "INTEGER", nullable: false),
                    receipt_printer_name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_raster_receipt = table.Column<bool>(type: "INTEGER", nullable: false),
                    backup_folder = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    automatic_backup_retention_count = table.Column<int>(type: "INTEGER", nullable: false),
                    automatic_backup_enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    last_backup_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_business_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name_bn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chart_of_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    name_bn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    account_type = table.Column<string>(type: "TEXT", nullable: false),
                    is_system = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chart_of_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    opening_balance = table.Column<long>(type: "INTEGER", nullable: false),
                    credit_limit = table.Column<long>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_sequences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sequence_key = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    month = table.Column<int>(type: "INTEGER", nullable: false),
                    last_number = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_sequences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expense_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name_bn = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    name_en = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    account_code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    is_system = table.Column<bool>(type: "INTEGER", nullable: false),
                    sort_order = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expense_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    entry_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    entry_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    memo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    source_type = table.Column<string>(type: "TEXT", nullable: false),
                    source_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    source_number = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_reversal = table.Column<bool>(type: "INTEGER", nullable: false),
                    reversal_of_entry_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    posted_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "license_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    license_id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    business_name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    business_identity = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    license_type = table.Column<string>(type: "TEXT", nullable: false),
                    binding_policy = table.Column<string>(type: "TEXT", nullable: false),
                    issued_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    activated_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    serial = table.Column<int>(type: "INTEGER", nullable: false),
                    payload_json = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    signature = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    public_key_id = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    superseded_at_utc = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_license_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "license_recovery_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    original_license_id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    replacement_license_id = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    issued_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_license_recovery_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "login_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    username = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    occurred_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    success = table.Column<bool>(type: "INTEGER", nullable: false),
                    reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_login_attempts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "machine_bindings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    license_record_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    fingerprint_hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    component_hashes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    first_seen_utc = table.Column<string>(type: "TEXT", nullable: false),
                    last_seen_utc = table.Column<string>(type: "TEXT", nullable: false),
                    is_current = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_machine_bindings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name_bn = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    name_en = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_cash = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_system = table.Column<bool>(type: "INTEGER", nullable: false),
                    sort_order = table.Column<int>(type: "INTEGER", nullable: false),
                    cash_account_code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_system = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stock_adjustments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    adjustment_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_voided = table.Column<bool>(type: "INTEGER", nullable: false),
                    voided_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    void_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    adjustment_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    adjustment_type = table.Column<string>(type: "TEXT", nullable: false),
                    reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    total_value_impact = table.Column<long>(type: "INTEGER", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_adjustments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    business_name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    contact_person = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    phone = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    email = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    opening_balance = table.Column<long>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name_bn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    name_en = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    short_bn = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    short_en = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    base_unit_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    factor_to_base = table.Column<long>(type: "INTEGER", nullable: false),
                    is_system_unit = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_units", x => x.id);
                    table.ForeignKey(
                        name: "fk_units_units_base_unit_id",
                        column: x => x.base_unit_id,
                        principalTable: "units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    username = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false, collation: "NOCASE"),
                    full_name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    is_owner = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    must_change_password = table.Column<bool>(type: "INTEGER", nullable: false),
                    last_login_utc = table.Column<string>(type: "TEXT", nullable: true),
                    failed_attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    locked_until_utc = table.Column<string>(type: "TEXT", nullable: true),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer_ledger_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    customer_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    entry_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    entry_type = table.Column<string>(type: "TEXT", nullable: false),
                    debit = table.Column<long>(type: "INTEGER", nullable: false),
                    credit = table.Column<long>(type: "INTEGER", nullable: false),
                    balance_after = table.Column<long>(type: "INTEGER", nullable: false),
                    reference_type = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    reference_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_ledger_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_ledger_entries_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customer_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    receipt_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    customer_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    method_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    amount = table.Column<long>(type: "INTEGER", nullable: false),
                    paid_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    reference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_reversed = table.Column<bool>(type: "INTEGER", nullable: false),
                    reversed_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    reversed_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    reversal_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_payments_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    invoice_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    sale_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    customer_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    customer_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    subtotal = table.Column<long>(type: "INTEGER", nullable: false),
                    line_discount_total = table.Column<long>(type: "INTEGER", nullable: false),
                    invoice_discount = table.Column<long>(type: "INTEGER", nullable: false),
                    tax_total = table.Column<long>(type: "INTEGER", nullable: false),
                    rounding_adjustment = table.Column<long>(type: "INTEGER", nullable: false),
                    grand_total = table.Column<long>(type: "INTEGER", nullable: false),
                    paid_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    due_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    change_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    total_cost = table.Column<long>(type: "INTEGER", nullable: false),
                    gross_profit = table.Column<long>(type: "INTEGER", nullable: false),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    voided_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    voided_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    void_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sales", x => x.id);
                    table.ForeignKey(
                        name: "fk_sales_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    expense_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    expense_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    expense_category_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    amount = table.Column<long>(type: "INTEGER", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    method_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    reference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    attachment_path = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_voided = table.Column<bool>(type: "INTEGER", nullable: false),
                    voided_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    void_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_expenses_expense_categories_expense_category_id",
                        column: x => x.expense_category_id,
                        principalTable: "expense_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "journal_entry_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    journal_entry_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    account_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    account_code_snapshot = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    debit = table.Column<long>(type: "INTEGER", nullable: false),
                    credit = table.Column<long>(type: "INTEGER", nullable: false),
                    memo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    customer_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    supplier_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_entry_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_chart_of_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_journal_entries_journal_entry_id",
                        column: x => x.journal_entry_id,
                        principalTable: "journal_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    role_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    permission_code = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_adjustment_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    stock_adjustment_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    previous_quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    adjusted_quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    final_quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_adjustment_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_adjustment_items_stock_adjustments_stock_adjustment_id",
                        column: x => x.stock_adjustment_id,
                        principalTable: "stock_adjustments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    purchase_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    purchase_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    supplier_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    supplier_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "TEXT", nullable: false),
                    subtotal = table.Column<long>(type: "INTEGER", nullable: false),
                    discount_total = table.Column<long>(type: "INTEGER", nullable: false),
                    tax_total = table.Column<long>(type: "INTEGER", nullable: false),
                    grand_total = table.Column<long>(type: "INTEGER", nullable: false),
                    paid_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    due_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    voided_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    voided_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    void_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchases", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchases_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplier_ledger_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    supplier_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    entry_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    entry_type = table.Column<string>(type: "TEXT", nullable: false),
                    debit = table.Column<long>(type: "INTEGER", nullable: false),
                    credit = table.Column<long>(type: "INTEGER", nullable: false),
                    balance_after = table.Column<long>(type: "INTEGER", nullable: false),
                    reference_type = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    reference_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supplier_ledger_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_supplier_ledger_entries_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplier_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    voucher_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    supplier_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    method_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    amount = table.Column<long>(type: "INTEGER", nullable: false),
                    paid_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    reference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_reversed = table.Column<bool>(type: "INTEGER", nullable: false),
                    reversed_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    reversed_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    reversal_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_supplier_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_supplier_payments_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name_bn = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    name_en = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    code = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    spec = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    sku = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    category_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    brand_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    unit_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    purchase_price = table.Column<long>(type: "INTEGER", nullable: false),
                    selling_price = table.Column<long>(type: "INTEGER", nullable: false),
                    wholesale_price = table.Column<long>(type: "INTEGER", nullable: false),
                    minimum_stock = table.Column<long>(type: "INTEGER", nullable: false),
                    tax_rate_percent = table.Column<long>(type: "INTEGER", nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    archived_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_units_unit_id",
                        column: x => x.unit_id,
                        principalTable: "units",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    role_id = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sale_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    method_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    amount = table.Column<long>(type: "INTEGER", nullable: false),
                    paid_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    reference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_reversed = table.Column<bool>(type: "INTEGER", nullable: false),
                    reversed_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    reversed_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    reversal_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sale_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_sale_payments_payment_methods_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_methods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sale_payments_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale_returns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    return_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_voided = table.Column<bool>(type: "INTEGER", nullable: false),
                    voided_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    void_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    sale_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    return_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    refund_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    restock_value = table.Column<long>(type: "INTEGER", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_refund_to_cash = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sale_returns", x => x.id);
                    table.ForeignKey(
                        name: "fk_sale_returns_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    purchase_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    payment_method_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    method_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    amount = table.Column<long>(type: "INTEGER", nullable: false),
                    paid_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    reference = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    is_reversed = table.Column<bool>(type: "INTEGER", nullable: false),
                    reversed_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    reversed_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    reversal_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_payments_purchases_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_returns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    return_number = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    idempotency_key = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    is_voided = table.Column<bool>(type: "INTEGER", nullable: false),
                    voided_at_utc = table.Column<string>(type: "TEXT", nullable: true),
                    void_reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    purchase_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    return_date_utc = table.Column<string>(type: "TEXT", nullable: false),
                    reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    credit_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_returns", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_returns_purchases_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_barcodes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    barcode = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    is_primary = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_barcodes", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_barcodes_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    purchase_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    unit_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    base_quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<long>(type: "INTEGER", nullable: false),
                    line_discount = table.Column<long>(type: "INTEGER", nullable: false),
                    tax_rate_percent = table.Column<long>(type: "INTEGER", nullable: false),
                    tax_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    line_total = table.Column<long>(type: "INTEGER", nullable: false),
                    returned_quantity = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_items_purchases_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sale_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_name_snapshot = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    unit_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    base_quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    unit_price = table.Column<long>(type: "INTEGER", nullable: false),
                    gross_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    line_discount = table.Column<long>(type: "INTEGER", nullable: false),
                    invoice_discount_share = table.Column<long>(type: "INTEGER", nullable: false),
                    tax_rate_percent = table.Column<long>(type: "INTEGER", nullable: false),
                    tax_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    line_total = table.Column<long>(type: "INTEGER", nullable: false),
                    net_revenue = table.Column<long>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<long>(type: "INTEGER", nullable: false),
                    cost_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    gross_profit = table.Column<long>(type: "INTEGER", nullable: false),
                    returned_quantity = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sale_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_sale_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sale_items_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_balances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    quantity_on_hand = table.Column<long>(type: "INTEGER", nullable: false),
                    average_cost = table.Column<long>(type: "INTEGER", nullable: false),
                    updated_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_balances", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_balances_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    moved_at_utc = table.Column<string>(type: "TEXT", nullable: false),
                    movement_type = table.Column<string>(type: "TEXT", nullable: false),
                    quantity_in = table.Column<long>(type: "INTEGER", nullable: false),
                    quantity_out = table.Column<long>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<long>(type: "INTEGER", nullable: false),
                    balance_after = table.Column<long>(type: "INTEGER", nullable: false),
                    reference_type = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    reference_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    reference_number = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_movements", x => x.id);
                    table.ForeignKey(
                        name: "fk_stock_movements_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_return_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    purchase_return_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    purchase_item_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    base_quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<long>(type: "INTEGER", nullable: false),
                    amount = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_return_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_purchase_return_items_purchase_items_purchase_item_id",
                        column: x => x.purchase_item_id,
                        principalTable: "purchase_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_return_items_purchase_returns_purchase_return_id",
                        column: x => x.purchase_return_id,
                        principalTable: "purchase_returns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale_return_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sale_return_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sale_item_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    product_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    base_quantity = table.Column<long>(type: "INTEGER", nullable: false),
                    unit_price = table.Column<long>(type: "INTEGER", nullable: false),
                    refund_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    unit_cost = table.Column<long>(type: "INTEGER", nullable: false),
                    cost_amount = table.Column<long>(type: "INTEGER", nullable: false),
                    is_restocked = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sale_return_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_sale_return_items_sale_items_sale_item_id",
                        column: x => x.sale_item_id,
                        principalTable: "sale_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sale_return_items_sale_returns_sale_return_id",
                        column: x => x.sale_return_id,
                        principalTable: "sale_returns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activation_records_activated_at_utc",
                table: "activation_records",
                column: "activated_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_application_settings_key",
                table: "application_settings",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_occurred_at_utc",
                table: "audit_logs",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_ordinal",
                table: "audit_logs",
                column: "ordinal",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_backup_history_created_at_utc",
                table: "backup_history",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_brands_name_bn",
                table: "brands",
                column: "name_bn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_name_bn",
                table: "categories",
                column: "name_bn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chart_of_accounts_code",
                table: "chart_of_accounts",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_ledger_entries_customer_id_entry_date_utc",
                table: "customer_ledger_entries",
                columns: new[] { "customer_id", "entry_date_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_customer_ledger_entries_reference_id",
                table: "customer_ledger_entries",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_payments_customer_id_paid_at_utc",
                table: "customer_payments",
                columns: new[] { "customer_id", "paid_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_customer_payments_idempotency_key",
                table: "customer_payments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_payments_receipt_number",
                table: "customer_payments",
                column: "receipt_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_name",
                table: "customers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_customers_phone",
                table: "customers",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "ix_document_sequences_sequence_key_year_month",
                table: "document_sequences",
                columns: new[] { "sequence_key", "year", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expense_categories_name_bn",
                table: "expense_categories",
                column: "name_bn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expenses_expense_category_id",
                table: "expenses",
                column: "expense_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_expense_date_utc",
                table: "expenses",
                column: "expense_date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_expense_number",
                table: "expenses",
                column: "expense_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expenses_idempotency_key",
                table: "expenses",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_entry_date_utc",
                table: "journal_entries",
                column: "entry_date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_entry_number",
                table: "journal_entries",
                column: "entry_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_source_type_source_id",
                table: "journal_entries",
                columns: new[] { "source_type", "source_id" });

            migrationBuilder.CreateIndex(
                name: "ix_journal_entry_lines_account_id",
                table: "journal_entry_lines",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entry_lines_journal_entry_id",
                table: "journal_entry_lines",
                column: "journal_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_license_records_license_id",
                table: "license_records",
                column: "license_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_login_attempts_occurred_at_utc",
                table: "login_attempts",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_machine_bindings_fingerprint_hash",
                table: "machine_bindings",
                column: "fingerprint_hash");

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_name_bn",
                table: "payment_methods",
                column: "name_bn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_barcodes_barcode",
                table: "product_barcodes",
                column: "barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_barcodes_product_id",
                table: "product_barcodes",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_brand_id",
                table: "products",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_code",
                table: "products",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_is_active_is_archived",
                table: "products",
                columns: new[] { "is_active", "is_archived" });

            migrationBuilder.CreateIndex(
                name: "ix_products_name_bn",
                table: "products",
                column: "name_bn");

            migrationBuilder.CreateIndex(
                name: "ix_products_unit_id",
                table: "products",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_product_id",
                table: "purchase_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_items_purchase_id",
                table: "purchase_items",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_payments_idempotency_key",
                table: "purchase_payments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_payments_purchase_id",
                table: "purchase_payments",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_return_items_purchase_item_id",
                table: "purchase_return_items",
                column: "purchase_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_return_items_purchase_return_id",
                table: "purchase_return_items",
                column: "purchase_return_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_returns_idempotency_key",
                table: "purchase_returns",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchase_returns_purchase_id",
                table: "purchase_returns",
                column: "purchase_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_returns_return_number",
                table: "purchase_returns",
                column: "return_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchases_idempotency_key",
                table: "purchases",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchases_purchase_date_utc",
                table: "purchases",
                column: "purchase_date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_purchase_number",
                table: "purchases",
                column: "purchase_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purchases_supplier_id",
                table: "purchases",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_role_id_permission_code",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sale_items_product_id",
                table: "sale_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_items_sale_id",
                table: "sale_items",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_payments_idempotency_key",
                table: "sale_payments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sale_payments_payment_method_id",
                table: "sale_payments",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_payments_sale_id",
                table: "sale_payments",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_return_items_sale_item_id",
                table: "sale_return_items",
                column: "sale_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_return_items_sale_return_id",
                table: "sale_return_items",
                column: "sale_return_id");

            migrationBuilder.CreateIndex(
                name: "ix_sale_returns_idempotency_key",
                table: "sale_returns",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sale_returns_return_date_utc",
                table: "sale_returns",
                column: "return_date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_sale_returns_return_number",
                table: "sale_returns",
                column: "return_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sale_returns_sale_id",
                table: "sale_returns",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "ix_sales_customer_id_sale_date_utc",
                table: "sales",
                columns: new[] { "customer_id", "sale_date_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_sales_idempotency_key",
                table: "sales",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_invoice_number",
                table: "sales",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sales_sale_date_utc",
                table: "sales",
                column: "sale_date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_stock_adjustment_items_stock_adjustment_id",
                table: "stock_adjustment_items",
                column: "stock_adjustment_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_adjustments_adjustment_date_utc",
                table: "stock_adjustments",
                column: "adjustment_date_utc");

            migrationBuilder.CreateIndex(
                name: "ix_stock_adjustments_adjustment_number",
                table: "stock_adjustments",
                column: "adjustment_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_adjustments_idempotency_key",
                table: "stock_adjustments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_balances_product_id",
                table: "stock_balances",
                column: "product_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_moved_at_utc",
                table: "stock_movements",
                column: "moved_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_product_id_moved_at_utc",
                table: "stock_movements",
                columns: new[] { "product_id", "moved_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_movements_reference_type_reference_id",
                table: "stock_movements",
                columns: new[] { "reference_type", "reference_id" });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_ledger_entries_reference_id",
                table: "supplier_ledger_entries",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_ledger_entries_supplier_id_entry_date_utc",
                table: "supplier_ledger_entries",
                columns: new[] { "supplier_id", "entry_date_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_payments_idempotency_key",
                table: "supplier_payments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_supplier_payments_supplier_id_paid_at_utc",
                table: "supplier_payments",
                columns: new[] { "supplier_id", "paid_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_supplier_payments_voucher_number",
                table: "supplier_payments",
                column: "voucher_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_name",
                table: "suppliers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_units_base_unit_id",
                table: "units",
                column: "base_unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_units_name_bn",
                table: "units",
                column: "name_bn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id_role_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activation_records");

            migrationBuilder.DropTable(
                name: "application_settings");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "backup_history");

            migrationBuilder.DropTable(
                name: "business_settings");

            migrationBuilder.DropTable(
                name: "customer_ledger_entries");

            migrationBuilder.DropTable(
                name: "customer_payments");

            migrationBuilder.DropTable(
                name: "document_sequences");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "journal_entry_lines");

            migrationBuilder.DropTable(
                name: "license_records");

            migrationBuilder.DropTable(
                name: "license_recovery_records");

            migrationBuilder.DropTable(
                name: "login_attempts");

            migrationBuilder.DropTable(
                name: "machine_bindings");

            migrationBuilder.DropTable(
                name: "product_barcodes");

            migrationBuilder.DropTable(
                name: "purchase_payments");

            migrationBuilder.DropTable(
                name: "purchase_return_items");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "sale_payments");

            migrationBuilder.DropTable(
                name: "sale_return_items");

            migrationBuilder.DropTable(
                name: "stock_adjustment_items");

            migrationBuilder.DropTable(
                name: "stock_balances");

            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "supplier_ledger_entries");

            migrationBuilder.DropTable(
                name: "supplier_payments");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "expense_categories");

            migrationBuilder.DropTable(
                name: "chart_of_accounts");

            migrationBuilder.DropTable(
                name: "journal_entries");

            migrationBuilder.DropTable(
                name: "purchase_items");

            migrationBuilder.DropTable(
                name: "purchase_returns");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "sale_items");

            migrationBuilder.DropTable(
                name: "sale_returns");

            migrationBuilder.DropTable(
                name: "stock_adjustments");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "purchases");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "sales");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "customers");
        }
    }
}
