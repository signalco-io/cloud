using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Signal.Core;
using Voyager.Api;

namespace Signal.Api.Handlers
{
    public abstract class ServiceHandler<TRequest, TResponse, TService, TServiceResult> : EndpointHandler<TRequest, TResponse>
        where TRequest : IRequest<ActionResult<TResponse>>
    {
        private readonly TService service;
        private readonly IMapper mapper;
        private readonly Func<TRequest, TService, CancellationToken, Task<TServiceResult>> serviceCall;

        protected ServiceHandler(
            IServiceProvider serviceProvider,
            Func<TRequest, TService, CancellationToken, Task<TServiceResult>> serviceCall)
        {
            this.service = (TService)serviceProvider.GetService(typeof(TService));
            this.mapper = (IMapper)serviceProvider.GetService(typeof(IMapper));
            this.serviceCall = serviceCall ?? throw new ArgumentNullException(nameof(serviceCall));
        }

        public override async Task<ActionResult<TResponse>> HandleRequestAsync(TRequest request, CancellationToken cancellationToken)
        {
            var result = await this.serviceCall.Invoke(request, this.service, cancellationToken);
            if (result != null)
                return result.MapTo<TResponse>(this.mapper);

            throw new Exception($"Didn't get response from service {typeof(TService).FullName}.");
        }
    }
}
