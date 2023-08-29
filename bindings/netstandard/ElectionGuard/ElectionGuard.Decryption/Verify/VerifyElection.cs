using ElectionGuard.Decryption.ElectionRecord;
namespace ElectionGuard.Decryption.Verify;

public static class VerifyElection
{
    public static async Task<VerificationResult> VerifyAsync(
        ElectionRecordData record)
    {
        var results = new List<VerificationResult>();

        // Verify the election parameters
        var parameters = await VerifyElectionParameters(
            record.Constants,
            record.Manifest,
            record.Context
        );
        results.Add(parameters);

        return new VerificationResult("Election Verification", results);
    }

    public static Task<VerificationResult> VerifyElectionParameters(
        ElectionConstants constants,
        Manifest manifest,
        CiphertextElectionContext context
    )
    {
        var results = new List<VerificationResult>();
        if (constants.P != null && constants.P.Equals(Constants.P))
        {
            // Verification 1.B
            results.Add(new VerificationResult(true, "- Verification 1.B: The large prime is equal to the large modulus p defined in Section 3.1.1."));
        }
        else
        {
            results.Add(new VerificationResult(false, $"- Verification 1.B: ElectionConstants.P does not match expected value"));
        }
        return Task.FromResult(new VerificationResult("Verification 1 (Parameter Validation)", results));
    }
}
