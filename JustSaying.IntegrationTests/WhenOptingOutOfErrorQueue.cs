﻿using System.Threading.Tasks;
using Amazon;
using Amazon.SQS;
using JustSaying.AwsTools.MessageHandling;
using JustSaying.Messaging.MessageHandling;
using JustSaying.TestingFramework;
using NUnit.Framework;

namespace JustSaying.IntegrationTests
{
    public class OrderPlacedHandler : IHandlerAsync<GenericMessage>
    {
        public Task<bool> Handle(GenericMessage message)
        {
            return Task.FromResult(true);
        }
    }

    public class WhenOptingOutOfErrorQueue
    {
        private IAmazonSQS _client;

        [SetUp]
        public void SetUp()
        {
            _client = CreateMeABus.DefaultClientFactory().GetSqsClient(RegionEndpoint.EUWest1);
        }

        [Test]
        public void ErrorQueueShouldNotBeCreated()
        {
            var queueName = "test-queue-issue-191";
            CreateMeABus.InRegion("eu-west-1")
                .WithSnsMessagePublisher<GenericMessage>()

                .WithSqsTopicSubscriber()
                .IntoQueue(queueName)
                .ConfigureSubscriptionWith(policy =>
                {
                    policy.ErrorQueueOptOut = true;
                })
                .WithMessageHandler(new OrderPlacedHandler());

            AssertThatQueueDoesNotExist(queueName+ "_error");
        }

        private void AssertThatQueueDoesNotExist(string name)
        {
            var sqsQueueByName = new SqsQueueByName(RegionEndpoint.EUWest1, name, _client, 1);
            Assert.IsFalse(sqsQueueByName.Exists(), $"Expecting queue '{name}' to not exist but it does.");
        }
    }
}