
namespace Gibraltar
{
    /// <summary>
    /// Defines the policy that should be used for handling internal Gibraltar exceptions
    /// </summary>
    public enum ExceptionPolicy
    {
        /// <summary>
        /// Raise exceptions for malformed arguments only, no others
        /// </summary>
        RaiseArgumentExceptionsOnly,

        /// <summary>
        /// Raise no exceptions at all
        /// </summary>
        RaiseNoExceptions,

        /// <summary>
        /// Raise all exceptions
        /// </summary>
        RaiseAllExceptions
    }
}