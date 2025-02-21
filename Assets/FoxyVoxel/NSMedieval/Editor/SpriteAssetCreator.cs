using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NSMedieval.Tools;
using TMPro;

namespace NSMedieval.Editor
{
    public class SpriteAssetCreator : EditorWindow
    {
        private const string SpriteAssetsName = "SpriteAssets";

        // Path to the template prefab
        private const string PrefabTemplatePath = "Assets/FoxyVoxel/SpriteAssets/protoAsset.asset"; 
        // Default folder for textures
        private string spriteFolderPath = ""; 
        // Target folder for new prefabs
        private string modRootPath = ""; 

        private List<string> selectedTextures = new List<string>();

        [MenuItem("Going Medieval/Create TMP SpriteAssets")]
        private static void ShowWindow()
        {
            GetWindow<SpriteAssetCreator>("TMP SpriteAsset Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("TMP SpriteAsset Creator", EditorStyles.boldLabel);

            if (selectedTextures.Count > 0)
            {
                // Folder selection
                EditorGUILayout.LabelField("Sprites Folder:", spriteFolderPath);
            }
            
            if (GUILayout.Button("Select Mod Root Folder"))
            {
                string path = EditorUtility.OpenFolderPanel("Select Texture Folder", "Assets/Mods", "");
                if (!string.IsNullOrEmpty(path))
                {
                    modRootPath = path.Replace(Application.dataPath, "Assets");
                    spriteFolderPath = $"{this.modRootPath}/Sprite";
                    LoadTextures();
                }
            }

            if (selectedTextures.Count == 0)
            {
                return;
            }

            // Display selected textures
            GUILayout.Label("Select Textures:", EditorStyles.boldLabel);
            for (int i = 0; i < selectedTextures.Count; i++)
            {
                bool selected = EditorGUILayout.Toggle(Path.GetFileName(selectedTextures[i]), true);
                if (!selected)
                {
                    selectedTextures.RemoveAt(i);
                    i--;
                }
            }

            if (GUILayout.Button("Create TMP SpriteAssets"))
            {
                CreateSpriteAssets();
            }

            if (GUILayout.Button("Clear All"))
            {
                spriteFolderPath = string.Empty;
                selectedTextures.Clear();
            }
        }

        private void LoadTextures()
        {
            selectedTextures.Clear();
            string[] textureFiles = Directory.GetFiles(spriteFolderPath, "*.png", SearchOption.TopDirectoryOnly);

            foreach (var file in textureFiles)
            {
                string assetPath = file.Replace("\\", "/");
                selectedTextures.Add(assetPath);
            }
        }

        private void CreateSpriteAssets()
        {
            string spriteAssetPath = Path.Combine(this.modRootPath, SpriteAssetsName);
            if (!AssetDatabase.IsValidFolder(spriteAssetPath))
            {
                AssetDatabase.CreateFolder(this.modRootPath, SpriteAssetsName);
            }

            foreach (var texturePath in selectedTextures)
            {
                string textureName = Path.GetFileNameWithoutExtension(texturePath);
                string targetPath = $"{spriteAssetPath}\\{textureName}.asset";

                if (!AssetDatabase.CopyAsset(PrefabTemplatePath, targetPath))
                {
                    Debug.LogWarning($"Failed to copy {targetPath} to {PrefabTemplatePath}");
                    continue;
                }

                TMP_SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(targetPath);
                if (spriteAsset == null)
                {
                    Debug.LogError($"Couldn't load proto asset: {targetPath}");
                    continue;
                }

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    Debug.LogError($"Couldn't load Texture: {texturePath}");
                    continue;
                }

                spriteAsset.spriteSheet = texture;
                Material mat = spriteAsset.material;
                if (mat != null)
                {
                    mat.mainTexture = texture;
                }

                EditorUtility.SetDirty(spriteAsset);
                AssetDatabase.SaveAssets();

                Debug.Log($"Assigned texture {texture.name} to {spriteAsset.name} SpriteAsset");

                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    Debug.LogError("Addressables settings not found! Initialize Addressables first.");
                    return;
                }

                // Add asset to Addressables
                string guid = AssetDatabase.AssetPathToGUID(targetPath);
                var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                entry.address = spriteAsset.name;
                
                // Assign Label
                entry.SetLabel(SpriteAssetsName, true);
                
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"Added {entry.address} to Addressables with label '{entry.labels}'.");
            }

            AssetDatabase.Refresh();

            Debug.Log($"Created {selectedTextures.Count} TMP SpriteAssets.");
        }
    }
}