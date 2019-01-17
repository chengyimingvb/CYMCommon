using AwesomeTechnologies.Utility;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    public partial class UnityTerrain
    {


        private void SetupHeatmapMaterial()
        {
            TerrainHeatmapMaterial = Instantiate(Resources.Load("TerrainHeatmap") as Material);
        }
        public void OverrideTerrainMaterial()
        {
            if (!_terrain) return;
            if (TerrainHeatmapMaterial == null) SetupHeatmapMaterial();

            if (!TerrainMaterialOverridden)
            {
                _originalTerrainMaterialType = _terrain.materialType;
                _originalTerrainMaterial = _terrain.materialTemplate;
                _originalTerrainheightmapPixelError = _terrain.heightmapPixelError;
                _originalBasemapDistance = _terrain.basemapDistance;
                    
#if UNITY_2018_3_OR_NEWER
                _originalTerrainInstanced = _terrain.drawInstanced;
                _terrain.drawInstanced = false;
#endif                
                TerrainMaterialOverridden = true;
            }

            _terrain.materialType = Terrain.MaterialType.Custom;
            _terrain.materialTemplate = TerrainHeatmapMaterial;
            _terrain.basemapDistance = 0;
            _terrain.heightmapPixelError = 1;
        }

        public void RestoreTerrainMaterial()
        {
            if (!_terrain || !TerrainMaterialOverridden) return;
            _terrain.materialType = _originalTerrainMaterialType;
            _terrain.materialTemplate = _originalTerrainMaterial;
            _terrain.heightmapPixelError = _originalTerrainheightmapPixelError;
            _terrain.basemapDistance = _originalBasemapDistance;
#if UNITY_2018_3_OR_NEWER
            _terrain.drawInstanced = _originalTerrainInstanced;
#endif        
            TerrainMaterialOverridden = false;
        }

        public void UpdateTerrainMaterial(float worldspaceSeaLevel, float worldspaceMaxTerrainHeight, TerrainTextureSettings terrainTextureSettings)
        {
            if (!TerrainHeatmapMaterial) return;
            TerrainHeatmapMaterial.SetFloat("_TerrainMinHeight", worldspaceSeaLevel);
            TerrainHeatmapMaterial.SetFloat("_TerrainMaxHeight", worldspaceMaxTerrainHeight);
            TerrainHeatmapMaterial.SetFloat("_MinHeight", 0);
            TerrainHeatmapMaterial.SetFloat("_MaxHeight", 0);
            TerrainHeatmapMaterial.SetFloat("_MinSteepness", 0);
            TerrainHeatmapMaterial.SetFloat("_MaxSteepness", 90);
            TerrainHeatmapMaterial.SetTexture("_CurveTexture", new Texture2D(1, 1));
            TerrainHeatmapMaterial.SetFloatArray("_HeightCurve", terrainTextureSettings.TextureHeightCurve.GenerateCurveArray());
            TerrainHeatmapMaterial.SetFloatArray("_SteepnessCurve", terrainTextureSettings.TextureSteepnessCurve.GenerateCurveArray());
            TerrainHeatmapMaterial.SetFloat("_UseNoise", terrainTextureSettings.UseNoise ? 1 : 0);
            TerrainHeatmapMaterial.SetFloat("_InverseNoise", terrainTextureSettings.InverseNoise ? 1 : 0);
            TerrainHeatmapMaterial.SetFloat("_NoiseScale", terrainTextureSettings.NoiseScale);
            TerrainHeatmapMaterial.SetVector("_NoiseOffset", new Vector4(terrainTextureSettings.NoiseOffset.x,0,terrainTextureSettings.NoiseOffset.y,0));
        }
    }
}
