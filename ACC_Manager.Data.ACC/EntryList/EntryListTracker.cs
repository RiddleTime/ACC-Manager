﻿using ACCManager.Broadcast;
using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.Tracker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.EntryList
{
    public class EntryListTracker
    {
        public class CarData
        {
            public CarInfo CarInfo { get; set; }
            public RealtimeCarUpdate RealtimeCarUpdate { get; set; }
        }

        private static EntryListTracker _instance;
        public static EntryListTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new EntryListTracker();
                return _instance;
            }
            private set { _instance = value; }
        }

        public List<KeyValuePair<int, CarData>> Cars { get { lock (_entryListCars) { return _entryListCars.ToList(); } } }
        private Dictionary<int, CarData> _entryListCars = new Dictionary<int, CarData>();

        private EntryListTracker()
        {

        }

        internal void Start()
        {
            BroadcastTracker.Instance.OnRealTimeCarUpdate += RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate += EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent += Broadcast_EventHandler;
        }

        internal void Stop()
        {
            BroadcastTracker.Instance.OnRealTimeCarUpdate -= RealTimeCarUpdate_EventHandler;
            BroadcastTracker.Instance.OnEntryListUpdate -= EntryListUpdate_EventHandler;
            BroadcastTracker.Instance.OnBroadcastEvent -= Broadcast_EventHandler;
            _entryListCars?.Clear();
        }

        private void Broadcast_EventHandler(object sender, BroadcastingEvent broadcastingEvent)
        {
            switch (broadcastingEvent.Type)
            {
                case BroadcastingCarEventType.LapCompleted:
                    {
                        if (broadcastingEvent.CarData == null)
                            break;

                        CarData carData;
                        if (_entryListCars.TryGetValue(broadcastingEvent.CarData.CarIndex, out carData))
                        {
                            carData.CarInfo = broadcastingEvent.CarData;
                        }
                        else
                        {
                            Debug.WriteLine($"BroadcastingCarEventType.LapCompleted car index: {broadcastingEvent.CarData.CarIndex} not found in entry list");
                            carData = new CarData();
                            carData.CarInfo = broadcastingEvent.CarData;
                            _entryListCars.Add(broadcastingEvent.CarData.CarIndex, carData);
                        }
                        break;
                    }
                case BroadcastingCarEventType.Accident:
                    {
                        if (broadcastingEvent.CarData == null)
                            break;

                        Debug.WriteLine($"Car: {broadcastingEvent.CarData.RaceNumber} had an accident");
                        break;
                    }
                default:
                    {
                        if (broadcastingEvent.CarData == null)
                            break;

                        Debug.WriteLine($"{broadcastingEvent.Type} - {broadcastingEvent.CarData}");
                        break;
                    }
            }
        }

        private void EntryListUpdate_EventHandler(object sender, CarInfo carInfo)
        {
            CarData carData;
            if (_entryListCars.TryGetValue(carInfo.CarIndex, out carData))
            {
                carData.CarInfo = carInfo;
            }
            else
            {
                carData = new CarData();
                carData.CarInfo = carInfo;
                _entryListCars.Add(carInfo.CarIndex, carData);
            }
        }

        private void RealTimeCarUpdate_EventHandler(object sender, RealtimeCarUpdate carUpdate)
        {
            CarData carData;
            if (_entryListCars.TryGetValue(carUpdate.CarIndex, out carData))
            {
                carData.RealtimeCarUpdate = carUpdate;
            }
            else
            {
                Debug.WriteLine($"RealTimeCarUpdate_EventHandler car index: {carUpdate.CarIndex} not found in entry list");
                carData = new CarData();
                carData.RealtimeCarUpdate = carUpdate;
                _entryListCars.Add(carUpdate.CarIndex, carData);
            }
        }
    }
}