using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Exceptions
{
    internal class KeyCeremonyException : Exception
    {
        public KeyCeremonyState State { get; set; }
        public string GuardianId { get; set; }

        public string KeyCeremonyId { get; set; }

        public KeyCeremonyException(
            KeyCeremonyState state, 
            string keyCeremonyId, 
            string guardian, 
            string? message = null) : base(message)
        { 
            State = state;
            KeyCeremonyId = keyCeremonyId;
            GuardianId = guardian;
        }
    }
}
