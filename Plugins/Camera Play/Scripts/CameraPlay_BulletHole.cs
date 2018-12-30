////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraPlay_BulletHole : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private Texture2D Texture2;
    private float TimeX = 1.0f;
    [Range(0, 1)] public float _Fade = 1;
    private Vector4 ScreenResolution;
    private Material SCMaterial;
    #endregion
    [Range(0, 47)] public int cframe = 5;
    [HideInInspector]
    public float PosX = 0.0f;
    [HideInInspector]
    public float PosY = 0.0f;
    [HideInInspector] public float Distortion = 1f;
    [HideInInspector] public float Duration = 1f;
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
        Rnd = Random.Range(0, 2);
        if (Rnd == 0) Texture2 = Resources.Load("CameraPlay_BulletHole_Anm") as Texture2D;
        if (Rnd == 1) Texture2 = Resources.Load("CameraPlay_BulletHole_Anm2") as Texture2D;
        SCShader = Shader.Find("CameraPlay/BulletHole");
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
            curve.AddKey(0.50f, 1);
            curve.AddKey(0.75f, 0.50f);
            curve.AddKey(1, 0);
            float fresult = curve.Evaluate(_Fade);

            cframe = (int)TimeX;
            material.SetFloat("_Fade", fresult);
            material.SetFloat("_cframe", (float)cframe);
            float PX = 0;
            float PY = 1;
            if (Rnd == 0) { PX = 0; PY = 0; }
            if (Rnd == 1) { PX = -0.03f; PY = -0.01f; }
            material.SetFloat("_PosX", PosX + PX);
            material.SetFloat("_PosY", PosY + PY);
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
            SCShader = Shader.Find("CameraPlay/BulletHole");
            if (Rnd == 0) Texture2 = Resources.Load("CameraPlay_BulletHole_Anm") as Texture2D;
            if (Rnd == 1) Texture2 = Resources.Load("CameraPlay_BulletHole_Anm2") as Texture2D;

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