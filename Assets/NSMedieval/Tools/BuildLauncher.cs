namespace NSMedieval.Tools
{
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.AddressableAssets.Build;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

    public static class BuildLauncher
    {
        public const string BuildScript = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
        public const string SettingsAsset = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        public const string ProfileName = "GM_Modding";
        
        private static AddressableAssetSettings settings;

        public static bool BuildAddressables()
        {
            GetSettingsObject(SettingsAsset);
            SetProfile(ProfileName);
            IDataBuilder builderScript = AssetDatabase.LoadAssetAtPath<ScriptableObject>(BuildScript) as IDataBuilder;

            if (builderScript == null)
            {
                Debug.LogError(BuildScript + " couldn't be found or isn't a build script.");
                return false;
            }

            SetBuilder(builderScript);

            return BuildAddressableContent();
        }

        private static void GetSettingsObject(string settingsAsset)
        {
            // This step is optional, you can also use the default settings:
            //settings = AddressableAssetSettingsDefaultObject.Settings;

            settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset) as AddressableAssetSettings;

            if (settings == null)
            {
                Debug.LogError($"{settingsAsset} couldn't be found or isn't " + "a settings object.");
            }
        }

        private static void SetProfile(string profile)
        {
            string profileId = settings.profileSettings.GetProfileId(profile);
            if (string.IsNullOrEmpty(profileId))
            {
                Debug.LogWarning($"Couldn't find a profile named, {profile}, " + "using current profile instead.");
            }
            else
            {
                settings.activeProfileId = profileId;
            }
        }

        private static void SetBuilder(IDataBuilder builder)
        {
            int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

            if (index > 0)
            {
                settings.ActivePlayerDataBuilderIndex = index;
            }
            else
            {
                Debug.LogWarning($"{builder} must be added to the " + "DataBuilders list before it can be made " + "active. Using last run builder instead.");
            }
        }

        private static bool BuildAddressableContent()
        {
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);

            if (!success)
            {
                Debug.LogError("Addressables build error encountered: " + result.Error);
            }

            return success;
        }
    }
    #endif
}