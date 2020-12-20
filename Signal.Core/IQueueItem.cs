using System;

namespace Signal.Core
{
    public interface IQueueItem
    {
        public Guid Id { get; set; }
        
        public string UserId { get; set; }
        
        public DateTime TimeStamp { get; set; }
    }
}