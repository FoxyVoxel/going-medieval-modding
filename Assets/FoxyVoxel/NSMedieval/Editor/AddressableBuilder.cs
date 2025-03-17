using System;
using Codice.CM.Client.Differences;
using PlasticGui;
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
        private const string DefaultModName = "FVMod";
        private Dictionary<string, Mod> modsById = new();

        private HashSet<string> skipOnCreateAddressables = new HashSet<string>(){"Exported"};

        private int selectedToggleIndex;

        private bool shouldRefresh = false;
        private bool creatingNew;
        
        private static readonly object settingsLock = new object();


        private readonly Dictionary<string, string> directoryToLabelDictionary = new()
        {
            {"Mesh", "Mesh"},
            {"Sprite", "Sprite"},
            {"Texture", "Texture"},
            {"SpriteAssets", "SpriteAsset"}
        };

        private string SelectedName => this.selectedToggleIndex >= this.ModIds.Length ? this.ModIds.First() : this.ModIds[this.selectedToggleIndex];

        string[] ModIds => this.modsById.Keys.ToArray();

        private Mod SelectedMod => this.modsById[this.SelectedName];


        [MenuItem("Going Medieval/Addressable Builder")]
        public static void ShowWindow()
        {
            AddressableBuilder window = GetWindow<AddressableBuilder>("Addressable Builder");
            window.minSize = new Vector2(250, 100);
        }

        private void OnEnable()
        {
           shouldRefresh = true;
        }

        private void OnFocus()
        {
           shouldRefresh = true;
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh"))
            {
                this.modsById.Clear();
                shouldRefresh = true;
            }
            
            if (shouldRefresh)
            {
                shouldRefresh = false;
                Refresh();
            }
            
            // Creates new Mod directory with folder structure
            if (GUILayout.Button("Create New"))
            {
                ModCreatorPopup.ShowPopupWindow();
                this.creatingNew = true;
            }
            
            if (this.modsById.Count == 0)
            {
                GUILayout.Box("To create a mod add new folder to \"Assets > Mods\" and give it a name");
                return;
            }

            GUILayout.Space(10);

            GUILayout.Label("Mods", EditorStyles.boldLabel);

            // Mod selection
            
            for (int i = 0; i < this.modsById.Count; i++)
            {
                bool isSelected = this.selectedToggleIndex == i;
                bool newValue = GUILayout.Toggle(isSelected, Path.GetFileName(this.ModIds[i]));
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
            
            if (this.SelectedMod.Name == DefaultModName)
            {
                return;
            }
            
            GUILayout.Space(10);
            if (GUILayout.Button("Delete"))
            {
                this.Delete();
            }
        }

        private void UpdateSelection()
        {
            int index = 0;
            foreach (var mod in this.modsById.Values)
            {
                this.ModifySchemaSafely(mod.Group, index);
                index++;
            }
        }


        private void Delete()
        {
            Mod selected = this.SelectedMod;
            if (selected.Name == DefaultModName)
            {
                Debug.LogWarning($"Deleting default is not recommended. Addressable builder tools depends on it.");
                return;
            }
            
            // Cache id, delete Directory and remove it from the list
            if (!@Directory.Exists(selected.Path))
            {
                Debug.LogError($"Directory: {selected.Path} does not exist!");
                return;
            }

            FileUtil.DeleteFileOrDirectory(selected.Path);
            string metaFile = selected.Path + ".meta";
            if (File.Exists(metaFile))
            {
                FileUtil.DeleteFileOrDirectory(metaFile); // Delete .meta file
            }
            
            AssetDatabase.Refresh();
            this.modsById.Remove(selected.Name);
            
            this.RemoveGroupSafely(selected.Group);
           
            this.selectedToggleIndex = 0;
        }

        private void Build()
        {
            EditorPrefs.SetString(ModdingUtils.SelectedModNameKey, this.SelectedName);
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
            
            string[] subdirectories = Directory.GetDirectories(this.SelectedMod.Path);
            foreach (var subdirectory in subdirectories)
            {
                string subdirectoryName = Path.GetFileName(subdirectory);
                if (this.skipOnCreateAddressables.Contains(subdirectoryName))
                {
                    continue;
                }
                
                //Debug.Log($"Found subdirectory: {subdirectoryName}");
                
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
                            newEntry.SetLabel(this.directoryToLabelDictionary[subdirectoryName], true);
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
        
        private bool IsGUIThread()
        {
            return Event.current != null && Event.current.type != EventType.Layout;
        }

        private void Refresh()
        {
            if (!IsSafeToRefresh()) 
            {
                Debug.LogWarning("Skipping Refresh() - not in a valid Unity GUI event.");
                return;
            }
            
            // Debug.Log($"Refresh() STARTED at {Time.realtimeSinceStartup}");
    
            try
            {
                RefreshModLists();
                RefreshAddressableGroups();
                UpdateSelection();
            }
            catch (Exception ex)
            {
                Debug.LogError($"ERROR in Refresh(): {ex.Message}\n{ex.StackTrace}");
            }

            this.shouldRefresh = false;
        }
        
        private bool IsSafeToRefresh()
        {
            return Event.current != null && Event.current.type == EventType.Layout;
        }

        private void RefreshModLists()
        {
            string path = ModdingUtils.GetModDirectoryPath();
            if (!Directory.Exists(path))
            {
                return;
            }

            List<string> dirs = Directory.GetDirectories(path)
                .OrderByDescending(Directory.GetCreationTime)
                .ToList();
            
            foreach (string modPath in dirs)
            {
                string modName = Path.GetFileName(modPath);
                this.modsById.TryAdd(modName, new Mod(modName, modPath));
            }
        }

        private void RefreshAddressableGroups()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return;
            }

            foreach (Mod mod in this.modsById.Values)
            {
                if (mod == null)
                {
                    Debug.LogError($"Mod is null, this should never happen! Please send this Log trace to devs.");
                    return;
                }

                // Check if this group already exists.
                if (mod.Group != null)
                {
                    continue;
                }

                AddressableAssetGroup group = settings.FindGroup(mod.Name);
                if (group == null)
                {
                    group = this.CreateGroupSafely(mod.Name);
                }

                if (group == null)
                {
                    Debug.LogError($"{mod.Name}: Couldn't find or create addressable group");
                    return;
                }
               
                this.AddGroupSafely(group);
                mod.AssignGroup(group); 
                Debug.Log($"Added group: {mod.Name} | {mod.Path} | {group}");
            }
            
            if (this.creatingNew)
            {
                if (this.modsById.Count > this.selectedToggleIndex + 1)
                {
                    this.selectedToggleIndex = this.modsById.Count - 1;
                }
                
                this.creatingNew = false;
            }
        }

        private AddressableAssetGroup CreateGroupSafely(string groupName)
        {
            lock (settingsLock)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                var defaultLocalGroup = settings.FindGroup(DefaultModName);
                if (defaultLocalGroup == null)
                {
                    Debug.LogError($"Couldn't find {DefaultModName} group to copy settings from. Make sure that this group exists. Re import project from GitHub if not");
                    return null;
                }
                
                Debug.Log($"Created Addressable group: {groupName}");
                return settings.CreateGroup(groupName, false, false, true, defaultLocalGroup.Schemas);
            }
        }
        
        private void AddGroupSafely(AddressableAssetGroup group)
        {
            lock (settingsLock)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                // Modify settings
                 
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded, group, true);
                AssetDatabase.SaveAssets();
            }
        }
        
        private void RemoveGroupSafely(AddressableAssetGroup group)
        {
            lock (settingsLock)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
            
                // Remove group and save
                settings.RemoveGroup(group);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, group, true);
                AssetDatabase.SaveAssets();
            }
        }
        
        private void ModifySchemaSafely(AddressableAssetGroup group, int index)
        {
            lock (settingsLock)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                // Modify settings
                
                bool isSelected = index == this.selectedToggleIndex;
                if (isSelected)
                {
                    // Debug.Log("Changing DefaultGroup to: " + kvp.Key);
                    settings.DefaultGroup = group;
                }

                var schema = group.GetSchema<BundledAssetGroupSchema>();
                if (schema != null)
                {
                    // Debug.Log("Updating IncludeInBuild for: " + kvp.Key);
                    schema.IncludeInBuild = isSelected;
                }

                settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupSchemaModified, group, true);
                AssetDatabase.SaveAssets();
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
        
        private class Mod
        {
            public Mod(string name, string path)
            {
                this.Name = name;
                this.Path = path;
            }

            public string Name { get; }

            public string Path { get; }

            public AddressableAssetGroup Group { get; private set; }

            public void AssignGroup(AddressableAssetGroup group)
            {
                this.Group = group;
            }
        }
    }
}