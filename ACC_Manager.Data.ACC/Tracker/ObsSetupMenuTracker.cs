﻿using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace ACCManager.Data.ACC.Tracker
{
    internal class ObsSetupMenuTracker : IDisposable
    {
        public static ObsSetupMenuTracker _instance;
        public static ObsSetupMenuTracker Instance
        {
            get
            {
                if (_instance == null) _instance = new ObsSetupMenuTracker();
                return _instance;
            }
        }

        private readonly OBSWebsocket _obsWebSocket;
        private bool _isTracking = false;
        private readonly ACCSharedMemory _sharedMemory;

        private ObsSetupMenuTracker()
        {
            Debug.WriteLine("Started OBS setup menu tracker");
            _obsWebSocket = new OBSWebsocket();
            _obsWebSocket.Connected += ObsWebSocket_Connected;
            _obsWebSocket.Disconnected += ObsWebSocket_Disconnected;

            _sharedMemory = new ACCSharedMemory();

            Connect();
            StartTracker();
        }

        private void Connect()
        {
            try
            {
                _obsWebSocket.Connect("ws://192.168.0.130:4444", "password");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void StartTracker()
        {
            _isTracking = true;
            new Thread(() =>
            {
                while (_isTracking)
                {
                    Thread.Sleep(50);
                    var pageGraphics = _sharedMemory.ReadGraphicsPageFile();

                    Toggle(pageGraphics.IsSetupMenuVisible);
                }
            }).Start();
        }

        private void ObsWebSocket_Disconnected(object sender, EventArgs e)
        {
            CloseEventArgs ev = (CloseEventArgs)e;
            Debug.WriteLine($"Disconnected obs websocket {ev.Code}");
        }

        private void ObsWebSocket_Connected(object sender, EventArgs e)
        {
            Debug.WriteLine("Connected obs websocket");
            Debug.WriteLine($"Current scene: {_obsWebSocket.GetCurrentScene().Name}");
            Debug.WriteLine($"");
            Toggle(true);
        }

        public void Toggle(bool enable)
        {
            SourceInfo setupHiderSource = _obsWebSocket.GetSourcesList().Find(x => x.Name == "SetupHider");
            if (setupHiderSource != null)
            {
                _obsWebSocket.SetSourceRender(setupHiderSource.Name, enable);
            }
        }


        public void Dispose()
        {
            _isTracking = false;
            Debug.WriteLine("Disposed OBS setup menu tracker");
            _obsWebSocket.Disconnect();
            _instance = null;
        }
    }
}