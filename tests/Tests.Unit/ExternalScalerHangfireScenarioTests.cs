using System;
using System.Collections.Generic;
using System.Linq;
using Externalscaler;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using HangfireExternalScaler.Configuration;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;

namespace HangfireExternalScaler.Tests.Unit
{
    /// <summary>
    /// This Test Fixture starts an instance of the Keda.ExternalScaler.Hangfire and then runs unit tests in a fixed order.
    ///
    /// NOTE: These tests are chained and you must run all of them together. NUnit will take care of enforcing the ordering.
    /// </summary>
    [NonParallelizable]
    public class ExternalScalerHangfireScenarioTests
    {
        private ExternalScalerHangfireHelper _server;
        private GrpcChannel _channel;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Log.Logger = SerilogConfiguration.Create("Keda.Scaler.Hangfire.Unit.Tests", LogEventLevel.Debug);

            ISettings settings = new Settings()
            {
                HangfireSqlInstances = new List<HangfireSqlServerSettings>()
                {
                    new HangfireSqlServerSettings()
                    {
                        Name = "Test1",
                        Username = "sa",
                        Password = "NotRequired",
                        Address = "IgnoreThis"
                    }
                }
            };

            _server = new ExternalScalerHangfireHelper(settings);
            _server.Run();
            
            _channel = GrpcChannel.ForAddress(new Uri("https://localhost:5001"));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Log.CloseAndFlush();
            _server.Stop();
        }

