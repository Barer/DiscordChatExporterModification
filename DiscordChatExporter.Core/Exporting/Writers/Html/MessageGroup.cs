using System;
using System.Collections.Generic;
using System.Linq;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Writers.Html
{
    // Used for grouping contiguous messages in HTML export
    internal partial class MessageGroup
    {
        public User Author { get; }

        public DateTimeOffset Timestamp { get; }

        public IReadOnlyList<UsersReactionsMessage> Messages { get; }

        public MessageReference? Reference { get; }

        public Message? ReferencedMessage {get; }

        public MessageGroup(
            User author,
            DateTimeOffset timestamp,
            MessageReference? reference,
            Message? referencedMessage,
            IReadOnlyList<UsersReactionsMessage> messages)
        {
            Author = author;
            Timestamp = timestamp;
            Reference = reference;
            ReferencedMessage = referencedMessage;
            Messages = messages;
        }
    }

    internal partial class MessageGroup
    {
        public static bool CanJoin(UsersReactionsMessage message1, UsersReactionsMessage message2) =>
            // Must be from the same author
            message1.Message.Author.Id == message2.Message.Author.Id &&
            // Author's name must not have changed between messages
            string.Equals(message1.Message.Author.FullName, message2.Message.Author.FullName, StringComparison.Ordinal) &&
            // Duration between messages must be 7 minutes or less
            (message2.Message.Timestamp - message1.Message.Timestamp).Duration().TotalMinutes <= 7 &&
            // Other message must not be a reply
            message2.Message.Reference is null;

        public static MessageGroup Join(IReadOnlyList<UsersReactionsMessage> messages)
        {
            var first = messages.First().Message;

            return new MessageGroup(
                first.Author,
                first.Timestamp,
                first.Reference,
                first.ReferencedMessage,
                messages
            );
        }
    }
}