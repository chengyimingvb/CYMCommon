////////////////////////////////////////////
///// CameraPlay - by VETASOFT 2017    /////
////////////////////////////////////////////


using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraPlay_RainDrop : MonoBehaviour
{
    #region Variables
    public Shader SCShader;
    private float TimeX = 1.0f;
    [HideInInspector]
    public float _Fade = 1.0f;
    private Vector4 ScreenResolution;
    private Material SCMaterial;
    private Texture2D Texture2;
    private Texture2D Texture3;

   [HideInInspector] public int Count = 0;
    private Vector4[] Coord = new Vector4[4];
    public bool CamTurnOff = false;
     [HideInInspector]
    public float flashy = 1f;
       [HideInInspector]
    public float Distortion = 0.4f;
       [HideInInspector]
    public float Zoom = 0.25f;
       [HideInInspector]
    public float Duration = 1f;
    [HideInInspector]
    private float Timer = 1f;
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

    void OnEnable()
    {
        Timer = 0;
        flashy = 0;
    }



    void Start()
    {
        for (int c = 0; c < 4; c++)
        {
            Coord[c] = new Vector4(Random.Range(0f, 0.9f), Random.Range(0.3f, 1.1f), Random.Range(0, 255), -1);
        }

        Texture2 = Resources.Load("CameraPlay_Waterdrop_Anm2") as Texture2D;
        Texture3 = Resources.Load("CameraPlay_Waterdrop_Anm") as Texture2D;
        SCShader = Shader.Find("CameraPlay/RainDrop");
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

            curve.AddKey(0, 0.001f);
            curve.AddKey(0.01f, 0.01f);
            curve.AddKey(0.25f, 0.5f);
            curve.AddKey(0.50f, 0.75f);
            curve.AddKey(0.95f, 0.95f);
            curve.AddKey(1, 1);

            float fresult = curve.Evaluate(flashy);
            material.SetFloat("_TimeX", TimeX);
            material.SetFloat("_Value", fresult);

            curve = new AnimationCurve();
            curve.AddKey(0, 0.01f);
            curve.AddKey(64, 5f);
            curve.AddKey(128, 80f);
            curve.AddKey(255, 255f);
            curve.AddKey(300, 255f);


            for (int c = 0; c < 4; c++)
            {
                Coord[c].z += 0.5f;
                if (Coord[c].w == -1) Coord[c].x = -5.0f;
                if (Coord[c].z > 254) Coord[c] = new Vector4(Random.Range(0f, 0.9f), Random.Range(0.2f, 1.1f), 0, Random.Range(0, 3));
                material.SetVector("Coord" + (c + 1).ToString(), new Vector4(Coord[c].x, Coord[c].y, (int)curve.Evaluate(Coord[c].z), Coord[c].w));
            }


            material.SetFloat("count", (int)Count);
            material.SetTexture("Texture2", Texture2);
            material.SetTexture("Texture3", Texture3);
            material.SetVector("_ScreenResolution", new Vector4(sourceTexture.width, sourceTexture.height, 0.0f, 0.0f));
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
            SCShader = Shader.Find("CameraPlay/RainDrop");
            Texture2 = Resources.Load("CameraPlay_Waterdrop_Anm2") as Texture2D;
            Texture3 = Resources.Load("CameraPlay_Waterdrop_Anm") as Texture2D;
        }
#endif

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
                CameraPlay.RainDrop_Switch = false;
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