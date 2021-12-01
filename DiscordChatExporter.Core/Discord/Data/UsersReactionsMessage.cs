namespace DiscordChatExporter.Core.Discord.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public record UsersReactionsMessage(
        Message Message,
        IReadOnlyList<UsersReaction> Reactions)
    {
    }
}
