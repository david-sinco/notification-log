using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using NotificationLog.RentalService.Application.Common.Storage;

namespace NotificationLog.RentalService.Infrastructure.Storage;

internal sealed class AzureBlobPhotoStorage(BlobContainerClient container) : IPhotoStorage, IPhotoUrlProvider
{
    private static readonly TimeSpan ReadUrlLifetime = TimeSpan.FromMinutes(20);

    private readonly BlobContainerClient _container = container;

    public Task SaveAsync(Guid listingId, string fileName, Stream content, string contentType, CancellationToken ct)
        => BlobFor(listingId, fileName).UploadAsync(
            content,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } },
            ct);

    public Task DeleteAsync(Guid listingId, string fileName, CancellationToken ct)
        => BlobFor(listingId, fileName).DeleteIfExistsAsync(cancellationToken: ct);

    public string ReadUrlFor(Guid listingId, string fileName)
        => BlobFor(listingId, fileName)
            .GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(ReadUrlLifetime))
            .ToString();

    private BlobClient BlobFor(Guid listingId, string fileName)
        => _container.GetBlobClient($"{listingId:N}/{fileName}");
}
