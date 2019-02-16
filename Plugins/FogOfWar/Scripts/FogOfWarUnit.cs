using UnityEngine;
using System.Collections.Generic;
using CYM;

namespace FoW
{
    public enum FogOfWarShapeType
    {
        Circle,
        Box
    }

    [AddComponentMenu("FogOfWar/FogOfWarUnit")]
    public class FogOfWarUnit : MonoBehaviour
    {
        public int team = 0;
        [Range(0, 1)]
        public float brightness = 1;

        [Header("Shape")]
        public FogOfWarShapeType shapeType = FogOfWarShapeType.Circle;
        public bool absoluteOffset = false;
        public Vector2 offset = Vector2.zero;

        [Tooltip("For Box and Texture")]
        public Vector2 boxSize = new Vector2(5, 5);
        [Tooltip("For Circle")]
        public float circleRadius = 5;
        [Tooltip("For Circle"), Range(0.0f, 1.0f)]
        public float innerRadius = 1;
        [Tooltip("For Circle"), Range(0.0f, 180.0f)]
        public float angle = 180;
        [Tooltip("For Texture")]
        public Texture2D texture;
        [Tooltip("For Texture")]
        public bool rotateToForward = false;

        [Header("Line of Sight")]
        public LayerMask lineOfSightMask = 0;
        public float lineOfSightPenetration = 0;
        public bool cellBased = false;
        public bool antiFlicker = false;

        float[] _distances = null;
        bool[] _visibleCells = null;

        Transform _transform;
        public static HashList<FogOfWarUnit> registeredUnits { get; } = new HashList<FogOfWarUnit>();

        void Awake()
        {
            _transform = transform;
        }

        void OnEnable()
        {
            registeredUnits.Add(this);
        }

        void OnDisable()
        {
            registeredUnits.Remove(this);
        }

        static bool CalculateLineOfSight2D(Vector2 eye, float radius, float penetration, LayerMask layermask, float[] distances)
        {
            bool hashit = false;
            float angle = 360.0f / distances.Length;
            RaycastHit2D hit;

            for (int i = 0; i < distances.Length; ++i)
            {
                Vector2 dir = Quaternion.AngleAxis(angle * i, Vector3.back) * Vector2.up;
                hit = Physics2D.Raycast(eye, dir, radius, layermask);
                if (hit.collider != null)
                {
                    distances[i] = (hit.distance + penetration) / radius;
                    if (distances[i] < 1)
                        hashit = true;
                    else
                        distances[i] = 1;
                }
                else
                    distances[i] = 1;
            }

            return hashit;
        }

        static bool CalculateLineOfSight3D(Vector3 eye, float radius, float penetration, LayerMask layermask, float[] distances, Vector3 up, Vector3 forward)
        {
            bool hashit = false;
            float angle = 360.0f / distances.Length;
            RaycastHit hit;

            for (int i = 0; i < distances.Length; ++i)
            {
                Vector3 dir = Quaternion.AngleAxis(angle * i, up) * forward;
                if (Physics.Raycast(eye, dir, out hit, radius, layermask))
                {
                    distances[i] = (hit.distance + penetration) / radius;
                    if (distances[i] < 1)
                        hashit = true;
                    else
                        distances[i] = 1;
                }
                else
                    distances[i] = 1;
            }

            return hashit;
        }

        public float[] CalculateLineOfSight(FogOfWarPhysics physicsmode, Vector3 eyepos, FogOfWarPlane plane, float distance)
        {
            if (lineOfSightMask == 0)
                return null;

            if (_distances == null)
                _distances = new float[256];

            if (physicsmode == FogOfWarPhysics.Physics2D)
            {
                if (CalculateLineOfSight2D(eyepos, distance, lineOfSightPenetration, lineOfSightMask, _distances))
                    return _distances;
            }
            else if (plane == FogOfWarPlane.XZ) // 3D
            {
                if (CalculateLineOfSight3D(eyepos, distance, lineOfSightPenetration, lineOfSightMask, _distances, Vector3.up, Vector3.forward))
                    return _distances;
            }
            else if (plane == FogOfWarPlane.XY) // 3D
            {
                if (CalculateLineOfSight3D(eyepos, distance, lineOfSightPenetration, lineOfSightMask, _distances, Vector3.back, Vector3.up))
                    return _distances;
            }
            return null;
        }

        static float Sign(float v)
        {
            if (Mathf.Approximately(v, 0))
                return 0;
            return v > 0 ? 1 : -1;
        }
        
