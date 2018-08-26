using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ResonantOrbitCalculator
{

    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class CCOLParams : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Resonant Orbit Calculator"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Resonant Orbit Calculator"; } }
        public override string DisplaySection { get { return "Resonant Orbit Calculator"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("Show planet image",
            toolTip ="Show an image of the planet at the center")]
        public bool showPlanetImage = true;

        [GameParameters.CustomParameterUI("Tooltips",
            toolTip ="Show tooltips")]
        public bool tooltips = true;

        [GameParameters.CustomParameterUI("Use last selected planet")]
        public bool useLastPlanet = true;



        public override void SetDifficultyPreset(GameParameters.Preset preset)
        { }
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return true;
        }
        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }

}
