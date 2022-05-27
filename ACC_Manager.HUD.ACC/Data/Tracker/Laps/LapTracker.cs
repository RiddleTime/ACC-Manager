﻿using ACCManager.Broadcast.Structs;
using ACCManager.Data.ACC.Tracker;
using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Data.Tracker.Laps
{

    /// <summary>
    /// All data except for the Index must be divided by 1000 to get the actual value (floating point precision is annoying)
    /// </summary>
    public class LapData
    {
        /// <summary>
        /// Lap Index
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// Lap Time
        /// </summary>
        public int Time { get; set; } = -1;
        public bool IsValid { get; set; } = true;
        public int Sector1 { get; set; } = -1;
        public int Sector2 { get; set; } = -1;
        public int Sector3 { get; set; } = -1;


        /// <summary>
        /// Fuel left at the end of the lap, divide by 1000...
        /// </summary>
        public int FuelUsage { get; set; } = -1;
    }

    internal class LapTracker
    {
        private static LapTracker _instance;
        public static LapTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new LapTracker();
                return _instance;
            }
        }

        private bool IsTracking = false;
        private ACCSharedMemory sharedMemory;
        private int CurrentSector = 0;

        internal List<LapData> Laps = new List<LapData>();
        internal LapData CurrentLap;

        public event EventHandler<LapData> LapFinished;

        private LapTracker()
        {
            sharedMemory = new ACCSharedMemory();
            CurrentLap = new LapData();

            if (!IsTracking)
                this.Start();

            BroadcastTracker.Instance.OnRealTimeCarUpdate += Instance_OnRealTimeCarUpdate;
        }

        private LapInfo _lastLapInfo;

        private void Instance_OnRealTimeCarUpdate(object sender, RealtimeCarUpdate e)
        {
            _lastLapInfo = e.LastLap;

            FinalizeCurrentLapData();
        }

        private void FinalizeCurrentLapData()
        {
            if (_lastLapInfo != null)
                if (_lastLapInfo.Splits != null)
                    if (_lastLapInfo.Splits.Count == 3)
                        if (_lastLapInfo.Splits[2].HasValue)
                        {
                            LapData lastData = Laps.Last();
                            if (_lastLapInfo.LaptimeMS == lastData.Time)
                                if (Laps[Laps.Count - 1].Sector3 != _lastLapInfo.Splits[2].Value)
                                {
                                    Laps[Laps.Count - 1].Sector3 = _lastLapInfo.Splits[2].Value;
                                    Laps[Laps.Count - 1].IsValid = !_lastLapInfo.IsInvalid;
                                    LapFinished?.Invoke(this, Laps[Laps.Count - 1]);
                                }
                        }
        }

        internal void Start()
        {
            if (IsTracking)
                return;

            IsTracking = true;
            new Thread(x =>
            {
                while (IsTracking)
                {
                    try
                    {
                        Thread.Sleep(1000 / 10);

                        var pageGraphics = sharedMemory.ReadGraphicsPageFile();

                        if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF)
                        {
                            Laps.Clear();
                            CurrentLap = new LapData() { Index = pageGraphics.CompletedLaps + 1 };
                        }

                        // collect sector times.
                        if (CurrentSector != pageGraphics.CurrentSectorIndex)
                        {
                            if (CurrentLap.Sector1 == -1 && CurrentSector != 0)
                            {
                                // simply don't collect, we're already into a lap and passed sector 1, can't properly calculate the sector times now.
                            }
                            else
                                switch (pageGraphics.CurrentSectorIndex)
                                {
                                    case 1: CurrentLap.Sector1 = pageGraphics.LastSectorTime; break;
                                    case 2: CurrentLap.Sector2 = pageGraphics.LastSectorTime - CurrentLap.Sector1; break;

                                        // this sector time is now finalized with the FinalizeCurrentLapData() function    
                                        //case 0: CurrentLap.Sector3 = pageGraphics.LastTimeMs - CurrentLap.Sector2 - CurrentLap.Sector1; break;
                                }

                            CurrentSector = pageGraphics.CurrentSectorIndex;
                        }

                        // finalize lap time data and add it to history.
                        if (CurrentLap.Index - 1 != pageGraphics.CompletedLaps && pageGraphics.LastTimeMs != int.MaxValue)
                        {
                            CurrentLap.Time = pageGraphics.LastTimeMs;
                            CurrentLap.FuelUsage = (int)(sharedMemory.ReadGraphicsPageFile().FuelXLap * 1000);

                            if (CurrentLap.Sector1 != -1)
                            {
                                lock (Laps)
                                    Laps.Add(CurrentLap);
                            }

                            CurrentLap = new LapData() { Index = pageGraphics.CompletedLaps + 1 };
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        LogWriter.WriteToLog(ex);
                    }
                }

                _instance = null;
                IsTracking = false;
            }).Start();


        }

        internal void Stop()
        {
            IsTracking = false;
        }
    }


}
