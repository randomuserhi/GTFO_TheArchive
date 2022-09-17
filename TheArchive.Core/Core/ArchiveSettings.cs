﻿namespace TheArchive.Core
{
    public class ArchiveSettings
    {
        public string CustomFileSaveLocation { get; set; } = string.Empty;
        public bool UseCommonArchiveSettingsFile { get; set; } = false;
        public bool SkipMissionUnlockRequirements { get; set; } = false;
        public bool DumpDataBlocks { get; set; } = true;
        public bool AlwaysOverrideDataBlocks { get; set; } = false;
        public bool FeatureDevMode { get; set; } = false;
    }
}
