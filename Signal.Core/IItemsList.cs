using System.Collections.Generic;

namespace Signal.Core
{
    public interface IItemsList<TItem>
    {
        IEnumerable<TItem> Items { get; set; }

        string? ContinuationMarker { get; set; }

        int? TotalItemsCount { get; set; }
    }
}
