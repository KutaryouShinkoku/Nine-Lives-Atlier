using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using WanFramework.UI.DataComponent;

namespace Game.Model
{
    public static class SaveUtils
    {
        private static readonly byte[] Magic = { 0x2e, 0x53, 0x61, 0x76 };
        private static readonly byte[] Version = { 0x01 };
        private const string SaveDirRoot = "Saves";
        private const string InGameFileName = "InGame.sav";
        private const string SettingFileName = "Settings.sav";
        private const string DefaultGameSaveName = "Last";
        private static string SaveRoot { get; } = Path.Combine(Application.persistentDataPath, SaveDirRoot);
        private static bool _enableSettingAutoSave = true;
        public static bool EnableSettingAutoSave
        {
            get => _enableSettingAutoSave;
            set
            {
                if (_enableSettingAutoSave == value) return;
                _enableSettingAutoSave = value;
                if (_enableSettingAutoSave)
                    SaveModel(DataModel<SettingModel>.Instance, SettingFileName);
            }
        }
        private static bool AssertStreamBytes(Stream stream, byte[] data)
        {
            Span<byte> cache = stackalloc byte[data.Length];
            if (stream.Read(cache) != data.Length) return false;
            for (var i = 0; i < data.Length; i++)
                if (data[i] != cache[i]) return false;
            return true;
        }
        private static void LoadModelInner(this DataModelBase model, string fullPath)
        {
            model.Reset();
            using var fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.None);
            if (!AssertStreamBytes(fs, Magic)) throw new FormatException("Invalid save file magic");
            if (!AssertStreamBytes(fs, Version)) throw new FormatException("Invalid save file version");
            using var sr = new StreamReader(fs);
            model.Deserialize(sr);
        }
        
        public static bool LoadModel(this DataModelBase model, string savePath)
        {
            var fullPath = Path.Join(SaveRoot, savePath);
            if (!File.Exists(fullPath)) return false;
            try
            {
                LoadModelInner(model, fullPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        public static void SaveModel(this DataModelBase model, string savePath)
        {
            var fullPath = Path.Join(SaveRoot, savePath);
            var oldPath = $"{fullPath}.old";
            var dirName = Path.GetDirectoryName(fullPath);
            if (dirName != null && !Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            if (File.Exists(oldPath)) File.Delete(oldPath);
            if (File.Exists(fullPath)) File.Move(fullPath, oldPath);
            using var fs = File.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.Write(Magic);
            fs.Write(Version);
            using var sw = new StreamWriter(fs);
            model.Serialize(sw);
        }

        public static void SaveGame(string saveName = DefaultGameSaveName)
        {
            var inGamePath = $"{saveName}_{InGameFileName}";
            DataModel<InGameModel>.Instance.SaveModel(inGamePath);
        }
        public static void LoadGame(string saveName = DefaultGameSaveName)
        {
            var inGamePath = $"{saveName}_{InGameFileName}";
            DataModel<InGameModel>.Instance.LoadModel(inGamePath);
            // 修复Logic中未明确区分InGameModel和BattleModel导致的加载存档后无法正确刷新商店问题
            DataModel<BattleModel>.Instance.PlayerModel.SetCharacterIdWithoutNotify(DataModel<InGameModel>.Instance.CharacterId);
        }
        public static void DeleteGame(string saveName = DefaultGameSaveName)
        {
            var fullSaveRoot = Path.Join(SaveRoot, $"{saveName}_{InGameFileName}");
            File.Delete(fullSaveRoot);
        }
        public static bool IsGameSaveExist(string saveName = DefaultGameSaveName)
        {
            var fullSaveRoot = Path.Join(SaveRoot, $"{saveName}_{InGameFileName}");
            return File.Exists(fullSaveRoot);
        }
        public static void RegisterSettingSaveListener()
        {
            LoadSetting();
            DataModel<SettingModel>.Instance.onPropertyChanged.AddListener((m, p) =>
            {
                if (EnableSettingAutoSave) m.SaveModel(SettingFileName);
            });
        }
        public static void LoadSetting()
        {
            if (DataModel<SettingModel>.Instance.LoadModel(SettingFileName)) return;
            DataModel<SettingModel>.Instance.Reset();
            DataModel<SettingModel>.Instance.SaveModel(SettingFileName);
        }
    }
}