using AwesomeTechnologies.VegetationSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation.Masks
{
//    [BurstCompile]
//    public struct SampleRgbaChannelIncludeMask : IJob
//    {
//        public NativeList<VegetationInstance> InstanceList;
//        [ReadOnly]
//        public NativeArray<RGBABytes> RgbaNativeArray;
//
//        public bool Inverse;
//        public int SelectedChannel;
//
//        public int Width;
//        public int Height;
//        public Rect TextureRect;
//
//        public float MinValue;
//        public float MaxValue;
//
//        public void Execute()
//        {
//            float2 cellSize = new float2(TextureRect.width/Width,TextureRect.height/Height);
//            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width/2f, 0, TextureRect.center.y - TextureRect.height / 2f);
//            
//            int minValue = Mathf.RoundToInt(MinValue * 256);
//            int maxValue = Mathf.RoundToInt(MaxValue * 256);
//
//            for (int i = 0; i <= InstanceList.Length - 1; i++)
//            {
//                VegetationInstance vegetationInstance = InstanceList[i];
//                float3 localPosition = vegetationInstance.Position - texturePosition;
//
//                int x = Mathf.RoundToInt(localPosition.x / cellSize.x);
//                int z = Mathf.RoundToInt(localPosition.z / cellSize.y);
//
//                if (x < 0 || x > Width - 1) continue;
//                if (z < 0 || z > Height -1) continue;
//
//                int value = 0;
//
//                
//
//                switch (SelectedChannel)
//                {
//                    case 0:
//                        value = RgbaNativeArray[x + (z * Width)].R;
//                        break;
//                    case 1:
//                        value = RgbaNativeArray[x + (z * Width)].G;
//                        break;
//                    case 2:
//                        value = RgbaNativeArray[x + (z * Width)].B;
//                        break;
//                    case 3:
//                        value = RgbaNativeArray[x + (z * Width)].A;
//                        break;
//                }
//
//                if (Inverse)
//                {
//                    value = 256 - value;
//                }               
//
//                if (value >= minValue && value <= maxValue)
//                {
//                    vegetationInstance.TextureMaskData = 1;
//                }
//                InstanceList[i] = vegetationInstance;
//            }
//        }
//    }

    [BurstCompile]
    public struct SampleRgbaChannelIncludeMaskJob : IJobParallelFor
    {
        public NativeArray<float3> Position;
        public NativeArray<byte> TextureMaskData;
        public NativeArray<byte> Excluded;
        [ReadOnly] public NativeArray<RGBABytes> RgbaNativeArray;

        public bool Inverse;
        public int SelectedChannel;

        public int Width;
        public int Height;
        public Rect TextureRect;

        public float MinValue;
        public float MaxValue;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            float2 cellSize = new float2(TextureRect.width / Width, TextureRect.height / Height);
            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width / 2f, 0,
                TextureRect.center.y - TextureRect.height / 2f);

            int minValue = Mathf.RoundToInt(MinValue * 256);
            int maxValue = Mathf.RoundToInt(MaxValue * 256);
        
            float3 localPosition = Position[index] - texturePosition;

            int x = Mathf.RoundToInt(localPosition.x / cellSize.x);
            int z = Mathf.RoundToInt(localPosition.z / cellSize.y);

            if (x < 0 || x > Width - 1) return;
            if (z < 0 || z > Height - 1) return;

            int value = 0;


            switch (SelectedChannel)
            {
                case 0:
                    value = RgbaNativeArray[x + (z * Width)].R;
                    break;
                case 1:
                    value = RgbaNativeArray[x + (z * Width)].G;
                    break;
                case 2:
                    value = RgbaNativeArray[x + (z * Width)].B;
                    break;
                case 3:
                    value = RgbaNativeArray[x + (z * Width)].A;
                    break;
            }

            if (Inverse)
            {
                value = 256 - value;
            }

            if (value >= minValue && value <= maxValue)
            {
                TextureMaskData[index] = 1;
            }
        }
    }

