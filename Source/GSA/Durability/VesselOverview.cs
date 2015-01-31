///////////////////////////////////////////////////////////////////////////////
//
//    Durability a plugin for Kerbal Space Program from SQUAD
//    (https://www.kerbalspaceprogram.com/)
//    and part of GSA Mod
//    (http://www.kerbalspaceprogram.de)
//
//    Author: runner78
//    Copyright (c) 2014 runner78
//
//    This program, coding and graphics are provided under the following Creative Commons license.
//    Attribution-NonCommercial 3.0 Unported
//    https://creativecommons.org/licenses/by-nc/3.0/
//
///////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

namespace GSA.Durability
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VesselOverview : MonoBehaviour
    {
        private bool _hideWindows = true;
        private bool _isActiveWindows = false;
        private bool _isHoverButton = false;
        private bool _runOnce = true;
        private bool _guiRunning = false;
        private Vessel _vessel;

        private GUIStyle _labelTxtLeft;
        private GUIStyle _labelTxtCenter;
        private GUIStyle _labelTxtRight;
        private GUIStyle _window;

        private ApplicationLauncherButton _launcherButton = null;

        private Texture2D _durabilityButtonTexture = null;
        private Texture2D _durabilityButtonIdle = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D _durabilityButtonWarn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D _durabilityButtonIWarn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D _durabilityButtonError = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        private Vector2 _partListScroll = Vector2.zero;

        protected Rect _mainWindowPos = new Rect(Screen.width - 400f, 38f, 400f, 300f);

        private void OnGUIApplicationLauncherReady()
        {
            // Create the button in the KSP AppLauncher
            if (_launcherButton == null)
            {
                GSA.Durability.Debug.Log("[GSA Durablity] VesselOverview->OnGUIApplicationLauncherReady: set _launcherButton");
                _launcherButton = ApplicationLauncher.Instance.AddModApplication(UIToggle, UIToggle,
                                                                            UIHover, UIHoverOut,
                                                                            null, null,
                                                                            ApplicationLauncher.AppScenes.FLIGHT,
                                                                            _durabilityButtonIdle);
            }
        }

        private void OnGUIApplicationLauncherDestroyed()
        {
            launcherButtonRemove();
        }

        public void UIToggle()
        {
            _hideWindows = !_hideWindows;
            _isActiveWindows = !_isActiveWindows;
        }
        public void UIHover()
        {
            if (_hideWindows)
                _hideWindows = false;
            _isHoverButton = true;
        }
        public void UIHoverOut()
        {
            _isHoverButton = false;
        }

        public void launcherButtonRemove()
        {
            if (_launcherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(_launcherButton);
                _launcherButton = null;
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
        public void OnSceneChangeRequest(GameScenes _scene)
        {
            launcherButtonRemove();
        }
        private void OnGUIAppLauncherUnreadifying(GameScenes _scene)
        {
            launcherButtonRemove();
        }

        public void Update()
        {
            if (FlightGlobals.ActiveVessel != null && _runOnce)
            {
                _vessel = FlightGlobals.ActiveVessel;
                _runOnce = false;
                setGUIStyles();
            }

            if(_vessel != null)
            {
                if(!_guiRunning)
                {
                    startGUI();
                }
            } 
            else
            {
                if (_guiRunning)
                {
                    stopGUI();
                }
            }
        }

        public void Awake()
        {
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconIdle"))
            {
                _durabilityButtonIdle = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconIdle", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconWarn"))
            {
                _durabilityButtonWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconIWarn"))
            {
                _durabilityButtonIWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconIWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconError"))
            {
                _durabilityButtonError = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/DurabilityToolbarIconError", false);
            }

            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIApplicationLauncherDestroyed);
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(OnGUIAppLauncherUnreadifying);
            GameEvents.onGameSceneLoadRequested.Add(OnSceneChangeRequest);
        }

        private void SetAppLauncherButtonTexture(Texture2D tex2d)
        {
            if (_launcherButton != null)
            {
                if (tex2d != _durabilityButtonTexture)
                {
                    _durabilityButtonTexture = tex2d;
                    _launcherButton.SetTexture(tex2d);
                    GSA.Durability.Debug.Log("GSA Durability Overview: set Texture");
                }
            }
        }

        private void WindowGUI(int windowID)
        {
            GUILayout.BeginHorizontal();
            _partListScroll = GUILayout.BeginScrollView(_partListScroll, false, false, GUILayout.Width(385f));
            getDurabiltys();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        protected void drawGUI()
        {
            if (!_hideWindows && _vessel != null)
            {
                GUI.skin = HighLogic.Skin;
                _mainWindowPos = GUILayout.Window(1, _mainWindowPos, WindowGUI, "Durability", _window);
                if (!_isHoverButton && !_isActiveWindows && !_mainWindowPos.Contains(Event.current.mousePosition))
                {
                    _hideWindows = true;
                }
            }
        }

        private void startGUI()
        {
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
            _guiRunning = true;
        }

        private void stopGUI()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));
            _guiRunning = false;
        }

        private void setGUIStyles()
        {
            _window = new GUIStyle(HighLogic.Skin.window);
            _window.stretchHeight = true;

            _labelTxtLeft = new GUIStyle(HighLogic.Skin.label);
            _labelTxtLeft.normal.textColor = Color.white;
            //_labelTxtLeft.fontSize = 12;
            _labelTxtLeft.alignment = TextAnchor.MiddleLeft;
            _labelTxtLeft.stretchWidth = true;

            _labelTxtCenter = new GUIStyle(HighLogic.Skin.label);
            _labelTxtCenter.normal.textColor = Color.white;
            //_labelTxtCenter.fontSize = 12;
            _labelTxtCenter.alignment = TextAnchor.MiddleCenter;

            _labelTxtRight = new GUIStyle(HighLogic.Skin.label);
            _labelTxtRight.normal.textColor = Color.white;
            //_labelTxtRight.fontSize = 12;
            _labelTxtRight.alignment = TextAnchor.MiddleRight;
        }

        private void getDurabiltys()
        {
            GUILayout.BeginVertical();
            foreach(Part part in _vessel.parts)
            {
                GUIContent content = new GUIContent();
                content.text = part.name;
                content.tooltip = "Part";

                GUILayout.BeginHorizontal();
                GUILayout.Label(content, _labelTxtLeft, GUILayout.ExpandWidth(true));

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

                    GUILayout.Label(contentD, _labelTxtRight, GUILayout.ExpandWidth(true));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}
