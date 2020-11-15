
using System;
using System.Collections.Generic;
using System.Text;

namespace Gibraltar.Data
{
    /// <summary>
    /// An enumerated flags value specifying the type of operation for a logic node.
    /// </summary>
    /// <remarks>See LogicNode.AllowedOperations for the list of user-selectable operations.</remarks>
    [Flags]
    public enum MatchOperationType
    {
        /// <summary>
        /// No operation specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Matches if the value is equal to the A field. (Internal use only. Use Equals instead.)
        /// </summary>
        EqualFieldA = 1,

        /// <summary>
        /// Matches if the value is less than the A field. (Internal use only. Use LessThan instead.)
        /// </summary>
        LessThanFieldA = 2,

        /// <summary>
        /// Matches if the value is greater than the A field. (Internal use only. Use GreaterThan instead.)
        /// </summary>
        GreaterThanFieldA = 4,

        /// <summary>
        /// Requires that both the A-field test and the B-field test must match, instead of either one. (Internal use only.)
        /// </summary>
        BothFieldsMatching = 8,

        /// <summary>
        /// Matches if the value is equal to the B field. (Internal use only. Use Equals instead.)
        /// </summary>
        EqualFieldB = 0x10,

        /// <summary>
        /// Matches if the value is less than the B field. (Internal use only. Use LessThan instead.)
        /// </summary>
        LessThanFieldB = 0x20,

        /// <summary>
        /// Matches if the value is greater than the B field. (Internal use only. Use GreaterThan instead.)
        /// </summary>
        GreaterThanFieldB = 0x40,

        /// <summary>
        /// A mask to select only the operation bits matching against FieldB.
        /// </summary>
        FieldBMask = 0x70,

        /// <summary>
        /// A mask to select only the standard operation bits, ignoring special flags.
        /// </summary>
        StandardOperationsMask = 0x7f,

        /// <summary>
        /// A mask to select all special flags, ignoring the standard operation bits.
        /// </summary>
        SpecialOperationsMask = ~0x7f,

        /// <summary>
        /// Pretends that an equals match on B is considered to be less than (the increment of) it. (Internal use only.
        /// See InRange and Between operation types.)
        /// </summary>
        /// <remarks>This is particularly useful for implicit range matches like starts-with or version matching
        /// where the first boundary is inclusive but the second is the exclusive limit of an implied range.
        /// Instead of determining the field B value to compare against for an exclusive less-than match, the
        /// field B value for an inclusive less-or-equal match (typically the same as the A field) can be used,
        /// and equality matches on B are returned as if they are less-than B (as if B were implicitly incremented
        /// to the next higher value).  Thus, equality against B is never returned, and the match should only test B
        /// for less-than.</remarks>
        ImplicitIncrementB = 0x80,

        /// <summary>
        /// String matching is looking for an ending rather than a prefix. (Internal use only. Use EndsWith instead.)
        /// </summary>
        StringEnd = 0x100,

        /// <summary>
        /// String matching is looking for any containing match rather than a prefix. (Internal use only. Use Contains instead.)
        /// </summary>
        StringContain = 0x200,

        /// <summary>
        /// Special handling for checkbox matching from a constrained list. (Internal use only. Use IncludeList or ExcludeList instead.)
        /// </summary>
        ListMatching = 0x400,

        /// <summary>
        /// Special representation for a logical AND. (Internal use only.)
        /// </summary>
        And = LessThanFieldA | EqualFieldA | GreaterThanFieldA | BothFieldsMatching | LessThanFieldB | EqualFieldB | GreaterThanFieldB,

        /// <summary>
        /// Special representation for a logical OR. (Internal use only.)
        /// </summary>
        Or = LessThanFieldA | EqualFieldA | GreaterThanFieldA | LessThanFieldB | EqualFieldB | GreaterThanFieldB,

        //
        // Public-use values begin here.  These must contain more than one bit to avoid being reported as the internal labels.
        //

