using FluentValidation;
using WPFCore.App.Modules.Customers.Models;

namespace WPFCore.App.Modules.Customers.Validation;

public sealed class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("Mã khách hàng không được để trống")
            .Length(3, 10).WithMessage("Mã khách hàng phải có độ dài 3-10 ký tự")
            .Matches("^[A-Z0-9]+$").WithMessage("Mã khách hàng chỉ chứa chữ in hoa và số");

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Tên khách hàng không được để trống")
            .MaximumLength(200).WithMessage("Tên khách hàng tối đa 200 ký tự");

        RuleFor(c => c.Email)
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .MaximumLength(200).WithMessage("Email tối đa 200 ký tự")
            .When(c => !string.IsNullOrWhiteSpace(c.Email));

        RuleFor(c => c.Phone)
            .Matches(@"^\+?[\d\s\-()]+$").WithMessage("Số điện thoại không đúng định dạng")
            .MaximumLength(50).WithMessage("Số điện thoại tối đa 50 ký tự")
            .When(c => !string.IsNullOrWhiteSpace(c.Phone));

        RuleFor(c => c.Address)
            .MaximumLength(500).WithMessage("Địa chỉ tối đa 500 ký tự")
            .When(c => !string.IsNullOrWhiteSpace(c.Address));
    }
}
