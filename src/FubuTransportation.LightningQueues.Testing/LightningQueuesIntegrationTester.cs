﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Logging;
using FubuMVC.Core.Runtime.Logging;
using FubuMVC.Core.ServiceBus.Configuration;
using FubuMVC.Core.ServiceBus.Runtime;
using FubuMVC.Core.ServiceBus.Runtime.Delayed;
using FubuTestingSupport;
using FubuTransportation.Testing;
using LightningQueues.Model;
using NUnit.Framework;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class LightningQueuesIntegrationTester
    {
        [SetUp]
        public void Setup()
        {
            SetupTransport("lq.tcp://localhost:2032/upstream");
        }

        private void SetupTransport(string uri)
        {
            graph = new ChannelGraph();
            node = graph.ChannelFor<ChannelSettings>(x => x.Upstream);
            node.Uri = new Uri(uri);
            node.Incoming = true;

            var delayedCache = new DelayedMessageCache<MessageId>();
            queues = new PersistentQueues(new RecordingLogger(), delayedCache, new LightningQueueSettings());
            queues.ClearAll();
            transport = new LightningQueuesTransport(queues, new LightningQueueSettings(), delayedCache);

            transport.OpenChannels(graph);
        }

        [TearDown]
        public void TearDown()
        {
            queues.Dispose();
        }

        private PersistentQueues queues;
        private LightningQueuesTransport transport;
        private ChannelGraph graph;
        private ChannelNode node;

        [Test]
        public void registers_a_reply_queue_corrected_to_the_machine_name()
        {
            var uri = graph.ReplyChannelFor(LightningUri.Protocol);
            uri.ShouldNotBeNull();

            uri.Host.ToUpperInvariant().ShouldEqual(Environment.MachineName.ToUpperInvariant());
        }

        [Test]
        public void reply_uri_is_machine_specific_when_dns_address_is_used()
        {
            queues.Dispose();
            SetupTransport("lq.tcp://www.foo.com:2032/upstream");

            var uri = graph.ReplyChannelFor(LightningUri.Protocol);
            uri.Host.ToUpperInvariant().ShouldEqual(Environment.MachineName.ToUpperInvariant());
        }

        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void send_a_message_and_get_it_back()
        {
            var envelope = new Envelope() {Data = new byte[] {1, 2, 3, 4, 5}};
            envelope.Headers["foo"] = "bar";

            var receiver = new RecordingReceiver();

            node.StartReceiving(receiver, new RecordingLogger());

            node.Channel.As<LightningQueuesChannel>().Send(envelope.Data, envelope.Headers);
            Wait.Until(() => receiver.Received.Any());

            graph.Dispose();
            queues.Dispose();

            receiver.Received.Any().ShouldBeTrue();

            Envelope actual = receiver.Received.Single();
            actual.Data.ShouldEqual(envelope.Data);
            actual.Headers["foo"].ShouldEqual("bar");
        }
    }

    public class ChannelSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }
}