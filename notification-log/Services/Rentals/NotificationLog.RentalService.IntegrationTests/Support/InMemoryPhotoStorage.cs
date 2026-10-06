using System.Collections.Concurrent;
using NotificationLog.RentalService.Application.Common.Storage;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class InMemoryPhotoStorage : IPhotoStorage, IPhotoUrlProvider
{
    private readonly ConcurrentDictionary<string, string> _files = new();

    public Task SaveAsync(Guid listingId, string fileName, Stream content, string contentType, CancellationToken ct)
    {
        _files[Key(listingId, fileName)] = contentType;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid listingId, string fileName, CancellationToken ct)
    {
        _files.TryRemove(Key(listingId, fileName), out _);

        return Task.CompletedTask;
    }

    public string ReadUrlFor(Guid listingId, string fileName) => $"https://photos.test/{Key(listingId, fileName)}";

    public void Clear() => _files.Clear();

    private static string Key(Guid listingId, string fileName) => $"{listingId:N}/{fileName}";
}
