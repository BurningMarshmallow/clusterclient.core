﻿using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Tests.Model
{
    [TestFixture]
    internal class ClusterResult_Tests
    {
        private Request request;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("foo/bar");   
        }

        [Test]
        public void Throttled_factory_method_should_return_correct_result()
        {
            var result = ClusterResultFactory.Throttled(request);

            result.Status.Should().Be(ClusterResultStatus.Throttled);
            result.Request.Should().BeSameAs(request);
            result.ReplicaResults.Should().BeEmpty();
            result.Response.Code.Should().Be(ResponseCode.TooManyRequests);
        }
    }
}
