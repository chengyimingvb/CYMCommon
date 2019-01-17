using AwesomeTechnologies.VegetationSystem;
using UnityEngine;


namespace AwesomeTechnologies.Shaders
{
    // ReSharper disable once InconsistentNaming
    public class CTIShaderController : IShaderController
    {
        private static readonly string[] FoliageShaderNames =
        {
            "CTI/LOD Leaves"
        };

        public bool MatchShader(string shaderName)
        {
            if (string.IsNullOrEmpty(shaderName)) return false;

            for (int i = 0; i <= FoliageShaderNames.Length - 1; i++)
            {
                if (FoliageShaderNames[i] == shaderName) return true;
            }

            return false;
        }

        public ShaderControllerSettings Settings { get; set; }
        public void CreateDefaultSettings(Material[] materials)
        {
            Settings = new ShaderControllerSettings
            {
                Heading = "CTI Tree shader",
                Description = "",
                LODFadePercentage = true,
                LODFadeCrossfade = true,
                SampleWind = false,
                SupportsInstantIndirect = false,
                BillboardHDWind = false,
                OverrideBillboardAtlasNormalShader = "AwesomeTechnologies/Billboards/RenderNormalsAtlasCTI",
                OverrideBillboardAtlasShader = "AwesomeTechnologies/Billboards/RenderDiffuseAtlasCTI"
            };

            Settings.AddLabelProperty("Foliage settings");

            Settings.AddColorProperty("ColorVariation", "Color variation", "",
                ShaderControllerSettings.GetColorFromMaterials(materials, "_HueVariation"));
            Settings.AddFloatProperty("TranslucencyStrength", "Translucency strength", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TranslucencyStrength"), 0, 1);
            Settings.AddFloatProperty("AlphaCutoff", "Alpha cutoff", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_Cutoff"), 0, 1);

            Settings.AddLabelProperty("Wind settings");

            Settings.AddFloatProperty("TumbleStrength", "Tumble Strength", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TumbleStrength"), -1, 1);
            Settings.AddFloatProperty("TumbleFrequency", "Tumble Frequency", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TumbleFrequency"), 0, 4);
            Settings.AddFloatProperty("TimeOffset", "Time Offset", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_TimeOffset"), 0, 2);
            Settings.AddFloatProperty("LeafTurbulence", "Leaf Turbulence", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_LeafTurbulence"), 0, 4);
            Settings.AddFloatProperty("EdgeFlutterInfluence", "Edge Flutter Influence", "", ShaderControllerSettings.GetFloatFromMaterials(materials, "_EdgeFlutterInfluence"), 0, 1);
        }

        public void UpdateMaterial(Material material, EnvironmentSettings environmentSettings)
        {
            if (Settings == null) return;
            material.SetColor("_HueVariation", Settings.GetColorPropertyValue("ColorVariation"));

            material.SetFloat("_TranslucencyStrength", Settings.GetFloatPropertyValue("TranslucencyStrength"));
            material.SetFloat("_Cutoff", Settings.GetFloatPropertyValue("AlphaCutoff"));

            material.SetFloat("_TumbleStrength", Settings.GetFloatPropertyValue("TumbleStrength"));
            material.SetFloat("_TumbleFrequency", Settings.GetFloatPropertyValue("TumbleFrequency"));
            material.SetFloat("_TimeOffset", Settings.GetFloatPropertyValue("TimeOffset"));
            material.SetFloat("_LeafTurbulence", Settings.GetFloatPropertyValue("LeafTurbulence"));
            material.SetFloat("_EdgeFlutterInfluence", Settings.GetFloatPropertyValue("EdgeFlutterInfluence"));
        }

        public void UpdateWind(Material material, WindSettings windSettings)
        {
           
        }

        public bool MatchBillboardShader(Material[] materials)
        {
            return false;
        }
    }
}
