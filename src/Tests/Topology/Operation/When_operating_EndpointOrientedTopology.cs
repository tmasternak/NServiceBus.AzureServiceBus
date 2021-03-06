namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Operation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Tests;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;
    using Transport;

#pragma warning disable 618
    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_operating_EndpointOrientedTopology
    {
        [Test]
        public async Task Receives_incoming_messages_from_endpoint_queue()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupEndpointOrientedTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                completed.Set();

                return TaskEx.Completed;
            });
            topologyOperator.OnError(exception =>
            {
                ex = exception;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());


            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            await topologyOperator.Stop();
        }

        [Test]
        public async Task Calls_on_error_when_error_during_processing()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupEndpointOrientedTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var error = new AsyncAutoResetEvent(false);

            var received = false;
            var errorOccured = false;

            topologyOperator.OnIncomingMessage(async (message, context) =>
            {
                received = true;
                await Task.Delay(1).ConfigureAwait(false);
                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await error.WaitAsync(cts.Token);

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await topologyOperator.Stop();
        }

   
        [Test]
        public async Task Completes_incoming_message_when_successfully_received()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupEndpointOrientedTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                completed.Set();

                return TaskEx.Completed;
            });
            topologyOperator.OnError(exception =>
            {
                ex = exception;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get("namespace");
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(0, queueDescription.MessageCount);

            await topologyOperator.Stop();
        }

        [Test]
        public async Task Aborts_incoming_message_when_error_during_processing()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupEndpointOrientedTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            var errorOccured = false;

            topologyOperator.OnIncomingMessage(async (message, context) =>
            {
                received = true;
                await Task.Delay(1).ConfigureAwait(false);
                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get("namespace");
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(1, queueDescription.MessageCount);

            await topologyOperator.Stop();
        }

        async Task<ITopologySectionManager> SetupEndpointOrientedTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            settings.Set<Conventions>(new Conventions());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace", AzureServiceBusConnectionString.Value);

            var topology = new EndpointOrientedTopology(container);
            topology.Initialize(settings);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopology)container.Resolve(typeof(TopologyCreator));

            var sectionManager = container.Resolve<ITopologySectionManager>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate(new QueueBindings()));
            return sectionManager;
        }
    }
}
