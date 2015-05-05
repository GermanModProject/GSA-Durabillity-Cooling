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

using UnityEngine;

namespace GSA.Cooling
{
    static class Simulator
    {
        static bool first = true;

        /// <summary>
        /// Calculate heat radiation
        /// </summary>
        /// <returns></returns>
        public static double CalculateCoolantHeatRadiation()
        {
            double currentTempIn = TemperatureManager.Instance.CoolantTemperatureRadiatorsIn;
            double currentTempOut = TemperatureManager.Instance.CoolantTemperatureRadiatorsOut;
            float currentFlowRate = TemperatureManager.Instance.CoolantFlowRate;

            foreach (CoolingRadiatorModule radiator in TemperatureManager.Instance.CoolingRadiatorModuleList)
            {
                float timeToCool = Time.deltaTime;
                if (currentFlowRate > 0)
                {
                    timeToCool = (radiator.coolantAmount / currentFlowRate) * Time.deltaTime;
                }

                double extenamDifference = radiator.part.externalTemperature - currentTempIn;
                double partDifference = radiator.part.temperature - currentTempIn;
                double radiationFactor = ((extenamDifference + partDifference) / 4) * radiator.coolingFactor;
                double radiationFactorTime = radiationFactor * timeToCool;

                if (radiationFactorTime < 0)
                {
                    currentTempOut = currentTempIn + radiationFactorTime;
                    if (currentTempOut < 0)
                    {
                        currentTempOut = 0;
                    }
                    currentTempIn = currentTempOut;
                    radiator.coolantInTemperature = currentTempIn.ToString("0.00");
                    radiator.coolantOutTemperature = currentTempOut.ToString("0.00");
                    radiator.coolantRadiant = radiationFactorTime;

                    if (currentFlowRate > 0)
                    {
                        if (first)
                        {
                            GSA.Debug.Log("[GSA Cooling] Simulator->CalculateCoolantHeatRadiation (radiator.coolantAmount / currentFlowRate) * Time.deltaTime: (" + radiator.coolantAmount + " / " + currentFlowRate + ") * " + Time.deltaTime);
                            GSA.Debug.Log("[GSA Cooling] Simulator->CalculateCoolantHeatRadiation timeToCool: " + timeToCool);
                            GSA.Debug.Log("[GSA Cooling] Simulator->CalculateCoolantHeatRadiation extenamDifference: " + extenamDifference);
                            GSA.Debug.Log("[GSA Cooling] Simulator->CalculateCoolantHeatRadiation partDifference: " + partDifference);
                            GSA.Debug.Log("[GSA Cooling] Simulator->CalculateCoolantHeatRadiation radiationFactor: " + radiationFactor);
                            GSA.Debug.Log("[GSA Cooling] Simulator->CalculateCoolantHeatRadiation radiationFactorTime: " + radiationFactorTime);
                            GSA.Debug.Log("[GSA Cooling] Simulator->CalculateCoolantHeatRadiation currentTempOut: " + currentTempOut);
                            first = false;
                        }
                    }
                }
                else
                {
                    radiator.coolantInTemperature = radiator.part.temperature.ToString("0.00");
                    radiator.coolantOutTemperature = radiator.part.temperature.ToString("0.00");
                    radiator.coolantRadiant = radiationFactorTime;
                }
            }
            return currentTempOut;
        }

        /// <summary>
        /// Calculate max coolant flow rate
        /// </summary>
        /// <returns></returns>
        public static float CalculateCoolantFlowRate()
        {
            float currentFlowRate = 0;
            float currentPowerState = GetOptimalPowerState();
            foreach (CoolingPumpModule pump in TemperatureManager.Instance.CoolingPumpModuleList)
            {
                if (!pump.part.frozen && pump.part.isActiveAndEnabled)
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
            if (TemperatureManager.Instance.PriorityList.Count > 0 || TemperatureManager.Instance.CoolantTemperatureRadiatorsIn > TemperatureManager.Instance.Vessel.externalTemperature)
            {
                currentPowerState = 1;
            }
            return currentPowerState;
        }

        public static double CalculatePartCooling(Part part, double coolantInTemperature)
        {
            if (part.temperature > coolantInTemperature)
            {
                double coolingRate = (part.temperature - coolantInTemperature) * Time.deltaTime;
                double coolingRateMass = coolingRate / (part.mass * 1000);
                double coolingRateFinal = coolingRateMass * TemperatureManager.Instance.CoolantFlowRate;
                part.temperature -= coolingRateFinal;

                coolantInTemperature += coolingRateFinal * (part.mass * 1000);
            }
            return coolantInTemperature;
        }
    }
}
