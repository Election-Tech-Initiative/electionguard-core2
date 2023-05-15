namespace ElectionGuard.ElectionSetup;

/// <summary>
/// Disposable Record Base
/// </summary>
public record DisposableRecordBase : IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// Default Ctor
    /// </summary>
    public DisposableRecordBase()
    {
    }

    public DisposableRecordBase(DisposableRecordBase other)
    {
        _disposedValue = other._disposedValue;
    }

    /// <summary>
    /// dispose managed state (managed objects)
    /// </summary>
    protected virtual void DisposeManaged()
    {
        // dispose managed state (managed objects)
    }

    /// <summary>
    /// free unmanaged resources (unmanaged objects) and override finalizer
    /// set large fields to null
    /// </summary>
    protected virtual void DisposeUnmanaged()
    {
        // free unmanaged resources (unmanaged objects) and override finalizer
        // set large fields to null
    }

    /// <summary>
    /// Dispose Managed and Unmanaged resources
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                DisposeManaged();
            }
            DisposeUnmanaged();
            _disposedValue = true;
        }
    }

    /// <summary>
    /// override finalizer only if 'Dispose(bool disposing)'
    /// has code to free unmanaged resources
    /// </summary>
    ~DisposableRecordBase()
    {
        // Do not change this code.
        // Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    /// <summary>
    /// Dispose the object
    /// </summary>
    public void Dispose()
    {
        // Do not change this code.
        // Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
