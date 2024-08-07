using Project.Core;
using Project.DataSaving;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Project.EventTracking
{
    public class EventTrackingService : MonoBehaviour
    {
        [SerializeField] private DataSavingService _dataSavingService;
        [Space]
        [SerializeField] private string _serverUrl;
        [SerializeField] private float _cooldownBeforeSend = 2f;

        private const string DATA_FILE_NAME = "TrackedEventsData";

        private List<TrackedEventData> _trackedEvents;
        private bool _isInitialized;

        private Timer _sendTimer;

        public bool IsInitialized => _isInitialized;

        #region UNITY_EVENTS

        private void Start()
        {
            Initialzie();
        }

        private void OnDestroy()
        {
            Deinitialzie();
        }


        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            var delta = Time.deltaTime;
            UpdateTimer(delta);

            //FAKE EVENTS
            if (Input.GetKeyDown(KeyCode.K))
            {
                TrackEvent("Event01", "a:3, b:0");
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                TrackEvent("Event02", "level:7, hp:100");
            }
        }

        #endregion

        private void Initialzie()
        {
            if (_isInitialized)
            {
                return;
            }

            _trackedEvents = LoadTrackedEvents();

            _sendTimer = new Timer();
            _sendTimer.TimerFinished += HandleSendTimerFinished;

            if(_trackedEvents.Count > 0)
            {
                _sendTimer.Init(_cooldownBeforeSend);
            }

            _isInitialized = true;
        }

        private void Deinitialzie()
        {
            if (!_isInitialized)
            {
                return;
            }

            _sendTimer.TimerFinished -= HandleSendTimerFinished;
            _sendTimer = null;

            _trackedEvents.Clear();

            _isInitialized = false;
        }

        public void TrackEvent(string type, string data)
        {
            if (!_isInitialized)
            {
                return;
            }

            var trackedEventData = new TrackedEventData()
            {
                type = type,
                data = data
            };

            _sendTimer.Init(_cooldownBeforeSend);

            _trackedEvents.Add(trackedEventData);
            SaveTrackedEvents();
        }

        public async Task<bool> TrySendTrackedEventsAsync()
        {
            if (!_isInitialized)
            {
                return false;
            }

            if (_trackedEvents.Count <= 0)
            {
                return true;
            }

            var request = new EventTrackingServerRequest(_trackedEvents);
            if(!await TrySendTrackedEventsAsyncInternal(request))
            {
                _sendTimer.Reset();
                return false;
            }

            _sendTimer.DeInit();
            _trackedEvents.Clear();
            SaveTrackedEvents();

            return true;
        }

        private void UpdateTimer(float delta)
        {
            _sendTimer?.Update(delta);
        }

        private void HandleSendTimerFinished()
        {
            TrySendTrackedEventsAsync().GetAwaiter();
        }

        public async Task<bool> TrySendTrackedEventsAsyncInternal(EventTrackingServerRequest trackingServerRequest)
        {
            if(Application.internetReachability == NetworkReachability.NotReachable)
            {
                return false;
            }

            var requestJson = JsonUtility.ToJson(trackingServerRequest);
            Debug.Log($"Sending tracked events: {requestJson}");

            using (UnityWebRequest request = UnityWebRequest.Post(_serverUrl, requestJson))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = 20;
                request.SendWebRequest();

                while (!request.isDone)
                {
                    await Task.Delay(100);
                }

                Debug.Log($"Sending finished with {request.responseCode} code");
                return request.isDone && request.responseCode == 200;
            }
        }

        private List<TrackedEventData> LoadTrackedEvents()
        {
            if(!_dataSavingService.TryLoadData<EventTrackingServiceSaveData>(DATA_FILE_NAME, out var loadedData))
            {
                return new List<TrackedEventData>();
            }

            return loadedData.TrackedEvents;
        }

        private void SaveTrackedEvents()
        {
            var saveData = new EventTrackingServiceSaveData()
            {
                TrackedEvents = _trackedEvents
            };

            _dataSavingService.SaveData<EventTrackingServiceSaveData>(DATA_FILE_NAME, saveData);
        }
    }
}
