using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using UnityEngine.Rendering;
using System.Linq;


#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
#endif

#if VEGETATION_STUDIO
using AwesomeTechnologies;
using AwesomeTechnologies.VegetationStudio;
#endif



[CustomEditor(typeof(RamSpline)), CanEditMultipleObjects]
public class RamSplineEditor : Editor
{
    //Vector2 scrollPos;

    RamSpline[] splines;
    RamSpline spline;

    Texture2D logo;
    int selectedPosition = -1;
    Vector3 pivotChange = Vector3.zero;

    bool terrainShapeShow = false;
    List<List<Vector3>> positionArray;

    GameObject meshGO;

    Mesh meshTerrain;
    public string[] shadowcastingOptions = new string[] { "Off", "On", "TwoSided", "ShadowsOnly" };

    public string[] toolbarStrings = new string[] {
        "Basic",
        "Points",
        "Vertex Color",
        "Flow Map",
        "Simulate\n[ALPHA] ",
        "Terrain\n[ALPHA V2]",
        "File Points",
        "Tips",
        "Manual",
        "Video Tutorials"
    };
    //, "Debug" };

    //	/// <summary>
    //	/// The button editing style.
    //	/// </summary>
    //	GUIStyle buttonEditingStyle;

    [MenuItem("GameObject/3D Object/Create River Spline")]
    static public void CreateSpline()
    {

        Selection.activeGameObject = RamSpline.CreateSpline(AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat")).gameObject;
    }

    void OnEnable()
    {
        splines = FindObjectsOfType<RamSpline>();
#if VEGETATION_STUDIO
        spline = (RamSpline)target;
        spline.vegetationMaskArea = spline.gameObject.GetComponent<VegetationMaskArea>();
#endif

    }

    private void OnDisable()
    {
        if (meshGO != null)
            DestroyImmediate(meshGO);
    }


    private void OnDestroy()
    {
        if (meshGO != null)
            DestroyImmediate(meshGO);
    }


    void CheckRotations()
    {
        bool nan = false;
        if (spline.controlPointsRotations == null)
        {
            spline.controlPointsRotations = new List<Quaternion>();
            nan = true;
        }
        if (spline.controlPoints.Count > spline.controlPointsRotations.Count)
        {
            nan = true;
            for (int i = 0; i < spline.controlPoints.Count - spline.controlPointsRotations.Count; i++)
            {
                spline.controlPointsRotations.Add(Quaternion.identity);
            }
        }
        for (int i = 0; i < spline.controlPointsRotations.Count; i++)
        {

            if (float.IsNaN(spline.controlPointsRotations[i].x) || float.IsNaN(spline.controlPointsRotations[i].y) || float.IsNaN(spline.controlPointsRotations[i].z) || float.IsNaN(spline.controlPointsRotations[i].w))
            {
                spline.controlPointsRotations[i] = Quaternion.identity;
                nan = true;
            }
            if (spline.controlPointsRotations[i].x == 0 && spline.controlPointsRotations[i].y == 0 && spline.controlPointsRotations[i].z == 0 && spline.controlPointsRotations[i].w == 0)
            {

                spline.controlPointsRotations[i] = Quaternion.identity;
                nan = true;
            }

            spline.controlPointsRotations[i] = Quaternion.Euler(spline.controlPointsRotations[i].eulerAngles);
        }



        if (nan)
            spline.GenerateSpline();
    }

    public override void OnInspectorGUI()
    {


        EditorGUILayout.Space();
        logo = (Texture2D)Resources.Load("logoRAM");



        Color baseCol = GUI.color;

        spline = (RamSpline)target;

        CheckRotations();

        if (spline.controlPoints.Count > spline.controlPointsSnap.Count)
        {

            for (int i = 0; i < spline.controlPoints.Count - spline.controlPointsSnap.Count; i++)
            {
                spline.controlPointsSnap.Add(0);
            }
        }

        if (spline.controlPoints.Count > spline.controlPointsMeshCurves.Count)
        {

            for (int i = 0; i < spline.controlPoints.Count - spline.controlPointsMeshCurves.Count; i++)
            {
                spline.controlPointsMeshCurves.Add(new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 0) }));
            }
        }



        if (spline.controlPoints.Count > spline.controlPointsOrientation.Count)
            spline.GenerateSpline();


        GUIContent btnTxt = new GUIContent(logo);

        var rt = GUILayoutUtility.GetRect(btnTxt, GUI.skin.label, GUILayout.ExpandWidth(false));
        rt.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, rt.center.y);

        GUI.Button(rt, btnTxt, GUI.skin.label);

        EditorGUI.BeginChangeCheck();

        //  GUILayout.Toolbar(spline.toolbarInt, toolbarStrings);
        int toolbarNew = GUILayout.SelectionGrid(spline.toolbarInt, toolbarStrings, 4);

        if (toolbarNew == 8)
        {
            toolbarNew = spline.toolbarInt;
            string[] guids1 = AssetDatabase.FindAssets("River Auto and Lava Volcano Environment Manual");
            Application.OpenURL("file:///" + Application.dataPath.Replace("Assets", "") + AssetDatabase.GUIDToAssetPath(guids1[0]));
        }
        if (toolbarNew == 9)
        {
            toolbarNew = spline.toolbarInt;
            Application.OpenURL("https://www.youtube.com/playlist?list=PLWMxYDHySK5Oc3prR5toZALZRH_7F_x-h");


        }


        spline.toolbarInt = toolbarNew;

        EditorGUILayout.Space();


        spline.drawOnMesh = false;
        spline.drawOnMeshFlowMap = false;


        if (spline.showFlowMap)
        {
            if (spline.uvRotation)
                spline.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_RotateUV", 1);
            else
                spline.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_RotateUV", 0);
        }



        if (spline.toolbarInt == 0)
        {

            EditorGUILayout.HelpBox("Add Point  - CTRL + Left Mouse Button Click \n" +
                "Add point between existing points - SHIFT + Left Button Click \n" +
                "Remove point - CTRL + SHIFT + Left Button Click", MessageType.Info);
            EditorGUI.indentLevel++;

            AddPointAtEnd();

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            MeshSettings();



            GUILayout.Label("UV settings:", EditorStyles.boldLabel);
            if (spline.beginningSpline == null && spline.endingSpline == null)
            {
                spline.uvScale = EditorGUILayout.FloatField("UV scale (texture tiling)", spline.uvScale);
            }
            else
            {

                spline.uvScaleOverride = EditorGUILayout.Toggle("Parent UV scale override", spline.uvScaleOverride);
                if (!spline.uvScaleOverride)
                {
                    if (spline.beginningSpline != null)
                        spline.uvScale = spline.beginningSpline.uvScale;
                    if (spline.endingSpline != null)
                        spline.uvScale = spline.endingSpline.uvScale;


                    GUI.enabled = false;
                }
                spline.uvScale = EditorGUILayout.FloatField("UV scale (texture tiling)", spline.uvScale);
                GUI.enabled = true;



            }
            spline.invertUVDirection = EditorGUILayout.Toggle("Invert UV direction", spline.invertUVDirection);

            spline.uvRotation = EditorGUILayout.Toggle("Rotate UV", spline.uvRotation);


            GUILayout.Label("Lightning settings:", EditorStyles.boldLabel);
            spline.receiveShadows = EditorGUILayout.Toggle("Receive Shadows", spline.receiveShadows);



            spline.shadowCastingMode = (ShadowCastingMode)EditorGUILayout.EnumPopup("Shadow Casting Mode", spline.shadowCastingMode);


            EditorGUILayout.Space();

            //SetMaterials ();


            EditorGUILayout.Space();

            ParentingSplineUI();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            GUILayout.Label("Mesh spliting:", EditorStyles.boldLabel);
            spline.generateMeshParts = EditorGUILayout.Toggle("Split mesh into parts", spline.generateMeshParts);
            if (spline.generateMeshParts)
            {
                spline.meshPartsCount = EditorGUILayout.IntSlider("Parts", spline.meshPartsCount, 2, (int)((1 / (float)spline.traingleDensity) * (spline.controlPoints.Count - 1) * 0.5));

            }

            EditorGUILayout.Space();

            GUILayout.Label("Object settings:", EditorStyles.boldLabel);


            EditorGUILayout.Space();
            if (GUILayout.Button("Set object pivot to center"))
            {
                Vector3 center = spline.meshfilter.sharedMesh.bounds.center;

                ChangePivot(center);

            }
            EditorGUILayout.BeginHorizontal();
            {

                if (GUILayout.Button("Set object pivot position"))
                {
                    ChangePivot(pivotChange - spline.transform.position);
                }
                pivotChange = EditorGUILayout.Vector3Field("", pivotChange);



            }
            EditorGUILayout.EndHorizontal();






            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Regenerate spline", "Racalculates whole mesh")))
            {
                spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
                RegeneratBiomeMask();
#endif
            }
            spline.generateOnStart = EditorGUILayout.Toggle("Regenerate on Start", spline.generateOnStart);

            EditorGUILayout.Space();
            if (GUILayout.Button("Export as mesh"))
            {

                string path = EditorUtility.SaveFilePanelInProject("Save river mesh", "", "asset", "Save river mesh");


                if (path.Length != 0 && spline.meshfilter.sharedMesh != null)
                {

                    AssetDatabase.CreateAsset(spline.meshfilter.sharedMesh, path);

                    AssetDatabase.Refresh();
                    spline.GenerateSpline();
                }

            }



            EditorGUILayout.Space();
            GUILayout.Label("Debug Settings: ", EditorStyles.boldLabel);


            spline.debug = EditorGUILayout.Toggle("Show debug gizmos", spline.debug);

            EditorGUILayout.Space();
            EditorGUILayout.Space();


#if VEGETATION_STUDIO
            EditorGUILayout.Space();
            GUILayout.Label("Vegetation Studio: ", EditorStyles.boldLabel);
            spline.vegetationMaskPerimeter = EditorGUILayout.FloatField("Vegetation Mask Perimeter", spline.vegetationMaskPerimeter);
            if (spline.vegetationMaskArea == null && GUILayout.Button("Add Vegetation Mask Area"))
            {
                spline.vegetationMaskArea = spline.gameObject.AddComponent<VegetationMaskArea>();
                RegenerateVegetationMask();
            }
            if (spline.vegetationMaskArea != null && GUILayout.Button("Calculate hull outline"))
            {

                RegenerateVegetationMask();
            }



#endif

#if VEGETATION_STUDIO_PRO
            EditorGUILayout.Space();
            GUILayout.Label("Vegetation Studio Pro: ", EditorStyles.boldLabel);
            spline.vegetationBlendDistance = EditorGUILayout.FloatField("Vegetation Blend Distance", spline.vegetationBlendDistance);
            spline.biomMaskResolution = EditorGUILayout.Slider("Mask Resolution", spline.biomMaskResolution, 0.1f, 1);
            if (spline.biomeMaskArea != null)
                spline.refreshMask = EditorGUILayout.Toggle("Auto Refresh Biome Mask", spline.refreshMask);

            if (GUILayout.Button("Add Vegetation Biom Mask Area"))
            {
                spline.GenerateSpline();
                if (spline.biomeMaskArea == null)
                {
                    spline.biomeMaskArea = spline.GetComponentInChildren<BiomeMaskArea>();
                    if (spline.biomeMaskArea == null)
                    {
                        GameObject maskObject = new GameObject("MyMask");
                        maskObject.transform.SetParent(spline.transform);
                        spline.biomeMaskArea = maskObject.AddComponent<BiomeMaskArea>();
                        spline.biomeMaskArea.BiomeType = BiomeType.River;
                    }
                }

                if (spline.biomeMaskArea == null)
                    return;

                RegeneratBiomeMask(false);

            }



#endif


        }
        else if (spline.toolbarInt == 1)
        {

            PointsUI();

        }
        else if (spline.toolbarInt == 2)
        {

            DrawVertexColorsUI();

        }
        else if (spline.toolbarInt == 3)
        {

            DrawFlowColorsUI();

        }
        else if (spline.toolbarInt == 4)
        {

            EditorGUILayout.HelpBox("\nSet 1 point and R.A.M will show potential river direction.\n", MessageType.Info);
            GUILayout.Label("River simulation:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            spline.simulatedRiverLength = EditorGUILayout.FloatField("Simulation river length", spline.simulatedRiverLength);
            if (spline.simulatedRiverLength < 1)
                spline.simulatedRiverLength = 1;
            spline.simulatedRiverPoints = EditorGUILayout.IntSlider("Simulation river points", spline.simulatedRiverPoints, 1, 100);
            spline.simulationMinStepSize = EditorGUILayout.Slider("Simulation min step size", spline.simulationMinStepSize, 0.5f, 5);
            EditorGUILayout.Space();

            if (GUILayout.Button("Show simulated River"))
            {
                SimulateRiver();
            }
            if (GUILayout.Button("Generate Simulated River"))
            {
                SimulateRiver(true);
            }
            if (GUILayout.Button("Remove points except first"))
            {
                spline.RemovePoints(0);
            }
            EditorGUI.indentLevel--;
        }
        else if (spline.toolbarInt == 5)
        {
            RiverChannel();
        }
        else if (spline.toolbarInt == 6)
        {
            FilesManager();
        }
        else if (spline.toolbarInt == 7)
        {

            Tips();
        }

        //if (spline.toolbarInt == 6)
        //{

        //    DebugOptions();
        //}


        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Spline changed");
            spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
            RegeneratBiomeMask();
#endif

        }

        EditorGUILayout.Space();

        if (spline.beginningSpline)
        {
            if (!spline.beginningSpline.endingChildSplines.Contains(spline))
            {
                spline.beginningSpline.endingChildSplines.Add(spline);

            }
        }

        if (spline.endingSpline)
        {
            if (!spline.endingSpline.beginnigChildSplines.Contains(spline))
            {
                spline.endingSpline.beginnigChildSplines.Add(spline);

            }
        }
    }

