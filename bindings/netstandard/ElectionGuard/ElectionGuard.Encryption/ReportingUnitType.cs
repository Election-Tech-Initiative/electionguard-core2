namespace ElectionGuard
{
    /// <Summary>
    /// Enumeration for the type of geopolitical unit
    /// see: https://developers.google.com/elections-data/reference/reporting-unit-type
    /// </Summary>
    public enum ReportingUnitType
    {
        /// <summary>
        /// an unknown value
        /// </summary>
        unknown = 0,
        /// <summary>
        /// Used to report batches of ballots that might cross precinct boundaries.
        /// </summary>
        ballotBatch = 1,
        /// <summary>
        /// Used for a ballot-style area that's generally composed of precincts.
        /// </summary>
        ballotStyleArea = 2,
        /// <summary>
        /// Used as a synonym for a county.
        /// </summary>
        borough = 3,
        /// <summary>
        /// Used for a city that reports results or for the district that encompasses it.
        /// </summary>
        city = 4,
        /// <summary>
        /// Used for city council districts.
        /// </summary>
        cityCouncil = 5,
        /// <summary>
        /// Used for one or more precincts that have been combined for the purposes of reporting. 
        /// If the term ward is used interchangeably with combined precinct, use combined-precinct 
        /// for the ReportingUnitType.
        /// </summary>
        combinedPrecinct = 6,
        /// <summary>
        /// Used for national legislative body districts.
        /// </summary>
        congressional = 7,
        /// <summary>
        /// Used for a country.
        /// </summary>
        country = 8,
        /// <summary>
        /// Used for a county or for the district that encompasses it. 
        /// Synonymous with borough and parish in some localities.
        /// </summary>
        county = 9,
        /// <summary>
        /// Used for county council districts.
        /// </summary>
        countyCouncil = 10,
        /// <summary>
        /// Used for a dropbox for absentee ballots.
        /// </summary>
        dropBox = 11,
        /// <summary>
        /// Used for judicial districts.
        /// </summary>
        judicial = 12,
        /// <summary>
        /// Used as applicable for various units such as towns, 
        /// townships, villages that report votes, or for the district 
        /// that encompasses them.
        /// </summary>
        municipality = 13,
        /// <summary>
        /// Used for a polling place.
        /// </summary>
        polling_place = 14,
        /// <summary>
        /// Used if the terms for ward or district are used interchangeably with precinct.
        /// </summary>
        precinct = 15,
        /// <summary>
        /// Used for a school district.
        /// </summary>
        school = 16,
        /// <summary>
        /// Used for a special district.
        /// </summary>
        special = 17,
        /// <summary>
        /// Used for splits of precincts.
        /// </summary>
        splitPrecinct = 18,
        /// <summary>
        /// Used for a state or for the district that encompasses it.
        /// </summary>
        state = 19,
        /// <summary>
        /// Used for a state house or assembly district.
        /// </summary>
        stateHouse = 20,
        /// <summary>
        /// Used for a state senate district.
        /// </summary>
        stateSenate = 21,
        /// <summary>
        /// Used for type of municipality that reports votes or for the district that encompasses it.
        /// </summary>
        township = 22,
        /// <summary>
        /// Used for a utility district.
        /// </summary>
        utility = 23,
        /// <summary>
        /// Used for a type of municipality that reports votes or for the district that encompasses it.
        /// </summary>
        village = 24,
        /// <summary>
        /// Used for a vote center.
        /// </summary>
        voteCenter = 25,
        /// <summary>
        /// Used for combinations or groupings of precincts or other units.
        /// </summary>
        ward = 26,
        /// <summary>
        /// Used for a water district.
        /// </summary>
        water = 27,
        /// <summary>
        /// Used for other types of reporting units that aren't included in this enumeration. 
        /// If used, provide the item's custom type in an OtherType element.
        /// </summary>
        other = 28,
    };
}