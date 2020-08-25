using System.Collections.Generic;

namespace Signal.Core
{
    public abstract class ItemsList<TItem> : IItemsList<TItem>
    {
        public IEnumerable<TItem> Items { get; set; }

        public string? ContinuationMarker { get; set; }

        public int? TotalItemsCount { get; set; }

        public ItemsList(IEnumerable<TItem> items)
        {
            Items = items ?? throw new System.ArgumentNullException(nameof(items));
        }
    }
}
