﻿using System;
using FubuMVC.Core.ServiceBus.InMemory;
using FubuMVC.Core.ServiceBus.Runtime;
using NUnit.Framework;
using System.Linq;
using Shouldly;

namespace FubuTransportation.Testing.InMemory
{
    [TestFixture]
    public class InMemoryCallbackTester
    {
        [Test]
        public void moves_to_delayed_queue()
        {
            InMemoryQueueManager.ClearAll();

            var envelope = new EnvelopeToken();
            var callback = new InMemoryCallback(null, envelope);
            callback.MoveToDelayedUntil(DateTime.Now);

            var delayed = InMemoryQueueManager.DelayedEnvelopes().Single();
            delayed.CorrelationId.ShouldBe(envelope.CorrelationId);
            delayed.ExecutionTime.HasValue.ShouldBeTrue();
        }
    }
}