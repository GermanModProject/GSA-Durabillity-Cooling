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

using GSA.Durability;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GSA.Cooling
{
    public struct CoolingPumpList
    {
        CoolingPumpModule Module { get; set; }
        float Priority { get; set; }
    }

    public class TemperatureManager
    {
        private static volatile TemperatureManager instance;
        private static object syncRoot = new Object();

        public static TemperatureManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TemperatureManager();
                    }
                }
                return instance;
            }
        }
        
        /// <summary>
        /// List of CoolingPumpModule Parts
        /// </summary>
        public List<CoolingPumpModule> CoolingPumpModuleList { get; private set; }

        /// <summary>
        /// List of CoolingRadiatorModule Parts
        /// </summary>
        public List<CoolingRadiatorModule> CoolingRadiatorModuleList { get; private set; }

        /// <summary>
        /// List of Parts to be cooled
        /// </summary>
        public List<Part> CoolingParts { get; private set; }

        public double CoolantTemperatureRadiatorsIn { get; set; }
        public double CoolantTemperatureRadiatorsOut { get; set; }

        public double CoolantTemperaturePartsIn { get; set; }
        public double CoolantTemperaturePartsOut { get; set; }

        public float CoolantAmount { get; private set; }
        public float CoolantFlowRate { get; private set; }

        private SortedDictionary<float, Part> priorityList;
        private Vessel vessel = null;
        private bool look = false;
        private int maxCoolingParts = 0;

        /// <summary>
        /// Get sortet List of Parts
        /// </summary>
        public SortedDictionary<float, Part> PriorityList
        {
            get
            {
                return this.priorityList;
            }
        }

        private TemperatureManager()
        {
            this.CoolingParts = new List<Part>();
            this.CoolingPumpModuleList = new List<CoolingPumpModule>();
            this.CoolingRadiatorModuleList = new List<CoolingRadiatorModule>();
            this.priorityList = new SortedDictionary<float, Part>();
            this.CoolantTemperaturePartsIn = -1;
            this.CoolantTemperaturePartsOut = -1;
            this.CoolantTemperatureRadiatorsIn = -1;
            this.CoolantTemperatureRadiatorsOut = -1;
        }

        public void UpdateFlowRate()
        {
            CoolantFlowRate = Simulator.CalculateCoolantFlowRate();
        }

        public void SetVessel(Vessel vessel)
        {
            GSA.Debug.Log("[GSA Cooling] TemperatureManager->SetVessel: " + vessel.name);
            this.vessel = vessel;
            FindPartsToBoCooled();
        }

        public void AddCoolingPumpModule(CoolingPumpModule coolingPumpModule)
        {
            GSA.Debug.Log("[GSA Cooling] TemperatureManager->AddCoolingPumpModule " + coolingPumpModule.part.name);
            CoolingPumpModuleList.Add(coolingPumpModule);
            CoolantAmount += 10;
            this.SetDefaultCollantTemperature(coolingPumpModule.part);
        }

        public void RemoveCoolingPumpModule(CoolingPumpModule coolingPumpModule)
        {
            if (CoolingPumpModuleList.Contains(coolingPumpModule))
            {
                GSA.Debug.Log("[GSA Cooling] TemperatureManager->RemoveCoolingPumpModule: " + coolingPumpModule.part.name);
                CoolingPumpModuleList.Remove(coolingPumpModule);
                CoolantAmount -= 10;
            }
        }

        public void AddCoolingRadiatorModule(CoolingRadiatorModule coolingRadiatorModule)
        {
            GSA.Debug.Log("[GSA Cooling] TemperatureManager->AddCoolingRadiatorModule " + coolingRadiatorModule.part.name);
            CoolingRadiatorModuleList.Add(coolingRadiatorModule);
            CoolantAmount += coolingRadiatorModule.coolantAmount;
            this.SetDefaultCollantTemperature(coolingRadiatorModule.part);
        }

        public void RemoveCoolingRadiatorModule(CoolingRadiatorModule coolingRadiatorModule)
        {
            if (CoolingRadiatorModuleList != null && CoolingRadiatorModuleList.Contains(coolingRadiatorModule))
            {
                GSA.Debug.Log("[GSA Cooling] TemperatureManager->RemoveCoolingRadiatorModule: " + coolingRadiatorModule.part.name);
                CoolingRadiatorModuleList.Remove(coolingRadiatorModule);
                CoolantAmount -= coolingRadiatorModule.coolantAmount;
            }
        }

        /// <summary>
        /// Get average coolant Temperatur
        /// </summary>
        /// <returns></returns>
        public double GetCoolantTemperature()
        {
            double temp = 0;
            if (this.CoolingPumpModuleList != null)
            {
                foreach (CoolingPumpModule cpm in this.CoolingPumpModuleList)
                {
                    temp += cpm.coolantTemperature;
                }

                temp = temp / this.CoolingPumpModuleList.Count;
            }
            return temp;
        }

        /// <summary>
        /// Find all Parts, want to be cooled
        /// </summary>
        public void FindPartsToBoCooled()
        {
            GSA.Debug.Log("[GSA Cooling] TemperatureManager->FindPartsToBoCooled Start");
            CoolingParts.Clear();
            if (vessel != null)
            {
                foreach (Part vpart in vessel.Parts)
                {
                    //GSA.Debug.Log("[GSA Cooling] TemperatureManager->FindPartsToBoCooled PART: " + vpart.name);
                    if (vpart.Modules.Contains("DurabilityModule"))
                    {
                        DurabilityModule durabilityModule = (DurabilityModule)vpart.Modules["DurabilityModule"];
                        if (durabilityModule.cooling)
                        {
                            this.CoolingParts.Add(vpart);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find all Parts, want to be cooled
        /// </summary>
        public IEnumerator UpdatePriority()
        {
            this.look = true;
            priorityList.Clear();
            int loops = CoolingParts.Count;
            //GSA.Debug.Log("[GSA Cooling] TemperatureManager->UpdatePriority CoolingParts.Count: " + loops.ToString());
            int currentLoop = 0;
            foreach (Part part in CoolingParts)
            {
                float priority = GetPartCoolingPrority(part);
                if (priority == 0)
                {
                    continue;
                }
                while (true)
                {
                    if (priorityList.ContainsKey(priority))
                    {
                        priority += 0.01f;
                    }
                    else
                    {
                        break;
                    }
                }
                if (part.Modules.Contains("DurabilityModule"))
                {
                    DurabilityModule durabilityModule = (DurabilityModule)part.Modules["DurabilityModule"];
                    durabilityModule.coolingPriority = priority;
                    //GSA.Debug.Log("[GSA Cooling] TemperatureManager->UpdatePriority HAS DurabilityModule(" + part.name + "): " + priority);
                }
                try
                {
                    priorityList.Add(priority, part);
                }
                catch (System.Exception e)
                {
                    GSA.Debug.LogError("[GSA Cooling] TemperatureManager->UpdatePriority _priorityList.Add: " + e.Message + "; " + priority.ToString("0.00"));
                }
                yield return null;

                currentLoop++;
                if (currentLoop == loops)
                {
                    this.look = false;
                }
            }
        }


        public float GetPartCoolingPrority(Part part, bool fromCache)
        {
            if (!fromCache)
            {
                return GetPartCoolingPrority(part);
            }
            else
            {
                if (priorityList.ContainsValue(part))
                {
                    foreach (KeyValuePair<float, Part> pinfo in priorityList)
                    {
                        if (pinfo.Value == part)
                        {
                            return pinfo.Key;
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Get the Priority, 0: has ideal temperature, 0 > to warm, 0 < to could
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public static float GetPartCoolingPrority(Part part)
        {
            if (part.Modules.Contains("DurabilityModule"))
            {
                DurabilityModule durabilityModule = (DurabilityModule)part.Modules["DurabilityModule"];
                FloatCurve idealTemperature = durabilityModule.idealTemp;
                float prority = 0;
                prority = idealTemperature.Evaluate(System.Convert.ToSingle(part.temperature));
                bool isLow = true;
                for (int i = -272; i < 4000; i++)
                {
                    if (idealTemperature.Evaluate(i) == 0)
                    {
                        isLow = false;
                        break;
                    }
                    if (i >= part.temperature)
                    {
                        break;
                    }
                }
                if(isLow && prority > 0)
                {
                    prority = -prority;
                }

                return prority;
            }
            return 0;
        }

        /// <summary>
        /// Get current maximal count of part can cooling
        /// </summary>
        /// <returns></returns>
        public int UpdateMaxCoolingPartCount()
        {
            int maxCoolingPartCount = 0;
            foreach (CoolingPumpModule coolingPumpModule in CoolingPumpModuleList)
            {
                if (coolingPumpModule.isEnabled)
                {
                    maxCoolingPartCount += coolingPumpModule.maxCoolingParts;
                }
            }
            maxCoolingParts = maxCoolingPartCount;
            return maxCoolingPartCount;
        }

        /// <summary>
        /// Perform the cooling
        /// </summary>
        public void Cooling()
        {
            if (this.CoolantFlowRate > 0)
            {
                int loop = 0;
                //CoolantTemperature = Simulator.CalculateCoolantHeatRadiation();
                this.CoolantTemperatureRadiatorsOut = Simulator.CalculateCoolantHeatRadiation();
                this.CoolantTemperaturePartsIn = this.CoolantTemperatureRadiatorsOut - (this.CoolantFlowRate) * 10;
                foreach (KeyValuePair<float, Part> coolingPart in priorityList.Reverse())
                {
                    this.CoolantTemperaturePartsOut = Simulator.CalculatePartCooling(coolingPart.Value, this.CoolantTemperaturePartsIn);
                    loop++;
                    if (loop >= maxCoolingParts)
                    {
                        break;
                    }
                }
                //this.CoolantTemperaturePartsOut = this.CoolantTemperaturePartsIn + 5;
                this.CoolantTemperatureRadiatorsIn = this.CoolantTemperaturePartsOut + (this.CoolantFlowRate) * 10;
            }
        }

        private void SetDefaultCollantTemperature(Part part)
        {
            if(this.CoolantTemperaturePartsIn < 0)
            {
                this.CoolantTemperaturePartsIn = part.temperature;
            }
            if (this.CoolantTemperaturePartsOut < 0)
            {
                this.CoolantTemperaturePartsOut = part.temperature;
            }
            if (this.CoolantTemperatureRadiatorsIn < 0)
            {
                this.CoolantTemperatureRadiatorsIn = part.temperature;
            }
            if (this.CoolantTemperatureRadiatorsOut < 0)
            {
                this.CoolantTemperatureRadiatorsOut = part.temperature;
            }
        }
    }
}
