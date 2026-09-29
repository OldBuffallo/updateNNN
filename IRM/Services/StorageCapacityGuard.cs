namespace IRM.Services;

public sealed class StorageCapacityGuard : IStorageCapacityGuard
{
    private readonly long _stopUploadsBelowFreeBytes;
    private readonly long _warnBelowFreeBytes;
    private readonly ILogger<StorageCapacityGuard> _logger;

    public StorageCapacityGuard(IConfiguration configuration, ILogger<StorageCapacityGuard> logger)
    {
        _stopUploadsBelowFreeBytes = configuration.GetValue<long?>("Storage:StopUploadsBelowFreeBytes")
            ?? 1024L * 1024 * 1024;
        _warnBelowFreeBytes = configuration.GetValue<long?>("Storage:WarnBelowFreeBytes")
            ?? 2L * 1024 * 1024 * 1024;
        _logger = logger;
    }

    public void EnsureCanWrite(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath)
            ?? throw new InvalidOperationException("Không xác định được ổ đĩa lưu tệp.");
        var available = new DriveInfo(root).AvailableFreeSpace;
        if (available < _stopUploadsBelowFreeBytes)
            throw new InvalidOperationException("Ổ đĩa còn dưới 1 GB. Hệ thống đã chặn tải tệp mới để bảo vệ dữ liệu.");
        if (available < _warnBelowFreeBytes)
            _logger.LogWarning("Dung lượng lưu trữ IRM còn thấp: {AvailableBytes} bytes tại {StoragePath}", available, fullPath);
    }
}
