using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WPFCore.App.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "menu_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ActionKey = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Glyph = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_menu_items_menu_items_ParentId",
                        column: x => x.ParentId,
                        principalTable: "menu_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "menu_items",
                columns: new[] { "Id", "ActionKey", "Glyph", "IsEnabled", "ParentId", "SortOrder", "Title" },
                values: new object[,]
                {
                    { 1, "Dashboard", null, true, null, 1, "Thống kê" },
                    { 10, null, null, true, null, 2, "Danh mục" },
                    { 20, null, null, true, null, 3, "Hệ thống" },
                    { 30, null, null, true, null, 4, "Trợ giúp" },
                    { 11, "MapLayer", null, true, 10, 1, "Quản lý lớp dữ liệu bản đồ" },
                    { 12, "GeoFeature", null, true, 10, 2, "Quản lý đối tượng địa lý" },
                    { 13, "DataSource", null, true, 10, 3, "Quản lý nguồn dữ liệu" },
                    { 14, "CheckRules", null, true, 10, 4, "Quản lý quy định kiểm tra" },
                    { 15, "MapType", null, true, 10, 5, "Quản lý loại bản đồ" },
                    { 16, "Users", null, true, 10, 6, "Quản lý người dùng" },
                    { 21, "RunCheck", null, true, 20, 1, "Kiểm tra dữ liệu bản đồ" },
                    { 22, "CheckResults", null, true, 20, 2, "Xem kết quả kiểm tra" },
                    { 23, "SummaryReport", null, true, 20, 3, "Báo cáo tổng hợp" },
                    { 24, "ExportReport", null, true, 20, 4, "Xuất báo cáo (Excel/PDF)" },
                    { 25, "CustomerList", null, true, 20, 5, "Quản lý khách hàng (CRUD mẫu)" },
                    { 31, "About", null, true, 30, 1, "Giới thiệu" },
                    { 32, "Exit", null, true, 30, 2, "Thoát" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_menu_items_ParentId",
                table: "menu_items",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "menu_items");
        }
    }
}
