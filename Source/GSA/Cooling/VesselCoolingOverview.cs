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

using System.Collections.Generic;
using UnityEngine;

namespace GSA.Cooling
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VesselCoolingOverview : MonoBehaviour
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
        private GUIStyle _labelTxtHeader;
        private GUIStyle _labelTxtLinkLeft;
        private GUIStyle _window;

        private ApplicationLauncherButton _launcherButton = null;

        private Texture2D _coolingButtonTexture = null;
        private Texture2D _coolingButtonIdle = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D _coolingButtonWarn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D _coolingButtonIWarn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D _coolingButtonError = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        private Texture2D _listActive = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private Texture2D _listInactive = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private Texture2D _listOk = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private Texture2D _listWarn = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private Texture2D _listCritical = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        protected Rect _mainWindowPos = new Rect(Screen.width - 460f, 38f, 460f, 300f);

        private Vector2 _partListScroll = Vector2.zero;
        private Vector2 _partDetailScroll = Vector2.zero;
        private Part _selectedPart;

        private void OnGUIApplicationLauncherReady()
        {
            // Create the button in the KSP AppLauncher
            if (_launcherButton == null)
            {
                GSA.Debug.Log("[GSA Cooling] VesselCoolingOverview->OnGUIApplicationLauncherReady: set _launcherButton");
                _launcherButton = ApplicationLauncher.Instance.AddModApplication(UIToggle, UIToggle,
                                                                            UIHover, UIHoverOut,
                                                                            null, null,
                                                                            ApplicationLauncher.AppScenes.FLIGHT,
                                                                            _coolingButtonIdle);
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
                GSA.Debug.Log("[GSA Cooling] VesselCoolingOverview->launcherButtonRemove: RemoveModApplication");
            }
            else
            {
                GSA.Debug.Log("[GSA Cooling] VesselCoolingOverview->launcherButtonRemove: _launcherButton is null");
            }
        }
        public void OnSceneChangeRequest(GameScenes _scene)
        {
            launcherButtonRemove();
            //_hideWindows = false;
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
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconIdle"))
            {
                _coolingButtonIdle = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconIdle", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconWarn"))
            {
                _coolingButtonWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconIWarn"))
            {
                _coolingButtonIWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconIWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconError"))
            {
                _coolingButtonError = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconError", false);
            }

            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListActive"))
            {
                _listActive = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListActive", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListInactive"))
            {
                _listInactive = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListInactive", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListOk"))
            {
                _listOk = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListOk", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListWarn"))
            {
                _listWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListCritical"))
            {
                _listCritical = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListCritical", false);
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
                if (tex2d != _coolingButtonTexture)
                {
                    _coolingButtonTexture = tex2d;
                    _launcherButton.SetTexture(tex2d);
                }
            }
        }

        private void WindowGUI(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();;
            GUILayout.Label("Coolant Temp", _labelTxtLeft, GUILayout.ExpandWidth(true));

            GUIContent contentCoolant = new GUIContent();
            contentCoolant.text = TemperatureManager.Instance.CoolantTemp.ToString("0.00") + "° C";
            GUILayout.Label(contentCoolant, _labelTxtRight, GUILayout.ExpandWidth(true));

            GUILayout.EndHorizontal();
            getCoolings();
        }

        protected void drawGUI()
        {
            if (!_hideWindows && _vessel != null)
            {
                GUI.skin = HighLogic.Skin;
                _mainWindowPos = GUILayout.Window(1, _mainWindowPos, WindowGUI, "Cooling", _window);
                if(!_isHoverButton && !_isActiveWindows && !_mainWindowPos.Contains(Event.current.mousePosition))
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

            _labelTxtLinkLeft = _labelTxtLeft;
            _labelTxtLinkLeft.hover.textColor = Color.gray;
            _labelTxtLinkLeft.active.textColor = Color.yellow;

            _labelTxtHeader = new GUIStyle(HighLogic.Skin.label);
            _labelTxtHeader.normal.textColor = Color.white;
            _labelTxtHeader.fontSize = 14;
            _labelTxtHeader.alignment = TextAnchor.MiddleLeft;
            _labelTxtHeader.fontStyle = FontStyle.Bold;
        }

        private void getCoolings()
        {
            Rect iconRect;
            GUILayout.BeginHorizontal();

            _partListScroll = GUILayout.BeginScrollView(_partListScroll, false, false, GUILayout.Width(230f));
            foreach(Part part in _vessel.parts)
            {
                GUIContent content = new GUIContent();
                content.text = part.name;
                content.tooltip = "Part";

                GUILayout.BeginHorizontal();
               // GUILayout.Space(44f);  
 
                //Cooling Toggle
                if (part.Modules.Contains("DurabilityModule"))
                {
                    PartModule durabilityModule = part.Modules["DurabilityModule"];
                    BaseField coolingField = null;
                    foreach (BaseField f in durabilityModule.Fields)
                    {
                        if (f.FieldInfo.Name == "cooling")
                        {
                            coolingField = f;
                            break;
                        }
                    }
                    bool boCool = (bool)coolingField.GetValue(durabilityModule);
                    bool newBoCool; newBoCool = GUILayout.Toggle(boCool, "");
                    if (boCool != newBoCool)
                    {
                        GSA.Debug.Log("[GSA Cooling] VesselCoolingOverview->getCoolings: toogle Cooling " + newBoCool.ToString());
                        coolingField.SetValue(newBoCool, durabilityModule);
                        TemperatureManager.Instance.FindPartsToBoCooled();
                    }
                }        

                //Label
                GUILayout.Label(content, _labelTxtLinkLeft, GUILayout.ExpandWidth(true));
                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    part.SetHighlightType(Part.HighlightType.OnMouseOver);
                    part.SetHighlight(true, false);
                    if (Event.current.type == EventType.MouseDown)
                    {
                        _selectedPart = part;
                        GSA.Debug.Log("[GSA Cooling] VesselCoolingOverview->getCoolings: Part selected: " + part.name);
                    }
                }
                else
                {
                    part.SetHighlight(false, false);
                }
                              
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            _partDetailScroll = GUILayout.BeginScrollView(_partDetailScroll, false, false, GUILayout.Width(205f));
            if (_selectedPart != null)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label(_selectedPart.name, _labelTxtHeader, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
