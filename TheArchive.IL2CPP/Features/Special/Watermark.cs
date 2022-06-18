﻿using System;
using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Utilities;

namespace TheArchive.Features.Special
{
    [EnableFeatureByDefault, HideInModSettings]
    public class Watermark : Feature
    {
        public override string Name => "Watermark";

		public override string Group => FeatureGroups.Special;

		public const string ColorHex = "FBF3FF";

#if MONO
		private static MethodAccessor<PUI_Watermark> A_PUI_Watermark_UpdateWatermark;
#endif

		private float _rtss;

		public override void Init()
        {
#if MONO
			A_PUI_Watermark_UpdateWatermark = MethodAccessor<PUI_Watermark>.GetAccessor("UpdateWatermark");
#endif
			_rtss = UnityEngine.Time.realtimeSinceStartup + 0.1f;
		}


        public override void OnEnable()
        {
			if(_rtss < UnityEngine.Time.realtimeSinceStartup)
				CallUpdateWatermark();
		}

		public override void OnDisable()
		{
			CallUpdateWatermark();
		}

		private static void CallUpdateWatermark()
        {
#if IL2CPP
			GuiManager.WatermarkLayer?.m_watermark?.UpdateWatermark();
#else
			var watermark = GuiManager.WatermarkLayer?.m_watermark;
			if (watermark != null)
				A_PUI_Watermark_UpdateWatermark.Invoke(watermark);
#endif
		}

		[ArchivePatch(typeof(PUI_Watermark), "UpdateWatermark")]
		internal static class PUI_Watermark_UpdateWatermarkPatch
        {
#if IL2CPP
			public static void Postfix(PUI_Watermark __instance)
			{
				var rundownKey = __instance.m_rundownKey;
				var revision = __instance.m_revision;
				var ogText = __instance.m_watermark;
#else
			public static void Postfix(PUI_Watermark __instance, string ___m_rundownKey, int ___m_revision, ref string ___m_watermark, TMPro.TextMeshPro ___m_watermarkText)
			{
				var rundownKey = ___m_rundownKey;
				var revision = ___m_revision;
				var ogText = ___m_watermark;
#endif
				try
                {
					string secondLine;
					if (BuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownSix.ToLatest()))
						secondLine = ogText;
					else
						secondLine = ogText.Split(new string[] { "\n" }, 2, StringSplitOptions.None)[1];

					var text = $"<color=#{ColorHex}>TheArchive v{ArchiveMod.VersionString}</color>\n{secondLine}";

#if IL2CPP
					__instance.m_watermark = text;
					__instance.m_watermarkText.text = text;
#else
					___m_watermark = text;
					___m_watermarkText.text = text;
#endif
				}
				catch (Exception ex)
                {
					ArchiveLogger.Error($"Watermark broke! Please fix~ {ex}: {ex.Message}");
					ArchiveLogger.Exception(ex);
                }
			}
		}
	}
}