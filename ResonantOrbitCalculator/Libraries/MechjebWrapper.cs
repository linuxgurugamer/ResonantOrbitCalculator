using System;

using System.Reflection;

namespace ResonantOrbitCalculator
{
    public class MechjebWrapper
    {
        System.Type CoreType;

        bool available = false;
        public bool Available
        {
            get { if (!Initialized) available = init(); return available; }
            set { available = value; }
        }
        public bool Initialized = false;
        public static Vessel vessel { get { return FlightGlobals.ActiveVessel; } }
        PartModule core = null;

        bool GetCore()
        {
            foreach (Part p in vessel.parts)
            {
                foreach (PartModule module in p.Modules)
                {
                    if (module.GetType() == CoreType)
                    {
                        core = module;
                        return true;
                    }
                }

            }
            return false;
        }

        System.Type FindMechJebModule(string module)
        {
            Log.Info("FindMechJebModule");

            Type type = null;
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>
            {
                if (t.FullName == module)
                {
                    type = t;
                }
            });

            return type;
        }

        public bool init()
        {
            Log.Info("MechjebWrapper.init");
            if (Initialized)
                return Available;
            CoreType = FindMechJebModule("MuMech.MechJebCore");

            if (CoreType == null)
            {
                Log.Info("MechJeb assembly not found");
                return false;
            }
            if (!GetCore())
            {
                Log.Info("MechJeb core not found");
                return false;
            }
            Log.Info("Found MechJeb core");
            Initialized = true;
            Available = true;
            return true;
        }

        public void ExecuteNode()
        {
            var coreNodeInfo = CoreType.GetField("node");
            var coreNode = coreNodeInfo.GetValue(core);
            var NodeExecute = coreNode.GetType().GetMethod("ExecuteOneNode", BindingFlags.Public | BindingFlags.Instance);
            NodeExecute.Invoke(coreNode, new object[] { this });
        }



    }
}
