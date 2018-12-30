using UnityEngine;

namespace FoW
{
    // These are the different coordinate systems used:
    //  - World Space: Vector3 in Unity's world space coordinates
    //  - Fog Plane: Vector2 of World Space projected onto the plane. This will just be World Space but the Y or Z component dropped
    //  - Fog Space: Vector2 in the fog texture space. Valid positions will be 0 to FogOfWar.Resolution
    public static class FogOfWarConversion
    {
        // converts world position to fog plane pos where x and y are on the plane
        public static Vector2 WorldToFogPlane(Vector3 position, FogOfWarPlane plane)
        {
            if (plane == FogOfWarPlane.XY)
                return new Vector2(position.x, position.y);
            else if (plane == FogOfWarPlane.YZ)
                return new Vector2(position.y, position.z);
            else if (plane == FogOfWarPlane.XZ)
                return new Vector2(position.x, position.z);

            Debug.LogError("FogOfWarPlane is an invalid value!");
            return Vector2.zero;
        }

        // converts world position to fog plane pos where x and y are on the plane and z is perpendicular
        public static Vector3 WorldToFogPlane3(Vector3 position, FogOfWarPlane plane)
        {
            if (plane == FogOfWarPlane.XY)
                return new Vector3(position.x, position.y, position.z);
            else if (plane == FogOfWarPlane.YZ)
                return new Vector3(position.y, position.z, position.x);
            else if (plane == FogOfWarPlane.XZ)
                return new Vector3(position.x, position.z, position.y);
            else
            {
                Debug.LogError("FogOfWarPlane is an invalid value!");
                return Vector2.zero;
            }
        }

        // gets a transform's forward direction on a fog plane
        public static Vector2 TransformFogPlaneForward(Transform transform, FogOfWarPlane plane)
        {
            if (plane == FogOfWarPlane.XY)
                return new Vector2(transform.up.x, transform.up.y).normalized;
            else if (plane == FogOfWarPlane.YZ)
                return new Vector2(transform.up.z, transform.up.y).normalized;
            else if (plane == FogOfWarPlane.XZ)
                return new Vector2(transform.forward.x, transform.forward.z).normalized;

            Debug.LogError("FogOfWarPlane is an invalid value!");
            return Vector2.zero;
        }

        public static Vector2 FogToWorldSize(Vector2 fpos, Vector2i resolution, float size)
        {
            Vector2 res = resolution.vector2;
            fpos.x *= size / res.x;
            fpos.y *= size / res.y;
            return fpos;
        }
        
        public static Vector2 FogToWorld(Vector2 fpos, Vector2 offset, Vector2i resolution, float size)
        {
            Vector2 res = resolution.vector2;
            fpos -= res * 0.5f;
            fpos.x *= size / res.x;
            fpos.y *= size / res.y;
            return fpos + offset;
        }

        public static Vector2 WorldToFog(Vector2 wpos, Vector2 offset, Vector2i resolution, float size)
        {
            wpos -= offset;
            Vector2 res = resolution.vector2;
            wpos.x *= res.x / size;
            wpos.y *= res.y / size;
            return wpos + res * 0.4999f; // this should be 0.5f, but it can cause some visible floating point issues when using lower resolution maps
        }

        public static Vector2 WorldToFog(Vector3 wpos, FogOfWarPlane plane, Vector2 offset, Vector2i resolution, float size)
        {
            return WorldToFog(WorldToFogPlane(wpos, plane), offset, resolution, size);
        }

        public static Vector3 FogPlaneToWorld(float x, float y, float z, FogOfWarPlane plane)
        {
            if (plane == FogOfWarPlane.XY)
                return new Vector3(x, y, z);
            else if (plane == FogOfWarPlane.YZ)
                return new Vector3(z, x, y);
            else if (plane == FogOfWarPlane.XZ)
                return new Vector3(x, z, y);

            Debug.LogError("FogOfWarPlane is an invalid value!");
            return Vector3.zero;
        }

        public static Vector2 SnapToNearestFogPixel(Vector2 fogpos)
        {
            fogpos.x = Mathf.Floor(fogpos.x) + 0.4999f;
            fogpos.y = Mathf.Floor(fogpos.y) + 0.4999f;
            return fogpos;
        }

        // Snaps a world point to a fog pixel. It returns the position 
        public static Vector2 SnapWorldPositionToNearestFogPixel(FogOfWar fow, Vector2 worldpos, Vector2 offset, Vector2i resolution, float size)
        {
            Vector2 fogpos = WorldToFog(worldpos, fow.mapOffset, fow.mapResolution, fow.mapSize);
            fogpos = SnapToNearestFogPixel(fogpos);
            return FogToWorld(fogpos, fow.mapOffset, fow.mapResolution, fow.mapSize);
        }
    }
}
