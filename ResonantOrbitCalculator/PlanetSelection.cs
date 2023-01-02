using System;
using System.Collections.Generic;
using UnityEngine;
using ClickThroughFix;
using Object = UnityEngine.Object;


namespace ResonantOrbitCalculator
{

    public class PlanetSelection : MonoBehaviour
    {
        static public CelestialBody selectedBody = Planetarium.fetch.Home;
        static public Texture2D planetImg = null;

        List<CelestialBody> bodiesList = null;
        public static bool isActive = false;
        const int wnd_width = 200;
        const int wnd_height = GraphWindow.wnd_height;
        Rect planetWin = new Rect(100.0f, 100.0f, wnd_width, wnd_height);
        Vector2 bodiesScrollPosition;
        GUIStyle winStyle;

        static void loadBodyImage(string name)
        {
            if (planetImg != null)
                Object.Destroy(planetImg);
            Log.Info("PlanetSelction.loadBodyImage: " + name);
            planetImg = LoadPNG(PlanetSelector.filePath + selectedBody.name + ".png");
        }

        public static void setSelectedBody(string name)
        {
            foreach (CelestialBody body in GameObject.FindObjectsOfType(typeof(CelestialBody)))
            {
                if (body.name == name)
                {
                    selectedBody = body;
                    loadBodyImage(selectedBody.name);
                    ResonantOrbitCalculator_Persistent.Instance.lastSelectedPlanet = name;
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
                if (body.orbit != null && body.orbit.referenceBody != null)
                {
                    parent = body.orbit.referenceBody;
                }
                else
                    parent = null;
                //if (body.atmosphere)
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

            tex.SetPixels32(pixels);
            tex.Apply();

            winStyle.active.background = tex;
            winStyle.focused.background = tex;
            winStyle.normal.background = tex;

            loadBodyImage(selectedBody.name);
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
            ClickThruBlocker.GUILayoutWindow(565949, planetWin, planetSelWin, "Planetary Body Selection", winStyle);
        }

        void planetSelWin(int id)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Selected planet:");
                GUILayout.TextField(selectedBody.name);
            }
            bodiesScrollPosition = GUILayout.BeginScrollView(bodiesScrollPosition);
            foreach (CelestialBody body in bodiesList)
            {
                if (GUILayout.Button(body.name, GUILayout.Height(20)))
                {
                    selectedBody = body;

                    ResonantOrbitCalculator_Persistent.Instance.lastSelectedPlanet = body.name;

                    if (ResonantOrbitCalculator.Instance.graphWindow.shown)
                    {
                        loadBodyImage(selectedBody.name);

                        ResonantOrbitCalculator.Instance.graphWindow.UpdateGraph();
                    }
                }
            }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                Destroy(this);
            }
            GUI.DragWindow();
        }
        internal void DestroyThis()
        {
            Destroy(this);
        }

        static Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (System.IO.File.Exists(filePath))
            {
#if true
                fileData = System.IO.File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
#else
                var _imagetex = new WWW(filePath);
                tex = _imagetex.texture;
                _imagetex.Dispose();
#endif

            }
            return tex;
        }


    }
}