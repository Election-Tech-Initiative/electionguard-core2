namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// Usage case for proof
/// </summary>
public enum ProofUsage
{
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = 1,

    /// <summary>
    /// Prove knowledge of secret value
    /// </summary>
    SecretValue = 2,

    /// <summary>
    /// Prove value within selection's limit
    /// </summary>
    SelectionLimit = 3,

    /// <summary>
    /// Prove selection's value (0 or 1)
    /// </summary>
    SelectionValue = 4
}