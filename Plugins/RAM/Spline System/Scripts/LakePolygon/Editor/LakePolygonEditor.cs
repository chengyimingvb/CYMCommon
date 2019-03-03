using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationSystem.Biomes;
#endif

#if VEGETATION_STUDIO
using AwesomeTechnologies;
using AwesomeTechnologies.VegetationStudio;
#endif

[CustomEditor(typeof(LakePolygon))]
public class LakePolygonEditor : Editor
{
    LakePolygon lakePolygon;
    int selectedPosition = -1;

    bool pointsFolded = false;

    [MenuItem("GameObject/3D Object/Create Lake Polygon")]
    static public void CreatelakePolygon()
    {

        Selection.activeGameObject = LakePolygon.CreatePolygon(AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat")).gameObject;
    }

    public override void OnInspectorGUI()
    {
        if (lakePolygon == null)
            lakePolygon = (LakePolygon)target;
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Add Point  - CTRL + Left Mouse Button Click \n" +
            "Add point between existing points - SHIFT + Left Button Click \n" +
            "Remove point - CTRL + SHIFT + Left Button Click", MessageType.Info);

        EditorGUILayout.Space();
        lakePolygon.currentProfile = (LakePolygonProfile)EditorGUILayout.ObjectField("Lake profile", lakePolygon.currentProfile, typeof(LakePolygonProfile), false);

        if (GUILayout.Button("Create profile from settings"))
        {

            LakePolygonProfile asset = ScriptableObject.CreateInstance<LakePolygonProfile>();

            MeshRenderer ren = lakePolygon.GetComponent<MeshRenderer>();
            asset.lakeMaterial = ren.sharedMaterial;
            asset.terrainCarve = lakePolygon.terrainCarve;
            asset.distSmooth = lakePolygon.distSmooth;
            asset.uvScale = lakePolygon.uvScale;
            asset.distSmoothStart = lakePolygon.distSmoothStart;
            asset.terrainPaintCarve = lakePolygon.terrainPaintCarve;
            asset.currentSplatMap = lakePolygon.currentSplatMap;

            asset.maximumTriangleSize = lakePolygon.maximumTriangleSize;
            asset.traingleDensity = lakePolygon.traingleDensity;



            string path = EditorUtility.SaveFilePanelInProject("Save new spline profile", lakePolygon.gameObject.name + ".asset", "asset", "Please enter a file name to save the spline profile to");

            if (!string.IsNullOrEmpty(path))
            {

                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                lakePolygon.currentProfile = asset;
            }
        }

        if (lakePolygon.currentProfile != null && GUILayout.Button("Save profile from settings"))
        {


            MeshRenderer ren = lakePolygon.GetComponent<MeshRenderer>();

            lakePolygon.currentProfile.lakeMaterial = ren.sharedMaterial;

            lakePolygon.currentProfile.terrainCarve = lakePolygon.terrainCarve;
            lakePolygon.currentProfile.distSmooth = lakePolygon.distSmooth;
            lakePolygon.currentProfile.uvScale = lakePolygon.uvScale;
            lakePolygon.currentProfile.distSmoothStart = lakePolygon.distSmoothStart;
            lakePolygon.currentProfile.terrainPaintCarve = lakePolygon.terrainPaintCarve;
            lakePolygon.currentProfile.currentSplatMap = lakePolygon.currentSplatMap;

            lakePolygon.currentProfile.maximumTriangleSize = lakePolygon.maximumTriangleSize;
            lakePolygon.currentProfile.traingleDensity = lakePolygon.traingleDensity;


            AssetDatabase.SaveAssets();
        }


        if (lakePolygon.currentProfile != null && lakePolygon.currentProfile != lakePolygon.oldProfile)
        {

            ResetToProfile();
            EditorUtility.SetDirty(lakePolygon);

        }

        if (CheckProfileChange())
            EditorGUILayout.HelpBox("Profile data changed.", MessageType.Info);

        if (lakePolygon.currentProfile != null && GUILayout.Button("Reset to profile"))
        {

            ResetToProfile();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUI.BeginChangeCheck();


        pointsFolded = EditorGUILayout.Foldout(pointsFolded, "Points:");
        EditorGUI.indentLevel++;
        if (pointsFolded)
            PointsUI();

        lakePolygon.lockHeight = EditorGUILayout.Toggle("Lock height", lakePolygon.lockHeight);

        EditorGUILayout.BeginHorizontal();
        lakePolygon.height = EditorGUILayout.FloatField(lakePolygon.height);
        if (GUILayout.Button("Set heights"))
        {
            for (int i = 0; i < lakePolygon.points.Count; i++)
            {
                Vector3 point = lakePolygon.points[i];
                point.y = lakePolygon.height - lakePolygon.transform.position.y;
                lakePolygon.points[i] = point;
            }
            lakePolygon.GeneratePolygon();
        }

        EditorGUILayout.EndHorizontal();

        lakePolygon.yOffset = EditorGUILayout.FloatField("Y offset mesh", lakePolygon.yOffset);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();


        GUILayout.Label("Mesh settings:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        string meshResolution = "Triangles density" + "(" + lakePolygon.trianglesGenerated + " tris)";

        EditorGUILayout.LabelField(meshResolution);

        lakePolygon.maximumTriangleSize = EditorGUILayout.FloatField("Maximum triangle size", lakePolygon.maximumTriangleSize);
        lakePolygon.traingleDensity = 1 / (float)EditorGUILayout.IntSlider("Spline density", (int)(1 / (float)lakePolygon.traingleDensity), 1, 100);
        lakePolygon.uvScale = EditorGUILayout.FloatField("UV scale", lakePolygon.uvScale);

        if (EditorGUI.EndChangeCheck())
        {

            Undo.RecordObject(lakePolygon, "Lake changed");
            lakePolygon.GeneratePolygon();

        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate polygon"))
        {
            lakePolygon.GeneratePolygon();

        }
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.Space();

        Terrain terrain = Terrain.activeTerrain;
        GUILayout.Label("Terrain carve:", EditorStyles.boldLabel);

        if (terrain != null)
        {
            EditorGUI.indentLevel++;
            lakePolygon.terrainCarve = EditorGUILayout.CurveField("Terrain carve", lakePolygon.terrainCarve);
            lakePolygon.distSmooth = EditorGUILayout.FloatField("Smooth distance", lakePolygon.distSmooth);
            lakePolygon.distSmoothStart = EditorGUILayout.FloatField("Smooth start distance", lakePolygon.distSmoothStart);
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(lakePolygon, "Lake curve changed");
            }

            if (GUILayout.Button("Carve Terrain"))
            {
                TerrainCarve();
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            lakePolygon.terrainPaintCarve = EditorGUILayout.CurveField("Terrain paint", lakePolygon.terrainPaintCarve);

            int splatNumber = terrain.terrainData.splatPrototypes.Length;
            if (splatNumber > 0)
            {
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
                lakePolygon.currentSplatMap = EditorGUILayout.Popup(lakePolygon.currentSplatMap, options);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(lakePolygon, "Lake curve changed");
                }

                if (GUILayout.Button("Paint Terrain"))
                {
                    TerrainPaint();
                }
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Terrain has no splatmaps.", MessageType.Info);

            }
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("No Terrain On Scene.", MessageType.Info);

        }
#if VEGETATION_STUDIO
        EditorGUILayout.Space();
        GUILayout.Label("Vegetation Studio: ", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUI.BeginChangeCheck();
        lakePolygon.vegetationMaskResolution = EditorGUILayout.Slider("Mask Resolution", lakePolygon.vegetationMaskResolution, 0.1f, 1);
        lakePolygon.vegetationMaskPerimeter = EditorGUILayout.FloatField("Vegetation Mask Perimeter", lakePolygon.vegetationMaskPerimeter);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(lakePolygon, "Lake curve changed");
            RegenerateVegetationMask();
        }
        EditorGUI.indentLevel--;
        if (lakePolygon.vegetationMaskArea == null && GUILayout.Button("Add Vegetation Mask Area"))
        {
            lakePolygon.vegetationMaskArea = lakePolygon.gameObject.AddComponent<VegetationMaskArea>();
            RegenerateVegetationMask();
        }
        if (lakePolygon.vegetationMaskArea != null && GUILayout.Button("Calculate hull outline"))
        {

            RegenerateVegetationMask();
        }
#endif

#if VEGETATION_STUDIO_PRO
        EditorGUILayout.Space();
        GUILayout.Label("Vegetation Studio Pro: ", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        lakePolygon.vegetationBlendDistance = EditorGUILayout.FloatField("Vegetation Blend Distance", lakePolygon.vegetationBlendDistance);
        lakePolygon.biomMaskResolution = EditorGUILayout.Slider("Mask Resolution", lakePolygon.biomMaskResolution, 0.1f, 1);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(lakePolygon, "Lake curve changed");
            RegenerateBiomMask();
        }
        if (lakePolygon.biomeMaskArea != null)
            lakePolygon.refreshMask = EditorGUILayout.Toggle("Auto Refresh Biome Mask", lakePolygon.refreshMask);

        if (GUILayout.Button("Add Vegetation Biom Mask Area"))
        {
            lakePolygon.GeneratePolygon();

            if (lakePolygon.biomeMaskArea == null)
            {
                GameObject maskObject = new GameObject("MyMask");
                maskObject.transform.SetParent(lakePolygon.transform);
                maskObject.transform.localPosition = Vector3.zero;

                lakePolygon.biomeMaskArea = maskObject.AddComponent<BiomeMaskArea>();
            }

            if (lakePolygon.biomeMaskArea == null)
                return;

            RegenerateBiomMask(false);
        }

#endif




    }

    void PointsUI()
    {
        if (GUILayout.Button(new GUIContent("Remove all points", "Removes all points")))
        {
            lakePolygon.RemovePoints();

        }

        for (int i = 0; i < lakePolygon.points.Count; i++)
        {

            GUILayout.Label("Point: " + i.ToString(), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            lakePolygon.points[i] = EditorGUILayout.Vector3Field("", lakePolygon.points[i]);
            if (GUILayout.Button(new GUIContent("A", "Add point after this point"), GUILayout.MaxWidth(20)))
            {

                lakePolygon.AddPointAfter(i);
                lakePolygon.GeneratePolygon();
            }
            if (GUILayout.Button(new GUIContent("R", "Remove this Point"), GUILayout.MaxWidth(20)))
            {

                lakePolygon.RemovePoint(i);
                lakePolygon.GeneratePolygon();
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


            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
        }

    }

#if VEGETATION_STUDIO_PRO
    void RegenerateBiomMask(bool checkAuto = true)
    {
        if (checkAuto && !lakePolygon.refreshMask)
            return;

        if (lakePolygon.biomeMaskArea == null)
            return;

        lakePolygon.biomeMaskArea.BiomeType = BiomeType.Underwater;

        List<Vector3> worldspacePointList = new List<Vector3>();
        for (int i = 0; i < lakePolygon.splinePoints.Count; i += (int)(1 / (float)lakePolygon.biomMaskResolution))
        {
            Vector3 position = lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[i])
    + (lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[i]) - lakePolygon.transform.position).normalized * lakePolygon.vegetationBlendDistance;

            worldspacePointList.Add(position);
        }


        lakePolygon.biomeMaskArea.ClearNodes();

        for (var i = 0; i <= worldspacePointList.Count - 1; i++)
        {
            lakePolygon.biomeMaskArea.AddNodeToEnd(worldspacePointList[i]);
        }

        //these have default values but you can set them if you want a different default setting
        lakePolygon.biomeMaskArea.BlendDistance = lakePolygon.vegetationBlendDistance * 0.5f;
        lakePolygon.biomeMaskArea.NoiseScale = 5;
        lakePolygon.biomeMaskArea.UseNoise = true;

        //These 3 curves holds the blend curves for vegetation and textures. they have default values;
        //biomeMaskArea.BlendCurve;
        //biomeMaskArea.InverseBlendCurve;
        //biomeMaskArea.TextureBlendCurve;

        if (lakePolygon.currentProfile != null)
        {
            lakePolygon.biomeMaskArea.BiomeType = (BiomeType)lakePolygon.currentProfile.biomeType;

        }
        else
            lakePolygon.biomeMaskArea.BiomeType = BiomeType.River;

        lakePolygon.biomeMaskArea.UpdateBiomeMask();

    }
#endif

#if VEGETATION_STUDIO
    private void RegenerateVegetationMask()
    {
        if (lakePolygon.vegetationMaskArea == null)
            return;

        lakePolygon.vegetationMaskArea.AdditionalGrassPerimiterMax = lakePolygon.vegetationMaskPerimeter;
        lakePolygon.vegetationMaskArea.AdditionalLargeObjectPerimiterMax = lakePolygon.vegetationMaskPerimeter;
        lakePolygon.vegetationMaskArea.AdditionalObjectPerimiterMax = lakePolygon.vegetationMaskPerimeter;
        lakePolygon.vegetationMaskArea.AdditionalPlantPerimiterMax = lakePolygon.vegetationMaskPerimeter;
        lakePolygon.vegetationMaskArea.AdditionalTreePerimiterMax = lakePolygon.vegetationMaskPerimeter;
        lakePolygon.vegetationMaskArea.GenerateHullNodes(lakePolygon.vegetationMaskArea.ReductionTolerance);

        lakePolygon.GeneratePolygon();
        List<Vector3> worldspacePointList = new List<Vector3>();
        for (int i = 0; i < lakePolygon.splinePoints.Count; i += (int)(1 / (float)lakePolygon.vegetationMaskResolution))
        {
            Vector3 position = lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[i])
        + (lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[i]) - lakePolygon.transform.position).normalized * lakePolygon.vegetationMaskPerimeter;

            worldspacePointList.Add(position);
        }


        lakePolygon.vegetationMaskArea.ClearNodes();

        for (var i = 0; i <= worldspacePointList.Count - 1; i++)
        {
            lakePolygon.vegetationMaskArea.AddNodeToEnd(worldspacePointList[i]);
        }
        lakePolygon.vegetationMaskArea.UpdateVegetationMask();
    }
#endif


