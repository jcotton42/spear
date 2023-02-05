using System.Text.Json;
using NodaTime;
using Remora.Rest.Core;

namespace Spear.Models;

public class AuditEntry : IDisposable {
    public int Id { get; set; }
    public required Instant Timestamp { get; set; }
    public required AuditEntryType Type { get; set; }
    public required Snowflake? GuildId { get; set; }
    public required Snowflake UserId { get; set; }
    public JsonDocument? Metadata { get; set; }

    public void Dispose() => Metadata?.Dispose();
}
