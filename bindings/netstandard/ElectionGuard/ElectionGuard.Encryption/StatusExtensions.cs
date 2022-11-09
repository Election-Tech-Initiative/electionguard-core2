using System;

namespace ElectionGuard
{
    static internal class StatusExtensions
    {
        public static void ThrowIfError(this Status status)
        {
            switch (status)
            {
                case Status.ELECTIONGUARD_STATUS_SUCCESS:
                    return;
                case Status.ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT:
                    throw new ArgumentException($"{status}");
                case Status.ELECTIONGUARD_STATUS_ERROR_OUT_OF_RANGE:
                    throw new ArgumentOutOfRangeException($"{status}");
                case Status.ELECTIONGUARD_STATUS_ERROR_IO_ERROR:
                    throw new SystemException($"{status}");
                case Status.ELECTIONGUARD_STATUS_ERROR_BAD_ACCESS:
                    throw new AccessViolationException($"{status}");
                case Status.ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC:
                    throw new OutOfMemoryException($"{status}");
                case Status.ELECTIONGUARD_STATUS_ERROR_ALREADY_EXISTS:
                    throw new TypeLoadException($"{status}");
                case Status.ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR:
                    throw new AggregateException($"{status}");
                case Status.ELECTIONGUARD_STATUS_UNKNOWN:
                default:
                    throw new Exception($"{status}");
            }
        }
    }
}