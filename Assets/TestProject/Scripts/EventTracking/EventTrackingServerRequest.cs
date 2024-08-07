using System;
using System.Collections.Generic;

namespace Project.EventTracking
{
    [Serializable]
    public class EventTrackingServerRequest
    {
        public List<TrackedEventData> events;

        public EventTrackingServerRequest(ICollection<TrackedEventData> trackedEvents)
        {
            events = new List<TrackedEventData>(trackedEvents);
        }
    }
}
