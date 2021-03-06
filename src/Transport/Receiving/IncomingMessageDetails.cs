namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class IncomingMessageDetails
    {
        public IncomingMessageDetails(string messageId, Dictionary<string, string> headers, byte[] body)
        {
            MessageId = messageId;
            Headers = headers;
            Body = body;
        }

        public string MessageId { get; }

        public Dictionary<string, string> Headers { get; }

        public byte[] Body { get; }
    }
}