using System;

namespace ApiBoilerPlateMyTest.Infrastructure.Helpers
{
    public static class PropertyValidation
    {
        public static bool IsValidDateTime(DateTime date)
        {
            if (date == default(DateTime))
                return false;
            return true;
        }
    }
}
