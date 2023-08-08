namespace ElectionGuard
{
    /// <Summary>
    /// Enumeration for the type of ElectionType
    /// see: https://developers.google.com/elections-data/reference/election-type
    /// </Summary>
    public enum ElectionType
    {
        /// <summary>
        /// an unknown value
        /// </summary>
        unknown = 0,
        /// <summary>
        /// An election that's typically held on the national day for elections.
        /// </summary>
        general = 1,
        /// <summary>
        /// A primary election that's for a specific party, where voter eligibility for this contest is based on registration.
        /// </summary>
        partisanPrimaryClosed = 2,
        /// <summary>
        /// A primary election that's for a specific party, where voters declare their desired party or choose in private
        /// </summary>
        partisanPrimaryOpen = 3,
        /// <summary>
        /// A primary election without a specified type, such as a nonpartisan primary.
        /// </summary>
        primary = 4,
        /// <summary>
        /// An election to decide a prior contest that ended with no candidate receiving a majority of the votes
        /// </summary>
        runoff = 5,
        /// <summary>
        /// An election held out of sequence for special circumstances, such as to fill a vacated office.
        /// </summary>
        special = 6,
        /// <summary>
        /// The election is a type that isn't listed in this enumeration. If used, include the item's custom type in an OtherType element.
        /// </summary>
        other = 7
    };
}