﻿using System.Net;
using FubuMVC.Core;
using FubuMVC.Katana;
using Shouldly;
using NUnit.Framework;

namespace FubuMVC.IntegrationTesting
{
    [TestFixture]
    public class Auto_Katana_Hosting_Integration_Tester
    {
        [Test]
        public void use_auto_enabled_hosting_from_embedded_server()
        {
            using (var server = FubuApplication.For<KatanaRegistry>().RunEmbedded())
            {
                server.Endpoints.Get<KatanaEndpoint>(x => x.get_katana())
                    .ReadAsText()
                    .ShouldBe("this is served by katana");
            }
        }

        [Test]
        public void use_auto_enabled_hosting_as_is()
        {
            using (var runtime = FubuApplication.For<KatanaRegistry>().Bootstrap())
            {
                var client = new WebClient();
                client.DownloadString("http://localhost:5601/katana")
                    .ShouldBe("this is served by katana");
            }
        }
    }

    // SAMPLE: katana-auto-hosting
    public class KatanaRegistry : FubuRegistry
    {
        public KatanaRegistry()
        {
            Actions.IncludeType<KatanaEndpoint>();

            AlterSettings<KatanaSettings>(x =>
            {
                // This line is absolutely mandatory
                x.AutoHostingEnabled = true;

                x.Port = 5601;
            });
        }
    }

    // ENDSAMPLE

    public class KatanaEndpoint
    {
        public string get_katana()
        {
            return "this is served by katana";
        }
    }
}