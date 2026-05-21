using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PFP.API.Models;

/// <summary>Multipart fields for file upload (flat snake_case form keys).</summary>
public sealed class UploadFileForm
{
    [FromForm(Name = "entity_type")]
    public string EntityType { get; set; } = "";

    [FromForm(Name = "entity_id")]
    public Guid EntityId { get; set; }

    [FromForm(Name = "file")]
    public IFormFile? File { get; set; }

    [FromForm(Name = "mime_type")]
    public string? MimeType { get; set; }

    [FromForm(Name = "max_file_size_mb")]
    public int MaxFileSizeMb { get; set; } = 10;

    [FromForm(Name = "is_public")]
    public bool IsPublic { get; set; }
}
