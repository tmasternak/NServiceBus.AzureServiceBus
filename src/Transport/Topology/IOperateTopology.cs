﻿namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    /// <summary>
    /// Operational aspects of running on top of the topology
    /// Takes care of the topology and it's specific state at runtime
    /// Examples
    /// Decisions of currently active namespace go here f.e.
    /// So is the list of notifiers etc...
    /// etc..
    /// </summary>
    public interface IOperateTopology
    {
        //invoked for static parts of the topology

        Task Start(TopologySection topology, int maximumConcurrency);
        Task Stop();

        //invoked whenever subscriptions are added or removed

        Task Start(IEnumerable<EntityInfo> subscriptions);
        Task Stop(IEnumerable<EntityInfo> subscriptions);

        // callback when there is a new message available, or an error occurs

        void OnIncomingMessage(Func<IncomingMessage, ReceiveContext, Task> func);

        void OnError(Func<Exception, Task> func);
    }
}