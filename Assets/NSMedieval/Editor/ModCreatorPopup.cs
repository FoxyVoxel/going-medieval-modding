namespace NSMedieval.Editor
{
    using System.IO;
    using Modding;
    using UnityEditor;
    using UnityEngine;

    public class ModCreatorPopup : EditorWindow
    {
        private string modName = "NewMod";

        private void OnGUI()
        {
            GUILayout.Label("Create a New Mod", EditorStyles.boldLabel);

            // Input field for mod name
            this.modName = EditorGUILayout.TextField("Mod Name", this.modName);

            // Create button
            if (GUILayout.Button("Create"))
            {
                this.CreateModFolder(this.modName);
            }
        }

        public static void ShowPopupWindow()
        {
            ModCreatorPopup popup = GetWindow<ModCreatorPopup>("Create New Mod", true);
            Vector2 popupSize = new(300, 150);
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            popup.position = new Rect( // Center popup within main window
                main.x + (main.width - popupSize.x) / 2, main.y + (main.height - popupSize.y) / 2, popupSize.x, popupSize.y);
        }

        private void CreateModFolder(string name)
        {
            // Ensure the Mods directory exists
            string modsPath = Path.Combine("Assets", "Mods");
            if (!AssetDatabase.IsValidFolder(modsPath))
            {
                AssetDatabase.CreateFolder("Assets", "Mods");
            }

            // Sanitize mod name
            string sanitizedModName = this.SanitizeFolderName(name);

            // Check if folder already exists
            string newModPath = Path.Combine(modsPath, sanitizedModName);
            if (AssetDatabase.IsValidFolder(newModPath))
            {
                EditorUtility.DisplayDialog("Error", $"A mod named '{sanitizedModName}' already exists.", "OK");
                return;
            }

            // Create the new folder
            AssetDatabase.CreateFolder(modsPath, sanitizedModName);
            foreach (string subdirectory in ModdingUtils.DefaultSubdirectories)
            {
                AssetDatabase.CreateFolder(newModPath, subdirectory);
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Mod '{sanitizedModName}' has been created successfully!", "OK");
            this.Close();
        }

        private string SanitizeFolderName(string name)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar.ToString(), "_");
            }

            return name;
        }
    }
}