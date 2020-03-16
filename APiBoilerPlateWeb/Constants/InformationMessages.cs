using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APiBoilerPlateWeb.Constants
{
    public class InformationMessages
    {
        public const string userAlreadyLocked = "User already locked out";
        public const string userLockPartialFail = "Error occurred locking out user. Please retry.";
        public const string userLockFail = "Error occurred locking out user.";

        public const string userNotLocked = "This user is not locked out";
        public const string userUnlockFailed = "Error occurred unlocking the user.";
        public const string userUnlockPartialFail = "Error occurred unlocking the user. Unlock may not have completed.";

    }
}
