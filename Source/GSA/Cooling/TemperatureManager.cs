using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GSA.Cooling
{
    public struct CoolingPumpList
    {
        CoolingPumpModule Module {get; set;}
        float Priority { get; set; }
    }

    public class TemperatureManager
    {
        static public TemperatureManager Instance = new TemperatureManager();

        public List<CoolingPumpModule> CoolingPumpModule { get; private set; }
        public List<CoolingRadiatorModule> CoolingRadiatorModule { get; private set; }
        
        public List<Part> CoolingParts { get; private set; }

        private SortedDictionary<float, Part> _priorityList;
        private Vessel _vessel = null;

        private float _updateFrequency = 1000;
        private float _lastUpdate = 0;
        private bool _look = false;


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
            _vessel = vessel;
        }

        public void AddCoolingPumpPart(CoolingPumpModule coolingPumpModule)
        {
            CoolingPumpModule.Add(coolingPumpModule);            
        }

        public void RemoveCoolingPumpPart(CoolingPumpModule coolingPumpModule)
        {
            if (CoolingPumpModule != null && CoolingPumpModule.Contains(coolingPumpModule))
            {
                CoolingPumpModule.Remove(coolingPumpModule);                
            }            
        }

        public void AddCoolingRadiatorModule(CoolingRadiatorModule coolingRadiatorModule)
        {
            if (CoolingRadiatorModule == null)
            {
                CoolingRadiatorModule = new List<CoolingRadiatorModule>();
            }
            CoolingRadiatorModule.Add(coolingRadiatorModule);
        }

        public void RemoveCoolingRadiatorModule(CoolingRadiatorModule coolingRadiatorModule)
        {
            if (CoolingRadiatorModule != null && CoolingRadiatorModule.Contains(coolingRadiatorModule))
            {
                CoolingRadiatorModule.Remove(coolingRadiatorModule);
            }
        }

        /// <summary>
        /// Get average coolant Temperatur
        /// </summary>
        /// <returns></returns>
        public float GetCoolantTemperature()
        {
            float temp = 0;
            if (CoolingPumpModule != null)
            {
                foreach(CoolingPumpModule cpm in CoolingPumpModule)
                {
                    temp += cpm.coolantTemperature;
                }

                temp = temp / CoolingPumpModule.Count;
            }
            return temp;
        }

        /// <summary>
        /// Find all Parts, want to be cooled
        /// </summary>
        public void FindPartsToBoCooled()
        {
            CoolingParts.Clear();
            if (_vessel != null)
            {
                foreach (Part vpart in _vessel.Parts)
                {
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
                        if(boCool)
                        {
                            CoolingParts.Add(vpart);
                        }
                    }
                }
            }
            UpdatePriority();
            TemperatureManagerAddon.Instance.UpdatePriority();
        }

        /// <summary>
        /// Find all Parts, want to be cooled
        /// </summary>
        public IEnumerator UpdatePriority()
        {
            _look = true;
            _priorityList.Clear();
            int loops = CoolingParts.Count;
            int currentLoop = 0;
            foreach (Part part in CoolingParts)
            {
                float priority = GetPartCoolingPrority(part);
                if (priority > 0)
                {
                    while (true)
                    {
                        if (_priorityList.ContainsKey(priority))
                        {
                            priority += 0.0001f;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                _priorityList.Add(priority, part);
                yield return null;

                currentLoop++;
                if(currentLoop == loops)
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
                return prority;
            }
            return 0;
        }

        /// <summary>
        /// Perform the cooling
        /// </summary>
        private void cooling()
        {            
            foreach (KeyValuePair<float, Part> x in _priorityList.Reverse())
            {
                
            }
        }
    }
}
