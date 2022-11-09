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
        Unknown = 0,
        /// <summary>
        /// Used to report batches of ballots that might cross precinct boundaries.
        /// </summary>
        BallotBatch = 1,
        /// <summary>
        /// Used for a ballot-style area that's generally composed of precincts.
        /// </summary>
        BallotStyleArea = 2,
        /// <summary>
        /// Used as a synonym for a county.
        /// </summary>
        Borough = 3,
        /// <summary>
        /// Used for a city that reports results or for the district that encompasses it.
        /// </summary>
        City = 4,
        /// <summary>
        /// Used for city council districts.
        /// </summary>
        CityCouncil = 5,
        /// <summary>
        /// Used for one or more precincts that have been combined for the purposes of reporting. 
        /// If the term ward is used interchangeably with combined precinct, use combined-precinct 
        /// for the ReportingUnitType.
        /// </summary>
        CombinedPrecinct = 6,
        /// <summary>
        /// Used for national legislative body districts.
        /// </summary>
        Congressional = 7,
        /// <summary>
        /// Used for a country.
        /// </summary>
        Country = 8,
        /// <summary>
        /// Used for a county or for the district that encompasses it. 
        /// Synonymous with borough and parish in some localities.
        /// </summary>
        County = 9,
        /// <summary>
        /// Used for county council districts.
        /// </summary>
        CountyCouncil = 10,
        /// <summary>
        /// Used for a drop box for absentee ballots.
        /// </summary>
        DropBox = 11,
        /// <summary>
        /// Used for judicial districts.
        /// </summary>
        Judicial = 12,
        /// <summary>
        /// Used as applicable for various units such as towns, 
        /// townships, villages that report votes, or for the district 
        /// that encompasses them.
        /// </summary>
        Municipality = 13,
        /// <summary>
        /// Used for a polling place.
        /// </summary>
        PollingPlace = 14,
        /// <summary>
        /// Used if the terms for ward or district are used interchangeably with precinct.
        /// </summary>
        Precinct = 15,
        /// <summary>
        /// Used for a school district.
        /// </summary>
        School = 16,
        /// <summary>
        /// Used for a special district.
        /// </summary>
        Special = 17,
        /// <summary>
        /// Used for splits of precincts.
        /// </summary>
        SplitPrecinct = 18,
        /// <summary>
        /// Used for a state or for the district that encompasses it.
        /// </summary>
        State = 19,
        /// <summary>
        /// Used for a state house or assembly district.
        /// </summary>
        StateHouse = 20,
        /// <summary>
        /// Used for a state senate district.
        /// </summary>
        StateSenate = 21,
        /// <summary>
        /// Used for type of municipality that reports votes or for the district that encompasses it.
        /// </summary>
        Township = 22,
        /// <summary>
        /// Used for a utility district.
        /// </summary>
        Utility = 23,
        /// <summary>
        /// Used for a type of municipality that reports votes or for the district that encompasses it.
        /// </summary>
        Village = 24,
        /// <summary>
        /// Used for a vote center.
        /// </summary>
        VoteCenter = 25,
        /// <summary>
        /// Used for combinations or groupings of precincts or other units.
        /// </summary>
        Ward = 26,
        /// <summary>
        /// Used for a water district.
        /// </summary>
        Water = 27,
        /// <summary>
        /// Used for other types of reporting units that aren't included in this enumeration. 
        /// If used, provide the item's custom type in an OtherType element.
        /// </summary>
        Other = 28,
    };
}