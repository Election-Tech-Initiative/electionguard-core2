using System.Text;
using ElectionGuard.Base;

namespace ElectionGuard.Decryption.Verify;

/// <summary>
/// The result of a ballot validation operation.
/// </summary>
public class VerificationResult : IValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; }

    public bool AllValid => IsValid && Children.All(x => x.AllValid);

    public List<VerificationResult> Children { get; set; }

    List<IValidationResult> IValidationResult.Children => Children.Select(x => (IValidationResult)x).ToList();

    public VerificationResult(
        bool isValid,
        List<VerificationResult> children = null)
    {
        IsValid = isValid;
        Message = string.Empty;
        Children = children ?? new List<VerificationResult>();
    }

    public VerificationResult(
        string message,
        List<VerificationResult> children = null)
    {
        IsValid = children?.All(x => x.AllValid) ?? false;
        Message = message;
        Children = children ?? new List<VerificationResult>();
    }

    public VerificationResult(
        bool isValid,
        string message,
        List<VerificationResult> children = null)
    {
        IsValid = isValid;
        Message = message;
        Children = children ?? new List<VerificationResult>();
    }

    public override string ToString()
    {
        return ToString(printTrues: false);
    }

    public string ToString(bool printTrues)
    {
        var sb = new StringBuilder();
        if (printTrues || !AllValid)
        {
            _ = sb.AppendLine($"{Message} {AllValid}");
        }

        foreach (var child in Children)
        {
            if (printTrues || !child.IsValid)
            {
                _ = sb.Append($"  {child}");
            }
        }
        return sb.ToString();
    }
}

