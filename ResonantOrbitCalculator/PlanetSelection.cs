using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using KSP.IO;
using ClickThroughFix;

namespace ResonantOrbitCalculator
{

    public class PlanetSelection : MonoBehaviour
    {
        static public CelestialBody selectedBody = Planetarium.fetch.Home;

        List<CelestialBody> bodiesList = null;
        public static bool isActive = false;
        const int wnd_width = 200;
        const int wnd_height = 500;
        Rect planetWin = new Rect(100.0f, 100.0f, wnd_width, wnd_height);
        Vector2 bodiesScrollPosition;
        GUIStyle winStyle;

        public static void setSelectedBody(string name)
        {
            foreach (CelestialBody body in GameObject.FindObjectsOfType(typeof(CelestialBody)))
            {
                if (body.name == name)
                {
                    selectedBody = body;
                    return;
                }
            }
        }

        List<CelestialBody> getAllowableBodies(String filter = "ALL")
        {
            CelestialBody parent;
            bodiesList = new List<CelestialBody>();
            CelestialBody[] tmpBodies = GameObject.FindObjectsOfType(typeof(CelestialBody)) as CelestialBody[];

            foreach (CelestialBody body in GameObject.FindObjectsOfType(typeof(CelestialBody)))
            {
                // Log.Info("body name: " + body.name);
                if (body.orbit != null && body.orbit.referenceBody != null)
                {
                    parent = body.orbit.referenceBody;
                }
                else
                    parent = null;
                if (body.atmosphere)
                {
                    switch (filter)
                    {
                        case "ALL":
                            bodiesList.Add(body);
                            break;
                        case "PLANETS":
                            if (parent != null && parent == Sun.Instance.sun)
                                bodiesList.Add(body);
                            break;
                        case "MOONS":
                            if (parent != null && parent != Sun.Instance.sun)
                                bodiesList.Add(body);
                            break;
                        default:
                            bodiesList.Add(body);
                            break;
                    }
                }
            }
            return bodiesList;
        }

        void Start()
        {
            bodiesList = getAllowableBodies();
            isActive = true;
            GUI.color = new Color(0.85f, 0.85f, 0.85f, 1);

            winStyle = new GUIStyle(HighLogic.Skin.window);
            winStyle.active.background = winStyle.normal.background;
            Texture2D tex = winStyle.normal.background; //.CreateReadable();

            var pixels = tex.GetPixels32();
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i].a = 255;

            tex.SetPixels32(pixels); tex.Apply();

            winStyle.active.background = tex;
            winStyle.focused.background = tex;
            winStyle.normal.background = tex;
        }

        void OnDestroy()
        {
            isActive = false;
        }

        public void OnGUI()
        {
            planetWin.height = GraphWindow.wnd_rect.height;
            planetWin.x = GraphWindow.wnd_rect.x + GraphWindow.wnd_rect.width;
            planetWin.y = GraphWindow.wnd_rect.y;
            planetWin = ClickThruBlocker.GUILayoutWindow(565949, planetWin, planetSelWin, "Planetary Body Selection", winStyle);
        }

        void planetSelWin(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Selected planet:");
            GUILayout.TextField(selectedBody.name);
            GUILayout.EndHorizontal();
            bodiesScrollPosition = GUILayout.BeginScrollView(bodiesScrollPosition);
            foreach (CelestialBody body in bodiesList)
            {
                if (GUILayout.Button(body.name, GUILayout.Height(30)))
                {
                    selectedBody = body;

                    ResonantOrbitCalculator_Persistent.Instance.lastSelectedPlanet = body.name;

                    if (ResonantOrbitCalculator.Instance.graphWindow.autoUpdate && ResonantOrbitCalculator.Instance.graphWindow.shown)
                        ResonantOrbitCalculator.Instance.graphWindow.update_graphs();
                }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                Destroy(this);
            }
            GUI.DragWindow();
        }
    }
}