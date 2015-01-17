using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GSA.Cooling
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TemperatureManagerAddon : MonoBehaviour
    {
        private Vessel _vessel = null;
        private bool _runOnce = true;

        public static TemperatureManagerAddon Instance { get; private set; }

        private float _updateFrequency = 1000;
        private float _lastUpdate = 0;
        private bool _look = false;

        public void Start()
        {
            Instance = this;
        }

        public void Update()
        {
            if (_runOnce && FlightGlobals.ActiveVessel != null)
            {
                _vessel = FlightGlobals.ActiveVessel;
                TemperatureManager.Instance.SetVessel(_vessel);
                _runOnce = false;
            }


            _lastUpdate += Time.deltaTime;
            if (_lastUpdate >= _updateFrequency && !_look)
            {
                _lastUpdate = 0;
                UpdatePriority();
            }
        }

        public void UpdatePriority()
        {
            StartCoroutine(TemperatureManager.Instance.UpdatePriority());
        }
    }
}