#if VEGETATION_STUDIO
    private void RegenerateVegetationMask()
    {
        spline.vegetationMaskArea.AdditionalGrassPerimiterMax = spline.vegetationMaskPerimeter;
        spline.vegetationMaskArea.AdditionalLargeObjectPerimiterMax = spline.vegetationMaskPerimeter;
        spline.vegetationMaskArea.AdditionalObjectPerimiterMax = spline.vegetationMaskPerimeter;
        spline.vegetationMaskArea.AdditionalPlantPerimiterMax = spline.vegetationMaskPerimeter;
        spline.vegetationMaskArea.AdditionalTreePerimiterMax = spline.vegetationMaskPerimeter;
        spline.vegetationMaskArea.GenerateHullNodes(spline.vegetationMaskArea.ReductionTolerance);

        spline.GenerateSpline();
        List<Vector3> worldspacePointList = new List<Vector3>();
        for (int i = 0; i < spline.pointsUp.Count; i += 5)
        {
            Vector3 position = spline.transform.TransformPoint(spline.pointsUp[i]) + (spline.transform.TransformPoint(spline.pointsUp[i]) - spline.transform.TransformPoint(spline.pointsDown[i])).normalized * spline.vegetationMaskPerimeter;

            worldspacePointList.Add(position);
        }
        for (int i = 0; i < spline.pointsDown.Count; i += 5)
        {
            int ind = spline.pointsDown.Count - i - 1;
            Vector3 position = spline.transform.TransformPoint(spline.pointsDown[ind]) + (spline.transform.TransformPoint(spline.pointsDown[ind]) - spline.transform.TransformPoint(spline.pointsUp[ind])).normalized * spline.vegetationMaskPerimeter;
            worldspacePointList.Add(position);
        }

        spline.vegetationMaskArea.ClearNodes();

        for (var i = 0; i <= worldspacePointList.Count - 1; i++)
        {
            spline.vegetationMaskArea.AddNodeToEnd(worldspacePointList[i]);
        }
        spline.vegetationMaskArea.UpdateVegetationMask();
    }
#endif

#if VEGETATION_STUDIO_PRO
    void RegeneratBiomeMask(bool checkAuto = true)
    {

        if (checkAuto && !spline.refreshMask)
            return;

        if (spline.biomeMaskArea == null)
            return;


        List<bool> disableEdges = new List<bool>();
        List<Vector3> worldspacePointList = new List<Vector3>();
        for (int i = 0; i < spline.pointsUp.Count; i += (int)(1 / (float)spline.biomMaskResolution))
        {
            Vector3 position = spline.transform.TransformPoint(spline.pointsUp[i]) + (spline.transform.TransformPoint(spline.pointsUp[i]) - spline.transform.TransformPoint(spline.pointsDown[i])).normalized * spline.vegetationBlendDistance;

            worldspacePointList.Add(position);

            if (i == 0 || (i + (int)(1 / (float)spline.biomMaskResolution)) >= spline.pointsUp.Count)
            {
                disableEdges.Add(true);
            }
            else
            {
                disableEdges.Add(false);
            }
        }
        for (int i = 0; i < spline.pointsDown.Count; i += (int)(1 / (float)spline.biomMaskResolution))
        {
            int ind = spline.pointsDown.Count - i - 1;
            Vector3 position = spline.transform.TransformPoint(spline.pointsDown[ind]) + (spline.transform.TransformPoint(spline.pointsDown[ind]) - spline.transform.TransformPoint(spline.pointsUp[ind])).normalized * spline.vegetationBlendDistance;
            worldspacePointList.Add(position);

            if (i == 0 || (i + (int)(1 / (float)spline.biomMaskResolution)) >= spline.pointsDown.Count)
            {
                disableEdges.Add(true);
            }
            else
            {
                disableEdges.Add(false);
            }
        }

        spline.biomeMaskArea.ClearNodes();

        spline.biomeMaskArea.AddNodesToEnd(worldspacePointList.ToArray(), disableEdges.ToArray());



        //these have default values but you can set them if you want a different default setting
        spline.biomeMaskArea.BlendDistance = spline.vegetationBlendDistance * 0.5f;
        spline.biomeMaskArea.NoiseScale = 5;
        spline.biomeMaskArea.UseNoise = true;

        if (spline.currentProfile != null)
        {
            spline.biomeMaskArea.BiomeType = (BiomeType)spline.currentProfile.biomeType;

        }
        else
            spline.biomeMaskArea.BiomeType = BiomeType.River;

        //These 3 curves holds the blend curves for vegetation and textures. they have default values;
        //biomeMaskArea.BlendCurve;
        //biomeMaskArea.InverseBlendCurve;
        //biomeMaskArea.TextureBlendCurve;

        spline.biomeMaskArea.UpdateBiomeMask();

    }
