using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace NSMedieval.Editor
{
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
        private List<string> modDirectories = new();
        private Dictionary<string, AddressableAssetGroup> modGroupsById = new();

        private HashSet<string> skipOnCreateAddressables = new HashSet<string>(){"Exported"};
        
        private int selectedToggleIndex;
        

        [MenuItem("Going Medieval/Addressable Builder")]
        public static void ShowWindow()
        {
            AddressableBuilder window = GetWindow<AddressableBuilder>("Addressable Builder");
            window.minSize = new Vector2(250, 100);
        }

        private void OnEnable()
        {
           this.Refresh();
        }

        private void OnGUI()
        {
            // Creates new Mod directory with folder structure
            if (GUILayout.Button("Create New"))
            {
                ModCreatorPopup.ShowPopupWindow();
            }
            
            
            if (this.modDirectories.Count == 0)
            {
                GUILayout.Box("To create a mod add new folder to \"Assets > Mods\" and give it a name");
                return;
            }
            
            if (GUILayout.Button("Refresh"))
            {
                this.RefreshModLists();
            }

            GUILayout.Space(10);

            GUILayout.Label("Mods", EditorStyles.boldLabel);

            // Mod selection
            for (int i = 0; i < this.modDirectories.Count; i++)
            {
                bool isSelected = this.selectedToggleIndex == i;
                bool newValue = GUILayout.Toggle(isSelected, Path.GetFileName(this.modDirectories[i]));
                if (newValue && !isSelected)
                {
                    this.selectedToggleIndex = i; // Update the selected index
                    this.UpdateSelection();
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Create Addressables"))
            {
                this.CreateAddressables();
            }
            
            if (GUILayout.Button("Build"))
            {
                this.Build();
            }
            
            GUILayout.Space(10);
            if (GUILayout.Button("Delete"))
            {
                this.Delete();
            }
        }

        private void UpdateSelection()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }

            // Get group by selectionIndex 
            int index = 0;
            foreach (var kvp in this.modGroupsById)
            {
                bool isSelected = index == this.selectedToggleIndex;
                if (isSelected)
                {
                    // Debug.Log($"Selected: {kvp.Key}");
                    settings.DefaultGroup = kvp.Value;
                }
                
                var schema = kvp.Value.GetSchema<BundledAssetGroupSchema>();
                if (schema != null)
                {
                    schema.IncludeInBuild = isSelected;
                }
                
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupSchemaModified,  kvp.Value, true);
                index++;
            }
        }

        private void Delete()
        {
            // Cache id, delete Directory and remove it from the list
            string directoryPath = this.modDirectories[this.selectedToggleIndex];
            if (!@Directory.Exists(directoryPath))
            {
                Debug.LogError($"Directory: {directoryPath} does not exist!");
                return;
            }

            FileUtil.DeleteFileOrDirectory(directoryPath);
            string metaFile = directoryPath + ".meta";
            if (File.Exists(metaFile))
            {
                FileUtil.DeleteFileOrDirectory(metaFile); // Delete .meta file
            }
            
            AssetDatabase.Refresh();
            this.modDirectories.RemoveAt(this.selectedToggleIndex);


            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }

            // Cache group before deleting it
            string modId = Path.GetFileName(directoryPath);
            var group = this.modGroupsById[modId];
            this.modGroupsById.Remove(modId);

            // Remove group and save
            settings.RemoveGroup(group);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, group, true);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Deleted Addressable group: {modId}");
        }

        private void Build()
        {
            string s = Path.GetFileName(this.modDirectories[this.selectedToggleIndex]);
            EditorPrefs.SetString(ModdingUtils.SelectedModNameKey, s);
            this.ClearDirectory(ModdingUtils.BuildPath);
            
            Debug.Log($"Starting Addressables build at Path: {ModdingUtils.BuildPath}");
            BuildLauncher.BuildAddressables();
        }

        private void CreateAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }
            
            string[] subdirectories = Directory.GetDirectories(this.modDirectories[this.selectedToggleIndex]);
            foreach (var subdirectory in subdirectories)
            {
                string subdirectoryName = Path.GetFileName(subdirectory);
                if (this.skipOnCreateAddressables.Contains(subdirectoryName))
                {
                    continue;
                }
                
                Debug.Log($"Found subdirectory: {subdirectoryName}");
                
                string[] files = Directory.GetFiles(subdirectory);

                foreach (var file in files)
                {
                    // Check if the file is addressable
                    string assetPath = file.Replace("\\", "/"); // Convert path to Unity's format
                    string relativePath = assetPath.Replace(Application.dataPath, "Assets");
                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);

                    if (asset != null)
                    {
                        // Get the GUID for the asset path
                        string assetGUID = AssetDatabase.AssetPathToGUID(relativePath);
                        
                        // Check if the asset is addressable
                        var entry = settings.FindAssetEntry(assetGUID);
                        if (entry == null)
                        {
                            // Make the asset addressable
                            var newEntry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(relativePath), settings.DefaultGroup);
                            newEntry.address = Path.GetFileNameWithoutExtension(file);
                            newEntry.SetLabel(subdirectoryName, true);
                            Debug.Log($"Made {relativePath} addressable.");
                        }
                        else
                        {
                            Debug.Log($"Asset {relativePath} is already addressable.");
                        }
                    }
                }
            }
        }

        private void OnFocus()
        {
            this.Refresh();
        }

        private void Refresh()
        {
            this.RefreshModLists();
            this.RefreshAddressableGroups();
            this.UpdateSelection();
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

        private void RefreshAddressableGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }

            for (var i = 0; i < this.modDirectories.Count; i++)
            {
                var modDirectory = this.modDirectories[i];
                string modName = Path.GetFileName(modDirectory);
                if (this.modGroupsById.ContainsKey(modName))
                {
                    continue;
                }

                AddressableAssetGroup group = settings.FindGroup(modName) ??
                                              CreateGroup(modName);
                
                this.modGroupsById.Add(modName, group);

                settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded, group, true);
                AssetDatabase.SaveAssets();
            }
            
            return;
            
            AddressableAssetGroup CreateGroup(string modName)
            {
                var defaultLocalGroup = settings.FindGroup("FVMod");
                if (defaultLocalGroup == null)
                {
                    Debug.LogError($"Couldn't find FVMod group to copy settings from. Make sure that this group exists. Re import project from GitHub if not");
                    return null;
                }
                
                Debug.Log($"Created Addressable group: {modName}");
                return settings.CreateGroup(modName, false, false, true, defaultLocalGroup.Schemas);
            }
        }
        
        private void ClearDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                // Delete all files
                foreach (var file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }

                // Delete all subdirectories
                foreach (var dir in Directory.GetDirectories(path))
                {
                    Directory.Delete(dir, true);
                }

                AssetDatabase.Refresh(); // Refresh Unity's Asset Database
            }
        }
    }
}