    void TerrainCarve()
    {

        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;
        float sizeX = terrain.terrainData.size.x;
        float sizeY = terrain.terrainData.size.y;
        float sizeZ = terrain.terrainData.size.z;
        Undo.RegisterCompleteObjectUndo(terrainData, "Lake curve");

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        for (int i = 0; i < lakePolygon.splinePoints.Count; i++)
        {
            Vector3 point = lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[i]);


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


        float terrainTowidth = (1 / sizeX * (terrainData.heightmapWidth - 1));
        float terrainToheight = (1 / sizeZ * (terrainData.heightmapHeight - 1));
        minX -= terrain.transform.position.x + lakePolygon.distSmooth;
        maxX -= terrain.transform.position.x - lakePolygon.distSmooth;

        minZ -= terrain.transform.position.z + lakePolygon.distSmooth;
        maxZ -= terrain.transform.position.z - lakePolygon.distSmooth;


        minX = minX * terrainToheight;
        maxX = maxX * terrainToheight;

        minZ = minZ * terrainTowidth;
        maxZ = maxZ * terrainTowidth;

        minX = (int)Mathf.Clamp(minX, 0, (terrainData.heightmapWidth));
        maxX = (int)Mathf.Clamp(maxX, 0, (terrainData.heightmapWidth));
        minZ = (int)Mathf.Clamp(minZ, 0, (terrainData.heightmapHeight));
        maxZ = (int)Mathf.Clamp(maxZ, 0, (terrainData.heightmapHeight));

        float[,] heightmapData = terrainData.GetHeights((int)minX, (int)minZ, (int)(maxX - minX), (int)(maxZ - minZ));

        MeshCollider meshCollider = lakePolygon.gameObject.AddComponent<MeshCollider>();


        float polygonHeight = lakePolygon.transform.position.y;

        for (int x = 0; x < heightmapData.GetLength(0); x++)
        {
            for (int z = 0; z < heightmapData.GetLength(1); z++)
            {


                Vector3 position = new Vector3((z + minX) / (float)terrainToheight + terrain.transform.position.x, polygonHeight, (x + minZ) / (float)terrainTowidth + terrain.transform.position.z);

                Ray ray = new Ray(position + Vector3.up * 1000, Vector3.down);
                RaycastHit hit;

                if (meshCollider.Raycast(ray, out hit, 10000))
                {
                    // Debug.DrawLine(hit.point, hit.point + Vector3.up * 30, Color.green, 3);

                    float minDist = float.MaxValue;
                    for (int i = 0; i < lakePolygon.splinePoints.Count; i++)
                    {
                        int idOne = i;
                        int idTwo = (i + 1) % lakePolygon.splinePoints.Count;

                        float dist = DistancePointLine(hit.point, lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[idOne]), lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[idTwo]));
                        if (minDist > dist)
                            minDist = dist;
                    }


                    heightmapData[x, z] = (polygonHeight + lakePolygon.terrainCarve.Evaluate(minDist)) / (float)sizeY;// heightmapData[x, z] + lakePolygon.terrainCarve.Evaluate(distances[0]) / (float)sizeY;


                }
                else
                {
                    float minDist = float.MaxValue;
                    for (int i = 0; i < lakePolygon.splinePoints.Count; i++)
                    {
                        int idOne = i;
                        int idTwo = (i + 1) % lakePolygon.splinePoints.Count;

                        float dist = DistancePointLine(position, lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[idOne]), lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[idTwo]));
                        if (minDist > dist)
                            minDist = dist;
                    }
                    float smooth = Mathf.Clamp01((1 - (minDist - lakePolygon.distSmoothStart) / (float)lakePolygon.distSmooth));
                    heightmapData[x, z] = Mathf.Lerp(heightmapData[x, z], (polygonHeight + lakePolygon.terrainCarve.Evaluate(-minDist)) / (float)sizeY, smooth);

