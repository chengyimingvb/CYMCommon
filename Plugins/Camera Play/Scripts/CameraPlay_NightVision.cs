////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class CameraPlay_NightVision : MonoBehaviour
{
    #region Variables
    [HideInInspector] public Shader SCShader;

    public enum NightVision_Preset
    {
        Night_Vision_Personalized = -1,
        Night_Vision_FX = 0,
        Night_Vision_Classic = 1,
        Night_Vision_Full = 2,
        Night_Vision_Dark = 3,
        Night_Vision_Sharp = 4,
        Night_Vision_BlueSky = 5,
        Night_Vision_Low_Light = 6,
        Night_Vision_Pinky = 7,
        Night_Vision_RedBurn = 8,
        Night_Vision_PurpleShadow = 9
    };

    [HideInInspector] public NightVision_Preset Preset;

    private NightVision_Preset PresetMemo;
    private float TimeX = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;

    [HideInInspector] public float OnOff = 0.0f;
    [HideInInspector] public float Greenness = 1.0f;
    [HideInInspector] public float Vignette = 1.0f;
    [HideInInspector] public float Vignette_Alpha = 1.0f;
    [HideInInspector] public float Distortion = 1.0f;
    [HideInInspector] public float Noise = 1.0f;
    [HideInInspector] public float Intensity = -1.0f;
    [HideInInspector] public float Light = 1.0f;
    [HideInInspector] public float Light2 = 1.0f;
    [HideInInspector] public float Line = 1.0f;
    [HideInInspector] public float Color_R = 0.0f;
    [HideInInspector] public float Color_G = 0.0f;
    [HideInInspector] public float Color_B = 0.0f;
    [HideInInspector] public float _Binocular_Size = 0.499f;
    [HideInInspector] public float _Binocular_Smooth = 0.113f;
    [HideInInspector] public float _Binocular_Dist = 0.286f;

    [HideInInspector] public bool CamTurnOff = false;
    [HideInInspector] public float flashy = 0.0f;
    [HideInInspector] public float Duration = 1f;
    [HideInInspector] private float Timer = 1f;
    #endregion

    void OnEnable()
    {
        Timer = 0;
    }


    #region Properties
    Material material
    {
        get
        {
            if (SCMaterial == null)
            {
                SCMaterial = new Material(SCShader);
                SCMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return SCMaterial;
        }
    }
    #endregion

    void Start()
    {

        SCShader = Shader.Find("CameraPlay/NightVision");

        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
    }

    void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
    {
        if (SCShader != null)
        {
            TimeX += Time.deltaTime;
            if (TimeX > 100) TimeX = 0;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(0.25f, 0.25f);
            curve.AddKey(0.50f, 2);
            curve.AddKey(0.80f, 1.1f);
            curve.AddKey(1, 1);
            float fresult = curve.Evaluate(flashy);
            material.SetFloat("_flashy", fresult);
            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_OnOff", OnOff);
            material.SetFloat("_Greenness", Greenness);
            material.SetFloat("_Vignette", Vignette);
            material.SetFloat("_Vignette_Alpha", Vignette_Alpha);
            material.SetFloat("_Distortion", Distortion);
            material.SetFloat("_Noise", Noise);
            material.SetFloat("_Intensity", Intensity);
            material.SetFloat("_Light", Light);
            material.SetFloat("_Light2", Light2);
            material.SetFloat("_Line", Line);
            material.SetFloat("_Color_R", Color_R);
            material.SetFloat("_Color_G", Color_G);
            material.SetFloat("_Color_B", Color_B);
            material.SetFloat("_Size", _Binocular_Size);
            material.SetFloat("_Dist", _Binocular_Dist);
            material.SetFloat("_Smooth", _Binocular_Smooth);
            material.SetVector("_ScreenResolution", new Vector2(Screen.width, Screen.height));
            Graphics.Blit(sourceTexture, destTexture, material);
        }
        else
        {
            Graphics.Blit(sourceTexture, destTexture);
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying != true)
        {
            SCShader = Shader.Find("CameraPlay/NightVision");
        }
#endif

        if (PresetMemo != Preset)
        {
            PresetMemo = Preset;

            float[] P1 = new float[12] { 0.757f, 0.098f, 0.458f, -2.49f, 0.559f, -0.298f, 1.202f, 0.515f, 1, 0, 0, 0 };
            float[] P2 = new float[12] { 0.2f, 0.202f, 0.68f, -1.49f, 0.084f, -0.019f, 2f, 0.166f, 1.948f, -0.1f, 0.15f, -0.07f };
            float[] P3 = new float[12] { 1.45f, 0.01f, 0.112f, -0.07f, 0.111f, -0.077f, 0.071f, 0, 0.245f, 0, 0, 0 };
            float[] P4 = new float[12] { 0.779f, 0.185f, 0.706f, 1.21f, 0.24f, 0.138f, 2, 0.07f, 1.224f, -0.21f, -0.34f, 0 };
            float[] P5 = new float[12] { 0.2f, 0.028f, 0.706f, 1.21f, 0.397f, -0.24f, 2, 0.298f, 1.224f, -0.08f, 0.48f, -0.57f };
            float[] P6 = new float[12] { 0.2f, 0.159f, 0.622f, -2.28f, 0.409f, -0.24f, 0.166f, 0.028f, 2, -0.08f, 0.22f, 0.57f };
            float[] P7 = new float[12] { 2, 0.054f, 1, -2.28f, 0.409f, -1f, 2, 0.187f, 0.241f, 0, 1.58f, 0.21f };
            float[] P8 = new float[12] { 2, 0.054f, 1, 1.28f, 0.409f, -1, 0.41f, 0.656f, 0.427f, 0.95f, -0.35f, 1.41f };
            float[] P9 = new float[12] { 2, 0.281f, 0.156f, 1.85f, 0.709f, -1, 0.41f, 0.109f, 0.34f, 0.95f, 0.36f, -0.14f };
            float[] P10 = new float[12] { 0.905f, 0.281f, 0.156f, 1.85f, 0.558f, -0.974f, 1.639f, 0.252f, 1.074f, 0.46f, 0.95f, 0.58f };

            float[] PR = new float[12];

            if (Preset == NightVision_Preset.Night_Vision_FX) PR = P1;
            if (Preset == NightVision_Preset.Night_Vision_Classic) PR = P2;
            if (Preset == NightVision_Preset.Night_Vision_Full) PR = P3;
            if (Preset == NightVision_Preset.Night_Vision_Dark) PR = P4;
            if (Preset == NightVision_Preset.Night_Vision_Sharp) PR = P5;
            if (Preset == NightVision_Preset.Night_Vision_BlueSky) PR = P6;
            if (Preset == NightVision_Preset.Night_Vision_Low_Light) PR = P7;
            if (Preset == NightVision_Preset.Night_Vision_Pinky) PR = P8;
            if (Preset == NightVision_Preset.Night_Vision_RedBurn) PR = P9;
            if (Preset == NightVision_Preset.Night_Vision_PurpleShadow) PR = P10;
            if (Preset != NightVision_Preset.Night_Vision_Personalized)
            {
                Greenness = PR[0];
                Vignette = PR[1];
                Vignette_Alpha = PR[2];
                Distortion = PR[3];
                Noise = PR[4];
                Intensity = PR[5];
                Light = PR[6];
                Light2 = PR[7];
                Line = PR[8];
                Color_R = PR[9];
                Color_G = PR[10];
                Color_B = PR[11];
            }
        }
        if (CamTurnOff == false)
        {
            Timer += Time.deltaTime * (1 / Duration);
            if (Timer > 1f) Timer = 1;
            flashy = Timer;
        }

        if (CamTurnOff)
        {
            Timer -= Time.deltaTime * (1 / Duration);
            flashy = Timer;
            if (Timer < 0)
            {
                CameraPlay.NightVision_Switch = false;
                Object.Destroy(this);
            }
        }

    }

    void OnDisable()
    {
        if (SCMaterial)
        {
            DestroyImmediate(SCMaterial);
        }
    }
}