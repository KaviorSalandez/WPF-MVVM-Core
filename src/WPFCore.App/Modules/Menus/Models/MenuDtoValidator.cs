using FluentValidation;

namespace WPFCore.App.Modules.Menus.Models;

public sealed class MenuDtoValidator : AbstractValidator<MenuDto>
{
    public MenuDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tên menu không được để trống")
            .MaximumLength(100).WithMessage("Tên menu không được vượt quá 100 ký tự");

        RuleFor(x => x.ActionKey)
            .MaximumLength(100).WithMessage("ActionKey không được vượt quá 100 ký tự");
    }
}
