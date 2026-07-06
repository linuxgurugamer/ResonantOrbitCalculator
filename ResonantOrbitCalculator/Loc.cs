using KSP.Localization;
using UnityEngine;

namespace ResonantOrbitCalculator
{
    public static class Loc
    {
        public const string TagPrefix = "#LOC_ROC_";

        public static string Tag(string key) => TagPrefix + key;

        public static string T(string key, string fallback)
        {
            var tag = Tag(key);
            return Localizer.TryGetStringByTag(tag, out var result) ? result : fallback;
        }

        public static string F(string key, string fallback, params object[] args)
        {
            var tag = Tag(key);
            if (Localizer.TryGetStringByTag(tag, out var template))
                return Localizer.Format(template, args);
            return Localizer.Format(fallback, args);
        }

        public static GUIContent Label(string key, string text) => new GUIContent(T(key, text));
    }
}
