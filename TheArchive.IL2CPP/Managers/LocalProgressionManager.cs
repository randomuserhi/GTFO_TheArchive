﻿using Newtonsoft.Json;
using System;
using System.IO;
using TheArchive.Core;
using TheArchive.Interfaces;
using TheArchive.Models;
using TheArchive.Models.Progression;
using TheArchive.Utilities;

namespace TheArchive.Managers
{
    public class LocalProgressionManager : InitSingletonBase<LocalProgressionManager>, IInitAfterGameDataInitialized, IInjectLogger
    {
        public ExpeditionSession CurrentActiveSession { get; private set; }

        public IArchiveLogger Logger { get; set; }

        public static event Action<ExpeditionCompletionData> OnExpeditionCompleted;

        private static LocalRundownProgression _localRundownProgression = null;
        public static LocalRundownProgression LocalRundownProgression => _localRundownProgression ??= LoadFromProgressionFile();

        public void Init()
        {
            Logger.Msg(ConsoleColor.Magenta, "New Progression Manager has inited!");
        }

        public void StartNewExpeditionSession(string rundownId, string expeditionId, string sessionId)
        {
            CurrentActiveSession = ExpeditionSession.InitNewSession(rundownId, expeditionId, sessionId, Logger);
        }

        public void OnLevelEntered()
        {
            CurrentActiveSession?.OnLevelEntered();
        }

        public void IncreaseLayerProgression(string strLayer, string strState)
        {
            if(!Enum.TryParse<Layers>(strLayer, out var layer)
                | !Enum.TryParse<LayerState>(strState, out var state))
            {
                Logger.Error($"Either {nameof(Layers)} and/or {nameof(LayerState)} could not be parsed! ({strLayer}, {strState})");
                return;
            }

            CurrentActiveSession?.SetLayer(layer, state);
        }

        public void SaveAtCheckpoint()
        {
            CurrentActiveSession?.OnCheckpointSave();
        }

        public void ReloadFromCheckpoint()
        {
            CurrentActiveSession?.OnCheckpointReset();
        }

        public void ArtifactCountUpdated(int count)
        {
            if (CurrentActiveSession == null) return;
            CurrentActiveSession.ArtifactsCollected = count;
            Logger.Info($"current Artifact count: {count}");
        }

        public void EndCurrentExpeditionSession(bool success)
        {
            CurrentActiveSession?.OnExpeditionCompleted(success);

            var hasCompletionData = LocalRundownProgression.AddSessionResults(CurrentActiveSession, out var completionData);

            CurrentActiveSession = null;

            SaveToProgressionFile(LocalRundownProgression);

            if (hasCompletionData)
            {
                Logger.Notice($"Expedition time: {completionData.RawSessionData.EndTime - completionData.RawSessionData.StartTime}");

                OnExpeditionCompleted?.Invoke(completionData);
            }
        }

        public static void SaveToProgressionFile(LocalRundownProgression data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Instance.Logger.Msg(ConsoleColor.DarkRed, $"Saving progression to disk at: {LocalFiles.LocalProgressionPath}");
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(LocalFiles.LocalProgressionPath, json);
        }

        public static LocalRundownProgression LoadFromProgressionFile()
        {
            Instance.Logger.Msg(ConsoleColor.Green, $"Loading progression from disk at: {LocalFiles.LocalProgressionPath}");
            if (!File.Exists(LocalFiles.LocalProgressionPath))
                return new LocalRundownProgression();
            var json = File.ReadAllText(LocalFiles.LocalProgressionPath);

            return JsonConvert.DeserializeObject<LocalRundownProgression>(json);
        }
    }
}
