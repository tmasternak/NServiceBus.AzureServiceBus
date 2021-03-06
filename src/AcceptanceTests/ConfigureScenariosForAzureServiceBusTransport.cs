using System;
using System.Collections.Generic;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.AcceptanceTests.ScenarioDescriptors;

public class ConfigureScenariosForAzureServiceBusTransport : IConfigureSupportedScenariosForTestExecution
{
    public IEnumerable<Type> UnsupportedScenarioDescriptorTypes { get; } = new[]
    {
        typeof(AllDtcTransports),
        typeof(AllTransportsWithMessageDrivenPubSub),
        typeof(AllTransportsWithoutNativeDeferral),
        typeof(AllNativeMultiQueueTransactionTransports),
        typeof(AllTransportsWithoutNativeDeferralAndWithAtomicSendAndReceive)
    };
}