#endif
    private void SimulateRiver(bool generate = false)
    {
        if (spline.controlPoints.Count == 0)
        {
            Debug.Log("Add one point to start Simulating River");
            return;
        }


        Ray ray = new Ray();
        RaycastHit hit;



        Vector3 lastPosition = spline.transform.TransformPoint((Vector3)spline.controlPoints[0]);

        List<Vector3> positionsGenerated = new List<Vector3>();
        List<Vector3> samplePositionsGenerated = new List<Vector3>();
        samplePositionsGenerated.Add(lastPosition);

        Debug.DrawRay(lastPosition + new Vector3(0, 3, 0), Vector3.down * 20, Color.white, 3);

        float length = 0;
        int i = -1;
        do
        {
            i++;
            if (i > 0)
            {
                Vector3 maxPosition = Vector3.zero;
                float max = float.MinValue;
                bool foundNextPositon = false;
                for (float j = spline.simulationMinStepSize; j < 10; j += 0.1f)
                {
                    for (int angle = 0; angle < 36; angle++)
                    {
                        float x = j * Mathf.Cos(angle);
                        float z = j * Mathf.Sin(angle);

                        ray.origin = lastPosition + new Vector3(0, 10, 0) + new Vector3(x, 0, z);
                        ray.direction = Vector3.down * 1000;

                        if (Physics.Raycast(ray, out hit, 10000))
                        {


                            if (hit.distance > max)
                            {
                                bool goodPoint = true;


                                foreach (var item in positionsGenerated)
                                {
                                    if (Vector3.Distance(item, lastPosition) > Vector3.Distance(item, hit.point) + 0.5f)
                                    {
                                        goodPoint = false;
                                        break;
                                    }
                                }
                                if (goodPoint)
                                {


                                    foundNextPositon = true;
                                    max = hit.distance;
                                    maxPosition = hit.point;
                                }
                            }
                            //Debug.DrawRay(ray.origin, ray.direction, Color.Lerp(Color.red, Color.green, hit.distance / 100.0f), 3);

                        }

                    }
                    if (foundNextPositon)
                        break;
                }
                if (!foundNextPositon)
                    break;


                //Debug.DrawRay(maxPosition + new Vector3(0, 5, 0), ray.direction*10, Color.green, 3);

                length += Vector3.Distance(maxPosition, lastPosition);
                if (i % spline.simulatedRiverPoints == 0 || spline.simulatedRiverLength <= length)
                {
                    Debug.DrawRay(maxPosition + new Vector3(0, 5, 0), ray.direction * 20, Color.white, 3);

                    samplePositionsGenerated.Add(maxPosition);

                    if (generate)
                    {
                        Vector4 newPosition = maxPosition - spline.transform.position;
                        if (spline.controlPoints.Count > 0)
                            newPosition.w = spline.controlPoints[spline.controlPoints.Count - 1].w;
                        else
                            newPosition.w = spline.width;


                        spline.controlPointsRotations.Add(Quaternion.identity);
                        spline.controlPoints.Add(newPosition);
                        spline.controlPointsSnap.Add(0);
                        spline.controlPointsMeshCurves.Add(new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 0) }));
                    }
                }
                else
                    Debug.DrawRay(maxPosition + new Vector3(0, 3, 0), ray.direction * 20, Color.Lerp(Color.red, Color.green, i / (float)spline.simulatedRiverLength), 3);


                positionsGenerated.Add(lastPosition);
                lastPosition = maxPosition;

            }




        } while (spline.simulatedRiverLength > length);

        for (i = 0; i < samplePositionsGenerated.Count - 1; i++)
        {
            Debug.DrawLine(samplePositionsGenerated[i], samplePositionsGenerated[i + 1], Color.white, 3);

        }
    }

    void RiverChannel()
    {



        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Space();
        GUILayout.Label("Terrain carve:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        spline.terrainCarve = EditorGUILayout.CurveField("Terrain carve", spline.terrainCarve);
        spline.terrainSmoothMultiplier = EditorGUILayout.Slider("Smooth", spline.terrainSmoothMultiplier, 0, 20);
        spline.distSmooth = EditorGUILayout.FloatField("Smooth distance", spline.distSmooth);
        //spline.distSmoothStart = EditorGUILayout.FloatField("Smooth start distance", spline.distSmoothStart);
        EditorGUI.indentLevel--;

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "River terrain changed");
            if (terrainShapeShow)
                ShowTerrainCarve();
        }

        if (!terrainShapeShow && GUILayout.Button("Show terrain shape"))
        {
            ShowTerrainCarve();
            terrainShapeShow = true;

        }
        if (terrainShapeShow && GUILayout.Button("Hide terrain shape"))
        {
            if (meshGO != null)
                DestroyImmediate(meshGO);

            terrainShapeShow = false;
        }


        if (GUILayout.Button("Carve terrain"))
        {
            ShowTerrainCarve();
            TerrainCarve();
            terrainShapeShow = false;

        }



        EditorGUILayout.Space();
        EditorGUI.indentLevel++;




        EditorGUI.BeginChangeCheck();
        spline.terrainPaintCarve = EditorGUILayout.CurveField("Terrain paint", spline.terrainPaintCarve);

        Terrain terrain = Terrain.activeTerrain;
        int splatNumber = terrain.terrainData.splatPrototypes.Length;
        string[] options = new string[splatNumber];
        for (int i = 0; i < splatNumber; i++)
        {
            options[i] = i + " - ";
            if (terrain.terrainData.splatPrototypes[i].texture != null)
            {
                options[i] += terrain.terrainData.splatPrototypes[i].texture.name;
            }
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("    Splat id:");



        spline.currentSplatMap = EditorGUILayout.Popup(spline.currentSplatMap, options);
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "River terrain changed");
        }

        EditorGUI.indentLevel--;

        if (GUILayout.Button("Paint Terrain"))
        {
            TerrainPaint();
        }

    }



    void ShowTerrainCarve()
    {
        if (meshGO != null)
            DestroyImmediate(meshGO);

        Mesh mesh = spline.meshfilter.sharedMesh;
        RaycastHit hit;
        Vector3 rayPointDown;
        Vector3 rayPointUp;
        Vector3 point;

        spline.detailTerrainForward = 2;
        spline.detailTerrain = 10;


        spline.terrainAdditionalWidth = spline.distSmooth + spline.distSmoothStart;

        positionArray = new List<List<Vector3>>();

        for (int i = 0; i < spline.pointsDown.Count - 1; i++)
        {

            for (int tf = 0; tf <= spline.detailTerrainForward; tf++)
            {
                List<Vector3> positionArrayRow = new List<Vector3>();

                rayPointDown = Vector3.Lerp(spline.pointsDown[i], spline.pointsDown[i + 1], tf / (float)spline.detailTerrainForward);
                rayPointUp = Vector3.Lerp(spline.pointsUp[i], spline.pointsUp[i + 1], tf / (float)spline.detailTerrainForward);


                Vector3 diff = rayPointDown - rayPointUp;
                float diffMagintude = diff.magnitude;
                rayPointDown += diff * 0.05f;
                rayPointUp -= diff * 0.05f;

                diff.Normalize();
                Vector3 rayPointDownNew = rayPointDown + diff * spline.terrainAdditionalWidth * 0.5f;
                Vector3 rayPointUpNew = rayPointUp - diff * spline.terrainAdditionalWidth * 0.5f;


                if (spline.terrainAdditionalWidth > 0)
                {
                    for (int t = 0; t < spline.detailTerrain; t++)
                    {

                        point = Vector3.Lerp(rayPointDownNew, rayPointDown, t / (float)spline.detailTerrain) + spline.transform.position;
                        if (Physics.Raycast(point + Vector3.up * 500, Vector3.down, out hit))
                        {
                            float evaluate = 1 - t / (float)spline.detailTerrain;
                            evaluate *= spline.terrainAdditionalWidth;
                            float height = point.y + spline.terrainCarve.Evaluate(-evaluate);

                            float smoothValue = t / (float)spline.detailTerrain;
                            smoothValue = Mathf.Pow(smoothValue, spline.terrainSmoothMultiplier);

                            height = Mathf.Lerp(hit.point.y, height, smoothValue);

                            Vector3 newPos = new Vector3(hit.point.x, height, hit.point.z);
                            positionArrayRow.Add(newPos);



                        }
                        else
                            positionArrayRow.Add(point);
                    }
                }
                for (int t = 0; t <= spline.detailTerrain; t++)
                {

                    point = Vector3.Lerp(rayPointDown, rayPointUp, t / (float)spline.detailTerrain) + spline.transform.position;
                    if (Physics.Raycast(point + Vector3.up * 500, Vector3.down, out hit))
                    {
                        float height = point.y + spline.terrainCarve.Evaluate(diffMagintude * (0.5f - Mathf.Abs(0.5f - t / (float)spline.detailTerrain)));

                        float smoothValue = 1 - 2 * Mathf.Abs(t / (float)spline.detailTerrain - 0.5f);
                        smoothValue = Mathf.Pow(smoothValue, spline.terrainSmoothMultiplier);

                        height = Mathf.Lerp(hit.point.y, height, 1);

                        Vector3 newPos = new Vector3(hit.point.x, height, hit.point.z);

                        positionArrayRow.Add(newPos);


                    }
                    else
                        positionArrayRow.Add(point);
                }



                if (spline.terrainAdditionalWidth > 0)
                {
                    for (int t = 1; t <= spline.detailTerrain; t++)
                    {
                        point = Vector3.Lerp(rayPointUp, rayPointUpNew, t / (float)spline.detailTerrain) + spline.transform.position;
                        if (Physics.Raycast(point + Vector3.up * 50, Vector3.down, out hit))
                        {
                            float evaluate = t / (float)spline.detailTerrain;
                            evaluate *= spline.terrainAdditionalWidth;

                            float height = point.y + spline.terrainCarve.Evaluate(-evaluate);

                            float smoothValue = 1 - t / (float)spline.detailTerrain;
                            smoothValue = Mathf.Pow(smoothValue, spline.terrainSmoothMultiplier);

                            height = Mathf.Lerp(hit.point.y, height, smoothValue);
                            Vector3 newPos = new Vector3(hit.point.x, height, hit.point.z);
                            positionArrayRow.Add(newPos);

                        }
                        else
                            positionArrayRow.Add(point);
                    }


                }
                positionArray.Add(positionArrayRow);
            }
        }


        meshTerrain = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (var positionRow in positionArray)
        {
            foreach (var vert in positionRow)
            {
                vertices.Add(vert);
            }
        }

        for (int i = 0; i < positionArray.Count - 1; i++)
        {
            List<Vector3> rowPosition = positionArray[i];

            for (int j = 0; j < rowPosition.Count - 1; j++)
            {
                triangles.Add(j + i * rowPosition.Count);
                triangles.Add(j + (i + 1) * rowPosition.Count);
                triangles.Add((j + 1) + i * rowPosition.Count);

                triangles.Add((j + 1) + i * rowPosition.Count);
                triangles.Add(j + (i + 1) * rowPosition.Count);
                triangles.Add((j + 1) + (i + 1) * rowPosition.Count);

            }
        }


        meshTerrain.SetVertices(vertices);
        meshTerrain.SetTriangles(triangles, 0);

        meshTerrain.RecalculateNormals();
        meshTerrain.RecalculateTangents();
        meshTerrain.RecalculateBounds();

        //if (meshGO == null)
        //{
        meshGO = new GameObject("TerrainMesh");
        meshGO.hideFlags = HideFlags.HideAndDontSave;
        meshGO.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshGO.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Debug Terrain Carve"));
        meshRenderer.sharedMaterial.color = new Color(0, 0.5f, 0);

        // }
        meshGO.transform.position = Vector3.zero;
        meshGO.GetComponent<MeshFilter>().sharedMesh = meshTerrain;

    }

    void TerrainCarve()
    {
        bool debugLines = false;


        double time = EditorApplication.timeSinceStartup;

        foreach (Terrain terrain in Terrain.activeTerrains)
        {

            TerrainData terrainData = terrain.terrainData;
            float sizeX = terrain.terrainData.size.x;
            float sizeY = terrain.terrainData.size.y;
            float sizeZ = terrain.terrainData.size.z;
            float terrainTowidth = (1 / (float)sizeX * (terrainData.heightmapWidth - 1));
            float terrainToheight = (1 / (float)sizeZ * (terrainData.heightmapHeight - 1));
            Undo.RegisterCompleteObjectUndo(terrainData, "River curve");

            float minX;
            float maxX;
            float minZ;
            float maxZ;
            TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
            MeshCollider meshCollider = meshGO.gameObject.AddComponent<MeshCollider>();
            Physics.autoSyncTransforms = false;

            List<Vector3> transformPointUp = new List<Vector3>();
            List<Vector3> transformPointDown = new List<Vector3>();

            int pointsCount = 5;
            int pointsStart = 0;//spline.pointsUp.Count

            List<Vector2> done = new List<Vector2>();

            //List<Vector3> positionArray = new List<Vector3>();

            Vector3 pointOne = Vector3.zero;
            Vector3 pointTwo = Vector3.zero;

            for (pointsStart = 0; pointsStart < spline.pointsUp.Count; pointsStart = Mathf.Clamp(pointsStart + pointsCount - 1, 0, spline.pointsUp.Count))
            {
                int end = Mathf.Min(pointsStart + pointsCount, spline.pointsUp.Count);
                int currentCount = Mathf.Min(pointsCount, spline.pointsUp.Count - pointsStart);

                transformPointUp.Clear();
                transformPointDown.Clear();

                for (int i = pointsStart; i < end; i++)
                {
                    transformPointUp.Add(spline.transform.TransformPoint(spline.pointsUp[i]));
                    transformPointDown.Add(spline.transform.TransformPoint(spline.pointsDown[i]));
                }

                minX = float.MaxValue;
                maxX = float.MinValue;
                minZ = float.MaxValue;
                maxZ = float.MinValue;



                for (int i = 0; i < transformPointUp.Count; i++)
                {
                    Vector3 point = transformPointUp[i];


                    if (minX > point.x)
                        minX = point.x;

                    if (maxX < point.x)
                        maxX = point.x;

                    if (minZ > point.z)
                        minZ = point.z;

                    if (maxZ < point.z)
                        maxZ = point.z;
                }

                for (int i = 0; i < transformPointDown.Count; i++)
                {
                    Vector3 point = transformPointDown[i];


                    if (minX > point.x)
                        minX = point.x;

                    if (maxX < point.x)
                        maxX = point.x;

                    if (minZ > point.z)
                        minZ = point.z;

                    if (maxZ < point.z)
                        maxZ = point.z;
                }

                minX -= terrain.transform.position.x + spline.distSmooth;
                maxX -= terrain.transform.position.x - spline.distSmooth;

                minZ -= terrain.transform.position.z + spline.distSmooth;
                maxZ -= terrain.transform.position.z - spline.distSmooth;


                minX = minX * terrainToheight;
                maxX = maxX * terrainToheight;

                minZ = minZ * terrainTowidth;
                maxZ = maxZ * terrainTowidth;

                maxX = Mathf.Ceil(Mathf.Clamp(maxX + 1, 0, (terrainData.heightmapWidth)));
                minZ = Mathf.Floor(Mathf.Clamp(minZ, 0, (terrainData.heightmapHeight)));
                maxZ = Mathf.Ceil(Mathf.Clamp(maxZ + 1, 0, (terrainData.heightmapHeight)));
                minX = Mathf.Floor(Mathf.Clamp(minX, 0, (terrainData.heightmapWidth)));

                float[,] heightmapData = terrainData.GetHeights((int)minX, (int)minZ, (int)(maxX - minX), (int)(maxZ - minZ));

                Vector3 position = Vector3.zero;
                Vector3 pointMin = Vector3.zero;

                for (int x = 0; x < heightmapData.GetLength(0); x++)
                {


                    for (int z = 0; z < heightmapData.GetLength(1); z++)
                    {


                        position.x = (z + minX) / (float)terrainToheight + terrain.transform.position.x;
                        position.z = (x + minZ) / (float)terrainTowidth + terrain.transform.position.z;


                        Ray ray = new Ray(position + Vector3.up * 3000, Vector3.down);
                        RaycastHit hit;
                        if (meshCollider.Raycast(ray, out hit, 10000))
                        {
                            //Debug.DrawLine(position, position + Vector3.up * 60f, Color.black, 10);

                            float height = hit.point.y;
                            heightmapData[x, z] = height / (float)sizeY;

                            //done.Add(new Vector2(z + minX, x + minZ));
                            //positionArray.Add(new Vector3(position.x, heightmapData[x, z] * sizeY, position.z));

                            // Debug.DrawLine(hit.point - Vector3.down * 2, hit.point + Vector3.down * 4, Color.magenta, 10);

                            if (debugLines)
                            {
                                Debug.DrawLine(hit.point, hit.point + Vector3.up * 0.5f, Color.magenta, 10);

                            }


                        }

                    }
                }

                terrainData.SetHeights((int)minX, (int)minZ, heightmapData);


            }
            DestroyImmediate(meshCollider);
            terrain.Flush();
        }

        Physics.autoSyncTransforms = true;





        if (meshGO != null)
            DestroyImmediate(meshGO);


        time = EditorApplication.timeSinceStartup - time;
    }



    void TerrainPaint()
    {

        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;

        float sizeX = terrain.terrainData.size.x;
        //float sizeY = terrain.terrainData.size.y;
        float sizeZ = terrain.terrainData.size.z;
        Undo.RegisterCompleteObjectUndo(terrainData, "Paint lake");
        Undo.RegisterCompleteObjectUndo(terrain, "Terrain draw texture");
        Undo.RegisterCompleteObjectUndo(terrainData.alphamapTextures, "alpha");

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        for (int i = 0; i < spline.pointsUp.Count; i++)
        {
            Vector3 point = spline.transform.TransformPoint(spline.pointsUp[i]);


            if (minX > point.x)
                minX = point.x;

            if (maxX < point.x)
                maxX = point.x;

            if (minZ > point.z)
                minZ = point.z;

            if (maxZ < point.z)
                maxZ = point.z;
        }

        for (int i = 0; i < spline.pointsDown.Count; i++)
        {
            Vector3 point = spline.transform.TransformPoint(spline.pointsDown[i]);


            if (minX > point.x)
                minX = point.x;

            if (maxX < point.x)
                maxX = point.x;

            if (minZ > point.z)
                minZ = point.z;

            if (maxZ < point.z)
                maxZ = point.z;
        }


        //Debug.DrawLine(new Vector3(minX, 0, minZ), new Vector3(minX, 0, minZ) + Vector3.up * 100, Color.green, 3);
        // Debug.DrawLine(new Vector3(maxX, 0, maxZ), new Vector3(maxX, 0, maxZ) + Vector3.up * 100, Color.blue, 3);


        float terrainTowidth = (1 / (float)sizeX * terrainData.alphamapHeight);
        float terrainToheight = (1 / (float)sizeZ * terrainData.alphamapWidth);

        minX -= terrain.transform.position.x;
        maxX -= terrain.transform.position.x;

        minZ -= terrain.transform.position.z;
        maxZ -= terrain.transform.position.z;

        minX = minX * terrainToheight;
        maxX = maxX * terrainToheight;

        minZ = minZ * terrainTowidth;
        maxZ = maxZ * terrainTowidth;

        minX = (int)Mathf.Clamp(minX, 0, terrainData.alphamapWidth);
        maxX = (int)Mathf.Clamp(maxX, 0, terrainData.alphamapWidth);
        minZ = (int)Mathf.Clamp(minZ, 0, terrainData.alphamapHeight);
        maxZ = (int)Mathf.Clamp(maxZ, 0, terrainData.alphamapHeight);

        float[,,] alphamapData = terrainData.GetAlphamaps((int)minX, (int)minZ, (int)(maxX - minX), (int)(maxZ - minZ));
        // Debug.Log(minX + " " + minZ);
        MeshCollider meshCollider = spline.gameObject.AddComponent<MeshCollider>();


        Vector3 position = Vector3.zero;
        for (int x = 0; x < alphamapData.GetLength(0); x++)
        {
            for (int z = 0; z < alphamapData.GetLength(1); z++)
            {


                position.x = (z + minX) / (float)terrainToheight + terrain.transform.position.x;
                position.z = (x + minZ) / (float)terrainTowidth + terrain.transform.position.z;

                Ray ray = new Ray(position + Vector3.up * 1000, Vector3.down);
                RaycastHit hit;

                if (meshCollider.Raycast(ray, out hit, 10000))
                {
                    //Debug.DrawLine(position, position + Vector3.up * 30, Color.green, 3);


                    float minDist = float.MaxValue;
                    for (int i = 0; i < spline.pointsUp.Count - 1; i++)
                    {
                        int idOne = i;
                        int idTwo = (i + 1) % spline.pointsUp.Count;

                        float dist = DistancePointLine(hit.point, spline.transform.TransformPoint(spline.pointsUp[idOne]), spline.transform.TransformPoint(spline.pointsUp[idTwo]));
                        if (minDist > dist)
                            minDist = dist;
                    }
                    for (int i = 0; i < spline.pointsDown.Count - 1; i++)
                    {
                        int idOne = i;
                        int idTwo = (i + 1) % spline.pointsDown.Count;

                        float dist = DistancePointLine(hit.point, spline.transform.TransformPoint(spline.pointsDown[idOne]), spline.transform.TransformPoint(spline.pointsDown[idTwo]));
                        if (minDist > dist)
                            minDist = dist;
                    }

                    float oldValue = alphamapData[x, z, spline.currentSplatMap];

                    alphamapData[x, z, spline.currentSplatMap] = Mathf.Lerp(alphamapData[x, z, spline.currentSplatMap], 1, spline.terrainPaintCarve.Evaluate(minDist));


                    for (int l = 0; l < terrainData.splatPrototypes.Length; l++)
                    {
                        if (l != spline.currentSplatMap)
                        {
                            alphamapData[x, z, l] = oldValue == 1 ? 0 : Mathf.Clamp01(alphamapData[x, z, l] * ((1 - alphamapData[x, z, spline.currentSplatMap]) / (1 - oldValue)));

                        }
                    }

                }
                // else
                //     Debug.DrawLine(position, position + Vector3.up * 30, Color.red, 3);



            }
        }
        //Debug.DrawRay(new Vector3((minX) / (float)terrainToheight, 0, (minZ) / (float)terrainTowidth), Vector3.up * 30, Color.black, 3);
        // Debug.DrawRay(new Vector3((maxX) / (float)terrainToheight, 0, (maxZ) / (float)terrainTowidth), Vector3.up * 30, Color.black, 3);

        DestroyImmediate(meshCollider);
        terrainData.SetAlphamaps((int)minX, (int)minZ, alphamapData);
        terrain.Flush();
    }

    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Distance(ProjectPointLine(point, lineStart, lineEnd), point);
    }
    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector2 = lineEnd - lineStart;
        float magnitude = vector2.magnitude;
        Vector3 lhs = vector2;
        if (magnitude > 1E-06f)
        {
            lhs = (Vector3)(lhs / magnitude);
        }
        float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
        return (lineStart + ((Vector3)(lhs * num2)));
    }


    void DebugOptions()
    {
        EditorGUILayout.LabelField("splitParameter", spline.uvBeginning.ToString());
        EditorGUILayout.LabelField("beginningMinWidth", spline.beginningMinWidth.ToString());
        EditorGUILayout.LabelField("beginningMaxWidth", spline.beginningMaxWidth.ToString());
        EditorGUILayout.LabelField("minMaxWidth", spline.minMaxWidth.ToString());
        EditorGUILayout.LabelField("uvBeginning", spline.uvBeginning.ToString());
        EditorGUILayout.LabelField("uvWidth", spline.uvWidth.ToString());
        if (GUILayout.Button(new GUIContent("Regenerate spline", "Racalculates whole mesh")))
        {
            spline.GenerateSpline();
        }
    }


    void AddPointAtEnd()
    {
        if (GUILayout.Button("Add point at end"))
        {

            int i = spline.controlPoints.Count - 1;
            Vector4 position = Vector3.zero;
            position.w = spline.width;
            if (i < spline.controlPoints.Count - 1 && spline.controlPoints.Count > i + 1)
            {
                position = spline.controlPoints[i];
                Vector4 positionSecond = spline.controlPoints[i + 1];
                if (Vector3.Distance((Vector3)positionSecond, (Vector3)position) > 0)
                    position = (position + positionSecond) * 0.5f;
                else
                    position.x += 1;
            }
            else if (spline.controlPoints.Count > 1 && i == spline.controlPoints.Count - 1)
            {
                position = spline.controlPoints[i];
                Vector4 positionSecond = spline.controlPoints[i - 1];
                if (Vector3.Distance((Vector3)positionSecond, (Vector3)position) > 0)
                    position = position + (position - positionSecond);
                else
                    position.x += 1;
            }
            else if (spline.controlPoints.Count > 0)
            {
                position = spline.controlPoints[i];
                position.x += 1;
            }
            spline.controlPointsRotations.Add(Quaternion.identity);
            spline.controlPoints.Add(position);
            spline.controlPointsSnap.Add(0);
            spline.controlPointsMeshCurves.Add(new AnimationCurve(new Keyframe[] {
                new Keyframe (0, 0),
                new Keyframe (1, 0)
            }));
            spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
            RegeneratBiomeMask();
#endif
        }
    }

    void MeshSettings()
    {
        GUILayout.Label("Mesh settings:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        spline.currentProfile = (SplineProfile)EditorGUILayout.ObjectField("Spline profile", spline.currentProfile, typeof(SplineProfile), false);

        if (GUILayout.Button("Create profile from settings"))
        {


            SplineProfile asset = ScriptableObject.CreateInstance<SplineProfile>();


            asset.meshCurve = spline.meshCurve;

            MeshRenderer ren = spline.GetComponent<MeshRenderer>();
            asset.splineMaterial = ren.sharedMaterial;

            asset.minVal = spline.minVal;
            asset.maxVal = spline.maxVal;


            asset.traingleDensity = spline.traingleDensity;
            asset.vertsInShape = spline.vertsInShape;

            asset.uvScale = spline.uvScale;

            asset.uvRotation = spline.uvRotation;

            asset.flowFlat = spline.flowFlat;
            asset.flowWaterfall = spline.flowWaterfall;

            asset.terrainCarve = spline.terrainCarve;
            asset.distSmooth = spline.distSmooth;
            asset.distSmoothStart = spline.distSmoothStart;
            asset.terrainPaintCarve = spline.terrainPaintCarve;

            asset.receiveShadows = spline.receiveShadows;
            asset.shadowCastingMode = spline.shadowCastingMode;


            string path = EditorUtility.SaveFilePanelInProject("Save new spline profile", spline.gameObject.name + ".asset", "asset", "Please enter a file name to save the spline profile to");

            if (!string.IsNullOrEmpty(path))
            {

                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                spline.currentProfile = asset;
            }
        }
        if (spline.currentProfile != null && GUILayout.Button("Save profile from settings"))
        {

            spline.currentProfile.meshCurve = spline.meshCurve;

            MeshRenderer ren = spline.GetComponent<MeshRenderer>();
            spline.currentProfile.splineMaterial = ren.sharedMaterial;

            spline.currentProfile.minVal = spline.minVal;
            spline.currentProfile.maxVal = spline.maxVal;


            spline.currentProfile.traingleDensity = spline.traingleDensity;
            spline.currentProfile.vertsInShape = spline.vertsInShape;

            spline.currentProfile.uvScale = spline.uvScale;

            spline.currentProfile.uvRotation = spline.uvRotation;

            spline.currentProfile.flowFlat = spline.flowFlat;
            spline.currentProfile.flowWaterfall = spline.flowWaterfall;

            spline.currentProfile.terrainCarve = spline.terrainCarve;
            spline.currentProfile.distSmooth = spline.distSmooth;
            spline.currentProfile.distSmoothStart = spline.distSmoothStart;
            spline.currentProfile.terrainPaintCarve = spline.terrainPaintCarve;


            spline.currentProfile.receiveShadows = spline.receiveShadows;
            spline.currentProfile.shadowCastingMode = spline.shadowCastingMode;


            AssetDatabase.SaveAssets();
        }


        if (spline.currentProfile != null && spline.currentProfile != spline.oldProfile)
        {

            ResetToProfile();
            EditorUtility.SetDirty(spline);

        }

        if (CheckProfileChange())
            EditorGUILayout.HelpBox("Profile data changed.", MessageType.Info);

        if (spline.currentProfile != null && GUILayout.Button("Reset to profile"))
        {

            ResetToProfile();
        }
        EditorGUILayout.Space();


        string meshResolution = "Triangles density";
        if (spline.meshfilter != null && spline.meshfilter.sharedMesh != null)
        {
            float tris = spline.meshfilter.sharedMesh.triangles.Length / 3;
            meshResolution += " (" + tris + " tris)";
        }
        else if (spline.meshfilter != null && spline.meshfilter.sharedMesh == null)
        {
            spline.GenerateSpline();
        }


        EditorGUILayout.LabelField(meshResolution);
        EditorGUI.indentLevel++;
        spline.traingleDensity = 1 / (float)EditorGUILayout.IntSlider("U", (int)(1 / (float)spline.traingleDensity), 1, 100);

        if (spline.beginningSpline == null && spline.endingSpline == null)
        {

            spline.vertsInShape = EditorGUILayout.IntSlider("V", spline.vertsInShape - 1, 1, 20) + 1;

        }
        else
        {
            GUI.enabled = false;
            if (spline.beginningSpline != null)
            {
                spline.vertsInShape = (int)Mathf.Round((spline.beginningSpline.vertsInShape - 1) * (spline.beginningMaxWidth - spline.beginningMinWidth) + 1);
            }
            else if (spline.endingSpline != null)
                spline.vertsInShape = (int)Mathf.Round((spline.endingSpline.vertsInShape - 1) * (spline.endingMaxWidth - spline.endingMinWidth) + 1);

            EditorGUILayout.IntSlider("V", spline.vertsInShape - 1, 1, 20);
            GUI.enabled = true;

        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();


        EditorGUILayout.BeginHorizontal();
        {
            spline.width = EditorGUILayout.FloatField("River width", spline.width);
            if (GUILayout.Button("Change width for whole river"))
            {
                if (spline.width > 0)
                {
                    for (int i = 0; i < spline.controlPoints.Count; i++)
                    {
                        Vector4 point = spline.controlPoints[i];
                        point.w = spline.width;
                        spline.controlPoints[i] = point;
                    }
                    spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
                    RegeneratBiomeMask();
#endif
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();


        spline.meshCurve = EditorGUILayout.CurveField("Mesh curve", spline.meshCurve);
        if (GUILayout.Button("Set all mesh curves"))
        {
            for (int i = 0; i < spline.controlPointsMeshCurves.Count; i++)
            {
                spline.controlPointsMeshCurves[i] = new AnimationCurve(spline.meshCurve.keys);
            }
        }
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Vertice distribution: " + spline.minVal.ToString() + " " + spline.maxVal.ToString());
        EditorGUILayout.MinMaxSlider(ref spline.minVal, ref spline.maxVal, 0, 1);

        //Debug.Log (spline.minVal + " " + spline.maxVal);

        spline.minVal = (int)(spline.minVal * 100) * 0.01f;
        spline.maxVal = (int)(spline.maxVal * 100) * 0.01f;


        if (spline.minVal > 0.5f)
            spline.minVal = 0.5f;

        if (spline.minVal < 0.01f)
            spline.minVal = 0.01f;

        if (spline.maxVal < 0.5f)
            spline.maxVal = 0.5f;

        if (spline.maxVal > 0.99f)
            spline.maxVal = 0.99f;

        EditorGUILayout.Space();

        if (GUILayout.Button("Snap/Unsnap mesh to terrain"))
        {
            spline.snapToTerrain = !spline.snapToTerrain;
            for (int i = 0; i < spline.controlPointsSnap.Count; i++)
            {
                spline.controlPointsSnap[i] = spline.snapToTerrain == true ? 1 : 0;
            }
        }
        ///spline.snapMask = EditorGUILayout.MaskField ("Layers", spline.snapMask, InternalEditorUtility.layers);
        spline.snapMask = LayerMaskField("Layers", spline.snapMask, true);

        spline.normalFromRaycast = EditorGUILayout.Toggle("Take Normal from terrain", spline.normalFromRaycast);
        EditorGUILayout.Space();
    }

    bool CheckProfileChange()
    {
        if (spline.currentProfile == null)
            return false;
        //if (ren.material != spline.currentProfile.splineMaterial)
        //	return true;

        if (spline.minVal != spline.currentProfile.minVal)
            return true;
        if (spline.maxVal != spline.currentProfile.maxVal)
            return true;

        if (spline.traingleDensity != spline.currentProfile.traingleDensity)
            return true;
        if (spline.vertsInShape != spline.currentProfile.vertsInShape)
            return true;

        if (spline.uvScale != spline.currentProfile.uvScale)
            return true;

        if (spline.uvRotation != spline.currentProfile.uvRotation)
            return true;


        if (spline.flowFlat != spline.currentProfile.flowFlat)
            return true;

        if (spline.flowWaterfall != spline.currentProfile.flowWaterfall)
            return true;

        return false;
    }

    void ResetToProfile()
    {
        spline.meshCurve = spline.currentProfile.meshCurve;
        for (int i = 0; i < spline.controlPointsMeshCurves.Count; i++)
        {
            spline.controlPointsMeshCurves[i] = new AnimationCurve(spline.meshCurve.keys);
        }
        MeshRenderer ren = spline.GetComponent<MeshRenderer>();
        ren.sharedMaterial = spline.currentProfile.splineMaterial;

        spline.minVal = spline.currentProfile.minVal;
        spline.maxVal = spline.currentProfile.maxVal;


        spline.traingleDensity = spline.currentProfile.traingleDensity;
        spline.vertsInShape = spline.currentProfile.vertsInShape;

        spline.uvScale = spline.currentProfile.uvScale;

        spline.uvRotation = spline.currentProfile.uvRotation;

        spline.flowFlat = spline.currentProfile.flowFlat;
        spline.flowWaterfall = spline.currentProfile.flowWaterfall;

        spline.terrainCarve = spline.currentProfile.terrainCarve;
        spline.distSmooth = spline.currentProfile.distSmooth;
        spline.distSmoothStart = spline.currentProfile.distSmoothStart;
        spline.terrainPaintCarve = spline.currentProfile.terrainPaintCarve;


        spline.receiveShadows = spline.currentProfile.receiveShadows;
        spline.shadowCastingMode = spline.currentProfile.shadowCastingMode;

        spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
        RegeneratBiomeMask();
#endif
        spline.oldProfile = spline.currentProfile;


    }

    void DrawVertexColorsUI()
    {
        spline.drawOnMesh = true;
        if (spline.drawOnMesh)
        {


            EditorGUILayout.HelpBox("R - Slow Water G - Small Cascade B - Big Cascade A - Opacity", MessageType.Info);
            EditorGUILayout.Space();
            spline.drawColor = EditorGUILayout.ColorField("Draw color", spline.drawColor);

            spline.opacity = EditorGUILayout.FloatField("Opacity", spline.opacity);
            spline.drawSize = EditorGUILayout.FloatField("Size", spline.drawSize);
            if (spline.drawSize < 0)
            {
                spline.drawSize = 0;
            }
            EditorGUILayout.Space();
            spline.drawOnMultiple = EditorGUILayout.Toggle("Draw on multiple rivers", spline.drawOnMultiple);
        }

        EditorGUILayout.Space();
        if (!spline.showVertexColors)
        {
            if (GUILayout.Button("Show vertex colors"))
            {

                if (!spline.showFlowMap && !spline.showVertexColors)
                    spline.oldMaterial = spline.GetComponent<MeshRenderer>().sharedMaterial;
                ResetMaterial();
                spline.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("NatureManufacture Shaders/Debug/Vertex color"));
                spline.showVertexColors = true;
            }
        }
        else
        {
            if (GUILayout.Button("Hide vertex colors"))
            {
                ResetMaterial();
                spline.GetComponent<MeshRenderer>().sharedMaterial = spline.oldMaterial;
                spline.showVertexColors = false;
            }
        }

        if (GUILayout.Button("Reset vertex colors") && EditorUtility.DisplayDialog("Reset vertex colors?",
                "Are you sure you want to reset f vertex colors?", "Yes", "No"))
        {
            spline.colors = null;
            spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
            RegeneratBiomeMask();
#endif
        }
    }

    void DrawFlowColorsUI()
    {
        spline.drawOnMeshFlowMap = true;
        if (spline.drawOnMeshFlowMap)
        {
            EditorGUILayout.HelpBox("Sharp gradient could generate bugged effect. Keep flow changes smooth.", MessageType.Info);
            EditorGUILayout.Space();
            spline.flowSpeed = EditorGUILayout.Slider("Flow U Speed", spline.flowSpeed, -1, 1);
            spline.flowDirection = EditorGUILayout.Slider("Flow V Speed", spline.flowDirection, -1, 1);
            spline.opacity = EditorGUILayout.FloatField("Opacity", spline.opacity);
            spline.drawSize = EditorGUILayout.FloatField("Size", spline.drawSize);
            if (spline.drawSize < 0)
            {
                spline.drawSize = 0;
            }

            EditorGUILayout.Space();
            spline.drawOnMultiple = EditorGUILayout.Toggle("Draw on multiple rivers", spline.drawOnMultiple);
        }

        EditorGUILayout.Space();
        if (!spline.showFlowMap)
        {



            if (GUILayout.Button("Show flow directions"))
            {
                if (!spline.showFlowMap && !spline.showVertexColors)
                    spline.oldMaterial = spline.GetComponent<MeshRenderer>().sharedMaterial;
                ResetMaterial();
                spline.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("NatureManufacture Shaders/Debug/Flowmap Direction"));
                spline.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Direction", Resources.Load<Texture2D>("Debug_Arrow"));



                spline.showFlowMap = true;
            }
            if (GUILayout.Button("Show flow smoothness"))
            {
                if (!spline.showFlowMap && !spline.showVertexColors)
                    spline.oldMaterial = spline.GetComponent<MeshRenderer>().sharedMaterial;
                ResetMaterial();
                spline.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("NatureManufacture Shaders/Debug/FlowMapUV4"));
                spline.showFlowMap = true;
            }
        }

        if (spline.showFlowMap)
        {

            if (GUILayout.Button("Hide flow"))
            {
                ResetMaterial();
                spline.GetComponent<MeshRenderer>().sharedMaterial = spline.oldMaterial;
            }
        }

        EditorGUILayout.Space();

        spline.flowFlat = EditorGUILayout.CurveField("Flow curve flat speed", spline.flowFlat);
        spline.flowWaterfall = EditorGUILayout.CurveField("Flow curve waterfall speed", spline.flowWaterfall);

        if (GUILayout.Button("Reset flow to automatic") && EditorUtility.DisplayDialog("Reset flow to automatic?",
                "Are you sure you want to reset flow to automatic?", "Yes", "No"))
        {
            spline.overrideFlowMap = false;
            spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
            RegeneratBiomeMask();
#endif
        }
    }

    void ResetMaterial()
    {
        //if (spline.oldMaterial != null)
        //	spline.GetComponent<MeshRenderer> ().sharedMaterial = spline.oldMaterial;
        spline.showFlowMap = false;
        spline.showVertexColors = false;
    }

    void FilesManager()
    {
        if (GUILayout.Button("Save points to csv file"))
        {
            PointsToFile();
        }

        if (GUILayout.Button("Load points from csv file"))
        {
            PointsFromFile();
        }
    }

    void Tips()
    {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("\nReflections - Use box projection in reflection probes to get proper render even at multiple river conection.\n", MessageType.Info);
        EditorGUILayout.HelpBox("\nKeep resonable quasi- square vertex shapes at river mesh, " + "this will give better tesselation result. Don't worry about low amount of poly, tesselation will smooth shapes.\n", MessageType.Info);
        EditorGUILayout.HelpBox("\nBy rotating point you could get simmilar effect as vertex color painting.\n" + "You could adjust waterfalls or add noise in the river. " + "Note that if rotation will be bigger then +/- 90 degree you could invert normals.\n", MessageType.Info);
        EditorGUILayout.HelpBox("\nUse low resolution reflection probes, and only around the water. " + "\nFar clip planes also should be short, you probably only need colors from the surounding world.\n", MessageType.Info);
        EditorGUILayout.HelpBox("\nPut reflection probes behind , in and after dark area (tunel, cave) so you will get exelent result in lighting and reflections.\n", MessageType.Info);
        EditorGUILayout.HelpBox("\nTry to keep quite simmilar distance between spline points. Huge distance between them could create strange result.\n", MessageType.Info);
        EditorGUILayout.HelpBox("\nWhen you use multiple connected rivers, you shoud put reflection probe at fork of the rivers to keep proper reflections\n", MessageType.Info);


        EditorGUILayout.Space();
    }



    void ParentingSplineUI()
    {
        GUILayout.Label("Rivers connections", EditorStyles.boldLabel);


        spline.beginningSpline = (RamSpline)EditorGUILayout.ObjectField("Beginning spline", spline.beginningSpline, typeof(RamSpline), true);

        if (spline.beginningSpline == spline)
            spline.beginningSpline = null;

        spline.endingSpline = (RamSpline)EditorGUILayout.ObjectField("Ending spline", spline.endingSpline, typeof(RamSpline), true);
        if (spline.endingSpline == spline)
            spline.endingSpline = null;


        if (spline.beginningSpline != null)
        {
            if (spline.controlPoints.Count > 0 && spline.beginningSpline.points.Count > 0)
            {
                spline.beginningMinWidth = spline.beginningMinWidth * (spline.beginningSpline.vertsInShape - 1);
                spline.beginningMaxWidth = spline.beginningMaxWidth * (spline.beginningSpline.vertsInShape - 1);
                EditorGUILayout.MinMaxSlider("Part parent", ref spline.beginningMinWidth, ref spline.beginningMaxWidth, 0, spline.beginningSpline.vertsInShape - 1);
                spline.beginningMinWidth = (int)spline.beginningMinWidth;
                spline.beginningMaxWidth = (int)spline.beginningMaxWidth;
                spline.beginningMinWidth = Mathf.Clamp(spline.beginningMinWidth, 0, spline.beginningSpline.vertsInShape - 1);
                spline.beginningMaxWidth = Mathf.Clamp(spline.beginningMaxWidth, 0, spline.beginningSpline.vertsInShape - 1);
                if (spline.beginningMinWidth == spline.beginningMaxWidth)
                {
                    if (spline.beginningMinWidth > 0)
                        spline.beginningMinWidth--;
                    else
                        spline.beginningMaxWidth++;
                }
                spline.vertsInShape = (int)(spline.beginningMaxWidth - spline.beginningMinWidth) + 1;
                spline.beginningMinWidth = spline.beginningMinWidth / (float)(spline.beginningSpline.vertsInShape - 1);
                spline.beginningMaxWidth = spline.beginningMaxWidth / (float)(spline.beginningSpline.vertsInShape - 1);

                spline.GenerateBeginningParentBased();
            }
        }
        else
        {
            spline.beginningMaxWidth = 1;
            spline.beginningMinWidth = 0;
        }


        if (spline.endingSpline != null)
        {
            if (spline.controlPoints.Count > 1 && spline.endingSpline.points.Count > 0)
            {
                spline.endingMinWidth = spline.endingMinWidth * (spline.endingSpline.vertsInShape - 1);
                spline.endingMaxWidth = spline.endingMaxWidth * (spline.endingSpline.vertsInShape - 1);

                EditorGUILayout.MinMaxSlider("Part parent", ref spline.endingMinWidth, ref spline.endingMaxWidth, 0, spline.endingSpline.vertsInShape - 1);

                spline.endingMinWidth = (int)spline.endingMinWidth;
                spline.endingMaxWidth = (int)spline.endingMaxWidth;
                spline.endingMinWidth = Mathf.Clamp(spline.endingMinWidth, 0, spline.endingSpline.vertsInShape - 1);
                spline.endingMaxWidth = Mathf.Clamp(spline.endingMaxWidth, 0, spline.endingSpline.vertsInShape - 1);
                if (spline.endingMinWidth == spline.endingMaxWidth)
                {
                    if (spline.endingMinWidth > 0)
                        spline.endingMinWidth--;
                    else
                        spline.endingMaxWidth++;
                }
                spline.vertsInShape = (int)(spline.endingMaxWidth - spline.endingMinWidth) + 1;
                spline.endingMinWidth = spline.endingMinWidth / (float)(spline.endingSpline.vertsInShape - 1);
                spline.endingMaxWidth = spline.endingMaxWidth / (float)(spline.endingSpline.vertsInShape - 1);

                spline.GenerateEndingParentBased();
            }
        }
        else
        {
            spline.endingMaxWidth = 1;
            spline.endingMinWidth = 0;
        }

    }

    void PointsUI()
    {
        if (GUILayout.Button(new GUIContent("Remove all points", "Removes all points")))
        {
            spline.RemovePoints();

        }

        for (int i = 0; i < spline.controlPoints.Count; i++)
        {

            GUILayout.Label("Point: " + i.ToString(), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            spline.controlPoints[i] = EditorGUILayout.Vector4Field("", spline.controlPoints[i]);
            if (spline.controlPoints[i].w <= 0)
            {
                Vector4 vec4 = spline.controlPoints[i];
                vec4.w = 1;
                spline.controlPoints[i] = vec4;
            }
            if (GUILayout.Button(new GUIContent("A", "Add point after this point"), GUILayout.MaxWidth(20)))
            {

                spline.AddPointAfter(i);
                spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
                RegeneratBiomeMask();
#endif
            }
            if (GUILayout.Button(new GUIContent("R", "Remove this Point"), GUILayout.MaxWidth(20)))
            {

                spline.RemovePoint(i);
                spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
                RegeneratBiomeMask();
#endif
            }
            if (GUILayout.Toggle(selectedPosition == i, new GUIContent("S", "Select point"), "Button", GUILayout.MaxWidth(20)))
            {
                selectedPosition = i;
            }
            else if (selectedPosition == i)
            {
                selectedPosition = -1;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (spline.controlPointsRotations.Count > i)
                spline.controlPointsRotations[i] = Quaternion.Euler(EditorGUILayout.Vector3Field("", spline.controlPointsRotations[i].eulerAngles));
            if (GUILayout.Button(new GUIContent("    Clear rotation    ", "Clear Rotation")))
            {
                spline.controlPointsRotations[i] = Quaternion.identity;
                spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
                RegeneratBiomeMask();
#endif
            }
            EditorGUILayout.EndHorizontal();

            if (spline.controlPointsSnap.Count > i)
                spline.controlPointsSnap[i] = EditorGUILayout.Toggle("Snap to terrain", spline.controlPointsSnap[i] == 1 ? true : false) == true ? 1 : 0;
            if (spline.controlPointsMeshCurves.Count > i)
                spline.controlPointsMeshCurves[i] = EditorGUILayout.CurveField("Mesh curve", spline.controlPointsMeshCurves[i]);
            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
        }
    }

    void SetMaterials()
    {
        GUILayout.Label("Set materials: ", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        {

            if (GUILayout.Button("Basic", GUILayout.MinWidth(80)))
            {
                try
                {
                    string materialName = "RAM_River_Material_Gamma";
                    if (PlayerSettings.colorSpace == ColorSpace.Linear)
                        materialName = "RAM_River_Material_Linear";
                    Material riverMat = (Material)Resources.Load(materialName);
                    if (riverMat != null)
                    {
                        spline.GetComponent<MeshRenderer>().sharedMaterial = riverMat;
                    }
                }
                catch
                {
                }
            }
            if (GUILayout.Button("Vertex color", GUILayout.MinWidth(80)))
            {
                try
                {
                    string materialName = "RAM_River_Material_Gamma_Vertex_Color";
                    if (PlayerSettings.colorSpace == ColorSpace.Linear)
                        materialName = "RAM_River_Material_Linear_Vertex_Color";
                    Material riverMat = (Material)Resources.Load(materialName);
                    if (riverMat != null)
                    {
                        spline.GetComponent<MeshRenderer>().sharedMaterial = riverMat;
                    }
                }
                catch
                {
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Basic tesseled", GUILayout.MinWidth(80)))
            {
                try
                {
                    string materialName = "RAM_River_Material_Gamma_Tess";
                    if (PlayerSettings.colorSpace == ColorSpace.Linear)
                        materialName = "RAM_River_Material_Linear_Tess";
                    Material riverMat = (Material)Resources.Load(materialName);
                    if (riverMat != null)
                    {
                        spline.GetComponent<MeshRenderer>().sharedMaterial = riverMat;
                    }
                }
                catch
                {
                }
            }

            if (GUILayout.Button("Basic tesseled - vertex color", GUILayout.MinWidth(80)))
            {
                try
                {
                    string materialName = "RAM_River_Material_Gamma_Tess_Vertex_Color";
                    if (PlayerSettings.colorSpace == ColorSpace.Linear)
                        materialName = "RAM_River_Material_Linear_Tess_Vertex_Color";
                    Material riverMat = (Material)Resources.Load(materialName);
                    if (riverMat != null)
                    {
                        spline.GetComponent<MeshRenderer>().sharedMaterial = riverMat;
                    }
                }
                catch
                {
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void ChangePivot(Vector3 center)
    {
        Vector3 position = spline.transform.position;
        spline.transform.position += center;
        for (int i = 0; i < spline.controlPoints.Count; i++)
        {
            Vector4 vec = spline.controlPoints[i];
            vec.x -= center.x;
            vec.y -= center.y;
            vec.z -= center.z;
            spline.controlPoints[i] = vec;
        }
        spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
        RegeneratBiomeMask();
#endif
    }


    protected virtual void OnSceneGUI()
    {
        if (spline == null)
            spline = (RamSpline)target;

        Color baseColor = Handles.color;
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (spline != null)
        {
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            CheckRotations();

            if (spline.drawOnMesh || spline.drawOnMeshFlowMap)
            {
                Tools.current = Tool.None;
                if (spline.meshfilter != null)
                {
                    Handles.color = Color.magenta;
                    Vector3[] vertices = spline.meshfilter.sharedMesh.vertices;
                    Vector2[] uv4 = spline.meshfilter.sharedMesh.uv4;
                    Vector3[] normals = spline.meshfilter.sharedMesh.normals;
                    Quaternion up = Quaternion.Euler(90, 0, 0);
                    for (int i = 0; i < vertices.Length; i += 5)
                    {
                        Vector3 item = vertices[i];
                        Vector3 handlePos = spline.transform.TransformPoint(item);

                        if (spline.drawOnMesh)
                            Handles.RectangleHandleCap(0, handlePos, up, 0.05f, EventType.Repaint);
                    }

                }
                if (spline.drawOnMesh)
                    DrawOnVertexColors();
                else
                    DrawOnFlowMap();
                return;
            }

            if (Event.current.commandName == "UndoRedoPerformed")
            {

                spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
                RegeneratBiomeMask();
#endif
                return;
            }

            if (selectedPosition >= 0 && selectedPosition < spline.controlPoints.Count)
            {
                Handles.color = Color.red;
                Handles.SphereHandleCap(0, (Vector3)spline.controlPoints[selectedPosition] + spline.transform.position, Quaternion.identity, 1, EventType.Repaint);

            }

            if (spline.debug)
            {
                Vector3[] points = new Vector3[spline.controlPoints.Count];


                for (int i = 0; i < spline.controlPoints.Count; i++)
                {
                    points[i] = (Vector3)spline.controlPoints[i] + spline.transform.position;
                }


                Handles.color = Color.white;
                Handles.DrawPolyLine(points);

                Handles.color = new Color(1, 0, 0, 0.5f);

                for (int i = 0; i < spline.pointsDown.Count; i++)
                {

                    Vector3 handlePos = (Vector3)spline.pointsDown[i] + spline.transform.position;
                    Vector3 handlePos2 = (Vector3)spline.pointsUp[i] + spline.transform.position;
                    Handles.DrawLine(handlePos, handlePos2);
                }

                Handles.color = Color.green;
                foreach (var item in spline.controlPointsDown)
                {
                    Handles.SphereHandleCap(0, item + spline.transform.position, Quaternion.identity, 0.1f, EventType.Repaint);
                }

                points = new Vector3[spline.pointsDown.Count];


                for (int i = 0; i < spline.pointsDown.Count; i++)
                {
                    points[i] = (Vector3)spline.pointsDown[i] + spline.transform.position;
                }

                Handles.DrawPolyLine(points);



                Handles.color = Color.blue;
                foreach (var item in spline.controlPointsUp)
                {
                    Handles.SphereHandleCap(0, item + spline.transform.position, Quaternion.identity, 0.1f, EventType.Repaint);
                }

                points = new Vector3[spline.pointsUp.Count];

                for (int i = 0; i < spline.pointsUp.Count; i++)
                {
                    points[i] = (Vector3)spline.pointsUp[i] + spline.transform.position;
                }

                Handles.DrawPolyLine(points);




                Handles.color = Color.red;

                points = spline.points.ToArray();
                for (int i = 0; i < points.Length; i++)
                {



                    points[i] += spline.transform.position;
                    Handles.DrawLine(points[i], points[i] + spline.normalsList[i]);
                }
                Handles.DrawPolyLine(points);
            }

            int controlPointToDelete = -1;




            for (int j = 0; j < spline.controlPoints.Count; j++)
            {



                EditorGUI.BeginChangeCheck();



                Vector3 handlePos = (Vector3)spline.controlPoints[j] + spline.transform.position;




                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;

                Vector3 screenPoint = Camera.current.WorldToScreenPoint(handlePos);

                if (screenPoint.z > 0)
                {

                    Handles.Label(handlePos + Vector3.up * HandleUtility.GetHandleSize(handlePos), "Point: " + j.ToString(), style);

                }
                float width = spline.controlPoints[j].w;
                if (Event.current.control && Event.current.shift)
                {
                    int id = GUIUtility.GetControlID(FocusType.Passive);



                    if (HandleUtility.nearestControl == id)
                    {
                        Handles.color = Color.white;
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                            controlPointToDelete = j;
                    }
                    else
                        Handles.color = Handles.xAxisColor;

                    float size = 0.6f;
                    size = HandleUtility.GetHandleSize(handlePos) * size;
                    if (Event.current.type == EventType.Repaint)
                    {
                        Handles.SphereHandleCap(id, (Vector3)spline.controlPoints[j] + spline.transform.position, Quaternion.identity, size, EventType.Repaint);
                    }
                    else if (Event.current.type == EventType.Layout)
                    {
                        Handles.SphereHandleCap(id, (Vector3)spline.controlPoints[j] + spline.transform.position, Quaternion.identity, size, EventType.Layout);
                    }

                }
                else if (Tools.current == Tool.Move)
                {

                    float size = 0.6f;
                    size = HandleUtility.GetHandleSize(handlePos) * size;

                    Handles.color = Handles.xAxisColor;
                    Vector4 pos = Handles.Slider((Vector3)spline.controlPoints[j] + spline.transform.position, Vector3.right, size, Handles.ArrowHandleCap, 0.01f) - spline.transform.position;
                    Handles.color = Handles.yAxisColor;
                    pos = Handles.Slider((Vector3)pos + spline.transform.position, Vector3.up, size, Handles.ArrowHandleCap, 0.01f) - spline.transform.position;
                    Handles.color = Handles.zAxisColor;
                    pos = Handles.Slider((Vector3)pos + spline.transform.position, Vector3.forward, size, Handles.ArrowHandleCap, 0.01f) - spline.transform.position;

                    Vector3 halfPos = (Vector3.right + Vector3.forward) * size * 0.3f;
                    Handles.color = Handles.yAxisColor;
                    pos = Handles.Slider2D((Vector3)pos + spline.transform.position + halfPos, Vector3.up, Vector3.right, Vector3.forward, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - spline.transform.position - halfPos;
                    halfPos = (Vector3.right + Vector3.up) * size * 0.3f;
                    Handles.color = Handles.zAxisColor;
                    pos = Handles.Slider2D((Vector3)pos + spline.transform.position + halfPos, Vector3.forward, Vector3.right, Vector3.up, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - spline.transform.position - halfPos;
                    halfPos = (Vector3.up + Vector3.forward) * size * 0.3f;
                    Handles.color = Handles.xAxisColor;
                    pos = Handles.Slider2D((Vector3)pos + spline.transform.position + halfPos, Vector3.right, Vector3.up, Vector3.forward, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - spline.transform.position - halfPos;

                    pos.w = width;
                    spline.controlPoints[j] = pos;


                }
                else if (Tools.current == Tool.Rotate)
                {

                    if (spline.controlPointsRotations.Count > j && spline.controlPointsOrientation.Count > j)
                    {

                        if (!((spline.beginningSpline && j == 0) || (spline.endingSpline && j == spline.controlPoints.Count - 1)))
                        {
                            float size = 0.6f;
                            size = HandleUtility.GetHandleSize(handlePos) * size;

                            Handles.color = Handles.zAxisColor;
                            Quaternion rotation = Handles.Disc(spline.controlPointsOrientation[j], handlePos, spline.controlPointsOrientation[j] * new Vector3(0, 0, 1), size, true, 0.1f);

                            Handles.color = Handles.yAxisColor;
                            rotation = Handles.Disc(rotation, handlePos, rotation * new Vector3(0, 1, 0), size, true, 0.1f);

                            Handles.color = Handles.xAxisColor;
                            rotation = Handles.Disc(rotation, handlePos, rotation * new Vector3(1, 0, 0), size, true, 0.1f);



                            spline.controlPointsRotations[j] *= (Quaternion.Inverse(spline.controlPointsOrientation[j]) * rotation);

                            if (float.IsNaN(spline.controlPointsRotations[j].x) || float.IsNaN(spline.controlPointsRotations[j].y) || float.IsNaN(spline.controlPointsRotations[j].z) || float.IsNaN(spline.controlPointsRotations[j].w))
                            {
                                spline.controlPointsRotations[j] = Quaternion.identity;
                                spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
                                RegeneratBiomeMask();
#endif
                            }
                            Handles.color = baseColor;
                            Handles.FreeRotateHandle(Quaternion.identity, handlePos, size);

                            Handles.CubeHandleCap(0, handlePos, spline.controlPointsOrientation[j], size * 0.3f, EventType.Repaint);

                            Handles.DrawLine(spline.controlPointsUp[j] + spline.transform.position, spline.controlPointsDown[j] + spline.transform.position);
                        }


                    }

                }
                else if (Tools.current == Tool.Scale)
                {

                    Handles.color = Handles.xAxisColor;
                    //Vector3 handlePos = (Vector3)spline.controlPoints [j] + spline.transform.position;

                    width = Handles.ScaleSlider(spline.controlPoints[j].w, (Vector3)spline.controlPoints[j] + spline.transform.position, new Vector3(0, 0.5f, 0),
                        Quaternion.Euler(-90, 0, 0), HandleUtility.GetHandleSize(handlePos), 0.01f);

                    Vector4 pos = spline.controlPoints[j];
                    pos.w = width;
                    spline.controlPoints[j] = pos;

                }



                if (EditorGUI.EndChangeCheck())
                {

                    CheckRotations();
                    Undo.RecordObject(spline, "Change Position");
                    spline.GenerateSpline();
#if VEGETATION_STUDIO_PRO
                    RegeneratBiomeMask();
#endif

                }

            }

            if (controlPointToDelete >= 0)
            {
                Undo.RecordObject(spline, "Remove point");


                spline.RemovePoint(controlPointToDelete);

                spline.GenerateSpline();

                GUIUtility.hotControl = controlId;
                Event.current.Use();
                HandleUtility.Repaint();
                controlPointToDelete = -1;
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control && !Event.current.shift)
            {


                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Undo.RecordObject(spline, "Add point");

                    Vector4 position = hit.point - spline.transform.position;
                    spline.AddPoint(position);

                    spline.GenerateSpline();

                    GUIUtility.hotControl = controlId;
                    Event.current.Use();
                    HandleUtility.Repaint();
                }


            }
            if (!Event.current.control && Event.current.shift)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    int idMin = -1;
                    float distanceMin = float.MaxValue;

                    for (int j = 0; j < spline.controlPoints.Count; j++)
                    {
                        Vector3 handlePos = (Vector3)spline.controlPoints[j] + spline.transform.position;

                        float pointDist = Vector3.Distance(hit.point, handlePos);
                        if (pointDist < distanceMin)
                        {
                            distanceMin = pointDist;
                            idMin = j;
                        }
                    }

                    Vector3 posOne = (Vector3)spline.controlPoints[idMin] + spline.transform.position;
                    Vector3 posTwo;



                    if (idMin == 0)
                    {
                        posTwo = (Vector3)spline.controlPoints[1] + spline.transform.position;
                    }
                    else if (idMin == spline.controlPoints.Count - 1)
                    {
                        posTwo = (Vector3)spline.controlPoints[spline.controlPoints.Count - 2] + spline.transform.position;

                        idMin = idMin - 1;
                    }
                    else
                    {
                        Vector3 posPrev = (Vector3)spline.controlPoints[idMin - 1] + spline.transform.position;
                        Vector3 posNext = (Vector3)spline.controlPoints[idMin + 1] + spline.transform.position;

                        if (Vector3.Distance(hit.point, posPrev) > Vector3.Distance(hit.point, posNext))
                            posTwo = posNext;
                        else
                        {
                            posTwo = posPrev;
                            idMin = idMin - 1;
                        }

                    }


                    Handles.DrawLine(hit.point, posOne);
                    Handles.DrawLine(hit.point, posTwo);

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {

                        Undo.RecordObject(spline, "Add point");

                        Vector4 position = hit.point - spline.transform.position;
                        spline.AddPointAfter(idMin);
                        spline.ChangePointPosition(idMin + 1, position);

                        spline.GenerateSpline();

                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        HandleUtility.Repaint();

#if VEGETATION_STUDIO_PRO
                        RegeneratBiomeMask();
#endif
                    }

                }

            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && Event.current.control)
            {
                GUIUtility.hotControl = 0;

            }
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && Event.current.shift)
            {
                GUIUtility.hotControl = 0;

            }



        }


    }

    void DrawOnVertexColors()
    {

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Camera sceneCamera = SceneView.lastActiveSceneView.camera;
        Vector2 mousePos = Event.current.mousePosition;
        mousePos.y = Screen.height - mousePos.y - 40;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);



        List<MeshCollider> meshColliders = new List<MeshCollider>();
        foreach (var item in splines)
        {
            meshColliders.Add(item.gameObject.AddComponent<MeshCollider>());
        }


        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        GameObject go = null;
        Vector3 hitPosition = Vector3.zero;
        Vector3 hitNormal = Vector3.zero;
        RamSpline hitedSpline = null;
        if (hits.Length > 0)
        {

            foreach (var hit in hits)
            {
                if (hit.collider is MeshCollider)
                {
                    go = hit.collider.gameObject;
                    hitedSpline = go.GetComponent<RamSpline>();

                    if (hitedSpline != null && (spline.drawOnMultiple || hitedSpline == spline))
                    {

                        hitPosition = hit.point;
                        hitNormal = hit.normal;
                        break;
                    }
                    else
                        go = null;
                }
            }

        }
        foreach (var item in meshColliders)
        {
            if (item != null)
                DestroyImmediate(item);
        }



        if (go != null)
        {
            Handles.color = new Color(spline.drawColor.r, spline.drawColor.g, spline.drawColor.b, 1);
            Handles.DrawLine(hitPosition, hitPosition + hitNormal * 2);
            Handles.CircleHandleCap(
                0,
                hitPosition,
                Quaternion.LookRotation(hitNormal),
                spline.drawSize,
                EventType.Repaint
            );
            Handles.color = Color.black;
            Handles.CircleHandleCap(
                0,
                hitPosition,
                Quaternion.LookRotation(hitNormal),
                spline.drawSize - 0.1f,
                EventType.Repaint
            );

            if (!(Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) || Event.current.button != 0)
                return;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
            }

            MeshFilter meshFilter = hitedSpline.GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (hitedSpline.colors.Length == 0)
                    hitedSpline.colors = new Color[mesh.vertices.Length];

                int length = mesh.vertices.Length;
                float dist = 0;
                hitPosition -= hitedSpline.transform.position;
                Vector3[] vertices = mesh.vertices;
                Color[] colors = hitedSpline.colors;

                for (int i = 0; i < length; i++)
                {
                    dist = Vector3.Distance(hitPosition, vertices[i]);

                    if (dist < hitedSpline.drawSize)
                    {

                        if (Event.current.shift)
                            colors[i] = Color.Lerp(colors[i], Color.white, spline.opacity);
                        else
                            colors[i] = Color.Lerp(colors[i], spline.drawColor, spline.opacity);

                    }
                }

                mesh.colors = colors;
                meshFilter.sharedMesh = mesh;
                if (hitedSpline.generateMeshParts)
                    hitedSpline.GenerateMeshParts(mesh);
            }
        }
    }

    void DrawOnFlowMap()
    {

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Camera sceneCamera = SceneView.lastActiveSceneView.camera;
        Vector2 mousePos = Event.current.mousePosition;
        mousePos.y = Screen.height - mousePos.y - 40;

        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        List<MeshCollider> meshColliders = new List<MeshCollider>();
        foreach (var item in splines)
        {
            meshColliders.Add(item.gameObject.AddComponent<MeshCollider>());
        }


        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        GameObject go = null;
        Vector3 hitPosition = Vector3.zero;
        Vector3 hitNormal = Vector3.zero;
        RamSpline hitedSpline = null;
        if (hits.Length > 0)
        {

            foreach (var hit in hits)
            {
                if (hit.collider is MeshCollider)
                {
                    go = hit.collider.gameObject;
                    hitedSpline = go.GetComponent<RamSpline>();


                    if (hitedSpline != null && (spline.drawOnMultiple || hitedSpline == spline))
                    {

                        hitPosition = hit.point;
                        hitNormal = hit.normal;
                        break;
                    }
                    else
                        go = null;
                }
            }

        }

        foreach (var item in meshColliders)
        {
            if (item != null)
                DestroyImmediate(item);
        }


        if (go != null)
        {

            Handles.color = new Color(spline.flowDirection, spline.flowSpeed, 0, 1);
            Handles.DrawLine(hitPosition, hitPosition + hitNormal * 2);
            Handles.CircleHandleCap(
                0,
                hitPosition,
                Quaternion.LookRotation(hitNormal),
                spline.drawSize,
                EventType.Repaint
            );
            Handles.color = Color.black;
            Handles.CircleHandleCap(
                0,
                hitPosition,
                Quaternion.LookRotation(hitNormal),
                spline.drawSize - 0.1f,
                EventType.Repaint
            );

            if (!(Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) || Event.current.button != 0)
                return;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
            }

            hitedSpline.overrideFlowMap = true;
            MeshFilter meshFilter = hitedSpline.GetComponent<MeshFilter>();


            if (meshFilter.sharedMesh != null)
            {
                Mesh mesh = meshFilter.sharedMesh;


                List<Vector2> colorsFlowMap = hitedSpline.colorsFlowMap;
                int length = mesh.vertices.Length;
                float dist = 0;
                float distValue = 0;
                hitPosition -= hitedSpline.transform.position;
                Vector3[] vertices = mesh.vertices;

                for (int i = 0; i < length; i++)
                {
                    dist = Vector3.Distance(hitPosition, vertices[i]);
                    if (dist < spline.drawSize)
                    {
                        distValue = (spline.drawSize - dist) / (float)spline.drawSize;
                        if (Event.current.shift)
                        {
                            colorsFlowMap[i] = Vector2.Lerp(colorsFlowMap[i], new Vector2(0, 0), spline.opacity);

                        }
                        else
                        {
                            colorsFlowMap[i] = Vector2.Lerp(colorsFlowMap[i], new Vector2(spline.flowDirection, spline.flowSpeed), spline.opacity * distValue);

                        }

                    }
                }

                mesh.uv4 = colorsFlowMap.ToArray();
                hitedSpline.colorsFlowMap = colorsFlowMap;
                meshFilter.sharedMesh = mesh;
                if (hitedSpline.generateMeshParts)
                    hitedSpline.GenerateMeshParts(mesh);
            }

        }
    }

    public static LayerMask LayerMaskField(string label, LayerMask selected, bool showSpecial)
    {

        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();

        string selectedLayers = "";

        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (layerName != "")
            {
                if (selected == (selected | (1 << i)))
                {
                    if (selectedLayers == "")
                    {
                        selectedLayers = layerName;
                    }
                    else
                    {
                        selectedLayers = "Mixed";
                    }
                }
            }
        }

        EventType lastEvent = Event.current.type;

        if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.ExecuteCommand)
        {
            if (selected.value == 0)
            {
                layers.Add("Nothing");
            }
            else if (selected.value == -1)
            {
                layers.Add("Everything");
            }
            else
            {
                layers.Add(selectedLayers);
            }
            layerNumbers.Add(-1);
        }

        if (showSpecial)
        {
            layers.Add((selected.value == 0 ? "[X] " : "      ") + "Nothing");
            layerNumbers.Add(-2);

            layers.Add((selected.value == -1 ? "[X] " : "      ") + "Everything");
            layerNumbers.Add(-3);
        }

        for (int i = 0; i < 32; i++)
        {

            string layerName = LayerMask.LayerToName(i);

            if (layerName != "")
            {
                if (selected == (selected | (1 << i)))
                {
                    layers.Add("[X] " + i + ": " + layerName);
                }
                else
                {
                    layers.Add("     " + i + ": " + layerName);
                }
                layerNumbers.Add(i);
            }
        }

        bool preChange = GUI.changed;

        GUI.changed = false;

        int newSelected = 0;

        if (Event.current.type == EventType.MouseDown)
        {
            newSelected = -1;
        }

        newSelected = EditorGUILayout.Popup(label, newSelected, layers.ToArray(), EditorStyles.layerMaskField);

        if (GUI.changed && newSelected >= 0)
        {
            if (showSpecial && newSelected == 0)
            {
                selected = 0;
            }
            else if (showSpecial && newSelected == 1)
            {
                selected = -1;
            }
            else
            {

                if (selected == (selected | (1 << layerNumbers[newSelected])))
                {
                    selected &= ~(1 << layerNumbers[newSelected]);
                }
                else
                {
                    selected = selected | (1 << layerNumbers[newSelected]);
                }
            }
        }
        else
        {
            GUI.changed = preChange;
        }

        return selected;
    }

    public void PointsToFile()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "Save Spline Points",
            spline.name + "Points.csv",
            "csv",
            "Save Spline " + spline.name + " Points in CSV");

        if (string.IsNullOrEmpty(path))
            return;

        string fileData = "";

        foreach (Vector4 v in spline.controlPoints)
        {
            fileData += v.x + ";" + v.y + ";" + v.z + ";" + v.w + "\n";
        }
        if (fileData.Length > 0)
            fileData.Remove(fileData.Length - 1, 1);

        // Debug.Log(fileData);
        File.WriteAllText(path, fileData);

    }
    public void PointsFromFile()
    {
        string path = EditorUtility.OpenFilePanel("Read Spline Points from CSV", Application.dataPath, "csv");

        if (string.IsNullOrEmpty(path))
            return;

        string fileData = File.ReadAllText(path);

        string[] lines = fileData.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        Vector4[] vectors = new Vector4[lines.Length];

        for (int i = 0; i < vectors.Length; i++)
        {
            string[] values = lines[i].Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (values.Length != 4)
                Debug.LogError("Wrong file data");
            else
            {
                try
                {
                    vectors[i] = new Vector4(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
                }
                catch (System.Exception)
                {

                    Debug.LogError("Wrong file data");
                    return;

                }
            }

        }
        Undo.RecordObject(spline, "Spline changed");
        if (vectors.Length > 0)
        {
            foreach (var item in vectors)
            {
                spline.AddPoint(item);
            }

        }

    }

}
