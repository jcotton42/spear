using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Remora.Discord.API;
using Remora.Rest.Core;

namespace Spear.Models;

public class DiscordSnowflakeConverter : ValueConverter<Snowflake, ulong> {
    public DiscordSnowflakeConverter() : base(s => s.Value, s => DiscordSnowflake.New(s)) {}
}
