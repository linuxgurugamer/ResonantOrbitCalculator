using System;
using System.Linq;
using UnityEngine;
using ClickThroughFix;

namespace ResonantOrbitCalculator
{
    public partial class GraphWindow
    {
        public const int RIGHT_PANEL_WIDTH = 380;
        public const int PANEL_GAP = 20;
        public const int MODERN_WND_WIDTH = GRAPH_WIDTH + RIGHT_PANEL_WIDTH + PANEL_GAP;
        const int MinLabelColWidth = 118;
        static int labelColWidth = MinLabelColWidth;
        const int FieldWidth = 110;
        const int RowSpacing = 10;
        const int ParamRowSpacing = 3;
        const int UnitButtonWidth = 36;
        static float rowControlHeight;

        bool needsLayoutRecalc = true;
        bool pendingGraphUpdate = false;

        static GUIStyle rowLabelStyle;
        static GUIStyle rowValueStyle;
        static GUIStyle panelButtonStyle;
        static GUIStyle optionButtonStyle;
        static GUIStyle optionButtonSelectedStyle;
        static GUIStyle optionButtonCyanStyle;
        static GUIStyle optionButtonCyanSelectedStyle;
        static GUIStyle optionButtonWarningStyle;
        static GUIStyle optionButtonWarningSelectedStyle;
        static readonly Color OptionSelectedColor = new Color(0.55f, 1f, 0.55f);
        static GUIStyle sectionBoxStyle;
        static GUIStyle headerTitleStyle;
        static GUIStyle headerMetricStyle;
        static GUIStyle unitButtonStyle;
        static GUIStyle unitsLabelStyle;
        static GUIStyle suffixLabelStyle;

        static internal GUIStyle CreateWindowStyle(GUISkin skin)
        {
            var style = new GUIStyle(skin.window);
            Texture2D source = skin.window.normal.background;
            if (source == null)
                return style;

            var tex = UnityEngine.Object.Instantiate(source);
            var pixels = tex.GetPixels32();
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i].a = 255;
            tex.SetPixels32(pixels);
            tex.Apply();

            style.normal.background = tex;
            style.active.background = tex;
            style.focused.background = tex;
            style.onNormal.background = tex;
            return style;
        }

        static bool? stylesForAlternateSkin;

        static internal GUISkin GetGuiSkin(bool useAlternateSkin)
        {
            if (useAlternateSkin)
                return UnitySkinCapture.Skin ?? HighLogic.Skin;
            return HighLogic.Skin;
        }

        static internal void EnsureGuiStyles(bool useAlternateSkin)
        {
            if (stylesForAlternateSkin.HasValue && stylesForAlternateSkin.Value == useAlternateSkin && rowLabelStyle != null)
                return;

            stylesForAlternateSkin = useAlternateSkin;
            InitGuiStylesFromSkin(GetGuiSkin(useAlternateSkin));
        }

        internal void RequestLayoutRecalc()
        {
            needsLayoutRecalc = true;
        }

        internal void RequestGraphUpdate()
        {
            pendingGraphUpdate = true;
        }

        static void StabilizeButtonStyle(GUIStyle style)
        {
            style.hover.background = style.normal.background;
            style.active.background = style.normal.background;
            style.focused.background = style.normal.background;
            style.onHover.background = style.onNormal.background;
            style.onActive.background = style.onNormal.background;
            style.onFocused.background = style.onNormal.background;
        }

        static GUIStyle CreatePanelButtonStyle(GUISkin skin)
        {
            return new GUIStyle(skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = rowControlHeight,
                padding = new RectOffset(10, 8, 4, 4),
                margin = new RectOffset(0, 0, 0, 0),
                clipping = TextClipping.Clip,
                wordWrap = false
            };
        }

        static GUIStyle CloneButtonWithAccent(GUIStyle baseStyle, Color? normalTextColor, bool bold)
        {
            var style = new GUIStyle(baseStyle)
            {
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal
            };
            if (normalTextColor.HasValue)
                style.normal.textColor = normalTextColor.Value;
            return style;
        }

