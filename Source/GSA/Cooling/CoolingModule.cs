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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSA.Cooling
{
    /// <summary>
    /// Cooling Pump and cooleant
    /// </summary>
    public class CoolingModule : PartModule
    {
        /// <summary>
        /// Display Temperature
        /// </summary>
        [KSPField(isPersistant = false, guiActive = false, guiName = "Temperature", guiUnits = "° K", guiFormat = "F")]
        public string displayTemperature;

        /// <summary>
        /// Display Temperature in c°
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Temperature", guiUnits = "° C", guiFormat = "F")]
        public string displayTemperatureCelcius;

        public override void OnAwake()
        {
            this.moduleName = "";
        }
    }
}
