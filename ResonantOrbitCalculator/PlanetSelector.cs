using System.IO;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;


namespace ResonantOrbitCalculator
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class PlanetSelector : MonoBehaviour
    {
        public static string filePath { get { return KSPUtil.ApplicationRootPath + "GameData/ResonantOrbitCalculator/PluginData/Cache/"; } }
        bool hasRun = false;
        static bool runningKopernicus = false;

        /// <summary>
        /// Generate the thumbnails and create the body wrappers
        /// </summary>
        public void FixedUpdate()
        {
            if (hasRun)
                return;
            hasRun = true;

            System.IO.Directory.CreateDirectory(filePath);
            System.IO.FileInfo file = new System.IO.FileInfo(filePath);

            if (KopernicusOnDemand.TypeExists)
            {
                Log.Info("Kopernicus is running");
                runningKopernicus = true;
            }

            foreach (CelestialBody body in PSystemManager.Instance.localBodies)
            {
                string fileName = filePath + body.name + ".png";
                if (!File.Exists(fileName))
                {
                    Log.Info("PlanetSelector, generating thumbnail for " + body.name);
                    Texture2D thumb = GetPlanetThumbnail(body);
                    if (IsOverexposed(thumb))
                    {
                        RuntimePreviewGenerator.AdjustLighting = true;
                        thumb = GetPlanetThumbnail(body);
                    }
                    byte[] bytes = thumb.EncodeToPNG();
                    File.WriteAllBytes(fileName, bytes);
                    Destroy(thumb);
                }
            }
        }

        /// <summary>
        /// Generates a thumbnail for the planet
        /// </summary>
        public static Texture2D GetPlanetThumbnail(CelestialBody body)
        {
            // Config
            RuntimePreviewGenerator.TransparentBackground = true;
            RuntimePreviewGenerator.BackgroundColor = Color.clear;
            RuntimePreviewGenerator.PreviewDirection = Vector3.forward;
            RuntimePreviewGenerator.Padding = -0.15f;

            if (runningKopernicus && KopernicusOnDemand.BodyHasComponent(body))
            {
                Log.Info("Found Kopernicus and OnDemand Component");
                if (!KopernicusOnDemand.TextureIsLoaded)
                {
                    KopernicusOnDemand.LoadTexturesOnDemand();
                }
            }

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponentInChildren<MeshFilter>().sharedMesh = body.scaledBody.GetComponent<MeshFilter>().sharedMesh;
            sphere.GetComponentInChildren<MeshRenderer>().sharedMaterial = body.scaledBody.GetComponent<MeshRenderer>().sharedMaterial;

            Texture2D finalTexture = RuntimePreviewGenerator.GenerateModelPreview(sphere.transform, 512, 512);
            UnityEngine.Object.DestroyImmediate(sphere);
            
            if (runningKopernicus && KopernicusOnDemand.ManuellyLoaded)
            {
                KopernicusOnDemand.UnloadTexturesOnDemand();
            }

            return finalTexture;
        }

        private bool IsOverexposed(Texture2D texture)
        {
            int i = 0;
            Color[] toTest = texture.GetPixels();
            foreach (Color color in toTest)
            {
                if (color == Color.white)
                {
                    i++;
                }
            }

            Log.Info($"White pixels: {i}");
            if (i > 15000) //empirical value, need to be at least >10000 and <22000
            {
                return true;
            }
            return false;
        }
    }
}
