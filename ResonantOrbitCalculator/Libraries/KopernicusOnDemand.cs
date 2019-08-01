using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace ResonantOrbitCalculator
{
    public static class KopernicusOnDemand
    {
        public static Type ScaledSpaceOnDemandType { get; private set; }
        public static Component OnDemandComponent { get; private set; }
        public static bool TypeExists { get; private set; }
        public static bool ODComponentExists { get; private set; }
        public static bool ManuellyLoaded { get; private set; }

        public static bool TextureIsLoaded
        {
            get
            {
                return (bool)FieldIsLoaded.GetValue(OnDemandComponent);
            }
        }

        private static MethodInfo MethodLoadTextures;
        private static MethodInfo MethodUnloadTextures;
        private static FieldInfo FieldIsLoaded;

        static KopernicusOnDemand()
        {
            //Grab the ScalesSpaceOnDemand Type
            ScaledSpaceOnDemandType = AssemblyLoader.loadedAssemblies.
                Select(x => x.assembly.GetExportedTypes()).
                SelectMany(t => t).
                FirstOrDefault(t => t.FullName == "Kopernicus.OnDemand.ScaledSpaceOnDemand");

            //Without the ScaledSpaceOnDemandType, Kopernicus is not installed
            if (ScaledSpaceOnDemandType != null)
            {
                Log.Info("Found ScaledSpaceOnDemandType");
                TypeExists = true;

                //Once we got the Type, we can grab the required Methods and Fields as well
                DefineMethods();
                DefineFields();
            }
        }

        public static bool BodyHasComponent(CelestialBody body)
        {
            OnDemandComponent = body.scaledBody.GetComponent(ScaledSpaceOnDemandType);
            //note to myself: don't try to print out the name of the component for debugging purposes when it can be null...
            if (OnDemandComponent != null)
            {
                return true;
            }
            return false;
        }

        public static void LoadTexturesOnDemand()
        {
            MethodLoadTextures.Invoke(OnDemandComponent, null);
            ManuellyLoaded = true;
        }

        public static void UnloadTexturesOnDemand()
        {
            MethodUnloadTextures.Invoke(OnDemandComponent, null);
            ManuellyLoaded = false;
        }

        private static void DefineMethods()
        {
            MethodLoadTextures = ScaledSpaceOnDemandType.GetMethod("LoadTextures");
            MethodUnloadTextures = ScaledSpaceOnDemandType.GetMethod("UnloadTextures");
        }

        private static void DefineFields()
        {
            FieldIsLoaded = ScaledSpaceOnDemandType.GetField("isLoaded");
        }
    }
}
