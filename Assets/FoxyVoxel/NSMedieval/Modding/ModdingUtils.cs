namespace NSMedieval.Modding
{
    using System;
    using System.IO;
    using Tools;
    using UnityEditor;
    using UnityEngine;

    public static class ModdingUtils
    {
        public const string SelectedModNameKey = "SelectedModName";
        private const string InfoFileName = "ModInfo.json";
        private const string PreviewFileName = "Preview.png";
        private const string PreviewModFileName = "PreviewMod.png";
        private const string DefaultTemplateModName = "My Default Mod Template";
        private const string ModsDirectory = "Mods";
        private const string ModInfoDataPath = "Assets/ModInfoData.asset";
        private const string ModsDir = "Foxy Voxel/Going Medieval/Mods";
        private const string DataDir = "Data";
        private const string ExportedDir = "Exported";
        private const string AddressableDir = "AddressableAssets";
        public readonly static string[] DefaultSubdirectories = { "Mesh", "Sprite", "Texture",  "Exported" };

        // Used by Addressable Profiles 
        public static string BuildPath =>
            Path.Combine(UnityEngine.Application.dataPath, ModsDirectory, GetModName, ExportedDir);

        // Used by Addressable Profiles 
        public static string LoadPath = Path.Combine(MyDocumentsPath, ModsDir, GetModName, DataDir, AddressableDir);

        private static string MyDocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private static string GetModName
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.HasKey(SelectedModNameKey) ? EditorPrefs.GetString(SelectedModNameKey) : "none";
#endif
                return string.Empty;
            }
        }

        public static string GetModDirectoryPath()
        {
            return Path.Combine(Application.dataPath, ModsDirectory);
        }

        #region Paths & Dir Creation

        public static string GetRootDirectoryPath()
        {
            string rootDirectoryPath = Path.Combine(MyDocumentsPath, ModsDir);
            if (Directory.Exists(rootDirectoryPath))
            {
                return rootDirectoryPath;
            }

            // Create the directory if it does not exist
            try
            {
                Directory.CreateDirectory(rootDirectoryPath);
                Debug.Log("Directory created: " + rootDirectoryPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return string.Empty;
            }

            return rootDirectoryPath;
        }

        public static string GetModDataDirectoryPath(string modName)
        {
            return Path.Combine(Path.Combine(GetRootDirectoryPath(), modName), DataDir);
        }

        public static string GetPreviewModImagePath(string rootFolderPath)
        {
            return Path.Combine(rootFolderPath, PreviewFileName);
        }

        public static Sprite GetSpriteFromPath(string path)
        {
            Texture2D texture = new(640, 360);
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] bytes = FileUtils.SafeReadAllBytes(path);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static void CreateDefaultTemplate()
        {
            FilePathUtils.CheckAndCreatePath(Path.Combine(GetDefaultTemplateModPath(), AddressableDir));

            CreateDefaultModInfo(DefaultTemplateModName,
                GetModModel(DefaultTemplateModName, new[] { ModTag.General.ToString() }));
            CopyPreviewImage(DefaultTemplateModName);
        }

        public static void CreateDefaultTemplate(string modName)
        {
            FilePathUtils.CheckAndCreatePath(Path.Combine(GetDefaultTemplateModPath(), AddressableDir));

            CreateDefaultModInfo(DefaultTemplateModName,
                GetModModel(DefaultTemplateModName, new[] { ModTag.General.ToString() }));
            CopyPreviewImage(DefaultTemplateModName);
        }

        public static string GetDefaultTemplateModPath()
        {
            return Path.Combine(GetRootDirectoryPath(), DefaultTemplateModName, DataDir);
        }

        public static void CreateDefaultModInfo(string modName, ModModel modModel)
        {
            string modInfoPath = Path.Combine(GetRootDirectoryPath(), modName, InfoFileName);
            if (File.Exists(modInfoPath))
            {
                return;
            }

            string json = JsonUtility.ToJson(modModel, true);
            File.WriteAllText(modInfoPath, json);
        }

        public static void CopyPreviewImage(string modName, string sourceImage = PreviewModFileName)
        {
            string sourceDir = Path.Combine(Application.streamingAssetsPath, "Modding", sourceImage);
            string targetDir = Path.Combine(GetRootDirectoryPath(), modName, PreviewFileName);
            if (File.Exists(targetDir))
            {
                return;
            }

            File.Copy(sourceDir, targetDir);
        }

        private static ModModel GetModModel(string modName, string[] tags, string authorName = "")
        {
            string author = string.IsNullOrEmpty(authorName) ? "[author_name]" : authorName;

            return new ModModel($"{author}.{modName}".ToLower().Replace(" ", "_"), modName,
                "Add some description here...", author, "0.1", Application.version, tags);
        }

        #endregion
    }
}