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
    class Simulator
    {
        public static float CalculateCoolantTemp()
        {
            float currentTemp = TemperatureManager.Instance.CoolantTemp;
            float currentFlowRate = TemperatureManager.Instance.CoolantFlowRate;
            foreach(CoolingRadiatorModule radiator in TemperatureManager.Instance.CoolingRadiatorModuleList)
            {
                float timeToCool = radiator.coolantAmount / currentFlowRate;
                float thisOutTemp = currentTemp - (radiator.coolingFactor * timeToCool);
                radiator.coolantOutTemperature = thisOutTemp;
                currentTemp -= currentTemp / 2; 

            }
            return currentTemp;
        }

        /// <summary>
        /// Calculate max coolant flow rate
        /// </summary>
        /// <returns></returns>
        public static float CalculateCoolantFlowRate()
        {
            float currentFlowRate = 0;
            float currentPowerState = GetOptimalPowerState();
            foreach(CoolingPumpModule pump in TemperatureManager.Instance.CoolingPumpModuleList)
            {
                if(!pump.part.frozen)
                {
                    pump.PowerState = currentPowerState;
                    currentFlowRate += pump.maxFlowRate * currentPowerState;
                }
            }
            return currentFlowRate;
        }

        /// <summary>
        /// Calculate uptimal Powerstate (Current simple)
        /// </summary>
        /// <returns></returns>
        public static float GetOptimalPowerState()
        {
            float currentPowerState = 0;
            if (TemperatureManager.Instance.PriorityList.Count > 0 || TemperatureManager.Instance.CoolantTemp > 0)
            {
                currentPowerState = 1;
            }
            return currentPowerState;
        }

        public static void CalculatePartCooling(Part part)
        {
            float coolantTemp = TemperatureManager.Instance.CoolantTemp;
            float coolantFlowRate = TemperatureManager.Instance.CoolantFlowRate;

            float coolingRate = ((part.temperature - coolantTemp) * Time.deltaTime) / 60;
            TemperatureManager.Instance.CoolantTemp += coolingRate;
            part.temperature -= coolingRate;
        }
    }
}
