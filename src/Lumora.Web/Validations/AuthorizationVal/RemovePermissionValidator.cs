using Lumora.DTOs.Authorization;

namespace Lumora.Web.Validations.AuthorizationVal
{
    public class RemovePermissionValidator : AbstractValidator<RemovePermissionDto>
    {
        public RemovePermissionValidator(PermissionMessage messages)
        {
            RuleFor(x => x.RoleName)
                .NotEmpty().WithMessage(messages.MsgRoleNameRequired)
                .Matches("^[a-zA-Z0-9 ]+$").WithMessage(messages.MsgRoleNameInvalid);

            RuleFor(x => x.Permission)
                .NotEmpty().WithMessage(messages.MsgPermissionRequired)
                .Matches(@"^[A-Z][a-zA-Z0-9]+\.[A-Z][a-zA-Z0-9]+\.[A-Z][a-zA-Z0-9]+$")
                .WithMessage(messages.MsgPermissionFormatInvalid);
        }
    }
}
