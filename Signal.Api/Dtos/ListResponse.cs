using System.Collections.Generic;

namespace Signal.Api.Dtos
{
    public abstract class ListResponse<TItem> : Response
    {
        public IEnumerable<TItem> Items { get; set; }

        public string? ContinuationMarker { get; set; }

        public int? TotalItemsCount { get; set; }

        protected ListResponse(IEnumerable<TItem> items)
        {
            this.Items = items ?? throw new global::System.ArgumentNullException(nameof(items));
        }
    }
}
