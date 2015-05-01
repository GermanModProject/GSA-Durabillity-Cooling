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
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VesselOverview : MonoBehaviour
    {
        bool hideWindows = true;
        bool isActiveWindows = false;
        bool isHoverButton = false;
        bool runOnce = true;
        bool guiRunning = false;
        Vessel vessel;

        GUIStyle labelTxtLeft;
        GUIStyle labelTxtCenter;
        GUIStyle labelTxtRight;
        GUIStyle window;

        ApplicationLauncherButton launcherButton = null;

        Texture2D durabilityButtonTexture = null;
        Texture2D durabilityButtonIdle = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        Texture2D durabilityButtonWarn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        Texture2D durabilityButtonIWarn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        Texture2D durabilityButtonError = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        Vector2 partListScroll = Vector2.zero;

        protected Rect mainWindowPos = new Rect(Screen.width - 400f, 38f, 400f, 300f);

        private void OnGUIApplicationLauncherReady()
        {
            // Create the button in the KSP AppLauncher
            if (this.launcherButton == null)
            {
                try
                {
                    GSA.Durability.Debug.Log("[GSA Durablity] VesselOverview->OnGUIApplicationLauncherReady: set _launcherButton");
                    this.launcherButton = ApplicationLauncher.Instance.AddModApplication(this.UIToggle, this.UIToggle,
                                                                                this.UIHover, this.UIHoverOut,
                                                                                null, null,
                                                                                ApplicationLauncher.AppScenes.FLIGHT,
                                                                                this.durabilityButtonIdle);
                }
                catch (Exception ex)
                {
                    GSA.Durability.Debug.LogError("[GSA Durablity] VesselOverview->OnGUIApplicationLauncherReady: set _launcherButton ERROR:" + ex.Message);
                }
            }
        }

        private void OnGUIApplicationLauncherDestroyed()
        {
            GSA.Durability.Debug.Log("[GSA Durablity] VesselOverview->OnGUIApplicationLauncherDestroyed");
            LauncherButtonRemove();
        }

        public void UIToggle()
        {
            GSA.Durability.Debug.Log("[GSA Durablity] VesselOverview->UIToggle");
            this.hideWindows = !this.hideWindows;
            this.isActiveWindows = !this.isActiveWindows;
        }
        public void UIHover()
        {
            GSA.Durability.Debug.Log("[GSA Durablity] VesselOverview->UIHover");
            if (this.hideWindows)
                this.hideWindows = false;
            this.isHoverButton = true;
        }
        public void UIHoverOut()
        {
            GSA.Durability.Debug.Log("[GSA Durablity] VesselOverview->UIHoverOut");
            this.isHoverButton = false;
        }

        public void LauncherButtonRemove()
        {
            if (this.launcherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(this.launcherButton);
                this.launcherButton = null;
                GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIApplicationLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIApplicationLauncherDestroyed);
                GameEvents.onGUIApplicationLauncherUnreadifying.Remove(OnGUIAppLauncherUnreadifying);
                GameEvents.onGameSceneLoadRequested.Remove(OnSceneChangeRequest);
                GSA.Durability.Debug.Log("[GSA Durablity] VesselOverview->launcherButtonRemove: RemoveModApplication");
            }
            else
            {
                GSA.Durability.Debug.Log("[GSA Durablity] VesselOverview->launcherButtonRemove: _launcherButton is null");
            }
        }
        public void OnSceneChangeRequest(GameScenes scene)
        {
            LauncherButtonRemove();
        }
        private void OnGUIAppLauncherUnreadifying(GameScenes scene)
        {
            LauncherButtonRemove();
        }

        public void Update()
        {
            if (FlightGlobals.ActiveVessel != null && this.runOnce)
            {
                this.vessel = FlightGlobals.ActiveVessel;
                this.runOnce = false;
                SetGUIStyles();
            }

            if (this.vessel != null)
            {
                if (!this.guiRunning)
                {
                    StartGUI();
                }
            } 
            else
            {
                if (this.guiRunning)
                {
                    StopGUI();
                }
            }
        }

        public void Awake()
        {
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconIdle"))
            {
                this.durabilityButtonIdle = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconIdle", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconWarn"))
            {
                this.durabilityButtonWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconIWarn"))
            {
                this.durabilityButtonIWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconIWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconError"))
            {
                this.durabilityButtonError = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconError", false);
            }

            OnGUIApplicationLauncherReady();
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIApplicationLauncherDestroyed);
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(OnGUIAppLauncherUnreadifying);
            GameEvents.onGameSceneLoadRequested.Add(OnSceneChangeRequest);
        }

        private void SetAppLauncherButtonTexture(Texture2D tex2d)
        {
            if (this.launcherButton != null)
            {
                if (tex2d != this.durabilityButtonTexture)
                {
                    this.durabilityButtonTexture = tex2d;
                    this.launcherButton.SetTexture(tex2d);
                    GSA.Durability.Debug.Log("GSA Durability Overview: set Texture");
                }
            }
        }

        private void WindowGUI(int windowID)
        {
            GUILayout.BeginHorizontal();
            this.partListScroll = GUILayout.BeginScrollView(partListScroll, false, false, GUILayout.Width(385f));
            GetDurabiltys();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        protected void DrawGUI()
        {
            if (!this.hideWindows && this.vessel != null)
            {
                GUI.skin = HighLogic.Skin;
                this.mainWindowPos = GUILayout.Window(1, this.mainWindowPos, WindowGUI, "Durability", this.window);
                if (!this.isHoverButton && !this.isActiveWindows && !mainWindowPos.Contains(Event.current.mousePosition))
                {
                    this.hideWindows = true;
                }
            }
        }

        private void StartGUI()
        {
            RenderingManager.AddToPostDrawQueue(3, new Callback(DrawGUI));
            this.guiRunning = true;
        }

        private void StopGUI()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawGUI));
            this.guiRunning = false;
        }

        private void SetGUIStyles()
        {
            this.window = new GUIStyle(HighLogic.Skin.window);
            this.window.stretchHeight = true;

            this.labelTxtLeft = new GUIStyle(HighLogic.Skin.label);
            this.labelTxtLeft.normal.textColor = Color.white;
            //_labelTxtLeft.fontSize = 12;
            this.labelTxtLeft.alignment = TextAnchor.MiddleLeft;
            this.labelTxtLeft.stretchWidth = true;

            this.labelTxtCenter = new GUIStyle(HighLogic.Skin.label);
            this.labelTxtCenter.normal.textColor = Color.white;
            //_labelTxtCenter.fontSize = 12;
            this.labelTxtCenter.alignment = TextAnchor.MiddleCenter;

            this.labelTxtRight = new GUIStyle(HighLogic.Skin.label);
            this.labelTxtRight.normal.textColor = Color.white;
            //_labelTxtRight.fontSize = 12;
            this.labelTxtRight.alignment = TextAnchor.MiddleRight;
        }

        private void GetDurabiltys()
        {
            GUILayout.BeginVertical();
            foreach (Part part in this.vessel.parts)
            {
                GUIContent content = new GUIContent();
                content.text = part.name;
                content.tooltip = "Part";

                GUILayout.BeginHorizontal();
                GUILayout.Label(content, this.labelTxtLeft, GUILayout.ExpandWidth(true));

                if (part.Resources.Contains("Durability"))
                {

                    string durability, amount, percent;
                    durability = part.Resources["Durability"].maxAmount.ToString("0");
                    amount = part.Resources["Durability"].amount.ToString("0.00");
                    double percentD = (part.Resources["Durability"].amount / part.Resources["Durability"].maxAmount) * 100;
                    percent = percentD.ToString("0.0");

                    GUIContent contentD = new GUIContent();
                    contentD.text = amount + "/" + durability + " (" + percent + "%)";
                    contentD.tooltip = "Durability";

                    GUILayout.Label(contentD, this.labelTxtRight, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}
