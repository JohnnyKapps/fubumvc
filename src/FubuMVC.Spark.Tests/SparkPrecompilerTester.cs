using System;
using System.Collections.Generic;
using Bottles;
using Bottles.Diagnostics;
using FubuMVC.Core;
using FubuMVC.Spark.Rendering;
using FubuMVC.Spark.SparkModel;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using Spark;

namespace FubuMVC.Spark.Tests
{
    [TestFixture]
    public class SparkPrecompilerTester : InteractionContext<SparkPrecompiler>
    {
        protected override void beforeEach()
        {
            var descriptor = new SparkDescriptor(new Template("a.spark", "root", "origin"));
            var nativePartial = new SparkDescriptor(new Template("_Yeah.spark", "root", "origin"));

            MockFor<ISparkTemplateRegistry>().Expect(x => x.ViewDescriptors()).Return(new[] {descriptor, nativePartial});
            MockFor<IViewEntryProviderCache>().Expect(x => x.GetViewEntry(Arg<SparkViewDescriptor>.Is.NotNull)).Repeat.Twice();
        }

        [Test]
        public void does_not_activate_when_in_development_mode()
        {
            var activated = false;

            FubuMode.Mode(FubuMode.Development);
            ClassUnderTest.UseActivation(p => activated = true);
            ClassUnderTest.Activate(MockFor<IEnumerable<IPackageInfo>>(), MockFor<IPackageLog>());

            activated.ShouldBeFalse();
        }

        [Test]
        public void activates_in_non_development_modes()
        {
            var activated = false;

            FubuMode.Mode("Chuck Norris Mode");
            ClassUnderTest.UseActivation(p => activated = true);
            ClassUnderTest.Activate(MockFor<IEnumerable<IPackageInfo>>(), MockFor<IPackageLog>());

            activated.ShouldBeTrueBecause("Chuck Norris would otherwise switch to RoR or MS MVC");
        }

        [Test]
        public void precompile_invokes_provider_cache_with_non_partial_descriptors_from_registry()
        {
            ClassUnderTest.Precompile(new PackageLog());

            MockFor<ISparkTemplateRegistry>().VerifyAllExpectations();
            MockFor<IViewEntryProviderCache>().VerifyAllExpectations();
        }

        [Test]
        public void precompile_uses_log_trap_errors()
        {
            ClassUnderTest.Precompile(MockFor<IPackageLog>());
            MockFor<IPackageLog>().AssertWasCalled(x => x.TrapErrors(Arg<Action>.Is.NotNull));
        }
    }
}