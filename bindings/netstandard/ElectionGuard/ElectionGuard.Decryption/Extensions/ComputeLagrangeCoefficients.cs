using ElectionGuard.Decryption.Shares;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Extensions;

public static class ComputeLagrangeCoefficientExtensions
{
    public static Dictionary<string, LagrangeCoefficient> ComputeLagrangeCoefficients(
        this List<Tuple<ElectionPublicKey, TallyShare>> guardianShares)
    {
        return ComputeLagrangeCoefficients(guardianShares.Select(x => x.Item1).ToList());
    }

    public static Dictionary<string, LagrangeCoefficient> ComputeLagrangeCoefficients(
        this List<Tuple<ElectionPublicKey, BallotShare>> guardianShares)
    {
        return ComputeLagrangeCoefficients(guardianShares.Select(x => x.Item1).ToList());
    }

    public static Dictionary<string, LagrangeCoefficient> ComputeLagrangeCoefficients(
        this List<Tuple<ElectionPublicKey, SelectionShare>> guardianShares)
    {
        return ComputeLagrangeCoefficients(guardianShares.Select(x => x.Item1).ToList());
    }

    public static Dictionary<string, LagrangeCoefficient> ComputeLagrangeCoefficients(
        this List<Guardian> guardians)
    {
        return ComputeLagrangeCoefficients(guardians.Select(x => x.SharePublicKey()).ToList());
    }

    public static Dictionary<string, LagrangeCoefficient> ComputeLagrangeCoefficients(
        this List<ElectionPublicKey> guardians)
    {
        return LagrangeCoefficient.Compute(guardians)
            .ToDictionary(x => x.GuardianId, x => x);
    }
}