        [Test]
        public void ExternalScalerServer_IsActive_ShouldReturnSuccessfully()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);
            
            // act
            var reply = client.IsActive(new ScaledObjectRef()
            {
                Name = "SomeScalerName",
                Namespace = "default",
                ScalerMetadata =
                {
                    {"hangfireInstance","Test1"},
                    {"targetSize","5"},
                    {"queue","bar"}
                }
            });

            // assert
            reply.Should().NotBeNull();
        }

        [Test]
        public void ExternalScalerServer_IsActive_WhenHangfireInstanceDoesNotExist_ShouldLogError()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);

            // act
            try
            {
                client.IsActive(new ScaledObjectRef()
                {
                    Name = "ScalerName",
                    Namespace = "default",
                    ScalerMetadata =
                    {
                        {"hangfireInstance","DoesNotExist"},
                        {"targetSize","5"},
                        {"queue","bar"}
                    }
                });
            }
            catch (RpcException )
            {
            }

            // assert
            InMemorySink.Instance.LogEvents.Count(x => x.Level == LogEventLevel.Error &&
                                                       x.MessageTemplate.Text.Equals("Hangfire instance {HangfireInstanceName} is not configured"))
                .Should().Be(1);
        }

        [Test]
        public void ExternalScalerServer_IsActive_WhenHangfireInstanceDoesNotExist_ShouldThrowRpcException_WithStatusCodeInvalidArgument()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);

            RpcException returnedException = new RpcException(new Status(StatusCode.Unknown, "Initial test exception"));
            
            // act
            try
            {
                client.IsActive(new ScaledObjectRef()
                {
                    Name = "ScalerName",
                    Namespace = "default",
                    ScalerMetadata =
                    {
                        {"hangfireInstance","DoesNotExist"},
                        {"targetSize","5"},
                        {"queue","bar"}
                    }
                });
            }
            catch (RpcException ex)
            {
                returnedException = ex;
            }

            // assert
            returnedException.Should().NotBeNull();
            returnedException.StatusCode.Should().Be(StatusCode.InvalidArgument);
            returnedException.Message.Should().Contain("Detail=\"Hangfire instance DoesNotExist is not configured\"");
        }
        
        [Test]
        public void ExternalScalerServer_IsActive_WhenNoJobsAreEnqueuedOrFetched_ShouldReturnActiveAsFalse()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);

            _server.MonitoringApis.First(x => x.Name.Equals("Test1")).SetEnqueuedCount("QueueA", 0);
            _server.MonitoringApis.First(x => x.Name.Equals("Test1")).SetFetchedCount("QueueA", 0);

            var reply = client.IsActive(new ScaledObjectRef()
            {
                Name = "SomeScalerName",
                Namespace = "default",
                ScalerMetadata =
                {
                    {"hangfireInstance","Test1"},
                    {"targetSize","5"},
                    {"queue","QueueA"}
                }
            });

            // assert
            reply.Should().NotBeNull();
            reply.Result.Should().BeFalse();
        }

        [Test]
        public void ExternalScalerServer_IsActive_WhenNoJobsAreEnqueuedButJobsAreFetched_ShouldReturnActiveAsTrue()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);

            _server.MonitoringApis.First(x => x.Name.Equals("Test1")).SetEnqueuedCount("QueueA", 0);
            _server.MonitoringApis.First(x => x.Name.Equals("Test1")).SetFetchedCount("QueueA", 1);

            var reply = client.IsActive(new ScaledObjectRef()
            {
                Name = "SomeScalerName",
                Namespace = "default",
                ScalerMetadata =
                {
                    {"hangfireInstance","Test1"},
                    {"targetSize","5"},
                    {"queue","QueueA"}
                }
            });

            // assert
            reply.Should().NotBeNull();
            reply.Result.Should().BeTrue("While a job is being processed we should inform KEDA we are active");
        }

        [Test]
        public void ExternalScalerServer_IsActive_WhenJobsAreEnqueuedButNoJobsAreFetched_ShouldReturnActiveAsTrue()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);

            _server.MonitoringApis.First(x => x.Name.Equals("Test1")).SetEnqueuedCount("QueueA", 1);
            _server.MonitoringApis.First(x => x.Name.Equals("Test1")).SetFetchedCount("QueueA", 0);

            var reply = client.IsActive(new ScaledObjectRef()
            {
                Name = "SomeScalerName",
                Namespace = "default",
                ScalerMetadata =
                {
                    {"hangfireInstance","Test1"},
                    {"targetSize","5"},
                    {"queue","QueueA"}
                }
            });

            // assert
            reply.Should().NotBeNull();
            reply.Result.Should().BeTrue("While jobs are enqueued we should inform KEDA we are active");
        }


        [Test]
        public void ExternalScalerServer_GetMetricSpec_ShouldReturnTargetSizeAsScaleRecommendation()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);

            // act
            var reply = client.GetMetricSpec(new ScaledObjectRef()
            {
                Name = "SomeScalerName",
                Namespace = "default",
                ScalerMetadata =
                {
                    {"hangfireInstance","Test1"},
                    {"queue","QueueA"},
                    {"targetSize", "10"}
                }
            });

            // assert
            reply.Should().NotBeNull();
            reply.MetricSpecs.First(x => x.MetricName.Equals("ScaleRecommendation")).TargetSize.Should().Be(10);
        }

        [Test]
        public void ExternalScalerServer_GetMetrics_WhenNoJobsAreEnqueued_ShouldReturnQueueLengthAsZero()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);

            _server.MonitoringApis.First(x => x.Name.Equals("Test1")).SetEnqueuedCount("QueueA", 0);

            var reply = client.GetMetrics(new GetMetricsRequest()
            {
                MetricName = "queueLength",
                ScaledObjectRef = new ScaledObjectRef()
                {
                    Name = "SomeScalerName",
                    Namespace = "default",
                    ScalerMetadata =
                    {
                        {"hangfireInstance","Test1"},
                        {"queue","QueueA"},
                        {"targetSize", "10"}
                    }
                }
            });

            // assert
            reply.Should().NotBeNull();
            reply.MetricValues.First(x => x.MetricName.Equals("queueLength")).MetricValue_.Should().Be(0);
        }

        [Test]
        public void ExternalScalerServer_GetMetrics_WhenJobsAreEnqueued_ShouldReturnNumberOfEnqueueJobsAsQueueLength()
        {
            var client = new ExternalScaler.ExternalScalerClient(_channel);

            _server.MonitoringApis.First(x => x.Name.Equals("Test1")).SetEnqueuedCount("QueueA", 5);

            var reply = client.GetMetrics(new GetMetricsRequest()
            {
                MetricName = "queueLength",
                ScaledObjectRef = new ScaledObjectRef()
                {
                    Name = "SomeScalerName",
                    Namespace = "default",
                    ScalerMetadata =
                    {
                        {"hangfireInstance","Test1"},
                        {"queue","QueueA"},
                        {"targetSize", "10"}
                    }
                }
            });

            // assert
            reply.Should().NotBeNull();
            reply.MetricValues.First(x => x.MetricName.Equals("queueLength")).MetricValue_.Should().Be(5);
        }
    }
}
