using Lumora.DTOs.Authorization;

namespace Lumora.Web.Validations.AuthorizationVal
{
    public class AddMultiplePermissionsValidator : AbstractValidator<AddMultiplePermissionsDto>
    {
        public AddMultiplePermissionsValidator(PermissionMessage messages)
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage(messages.MsgRoleNameRequired)
                .Matches("^[a-zA-Z0-9 ]+$").WithMessage(messages.MsgRoleNameInvalid);

            RuleFor(x => x.Permissions)
                .NotNull().WithMessage(messages.MsgPermissionsRequired)
                .NotEmpty().WithMessage(messages.MsgPermissionsRequired)
                .Must(permissions => permissions.Distinct().Count() == permissions.Count)
                .WithMessage(messages.MsgPermissionsMustBeUnique);

            RuleForEach(x => x.Permissions)
                .Matches(@"^[A-Z][a-zA-Z0-9]+\.[A-Z][a-zA-Z0-9]+\.[A-Z][a-zA-Z0-9]+$")
                .WithMessage(messages.MsgPermissionFormatInvalid);
        }
    }
}
