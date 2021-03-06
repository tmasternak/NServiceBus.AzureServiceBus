namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using System.Collections.Generic;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_custom_outgoing_message_to_brokered_message_converter
    {
        [Test]
        public void Default_value_should_be_configured()
        {
            var settings = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settings);

            Assert.AreEqual(typeof(DefaultBatchedOperationsToBrokeredMessagesConverter), settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.FromOutgoingMessageConverter));
        }

        [Test]
        public void Should_be_able_to_set_custom_converter()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
#pragma warning disable 618
            extensions.UseOutgoingMessageToBrokeredMessageConverter<ConvertOutgoingMessagesToBrokeredMessages>();
#pragma warning restore 618
            Assert.AreEqual(typeof(ConvertOutgoingMessagesToBrokeredMessages), settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.FromOutgoingMessageConverter));
        }

#pragma warning disable 618
        class ConvertOutgoingMessagesToBrokeredMessages : IConvertOutgoingMessagesToBrokeredMessages
        {
            public IEnumerable<BrokeredMessage> Convert(IEnumerable<BatchedOperation> outgoingOperations, RoutingOptions routingOptions)
            {
                throw new NotImplementedException();
            }
        }
#pragma warning restore 618
    }
}