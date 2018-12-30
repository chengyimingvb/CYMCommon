using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;

namespace VolumetricFogAndMist {

    public class VolumetricFogShaderOptions {

        public bool pendingChanges;
        public ShaderAdvancedOption[] options;

        public void ReadOptions() {
            pendingChanges = false;
            // Populate known options
            options = new ShaderAdvancedOption[]
            {
                new ShaderAdvancedOption
                {
                    id = "FOG_ORTHO", name = "Orthographic Mode", description = "Enables support for orthographic camera projection."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_DEBUG",
                    name = "Debug Mode",
                    description = "Enables fog debug view."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_MASK",
                    name = "Enable Mask",
                    description = "Enables mask defined by geometry volumes (meshes)."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_VOID_HEAVY_LOOP",
                    name = "Raymarched void area",
                    description = "Computes void within ray loop improving quality."
                },
                new ShaderAdvancedOption
                {
                    id = "FOG_MAX_POINT_LIGHTS",
                    name = "",
                    description = "",
                    hasValue = true
                }
            };


            Shader shader = Shader.Find("VolumetricFogAndMist/VolumetricFog");
            if (shader != null) {
                string path = AssetDatabase.GetAssetPath(shader);
                string file = Path.GetDirectoryName(path) + "/VolumetricFogOptions.cginc";
                string[] lines = File.ReadAllLines(file, Encoding.UTF8);
                for (int k = 0; k < lines.Length; k++) {
                    for (int o = 0; o < options.Length; o++) {
                        if (lines[k].Contains(options[o].id)) {
                            options[o].enabled = !lines[k].StartsWith("//");
                            if (options[o].hasValue) {
                                string[] tokens = lines[k].Split(null);
                                if (tokens.Length > 2) {
                                    int.TryParse(tokens[2], out options[o].value);
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }


        public bool GetAdvancedOptionState(string optionId) {
            if (options == null)
                return false;
            for (int k = 0; k < options.Length; k++) {
                if (options[k].id.Equals(optionId)) {
                    return options[k].enabled;
                }
            }
            return false;
        }

        public void UpdateAdvancedOptionsFile() {
            // Reloads the file and updates it accordingly
            Shader shader = Shader.Find("VolumetricFogAndMist/VolumetricFog");
            if (shader != null) {
                string path = AssetDatabase.GetAssetPath(shader);
                string file = Path.GetDirectoryName(path) + "/VolumetricFogOptions.cginc";
                string[] lines = File.ReadAllLines(file, Encoding.UTF8);
                for (int k = 0; k < lines.Length; k++) {
                    for (int o = 0; o < options.Length; o++) {
                        if (lines[k].Contains(options[o].id)) {
                            if (options[o].hasValue) {
                                lines[k] = "#define " + options[o].id + " " + options[o].value;
                            } else {
                                if (options[o].enabled) {
                                    lines[k] = "#define " + options[o].id;
                                } else {
                                    lines[k] = "//#define " + options[o].id;
                                }
                            }
                            break;
                        }
                    }
                }
                File.WriteAllLines(file, lines, Encoding.UTF8);

                // Save VolumetricFog.cs change
                int maxPointLights = GetOptionValue("FOG_MAX_POINT_LIGHTS");
                file = Path.GetDirectoryName(path) + "/../../Scripts/VolumetricFogStaticParams.cs";
                if (!File.Exists(file)) {
                    Debug.LogError("VolumetricFogStaticParams.cs file not found!");
                } else {
                    lines = File.ReadAllLines(file, Encoding.UTF8);
                    for (int k = 0; k < lines.Length; k++) {
                        if (lines[k].Contains("MAX_POINT_LIGHTS")) {
                            lines[k] = "public const int MAX_POINT_LIGHTS = " + maxPointLights + ";";
                            break;
                        }
                    }
                    File.WriteAllLines(file, lines, Encoding.UTF8);
                }
            }

            pendingChanges = false;
            AssetDatabase.Refresh();
        }

        public int GetOptionValue(string id) {
            for (int k = 0; k < options.Length;k++) {
                if (options[k].hasValue && options[k].id.Equals(id)) {
                    return options[k].value;
                }
            }
            return 0;
        }

        public void SetOptionValue(string id, int value) {
            for (int k = 0; k < options.Length; k++) {
                if (options[k].hasValue && options[k].id.Equals(id)) {
                    options[k].value = value;
                }
            }
        }


    }

    public struct ShaderAdvancedOption {
        public string id;
        public string name;
        public string description;
        public bool enabled;
        public bool hasValue;
        public int value;
    }


}