        public bool[] CalculateLineOfSightCells(FogOfWar fow, FogOfWarPhysics physicsmode, Vector3 eyepos, float distance)
        {
            if (physicsmode == FogOfWarPhysics.Physics3D)
            {
                Debug.LogWarning("Physics3D is not supported with cells!", this);
                return null;
            }

            int rad = Mathf.RoundToInt(distance);
            int width = rad + rad + 1;
            if (_visibleCells == null || _visibleCells.Length != width * width)
                _visibleCells = new bool[width * width];

            Vector2 cellsize = (fow.mapResolution.vector2 * 1.1f) / fow.mapSize; // do 1.1 to bring it away from the collider a bit so the raycast won't hit it
            Vector2 playerpos = FogOfWarConversion.SnapWorldPositionToNearestFogPixel(fow, _transform.position, fow.mapOffset, fow.mapResolution, fow.mapSize);
            for (int y = -rad; y <= rad; ++y)
            {
                for (int x = -rad; x <= rad; ++x)
                {
                    Vector2i offset = new Vector2i(x, y);

                    // find the nearest point in the cell to the player and raycast to that point
                    Vector2 fogoffset = offset.vector2 - new Vector2(Sign(offset.x) * cellsize.x, Sign(offset.y) * cellsize.y) * 0.5f;
                    Vector2 worldoffset = FogOfWarConversion.FogToWorldSize(fogoffset, fow.mapResolution, fow.mapSize);
                    Vector2 worldpos = playerpos + worldoffset;

                    Debug.DrawLine(playerpos, worldpos);

                    int idx = (y + rad) * width + x + rad;

                    // if it is out of range
                    if (worldoffset.magnitude > distance)
                        _visibleCells[idx] = false;
                    else
                    {
                        _visibleCells[idx] = true;
                        RaycastHit2D hit = Physics2D.Raycast(playerpos, worldoffset.normalized, Mathf.Max(worldoffset.magnitude - lineOfSightPenetration, 0.00001f), lineOfSightMask);
                        _visibleCells[idx] = hit.collider == null;
                    }
                }
            }

            return _visibleCells;
        }

        void FillShape(FogOfWar fow, FogOfWarShape shape)
        {
            if (antiFlicker)
            {
                // snap to nearest fog pixel
                shape.eyePosition = FogOfWarConversion.SnapWorldPositionToNearestFogPixel(fow, FogOfWarConversion.WorldToFogPlane(_transform.position, fow.plane), fow.mapOffset, fow.mapResolution, fow.mapSize);
                shape.eyePosition = FogOfWarConversion.FogPlaneToWorld(shape.eyePosition.x, shape.eyePosition.y, _transform.position.y, fow.plane);
            }
            else
                shape.eyePosition = _transform.position;
            shape.brightness = brightness;
            shape.foward = FogOfWarConversion.TransformFogPlaneForward(_transform, fow.plane);
            shape.absoluteOffset = absoluteOffset;
            shape.offset = offset;
            shape.radius = circleRadius;
            shape.size = boxSize;
        }

        FogOfWarShape CreateShape(FogOfWar fow)
        {
            if (shapeType == FogOfWarShapeType.Circle)
            {
                FogOfWarShapeCircle shape = new FogOfWarShapeCircle();
                FillShape(fow, shape);
                shape.innerRadius = innerRadius;
                shape.angle = angle;
                return shape;
            }
            else if (shapeType == FogOfWarShapeType.Box)
            {
                FogOfWarShapeBox shape = new FogOfWarShapeBox();
                shape.texture = texture;
                shape.hasTexture = texture != null;
                shape.rotateToForward = rotateToForward;
                FillShape(fow, shape);
                return shape;
            }
            return null;
        }

        public FogOfWarShape GetShape(FogOfWar fow, FogOfWarPhysics physics, FogOfWarPlane plane)
        {
            FogOfWarShape shape = CreateShape(fow);
            if (shape == null)
                return null;

            if (cellBased)
            {
                shape.lineOfSight = null;
                shape.visibleCells = CalculateLineOfSightCells(fow, physics, shape.eyePosition, shape.CalculateMaxLineOfSightDistance());
            }
            else
            {
                shape.lineOfSight = CalculateLineOfSight(physics, shape.eyePosition, plane, shape.CalculateMaxLineOfSightDistance());
                shape.visibleCells = null;
            }
            return shape;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 pos;
            if (absoluteOffset)
                pos = transform.position + new Vector3(offset.x, offset.y, offset.y);
            else
                pos = transform.position + transform.forward * offset.y + transform.right * offset.x;
            if (shapeType == FogOfWarShapeType.Circle)
                Gizmos.DrawWireSphere(pos, circleRadius);
            else if (shapeType == FogOfWarShapeType.Box)
                Gizmos.DrawWireCube(pos, new Vector3(boxSize.x, boxSize.y, boxSize.y));
        }
    }
}