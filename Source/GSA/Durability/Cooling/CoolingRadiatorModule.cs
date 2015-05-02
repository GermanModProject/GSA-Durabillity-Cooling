﻿///////////////////////////////////////////////////////////////////////////////
//
//    Durability a plugin for Kerbal Space Program from SQUAD
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
    public class CoolingRadiatorModule : PartModule
    {
        #region KSPFields

        /// <summary>
        /// cooling factor (1L per Minute)
        /// </summary>
        [KSPField(isPersistant = false, guiActive = false)]
        public float coolingFactor = .5f;

        /// <summary>
        /// Coolant Amount
        /// </summary>
        [KSPField(isPersistant = false, guiActive = false)]
        public float coolantAmount = 50f;

        /// <summary>
        /// Cooleant in Temperature
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiName = "Coolant In", guiUnits = "° C", guiFormat = "0.000")]
        public string coolantInTemperature;

        /// <summary>
        /// Cooleant out Temperature
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiName = "Coolant Out", guiUnits = "° C", guiFormat = "0.000")]
        public string coolantOutTemperature;

        /// <summary>
        /// Coolant Amount
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Radiant", guiUnits = "° C", guiFormat = "F0")]
        public double coolantRadiant = 50f;

        #endregion //KSPFields

        public override void OnStart(StartState state)
        {
            if (state != StartState.Editor)
            {
                try
                {
                    TemperatureManager.Instance.AddCoolingRadiatorModule(this);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[GSA Cooling] CoolingRadiatorModule->OnStart: AddCoolingRadiatorModule: Message: " + ex.Message);
                    Debug.LogError("[GSA Cooling] CoolingRadiatorModule->OnStart: AddCoolingRadiatorModule: StackTrace: " + ex.StackTrace);
                }
            }
        }

        public override void OnUpdate()
        {
            this.coolantInTemperature = (TemperatureManager.Instance.CoolantTemperatureRadiatorsIn - 273.15).ToString("0.00");
            this.coolantOutTemperature = (TemperatureManager.Instance.CoolantTemperatureRadiatorsOut - 273.15).ToString("0.00");
        }

        public override void OnInactive()
        {
            TemperatureManager.Instance.RemoveCoolingRadiatorModule(this);
        }

        public override void OnActive()
        {
            TemperatureManager.Instance.AddCoolingRadiatorModule(this);
        }
    }
}
