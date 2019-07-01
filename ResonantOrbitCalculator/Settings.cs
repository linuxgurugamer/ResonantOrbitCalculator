using System.Collections;
using System.Reflection;

namespace ResonantOrbitCalculator
{

    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class ROCParams : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Resonant Orbit Calculator"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Resonant Orbit Calculator"; } }
        public override string DisplaySection { get { return "Resonant Orbit Calculator"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("Show planet image",
            toolTip = "Show an image of the planet at the center")]
        public bool showPlanetImage = true;

        [GameParameters.CustomParameterUI("Tooltips",
            toolTip = "Show tooltips")]
        public bool tooltips = true;

        [GameParameters.CustomParameterUI("Use alternate skin")]
        public bool useAlternateSkin = false;

        [GameParameters.CustomParameterUI("Hide UI when paused")]
        public bool hideWhenPaused = false;

        [GameParameters.CustomParameterUI("Use last selected planet")]
        public bool useLastPlanet = true;

        [GameParameters.CustomParameterUI("Editor SOI white",
            toolTip = "If false, the background in the editor will be a dark grey")]
        public bool editorSOIWhite = true;

        [GameParameters.CustomParameterUI("Flight SOI white",
            toolTip = "If false, the background in flight will be dark grey")]
        public bool flightSOIWhite = false;

        [GameParameters.CustomParameterUI("Regenerate Kopernicus Images",
        toolTip = "If true, will create a config file to disable the Kopernicus's mod\n" +
                  "'useOnDemand' and 'useManualMemoryManagement'\n" +
                  "options at the next boot.  This will be reset once\n" +
                  "the new images have been generated")]
        public bool regenerateKopernicusImages = false;


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        { }
        bool firstTime = true;
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "regenerateKopernicusImages" && !Kopernicus.HasKopernicus())
                return false;
            if (firstTime)
            {
                if (Kopernicus.KopernicusDisabledConfigExists())
                {
                    regenerateKopernicusImages = false;
                }
                else
                    regenerateKopernicusImages = true;
                firstTime = false;
            }
            if (regenerateKopernicusImages)
            {
                Kopernicus.WriteKopernicusConfig(false);
            }
            else
            {
                Kopernicus.DisableConfig();
            }
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
