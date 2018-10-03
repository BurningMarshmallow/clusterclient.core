﻿using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.ClusterClient.Core.Topology;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core
{
    /// <inheritdoc />
    [PublicAPI]
    public class ClusterClient : IClusterClient
    {
        private static readonly TimeSpan BudgetPrecision = TimeSpan.FromMilliseconds(15);

        private readonly ClusterClientConfiguration configuration;
        private readonly Func<IRequestContext, Task<ClusterResult>> pipelineDelegate;

        /// <summary>
        /// Creates a <see cref="ClusterClient"/> instance using given <paramref name="log"/> and <paramref name="setup"/> delegate.
        /// </summary>
        /// <exception cref="ClusterClientException">Configuration was incomplete or invalid.</exception>
        public ClusterClient(ILog log, ClusterClientSetup setup)
        {
            configuration = new ClusterClientConfiguration((log ?? new SilentLog()));

            setup(configuration);

            configuration.ValidateOrDie();
            configuration.AugmentWithDefaults();

            if (configuration.ReplicaTransform != null)
                configuration.ClusterProvider = new TransformingClusterProvider(configuration.ClusterProvider, configuration.ReplicaTransform);

            ReplicaStorageProvider = ReplicaStorageProviderFactory.Create(configuration.ReplicaStorageScope);

            var modules = RequestModuleChainBuilder.BuildChain(configuration, ReplicaStorageProvider);

            pipelineDelegate = RequestModuleChainBuilder.BuildChainDelegate(modules);
        }

        /// <summary>
        /// A <see cref="IClusterProvider"/> implementation that used by this <see cref="ClusterClient"/> instance.
        /// </summary>
        public IClusterProvider ClusterProvider => configuration.ClusterProvider;

        /// <summary>
        /// A <see cref="IReplicaStorageProvider"/> implementation that used by this <see cref="ClusterClient"/> instance.
        /// </summary>
        public IReplicaStorageProvider ReplicaStorageProvider { get; }

        /// <inheritdoc />
        public Task<ClusterResult> SendAsync(Request request, RequestParameters parameters, TimeSpan? timeout = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return pipelineDelegate(new RequestContext(
                request,
                CompleteParameters(parameters),
                RequestTimeBudget.StartNew(timeout ?? configuration.DefaultTimeout, BudgetPrecision),
                configuration.Log,
                configuration.Transport,
                configuration.MaxReplicasUsedPerRequest,
                configuration.ClientApplicationName,
                cancellationToken));
        }

        private RequestParameters CompleteParameters(RequestParameters parameters)
        {
            if (parameters == null)
                return new RequestParameters(configuration.DefaultRequestStrategy, configuration.DefaultPriority);
            
            if (parameters.Strategy == null)
                parameters = parameters.WithStrategy(configuration.DefaultRequestStrategy);
            if (parameters.Priority == null)
                parameters = parameters.WithPriority(configuration.DefaultPriority);
            return parameters;
        }
    }
}