using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace CYM
{
    public class AssetImportOptionsWindow : EditorWindow
    {
        private void OnGUI()
        {
            AssetImportOptions.PreferencesGUI();
        }

    }
}