                    //if (minDist < lakePolygon.distSmoothStart)
                    //{
                    //    Debug.DrawLine(position, position + Vector3.up * minDist, Color.green, 10);
                    //}
                    //else
                    //    Debug.DrawLine(position, position + Vector3.up * minDist, Color.red, 10);
                }



            }
        }
        //Debug.DrawRay(new Vector3((minX) / (float)terrainToheight, polygonHeight, (minZ) / (float)terrainTowidth), Vector3.up * 30, Color.black, 3);
        //Debug.DrawRay(new Vector3((maxX) / (float)terrainToheight, polygonHeight, (maxZ) / (float)terrainTowidth), Vector3.up * 30, Color.black, 3);

        DestroyImmediate(meshCollider);
        terrainData.SetHeights((int)minX, (int)minZ, heightmapData);
        terrain.Flush();
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

        for (int i = 0; i < lakePolygon.splinePoints.Count; i++)
        {
            Vector3 point = lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[i]);


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


        float terrainTowidth = (1 / sizeX * terrainData.alphamapHeight);
        float terrainToheight = (1 / sizeZ * terrainData.alphamapWidth);

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

        MeshCollider meshCollider = lakePolygon.gameObject.AddComponent<MeshCollider>();



        for (int x = 0; x < alphamapData.GetLength(0); x++)
        {
            for (int z = 0; z < alphamapData.GetLength(1); z++)
            {


                Vector3 position = new Vector3((z + minX) / (float)terrainToheight + terrain.transform.position.x, 0, (x + minZ) / (float)terrainTowidth + terrain.transform.position.z);

                Ray ray = new Ray(position + Vector3.up * 1000, Vector3.down);
                RaycastHit hit;

                if (meshCollider.Raycast(ray, out hit, 10000))
                {
                    //Debug.DrawLine(position, position + Vector3.up * 30, Color.green, 3);
                    List<float> distances = new List<float>();


                    for (int i = 0; i < lakePolygon.splinePoints.Count; i++)
                    {
                        int idOne = i;
                        int idTwo = (i + 1) % lakePolygon.splinePoints.Count;

                        float dist = DistancePointLine(hit.point, lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[idOne]), lakePolygon.transform.TransformPoint(lakePolygon.splinePoints[idTwo]));

                        distances.Add(dist);
                    }

                    distances.Sort();

                    float oldValue = alphamapData[x, z, lakePolygon.currentSplatMap];

                    alphamapData[x, z, lakePolygon.currentSplatMap] = Mathf.Lerp(alphamapData[x, z, lakePolygon.currentSplatMap], 1, lakePolygon.terrainPaintCarve.Evaluate(distances[0]));


                    for (int l = 0; l < terrainData.splatPrototypes.Length; l++)
                    {
                        if (l != lakePolygon.currentSplatMap)
                        {
                            alphamapData[x, z, l] = oldValue == 1 ? 0 : Mathf.Clamp01(alphamapData[x, z, l] * ((1 - alphamapData[x, z, lakePolygon.currentSplatMap]) / (1 - oldValue)));

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
        return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
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


    protected virtual void OnSceneGUI()
    {
        if (lakePolygon == null)
            lakePolygon = (LakePolygon)target;

        Color baseColor = Handles.color;
        int controlId = GUIUtility.GetControlID(FocusType.Passive);



        if (lakePolygon != null)

            if (lakePolygon.lockHeight)
            {
                for (int i = 1; i < lakePolygon.points.Count; i++)
                {
                    Vector3 vec = lakePolygon.points[i];
                    vec.y = lakePolygon.points[0].y;
                    lakePolygon.points[i] = vec;
                }

            }
        {
            Vector3[] points = new Vector3[lakePolygon.splinePoints.Count];


            for (int i = 0; i < lakePolygon.splinePoints.Count; i++)
            {
                points[i] = lakePolygon.splinePoints[i] + lakePolygon.transform.position;
            }


            Handles.color = Color.white;
            Handles.DrawPolyLine(points);

            if (Event.current.commandName == "UndoRedoPerformed")
            {

                lakePolygon.GeneratePolygon();
                return;
            }

            if (selectedPosition >= 0 && selectedPosition < lakePolygon.points.Count)
            {
                Handles.color = Color.red;
                Handles.SphereHandleCap(0, (Vector3)lakePolygon.points[selectedPosition] + lakePolygon.transform.position, Quaternion.identity, 1, EventType.Repaint);

            }


            int controlPointToDelete = -1;

            for (int j = 0; j < lakePolygon.points.Count; j++)
            {

                EditorGUI.BeginChangeCheck();

                Vector3 handlePos = (Vector3)lakePolygon.points[j] + lakePolygon.transform.position;

                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;

                Vector3 screenPoint = Camera.current.WorldToScreenPoint(handlePos);

                if (screenPoint.z > 0)
                {

                    Handles.Label(handlePos + Vector3.up * HandleUtility.GetHandleSize(handlePos), "Point: " + j.ToString(), style);

                }

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
                        Handles.SphereHandleCap(id, (Vector3)lakePolygon.points[j] + lakePolygon.transform.position, Quaternion.identity, size, EventType.Repaint);
                    }
                    else if (Event.current.type == EventType.Layout)
                    {
                        Handles.SphereHandleCap(id, (Vector3)lakePolygon.points[j] + lakePolygon.transform.position, Quaternion.identity, size, EventType.Layout);
                    }

                }
                else if (Tools.current == Tool.Move)
                {

                    float size = 0.6f;
                    size = HandleUtility.GetHandleSize(handlePos) * size;

                    Handles.color = Handles.xAxisColor;
                    Vector3 pos = Handles.Slider((Vector3)lakePolygon.points[j] + lakePolygon.transform.position, Vector3.right, size, Handles.ArrowHandleCap, 0.01f) - lakePolygon.transform.position;
                    if (!lakePolygon.lockHeight)
                    {
                        Handles.color = Handles.yAxisColor;

                        pos = Handles.Slider((Vector3)pos + lakePolygon.transform.position, Vector3.up, size, Handles.ArrowHandleCap, 0.01f) - lakePolygon.transform.position;
                    }
                    Handles.color = Handles.zAxisColor;
                    pos = Handles.Slider((Vector3)pos + lakePolygon.transform.position, Vector3.forward, size, Handles.ArrowHandleCap, 0.01f) - lakePolygon.transform.position;

                    Vector3 halfPos = (Vector3.right + Vector3.forward) * size * 0.3f;
                    Handles.color = Handles.yAxisColor;
                    pos = Handles.Slider2D((Vector3)pos + lakePolygon.transform.position + halfPos, Vector3.up, Vector3.right, Vector3.forward, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - lakePolygon.transform.position - halfPos;
                    halfPos = (Vector3.right + Vector3.up) * size * 0.3f;

                    if (!lakePolygon.lockHeight)
                    {
                        Handles.color = Handles.zAxisColor;
                        pos = Handles.Slider2D((Vector3)pos + lakePolygon.transform.position + halfPos, Vector3.forward, Vector3.right, Vector3.up, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - lakePolygon.transform.position - halfPos;
                        halfPos = (Vector3.up + Vector3.forward) * size * 0.3f;
                        Handles.color = Handles.xAxisColor;
                        pos = Handles.Slider2D((Vector3)pos + lakePolygon.transform.position + halfPos, Vector3.right, Vector3.up, Vector3.forward, size * 0.3f, Handles.RectangleHandleCap, 0.01f) - lakePolygon.transform.position - halfPos;
                    }

                    lakePolygon.points[j] = pos;


                }


                if (EditorGUI.EndChangeCheck())
                {

                    Undo.RecordObject(lakePolygon, "Change Position");
                    lakePolygon.GeneratePolygon();
#if VEGETATION_STUDIO
                    RegenerateVegetationMask();
#endif
#if VEGETATION_STUDIO_PRO
                    RegenerateBiomMask();
#endif

                }

            }

            if (controlPointToDelete >= 0)
            {
                Undo.RecordObject(lakePolygon, "Remove point");
                Undo.RecordObject(lakePolygon.transform, "Remove point");


                lakePolygon.RemovePoint(controlPointToDelete);

                lakePolygon.GeneratePolygon();

                GUIUtility.hotControl = controlId;
                Event.current.Use();
                HandleUtility.Repaint();
                controlPointToDelete = -1;
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.control)
            {


                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Undo.RecordObject(lakePolygon, "Add point");
                    Undo.RecordObject(lakePolygon.transform, "Add point");

                    Vector3 position = hit.point - lakePolygon.transform.position;
                    lakePolygon.AddPoint(position);

                    lakePolygon.GeneratePolygon();

#if VEGETATION_STUDIO
                    RegenerateVegetationMask();
#endif
#if VEGETATION_STUDIO_PRO
                    RegenerateBiomMask();
#endif

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

                    for (int j = 0; j < lakePolygon.points.Count; j++)
                    {
                        Vector3 handlePos = (Vector3)lakePolygon.points[j] + lakePolygon.transform.position;

                        float pointDist = Vector3.Distance(hit.point, handlePos);
                        if (pointDist < distanceMin)
                        {
                            distanceMin = pointDist;
                            idMin = j;
                        }
                    }

                    Vector3 posOne = (Vector3)lakePolygon.points[idMin] + lakePolygon.transform.position;
                    Vector3 posTwo;




                    Vector3 posPrev = (Vector3)lakePolygon.points[lakePolygon.ClampListPos(idMin - 1)] + lakePolygon.transform.position;
                    Vector3 posNext = (Vector3)lakePolygon.points[lakePolygon.ClampListPos(idMin + 1)] + lakePolygon.transform.position;

                    if (Vector3.Distance(hit.point, posPrev) > Vector3.Distance(hit.point, posNext))
                        posTwo = posNext;
                    else
                    {
                        posTwo = posPrev;
                        idMin = lakePolygon.ClampListPos(idMin - 1);
                    }




                    Handles.color = Handles.xAxisColor;
                    Handles.DrawLine(hit.point, posOne);
                    Handles.DrawLine(hit.point, posTwo);

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {

                        Undo.RecordObject(lakePolygon, "Add point");
                        Undo.RecordObject(lakePolygon.transform, "Add point");

                        Vector4 position = hit.point - lakePolygon.transform.position;
                        lakePolygon.AddPointAfter(idMin);
                        lakePolygon.ChangePointPosition(idMin + 1, position);

                        lakePolygon.GeneratePolygon();

                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                        HandleUtility.Repaint();
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

    bool CheckProfileChange()
    {
        if (lakePolygon.currentProfile == null)
            return false;

        if (lakePolygon.terrainCarve != lakePolygon.currentProfile.terrainCarve)
            return true;
        if (lakePolygon.distSmooth != lakePolygon.currentProfile.distSmooth)
            return true;
        if (lakePolygon.uvScale != lakePolygon.currentProfile.uvScale)
            return true;
        if (lakePolygon.distSmoothStart != lakePolygon.currentProfile.distSmoothStart)
            return true;
        if (lakePolygon.terrainPaintCarve != lakePolygon.currentProfile.terrainPaintCarve)
            return true;
        if (lakePolygon.currentSplatMap != lakePolygon.currentProfile.currentSplatMap)
            return true;

        if (lakePolygon.maximumTriangleSize != lakePolygon.currentProfile.maximumTriangleSize)
            return true;
        if (lakePolygon.traingleDensity != lakePolygon.currentProfile.traingleDensity)
            return true;


        return false;
    }

    void ResetToProfile()
    {


        MeshRenderer ren = lakePolygon.GetComponent<MeshRenderer>();
        ren.sharedMaterial = lakePolygon.currentProfile.lakeMaterial;


        lakePolygon.terrainCarve = lakePolygon.currentProfile.terrainCarve;
        lakePolygon.distSmooth = lakePolygon.currentProfile.distSmooth;
        lakePolygon.uvScale = lakePolygon.currentProfile.uvScale;
        lakePolygon.distSmoothStart = lakePolygon.currentProfile.distSmoothStart;
        lakePolygon.terrainPaintCarve = lakePolygon.currentProfile.terrainPaintCarve;
        lakePolygon.currentSplatMap = lakePolygon.currentProfile.currentSplatMap;

        lakePolygon.maximumTriangleSize = lakePolygon.currentProfile.maximumTriangleSize;
        lakePolygon.traingleDensity = lakePolygon.currentProfile.traingleDensity;



        lakePolygon.oldProfile = lakePolygon.currentProfile;


    }


}