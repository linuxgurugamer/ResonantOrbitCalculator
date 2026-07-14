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

        [GameParameters.CustomParameterUI("#LOC_ROC_ShowPlanetImage", toolTip = "#LOC_ROC_ShowPlanetImageTip")]
        public bool showPlanetImage = true;

        [GameParameters.CustomParameterUI("#LOC_ROC_Tooltips", toolTip = "#LOC_ROC_TooltipsTip")]
        public bool tooltips = true;

        [GameParameters.CustomParameterUI("#LOC_ROC_UseAlternateSkin")]
        public bool useAlternateSkin = false;

        [GameParameters.CustomParameterUI("#LOC_ROC_HideWhenPaused")]
        public bool hideWhenPaused = false;

        [GameParameters.CustomParameterUI("#LOC_ROC_UseLastPlanet")]
        public bool useLastPlanet = true;

        [GameParameters.CustomParameterUI("#LOC_ROC_EditorSoiWhite", toolTip = "#LOC_ROC_EditorSoiWhiteTip")]
        public bool editorSOIWhite = true;

        [GameParameters.CustomParameterUI("#LOC_ROC_FlightSoiWhite", toolTip = "#LOC_ROC_FlightSoiWhiteTip")]
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
