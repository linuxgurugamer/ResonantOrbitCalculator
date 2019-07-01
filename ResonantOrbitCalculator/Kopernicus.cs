using System;
using System.Linq;
using UnityEngine;



namespace ResonantOrbitCalculator
{
    public class Kopernicus
    {
        const string KOP_CONFIG = "disable_Kopernicus_onDemand.cfg";
        const string KOP_GENERATED = KOP_CONFIG + ".disabled";
        static string[] Config =  {
            "@Kopernicus:FINAL",
            "{",
            "	%useOnDemand = False",
            "	%useManualMemoryManagement = False",
            "}"
        };

        static bool kopChecked = false;
        static bool hasKopernicus = false;
        public static string dirPath { get { return KSPUtil.ApplicationRootPath + "GameData/ResonantOrbitCalculator/"; } }
        public static string filePath { get { return dirPath + KOP_CONFIG; } }
        public static string fileGenPath { get { return dirPath + KOP_GENERATED; } }

        public enum KopernicusStatus { notInitialized, preGeneration, postGeneration, error };

        // 
        //
        public static KopernicusStatus CheckAndInitializeKopernicus()
        {
            if (System.IO.File.Exists(filePath))
                return KopernicusStatus.preGeneration;

            if (System.IO.File.Exists(fileGenPath))
                return KopernicusStatus.postGeneration;

            if (WriteKopernicusConfig())
            {
                ClearSettingsFlag();
                return KopernicusStatus.notInitialized;
            }
            return KopernicusStatus.error;
        }

        public static void ClearSettingsFlag()
        {
            if (HighLogic.CurrentGame != null)
                HighLogic.CurrentGame.Parameters.CustomParams<ROCParams>().regenerateKopernicusImages = false;
        }
        public static bool HasKopernicus()
        {
            Log.Info("HasKopernicus, kopChecked: " + kopChecked + ", hasKopernicus: " + hasKopernicus);
            if (kopChecked)
                return hasKopernicus;

            AssemblyLoader.LoadedAssembly b = AssemblyLoader.loadedAssemblies.SingleOrDefault(a => a.assembly.GetName().Name == "Kopernicus");

            hasKopernicus = (b != null);
            kopChecked = true;
            Log.Info("hasKopernicus: " + hasKopernicus);
            return hasKopernicus;
        }


        public static bool WriteKopernicusConfig(bool showPopUp = true)
        {
            try
            {
                System.IO.File.WriteAllLines(filePath, Config);
                if (System.IO.File.Exists(fileGenPath))
                    System.IO.File.Delete(fileGenPath);
            }
            catch (Exception ex)
            {
                Log.Error("Error writing to file: " + filePath + ", error: " + ex.Message);
                return false;
            }
            if (showPopUp)
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "dialogName", "Resonant Orbit Calculator",
                    "Restart the game to complete the cache initialization for Resonant Orbit Calculator", "OK", true, HighLogic.UISkin);
            return true;
        }

        public static bool FinishKopernicusConfig()
        {
            try
            {
                System.IO.File.Move(filePath, fileGenPath);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Error renaming file: " + filePath + " to " + fileGenPath + ", error: " + ex.Message);
                return false;
            }
        }

        public static bool KopernicusConfigExists()
        {
            return System.IO.File.Exists(filePath);
        }
        public static bool KopernicusDisabledConfigExists()
        {
            return System.IO.File.Exists(fileGenPath);
        }
        public static bool DisableConfig()
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                    return FinishKopernicusConfig();
                return false;
            }
            catch (Exception ex)
            {
                Log.Error("Error renaming file: " + filePath + " to " + fileGenPath + ", error: " + ex.Message);
                return false;
            }
        }
    }
}
