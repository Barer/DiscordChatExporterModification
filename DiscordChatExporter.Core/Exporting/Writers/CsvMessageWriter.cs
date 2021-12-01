using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting.Writers.MarkdownVisitors;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Core.Exporting.Writers
{
    internal partial class CsvMessageWriter : MessageWriter
    {
        private readonly TextWriter _writer;

        public CsvMessageWriter(Stream stream, ExportContext context)
            : base(stream, context)
        {
            _writer = new StreamWriter(stream);
        }

        private string FormatMarkdown(string? markdown) =>
            PlainTextMarkdownVisitor.Format(Context, markdown ?? "");

        public override async ValueTask WritePreambleAsync(CancellationToken cancellationToken = default) =>
            await _writer.WriteLineAsync("AuthorID,Author,Date,Content,Attachments,Reactions");

        private async ValueTask WriteAttachmentsAsync(
            IReadOnlyList<Attachment> attachments,
            CancellationToken cancellationToken = default)
        {
            var buffer = new StringBuilder();

            foreach (var attachment in attachments)
            {
                cancellationToken.ThrowIfCancellationRequested();

                buffer
                    .AppendIfNotEmpty(',')
                    .Append(await Context.ResolveMediaUrlAsync(attachment.Url, cancellationToken));
            }

            await _writer.WriteAsync(CsvEncode(buffer.ToString()));
        }

        private async ValueTask WriteUsersReactionsAsync(
            IReadOnlyList<UsersReaction> usersReaction,
            CancellationToken cancellationToken = default)
        {
            var buffer = new StringBuilder();

            foreach (var reaction in usersReaction)
            {
                cancellationToken.ThrowIfCancellationRequested();

                buffer
                    .AppendIfNotEmpty(',')
                    .Append(reaction.Emoji.Name)
                    .Append(' ')
                    .Append('(')
                    .Append(string.Join(", ", reaction.Users.Select(x => x.Id)))
                    .Append(')');
            }

            await _writer.WriteAsync(CsvEncode(buffer.ToString()));
        }

        public override async ValueTask WriteUsersReactionsMessageAsync(
            UsersReactionsMessage usersReactionsMessage,
            CancellationToken cancellationToken = default)
        {
            var message = usersReactionsMessage.Message;

            await base.WriteUsersReactionsMessageAsync(usersReactionsMessage, cancellationToken);

            // Author ID
            await _writer.WriteAsync(CsvEncode(message.Author.Id.ToString()));
            await _writer.WriteAsync(',');

            // Author name
            await _writer.WriteAsync(CsvEncode(message.Author.FullName));
            await _writer.WriteAsync(',');

            // Message timestamp
            await _writer.WriteAsync(CsvEncode(Context.FormatDate(message.Timestamp)));
            await _writer.WriteAsync(',');

            // Message content
            await _writer.WriteAsync(CsvEncode(FormatMarkdown(message.Content)));
            await _writer.WriteAsync(',');

            // Attachments
            await WriteAttachmentsAsync(message.Attachments, cancellationToken);
            await _writer.WriteAsync(',');

            // Reactions
            await WriteUsersReactionsAsync(usersReactionsMessage.Reactions, cancellationToken);

            // Finish row
            await _writer.WriteLineAsync();
        }

        public override async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
            await base.DisposeAsync();
        }
    }

    internal partial class CsvMessageWriter
    {
        private static string CsvEncode(string value)
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
    }
}