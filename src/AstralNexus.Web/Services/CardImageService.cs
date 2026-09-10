using Microsoft.AspNetCore.Http;

namespace AstralNexus.Web.Services;

public class CardImageService(IWebHostEnvironment environment)
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp" };

    public async Task<string?> SaveAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return null;

        if (file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("Slika ne smije biti veća od 5 MB.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Dozvoljeni formati su PNG, JPG, JPEG i WEBP.");

        var folder = Path.Combine(environment.WebRootPath, "images", "cards", "uploads");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(folder, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/images/cards/uploads/{fileName}";
    }

    public void DeleteUploadedFile(string? webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath) ||
            !webPath.StartsWith("/images/cards/uploads/", StringComparison.OrdinalIgnoreCase))
            return;

        var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(environment.WebRootPath, relative);

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
