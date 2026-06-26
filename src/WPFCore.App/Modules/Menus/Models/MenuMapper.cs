using Riok.Mapperly.Abstractions;

namespace WPFCore.App.Modules.Menus.Models;

[Mapper]
public partial class MenuMapper
{
    public partial MenuDto ToDto(MenuItemEntity entity);
    public partial MenuItemEntity ToEntity(MenuDto dto);
}
