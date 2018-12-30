////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class CameraPlay_Drunk : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private float TimeX = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;
    [HideInInspector]
    public bool CamTurnOff = false;
    [HideInInspector]
    public float Duration = 1f;
    [HideInInspector]
    private float Timer = 1f;

    public enum Drunk_Preset
    {
        Default = 0,
        Drunk_A_Lot = 1,
        Drunk_Vodka = 2,
        Drunk_Poison = 3,
        Drunk_Yellow = 4,
        Drunk_Dark = 5,
        Drunk_Cold = 6,
        Drunk_Burn = 7,
        Drunk_Blood = 8,
        Drunk_To_Much = 9
    };

    [HideInInspector]
    public Drunk_Preset Preset;

    private Drunk_Preset PresetMemo;

    [HideInInspector]
    public float flashy = 0.0f;
    [HideInInspector]
    public float Value = 6.0f;
    [HideInInspector]
    public float Speed = 1.0f;
    [HideInInspector]
    public float Wavy = 1f;
    [HideInInspector]
    public float Distortion = 0f;
    [HideInInspector]
    public float DistortionWave = 0f;
    [HideInInspector]
    public float Fade = 1.0f;
    [HideInInspector]
    public float ColoredSaturate = 1.0f;
    [HideInInspector]
    public float ColoredChange = 0.0f;
    [HideInInspector]
    public float ChangeRed = 0.0f;
    [HideInInspector]
    public float ChangeGreen = 0.0f;
    [HideInInspector]
    public float ChangeBlue = 0.0f;



    #endregion
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
        SCShader = Shader.Find("CameraPlay/Drunk");
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
            curve.AddKey(0.50f, 1);
            curve.AddKey(0.80f, 1.1f);
            curve.AddKey(1, 1);
            float fresult = curve.Evaluate(flashy);

            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Value", Value);
            material.SetFloat("_Speed", Speed * fresult);
            material.SetFloat("_Distortion", Distortion * fresult);
            material.SetFloat("_DistortionWave", DistortionWave * fresult);
            material.SetFloat("_Wavy", Wavy * fresult);
            material.SetFloat("_Fade", fresult);
            material.SetFloat("_ColoredChange", ColoredChange * fresult);
            material.SetFloat("_ChangeRed", ChangeRed * fresult);
            material.SetFloat("_ChangeGreen", ChangeGreen * fresult);
            material.SetFloat("_ChangeBlue", ChangeBlue * fresult);
            material.SetFloat("_Colored", ColoredSaturate * fresult);
            material.SetVector("_ScreenResolution", new Vector4(sourceTexture.width, sourceTexture.height, 0.0f, 0.0f));
            Graphics.Blit(sourceTexture, destTexture, material);
        }
        else
        {
            Graphics.Blit(sourceTexture, destTexture);
        }
    }
    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying != true)
        {
            SCShader = Shader.Find("CameraPlay/Drunk");
        }
#endif
        if (PresetMemo != Preset)
        {
            PresetMemo = Preset;

            float[] P1 = new float[10] { 1f, 1f, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f }; // Default
            float[] P2 = new float[10] { 1f, 1f, 0f, 0.3f, 1f, 1.67f, 0f, 0f, 0f, 0f }; // Drunk A lot
            float[] P3 = new float[10] { 1.74f, 0.65f, 0.6f, 0.15f, 1f, -0.65f, -0.34f, 0f, 0f, 0f }; // Drunk Volka
            float[] P4 = new float[10] { 1.79f, 1f, 0.125f, 0.35f, 1f, 1.38f, -0.15f, -0.25f, 0.0f, -0.345f }; // Drunk Poison
            float[] P5 = new float[10] { 1.08f, 1f, 0f, 0.35f, 1f, 1.38f, -0.14f, -0.214f, -0.102f, 0.99f }; // Drunk Yellow
            float[] P6 = new float[10] { 1.08f, 0.104f, 0f, 0.25f, 1f, -2f, 0.33f, -0.326f, -0.195f, -0.111f }; // Drunk Dark
            float[] P7 = new float[10] { 2.02f, 0.104f, 0.103f, 0.314f, 1f, 1.75f, 0.6f, -0.186f, 0.215f, 0.44f }; // Drunk Cold
            float[] P8 = new float[10] { 0.9f, 0.5f, 0.25f, 0.4f, 1f, 1.75f, 0.5f, 0.375f, 0.100f, -1f }; // Drunk Burn
            float[] P9 = new float[10] { 1.41f, 0.33f, 0.155f, 0.1f, 1f, 1.35f, 1.5f, 0.038f, 0f, 0f }; // Drunk Blood
            float[] P10 = new float[10] { 0.52f, 1f, 0.272f, 1f, 1f, 2f, 0.5f, 0.039f, 0.2f, 0f }; // Drunk To Much



            float[] PR = new float[10];
            if (Preset == Drunk_Preset.Default) PR = P1;
            if (Preset == Drunk_Preset.Drunk_A_Lot) PR = P2;
            if (Preset == Drunk_Preset.Drunk_Vodka) PR = P3;
            if (Preset == Drunk_Preset.Drunk_Poison) PR = P4;
            if (Preset == Drunk_Preset.Drunk_Yellow) PR = P5;
            if (Preset == Drunk_Preset.Drunk_Dark) PR = P6;
            if (Preset == Drunk_Preset.Drunk_Cold) PR = P7;
            if (Preset == Drunk_Preset.Drunk_Burn) PR = P8;
            if (Preset == Drunk_Preset.Drunk_Blood) PR = P9;
            if (Preset == Drunk_Preset.Drunk_To_Much) PR = P10;

            Speed = PR[0];
            Wavy = PR[1];
            Distortion = PR[2];
            DistortionWave = PR[3];
            Fade = PR[4];
            ColoredSaturate = PR[5];
            ColoredChange = PR[6];
            ChangeRed = PR[7];
            ChangeGreen = PR[8];
            ChangeBlue = PR[9];
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
                CameraPlay.Drunk_Switch = false;
                Object.Destroy(this);
            }
        }

    }
    void OnEnable()
    {
        Timer = 0;
    }
    void OnDisable()
    {
        if (SCMaterial)
        {
            DestroyImmediate(SCMaterial);
        }
    }
}