////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraPlay_BloodHit : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private Texture2D Texture2;
    private float TimeX = 1.0f;
    [Range(0, 1)] public float _Fade = 1;
    private Vector4 ScreenResolution;
    private Material SCMaterial;
    #endregion
    [HideInInspector][Range(0, 47)] public int cframe = 5;
    [HideInInspector]
    public float PosX = 0.0f;
    [HideInInspector]
    public float PosY = 0.0f;
    [HideInInspector] public float Distortion = 1f;
    [HideInInspector] public float Duration = 1f;
    [HideInInspector] public Color color;
    [HideInInspector] private float Timer = 1f;
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
    private float Rnd = 0;
    void Start()
    {
        Rnd = Random.Range(1, 7);
        PosX = Random.Range(0.25f, 0.75f);
        PosY = Random.Range(0.15f, 0.75f);
        if (Rnd == 0) Texture2 = Resources.Load("CameraPlay_Blood_Anm1") as Texture2D;
        if (Rnd == 1) Texture2 = Resources.Load("CameraPlay_Blood_Anm2") as Texture2D;
        if (Rnd == 2) Texture2 = Resources.Load("CameraPlay_Blood_Anm3") as Texture2D;
        if (Rnd == 3) Texture2 = Resources.Load("CameraPlay_Blood_Anm4") as Texture2D;
        if (Rnd == 4) Texture2 = Resources.Load("CameraPlay_Blood_Anm5") as Texture2D;
        if (Rnd == 5) Texture2 = Resources.Load("CameraPlay_Blood_Anm6") as Texture2D;
        if (Rnd == 6) Texture2 = Resources.Load("CameraPlay_Blood_Anm7") as Texture2D;

        SCShader = Shader.Find("CameraPlay/BloodHit");
        cframe = 0;
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
            TimeX += Time.deltaTime * 30;
            Timer += Time.deltaTime * (1 / Duration);
            if (Timer > 1f) Object.Destroy(this);
            _Fade = Timer;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 1);
            curve.AddKey(0.25f, 1f);
            curve.AddKey(0.375f, 1f);
            curve.AddKey(0.50f, 0.8f);
            curve.AddKey(0.62f, 0.75f);
            curve.AddKey(0.75f, 0.50f);
            curve.AddKey(1, 0);
            float fresult = curve.Evaluate(_Fade);


            cframe = (int)TimeX;
            material.SetFloat("_Fade", fresult);
            material.SetFloat("_cframe", (float)cframe);
            material.SetFloat("_PosX", PosX);
            material.SetFloat("_PosY", PosY);
            material.SetColor("color", color);
            material.SetTexture("Texture2", Texture2);
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
            SCShader = Shader.Find("CameraPlay/BloodHit");
            if (Rnd == 0) Texture2 = Resources.Load("CameraPlay_Blood_Anm1") as Texture2D;
            if (Rnd == 1) Texture2 = Resources.Load("CameraPlay_Blood_Anm2") as Texture2D;
            if (Rnd == 2) Texture2 = Resources.Load("CameraPlay_Blood_Anm3") as Texture2D;
            if (Rnd == 3) Texture2 = Resources.Load("CameraPlay_Blood_Anm4") as Texture2D;
            if (Rnd == 4) Texture2 = Resources.Load("CameraPlay_Blood_Anm5") as Texture2D;
            if (Rnd == 5) Texture2 = Resources.Load("CameraPlay_Blood_Anm6") as Texture2D;
            if (Rnd == 6) Texture2 = Resources.Load("CameraPlay_Blood_Anm7") as Texture2D;


        }
#endif

    }
    void OnEnable()
    {
        _Fade = 0;
        Timer = 0;
        cframe = 0;
    }

    void OnDisable()
    {
        if (SCMaterial)
        {
            DestroyImmediate(SCMaterial);
        }

    }


}