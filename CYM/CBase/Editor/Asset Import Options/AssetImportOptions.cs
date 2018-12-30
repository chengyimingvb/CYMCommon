using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace CYM
{
    public class AssetImportOptions : AssetPostprocessor
    {
        private static bool loadedPreferences = false;
        private static Vector2 scroll = Vector2.zero;

        public static List<AudioParameters> audioParameters = new List<AudioParameters>();
        public static List<ModelParameters> modelParameters = new List<ModelParameters>();
        public static List<TextureParameters> textureParameters = new List<TextureParameters>();

        void OnPreprocessAudio()
        {
            LoadAudioPreferences();
            for (int i = 0; i < audioParameters.Count; i++)
            {
                string[] paths = assetPath.Split('/');
                string path = paths[paths.Length - 1].Split('.')[0];
                if (audioParameters[i].fileExtensionStyle == FileExtensionStyle.Suffix && path.EndsWith(audioParameters[i].fileExtension) || audioParameters[i].fileExtensionStyle == FileExtensionStyle.Prefix && path.StartsWith(audioParameters[i].fileExtension))
                {
                    AudioImporter audioImport = (AudioImporter)assetImporter;
                    audioImport.forceToMono = audioParameters[i].forceToMono;
                    audioImport.loadInBackground = audioParameters[i].loadInBackground;
                    audioImport.preloadAudioData = audioParameters[i].preloadAudioData;
                }
            }
        }

        void OnPreprocessModel()
        {
            LoadModelPreferences();
            for (int i = 0; i < modelParameters.Count; i++)
            {
                string[] paths = assetPath.Split('/');
                string path = paths[paths.Length - 1].Split('.')[0];
                if (modelParameters[i].fileExtensionStyle == FileExtensionStyle.Suffix && path.EndsWith(modelParameters[i].fileExtension) || modelParameters[i].fileExtensionStyle == FileExtensionStyle.Prefix && path.StartsWith(modelParameters[i].fileExtension))
                {
                    ModelImporter modelImport = (ModelImporter)assetImporter;
                    modelImport.globalScale = modelParameters[i].scaleFactor;
                    modelImport.meshCompression = modelParameters[i].meshCompression;
                    modelImport.isReadable = modelParameters[i].readWriteEnabled;
                    modelImport.optimizeMesh = modelParameters[i].optimizeMesh;
                    modelImport.importBlendShapes = modelParameters[i].importBlendShapes;
                    modelImport.addCollider = modelParameters[i].generateColliders;
                    modelImport.swapUVChannels = modelParameters[i].swapUVs;
                    modelImport.generateSecondaryUV = modelParameters[i].generateLightmapUVs;
                    modelImport.importNormals = modelParameters[i].normals;
                    modelImport.importTangents = modelParameters[i].tangents;
                    modelImport.normalSmoothingAngle = modelParameters[i].smoothingAngle;
                    modelImport.importMaterials = modelParameters[i].importMaterials;
                }
            }
        }

        void OnPreprocessTexture()
        {
            LoadTexturePreferences();
            for (int i = 0; i < textureParameters.Count; i++)
            {
                string[] paths = assetPath.Split('/');
                string path = paths[paths.Length - 1].Split('.')[0];
                if (textureParameters[i].fileExtensionStyle == FileExtensionStyle.Suffix && path.EndsWith(textureParameters[i].fileExtension) || textureParameters[i].fileExtensionStyle == FileExtensionStyle.Prefix && path.StartsWith(textureParameters[i].fileExtension))
                {
                    TextureImporter textureImport = (TextureImporter)assetImporter;
                    textureImport.textureType = textureParameters[i].textureType;
                }
            }
        }

        [PreferenceItem("Import Options")]
        public static void PreferencesGUI()
        {
            if (!loadedPreferences)
            {
                LoadAudioPreferences();
                LoadModelPreferences();
                LoadTexturePreferences();
                loadedPreferences = true;
            }

            // Create a Scroll View, as information displayed will exceed the size of the Preferences Panel.
            scroll = EditorGUILayout.BeginScrollView(scroll, false, false);

            // Audio Import Options
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Audio Import Options", EditorStyles.boldLabel);
            for (int a = 0; a < audioParameters.Count; a++)
            {
                EditorGUILayout.BeginVertical("box");
                audioParameters[a].foldout = EditorGUILayout.Foldout(audioParameters[a].foldout, audioParameters[a].name);
                if (audioParameters[a].foldout)
                {
                    EditorGUI.indentLevel++;
                    audioParameters[a].name = EditorGUILayout.TextField("Name", audioParameters[a].name);
                    audioParameters[a].fileExtension = EditorGUILayout.TextField("File Extension", audioParameters[a].fileExtension);
                    audioParameters[a].fileExtensionStyle = (FileExtensionStyle)EditorGUILayout.EnumPopup("File Extension Style", audioParameters[a].fileExtensionStyle);
                    audioParameters[a].forceToMono = EditorGUILayout.Toggle("Force to Mono", audioParameters[a].forceToMono);
                    audioParameters[a].loadInBackground = EditorGUILayout.Toggle("Load in Background", audioParameters[a].loadInBackground);
                    audioParameters[a].preloadAudioData = EditorGUILayout.Toggle("Preload Audio Data", audioParameters[a].preloadAudioData);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                audioParameters.Add(new AudioParameters());
            }
            GUI.enabled = (audioParameters.Count > 0);
            if (GUILayout.Button("Remove"))
            {
                audioParameters.RemoveAt(audioParameters.Count - 1);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // Model Import Options
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Model Import Options", EditorStyles.boldLabel);
            for (int m = 0; m < modelParameters.Count; m++)
            {
                EditorGUILayout.BeginVertical("box");
                modelParameters[m].foldout = EditorGUILayout.Foldout(modelParameters[m].foldout, modelParameters[m].name);
                if (modelParameters[m].foldout)
                {
                    EditorGUI.indentLevel++;
                    modelParameters[m].name = EditorGUILayout.TextField("Name", modelParameters[m].name);
                    modelParameters[m].fileExtension = EditorGUILayout.TextField("File Extension", modelParameters[m].fileExtension);
                    modelParameters[m].fileExtensionStyle = (FileExtensionStyle)EditorGUILayout.EnumPopup("File Extension Style", modelParameters[m].fileExtensionStyle);
                    modelParameters[m].scaleFactor = EditorGUILayout.FloatField("Scale Factor", modelParameters[m].scaleFactor);
                    modelParameters[m].meshCompression = (ModelImporterMeshCompression)EditorGUILayout.EnumPopup("Mesh Compression", modelParameters[m].meshCompression);
                    modelParameters[m].readWriteEnabled = EditorGUILayout.Toggle("Read/Write Enabled", modelParameters[m].readWriteEnabled);
                    modelParameters[m].optimizeMesh = EditorGUILayout.Toggle("Optimize Mesh", modelParameters[m].optimizeMesh);
                    modelParameters[m].importBlendShapes = EditorGUILayout.Toggle("Import BlendShapes", modelParameters[m].importBlendShapes);
                    modelParameters[m].generateColliders = EditorGUILayout.Toggle("Generate Colliders", modelParameters[m].generateColliders);
                    modelParameters[m].swapUVs = EditorGUILayout.Toggle("Swap UVs", modelParameters[m].swapUVs);
                    modelParameters[m].generateLightmapUVs = EditorGUILayout.Toggle("Generate Lightmap UVs", modelParameters[m].generateLightmapUVs);
                    modelParameters[m].normals = (ModelImporterNormals)EditorGUILayout.EnumPopup("Normals", modelParameters[m].normals);
                    modelParameters[m].tangents = (ModelImporterTangents)EditorGUILayout.EnumPopup("Tangents", modelParameters[m].tangents);
                    if (modelParameters[m].normals != ModelImporterNormals.Calculate)
                    {
                        modelParameters[m].smoothingAngle = 60.0f;
                        GUI.enabled = false;
                    }
                    modelParameters[m].smoothingAngle = EditorGUILayout.Slider("Smoothing Angle", modelParameters[m].smoothingAngle, 0.0f, 180.0f);
                    GUI.enabled = true;
                    modelParameters[m].importMaterials = EditorGUILayout.Toggle("Import Materials", modelParameters[m].importMaterials);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                modelParameters.Add(new ModelParameters());
            }
            GUI.enabled = (modelParameters.Count > 0);
            if (GUILayout.Button("Remove"))
            {
                modelParameters.RemoveAt(modelParameters.Count - 1);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // Texture Import Options
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Texture Import Options", EditorStyles.boldLabel);
            for (int t = 0; t < textureParameters.Count; t++)
            {
                EditorGUILayout.BeginVertical("box");
                textureParameters[t].foldout = EditorGUILayout.Foldout(textureParameters[t].foldout, textureParameters[t].name);
                if (textureParameters[t].foldout)
                {
                    EditorGUI.indentLevel++;
                    textureParameters[t].name = EditorGUILayout.TextField("Name", textureParameters[t].name);
                    textureParameters[t].fileExtension = EditorGUILayout.TextField("File Extension", textureParameters[t].fileExtension);
                    textureParameters[t].fileExtensionStyle = (FileExtensionStyle)EditorGUILayout.EnumPopup("File Extension Style", textureParameters[t].fileExtensionStyle);
                    textureParameters[t].textureType = (TextureImporterType)EditorGUILayout.EnumPopup("Texture Type", textureParameters[t].textureType);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                textureParameters.Add(new TextureParameters());
            }
            GUI.enabled = (textureParameters.Count > 0);
            if (GUILayout.Button("Remove"))
            {
                textureParameters.RemoveAt(textureParameters.Count - 1);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            // Save any changes and update the GUI.
            if (GUI.changed)
            {
                SaveAudioPreferences();
                SaveModelPreferences();
                SaveTexturePreferences();
            }
        }

        private static void LoadAudioPreferences()
        {
            int audioCount = EditorPrefs.GetInt("AudioImporterCount", 0);
            audioParameters = new List<AudioParameters>();
            for (int a = 0; a < audioCount; a++)
            {
                audioParameters.Add(new AudioParameters());
                audioParameters[a].name = EditorPrefs.GetString("AudioImporterName" + a.ToString(), audioParameters[a].name);
                audioParameters[a].fileExtension = EditorPrefs.GetString("AudioImporterFileExtension" + a.ToString(), audioParameters[a].fileExtension);
                audioParameters[a].fileExtensionStyle = (FileExtensionStyle)EditorPrefs.GetInt("AudioImporterFileExtensionStyle" + a.ToString(), (int)audioParameters[a].fileExtensionStyle);
                audioParameters[a].forceToMono = EditorPrefs.GetBool("AudioImporterForceToMono", audioParameters[a].forceToMono);
                audioParameters[a].loadInBackground = EditorPrefs.GetBool("AudioImporterLoadInBackground" + a.ToString(), audioParameters[a].loadInBackground);
                audioParameters[a].preloadAudioData = EditorPrefs.GetBool("AudioImporterPreloadAudioData" + a.ToString(), audioParameters[a].preloadAudioData);
            }
        }

        private static void LoadModelPreferences()
        {
            int modelCount = EditorPrefs.GetInt("ModelImporterCount", 0);
            modelParameters = new List<ModelParameters>();
            for (int m = 0; m < modelCount; m++)
            {
                modelParameters.Add(new ModelParameters());
                modelParameters[m].name = EditorPrefs.GetString("ModelImporterName" + m.ToString(), modelParameters[m].name);
                modelParameters[m].fileExtension = EditorPrefs.GetString("ModelImporterFileExtension" + m.ToString(), modelParameters[m].fileExtension);
                modelParameters[m].fileExtensionStyle = (FileExtensionStyle)EditorPrefs.GetInt("ModelImporterFileExtensionStyle" + m.ToString(), (int)modelParameters[m].fileExtensionStyle);
                modelParameters[m].scaleFactor = EditorPrefs.GetFloat("ModelImporterScaleFactor" + m.ToString(), modelParameters[m].scaleFactor);
                modelParameters[m].meshCompression = (ModelImporterMeshCompression)EditorPrefs.GetInt("ModelImporterMeshCompression" + m.ToString(), (int)modelParameters[m].meshCompression);
                modelParameters[m].readWriteEnabled = EditorPrefs.GetBool("ModelImporterReadWriteEnabled" + m.ToString(), modelParameters[m].readWriteEnabled);
                modelParameters[m].optimizeMesh = EditorPrefs.GetBool("ModelImporterOptimizeMesh" + m.ToString(), modelParameters[m].optimizeMesh);
                modelParameters[m].importBlendShapes = EditorPrefs.GetBool("ModelImporterImportBlendShapes" + m.ToString(), modelParameters[m].importBlendShapes);
                modelParameters[m].generateColliders = EditorPrefs.GetBool("ModelImporterGenerateColliders" + m.ToString(), modelParameters[m].generateColliders);
                modelParameters[m].swapUVs = EditorPrefs.GetBool("ModelImporterSwapUVs" + m.ToString(), modelParameters[m].swapUVs);
                modelParameters[m].generateLightmapUVs = EditorPrefs.GetBool("ModelImporterGenerateLightmapUVs" + m.ToString(), modelParameters[m].generateLightmapUVs);
                modelParameters[m].normals = (ModelImporterNormals)EditorPrefs.GetInt("ModelImporterNormals" + m.ToString(), (int)modelParameters[m].normals);
                modelParameters[m].tangents = (ModelImporterTangents)EditorPrefs.GetInt("ModelImporterTangents" + m.ToString(), (int)modelParameters[m].tangents);
                modelParameters[m].smoothingAngle = EditorPrefs.GetFloat("ModelImporterSmoothingAngle" + m.ToString(), modelParameters[m].smoothingAngle);
                modelParameters[m].importMaterials = EditorPrefs.GetBool("ModelImporterImportMaterials" + m.ToString(), modelParameters[m].importMaterials);
            }
        }

        private static void LoadTexturePreferences()
        {
            int textureCount = EditorPrefs.GetInt("TextureImporterCount", 0);
            textureParameters = new List<TextureParameters>();
            for (int t = 0; t < textureCount; t++)
            {
                textureParameters.Add(new TextureParameters());
                textureParameters[t].name = EditorPrefs.GetString("TextureImporterName" + t.ToString(), textureParameters[t].name);
                textureParameters[t].fileExtension = EditorPrefs.GetString("TextureImporterFileExtension" + t.ToString(), textureParameters[t].fileExtension);
                textureParameters[t].fileExtensionStyle = (FileExtensionStyle)EditorPrefs.GetInt("TextureImporterFileExtensionStyle" + t.ToString(), (int)textureParameters[t].fileExtensionStyle);
                textureParameters[t].textureType = (TextureImporterType)EditorPrefs.GetInt("TextureImporterTextureType" + t.ToString(), (int)textureParameters[t].textureType);
            }
        }

        private static void SaveAudioPreferences()
        {
            EditorPrefs.SetInt("AudioImporterCount", audioParameters.Count);
            for (int a = 0; a < audioParameters.Count; a++)
            {
                EditorPrefs.SetString("AudioImporterName" + a.ToString(), audioParameters[a].name);
                EditorPrefs.SetString("AudioImporterFileExtension" + a.ToString(), audioParameters[a].fileExtension);
                EditorPrefs.SetInt("AudioImporterFileExtensionStyle" + a.ToString(), (int)audioParameters[a].fileExtensionStyle);
                EditorPrefs.SetBool("AudioImporterForceToMono", audioParameters[a].forceToMono);
                EditorPrefs.SetBool("AudioImporterLoadInBackground" + a.ToString(), audioParameters[a].loadInBackground);
                EditorPrefs.SetBool("AudioImporterPreloadAudioData" + a.ToString(), audioParameters[a].preloadAudioData);
            }
        }

        private static void SaveModelPreferences()
        {
            EditorPrefs.SetInt("ModelImporterCount", modelParameters.Count);
            for (int m = 0; m < modelParameters.Count; m++)
            {
                EditorPrefs.SetString("ModelImporterName" + m.ToString(), modelParameters[m].name);
                EditorPrefs.SetString("ModelImporterFileExtension" + m.ToString(), modelParameters[m].fileExtension);
                EditorPrefs.SetInt("ModelImporterFileExtensionStyle" + m.ToString(), (int)modelParameters[m].fileExtensionStyle);
                EditorPrefs.SetFloat("ModelImporterScaleFactor" + m.ToString(), modelParameters[m].scaleFactor);
                EditorPrefs.SetInt("ModelImporterMeshCompression" + m.ToString(), (int)modelParameters[m].meshCompression);
                EditorPrefs.SetBool("ModelImporterReadWriteEnabled" + m.ToString(), modelParameters[m].readWriteEnabled);
                EditorPrefs.SetBool("ModelImporterOptimizeMesh" + m.ToString(), modelParameters[m].optimizeMesh);
                EditorPrefs.SetBool("ModelImporterImportBlendShapes" + m.ToString(), modelParameters[m].importBlendShapes);
                EditorPrefs.SetBool("ModelImporterGenerateColliders" + m.ToString(), modelParameters[m].generateColliders);
                EditorPrefs.SetBool("ModelImporterSwapUVs" + m.ToString(), modelParameters[m].swapUVs);
                EditorPrefs.SetBool("ModelImporterGenerateLightmapUVs" + m.ToString(), modelParameters[m].generateLightmapUVs);
                EditorPrefs.SetInt("ModelImporterNormals" + m.ToString(), (int)modelParameters[m].normals);
                EditorPrefs.SetInt("ModelImporterTangents" + m.ToString(), (int)modelParameters[m].tangents);
                EditorPrefs.SetFloat("ModelImporterSmoothingAngle" + m.ToString(), modelParameters[m].smoothingAngle);
                EditorPrefs.SetBool("ModelImporterImportMaterials" + m.ToString(), modelParameters[m].importMaterials);

            }
        }

        private static void SaveTexturePreferences()
        {
            EditorPrefs.SetInt("TextureImporterCount", textureParameters.Count);
            for (int t = 0; t < textureParameters.Count; t++)
            {
                EditorPrefs.SetString("TextureImporterName" + t.ToString(), textureParameters[t].name);
                EditorPrefs.SetString("TextureImporterFileExtension" + t.ToString(), textureParameters[t].fileExtension);
                EditorPrefs.SetInt("TextureImporterFileExtensionStyle" + t.ToString(), (int)textureParameters[t].fileExtensionStyle);
                EditorPrefs.SetInt("TextureImporterTextureType" + t.ToString(), (int)textureParameters[t].textureType);
            }
        }
    }

    #region Data Containers

    public class AudioParameters
    {
        public string name = "New Audio Parameter";
        public bool foldout = false;
        public string fileExtension = "XXX_";
        // The file extension to look for.
        public FileExtensionStyle fileExtensionStyle = FileExtensionStyle.Prefix;
        // Defines whether the extension is a suffix or prefix.
        public bool forceToMono = false;
        public bool loadInBackground = false;
        public bool preloadAudioData = true;
    }

    public class ModelParameters
    {
        public string name = "New Model Parameter";
        public bool foldout = false;
        public string fileExtension = "";
        // The file extension to look for.
        public FileExtensionStyle fileExtensionStyle = FileExtensionStyle.Prefix;
        // Defines whether the extension is a suffix or prefix.
        public float scaleFactor = 1.0f;
        public ModelImporterMeshCompression meshCompression = ModelImporterMeshCompression.Off;
        public bool readWriteEnabled = true;
        public bool optimizeMesh = true;
        public bool importBlendShapes = true;
        public bool generateColliders = false;
        public bool swapUVs = false;
        public bool generateLightmapUVs = false;
        public ModelImporterNormals normals = ModelImporterNormals.Import;
        public ModelImporterTangents tangents = ModelImporterTangents.CalculateMikk;
        public float smoothingAngle = 60.0f;
        public bool importMaterials = false;
    }

    public class TextureParameters
    {
        public string name = "New Texture Parameter";
        public bool foldout = false;
        public string fileExtension = "_x";
        // The file extension to look for.
        public FileExtensionStyle fileExtensionStyle = FileExtensionStyle.Suffix;
        // Defines whether the extension is a suffix or prefix.
        public TextureImporterType textureType = TextureImporterType.Default;
    }

    #endregion

    #region Enums

    public enum FileExtensionStyle
    {
        Prefix = 0,
        Suffix = 1
    }

    #endregion

}