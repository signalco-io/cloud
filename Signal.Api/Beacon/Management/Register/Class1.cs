using System;
using System.Collections.Generic;
using System.Text;
using Voyager.Api;

namespace Signal.Api.Beacon.Management.Register
{
    public class BeaconManagementRegisterResponse
    {
    }

    public class BeaconManagementRegisterHandler
    {

    }

    [VoyagerRoute(HttpMethod.Post, "beacon/management/register")]
    public class BeaconManagementRegisterRequest : EndpointRequest<BeaconManagementRegisterResponse>
    {
        public string Email { get; }
    }
}
