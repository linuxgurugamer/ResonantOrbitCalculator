/*
The MIT License (MIT)

Copyright (c) 2016 Boris-Barboris

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
to deal in this Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI.Screens;
using ToolbarControl_NS;

namespace ResonantOrbitCalculator
{

    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public partial class ResonantOrbitCalculator : MonoBehaviour
    {
        public static ResonantOrbitCalculator Instance;
        public EditorVesselOverlays overlays;
        public EditorMarker_CoL old_CoL_marker;
        //public static CoLMarkerFull new_CoL_marker;
        public static PhysicsGlobals.LiftingSurfaceCurve bodylift_curves;
        static bool far_searched = false;
        static bool far_found = false;
        internal static bool showStockMarker = false;
        //Button.ButtonClickedEvent clickEvent;
        public GraphWindow graphWindow;

        [KSPField(isPersistant = true)]
        public string testlastSelectedPlanet = "aaaabbbb";

        void Start()
        {
            Instance = this;
            Debug.Log("[ResonantOrbitCalculator]: Starting!");

            if (!far_searched)
            {
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (a.GetName().Name.Equals("FerramAerospaceResearch"))
                    {
                        far_found = true;
                        break;
                    }
                }
                far_searched = true;
            }
            if (far_found)
            {
                Debug.Log("[ResonantOrbitCalculator]: FAR found, disabling itself!");
                GameObject.Destroy(this.gameObject);
                return;
            }

            graphWindow = new GraphWindow();
            // should be called once, so let's deserialize graph here too                
            graphWindow.Start();
            graphWindow.load_settings();
            graphWindow.init_textures(true);
   

            onAppLauncherLoad();
#if false
         graphWindow.init_reflections();
            overlays = EditorVesselOverlays.fetch;
            if (overlays == null)
            {
                Debug.Log("[ResonantOrbitCalculator]: overlays is null!");
                GameObject.Destroy(this.gameObject);
                return;
            }
            old_CoL_marker = overlays.CoLmarker;
            if (old_CoL_marker == null)
            {
                Debug.Log("[ResonantOrbitCalculator]: CoL_marker is null!");
                GameObject.Destroy(this.gameObject);
                return;
            }
            bodylift_curves = PhysicsGlobals.GetLiftingSurfaceCurve("BodyLift");
            //   if (new_CoL_marker == null)
            {
                new_CoL_marker = this.gameObject.AddComponent<CoLMarkerFull>();

                CoLMarkerFull.lift_curves = bodylift_curves;
                new_CoL_marker.posMarkerObject = (GameObject)GameObject.Instantiate(old_CoL_marker.dirMarkerObject);
                new_CoL_marker.posMarkerObject.transform.parent = new_CoL_marker.transform;
                new_CoL_marker.posMarkerObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                new_CoL_marker.posMarkerObject.SetActive(false);
                new_CoL_marker.posMarkerObject.layer = 2;
                foreach (Transform child in new_CoL_marker.posMarkerObject.transform)
                {
                    child.gameObject.layer = 2;
                }
                GameEvents.onEditorRestart.Add(TurnOffCoL);
                graphWindow = new GraphWindow();
                // should be called once, so let's deserialize graph here too                
                graphWindow.Start();
                graphWindow.load_settings();
                graphWindow.init_textures(true);
                graphWindow.init_reflections();

                clickEvent = new Button.ButtonClickedEvent();
                clickEvent.AddListener(ToggleCoL);
            }
            GameEvents.onGUIApplicationLauncherReady.Add(onAppLauncherLoad);
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(onAppLauncherUnload);

            GameEvents.onEditorShipModified.Add(EditorPartEvent);

            onAppLauncherLoad();
            graphWindow.shown = false;
            new_CoL_marker.enabled = false;

            old_CoL_marker.gameObject.SetActive(false);
            overlays.toggleCoLbtn.onClick = clickEvent;
            //overlays.toggleCoLbtn.methodToInvoke = "ToggleCoL";
        }

        void EditorPartEvent(ShipConstruct s)
        {
            if (graphWindow.autoUpdate && graphWindow.shown)
                graphWindow.update_graphs();
        }

        public void SwapMarkers()
        {
            if (ColActive)
            {
                if (showStockMarker)
                {
                    old_CoL_marker.gameObject.SetActive(true);
                    //new_CoL_marker.gameObject.SetActive(false);
                    new_CoL_marker.enabled = false;
                    new_CoL_marker.posMarkerObject.SetActive(false);
                }
                else
                {
                    old_CoL_marker.gameObject.SetActive(false);
                    //new_CoL_marker.gameObject.SetActive(true);
                    new_CoL_marker.enabled = true;
                    new_CoL_marker.posMarkerObject.SetActive(true);
                }
            }
        }
        bool ColActive = false;

        public void ToggleCoL()
        {
            if (EditorLogic.fetch.ship != null && EditorLogic.fetch.ship.parts.Count > 0)
            {
                ColActive = !ColActive;
                if (ColActive)
                {
                    if (!showStockMarker)
                    {
                        old_CoL_marker.gameObject.SetActive(false);
                        //f (!new_CoL_marker.gameObject.activeSelf)
                      //      new_CoL_marker.gameObject.SetActive(true);
                        new_CoL_marker.enabled = true;
                    }
                    else
                    {
                        old_CoL_marker.gameObject.SetActive(true);
                        new_CoL_marker.enabled = false;
                    }
                } else
                {
                    new_CoL_marker.enabled = false;
                    old_CoL_marker.gameObject.SetActive(false);
                }

            }
            else
            {
                ColActive = false;
                new_CoL_marker.enabled = false;
                old_CoL_marker.gameObject.SetActive(false);
            }
            new_CoL_marker.posMarkerObject.SetActive(new_CoL_marker.enabled);
        }
#endif
        }
        public void OnDestroy()
        {
            if (graphWindow != null)
            {
                graphWindow.save_settings();
                graphWindow.shown = false;
            }
            if (GameEvents.onGUIApplicationLauncherReady != null)
                GameEvents.onGUIApplicationLauncherReady.Remove(onAppLauncherLoad);

                    #if false
            if (GameEvents.onEditorRestart != null)
                GameEvents.onEditorRestart.Remove(TurnOffCoL);
            if (GameEvents.onEditorShipModified != null)
                GameEvents.onEditorShipModified.Remove(EditorPartEvent);
            if (new_CoL_marker != null)
                Destroy(new_CoL_marker);
#endif
            graphWindow = null;
        }

#if false
        public void TurnOffCoL()
        {
            new_CoL_marker.enabled = false;
            new_CoL_marker.posMarkerObject.SetActive(false);
        }
#endif






       // static ApplicationLauncherButton launcher_btn;
        static ToolbarControl toolbarControl = null;
        internal const string MODID = "ResonantOrbitCalculator_NS";
        internal const string MODNAME = "Resonant Orbit Calculator";

        void onAppLauncherLoad()
        {

            if (toolbarControl != null)
                return;
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(OnALTrue, OnALFalse,
                 ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
                 MODID,
                "ResonantOrbitCalculatorButton",
                "ResonantOrbitCalculator/Images/icon",
                "ResonantOrbitCalculator/Images/iconSmall",
                MODNAME
            );

        }

        void Destroy()
        {
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);
            toolbarControl = null;
        }

        void OnALTrue()
        {
            graphWindow.shown = true;
        }

        void OnALFalse()
        {
            graphWindow.shown = false;
            if (graphWindow.planetSelection != null)
                Destroy(graphWindow.planetSelection);
            graphWindow.planetSelection = null;
        }

        void OnGUI()
        {
            graphWindow.OnGUI();
        }

    }
}
