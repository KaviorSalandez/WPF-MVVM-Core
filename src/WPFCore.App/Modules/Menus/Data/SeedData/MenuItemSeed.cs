using System.Collections.Generic;
using WPFCore.App.Modules.Menus.Models;

namespace WPFCore.App.Modules.Menus.Data.SeedData;

public static class MenuItemSeed
{
    public static IEnumerable<MenuItemEntity> GetMenuItems()
    {
        return new List<MenuItemEntity>
        {
            // Thống kê (top-level, mở Dashboard)
            new MenuItemEntity { Id = 1, ParentId = null, Title = "Thống kê", ActionKey = "Dashboard", SortOrder = 1 },

            // Danh mục (nhóm cha)
            new MenuItemEntity { Id = 10, ParentId = null, Title = "Danh mục", ActionKey = null, SortOrder = 2 },
            new MenuItemEntity { Id = 11, ParentId = 10, Title = "Quản lý lớp dữ liệu bản đồ", ActionKey = "MapLayer", SortOrder = 1 },
            new MenuItemEntity { Id = 12, ParentId = 10, Title = "Quản lý đối tượng địa lý", ActionKey = "GeoFeature", SortOrder = 2 },
            new MenuItemEntity { Id = 13, ParentId = 10, Title = "Quản lý nguồn dữ liệu", ActionKey = "DataSource", SortOrder = 3 },
            new MenuItemEntity { Id = 14, ParentId = 10, Title = "Quản lý quy định kiểm tra", ActionKey = "CheckRules", SortOrder = 4 },
            new MenuItemEntity { Id = 15, ParentId = 10, Title = "Quản lý loại bản đồ", ActionKey = "MapType", SortOrder = 5 },
            new MenuItemEntity { Id = 16, ParentId = 10, Title = "Quản lý người dùng", ActionKey = "Users", SortOrder = 6 },

            // Hệ thống (nhóm cha)
            new MenuItemEntity { Id = 20, ParentId = null, Title = "Hệ thống", ActionKey = null, SortOrder = 3 },
            new MenuItemEntity { Id = 21, ParentId = 20, Title = "Kiểm tra dữ liệu bản đồ", ActionKey = "RunCheck", SortOrder = 1 },
            new MenuItemEntity { Id = 22, ParentId = 20, Title = "Xem kết quả kiểm tra", ActionKey = "CheckResults", SortOrder = 2 },
            new MenuItemEntity { Id = 23, ParentId = 20, Title = "Báo cáo tổng hợp", ActionKey = "SummaryReport", SortOrder = 3 },
            new MenuItemEntity { Id = 24, ParentId = 20, Title = "Xuất báo cáo (Excel/PDF)", ActionKey = "ExportReport", SortOrder = 4 },
            new MenuItemEntity { Id = 25, ParentId = 20, Title = "Quản lý khách hàng (CRUD mẫu)", ActionKey = "CustomerList", SortOrder = 5 },
            new MenuItemEntity { Id = 26, ParentId = 20, Title = "Quản lý menu", ActionKey = "Menus", SortOrder = 6 },

            // Trợ giúp (nhóm cha)
            new MenuItemEntity { Id = 30, ParentId = null, Title = "Trợ giúp", ActionKey = null, SortOrder = 4 },
            new MenuItemEntity { Id = 31, ParentId = 30, Title = "Giới thiệu", ActionKey = "About", SortOrder = 1 },
            new MenuItemEntity { Id = 32, ParentId = 30, Title = "Thoát", ActionKey = "Exit", SortOrder = 2 },

            new MenuItemEntity { Id = 33, ParentId = null, Title = "Bản đồ địa giới", ActionKey = null, SortOrder = 2 },
            new MenuItemEntity { Id = 34, ParentId = 33, Title = "Xem bản đồ trực quan", ActionKey = "ViewMap", SortOrder = 1 }
        };
    }
}
