using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Diagnostics;
using FubuMVC.Core.Registration;
using Shouldly;
using NUnit.Framework;

namespace FubuMVC.Tests
{
    [TestFixture]
    public class FubuApplicationTester
    {
        [SetUp]
        public void SetUp()
        {
            var system = new FileSystem();
            system.FindFiles(".".ToFullPath(), new FileSet
            {
                Include = "*.asset.config;*.script.config"
            }).ToList().Each(system.DeleteFile);
        }

        [Test]
        public void the_restarted_property_is_set()
        {
            var floor = DateTime.Now.AddSeconds(-5);
            var ceiling = DateTime.Now.AddSeconds(5);

            FubuApplication.Restarted = null;

            using (var runtime = FubuApplication.For(new FubuRegistry()).Bootstrap())
            {
                (floor < FubuApplication.Restarted && FubuApplication.Restarted < ceiling).ShouldBeTrue();
            }
        }

        [Test]
        public void description_smoke_tester()
        {
            using (var runtime = FubuApplication.DefaultPolicies().Bootstrap())
            {
                var description = FubuApplicationDescriber.WriteDescription(runtime.Behaviors.Diagnostics);

                Console.WriteLine(description);
            }
        }

        [Test]
        public void can_use_the_default_policies()
        {
            var application = FubuApplication.DefaultPolicies().Bootstrap();
            var graph = application.Factory.Get<BehaviorGraph>();

            graph.BehaviorFor<TargetEndpoint>(x => x.get_hello()).ShouldNotBeNull();
        }
    }

    public class TargetEndpoint
    {
        public string get_hello()
        {
            return "Hello";
        }
    }
}