        static int ComputeLabelColWidth(GUIStyle labelStyle)
        {
            float max = MinLabelColWidth;
            void Consider(string text)
            {
                max = Mathf.Max(max, labelStyle.CalcSize(new GUIContent(text)).x);
            }
            Consider(Loc.T("NumSats", "Number of satellites:"));
            Consider(Loc.T("OrbitalPeriod", "Orbital Period: "));
            Consider(Loc.T("Altitude", "Altitude:"));
            Consider(Loc.T("Apoapsis", "Apoapsis:"));
            Consider(Loc.T("Periapsis", "Periapsis:"));
            Consider(Loc.T("InjectionDv", "Injection Δv:"));
            Consider(Loc.T("LosLength", "LOS Length:"));
            Consider(Loc.T("Atm", "Atm:"));
            Consider(Loc.T("Vac", "Vac:"));
            return Mathf.CeilToInt(max) + 4;
        }

        static void InitGuiStylesFromSkin(GUISkin skin)
        {
            rowControlHeight = Mathf.Max(skin.toggle.fixedHeight, skin.textField.fixedHeight, skin.button.fixedHeight, 24f);

            normalLabel = new GUIStyle(skin.label) { wordWrap = false, alignment = TextAnchor.MiddleLeft };

            warningLabel = new GUIStyle(normalLabel);
            warningLabel.normal.textColor = Color.red;
            warningLabel.normal.background = new Texture2D(2, 2);

            headerLabel = new GUIStyle(normalLabel);
            headerLabel.normal.textColor = Color.white;

            int size = 4;
            Color[] pix = new Color[size];
            for (int i = 0; i < size; i++)
                pix[i] = Color.yellow;
            warningLabel.normal.background.SetPixels(pix);
            warningLabel.normal.background.Apply();

            panelButtonStyle = CreatePanelButtonStyle(skin);
            optionButtonStyle = panelButtonStyle;
            optionButtonSelectedStyle = CloneButtonWithAccent(panelButtonStyle, OptionSelectedColor, true);
            optionButtonCyanStyle = CloneButtonWithAccent(panelButtonStyle, Color.cyan, false);
            optionButtonCyanSelectedStyle = CloneButtonWithAccent(panelButtonStyle, Color.cyan, true);
            optionButtonWarningStyle = CloneButtonWithAccent(panelButtonStyle, Color.red, false);
            optionButtonWarningSelectedStyle = CloneButtonWithAccent(panelButtonStyle, Color.red, true);

            rowLabelStyle = new GUIStyle(normalLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = rowControlHeight
            };
            labelColWidth = ComputeLabelColWidth(rowLabelStyle);
            rowValueStyle = new GUIStyle(normalLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Overflow,
                fixedHeight = rowControlHeight
            };
            suffixLabelStyle = new GUIStyle(rowValueStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedWidth = 14f
            };
            unitsLabelStyle = new GUIStyle(rowLabelStyle)
            {
                fixedWidth = 48f
            };
            unitButtonStyle = new GUIStyle(skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = rowControlHeight,
                fixedWidth = UnitButtonWidth,
                stretchWidth = false
            };
            StabilizeButtonStyle(unitButtonStyle);

            headerTitleStyle = new GUIStyle(skin.label)
            {
                wordWrap = false,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            headerMetricStyle = new GUIStyle(skin.label)
            {
                wordWrap = false,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow
            };

            sectionBoxStyle = new GUIStyle(skin.box) { padding = new RectOffset(8, 8, 8, 8) };

            labelResonantOrbit = new GUIStyle(skin.label);
            labelResonantOrbit.normal.textColor = Color.green;
            labelResonantOrbit.fontStyle = FontStyle.Bold;
            labelResonantOrbit.wordWrap = false;
            buttonRed = new GUIStyle(skin.button);
            buttonRed.normal.textColor = Color.red;
            buttonRed.hover.textColor = Color.red;

            textStyle = new GUIStyle(skin.textField) { alignment = TextAnchor.MiddleLeft, fixedHeight = rowControlHeight };
            textErrorStyle = new GUIStyle(skin.textField) { alignment = TextAnchor.MiddleLeft, fixedHeight = rowControlHeight };
            textErrorStyle.normal.textColor = Color.red;
            textErrorStyle.hover.textColor = Color.red;
            textErrorStyle.focused.textColor = Color.red;

        }
        void ProcessPendingUpdates()
        {
            if (!pendingGraphUpdate || Event.current.type != EventType.Repaint)
                return;
            pendingGraphUpdate = false;
            UpdateGraph();
        }

        bool layoutOcclusionModifiers;
        bool layoutKacHints;
        bool layoutFlightTargetOrbits;
        bool layoutFlightManeuvers;
        SelectedOrbit layoutSelectedOrbit;
        bool layoutCreateNodeAp;
        bool layoutCreateNodePe;
        bool layoutClearNodes;
        bool layoutManeuverExtras;
        bool layoutKacAlarmsButton;
        bool layoutMechJebButton;

        static float OcclusionRowsExtraHeight => (rowControlHeight + RowSpacing) * 2f;

        static void DrawParamRow(string label, string value, GUIStyle valueStyle = null, bool trailingSpace = true)
        {
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight)))
            {
                GUILayout.Label(label, rowLabelStyle, GUILayout.Width(labelColWidth));
                GUILayout.Label(value, valueStyle ?? rowValueStyle, GUILayout.ExpandWidth(true));
            }
            if (trailingSpace)
                GUILayout.Space(ParamRowSpacing);
        }

        enum OptionButtonAccent { Default, Cyan, Warning }

        static GUIStyle GetOptionButtonStyle(bool selected, OptionButtonAccent accent)
        {
            if (selected)
            {
                switch (accent)
                {
                    case OptionButtonAccent.Cyan: return optionButtonCyanSelectedStyle;
                    case OptionButtonAccent.Warning: return optionButtonWarningSelectedStyle;
                    default: return optionButtonSelectedStyle;
                }
            }
            switch (accent)
            {
                case OptionButtonAccent.Cyan: return optionButtonCyanStyle;
                case OptionButtonAccent.Warning: return optionButtonWarningStyle;
                default: return optionButtonStyle;
            }
        }

        bool DrawPanelButton(GUIContent content)
        {
            return GUILayout.Button(content, panelButtonStyle, GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight));
        }

        bool DrawOptionButton(bool selected, GUIContent content, OptionButtonAccent accent = OptionButtonAccent.Default)
        {
            var style = GetOptionButtonStyle(selected, accent);
            return GUILayout.Button(content, style, GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight));
        }

        bool DrawBoolOption(ref bool value, GUIContent content, OptionButtonAccent accent = OptionButtonAccent.Default)
        {
            if (!DrawOptionButton(value, content, accent))
                return false;
            value = !value;
            return true;
        }

        bool DrawOrbitPresetRadio(SelectedOrbit orbit, GUIContent content, ref SelectedOrbit selectedOrbit,
            OptionButtonAccent accent = OptionButtonAccent.Default)
        {
            bool wasSelected = selectedOrbit == orbit;
            if (!DrawOptionButton(wasSelected, content, accent))
                return false;
            if (wasSelected)
                return false;
            selectedOrbit = orbit;
            return true;
        }

        static void DrawUnitsRow(ref bool draw)
        {
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight)))
            {
                GUILayout.Label(Loc.T("Units", "Units:"), unitsLabelStyle);
                bool m1 = GUILayout.Toggle(OrbitCalc.units == OrbitCalc.Units.m, "m", unitButtonStyle);
                if (m1 && OrbitCalc.units != OrbitCalc.Units.m)
                {
                    OrbitCalc.units = OrbitCalc.Units.m;
                    sOrbitAltitude = orbitAltitude.ToString("F0");
                    draw = true;
                }
                bool km1 = GUILayout.Toggle(OrbitCalc.units == OrbitCalc.Units.km, "km", unitButtonStyle);
                if (km1 && OrbitCalc.units != OrbitCalc.Units.km)
                {
                    OrbitCalc.units = OrbitCalc.Units.km;
                    sOrbitAltitude = (orbitAltitude / 1000).ToString("F3");
                    draw = true;
                }
                bool Mm1 = GUILayout.Toggle(OrbitCalc.units == OrbitCalc.Units.Mm, "Mm", unitButtonStyle);
                if (Mm1 && OrbitCalc.units != OrbitCalc.Units.Mm)
                {
                    OrbitCalc.units = OrbitCalc.Units.Mm;
                    sOrbitAltitude = (orbitAltitude / 1000000).ToString("F3");
                    draw = true;
                }
                bool Gm1 = GUILayout.Toggle(OrbitCalc.units == OrbitCalc.Units.Gm, "Gm", unitButtonStyle);
                if (Gm1 && OrbitCalc.units != OrbitCalc.Units.Gm)
                {
                    OrbitCalc.units = OrbitCalc.Units.Gm;
                    sOrbitAltitude = (orbitAltitude / 1000000000).ToString("F3");
                    draw = true;
                }
            }
        }

        static void DrawGraphHeader()
        {
            string title = string.IsNullOrEmpty(OrbitCalc.header[0]) ? " " : OrbitCalc.header[0];
            GUILayout.Label(title, headerTitleStyle, GUILayout.ExpandWidth(true));

            string metrics = string.IsNullOrEmpty(OrbitCalc.carrierAp)
                ? " "
                : "Ap " + OrbitCalc.carrierAp + "   Pe " + OrbitCalc.carrierPe + "   Δv " + OrbitCalc.burnDV;
            GUILayout.Label(metrics, headerMetricStyle, GUILayout.ExpandWidth(true));
        }
        void SyncLayoutState()
        {
            if (Event.current.type != EventType.Layout)
                return;

            if (occlusionModifiers != layoutOcclusionModifiers)
            {
                wnd_rect.height += occlusionModifiers ? OcclusionRowsExtraHeight : -OcclusionRowsExtraHeight;
                if (wnd_rect.height < wnd_height)
                    wnd_rect.height = wnd_height;
            }
            layoutOcclusionModifiers = occlusionModifiers;
            layoutKacHints = KACWrapper.APIReady;
            layoutFlightTargetOrbits = HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != null
                && FlightGlobals.activeTarget != null && FlightGlobals.activeTarget.orbit != null;
            layoutFlightManeuvers = HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != null
                && FlightGlobals.ActiveVessel.patchedConicsUnlocked();
            layoutSelectedOrbit = selectedOrbit;
            layoutCreateNodeAp = layoutFlightManeuvers && layoutSelectedOrbit == SelectedOrbit.Ap;
            layoutCreateNodePe = layoutFlightManeuvers && layoutSelectedOrbit == SelectedOrbit.Pe;
            layoutClearNodes = layoutFlightManeuvers && FlightGlobals.ActiveVessel != null
                && FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes.Count() > 0;
            layoutManeuverExtras = layoutClearNodes
                && (layoutSelectedOrbit == SelectedOrbit.Ap || layoutSelectedOrbit == SelectedOrbit.Pe);
            layoutKacAlarmsButton = layoutManeuverExtras && KACWrapper.APIReady;
            layoutMechJebButton = layoutManeuverExtras && ResonantOrbitCalculator.Instance.mucore.Available;
        }

        void _drawModernGUI(int id)
        {
            if (needsLayoutRecalc && Event.current.type == EventType.Layout)
            {
                needsLayoutRecalc = false;
                if (wnd_rect.height < wnd_height)
                    wnd_rect.height = wnd_height;
            }
            SyncLayoutState();

            UpArrow = ResonantOrbitCalculator.upContent;
            DownArrow = ResonantOrbitCalculator.downContent;
            bool draw = false;

            using (new GUILayout.VerticalScope(GUILayout.Width(MODERN_WND_WIDTH)))
            {
            using (new GUILayout.HorizontalScope(GUILayout.Width(MODERN_WND_WIDTH)))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(GRAPH_WIDTH)))
                {
                    DrawGraphHeader();
                    GUILayout.Space(4);

                    GUILayout.Box(graph_texture, GUILayout.Width(GRAPH_WIDTH), GUILayout.Height(GRAPH_HEIGHT));
                    if (layoutKacHints)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label(Loc.T("KacHint1", "To use Kerbal Alarm Clock (KAC), select either Current Ap or Current Pe,"));
                        GUILayout.Label(Loc.T("KacHint2", "and then select from the options listed at the bottom"));
                    }
                }

                GUILayout.Space(PANEL_GAP);

                // draw side text
                using (new GUILayout.VerticalScope(GUILayout.Width(RIGHT_PANEL_WIDTH)))
                {

                    if (firstTime)
                    {
                        firstTime = false;
                        draw = true;
                    }
                    // if (!PlanetSelection.isActive)
                    {
                        if (DrawPanelButton(new GUIContent(Loc.T("SelectPlanet", "Select Planet"))))
                        {
                            if (!PlanetSelection.isActive)
                                planetSelection = new GameObject().AddComponent<PlanetSelection>();
                            else
                                planetSelection.DestroyThis();
                        }
                    }

                    GUILayout.Space(RowSpacing);
                    int butW = 19;
                    using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight)))
                    {
                        GUILayout.Label(Loc.Label("NumSats", "Number of satellites:"), rowLabelStyle, GUILayout.Width(labelColWidth));

                        var newsNumSats = GUILayout.TextField(sNumSats, textStyle, GUILayout.Width(FieldWidth));

                        if (GUILayout.Button(UpArrow, GUILayout.Width(butW), GUILayout.Height(rowControlHeight)))
                        {
                            numSats++;
                            sNumSats = numSats.ToString();
                            newsNumSats = sNumSats;
                            draw = true;
                        }
                        if (GUILayout.Button(DownArrow, GUILayout.Width(butW), GUILayout.Height(rowControlHeight)))
                        {
                            if (numSats > 1)
                                numSats--;
                            sNumSats = numSats.ToString();
                            newsNumSats = sNumSats;
                            draw = true;
                        }
                        if (sNumSats != newsNumSats)
                        {
                            bValidNumSats = int.TryParse(newsNumSats, out iTmp);
                            if (!bValidNumSats)
                                sNumSats = numSats.ToString();
                            else
                            {
                                numSats = iTmp;
                                sNumSats = newsNumSats;
                                draw = true;
                            }
                        }
                    }
                    GUILayout.Space(RowSpacing);
                    using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight)))
                    {
                        GUILayout.Label(Loc.Label("Altitude", "Altitude:"), rowLabelStyle, GUILayout.Width(labelColWidth));
                        GUI.SetNextControlName("altitude");
                        string newsOrbitAltitude = GUILayout.TextField(sOrbitAltitude, textStyle, GUILayout.ExpandWidth(true));
                        if (newsOrbitAltitude != sOrbitAltitude)
                        {
                            bValidOrbitAltitude = double.TryParse(newsOrbitAltitude, out dTmp);


                            if (bValidOrbitAltitude)
                            {
                                selectedOrbit = SelectedOrbit.None;
                                switch (OrbitCalc.units)
                                {
                                    case OrbitCalc.Units.m:
                                        sOrbitAltitude = dTmp.ToString("F0");
                                        break;
                                    case OrbitCalc.Units.km:
                                        sOrbitAltitude = dTmp.ToString("F3");
                                        dTmp *= 1000;
                                        break;
                                    case OrbitCalc.Units.Mm:
                                        sOrbitAltitude = dTmp.ToString("F3");
                                        dTmp *= 1000000;
                                        break;
                                    default:
                                        sOrbitAltitude = dTmp.ToString("F3");
                                        dTmp *= 1000000000;
                                        break;
                                }
                                orbitAltitude = dTmp;
                            }
                            draw = true;
                        }
                    }
                    GUILayout.Space(RowSpacing);
                    using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight)))
                    {
                        GUILayout.Label(Loc.T("OrbitalPeriod", "Orbital Period: "), rowLabelStyle, GUILayout.Width(labelColWidth));

                        GUI.SetNextControlName("periodHour");
                        string h = GUILayout.TextField(OrbitCalc.periodHour, bh ? textStyle : textErrorStyle, GUILayout.Width(36));
                        GUILayout.Label(Loc.T("HourSuffix", "h"), suffixLabelStyle);
                        GUI.SetNextControlName("periodMin");
                        string m = GUILayout.TextField(OrbitCalc.periodMin, bm ? textStyle : textErrorStyle, GUILayout.Width(36));
                        GUILayout.Label(Loc.T("MinSuffix", "m"), suffixLabelStyle);
                        GUI.SetNextControlName("periodSec");
                        string s = GUILayout.TextField(OrbitCalc.periodSec, bs ? textStyle : textErrorStyle, GUILayout.Width(52));
                        GUILayout.Label(Loc.T("SecSuffix", "s"), suffixLabelStyle);
                        OrbitCalc.periodEntry = GUI.GetNameOfFocusedControl() == "periodHour" ||
                            GUI.GetNameOfFocusedControl() == "periodMin" ||
                            GUI.GetNameOfFocusedControl() == "periodSec";


                        bh = Double.TryParse(h, out dh);
                        bm = Double.TryParse(m, out dm);
                        bs = Double.TryParse(s, out ds);
                        if (h != OrbitCalc.periodHour || m != OrbitCalc.periodMin || s != OrbitCalc.periodSec)
                        {
                            if (bh && bm && bs)
                            {
                                double T = dh * 3600 + dm * 60 + ds;

                                orbitAltitude = OrbitCalc.satelliteorbit.a(T);
                                sOrbitAltitude = FormatOrbitAltitude(orbitAltitude);
                                draw = true;
                            }
                            OrbitCalc.periodHour = h;
                            OrbitCalc.periodMin = m;
                            OrbitCalc.periodSec = s;
                        }
                    }

                    GUILayout.Space(RowSpacing);
                    using (new GUILayout.VerticalScope(sectionBoxStyle, GUILayout.ExpandWidth(true)))
                    {
                        if (!OrbitCalc.hasSyncOrbit)
                            GUI.enabled = false;
                        if (DrawOrbitPresetRadio(SelectedOrbit.Synchronous, new GUIContent(
                            Loc.F("SyncOrbit", "Synchronous orbit (<<1>>)", OrbitCalc.synchrorbit)),
                            ref selectedOrbit))
                        {
                            orbitAltitude = OrbitCalc.body.geoAlt;
                            sOrbitAltitude = orbitAltitude.ToString();
                            OrbitCalc.periodEntry = false;
                            draw = true;
                        }
                        GUI.enabled = true;

                        if (OrbitCalc.losorbit == "" || !OrbitCalc.hasLosOrbit)
                            GUI.enabled = false;
                        if (DrawOrbitPresetRadio(SelectedOrbit.MinLOS, new GUIContent(
                            Loc.F("MinLosOrbit", "Minimum LOS orbit (<<1>>)", OrbitCalc.losorbit)),
                            ref selectedOrbit,
                            OrbitCalc.losOrbitWarning ? OptionButtonAccent.Warning : OptionButtonAccent.Cyan))
                        {
                            orbitAltitude = OrbitCalc.minLOS;
                            sOrbitAltitude = orbitAltitude.ToString();
                            OrbitCalc.periodEntry = false;
                            draw = true;
                        }
                        GUI.enabled = true;

                        if (DrawBoolOption(ref showLOSlines, Loc.Label("ShowLosLines", "Show LOS lines")))
                        {
                            draw = true;
                        }

                        if (DrawBoolOption(ref occlusionModifiers, Loc.Label("OcclusionModifiers", "Occlusion modifiers")))
                        {
                            draw = true;
                        }
                        if (layoutOcclusionModifiers)
                        {
                            GUILayout.Space(RowSpacing);
                            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight)))
                            {
                                GUILayout.Label(Loc.Label("Atm", "Atm:"), rowLabelStyle, GUILayout.Width(labelColWidth));
                                var newsAtmOcclusion = GUILayout.TextField(sAtmOcclusion, textStyle, GUILayout.Width(FieldWidth));
                                if (GUILayout.Button(UpArrow, GUILayout.Width(butW), GUILayout.Height(rowControlHeight)))
                                {
                                    if (atmOcclusion < 1.1)
                                        atmOcclusion += 0.01f;
                                    sAtmOcclusion = atmOcclusion.ToString("F2");
                                    newsAtmOcclusion = sAtmOcclusion;
                                    draw = true;
                                }
                                if (GUILayout.Button(DownArrow, GUILayout.Width(butW), GUILayout.Height(rowControlHeight)))
                                {
                                    if (atmOcclusion > 0)
                                        atmOcclusion -= 0.01f;
                                    sAtmOcclusion = atmOcclusion.ToString("F2");
                                    newsAtmOcclusion = sAtmOcclusion;
                                    draw = true;
                                }
                                if (newsAtmOcclusion != sAtmOcclusion)
                                {
                                    bValidAtmOcclusion = double.TryParse(newsAtmOcclusion, out dTmp);
                                    if (!bValidAtmOcclusion)
                                        sAtmOcclusion = atmOcclusion.ToString("F2");
                                    else
                                    {
                                        atmOcclusion = dTmp;
                                        sAtmOcclusion = newsAtmOcclusion;
                                        draw = true;
                                    }
                                }
                            }

                            GUILayout.Space(RowSpacing);
                            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.Height(rowControlHeight)))
                            {
                                GUILayout.Label(Loc.Label("Vac", "Vac:"), rowLabelStyle, GUILayout.Width(labelColWidth));
                                var newsVacOcclusion = GUILayout.TextField(sVacOcclusion, textStyle, GUILayout.Width(FieldWidth));
                                if (GUILayout.Button(UpArrow, GUILayout.Width(butW), GUILayout.Height(rowControlHeight)))
                                {
                                    if (vacOcclusion < 1.1)
                                        vacOcclusion += 0.01f;
                                    sVacOcclusion = vacOcclusion.ToString("F2");
                                    newsVacOcclusion = sVacOcclusion;
                                    draw = true;
                                }
                                if (GUILayout.Button(DownArrow, GUILayout.Width(butW), GUILayout.Height(rowControlHeight)))
                                {
                                    if (vacOcclusion > 0)
                                        vacOcclusion -= 0.01f;
                                    sVacOcclusion = vacOcclusion.ToString("F2");
                                    newsVacOcclusion = sVacOcclusion;
                                    draw = true;
                                }
                                if (newsVacOcclusion != sVacOcclusion)
                                {
                                    bValidVacOcclusion = Double.TryParse(newsVacOcclusion, out dTmp);
                                    if (!bValidVacOcclusion)
                                        sVacOcclusion = vacOcclusion.ToString("F2");
                                    else
                                    {
                                        vacOcclusion = dTmp;
                                        sVacOcclusion = newsVacOcclusion;
                                        draw = true;
                                    }
                                }
                            }
                        }
                    }

                    if (layoutFlightTargetOrbits)
                    {
                        using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                        {
                            bool warning = (FlightGlobals.activeTarget.orbit.ApA < FlightGlobals.ActiveVessel.mainBody.atmosphereDepth);
                            if (DrawOrbitPresetRadio(SelectedOrbit.Ap, new GUIContent(
                                Loc.F("CurrentAp", "Current Ap (<<1>>)", FlightGlobals.activeTarget.orbit.ApA.ToString("N0"))),
                                ref selectedOrbit,
                                warning ? OptionButtonAccent.Warning : OptionButtonAccent.Default))
                            {
                                orbitAltitude = Math.Round(FlightGlobals.activeTarget.orbit.ApA);
                                sOrbitAltitude = orbitAltitude.ToString();
                                OrbitCalc.periodEntry = false;
                                draw = true;
                            }

                            warning = (FlightGlobals.activeTarget.orbit.PeA < FlightGlobals.ActiveVessel.mainBody.atmosphereDepth);
                            if (FlightGlobals.activeTarget.orbit.PeA < 1)
                                GUI.enabled = false;
                            if (DrawOrbitPresetRadio(SelectedOrbit.Pe, new GUIContent(
                                Loc.F("CurrentPe", "Current Pe (<<1>>)", FlightGlobals.activeTarget.orbit.PeA.ToString("N0"))),
                                ref selectedOrbit,
                                warning ? OptionButtonAccent.Warning : OptionButtonAccent.Default))
                            {
                                orbitAltitude = Math.Round(FlightGlobals.activeTarget.orbit.PeA);
                                sOrbitAltitude = orbitAltitude.ToString();
                                OrbitCalc.periodEntry = false;
                                draw = true;
                            }
                            GUI.enabled = true;
                        }
                    }

                    GUILayout.Space(8);
                    GUILayout.Label(Loc.T("ResonantOrbit", "Resonant Orbit"), labelResonantOrbit);
                    using (new GUILayout.VerticalScope(sectionBoxStyle, GUILayout.ExpandWidth(true)))
                    {
                        if (DrawBoolOption(ref flipOrbit, Loc.Label("DiveOrbit", "Dive orbit")))
                        {
                            draw = true;
                        }

                        DrawParamRow(Loc.T("OrbitalPeriod", "Orbital Period:"), OrbitCalc.carrierT);
                        DrawParamRow(Loc.T("Apoapsis", "Apoapsis:"), OrbitCalc.carrierAp);
                        DrawParamRow(Loc.T("Periapsis", "Periapsis:"),
                            OrbitCalc.carrierPe != "" ? OrbitCalc.carrierPe : Loc.T("NA", "n/a"),
                            OrbitCalc.carrierPeWarning ? warningLabel : normalLabel);
                        DrawParamRow(Loc.T("InjectionDv", "Injection Δv:"), OrbitCalc.burnDV);
                        DrawParamRow(Loc.T("LosLength", "LOS Length:"), OrbitCalc.actualLOSlength, trailingSpace: false);
                    }

                    if (layoutFlightManeuvers)
                    {
                        if (layoutCreateNodeAp)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(Loc.Label("CreateNodeAp", "Create Maneuver Node at Ap")))
                                {
                                    Vector3d circularizeDv = VesselOrbitalCalc.CircularizeAtAP(FlightGlobals.ActiveVessel);
                                    // If a flip, then  subtract
                                    double UT = Planetarium.GetUniversalTime();
                                    UT += FlightGlobals.ActiveVessel.orbit.timeToAp;
                                    var o = FlightGlobals.ActiveVessel.orbit;
                                    if (flipOrbit)
                                        circularizeDv -= OrbitCalc.dBurnDV * o.Horizontal(UT);
                                    else
                                        circularizeDv += OrbitCalc.dBurnDV * o.Horizontal(UT);
                                    FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes.Clear();
                                    VesselOrbitalCalc.PlaceManeuverNode(FlightGlobals.ActiveVessel, FlightGlobals.ActiveVessel.orbit, circularizeDv, UT);
                                }
                            }
                        }
                        if (layoutCreateNodePe)
                        {
                            if (OrbitCalc.carrierPeWarning)
                                buttonStyle = buttonRed;
                            else
                                buttonStyle = GUI.skin.button;
                            using (new GUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(Loc.Label("CreateNodePe", "Create Maneuver Node at Pe"), buttonStyle))
                                {
                                    double UT = Planetarium.GetUniversalTime();
                                    UT += FlightGlobals.ActiveVessel.orbit.timeToPe;
                                    var o = FlightGlobals.ActiveVessel.orbit;
                                    Vector3d circularizeDv = VesselOrbitalCalc.CircularizeAtPE(FlightGlobals.ActiveVessel);
                                    // If a flip, then  subtract 
                                    if (flipOrbit)
                                        circularizeDv -= OrbitCalc.dBurnDV * o.Horizontal(UT);
                                    else
                                        circularizeDv += OrbitCalc.dBurnDV * o.Horizontal(UT);
                                    FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes.Clear();
                                    VesselOrbitalCalc.PlaceManeuverNode(FlightGlobals.ActiveVessel, FlightGlobals.ActiveVessel.orbit, circularizeDv, UT);
                                }
                            }
                        }
                        if (layoutClearNodes)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(Loc.T("ClearNodes", "Clear all nodes")))
                                {
                                    for (int i = FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes.Count - 1; i >= 0; i--)
                                    {
                                        FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes[i].RemoveSelf();
                                    }
                                }
                            }

                            if (layoutManeuverExtras)
                            {
                                if (layoutKacAlarmsButton)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button(Loc.T("AddKacAlarms", "Add alarms to KAC")))
                                        {
                                            double timeToOrbit = FlightGlobals.ActiveVessel.patchedConicSolver.maneuverNodes[0].UT;
                                            double period = OrbitCalc.satelliteorbit.T;
                                            String aID;
                                            if (!ResonantOrbitCalculator.Instance.mucore.Available)
                                            {
                                                aID = KACWrapper.KAC.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.Maneuver, Loc.T("KacOrbitalManeuver", "Orbital Maneuver"), timeToOrbit - 60);
                                                KACWrapper.KAC.Alarms.First(z => z.ID == aID).Notes = Loc.T("KacResonantNotes", "Put carrier craft into resonant orbit");
                                                KACWrapper.KAC.Alarms.First(z => z.ID == aID).AlarmMargin = 60;
                                            }

                                            period = OrbitCalc.carrierorbit.T;

                                            timeToOrbit += period;
                                            for (int i = 0; i < numSats; i++)
                                            {
                                                aID = KACWrapper.KAC.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.Raw, Loc.F("KacDetachment", "Detachment # <<1>>", i + 1), timeToOrbit - 60);

                                                KACWrapper.KAC.Alarms.First(z => z.ID == aID).Notes = Loc.F("KacDetachNotes", "Detach satellite # <<1>> and circularize its orbit", i + 1);
                                                KACWrapper.KAC.Alarms.First(z => z.ID == aID).AlarmMargin = 60;
                                                timeToOrbit += period;
                                            }
                                        }
                                    }
                                }

                                if (layoutMechJebButton)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        if (GUILayout.Button(Loc.T("ExecuteManeuver", "Execute maneuver")))
                                        {
                                            ResonantOrbitCalculator.Instance.mucore.ExecuteNode();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            GUILayout.Space(8);
            using (new GUILayout.HorizontalScope(GUILayout.Width(MODERN_WND_WIDTH)))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(GRAPH_WIDTH)))
                {
                    DrawUnitsRow(ref draw);
                }
                GUILayout.Space(PANEL_GAP);
                using (new GUILayout.VerticalScope(GUILayout.Width(RIGHT_PANEL_WIDTH)))
                {
                    if (DrawPanelButton(Loc.Label("SaveWindow", "Save Window")))
                    {
                        saveScreen = true;
                    }
                }
            }
            }
            if (draw)
                RequestGraphUpdate();

            GUI.DragWindow();

        }


    }
}