using Remora.Rest.Core;

namespace Spear;

public static class CacheKeys {
    public static string GuildBooks(Snowflake guild) => $"books:{guild}";
}
