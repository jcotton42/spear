using Remora.Rest.Core;

namespace Spear;

public static class CacheKeys {
    public static string GuildBooks(Snowflake guild) => $"spear:books:{guild}";
    public static string GuildSettings(Snowflake guild) => $"spear:guild-settings:{guild}";
}
