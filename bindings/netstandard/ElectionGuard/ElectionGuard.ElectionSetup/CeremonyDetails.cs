namespace ElectionGuard.ElectionSetup;

/// <summary>
/// Details of key ceremony
/// </summary>
public record CeremonyDetails(string KeyCeremonyId, int numberOfGuardians, int quorum);
