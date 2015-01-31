///////////////////////////////////////////////////////////////////////////////
//
//    Cooling a plugin for Kerbal Space Program from SQUAD
//    (https://www.kerbalspaceprogram.com/)
//    and part of GSA Mod
//    (http://www.kerbalspaceprogram.de)
//
//    Author: runner78
//    Copyright (c) 2015 runner78
//
//    This program, coding and graphics are provided under the following Creative Commons license.
//    Attribution-NonCommercial 3.0 Unported
//    https://creativecommons.org/licenses/by-nc/3.0/
//
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace GSA.Cooling
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TemperatureManagerAddon : MonoBehaviour
    {
        private Vessel _vessel = null;
        private bool _runOnce = true;

        public static TemperatureManagerAddon Instance { get; private set; }

        private float _updateFrequency = 5;
        private float _lastUpdate = 0;
        private bool _look = false;

        public void Start()
        {
            GSA.Debug.Log("[GSA Cooling] TemperatureManagerAddon->Start");
            Instance = this;
        }

        public void Update()
        {
            if (_runOnce && FlightGlobals.ActiveVessel != null)
            {
                GSA.Debug.Log("[GSA Cooling] TemperatureManagerAddon->Update RunOnce");
                _vessel = FlightGlobals.ActiveVessel;
                TemperatureManager.Instance.SetVessel(_vessel);
                //UpdatePriority();
                //GSA.Debug.Log("[GSA Cooling] TemperatureManagerAddon->Update RunOnce _lastUpdate" + _lastUpdate.ToString("0.00000"));
                _runOnce = false;
            }


            _lastUpdate += Time.deltaTime;
            if (_lastUpdate >= _updateFrequency && !_look && !_runOnce)
            {
                //GSA.Debug.Log("[GSA Cooling] TemperatureManagerAddon->Update updateFrequency");
                _lastUpdate = 0;
                UpdatePriority();
                TemperatureManager.Instance.updateMaxCoolingPartCount();
            }
        }

        public void UpdatePriority()
        {
            //GSA.Debug.Log("[GSA Cooling] TemperatureManagerAddon->UpdatePriority");
            StartCoroutine(TemperatureManager.Instance.UpdatePriority());
        }
    }
}
