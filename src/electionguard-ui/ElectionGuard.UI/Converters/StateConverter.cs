using System.Globalization;

namespace ElectionGuard.UI.Converters;

internal class StateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var state = (KeyCeremonyState)value;

        return state switch
        {
            KeyCeremonyState.DoesNotExist => AppResources.DoesNotExist,
            KeyCeremonyState.PendingGuardiansJoin => AppResources.PendingGuardianJoin,
            KeyCeremonyState.PendingAdminAnnounce => AppResources.PendingAdminAnnounce,
            KeyCeremonyState.PendingGuardianBackups => AppResources.PendingGuardianBackups,
            KeyCeremonyState.PendingAdminToShareBackups => AppResources.PendingAdminToShareBackups,
            KeyCeremonyState.PendingGuardiansVerifyBackups => AppResources.PendingGuardiansVerifyBackups,
            KeyCeremonyState.PendingAdminToPublishJointKey => AppResources.PendingAdminToPublishJointKey,
            KeyCeremonyState.Complete => AppResources.Complete,
            _ => AppResources.DoesNotExist
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
