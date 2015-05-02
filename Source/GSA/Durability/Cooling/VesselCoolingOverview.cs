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
        private bool hideWindows = true;
        private bool isActiveWindows = false;
        private bool isHoverButton = false;
        private bool runOnce = true;
        private bool guiRunning = false;
        private Vessel vessel;

        private GUIStyle labelTxtLeft;
        private GUIStyle labelTxtCenter;
        private GUIStyle labelTxtRight;
        private GUIStyle labelTxtHeader;
        private GUIStyle labelTxtLinkLeft;
        private GUIStyle window;

        private ApplicationLauncherButton launcherButton = null;

        private Texture2D coolingButtonTexture = null;
        private Texture2D coolingButtonIdle = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D coolingButtonWarn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D coolingButtonIWarn = new Texture2D(38, 38, TextureFormat.ARGB32, false);
        private Texture2D coolingButtonError = new Texture2D(38, 38, TextureFormat.ARGB32, false);

        private Texture2D listIconActive = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private Texture2D listIconInactive = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private Texture2D listIconOk = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private Texture2D listIconWarn = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        private Texture2D listIconCritical = new Texture2D(16, 16, TextureFormat.ARGB32, false);

        protected Rect mainWindowPos = new Rect(Screen.width - 460f, 38f, 460f, 320f);

        private Vector2 partListScroll = Vector2.zero;
        private Vector2 partDetailScroll = Vector2.zero;
        private Vector2 temptListRightScroll = Vector2.zero;
        private Vector2 temptListLEftScroll = Vector2.zero;
        private Part selectedPart;

        private void OnGUIApplicationLauncherReady()
        {
            // Create the button in the KSP AppLauncher
            if (launcherButton == null)
            {
                GSA.Debug.Log("[GSA Cooling] VesselCoolingOverview->OnGUIApplicationLauncherReady: set _launcherButton");
                launcherButton = ApplicationLauncher.Instance.AddModApplication(UIToggle, UIToggle,
                                                                            UIHover, UIHoverOut,
                                                                            null, null,
                                                                            ApplicationLauncher.AppScenes.FLIGHT,
                                                                            coolingButtonIdle);
            }
        }

        private void OnGUIApplicationLauncherDestroyed()
        {
            launcherButtonRemove();
        }

        public void UIToggle()
        {
             hideWindows = !hideWindows;
             isActiveWindows = !isActiveWindows;
        }
        public void UIHover()
        {
            if (hideWindows)
                hideWindows = false;
            isHoverButton = true;
        }
        public void UIHoverOut()
        {
            isHoverButton = false;
        }

        public void launcherButtonRemove()
        {
            if (launcherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(launcherButton);
                launcherButton = null;
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
            if (FlightGlobals.ActiveVessel != null && runOnce)
            {
                vessel = FlightGlobals.ActiveVessel;
                runOnce = false;
                setGUIStyles();
            }

            if(vessel != null)
            {
                if(!guiRunning)
                {
                    startGUI();
                }
            } 
            else
            {
                if (guiRunning)
                {
                    stopGUI();
                }
            }
        }

        public void Awake()
        {
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconIdle"))
            {
                coolingButtonIdle = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconIdle", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconWarn"))
            {
                coolingButtonWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconIWarn"))
            {
                coolingButtonIWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconIWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconError"))
            {
                coolingButtonError = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/CoolingToolbarIconError", false);
            }

            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListActive"))
            {
                listIconActive = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListActive", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListInactive"))
            {
                listIconInactive = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListInactive", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListStatusOk"))
            {
                listIconOk = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListStatusOk", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListStatusWarn"))
            {
                listIconWarn = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListStatusWarn", false);
            }
            if (GameDatabase.Instance.ExistsTexture("GermanSpaceAlliance/Icons/ListStatusCritical"))
            {
                listIconCritical = GameDatabase.Instance.GetTexture("GermanSpaceAlliance/Icons/ListStatusCritical", false);
            }

            OnGUIApplicationLauncherReady();
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIApplicationLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIApplicationLauncherDestroyed);
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(OnGUIAppLauncherUnreadifying);
            GameEvents.onGameSceneLoadRequested.Add(OnSceneChangeRequest);
        }

        private void SetAppLauncherButtonTexture(Texture2D tex2d)
        {
            if (launcherButton != null)
            {
                if (tex2d != coolingButtonTexture)
                {
                    coolingButtonTexture = tex2d;
                    launcherButton.SetTexture(tex2d);
                }
            }
        }

        private void WindowGUI(int windowID)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            temptListRightScroll = GUILayout.BeginScrollView(temptListRightScroll, false, false, GUILayout.Width(230f));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Radiators In", labelTxtLeft, GUILayout.ExpandWidth(true));
            GUILayout.Label(new GUIContent()
            {
                text = (TemperatureManager.Instance.CoolantTemperatureRadiatorsIn - 273.15f).ToString("0.00") + "° C"
            }, labelTxtRight, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Radiators Out", labelTxtLeft, GUILayout.ExpandWidth(true));
            GUILayout.Label(new GUIContent()
            {
                text = (TemperatureManager.Instance.CoolantTemperatureRadiatorsOut - 273.15f).ToString("0.00") + "° C"
            }, labelTxtRight, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            temptListLEftScroll = GUILayout.BeginScrollView(temptListLEftScroll, false, false, GUILayout.Width(205f));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Parts In", labelTxtLeft, GUILayout.ExpandWidth(true));
            GUILayout.Label(new GUIContent()
            {
                text = (TemperatureManager.Instance.CoolantTemperaturePartsIn - 273.15f).ToString("0.00") + "° C"
            }, labelTxtRight, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Parts Out", labelTxtLeft, GUILayout.ExpandWidth(true));
            GUILayout.Label(new GUIContent()
            {
                text = (TemperatureManager.Instance.CoolantTemperaturePartsOut - 273.15f).ToString("0.00") + "° C"
            }, labelTxtRight, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Flow Rate", labelTxtLeft, GUILayout.ExpandWidth(true));

            GUIContent contentCoolant = new GUIContent();
            contentCoolant.text = TemperatureManager.Instance.CoolantFlowRate.ToString("0.00");
            GUILayout.Label(contentCoolant, labelTxtRight, GUILayout.ExpandWidth(true));

            GUILayout.EndHorizontal();
            getCoolings();
        }

        protected void drawGUI()
        {
            if (!hideWindows && vessel != null)
            {
                GUI.skin = HighLogic.Skin;
                mainWindowPos = GUILayout.Window(1, mainWindowPos, WindowGUI, "Cooling", window);
                if(!isHoverButton && !isActiveWindows && !mainWindowPos.Contains(Event.current.mousePosition))
                {
                    hideWindows = true;
                }
            }
        }

        private void startGUI()
        {
            RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
            guiRunning = true;
        }

        private void stopGUI()
        {
            RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));
            guiRunning = false;
        }

        private void setGUIStyles()
        {
            window = new GUIStyle(HighLogic.Skin.window);
            window.stretchHeight = true;

            labelTxtLeft = new GUIStyle(HighLogic.Skin.label);
            labelTxtLeft.normal.textColor = Color.white;
            labelTxtLeft.alignment = TextAnchor.MiddleLeft;
            labelTxtLeft.stretchWidth = true;

            labelTxtCenter = new GUIStyle(HighLogic.Skin.label);
            labelTxtCenter.normal.textColor = Color.white;
            //_labelTxtCenter.fontSize = 12;
            labelTxtCenter.alignment = TextAnchor.MiddleCenter;

            labelTxtRight = new GUIStyle(HighLogic.Skin.label);
            labelTxtRight.normal.textColor = Color.white;
            //_labelTxtRight.fontSize = 12;
            labelTxtRight.alignment = TextAnchor.MiddleRight;

            labelTxtLinkLeft = labelTxtLeft;
            labelTxtLinkLeft.hover.textColor = Color.gray;
            labelTxtLinkLeft.active.textColor = Color.yellow;

            labelTxtHeader = new GUIStyle(HighLogic.Skin.label);
            labelTxtHeader.normal.textColor = Color.white;
            labelTxtHeader.fontSize = 14;
            labelTxtHeader.alignment = TextAnchor.MiddleLeft;
            labelTxtHeader.fontStyle = FontStyle.Bold;
        }

        private void getCoolings()
        {
            Rect iconRect;
            GUILayout.BeginHorizontal();

            partListScroll = GUILayout.BeginScrollView(partListScroll, false, false, GUILayout.Width(230f));
            foreach(Part part in vessel.parts)
            {
                GUIContent content = new GUIContent();
                content.text = part.name;
                content.tooltip = "Part";

                GUILayout.BeginHorizontal();
                //GUILayout.Space(44f); 
 
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
                GUILayout.Space(23f); 

                //Label
                GUILayout.Label(content, labelTxtLinkLeft, GUILayout.ExpandWidth(true));

                //Status Icon
                iconRect = GUILayoutUtility.GetLastRect();
                iconRect.size = new Vector2(16, 16);
                iconRect.position = new Vector2(iconRect.position.x - 18f, iconRect.position.y);

                float priority = TemperatureManager.Instance.GetPartCoolingPrority(part, true);
                Texture2D icon = null;
                if(priority == 0) {
                    icon = listIconOk;
                }
                else if (priority > -100 || priority < 100)
                {
                    icon = listIconWarn;
                }
                else if (priority <= -100 || priority >= 100)
                {
                    icon = listIconCritical;
                }
                GUI.DrawTexture(iconRect, icon);

                if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    part.SetHighlightType(Part.HighlightType.OnMouseOver);
                    part.SetHighlight(true, false);
                    if (Event.current.type == EventType.MouseDown)
                    {
                        selectedPart = part;
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
            partDetailScroll = GUILayout.BeginScrollView(partDetailScroll, false, false, GUILayout.Width(205f));
            if (selectedPart != null)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label(selectedPart.name, labelTxtHeader, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
