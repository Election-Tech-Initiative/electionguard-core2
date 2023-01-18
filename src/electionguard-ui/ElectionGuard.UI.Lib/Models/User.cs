﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace ElectionGuard.UI.Lib.Models;

public partial class User : DatabaseRecord
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private bool _isAdmin = false;

}