        /// <summary>
        /// This operation can never match.
        /// </summary>
        Never = LessThanFieldA | BothFieldsMatching,

        /// <summary>
        /// This operation will always match.
        /// </summary>
        Always = LessThanFieldA | EqualFieldA | GreaterThanFieldA,

        /// <summary>
        /// Matches on equality to a single configured value.
        /// </summary>
        Equals = EqualFieldA | BothFieldsMatching | FieldBMask,

        /// <summary>
        /// Matches on inequality to a single configured value.
        /// </summary>
        NotEquals = LessThanFieldA | GreaterThanFieldA | BothFieldsMatching | FieldBMask,

        /// <summary>
        /// Matches if less than a single configured value.
        /// </summary>
        LessThan = LessThanFieldA | BothFieldsMatching | FieldBMask,

        /// <summary>
        /// Matches if greater than a single configured value.
        /// </summary>
        GreaterThan = GreaterThanFieldA | BothFieldsMatching | FieldBMask,

        /// <summary>
        /// Matches if less than or equal to a single configured value.
        /// </summary>
        LessOrEquals = LessThanFieldA | EqualFieldA,

        /// <summary>
        /// Matches if greater than or equal to a single configured value.
        /// </summary>
        GreaterOrEquals = GreaterThanFieldA | EqualFieldA,

        /// <summary>
        /// Matches if test value is within an inclusive range between two configured values. (Boundaries match.)
        /// </summary>
        InRange = GreaterThanFieldA | EqualFieldA | BothFieldsMatching | LessThanFieldB | EqualFieldB,

        /// <summary>
        /// Matches if test value is outside an inclusive range between two configured values. (Boundaries don't match.)
        /// </summary>
        NotInRange = LessThanFieldA | GreaterThanFieldB,

        /// <summary>
        /// Matches if test value is within an exclusive range between two configured values. (Boundaries don't match.)
        /// </summary>
        Between = GreaterThanFieldA | BothFieldsMatching | LessThanFieldB,

        /// <summary>
        /// Matches if test value is outside an exclusive range between two configured values. (Boundaries match.)
        /// </summary>
        NotBetween = LessThanFieldA | EqualFieldA | GreaterThanFieldB | EqualFieldB,

        /// <summary>
        /// Matches if test value (string or version type) starts with a configured prefix or range.
        /// </summary>
        StartsWith = GreaterThanFieldA | EqualFieldA | BothFieldsMatching | LessThanFieldB | EqualFieldB | ImplicitIncrementB,

        /// <summary>
        /// Matches if test value (string or version type) does not start with a configured prefix or range.
        /// </summary>
        NotStartsWith = LessThanFieldA | GreaterThanFieldB | ImplicitIncrementB,

        /// <summary>
        /// Matches if a test value (string) ends with a configured suffix (range only if endpoints are same length).
        /// </summary>
        EndsWith = StartsWith | StringEnd,

        /// <summary>
        /// Matches if a test value (string) does not end with a configured suffix (range only if endpoints are same length).
        /// </summary>
        NotEndsWith = NotStartsWith | StringEnd,

        /// <summary>
        /// Matches if a test value (string) contains a configured substring.
        /// </summary>
        Contains = StartsWith | StringContain,

        /// <summary>
        /// Matches if a test value (string) does not contain a configured substring.
        /// </summary>
        NotContains = NotStartsWith | StringContain,

        /// <summary>
        /// Matches if a test value is checked on the constrained list. (eg. OS type)
        /// </summary>
        IncludeList = EqualFieldA | EqualFieldB | ListMatching,

        /// <summary>
        /// Matches if a test value is unchecked on the constrained list. (eg. OS type)
        /// </summary>
        ExcludeList = LessThanFieldA | GreaterThanFieldA | BothFieldsMatching | LessThanFieldB | GreaterThanFieldB | ListMatching,
    }
}
