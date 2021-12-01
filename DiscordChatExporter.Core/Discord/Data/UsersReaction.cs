namespace DiscordChatExporter.Core.Discord.Data
{
    using System.Collections.Generic;

    public record UsersReaction(Emoji Emoji, IReadOnlyList<User> Users)
    {
    }
}
