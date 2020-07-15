using System.Collections.Generic;

namespace Signal.Api.Dtos
{
    public abstract class Response
    {
        public IEnumerable<LinkDto>? Links { get; set; }
    }
}
