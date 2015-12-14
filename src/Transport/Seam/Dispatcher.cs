namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Extensibility;
    using NServiceBus.Logging;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class Dispatcher : IDispatchMessages
    {
        ILog logger = LogManager.GetLogger<Dispatcher>();
        readonly Batcher batcher;

        public Dispatcher(IRouteOutgoingMessages routeOutgoingMessages, ReadOnlySettings settings)
        {
            batcher = new Batcher(routeOutgoingMessages, settings);
        }

        public Task Dispatch(IEnumerable<TransportOperation> outgoingMessages, ContextBag context)
        {
            ReceiveContext receiveContext;
            
            context.TryGet(out receiveContext);
            if (receiveContext == null) // not in a receive context, so send out immediately
            {
                return batcher.SendInBatches(outgoingMessages, null);
            }

            var brokeredMessageReceiveContext = receiveContext as BrokeredMessageReceiveContext;

            if (brokeredMessageReceiveContext != null) // apply brokered message specific dispatching rules
            {
                return DispatchBrokeredMessages(outgoingMessages, brokeredMessageReceiveContext, context);
            }

            return batcher.SendInBatches(outgoingMessages, null); // otherwise send out immediately
        }

        Task DispatchBrokeredMessages(IEnumerable<TransportOperation> outgoingMessages, BrokeredMessageReceiveContext receiveContext, ReadOnlyContextBag context)
        {
            if (receiveContext.ReceiveMode == ReceiveMode.ReceiveAndDelete) // received brokered message has already been completed, so send everything out immediately
            {
                return batcher.SendInBatches(outgoingMessages, receiveContext);
            }
            else
            {
                var transportOperations = outgoingMessages as IList<TransportOperation> ?? outgoingMessages.ToList();
                var toBeDispatchedImmediately = transportOperations.Where(t => t.DispatchOptions.RequiredDispatchConsistency == DispatchConsistency.Isolated);
                var toBeDispatchedOnComplete = transportOperations.Where(t => t.DispatchOptions.RequiredDispatchConsistency == DispatchConsistency.Default);

                // default behavior is to postpone sends until complete (and potentially complete them as a single tx if possible)
                receiveContext.OnComplete.Add(() => batcher.SendInBatches(toBeDispatchedOnComplete, receiveContext));

                // but some messages may need to go out immediately
                return batcher.SendInBatches(toBeDispatchedImmediately, receiveContext);
            }
        }
    }
}