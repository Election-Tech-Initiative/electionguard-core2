using System;

namespace ElectionGuard
{
    internal static class StatusExtensions
    {
        public static void ThrowIfError(this Status status)
        {
            if (status == Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                return;
            }
            ExceptionHandler.GetData(out var function, out var message, out var _);
            var errorMessage = $"status: {status} function: {function} message: {message}";
            switch (status)
            {
                case Status.ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT:
                    throw new ArgumentException(errorMessage);
                case Status.ELECTIONGUARD_STATUS_ERROR_OUT_OF_RANGE:
                    throw new ArgumentOutOfRangeException(errorMessage);
                case Status.ELECTIONGUARD_STATUS_ERROR_IO_ERROR:
                    throw new SystemException(errorMessage);
                case Status.ELECTIONGUARD_STATUS_ERROR_BAD_ACCESS:
                    throw new AccessViolationException(errorMessage);
                case Status.ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC:
                    throw new OutOfMemoryException(errorMessage);
                case Status.ELECTIONGUARD_STATUS_ERROR_ALREADY_EXISTS:
                    throw new TypeLoadException(errorMessage);
                case Status.ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR:
                    throw new AggregateException(errorMessage);
                case Status.ELECTIONGUARD_STATUS_UNKNOWN:
                default:
                    throw new Exception(errorMessage);
            }
        }
    }
}