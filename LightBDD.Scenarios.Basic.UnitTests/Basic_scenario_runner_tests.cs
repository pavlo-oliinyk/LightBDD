﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightBDD.Core.Extensibility;
using Moq;
using NUnit.Framework;

namespace LightBDD.Scenarios.Basic.UnitTests
{
    public interface ITestableBddRunner : IBddRunner, ICoreBddRunner { }

    [TestFixture]
    public class Basic_scenario_runner_tests
    {
        private StepDescriptor[] _capturedDescriptors;
        private Mock<IScenarioRunner> _mockScenarioRunner;
        private Mock<ITestableBddRunner> _mockRunner;
        private ITestableBddRunner Runner => _mockRunner.Object;

        [SetUp]
        public void SetUp()
        {
            _capturedDescriptors = null;
            _mockScenarioRunner = new Mock<IScenarioRunner>();
            _mockRunner = new Mock<ITestableBddRunner>();
        }

        [Test]
        public void It_should_allow_to_run_synchronous_scenarios()
        {
            ExpectNewScenario();
            ExpectWithCapturedScenarioDetails();
            ExpectWithSteps();
            ExpectRunSynchronously();

            Runner.Basic().RunScenario(Step_one, Step_two);

            _mockRunner.Verify();
            _mockScenarioRunner.Verify();

            Assert.That(_capturedDescriptors, Is.Not.Null);
            Assert.That(_capturedDescriptors.Length, Is.EqualTo(2));

            AssertStep(_capturedDescriptors[0], nameof(Step_one));
            AssertStep(_capturedDescriptors[1], nameof(Step_two));
        }

        [Test]
        public void It_should_make_synchronous_steps_finishing_immediately_in_async_mode()
        {
            ExpectNewScenario();
            ExpectWithCapturedScenarioDetails();
            ExpectWithSteps();
            ExpectRunSynchronously();

            Runner.Basic().RunScenario(Step_not_throwing_exception);

            Assert.That(_capturedDescriptors, Is.Not.Null);
            Assert.That(_capturedDescriptors.Length, Is.EqualTo(1));

            Assert.True(_capturedDescriptors[0].StepInvocation.Invoke(null, null).IsCompleted, "Synchronous step should be completed after invocation");
        }

        [Test]
        public async Task It_should_allow_to_run_asynchronous_scenarios()
        {
            ExpectNewScenario();
            ExpectWithCapturedScenarioDetails();
            ExpectWithSteps();
            ExpectRunAsynchronously();

            await Runner.Basic().RunScenarioAsync(Step_one_async, Step_two_async);

            _mockRunner.Verify();
            _mockScenarioRunner.Verify();

            Assert.That(_capturedDescriptors, Is.Not.Null);
            Assert.That(_capturedDescriptors.Length, Is.EqualTo(2));

            AssertStep(_capturedDescriptors[0], nameof(Step_one_async));
            AssertStep(_capturedDescriptors[1], nameof(Step_two_async));
        }

        private void AssertStep(StepDescriptor step, string expectedName)
        {
            Assert.That(step.RawName, Is.EqualTo(expectedName), nameof(step.RawName));
            Assert.That(step.Parameters, Is.Empty, nameof(step.Parameters));
            Assert.That(step.PredefinedStepType, Is.Null, nameof(step.PredefinedStepType));

            var ex = Assert.Throws<Exception>(() => step.StepInvocation.Invoke(null, null).GetAwaiter().GetResult());
            Assert.That(ex.Message, Is.EqualTo(expectedName));
        }

        #region Expectations

        private void ExpectRunSynchronously()
        {
            _mockScenarioRunner
                .Setup(r => r.RunSynchronously())
                .Verifiable();
        }

        private void ExpectWithSteps()
        {
            _mockScenarioRunner
                .Setup(s => s.WithSteps(It.IsAny<IEnumerable<StepDescriptor>>()))
                .Returns((IEnumerable<StepDescriptor> desc) =>
                {
                    _capturedDescriptors = desc.ToArray();
                    return _mockScenarioRunner.Object;
                })
                .Verifiable();
        }

        private void ExpectWithCapturedScenarioDetails()
        {
            _mockScenarioRunner
                .Setup(s => s.WithCapturedScenarioDetails())
                .Returns(_mockScenarioRunner.Object)
                .Verifiable();
        }

        private void ExpectNewScenario()
        {
            _mockRunner
                .Setup(r => r.NewScenario())
                .Returns(_mockScenarioRunner.Object)
                .Verifiable();
        }

        private void ExpectRunAsynchronously()
        {
            _mockScenarioRunner
                .Setup(r => r.RunAsynchronously())
                .Returns(Task.CompletedTask)
                .Verifiable();
        }

        #endregion
        #region Steps
        private void Step_one() { throw new Exception(nameof(Step_one)); }
        private void Step_two() { throw new Exception(nameof(Step_two)); }
        private void Step_not_throwing_exception() { }
        private async Task Step_one_async()
        {
            await Task.Yield();
            throw new Exception(nameof(Step_one_async));
        }
        private async Task Step_two_async()
        {
            await Task.Yield();
            throw new Exception(nameof(Step_two_async));
        }
        #endregion
    }
}
