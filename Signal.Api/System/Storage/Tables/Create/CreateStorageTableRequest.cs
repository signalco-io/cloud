using System.ComponentModel.DataAnnotations;
using Voyager.Api;

namespace Signal.Api.System.Storage.Tables.Create
{
    [Voyager.Api.Route(HttpMethod.Get, "system/storage/tables/create")]
    public class CreateStorageTableRequest : EndpointRequest
    {
        [Required]
        public string? Name { get; set; }
    }
}
