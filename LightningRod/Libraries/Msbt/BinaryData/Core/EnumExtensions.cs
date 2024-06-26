using System;
using System.Collections.Generic;
using System.Linq;

namespace OatmealDome.BinaryData.Core
{
    /// <summary>
    /// Represents a collection of methods extending enum types.
    /// </summary>
    internal static class EnumExtensions
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private static Dictionary<Type, bool> _flagEnums = new Dictionary<Type, bool>();

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Returns whether <paramref name="value"/> is a defined value in the enum of the given <paramref name="type"/>
        /// or a valid set of flags for enums decorated with the <see cref="FlagsAttribute"/>.
        /// </summary>
        /// <param name="type">The type of the enum.</param>
        /// <param name="value">The value to check against the enum type.</param>
        /// <returns><c>true</c> if the value is valid; otherwise <c>false</c>.</returns>
        internal static bool IsValid(Type type, object value)
        {
            // For enumerations decorated with the FlagsAttribute, allow sets of flags.
            bool valid = Enum.IsDefined(type, value);
            if (!valid && IsFlagsEnum(type))
            {
                long mask = 0;
                foreach (object definedValue in Enum.GetValues(type))
                {
                    mask |= Convert.ToInt64(definedValue);
                }
                long longValue = Convert.ToInt64(value);
                valid = (mask & longValue) == longValue;
            }
            return valid;
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static bool IsFlagsEnum(Type type)
        {
            if (!_flagEnums.TryGetValue(type, out bool value))
            {
                value = type.GetCustomAttributes(typeof(FlagsAttribute), true)?.Any() == true;
                _flagEnums.Add(type, value);
            }
            return value;
        }
    }
}
