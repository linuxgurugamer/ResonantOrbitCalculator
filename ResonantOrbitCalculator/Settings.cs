using System.Collections;
using System.Reflection;

namespace ResonantOrbitCalculator
{
    public class ROCParams : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Loc.Tag("ModName"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return Loc.Tag("ModName"); } }
        public override string DisplaySection { get { return Loc.Tag("ModName"); } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        [GameParameters.CustomParameterUI("#LOC_ROC_ShowPlanetImage")]
        public bool showPlanetImage = true;

        [GameParameters.CustomParameterUI("#LOC_ROC_UseAlternateSkin")]
        public bool useAlternateSkin = false;

        [GameParameters.CustomParameterUI("#LOC_ROC_HideWhenPaused")]
        public bool hideWhenPaused = false;

        [GameParameters.CustomParameterUI("#LOC_ROC_UseLastPlanet")]
        public bool useLastPlanet = true;

        [GameParameters.CustomParameterUI("#LOC_ROC_EditorSoiWhite")]
        public bool editorSOIWhite = true;

        [GameParameters.CustomParameterUI("#LOC_ROC_FlightSoiWhite")]
        public bool flightSOIWhite = false;

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        { }

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
