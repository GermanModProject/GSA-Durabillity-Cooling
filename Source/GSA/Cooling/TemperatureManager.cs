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
        static public TemperatureManager Instance = new TemperatureManager();

        public List<CoolingPumpModule> CoolingPumpModuleList { get; private set; }
        public List<CoolingRadiatorModule> CoolingRadiatorModuleList { get; private set; }

        public List<Part> CoolingParts { get; private set; }
        public float CoolantTemp { get; private set; }

        private SortedDictionary<float, Part> _priorityList;
        private Vessel _vessel = null;
        private bool _look = false;
        private int _maxCoolingParts = 0;
        


        private TemperatureManager()
        {
            CoolingParts = new List<Part>();
            _priorityList = new SortedDictionary<float, Part>();
        }

        public void Update()
        {
        }

        public void SetVessel(Vessel vessel)
        {
            GSA.Debug.Log("[GSA Cooling] TemperatureManager->SetVessel: " + vessel.name);
            _vessel = vessel;
            FindPartsToBoCooled();
        }

        public void AddCoolingPumpModule(CoolingPumpModule coolingPumpModule)
        {
            if (CoolingPumpModuleList == null)
            {
                CoolingPumpModuleList = new List<CoolingPumpModule>();
            }
            GSA.Debug.Log("[GSA Cooling] TemperatureManager->AddCoolingPumpModule " + coolingPumpModule.part.name);
            CoolingPumpModuleList.Add(coolingPumpModule);
        }

        public void RemoveCoolingPumpModule(CoolingPumpModule coolingPumpModule)
        {
            if (CoolingPumpModuleList.Contains(coolingPumpModule))
            {
                GSA.Debug.Log("[GSA Cooling] TemperatureManager->RemoveCoolingPumpModule: " + coolingPumpModule.part.name);
                CoolingPumpModuleList.Remove(coolingPumpModule);
            }
        }

        public void AddCoolingRadiatorModule(CoolingRadiatorModule coolingRadiatorModule)
        {
            if (CoolingRadiatorModuleList == null)
            {
                CoolingRadiatorModuleList = new List<CoolingRadiatorModule>();
            }
            GSA.Debug.Log("[GSA Cooling] TemperatureManager->AddCoolingRadiatorModule " + coolingRadiatorModule.part.name);
            CoolingRadiatorModuleList.Add(coolingRadiatorModule);
        }

        public void RemoveCoolingRadiatorModule(CoolingRadiatorModule coolingRadiatorModule)
        {
            if (CoolingRadiatorModuleList != null && CoolingRadiatorModuleList.Contains(coolingRadiatorModule))
            {
                GSA.Debug.Log("[GSA Cooling] TemperatureManager->RemoveCoolingRadiatorModule: " + coolingRadiatorModule.part.name);
                CoolingRadiatorModuleList.Remove(coolingRadiatorModule);
            }
        }

        /// <summary>
        /// Get average coolant Temperatur
        /// </summary>
        /// <returns></returns>
        public float GetCoolantTemperature()
        {
            float temp = 0;
            if (CoolingPumpModuleList != null)
            {
                foreach (CoolingPumpModule cpm in CoolingPumpModuleList)
                {
                    temp += cpm.coolantTemperature;
                }

                temp = temp / CoolingPumpModuleList.Count;
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
            if (_vessel != null)
            {
                foreach (Part vpart in _vessel.Parts)
                {
                    //GSA.Debug.Log("[GSA Cooling] TemperatureManager->FindPartsToBoCooled PART: " + vpart.name);
                    if (vpart.Modules.Contains("DurabilityModule"))
                    {
                        PartModule durabilityModule = vpart.Modules["DurabilityModule"];
                        BaseField field = null;
                        foreach (BaseField f in durabilityModule.Fields)
                        {
                            if (f.FieldInfo.Name == "cooling")
                            {
                                field = f;
                                break;
                            }
                        }
                        bool boCool = (bool)field.GetValue(durabilityModule);
                        //GSA.Debug.Log("[GSA Cooling] TemperatureManager->FindPartsToBoCooled boCool '" + vpart.name + "': " + boCool);
                        //GSA.Debug.Log(field);
                        if (boCool)
                        {
                            CoolingParts.Add(vpart);
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
            _look = true;
            _priorityList.Clear();
            int loops = CoolingParts.Count;
            GSA.Debug.Log("[GSA Cooling] TemperatureManager->UpdatePriority CoolingParts.Count: " + loops.ToString());
            int currentLoop = 0;
            foreach (Part part in CoolingParts)
            {
                float priority = GetPartCoolingPrority(part);
                while (true)
                {
                    if (_priorityList.ContainsKey(priority))
                    {
                        priority += 0.01f;
                    }
                    else
                    {
                        break;
                    }
                }
                BaseField priorityField = null;
                foreach (BaseField field in part.Fields)
                {
                    if (field.FieldInfo.Name == "coolingPriority")
                    {
                        priorityField = field;
                        priorityField.SetValue(priority, part);
                        break;
                    }
                }
                try
                {
                    _priorityList.Add(priority, part);
                }
                catch (System.Exception e)
                {
                    GSA.Debug.LogError("[GSA Cooling] TemperatureManager->UpdatePriority _priorityList.Add: " + e.Message + "; " + priority.ToString("0.00"));
                }
                yield return null;

                currentLoop++;
                if (currentLoop == loops)
                {
                    _look = false;
                }
            }
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
                PartModule durabilityModule = part.Modules["DurabilityModule"];
                BaseField idealTemp = null;
                foreach (BaseField f in durabilityModule.Fields)
                {
                    if (f.FieldInfo.Name == "idealTemp")
                    {
                        idealTemp = f;
                        break;
                    }
                }
                FloatCurve idealT = (FloatCurve)idealTemp.GetValue(durabilityModule);
                float prority = 0;
                prority = idealT.Evaluate(part.temperature);

                BaseField coolingPriority = null;
                foreach (BaseField f in durabilityModule.Fields)
                {
                    if (f.FieldInfo.Name == "coolingPriority")
                    {
                        coolingPriority = f;
                        coolingPriority.SetValue(prority, durabilityModule);
                        break;
                    }
                }
                return prority;
            }
            return 0;
        }

        /// <summary>
        /// Get current maximal count of part can cooling
        /// </summary>
        /// <returns></returns>
        public int updateMaxCoolingPartCount()
        {
            int maxCoolingPartCount = 0;
            foreach (CoolingPumpModule coolingPumpModule in CoolingPumpModuleList)
            {
                if (coolingPumpModule.isEnabled)
                {
                    maxCoolingPartCount += coolingPumpModule.maxCoolingParts;
                }
            }
            _maxCoolingParts = maxCoolingPartCount; 
            return maxCoolingPartCount;
        }

        /// <summary>
        /// Perform the cooling
        /// </summary>
        private void cooling()
        {
            int loop = 0;
            foreach (KeyValuePair<float, Part> coolingPart in _priorityList.Reverse())
            {               
                if(coolingPart.Key > 0)
                {
                    
                }
                else
                {
                    continue;
                }
                loop++;
                if (loop >= _maxCoolingParts)
                {
                    break;
                }
            }
        }
    }
}
