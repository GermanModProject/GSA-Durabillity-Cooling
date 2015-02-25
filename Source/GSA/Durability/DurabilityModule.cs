///////////////////////////////////////////////////////////////////////////////
//
//    Durability a plugin for Kerbal Space Program from SQUAD
//    (https://www.kerbalspaceprogram.com/)
//    and part of GSA Mod
//    (http://www.kerbalspaceprogram.de)
//
//    Author: runner78
//    Copyright (c) 2014-2015 runner78
//
//    This program, coding and graphics are provided under the following Creative Commons license.
//    Attribution-NonCommercial 3.0 Unported
//    https://creativecommons.org/licenses/by-nc/3.0/
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSA.Durability
{
    public class DurabilityModule : PartModule, IPartCostModifier
    {
        UIPartActionWindow _myWindow = null;
        UIPartActionWindow myWindow
        {
            get
            {
                if (_myWindow == null)
                {
                    foreach (UIPartActionWindow window in FindObjectsOfType(typeof(UIPartActionWindow)))
                    {
                        if (window.part == part) _myWindow = window;
                    }
                }
                return _myWindow;
            }
        }

        #region KSPFields

        /// <summary>
        /// Display Damage
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "[D]Damage")]
        public string displayDamage;

        /// <summary>
        /// Display Pressure
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "[D]Pressure")]
        public string displayPressure;

        /// <summary>
        /// Display Engine
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "[D]Engine")]
        public string displayEngine;

        /// <summary>
        /// Display GeeForce
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "[D]G")]
        public string displayGeeForce;

        /// <summary>
        /// Display TempM
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "[D]Temp")]
        public string displayTempM;

        /// <summary>
        /// Display ReactionWheel
        /// </summary>
        [KSPField(isPersistant = false, guiActive = false, guiName = "[D]RWheel")]
        public string displayReactionWheel;

        /// <summary>
        /// Display Experiment
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "[D]Expe.")]
        public string displayExp;

        /// <summary>
        /// Display Sun
        /// </summary>
        [KSPField(isPersistant = false, guiActive = false, guiName = "[D]Rad")]
        public string displaySun;

        /// <summary>
        /// Display Temperature
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Temperature", guiUnits = "° C", guiFormat = "F0")]
        public string displayTemperature;

        /// <summary>
        /// GForece Display Debug
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Expiry ", guiUnits = "", guiFormat = "F0")]
        public string displayTime;

        /// <summary>
        /// Cooling Property
        /// </summary>
        [KSPField(isPersistant = false, guiActive = true, guiName = "Cooling Priority", guiUnits = "", guiFormat = "F0")]
        public float coolingPriority = -10f;

        /// <summary>
        /// Quality from Part 0.0f - 1.0f; Default: 0.5f 
        /// </summary>
        [KSPField(isPersistant = true)]
        public double quality = 0.5f;

        /// <summary>
        /// Minimum Durability
        /// </summary>
        [KSPField(isPersistant = true)]
        public double minDurability = 150f;

        /// <summary>
        /// Part explode, if durability = 0
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool canExplode = false;

        /// <summary>
        /// Can repair the Part on EVA
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool canRepair = true;

        /// <summary>
        /// Reduce Quality Multiplicator
        /// </summary>
        [KSPField(isPersistant = false)]
        public float repairQualityReducer = 0.012f;

        /// <summary>
        /// Max count of remairs (-1 endless)
        /// </summary>
        [KSPField(isPersistant = true)]
        public int maxRepair = -1;

        /// <summary>
        /// Damagerate Multiplicator from Part
        /// </summary>
        [Obsolete("Use basicWear")]
        [KSPField(isPersistant = false)]
        public float damageRate = 0.018f;

        /// <summary>
        /// Damagerate Multiplicator from Part
        /// </summary>
        [KSPField(isPersistant = false)]
        public FloatCurve basicWear = new FloatCurve();

        /// <summary>
        /// Damagerate Multiplicator from Engines
        /// </summary>
        [KSPField(isPersistant = false)]
        public float engineWear = 0;

        /// <summary>
        /// Ideal Temperature
        /// </summary>
        [KSPField(isPersistant = false)]
        public FloatCurve idealTemp = new FloatCurve();

        /// <summary>
        /// Ideal Pressure
        /// </summary>
        [KSPField(isPersistant = false)]
        public FloatCurve idealPressure = new FloatCurve();

        /// <summary>
        /// Part explode is higher and durability is 0 (ignore canExplode)
        /// </summary>
        [KSPField(isPersistant = false)]
        public float maxPressure = 20;

        /// <summary>
        /// Radiatio absorption
        /// </summary>
        [KSPField(isPersistant = false)]
        public float radiationAbsorption = 0;

        /// <summary>
        /// Last reduce
        /// </summary>
        [KSPField(isPersistant = true, guiActive = false, guiName = "lastReduceRange ")]
        public double lastReduceRange = 0;

        /// <summary>
        /// Part will cooling
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiName = "Cooling status ")]
        public bool cooling = true;

        #endregion //KSPFields

        #region Private Fields

        bool isInit = false;
        bool isFirst = true;
        int countUpdates = 0;
        double displayDurability;
        double lastUpdateTime = 0;
        bool expDeployed = false;
        float initCost;
        float currentWear;
        double finalReduce = 0;
        StartState state;
        ModuleCommand command = null;
        ModuleReactionWheel reactionWheel = null;
        ModuleEngines engine = null;
        CelestialBody sun = null;
        ModuleScienceExperiment scienceExperiment = null;
        PartModule deadlyReentry = null;

        #endregion //Private Fields

        #region KSPEvent

        /// <summary>
        /// Repair Damage
        /// </summary>
        [KSPEvent(guiName = "No Damage (Durability)", guiActiveUnfocused = true, externalToEVAOnly = true, guiActive = false, unfocusedRange = 4f)]
        public void RepairDamage()
        {
            if (this.Events == null || !this.canRepair || this.maxRepair == 0)
                return;
            if (this.part.Resources.Contains("Durability"))
            {
                double difference = this.part.Resources["Durability"].maxAmount - this.part.Resources["Durability"].amount;
                GSA.Durability.Debug.Log("GSA Durability: [RepairDamage]: Start Repair (" + difference.ToString("0.0000") + ")");
                if (difference > 0)
                {
                    this.part.Resources["Durability"].amount = this.part.Resources["Durability"].maxAmount;
                    double differenceP = difference / this.part.Resources["Durability"].maxAmount;
                    GSA.Durability.Debug.Log("GSA Durability: [RepairDamage]: Repair differenceP (" + differenceP.ToString("0.0000") + ")");
                    this.quality -= this.repairQualityReducer * differenceP;
                    if (this.maxRepair > 0)
                    {
                        this.maxRepair--;
                    }
                    if (this.quality < 0.01d)
                    {
                        this.quality = 0.01d;
                    }
                    if (this.part.Resources.Contains("Quality"))
                    {
                        this.part.Resources["Quality"].amount = quality * 100;
                    }
                    this.currentWear = this.basicWear.Evaluate((float)this.quality);
                }
            }
            else
            {
                GSA.Durability.Debug.Log("GSA Durability: [RepairDamage]: Resource Durability not Found");
            }
            setEventLabel();
            if (this.myWindow != null)
                this.myWindow.displayDirty = true;
        }

        /// <summary>
        /// Toggle Cooling
        /// </summary>
        [KSPEvent(guiName = "Cooling", guiActive = true)]
        public void ToggleCooling()
        {
            this.cooling = !this.cooling;
            foreach (Part vpart in vessel.Parts)
            {
                if (vpart.Modules.Contains("CoolingPumpModule"))
                {
                    GSA.Durability.Debug.Log("[GSA Durability] DurabilityModule->ToggleCooling: found CoolingPumpModule");
                    PartModule coolingPumpModule = vpart.Modules["CoolingPumpModule"];
                    coolingPumpModule.GetType().GetMethod("ReInitCooling").Invoke(coolingPumpModule, null);
                    break;
                }
            }
        }

        [KSPEvent(guiName = "Add Temp (Debug)", guiActive = true)]
        public void AddTemp()
        {
            part.temperature += 10f; 
        }

        #endregion //KSPEvent

        #region Public override methods

        public override void OnAwake()
        {
            base.OnAwake();
            try
            {
                if (this.part && this.part.Modules != null)
                {
                    if (this.part.Modules.Contains("ModuleAeroReentry"))
                    {
                        this.deadlyReentry = this.part.Modules["ModuleAeroReentry"];
                    }
                }
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [OnAwake]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [OnAwake]: StackTrace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Start
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            GSA.Durability.Debug.Log("GSA Durability: [OnStart][" + state.ToString() + "]: " + this.name);
            this.state = state;

            try
            {
                AvailablePart currentPartInfo = PartLoader.getPartInfoByName(this.part.name.Replace("(Clone)", ""));
                this.initCost = currentPartInfo.cost;
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _initCost: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _initCost: StackTrace: " + ex.StackTrace);
            }

            if (state == StartState.Editor)
            {
                return;
            }

            if (this.part.Resources.Contains("Quality"))
            {
                this.quality = (this.part.Resources["Quality"].amount / (this.part.Resources["Quality"].maxAmount / 100)) / 100;
            }

            if (this.basicWear.findCurveMinMaxInterations == 0)
            {
                this.basicWear = new FloatCurve();
                this.basicWear.Add(0.1f, 0.69f);
                this.basicWear.Add(0.5f, 0.000181f);
                this.basicWear.Add(1f, 0.00001f);
            }

            this.currentWear = basicWear.Evaluate((float)quality);
            this.lastUpdateTime = vessel.missionTime;
            this.sun = Planetarium.fetch.Sun;
            //gameObject.AddComponent(typeof(LineRenderer));               

            if (this.part.Modules.Contains("ModuleCommand"))
            {
                try
                {
                    this.command = (ModuleCommand)this.part.Modules["ModuleCommand"];
                }
                catch (Exception ex)
                {
                    GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _command: Message: " + ex.Message);
                    GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _command: StackTrace: " + ex.StackTrace);
                }
            }

            if (this.part.Modules.Contains("ModuleReactionWheel"))
            {
                try
                {
                    this.reactionWheel = (ModuleReactionWheel)this.part.Modules["ModuleReactionWheel"];
                }
                catch (Exception ex)
                {
                    GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _reactionWheel: Message: " + ex.Message);
                    GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _reactionWheel: StackTrace: " + ex.StackTrace);
                }
            }

            if (this.part.Modules.Contains("ModuleEngines"))
            {
                GSA.Durability.Debug.Log("GSA Durability: [OnStart]: set _engine ");
                try
                {
                    this.engine = (ModuleEngines)this.part.Modules["ModuleEngines"];
                }
                catch (Exception ex)
                {
                    GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _engine: Message: " + ex.Message);
                    GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _engine: StackTrace: " + ex.StackTrace);
                }
            }
            if (this.part.Modules.Contains("ModuleScienceExperiment"))
            {
                GSA.Durability.Debug.Log("GSA Durability: [OnStart]: set _scienceExperiment ");
                try
                {
                    this.scienceExperiment = (ModuleScienceExperiment)this.part.Modules["ModuleScienceExperiment"];
                }
                catch (Exception ex)
                {
                    GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _scienceExperiment: Message: " + ex.Message);
                    GSA.Durability.Debug.LogError("GSA Durability: [OnStart] set _scienceExperiment: StackTrace: " + ex.StackTrace);
                }
            }
            CheckStatus();
            setEventLabel();
        }

        public override void OnUpdate()
        {
            if (this.isFirst)
            {
                this.isFirst = false;
                GSA.Durability.Debug.Log("GSA Durability: [OnUpdate]: Start First Update");
                GSA.Durability.Debug.Log("GSA Durability: [OnUpdate]: vessel.missionTime: " + vessel.missionTime.ToString());
                //Recalculate Durability
                try
                {
                    if (this.vessel.missionTime > 0 && this.lastUpdateTime > 0)
                    {
                        double reReduce = (this.vessel.missionTime - this.lastUpdateTime) * this.lastReduceRange;
                        GSA.Durability.Debug.Log("GSA Durability: [OnUpdate]: recalculate Duability: " + reReduce.ToString("F0"));
                        if (this.part.Resources.Contains("Durability") && reReduce > 0)
                        {
                            this.part.Resources["Durability"].amount -= reReduce;
                        }
                    }
                    this.isInit = true;
                }
                catch (Exception ex)
                {
                    GSA.Durability.Debug.LogError("GSA Durability: [OnUpdate] Recalculate: Message: " + ex.Message);
                    GSA.Durability.Debug.LogError("GSA Durability: [OnUpdate] Recalculate: StackTrace: " + ex.StackTrace);
                }
            }

            if (this.state != StartState.Editor)
            {
                CheckStatus();
                setEventLabel();
            }
            this.countUpdates++;
        }

        public void FixedUpdate()
        {
            if (this.state != StartState.Editor)
            {
                ReduceDurability();
            }
            else
            {
                if (this.part.Resources.Contains("Quality"))
                {
                    this.quality = (part.Resources["Quality"].amount / (this.part.Resources["Quality"].maxAmount / 100)) / 100;
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            try
            {
                GSA.Durability.Debug.Log(node);
                if (node.HasValue("lastUpdateTime"))
                {
                    double.TryParse(node.GetValue("lastUpdateTime"), out this.lastUpdateTime);
                }
                if (node.HasValue("lastReduceRange"))
                {
                    double.TryParse(node.GetValue("lastReduceRange"), out this.lastReduceRange);
                }

                GSA.Durability.Debug.Log("GSA Durability: [OnLoad]: vessel.missionTime: " + this.vessel.missionTime.ToString());
                GSA.Durability.Debug.Log("GSA Durability: [OnLoad]: lastReduceRange: " + this.lastReduceRange.ToString());
                GSA.Durability.Debug.Log("GSA Durability: [OnLoad]: lastUpdateTime: " + this.lastUpdateTime.ToString());
            }
            catch { }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (this.part.Resources.Contains("Durability"))
            {
                float baseValue = part.crashTolerance / 100;
                if (collision.relativeVelocity.magnitude > baseValue * 25 && collision.relativeVelocity.magnitude < part.crashTolerance)
                {
                    double baseDamage = part.Resources["Durability"].maxAmount / 1000;
                    double reduce = 0;
                    GSA.Durability.Debug.Log("GSA Durability: [OnCollisionEnter]: baseDamage: " + baseDamage.ToString("0.0000"));
                    if (this.part.Modules.Contains("ModuleLandingGear"))
                    {
                        baseDamage /= 3;
                        GSA.Durability.Debug.Log("GSA Durability: [OnCollisionEnter]: ModuleLandingGear baseDamage: " + baseDamage.ToString("0.0000"));
                    }
                    if (this.part.Modules.Contains("ModuleDockingNode"))
                    {
                        baseDamage /= 2;
                        GSA.Durability.Debug.Log("GSA Durability: [ModuleDockingNode]: ModuleLandingGear baseDamage: " + baseDamage.ToString("0.0000"));
                    }

                    if (collision.relativeVelocity.magnitude > baseValue * 90)
                    {
                        reduce = (collision.relativeVelocity.magnitude / baseValue) * baseDamage;
                        GSA.Durability.Debug.Log("GSA Durability: [ModuleDockingNode]: reduce over 90: " + reduce.ToString("0.0000"));
                    }
                    else
                    {
                        reduce = ((collision.relativeVelocity.magnitude / 2) / baseValue) * baseDamage;
                        GSA.Durability.Debug.Log("GSA Durability: [ModuleDockingNode]: reduce under 90: " + reduce.ToString("0.0000"));
                    }
                    this.part.Resources["Durability"].amount -= reduce;
                }
            }
        }

        public float GetModuleCost(float cost)
        {
            return CalculateCost();
        }

        #endregion //Public override methods

        #region Private methods

        /// <summary>
        /// Calculate ne Cost
        /// </summary>
        /// <returns></returns>
        private float CalculateCost()
        {
            double newCost = 0;
            if (this.quality == 0.5)
            {
                newCost = (double)this.initCost;
            }
            else if (quality > 0.5)
            {
                newCost = (double)this.initCost * Math.Pow((double)this.initCost, (quality - .5d));
            }
            else if (quality < 0.5)
            {
                newCost = (double)this.initCost / Math.Pow((double)this.initCost, (.5d - this.quality));
            }
            newCost -= (double)this.initCost;
            return (float)newCost;
        }

        /// <summary>
        /// Check durability status
        /// </summary>
        private void CheckStatus()
        {
            this.displayDurability = GetDurabilityPercent();
            this.displayTemperature = this.part.temperature.ToString("0.00");
            try
            {
                if (this.part.Resources.Contains("Durability"))
                {
                    if (this.part.Resources["Durability"].amount <= 0 && canExplode)
                    {
                        this.part.explode();
                        GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: explode[canExplode] " + part.name);
                    }
                    else
                    {
                        if (this.part.Resources["Durability"].amount <= minDurability && !this.part.frozen)
                        {
                            if (this.reactionWheel != null)
                            {
                                this.reactionWheel.wheelState = ModuleReactionWheel.WheelState.Broken;
                            }

                            if (this.engine != null)
                            {
                                GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: _engine is not Null:");
                                if (!this.engine.engineShutdown)
                                {
                                    this.engine.Events.Send(engine.Events["Shutdown"].id);
                                    GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: engineShutdown = True");
                                }
                            }
                            GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: _engine:" + this.engine.ToString());
                            GSA.Durability.Debug.Log(this.engine);
                            foreach (PartModule dModules in part.Modules)
                            {
                                if (dModules.moduleName != "DurabilityModule")
                                {
                                    dModules.isEnabled = false;
                                    GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: isEnabled = False:  " + dModules.moduleName);
                                }
                            }
                            this.part.freeze();
                            GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: freeze Part " + this.part.name);
                        }
                        else if (this.part.Resources["Durability"].amount > minDurability && this.part.frozen)
                        {
                            this.part.unfreeze();
                            if (reactionWheel != null)
                            {
                                this.reactionWheel.wheelState = ModuleReactionWheel.WheelState.Active;
                            }
                            foreach (PartModule aModules in this.part.Modules)
                            {
                                if (aModules.moduleName != "DurabilityModule")
                                {
                                    aModules.isEnabled = true;
                                    GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: isEnabled = True:  " + aModules.moduleName);
                                }
                            }
                            GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: unfreeze Part " + this.part.name);
                        }
                        if (part.Resources["Durability"].amount <= 0)
                        {
                            if (this.vessel.staticPressure >= this.maxPressure)
                            {
                                this.part.explode();
                                GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: explode[Pressure] " + this.part.name);
                            }
                            if (this.engine != null && this.quality == 0.01d)
                            {
                                this.part.explode();
                                GSA.Durability.Debug.Log("GSA Durability: [checkStatus]: explode[engine] " + this.part.name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [checkStatus]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [checkStatus]: StackTrace: " + ex.StackTrace);
            }
        }

        private void setEventLabel()
        {
            if (this.Events == null)
                return;
            try
            {
                //Repair
                if (this.displayDurability < 99 && this.canRepair && this.quality > 0.01 && (this.maxRepair > 0 || this.maxRepair == -1))
                {
                    this.Events["RepairDamage"].guiName = "Repair";
                }
                else if ((!this.canRepair || this.maxRepair == 0) && this.quality > 0.01)
                {
                    this.Events["RepairDamage"].guiName = "Can not Repair";
                }
                else if (this.quality <= 0.01)
                {
                    quality = 0.01d;
                    canRepair = false;
                    this.Events["RepairDamage"].guiName = "Quality to low! Can not Repair";
                }
                else
                {
                    this.Events["RepairDamage"].guiName = "No Damage";
                }

                //Cooling
                if (this.cooling)
                {
                    this.Events["ToggleCooling"].guiName = "Cooling On";
                }
                else
                {
                    this.Events["ToggleCooling"].guiName = "Cooling Off";
                }
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [setEventLabel]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [setEventLabel]: StackTrace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Reduce durability
        /// </summary>
        private void ReduceDurability()
        {
            this.finalReduce = 0;
            try
            {
                if (this.part.Resources.Contains("Durability"))
                {
                    if (this.part.Resources["Durability"].amount > 0)
                    {
                        // Default
                        this.finalReduce = currentWear;
                        double additionalReduce = 0;

                        //Temperature
                        additionalReduce += GetReduceTemperature();

                        //GeeForce
                        additionalReduce += GetReduceGeeForce();

                        //Pressure
                        additionalReduce += GetReducePressure();

                        //Engine
                        additionalReduce += GetReduceEngine();

                        //ScienceExperiment ModuleScienceExperiment (Not Multiply, fixed Damage)
                        GetReduceExperiment();

                        //Reaction Wheels
                        /*if (_reactionWheel != null)
                        {
                            displayReactionWheel = "YT" + _reactionWheel.YawTorque.ToString("0.000")
                                + "RT" + _reactionWheel.RollTorque.ToString("0.000")
                                + "PT" + _reactionWheel.PitchTorque.ToString("0.000");
                        }*/

                        //radiation
                        //additionalReduce += getReduceRadiation(reduce);

                        this.finalReduce += additionalReduce;
                        this.displayDamage = this.finalReduce.ToString("0.000000");

                        //DeltaTime
                        this.finalReduce *= TimeWarp.fixedDeltaTime;
                        this.displayDamage += " B: " + this.currentWear.ToString("0.000000");

                        this.displayTime = GetFormatedExpirationDate();

                        if (this.finalReduce > 0.0)
                        {
                            if (this.isInit && this.vessel.missionTime > 0.01)
                            {
                                this.lastUpdateTime = this.vessel.missionTime;
                                this.lastReduceRange = this.finalReduce * (1 / TimeWarp.fixedDeltaTime);
                            }
                        }
                    }
                    else if (this.part.Resources["Durability"].amount < 0)
                    {
                        this.part.Resources["Durability"].amount = 0;
                    }

                    this.part.Resources["Durability"].amount -= this.finalReduce;
                }
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [reduceDurability]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [reduceDurability]: StackTrace: " + ex.StackTrace);
            }
        }


        /// <summary>
        /// Get the current Durability as percent
        /// </summary>
        /// <returns></returns>
        private double GetDurabilityPercent()
        {
            double percent = 1.0d;
            if (this.part.Resources.Contains("Durability"))
            {
                percent = this.part.Resources["Durability"].amount / this.part.Resources["Durability"].maxAmount;
            }
            else
            {
                GSA.Durability.Debug.Log("KspDeMod [GetDurabilityPercent]: Resource Durability not Found");
            }
            return percent * 100;
        }

        /// <summary>
        /// Get the xxpiration date
        /// </summary>
        /// <returns>Seconds</returns>
        public double GetExpirationDate()
        {
            try
            {
                if (this.part.Resources.Contains("Durability"))
                {
                    return this.part.Resources["Durability"].amount / (this.finalReduce * (1 / TimeWarp.fixedDeltaTime));
                }
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [getExpirationDate]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [getExpirationDate]: StackTrace: " + ex.StackTrace);
                return -1;
            }
            return 0;
        }

        /// <summary>
        /// Get the expiration date Formated
        /// </summary>
        /// <returns>String</returns>
        public string GetFormatedExpirationDate()
        {
            try
            {
                double secondsToZero = GetExpirationDate();
                double kerbalYearSec = (426.08 * 6 * 60 * 60);
                double kerbalDaySec = (6 * 60 * 60);
                double kerbalHourSec = (60 * 60);

                int years = (int)Math.Floor(secondsToZero / kerbalYearSec);
                secondsToZero -= years * kerbalYearSec;

                int days = (int)Math.Floor(secondsToZero / kerbalDaySec);
                secondsToZero -= days * kerbalDaySec;

                int hours = (int)Math.Floor(secondsToZero / kerbalHourSec);
                secondsToZero -= hours * kerbalHourSec;

                int minutes = (int)Math.Floor(secondsToZero / 60);
                secondsToZero -= minutes * 60;

                int seconds = (int)Math.Floor(secondsToZero);

                return "T+ " + ((years > 0) ? "y" + years.ToString("D2") + ", " : "") +
                    ((days > 0) ? "d" + days.ToString("D2") + ", " : "") +
                    hours.ToString("D2") + ":" +
                    minutes.ToString("D2") + ":" +
                    seconds.ToString("D2");
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [getFormatedExpirationDate]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [getFormatedExpirationDate]: StackTrace: " + ex.StackTrace);
                return "ERROR";
            }
        }

        #endregion //Private methods

        #region Reduce methods

        /// <summary>
        /// Temperatur durability reduction
        /// </summary>
        /// <returns>additional reduction</returns>
        private double GetReduceTemperature()
        {
            double additionalReduce = 0;
            try
            {
                double tempMutli = 0;
                tempMutli = this.idealTemp.Evaluate(this.part.temperature);
                tempMutli = (tempMutli > 1) ? tempMutli : 1;
                additionalReduce += (this.currentWear * tempMutli) - this.currentWear;
                this.displayTempM = tempMutli.ToString("0.0000");
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceTemperature]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceTemperature]: StackTrace: " + ex.StackTrace);
            }
            return additionalReduce;
        }

        /// <summary>
        /// GForce durability reduction
        /// </summary>
        /// <returns>additional reduction</returns>
        private double GetReduceGeeForce()
        {
            double additionalReduce = 0;
            try
            {
                double geeForceMutli = 1;
                if (this.vessel.geeForce > 1 && this.vessel.geeForce < 17)
                {
                    geeForceMutli = Math.Pow(this.vessel.geeForce / 2, this.vessel.geeForce / 2.2);
                }
                else if (this.vessel.geeForce >= 17)
                {
                    geeForceMutli = Math.Pow(17 / 2, 17 / 2.2);
                }
                //GeeForceMutli = Math.Pow(vessel.geeForce / 2, vessel.geeForce / 2.2);
                geeForceMutli = (geeForceMutli > 1) ? geeForceMutli : 1;
                additionalReduce += (this.currentWear * geeForceMutli) - this.currentWear;
                this.displayGeeForce = geeForceMutli.ToString("0.0000");
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceGeeForce]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceGeeForce]: StackTrace: " + ex.StackTrace);
            }
            return additionalReduce;
        }

        /// <summary>
        /// Pressure durability reduction
        /// </summary>
        /// <param name="reduce">basic reduction</param>
        /// <returns>additional reduction</returns>
        private double GetReducePressure()
        {
            double additionalReduce = 0;
            try
            {
                double pressureMulti = 0;
                float pressure = 1f;
                pressure = Convert.ToSingle(this.vessel.staticPressure);
                pressureMulti = this.idealPressure.Evaluate(pressure);
                pressureMulti = (pressureMulti > 1) ? pressureMulti : 1;
                additionalReduce += (this.currentWear * pressureMulti) - this.currentWear;
                this.displayPressure = pressureMulti.ToString("0.0000");
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [getReducePressure]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [getReducePressure]: StackTrace: " + ex.StackTrace);
            }
            return additionalReduce;
        }

        /// <summary>
        /// Engine durability reduction
        /// </summary>
        /// <param name="reduce">basic reduction</param>
        /// <returns>additional reduction</returns>
        private double GetReduceEngine()
        {
            double additionalReduce = 0;
            try
            {
                double engineMutli = 1;
                if (this.engine != null)
                {
                    if (!this.engine.flameout && !this.engine.engineShutdown)
                    {
                        //EngineMutli = engineWear * (_engine.requestedThrust / _engine.maxThrust + 1);
                        engineMutli = Math.Pow(this.engineWear, (this.engine.requestedThrust / this.engine.maxThrust) * 100);
                        engineMutli = (engineMutli > 1) ? engineMutli : 1;
                        additionalReduce += (this.currentWear * engineMutli) - this.currentWear;
                    }
                }
                this.displayEngine = engineMutli.ToString("0.0000");
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceEngine]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceEngine]: StackTrace: " + ex.StackTrace);
            }
            return additionalReduce;
        }

        /// <summary>
        /// Experiment durability reduction
        /// </summary>
        private void GetReduceExperiment()
        {
            try
            {
                //ScienceExperiment ModuleScienceExperiment (Not Multiply, fixed Damage)
                if (this.scienceExperiment != null)
                {
                    if (this.scienceExperiment.Deployed && !this.expDeployed)
                    {
                        expDeployed = true;
                        GSA.Durability.Debug.Log("GSA Durability: [getReduceExperiment]");

                        int minRange = (int)Math.Round(5 * (2 - this.quality));
                        int maxRange = (int)Math.Round(35 * (2 - this.quality));

                        double experimentDamage = (this.part.Resources["Durability"].maxAmount / 100) * UnityEngine.Random.Range(minRange, maxRange);
                        this.part.Resources["Durability"].amount -= experimentDamage;
                        this.displayExp = experimentDamage.ToString("0.0000");
                    }
                }
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceExperiment]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceExperiment]: StackTrace: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Engine durability reduction
        /// </summary>
        /// <param name="reduce">basic reduction</param>
        /// <returns>additional reduction</returns>
        private double GetReduceRadiation()
        {
            double additionalReduce = 0;
            try
            {
                double radiationMutli = 1;
                Transform target = this.sun.transform;
                RaycastHit hit;
                this.displaySun = "";

                //LineRenderer line = (LineRenderer)gameObject.GetComponent(typeof(LineRenderer));
                //line.SetVertexCount(2);
                //line.SetWidth(0.1f, 0.1f);

                float sunDis = Convert.ToSingle(Vector3d.Distance(part.gameObject.transform.position, this.sun.transform.position));
                //if (Physics.Raycast(part.gameObject.transform.position, target.position, out hit, sunDis))
                if (Physics.Linecast(this.part.gameObject.transform.position, target.position, out hit))
                {
                    this.displaySun = "c: " + (hit.collider != null ? hit.collider.name : "N/A") + "; dis: " + hit.distance.ToString("0.0000");

                    //line.SetColors(Color.red, Color.red);
                    //line.SetPosition(0, part.gameObject.transform.position);
                    //line.SetPosition(1, hit.point);
                }
                else
                {
                    try
                    {
                        double atmoAbsob = (this.vessel.staticPressure > 1.05 ? 0 : 1.05 - this.vessel.staticPressure);
                        radiationMutli = sunDis * 1.4995217994990961392430031278165e-9;
                        radiationMutli *= atmoAbsob;
                        //displaySun += ";" + radiationMutli.ToString("0.0000"); 
                        radiationMutli -= radiationAbsorption;
                        this.displaySun += radiationMutli.ToString("0.0000");
                        //reduce *= (radiationMutli > 0 ? radiationMutli : 1);
                        additionalReduce += (this.currentWear * radiationMutli) - this.currentWear;

                        //line.SetColors(Color.green, Color.green);
                        //line.SetPosition(0, part.gameObject.transform.position);
                        //line.SetPosition(1, _sun.transform.position);
                        //line.SetWidth(0.01f, 0.01f);
                    }
                    catch (Exception ex)
                    {
                        //displaySun = "Error drow line";
                        GSA.Durability.Debug.LogError("GSA Durability: [getReduceRadiation]: Message: " + ex.Message);
                    }
                }
                this.displaySun = radiationMutli.ToString("0.0000");
            }
            catch (Exception ex)
            {
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceRadiation]: Message: " + ex.Message);
                GSA.Durability.Debug.LogError("GSA Durability: [getReduceRadiation]: StackTrace: " + ex.StackTrace);
            }
            return additionalReduce;
        }

        #endregion //Reduce methods
    }
}