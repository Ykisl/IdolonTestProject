using System;
using System.Collections.Generic;

namespace Project.EventTracking
{
    [Serializable]
    public class EventTrackingServiceSaveData
    {
        public List<TrackedEventData> TrackedEvents = new List<TrackedEventData>();
    }
}
