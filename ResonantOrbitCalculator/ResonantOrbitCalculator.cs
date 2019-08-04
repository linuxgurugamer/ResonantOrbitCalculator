﻿/*
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
using System.IO;
using System.Collections;
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
        public GraphWindow graphWindow;
        internal MechjebWrapper mucore = new MechjebWrapper();

        static Texture2D upArrow;
        static Texture2D downArrow;
        internal GUIContent upContent;
        internal GUIContent downContent;

        void Start()
        {
            Instance = this;
            Debug.Log("[ResonantOrbitCalculator]: Starting!");
            
            graphWindow = new GraphWindow();
            // should be called once, so let's deserialize graph here too                
            graphWindow.Start();
            graphWindow.load_settings();
            graphWindow.init_textures();

            KACWrapper.InitKACWrapper();

            GameEvents.onHideUI.Add(this.HideUI);
            GameEvents.onShowUI.Add(this.ShowUI);
            GameEvents.onGamePause.Add(this.HideUIWhenPaused);
            GameEvents.onGameUnpause.Add(this.ShowUIwhenUnpaused);
            onAppLauncherLoad();

            if (upArrow == null)
            {
                upArrow = new Texture2D(2, 2);
                if (ToolbarControl.LoadImageFromFile(ref upArrow, KSPUtil.ApplicationRootPath + "GameData/ResonantOrbitCalculator/PluginData/Images/up"))
                    upContent = new GUIContent("", upArrow, "");
                else
                    upContent = new GUIContent("^", null, "");
            }
            if (downArrow == null)
            {
                downArrow = new Texture2D(2, 2);
                if (ToolbarControl.LoadImageFromFile(ref downArrow, KSPUtil.ApplicationRootPath + "GameData/ResonantOrbitCalculator/PluginData/Images/down"))
                    downContent = new GUIContent("", downArrow, "");
                else
                    downContent = new GUIContent("v", null, "");
            }

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

            GameEvents.onHideUI.Remove(this.HideUI);
            GameEvents.onShowUI.Remove(this.ShowUI);
            GameEvents.onGamePause.Remove(this.HideUIWhenPaused);
            GameEvents.onGameUnpause.Remove(this.ShowUIwhenUnpaused);

            graphWindow = null;
        }
        bool hideUI = false;
        bool hideUIpaused = false;

        public void HideUI()
        {
            hideUI = true;
        }

        void ShowUI()
        {
            hideUI = false;
        }

        public void HideUIWhenPaused()
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<ROCParams>().hideWhenPaused)
                hideUIpaused = true;
        }

  
        void ShowUIwhenUnpaused()
        {
            hideUIpaused = false;
        }

        
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
                 ApplicationLauncher.AppScenes.FLIGHT |
                 ApplicationLauncher.AppScenes.SPH | 
                 ApplicationLauncher.AppScenes.VAB | 
                 ApplicationLauncher.AppScenes.MAPVIEW,
                 MODID,
                "ResonantOrbitCalculatorButton",
                "ResonantOrbitCalculator/PluginData/Images/ResonantOrbit_38",
                "ResonantOrbitCalculator/PluginData/Images/ResonantOrbit_24",
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
            if (!hideUI && !hideUIpaused)
            {
                graphWindow.OnGUI();
                if (graphWindow.saveScreen)
                {
                    graphWindow.saveScreen = false;
                    StartCoroutine(ScreenshotEncode());
                }
            }
        }

        const string fname = "ResonantOrbit";
        IEnumerator ScreenshotEncode()
        {
            // wait for graphics to render
            yield return new WaitForEndOfFrame();

            Rect pixelRect = GraphWindow.wnd_rect; // copy values

            pixelRect.y = Mathf.Max(Screen.height - GraphWindow.wnd_rect.y - GraphWindow.wnd_rect.height, 0f);

            Texture2D captureTex = new Texture2D(Mathf.CeilToInt(pixelRect.width), Mathf.CeilToInt(pixelRect.height));

            captureTex.ReadPixels(pixelRect, 0, 0, false);

            byte[] bytes = captureTex.EncodeToPNG();
            string filePath = KSPUtil.ApplicationRootPath + "Screenshots/";;
            FileInfo file = new System.IO.FileInfo(filePath);

   
            int i = 0;
            while (File.Exists(filePath + fname + i.ToString() + ".png"))
                i++;
            File.WriteAllBytes(filePath + fname + i.ToString() + ".png", bytes);
    
            DestroyObject(captureTex);
        }

    }
}
