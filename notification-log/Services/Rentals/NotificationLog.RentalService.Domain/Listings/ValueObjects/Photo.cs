using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Listings.ValueObjects;

public sealed record Photo
{
    private const int HeaderLength = 12;

    public Guid Id { get; }
    public string Extension { get; }

    private Photo(Guid id, string extension)
    {
        Id = id;
        Extension = extension;
    }

    public string FileName => $"{Id:N}.{Extension}";

    public string ContentType => Extension switch
    {
        "jpg" => "image/jpeg",
        "png" => "image/png",
        "webp" => "image/webp",
        _ => throw new InvalidOperationException($"La extensión '{Extension}' no tiene un tipo de contenido asociado.")
    };

    public static async Task<Photo> FromStreamAsync(Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanSeek)
            throw new ArgumentException("El contenido de la foto debe permitir volver al inicio.", nameof(content));

        var header = new byte[HeaderLength];
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken);
        content.Position = 0;

        var extension = DetectExtension(header.AsSpan(0, read))
            ?? throw new DomainException("El archivo no es una imagen JPEG, PNG o WebP.");

        return new Photo(Guid.NewGuid(), extension);
    }

    internal static Photo FromStorage(string fileName)
    {
        var separator = fileName.LastIndexOf('.');

        return new Photo(Guid.ParseExact(fileName[..separator], "N"), fileName[(separator + 1)..]);
    }

    public override string ToString() => FileName;

    private static string? DetectExtension(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith<byte>([0xFF, 0xD8, 0xFF]))
            return "jpg";

        if (header.StartsWith<byte>([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
            return "png";

        if (header.Length >= HeaderLength
            && header[..4].SequenceEqual("RIFF"u8)
            && header[8..12].SequenceEqual("WEBP"u8))
            return "webp";

        return null;
    }
}
