using System.IO;
using UnityEngine;


namespace ResonantOrbitCalculator
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class PlanetSelector : MonoBehaviour
    {
        public static string filePath { get { return KSPUtil.ApplicationRootPath + "GameData/ResonantOrbitCalculator/PluginData/Cache/"; } }
        bool hasRun = false;
        /// <summary>
        /// Generate the thumbnails and create the body wrappers
        /// </summary>
        public void FixedUpdate()
        {
            if (hasRun)
                return;
            hasRun = true;
            // Special code for Kopernicus
            // If Kopernicus is loaded, then CheckAndInitializeKopernicus will check to see if it is initialized
            // If it isn't, it sets the flag and then returns.
            // If it is, the continue through the thumbnail generation.  
            // At the end, finish the Kopernicus config
            if (Kopernicus.HasKopernicus())
            {
                if (Kopernicus.CheckAndInitializeKopernicus() == Kopernicus.KopernicusStatus.notInitialized)
                    return;
                if (Kopernicus.KopernicusDisabledConfigExists())
                    return;
            }
            System.IO.Directory.CreateDirectory(filePath);

            System.IO.FileInfo file = new System.IO.FileInfo(filePath);
            foreach (CelestialBody body in PSystemManager.Instance.localBodies)
            {
                string fileName = filePath + body.name + ".png";
                if (!File.Exists(fileName))
                {
                    Log.Info("PlanetSelector, generating thumbnail");
                    Texture2D thumb = GetPlanetThumbnail(body);
                    byte[] bytes = thumb.EncodeToPNG();
                    File.WriteAllBytes(fileName, bytes);
                    Destroy(thumb);
                }
            }
            if (Kopernicus.HasKopernicus())
            {
                Kopernicus.FinishKopernicusConfig();
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
#if false
            ScaledSpaceOnDemand od = body.scaledBody.GetComponent<ScaledSpaceOnDemand>();
            Boolean isLoaded = true;
            if (od != null)
            {
                isLoaded = od.isLoaded;
                if (!isLoaded)
                {
                    od.LoadTextures();
                }
            }
#endif
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.GetComponentInChildren<MeshFilter>().sharedMesh =
                body.scaledBody.GetComponent<MeshFilter>().sharedMesh;
            sphere.GetComponentInChildren<MeshRenderer>().sharedMaterial =
                body.scaledBody.GetComponent<MeshRenderer>().sharedMaterial;

            Texture2D finalTexture = RuntimePreviewGenerator.GenerateModelPreview(sphere.transform, 512, 512);
            UnityEngine.Object.DestroyImmediate(sphere);

#if false
            if (!isLoaded)
            {
                od.UnloadTextures();
            }
#endif
            return finalTexture;
        }
    }
}
