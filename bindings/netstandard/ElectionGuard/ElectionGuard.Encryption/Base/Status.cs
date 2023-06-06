// ReSharper disable InconsistentNaming
namespace ElectionGuard
{
    /// <summary>
    /// Enumeration used to keep the status error messages for the exception handling
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The operation finished successfully
        /// </summary>
        ELECTIONGUARD_STATUS_SUCCESS = 0,

        /// <summary>
        /// There was an error with the arguments passed in
        /// </summary>
        ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT,

        /// <summary>
        /// Data was being accessed outside its range
        /// </summary>
        ELECTIONGUARD_STATUS_ERROR_OUT_OF_RANGE,

        /// <summary>
        /// Error with writing/reading the data
        /// </summary>
        ELECTIONGUARD_STATUS_ERROR_IO_ERROR,

        /// <summary>
        /// Had issue accessing data
        /// </summary>
        ELECTIONGUARD_STATUS_ERROR_BAD_ACCESS,

        /// <summary>
        /// Error allocating the data
        /// </summary>
        ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC,

        /// <summary>
        /// The data already exists
        /// </summary>
        ELECTIONGUARD_STATUS_ERROR_ALREADY_EXISTS,

        /// <summary>
        /// Error used as a default when catching any exception
        /// </summary>
        ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR,

        /// <summary>
        /// This code should always be the last code in the collection
        /// so that the status codes string can be correctly derived
        /// </summary>
        ELECTIONGUARD_STATUS_UNKNOWN


    }
}