//    [BurstCompile]
//    public struct SampleRgbaChannelExcludeMask : IJob
//    {
//        public NativeList<VegetationInstance> InstanceList;
//        [ReadOnly] public NativeArray<RGBABytes> RgbaNativeArray;
//
//        public bool Inverse;
//        public int SelectedChannel;
//
//        public int Width;
//        public int Height;
//        public Rect TextureRect;
//
//        public float MinValue;
//        public float MaxValue;
//
//        public void Execute()
//        {
//            float2 cellSize = new float2(TextureRect.width / Width, TextureRect.height / Height);
//            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width / 2f, 0,
//                TextureRect.center.y - TextureRect.height / 2f);
//
//            int minValue = Mathf.RoundToInt(MinValue * 256);
//            int maxValue = Mathf.RoundToInt(MaxValue * 256);
//
//            for (int i = InstanceList.Length - 1; i >= 0; i--)
//            {
//                VegetationInstance vegetationInstance = InstanceList[i];
//                float3 localPosition = vegetationInstance.Position - texturePosition;
//
//                int x = Mathf.RoundToInt(localPosition.x / cellSize.x);
//                int z = Mathf.RoundToInt(localPosition.z / cellSize.y);
//
//                if (x < 0 || x > Width - 1) continue;
//                if (z < 0 || z > Height - 1) continue;
//
//                int value = 0;
//
//                switch (SelectedChannel)
//                {
//                    case 0:
//                        value = RgbaNativeArray[x + (z * Width)].R;
//                        break;
//                    case 1:
//                        value = RgbaNativeArray[x + (z * Width)].G;
//                        break;
//                    case 2:
//                        value = RgbaNativeArray[x + (z * Width)].B;
//                        break;
//                    case 3:
//                        value = RgbaNativeArray[x + (z * Width)].A;
//                        break;
//                }
//
//                if (Inverse)
//                {
//                    value = 256 - value;
//                }
//
//                if (value >= minValue && value <= maxValue)
//                {
//                    InstanceList.RemoveAtSwapBack(i);
//                }
//            }
//        }
//    }

    [BurstCompile]
    public struct SampleRgbaChannelExcludeMaskJob : IJobParallelFor
    {
        public NativeArray<float3> Position;
        public NativeArray<byte> Excluded;
        [ReadOnly] public NativeArray<RGBABytes> RgbaNativeArray;

        public bool Inverse;
        public int SelectedChannel;

        public int Width;
        public int Height;
        public Rect TextureRect;

        public float MinValue;
        public float MaxValue;

        public void Execute(int index)
        {
            if (Excluded[index] == 1) return;
            
            float2 cellSize = new float2(TextureRect.width / Width, TextureRect.height / Height);
            float3 texturePosition = new float3(TextureRect.center.x - TextureRect.width / 2f, 0,
                TextureRect.center.y - TextureRect.height / 2f);

            int minValue = Mathf.RoundToInt(MinValue * 256);
            int maxValue = Mathf.RoundToInt(MaxValue * 256);

            float3 localPosition = Position[index] - texturePosition;

            int x = Mathf.RoundToInt(localPosition.x / cellSize.x);
            int z = Mathf.RoundToInt(localPosition.z / cellSize.y);

            if (x < 0 || x > Width - 1) return;
            if (z < 0 || z > Height - 1) return;

            int value = 0;

            switch (SelectedChannel)
            {
                case 0:
                    value = RgbaNativeArray[x + (z * Width)].R;
                    break;
                case 1:
                    value = RgbaNativeArray[x + (z * Width)].G;
                    break;
                case 2:
                    value = RgbaNativeArray[x + (z * Width)].B;
                    break;
                case 3:
                    value = RgbaNativeArray[x + (z * Width)].A;
                    break;
            }

            if (Inverse)
            {
                value = 256 - value;
            }

            if (value >= minValue && value <= maxValue)
            {
                Excluded[index] = 1;
            }
        }
    }

    [System.Serializable]
    public class TextureMask
    {
        public Rect TextureRect;
        public Texture2D MaskTexture;

        private NativeArray<RGBABytes> _rgbaNativeArray;

        public JobHandle SampleIncludeMask(VegetationInstanceData instanceData, Rect spawnRect,
            TextureMaskType textureMaskType, TextureMaskRule textureMaskRule, JobHandle dependsOn)
        {
            if (!spawnRect.Overlaps(TextureRect)) return dependsOn;
            if (MaskTexture == null) return dependsOn;

            switch (textureMaskType)
            {
                case TextureMaskType.RGBAChannel:
                    if (!_rgbaNativeArray.IsCreated) _rgbaNativeArray = MaskTexture.GetRawTextureData<RGBABytes>();

                    bool inverse = textureMaskRule.GetBooleanPropertyValue("Inverse");
                    int selectedChannel = textureMaskRule.GetIntPropertyValue("ChannelSelector");

                    if (MaskTexture.format == TextureFormat.RGBA32)
                    {
                        selectedChannel--;
                        if (selectedChannel == -1) selectedChannel = 3;
                    }

                    SampleRgbaChannelIncludeMaskJob sampleRgbaChannelIncludeMaskJob =
                        new SampleRgbaChannelIncludeMaskJob();
                    sampleRgbaChannelIncludeMaskJob.Width = MaskTexture.width;
                    sampleRgbaChannelIncludeMaskJob.Height = MaskTexture.height;

                    sampleRgbaChannelIncludeMaskJob.Excluded = instanceData.Excluded.ToDeferredJobArray();
                    sampleRgbaChannelIncludeMaskJob.Position = instanceData.Position.ToDeferredJobArray();
                    sampleRgbaChannelIncludeMaskJob.TextureMaskData = instanceData.TextureMaskData.ToDeferredJobArray();
                    sampleRgbaChannelIncludeMaskJob.RgbaNativeArray = _rgbaNativeArray;
                    sampleRgbaChannelIncludeMaskJob.SelectedChannel = selectedChannel;
                    sampleRgbaChannelIncludeMaskJob.Inverse = inverse;
                    sampleRgbaChannelIncludeMaskJob.TextureRect = TextureRect;
                    sampleRgbaChannelIncludeMaskJob.MinValue = textureMaskRule.MinDensity;
                    sampleRgbaChannelIncludeMaskJob.MaxValue = textureMaskRule.MaxDensity;
                    return sampleRgbaChannelIncludeMaskJob.Schedule(instanceData.Excluded, 32, dependsOn);
            }

            return dependsOn;
        }

        public JobHandle SampleExcludeMask(VegetationInstanceData instanceData, Rect spawnRect,
            TextureMaskType textureMaskType, TextureMaskRule textureMaskRule, JobHandle dependsOn)
        {
            if (!spawnRect.Overlaps(TextureRect)) return dependsOn;
            if (MaskTexture == null) return dependsOn;
            switch (textureMaskType)
            {
                case TextureMaskType.RGBAChannel:
                    if (!_rgbaNativeArray.IsCreated) _rgbaNativeArray = MaskTexture.GetRawTextureData<RGBABytes>();

                    bool inverse = textureMaskRule.GetBooleanPropertyValue("Inverse");
                    int selectedChannel = textureMaskRule.GetIntPropertyValue("ChannelSelector");

                    if (MaskTexture.format == TextureFormat.RGBA32)
                    {
                        selectedChannel--;
                        if (selectedChannel == -1) selectedChannel = 3;
                    }

                    SampleRgbaChannelExcludeMaskJob sampleRgbaChannelExcludeMaskJob = new SampleRgbaChannelExcludeMaskJob();
                    sampleRgbaChannelExcludeMaskJob.Width = MaskTexture.width;
                    sampleRgbaChannelExcludeMaskJob.Height = MaskTexture.height;                                       
                    sampleRgbaChannelExcludeMaskJob.Excluded = instanceData.Excluded.ToDeferredJobArray();
                    sampleRgbaChannelExcludeMaskJob.Position = instanceData.Position.ToDeferredJobArray();                                        
                    sampleRgbaChannelExcludeMaskJob.RgbaNativeArray = _rgbaNativeArray;
                    sampleRgbaChannelExcludeMaskJob.SelectedChannel = selectedChannel;
                    sampleRgbaChannelExcludeMaskJob.Inverse = inverse;
                    sampleRgbaChannelExcludeMaskJob.TextureRect = TextureRect;
                    sampleRgbaChannelExcludeMaskJob.MinValue = textureMaskRule.MinDensity;
                    sampleRgbaChannelExcludeMaskJob.MaxValue = textureMaskRule.MaxDensity;
                    return sampleRgbaChannelExcludeMaskJob.Schedule(instanceData.Excluded,32,dependsOn);
            }

            return dependsOn;
        }

        public void Dispose()
        {
        }
    }
}