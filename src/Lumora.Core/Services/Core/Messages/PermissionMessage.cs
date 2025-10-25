namespace Lumora.Services.Core.Messages
{
    public class PermissionMessage(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgPermissionCannotBeEmpty => _localization.GetLocalizedString("PermissionCannotBeEmpty");
        public string MsgPermissionNotDefined => _localization.GetLocalizedString("PermissionNotDefined");
        public string MsgRoleNotFound => _localization.GetLocalizedString("RoleNotFound");
        public string MsgPermissionAlreadyExistsForRole => _localization.GetLocalizedString("PermissionAlreadyExistsForRole");
        public string MsgAddPermissionFailed => _localization.GetLocalizedString("AddPermissionFailed");
        public string MsgAddPermissionSucceeded => _localization.GetLocalizedString("AddPermissionSucceeded");
        public string MsgUnexpectedAddPermissionError => _localization.GetLocalizedString("UnexpectedAddPermissionError");
        public string MsgPermissionsListCannotBeEmpty => _localization.GetLocalizedString("PermissionsListCannotBeEmpty");
        public string MsgAddPermissionsPartialSuccess => _localization.GetLocalizedString("AddPermissionsPartialSuccess");
        public string MsgUnexpectedAddPermissionsError => _localization.GetLocalizedString("UnexpectedAddPermissionsError");
        public string MsgRemovePermissionFailed => _localization.GetLocalizedString("RemovePermissionFailed");
        public string MsgRemovePermissionSucceeded => _localization.GetLocalizedString("RemovePermissionSucceeded");
        public string MsgUnexpectedRemovePermissionError => _localization.GetLocalizedString("UnexpectedRemovePermissionError");
        public string MsgPermissionsRetrievedForRole => _localization.GetLocalizedString("PermissionsRetrievedForRole");
        public string MsgUnexpectedRetrievePermissionsError => _localization.GetLocalizedString("UnexpectedRetrievePermissionsError");
        public string MsgUserCannotBeNull => _localization.GetLocalizedString("UserCannotBeNull");
        public string MsgPermissionsRetrievedForUser => _localization.GetLocalizedString("PermissionsRetrievedForUser");
        public string MsgUnexpectedRetrieveUserPermissionsError => _localization.GetLocalizedString("UnexpectedRetrieveUserPermissionsError");
        public string MsgUserHasNoPermissions => _localization.GetLocalizedString("UserHasNoPermissions");
        public string MsgUserHasPermission => _localization.GetLocalizedString("UserHasPermission");
        public string MsgUserDoesNotHavePermission => _localization.GetLocalizedString("UserDoesNotHavePermission");
        public string MsgUnexpectedCheckPermissionError => _localization.GetLocalizedString("UnexpectedCheckPermissionError");
        public string MsgUserDoesNotHaveSpecifiedPermission => _localization.GetLocalizedString("UserDoesNotHaveSpecifiedPermission");
        public string MsgRemovePermissionFromUserFailed => _localization.GetLocalizedString("RemovePermissionFromUserFailed");
        public string MsgRemovePermissionFromUserSucceeded => _localization.GetLocalizedString("RemovePermissionFromUserSucceeded");
        public string MsgUnexpectedRemovePermissionFromUserError => _localization.GetLocalizedString("UnexpectedRemovePermissionFromUserError");
        public string MsgRoleNameRequired => _localization.GetLocalizedString("RoleNameRequired");
        public string MsgRoleNameInvalid => _localization.GetLocalizedString("RoleNameInvalid");
        public string MsgPermissionRequired => _localization.GetLocalizedString("PermissionRequired");
        public string MsgPermissionFormatInvalid => _localization.GetLocalizedString("PermissionFormatInvalid");
        public string MsgPermissionsRequired => _localization.GetLocalizedString("PermissionsRequired");
        public string MsgPermissionsMustBeUnique => _localization.GetLocalizedString("PermissionsMustBeUnique");
    }
}
