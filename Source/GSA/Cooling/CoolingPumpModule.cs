﻿///////////////////////////////////////////////////////////////////////////////
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

using System;
using UnityEngine;

namespace GSA.Cooling
{
    /// <summary>
    /// Cooling Pump and cooleant
    /// </summary>
    public class CoolingPumpModule : PartModule
    {
        #region KSPFields

        /// <summary>
        /// part can cooling
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiName = "Max coolings Part")]
        public int maxCoolingParts = 10;

        /// <summary>
        /// coolant temperature
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiName = "Coolant Temp", guiUnits = "° C", guiFormat = "F0")]
        public float coolantTemperature = 0;

        /// <summary>
        /// coolant amount (auto calculadet)
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        public float coolantAmount = 0;

        /// <summary>
        /// Max coolant flow rate
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        public float maxFlowRate = 2f;

        /// <summary>
        /// Max maxElectric Charge rate
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        public float maxElectricChargeRate = .5f;

        #endregion //KSPFields

        private float _currentPowerstate = 0;
        /// <summary>
        /// Current Power state (procent)
        /// </summary>
        public float PowerState
        {
            get { return _currentPowerstate; }
            set
            {
                if (value > 1)
                    _currentPowerstate = 1;
                if (value < 0)
                    _currentPowerstate = 0;
                _currentPowerstate = value;
            }
        }

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor)
            {
                try
                {
                    TemperatureManager.Instance.AddCoolingPumpModule(this);
                }
                catch (Exception ex)
                {
                    GSA.Debug.LogError("[GSA Cooling] CoolingPumpModule->OnStart: AddCoolingPumpModule: Message: " + ex.Message);
                    GSA.Debug.LogError("[GSA Cooling] CoolingPumpModule->OnStart: AddCoolingPumpModule: StackTrace: " + ex.StackTrace);
                }
            }
        }

        public override void OnUpdate()
        {
            coolantTemperature = TemperatureManager.Instance.CoolantTemp;
        }

        public override void OnInactive()
        {
            TemperatureManager.Instance.RemoveCoolingPumpModule(this);
        }

        public override void OnActive()
        {
            try
            {
                TemperatureManager.Instance.AddCoolingPumpModule(this);
            }
            catch (Exception ex)
            {
                GSA.Debug.LogError("[GSA Cooling] CoolingPumpModule->OnActive: AddCoolingPumpPart: Message: " + ex.Message);
                GSA.Debug.LogError("[GSA Cooling] CoolingPumpModule->OnActive: AddCoolingPumpPart: StackTrace: " + ex.StackTrace);
            }
        }

        public void ReInitCooling()
        {
            TemperatureManager.Instance.FindPartsToBoCooled();
        }

    }
}
