using Signal.Api.Common.Users;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Signal.Api.Common.Entities
{
    [Serializable]
    public class EntityDetailsDto
    {
        public EntityDetailsDto(string id, string alias)
        {
            this.Id = id;
            this.Alias = alias;
        }

        [JsonPropertyName("id")]
        public string Id { get; }

        [JsonPropertyName("alias")]
        public string Alias { get; }

        [JsonPropertyName("contacts")]
        public IEnumerable<ContactDto>? Contacts { get; set; }

        [JsonPropertyName("sharedWith")]
        public IEnumerable<UserDto>? SharedWith { get; set; }
    }
}
