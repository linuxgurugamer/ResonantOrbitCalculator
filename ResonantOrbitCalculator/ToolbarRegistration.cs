using UnityEngine;
using ToolbarControl_NS;

namespace ResonantOrbitCalculator
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(ResonantOrbitCalculator.MODID, ResonantOrbitCalculator.MODNAME);
        }
    }
}