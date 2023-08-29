using System.Collections.Generic;

namespace ElectionGuard.Base
{
    public interface IValidationResult
    {
        bool IsValid { get; }
        string Message { get; }
        List<IValidationResult> Children { get; }
    }
}
