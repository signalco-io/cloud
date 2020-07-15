using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Signal.Core;
using Voyager.Api;

namespace Signal.Api.System.Storage.Queues.List
{
    public abstract class ServiceHandler<TRequest, TResponse, TService> : EndpointHandler<TRequest, TResponse>
        where TRequest : IRequest<ActionResult<TResponse>>
    {
        private readonly TService service;
        private readonly Expression<Func<TService, Task<TResponse>>> serviceCall;

        public ServiceHandler(
            TService service,
            Expression<Func<TService, Task<TResponse>>> serviceCall)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.serviceCall = serviceCall ?? throw new ArgumentNullException(nameof(serviceCall));
        }

        public override async Task<ActionResult<TResponse>> HandleRequestAsync(TRequest request) =>
            await this.serviceCall.Compile().Invoke(this.service);
    }

    public class StorageQueuesListHandler : ServiceHandler<StorageQueuesListRequest, StorageQueuesListResponse, IAzureStorage>
    {
        public StorageQueuesListHandler(IAzureStorage azureStorage)
            : base(azureStorage, service => Call(service))
        {
        }

        private static async Task<StorageQueuesListResponse> Call(IAzureStorage service)
        {
            return new StorageQueuesListResponse() { Items = await service.ListQueues() };
        }
    }
}
