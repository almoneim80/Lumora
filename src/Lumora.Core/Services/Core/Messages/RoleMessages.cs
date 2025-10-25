namespace Lumora.Services.Core.Messages
{
    public class RoleMessages(ILocalizationManager localization) : GeneralMessage(localization)
    {
        private readonly ILocalizationManager _localization = localization;

        // MESSAGES
        public string MsgRoleCreationFailed => _localization.GetLocalizedString("RoleCreationFailed");
        public string MsgRolesCreated => _localization.GetLocalizedString("RolesCreated");
        public string MsgDefaultRolesExist => _localization.GetLocalizedString("DefaultRolesExist");
        public string MsgDefaultRolesError => _localization.GetLocalizedString("DefaultRolesError");
        public string MsgRolesRetrieved => _localization.GetLocalizedString("RolesRetrieved");
        public string MsgRolesRetrievalError => _localization.GetLocalizedString("RolesRetrievalError");
        public string MsgRoleNameEmpty => _localization.GetLocalizedString("RoleNameEmpty");
        public string MsgRoleAlreadyExists => _localization.GetLocalizedString("RoleAlreadyExists");
        public string MsgAddRoleFailed => _localization.GetLocalizedString("AddRoleFailed");
        public string MsgAddRoleSuccess => _localization.GetLocalizedString("AddRoleSuccess");
        public string MsgAddRoleError => _localization.GetLocalizedString("AddRoleError");
        public string MsgRoleExists => _localization.GetLocalizedString("RoleExists");
        public string MsgRoleExistenceCheckError => _localization.GetLocalizedString("RoleExistenceCheckError");
        public string MsgUserNotFoundOrInactive => _localization.GetLocalizedString("UserNotFoundOrInactive");
        public string MsgRoleNotFound => _localization.GetLocalizedString("RoleNotFound");
        public string MsgAssignRoleFailed => _localization.GetLocalizedString("AssignRoleFailed");
        public string MsgAssignRoleSuccess => _localization.GetLocalizedString("AssignRoleSuccess");
        public string MsgAssignRoleError => _localization.GetLocalizedString("AssignRoleError");
        public string MsgDeleteRoleNotFound => _localization.GetLocalizedString("DeleteRoleNotFound");
        public string MsgDeleteRoleAssigned => _localization.GetLocalizedString("DeleteRoleAssigned");
        public string MsgDeleteAdminRole => _localization.GetLocalizedString("DeleteAdminRole");
        public string MsgDeleteRoleFailed => _localization.GetLocalizedString("DeleteRoleFailed");
        public string MsgDeleteRoleSuccess => _localization.GetLocalizedString("DeleteRoleSuccess");
        public string MsgDeleteRoleError => _localization.GetLocalizedString("DeleteRoleError");
        public string MsgUpdateRoleNameFailed => _localization.GetLocalizedString("UpdateRoleNameFailed");
        public string MsgUpdateRoleNameSuccess => _localization.GetLocalizedString("UpdateRoleNameSuccess");
        public string MsgUpdateRoleNameError => _localization.GetLocalizedString("UpdateRoleNameError");
        public string MsgRemoveRoleFailed => _localization.GetLocalizedString("RemoveRoleFailed");
        public string MsgRemoveRoleSuccess => _localization.GetLocalizedString("RemoveRoleSuccess");
        public string MsgRemoveRoleError => _localization.GetLocalizedString("RemoveRoleError");
        public string MsgUsersInRoleRetrieved => _localization.GetLocalizedString("UsersInRoleRetrieved");
        public string MsgUsersInRoleError => _localization.GetLocalizedString("UsersInRoleError");
        public string MsgUserInRole => _localization.GetLocalizedString("UserInRole");
        public string MsgUserNotInRole => _localization.GetLocalizedString("UserNotInRole");
        public string MsgUserInRoleCheckError => _localization.GetLocalizedString("UserInRoleCheckError");
        public string MsgUserIdEmpty => _localization.GetLocalizedString("UserIdEmpty");
        public string MsgUserByIdNotFound => _localization.GetLocalizedString("UserByIdNotFound");
        public string MsgUserRolesRetrieved => _localization.GetLocalizedString("UserRolesRetrieved");
        public string MsgUserRolesRetrievalError => _localization.GetLocalizedString("UserRolesRetrievalError");
    }
}
