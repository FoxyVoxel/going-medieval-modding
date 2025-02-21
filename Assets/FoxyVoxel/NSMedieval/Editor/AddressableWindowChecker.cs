using NSMedieval.Editor;
using UnityEditor;

namespace NSMedieval.Tools
{
    [InitializeOnLoad]
    public class AddressableWindowChecker
    {
        static AddressableWindowChecker()
        {
            // Check if the AddressableBuilder window is open
            if (!EditorWindow.HasOpenInstances<AddressableBuilder>())
            {
                // Open the AddressableBuilder window if it's not open
                EditorWindow.GetWindow<AddressableBuilder>();
            }
        }
    }
}