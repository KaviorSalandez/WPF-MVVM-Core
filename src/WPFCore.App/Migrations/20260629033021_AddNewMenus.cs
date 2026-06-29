using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WPFCore.App.Migrations
{
    /// <inheritdoc />
    public partial class AddNewMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 1,
                column: "Title",
                value: "Thống kê");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 10,
                column: "Title",
                value: "Danh mục");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 11,
                column: "Title",
                value: "Quản lý lớp dữ liệu bản đồ");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 12,
                column: "Title",
                value: "Quản lý đối tượng địa lý");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 13,
                column: "Title",
                value: "Quản lý nguồn dữ liệu");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 14,
                column: "Title",
                value: "Quản lý quy định kiểm tra");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 15,
                column: "Title",
                value: "Quản lý loại bản đồ");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 16,
                column: "Title",
                value: "Quản lý người dùng");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 20,
                column: "Title",
                value: "Hệ thống");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 21,
                column: "Title",
                value: "Kiểm tra dữ liệu bản đồ");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 22,
                column: "Title",
                value: "Xem kết quả kiểm tra");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 23,
                column: "Title",
                value: "Báo cáo tổng hợp");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 24,
                column: "Title",
                value: "Xuất báo cáo (Excel/PDF)");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 25,
                column: "Title",
                value: "Quản lý khách hàng (CRUD mẫu)");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 30,
                column: "Title",
                value: "Trợ giúp");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 31,
                column: "Title",
                value: "Giới thiệu");

            migrationBuilder.InsertData(
                table: "menu_items",
                columns: new[] { "Id", "ActionKey", "Glyph", "IsEnabled", "ParentId", "SortOrder", "Title" },
                values: new object[,]
                {
                    { 26, "Menus", null, true, 20, 6, "Quản lý menu" },
                    { 33, null, null, true, null, 2, "Bản đồ địa giới" },
                    { 34, "ViewMap", null, true, 33, 1, "Xem bản đồ trực quan" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 1,
                column: "Title",
                value: "Th?ng kê");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 10,
                column: "Title",
                value: "Danh m?c");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 11,
                column: "Title",
                value: "Qu?n lý l?p d? li?u b?n d?");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 12,
                column: "Title",
                value: "Qu?n lý d?i tu?ng d?a lý");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 13,
                column: "Title",
                value: "Qu?n lý ngu?n d? li?u");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 14,
                column: "Title",
                value: "Qu?n lý quy d?nh ki?m tra");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 15,
                column: "Title",
                value: "Qu?n lý lo?i b?n d?");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 16,
                column: "Title",
                value: "Qu?n lý ngu?i dùng");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 20,
                column: "Title",
                value: "H? th?ng");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 21,
                column: "Title",
                value: "Ki?m tra d? li?u b?n d?");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 22,
                column: "Title",
                value: "Xem k?t qu? ki?m tra");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 23,
                column: "Title",
                value: "Báo cáo t?ng h?p");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 24,
                column: "Title",
                value: "Xu?t báo cáo (Excel/PDF)");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 25,
                column: "Title",
                value: "Qu?n lý khách hàng (CRUD m?u)");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 30,
                column: "Title",
                value: "Tr? giúp");

            migrationBuilder.UpdateData(
                table: "menu_items",
                keyColumn: "Id",
                keyValue: 31,
                column: "Title",
                value: "Gi?i thi?u");
        }
    }
}
