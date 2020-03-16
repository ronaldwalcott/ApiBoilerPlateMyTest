using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APiBoilerPlateWeb.Constants
{
    public class LoggingErrorText
    {
        public const string recordAlreadyDefined = "This record is probably already defined, please try again";
        public const string errorWithValues = "There is an error with these values";
        public const string recordChanged = "Record has been changed since it was retrieved for editing, please retrieve the new record for editing";

        public const string newRoleSucceed = "New role created with role name {RoleName} by {username}";
        public const string newRoleFailed = "Errors generated trying to add role name {RoleName} by {username} {generatedErrors}";
        public const string editRoleSucceed = "Role name changed from {OldRoleName} to {NewRoleName} by {username}";
        public const string editRoleFailed = "Errors generated trying to change role name from {OldRoleName} to {NewRoleName} by {username} {generatedErrors}";
        public const string deleteRoleSucceed = "Role name {RoleName} deleted by {username}";
        public const string deleteRoleFailed = "Errors generated trying to delete role name {RoleName} by {username} {generatedErrors}";

        public const string editUserSucceed = "User information has been changed {OldUserInfo} to {NewUserInfo} by {username}";
        public const string editUserFailed = "Errors generated trying to change user information from {OldUserInfo} to {NewUserInfo} by {username} {generatedErrors}";
        public const string deleteUserSucceed = "User {UserInfo} deleted by {username}";
        public const string deleteUserFailed = "Errors generated trying to delete user {UserInfo} by {username} {generatedErrors}";
        public const string retrieveUserFailed = "Errors generated trying to retrieve user with {Id} by {username}";

        public const string removeRoleFailed = "Errors generated trying to remove role {role} from user {OldUserInfo} by {username} {generatedErrors}";
        public const string addRoleFailed = "Errors generated trying to add role {role} to user {OldUserInfo} by {username} {generatedErrors}";

        public const string addClaimFailed = "Errors generated trying to add claim name {ClaimName} to role name {RoleName} by {username} {generatedErrors}";
        public const string removeClaimFailed = "Errors generated trying to remove claim name {ClaimName} from role name {RoleName} by {username} {generatedErrors}";


        public const string userLockSucceed = "The user account {UserInfo} has been locked by {username}";
        public const string userLockFailed = "Errors generated trying to lock user account {UserInfo} by {username} {generatedErrors}";
        public const string userLockPartialFailed = "Partial failure trying to lock user account {UserInfo} by {username} {generatedErrors}";

        public const string userUnlockSucceed = "The user account {UserInfo} has been unlocked by {username}";
        public const string userUnlockFailed = "Errors generated trying to unlock user account {UserInfo} by {username} {generatedErrors}";
        public const string userUnlockPartialFailed = "Partial failure trying to unlock user account {UserInfo} by {username} {generatedErrors}";

        public const string resetPasswordSucceeded = "The user account {UserInfo} password has been reset by {username}";
        public const string resetPasswordFailed = "Errors generated trying to reset password on user account {UserInfo} by {username} {generatedErrors}";
        public const string removePasswordFailed = "Errors generated in the remove password portion of the reset password on user account {UserInfo} by {username} {generatedErrors}";

    }
}
