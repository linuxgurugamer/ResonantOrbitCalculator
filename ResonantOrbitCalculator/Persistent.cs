
using UnityEngine;


//using System.Diagnostics;
using Upgradeables;

#if true
namespace ResonantOrbitCalculator
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {
        GameScenes.SPACECENTER,
        GameScenes.EDITOR,
        GameScenes.FLIGHT,
        GameScenes.TRACKSTATION,
        GameScenes.SPACECENTER
     })]
    public class ResonantOrbitCalculator_Persistent : ScenarioModule
    {
        static public ResonantOrbitCalculator_Persistent Instance;

        [KSPField(isPersistant = true)]
        public string lastSelectedPlanet = "";

        override public void OnAwake()
        {
            Instance = this;
        }

        void Start()
        {
            Debug.Log("CorrectCoL_Persistent.Start");
            if (HighLogic.CurrentGame.Parameters.CustomParams<ROCParams>().useLastPlanet)
                if (lastSelectedPlanet != "")
                    PlanetSelection.setSelectedBody(lastSelectedPlanet);
        }

    }
}

#endif