using System;

namespace Couchbase.Extensions.Locks.Example.Models
{
    public class RequestWithLockModel
    {
        public bool WasLocked { get; set; }
        public TimeSpan LockDelayTime { get; set; }
        public TimeSpan LockHoldTime { get; set; }
    }
}
