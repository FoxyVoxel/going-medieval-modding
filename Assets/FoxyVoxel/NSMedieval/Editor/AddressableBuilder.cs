namespace NSMedieval.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Modding;
    using Tools;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Build;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

    public class AddressableBuilder : EditorWindow
    {
        private const string ModInfoDataPath = "Assets/ModInfoData.asset";
        private List<string> modDirectories = new();
        private ModInfoData modInfoData;
        private int selectedToggleIndex;

        [MenuItem("Going Medieval/Addressable Builder")]
        public static void ShowWindow()
        {
            AddressableBuilder window = GetWindow<AddressableBuilder>("Addressable Builder");
            window.minSize = new Vector2(250, 100);
        }

        private void OnEnable()
        {
            // Load or create the ScriptableObject
            this.modInfoData = AssetDatabase.LoadAssetAtPath<ModInfoData>(ModInfoDataPath);

            if (this.modInfoData == null)
            {
                this.modInfoData = CreateInstance<ModInfoData>();
                AssetDatabase.CreateAsset(this.modInfoData, ModInfoDataPath);
                AssetDatabase.SaveAssets();
                Debug.Log("Created new ModInfoData.asset.");
            }
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Create New"))
            {
                ModCreatorPopup.ShowPopupWindow();
            }

            GUILayout.Space(10);

            GUILayout.Label("Mods", EditorStyles.boldLabel);
            if (this.modDirectories.Count == 0)
            {
                GUILayout.Box("To create a mod add new folder to \"Assets > Mods\" and give it a name");
                return;
            }

            // Mod selection
            for (int i = 0; i < this.modDirectories.Count; i++)
            {
                bool isSelected = this.selectedToggleIndex == i;
                bool newValue = GUILayout.Toggle(isSelected, Path.GetFileName(this.modDirectories[i]));
                if (newValue && !isSelected)
                {
                    this.selectedToggleIndex = i; // Update the selected index
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Build"))
            {
                this.Build();
            }
        }

        private void Build()
        {
            string s = Path.GetFileName(this.modDirectories[this.selectedToggleIndex]);
            EditorPrefs.SetString(ModdingUtils.SelectedModNameKey, s);

            Debug.Log("Starting Addressables build...");
            Debug.Log($"Build Path: {ModdingUtils.BuildPath}");

            BuildLauncher.BuildAddressables();
        }

        private void OnFocus()
        {
            this.RefreshModLists();
        }

        private void RefreshModLists()
        {
            string path = ModdingUtils.GetModDirectoryPath();
            if (!Directory.Exists(path))
            {
                return;
            }

            this.modDirectories = Directory.GetDirectories(path).ToList();
        }
    }
}