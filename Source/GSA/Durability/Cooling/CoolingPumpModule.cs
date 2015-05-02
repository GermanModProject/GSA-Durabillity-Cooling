///////////////////////////////////////////////////////////////////////////////
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
    public class CoolingPumpModule : PartModule
    {
        #region KSPFields

        /// <summary>
        /// part can cooling
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiName = "Max coolings Part")]
        public int maxCoolingParts = 10;

        /// <summary>
        /// coolant temperature (Kelvin)
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false)]
        public double coolantTemperature = 0;

        /// <summary>
        /// coolant temperature (display in Celcius)
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Coolant Temp", guiUnits = "° C")]
        public string coolantTemperatureCelcius;

        /// <summary>
        /// coolant temperature Radiators In
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Radiators In", guiUnits = "° C")]
        public string CoolantTemperatureRadiatorsIn;

        /// <summary>
        /// coolant temperature Radiators Out
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Radiators Out", guiUnits = "° C")]

        public string CoolantTemperatureRadiatorsOut;
        /// <summary>
        /// coolant temperature Parts in
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Parts In", guiUnits = "° C")]
        public string CoolantTemperaturePartsIn;

        /// <summary>
        /// coolant temperature Parts Out
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Parts Out", guiUnits = "° C")]
        public string CoolantTemperaturePartsOut;

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

        private float currentPowerState = 0;

        /// <summary>
        /// Current Power state (procent)
        /// </summary>
        public float PowerState
        {
            get { return this.currentPowerState; }
            set
            {
                if (value > 1)
                    this.currentPowerState = 1;
                if (value < 0)
                    this.currentPowerState = 0;
                this.currentPowerState = value;
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
            //this.coolantTemperature = TemperatureManager.Instance.CoolantTemperature;
            this.coolantTemperatureCelcius = (this.coolantTemperature - 273.15d).ToString("0.00");
            this.CoolantTemperaturePartsIn = (TemperatureManager.Instance.CoolantTemperaturePartsIn - 273.15d).ToString("0.00");
            this.CoolantTemperaturePartsOut = (TemperatureManager.Instance.CoolantTemperaturePartsOut - 273.15d).ToString("0.00");
            this.CoolantTemperatureRadiatorsIn = (TemperatureManager.Instance.CoolantTemperatureRadiatorsIn - 273.15d).ToString("0.00");
            this.CoolantTemperatureRadiatorsOut = (TemperatureManager.Instance.CoolantTemperatureRadiatorsOut - 273.15d).ToString("0.00");
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
