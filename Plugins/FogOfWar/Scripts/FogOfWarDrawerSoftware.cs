using UnityEngine;

namespace FoW
{
    public class FogOfWarDrawerSoftware : FogOfWarDrawer
    {
        byte[] _values;
        FilterMode _filterMode;

        public override void GetValues(byte[] outvalues)
        {
            System.Array.Copy(_values, outvalues, _values.Length);
        }

        public override void SetValues(byte[] values)
        {
            System.Array.Copy(values, _values, _values.Length);
        }

        protected override void OnInitialise()
        {
            if (_values == null || _values.Length != _map.pixelCount)
                _values = new byte[_map.pixelCount];
            
            _filterMode = _map.filterMode;
        }

        public override void Clear(byte value)
        {
            for (int i = 0; i < _values.Length; ++i)
                _values[i] = value;
        }

        public override void Fade(byte[] currentvalues, byte[] totalvalues, float partialfogamount, int amount)
        {
            // partial fog needs to be inversed
            partialfogamount = 1 - partialfogamount;
            int partialfog = (int)(partialfogamount * (1 << 8));

            for (int i = 0; i < currentvalues.Length; ++i)
            {
                // if nothing has changed, don't do anything
                if (currentvalues[i] == totalvalues[i])
                    continue;

                // decrease fog
                if (currentvalues[i] < totalvalues[i])
                    totalvalues[i] = (byte)Mathf.Max(totalvalues[i] - amount, currentvalues[i]);
                else
                {
                    // increase fog
                    int target = (currentvalues[i] * partialfog) >> 8;
                    if (totalvalues[i] < target)
                        totalvalues[i] = (byte)Mathf.Min(totalvalues[i] + amount, target);
                }
            }
        }

        bool LineOfSightCanSee(FogOfWarShape shape, Vector2 offset, float fogradius)
        {
            if (shape.lineOfSight == null)
                return true;
            
            float idx = FogOfWarUtils.ClockwiseAngle(Vector2.up, offset) * shape.lineOfSight.Length / 360.0f;
            if (idx < 0)
                idx += shape.lineOfSight.Length;

            // sampling
            float value;
            if (_map.filterMode == FilterMode.Point)
                value = shape.lineOfSight[Mathf.RoundToInt(idx) % shape.lineOfSight.Length];
            else
            {
                int idxlow = Mathf.FloorToInt(idx);
                int idxhigh = (idxlow + 1) % shape.lineOfSight.Length;
                value = Mathf.LerpUnclamped(shape.lineOfSight[idxlow], shape.lineOfSight[idxhigh], idx % 1);
            }

            float dist = value * fogradius;
            return offset.sqrMagnitude < dist * dist;
        }

        bool LineOfSightCanSeeCell(FogOfWarShape shape, Vector2i offset)
        {
            if (shape.visibleCells == null)
                return true;

            int radius = Mathf.RoundToInt(shape.radius);
            int width = radius + radius + 1;

            offset.x += radius;
            if (offset.x < 0 || offset.x >= width)
                return true;

            offset.y += radius;
            if (offset.y < 0 || offset.y >= width)
                return true;

            return shape.visibleCells[offset.y * width + offset.x];
        }

        struct DrawInfo
        {
            public Vector2 fogCenterPos;
            public Vector2i fogEyePos;
            public Vector2 fogForward;
            public float forwardAngle;
            public int xMin;
            public int xMax;
            public int yMin;
            public int yMax;

            public DrawInfo(FogOfWarMap map, FogOfWarShape shape)
            {
                // convert size to fog space
                Vector2 radius = shape.CalculateRadius() * map.pixelSize;
                fogForward = shape.foward;
                Vector2 relativeoffset;
                if (shape.absoluteOffset)
                {
                    forwardAngle = 0;
                    relativeoffset = shape.offset;
                }
                else
                {
                    forwardAngle = FogOfWarUtils.ClockwiseAngle(Vector2.up, fogForward) * Mathf.Deg2Rad;
                    float sin = Mathf.Sin(-forwardAngle);
                    float cos = Mathf.Cos(-forwardAngle);
                    relativeoffset = new Vector2(shape.offset.x * cos - shape.offset.y * sin, shape.offset.x * sin + shape.offset.y * cos);
                }

                fogCenterPos = FogOfWarConversion.WorldToFog(FogOfWarConversion.WorldToFogPlane(shape.eyePosition, map.plane) + relativeoffset, map.offset, map.resolution, map.size);
                fogEyePos = new Vector2i(FogOfWarConversion.WorldToFog(shape.eyePosition, map.plane, map.offset, map.resolution, map.size));

                // find ranges
                if (shape.visibleCells == null)
                {
                    xMin = Mathf.Max(0, Mathf.RoundToInt(fogCenterPos.x - radius.x));
                    xMax = Mathf.Min(map.resolution.x - 1, Mathf.RoundToInt(fogCenterPos.x + radius.x));
                    yMin = Mathf.Max(0, Mathf.RoundToInt(fogCenterPos.y - radius.y));
                    yMax = Mathf.Min(map.resolution.y - 1, Mathf.RoundToInt(fogCenterPos.y + radius.y));
                }
                else
                {
                    fogCenterPos = FogOfWarConversion.SnapToNearestFogPixel(fogCenterPos);
                    fogEyePos = new Vector2i(FogOfWarConversion.SnapToNearestFogPixel(FogOfWarConversion.WorldToFog(shape.eyePosition, map.offset, map.resolution, map.size)));

                    Vector2i pos = new Vector2i(Mathf.RoundToInt(fogCenterPos.x), Mathf.RoundToInt(fogCenterPos.y));
                    Vector2i rad = new Vector2i(Mathf.RoundToInt(radius.x), Mathf.RoundToInt(radius.y));
                    xMin = Mathf.Max(0, Mathf.RoundToInt(pos.x - rad.x));
                    xMax = Mathf.Min(map.resolution.x - 1, Mathf.RoundToInt(pos.x + rad.x));
                    yMin = Mathf.Max(0, Mathf.RoundToInt(pos.y - rad.y));
                    yMax = Mathf.Min(map.resolution.y - 1, Mathf.RoundToInt(pos.y + rad.y));
                }
            }
        }

        byte SampleTexture(Texture2D texture, float u, float v, float brightness)
        {
            // GetPixel() and GetPixelBilinear() are not supported on other threads!
            if (_map.multithreaded)
                return 0;

            float value = 0;
            if (_filterMode == FilterMode.Point)
                value = 1 - texture.GetPixel(Mathf.FloorToInt(u * texture.width), Mathf.FloorToInt(v * texture.height)).a;
            else
                value = 1 - texture.GetPixelBilinear(u, v).a;
            value = 1 - (1 - value) * brightness;
            return (byte)(value * 255);
        }

        void Unfog(int x, int y, byte v)
        {
            int index = y * _map.resolution.x + x;
            if (_values[index] > v)
                _values[index] = v;
        }

        protected override void DrawCircle(FogOfWarShapeCircle shape)
        {
            int fogradius = Mathf.RoundToInt(shape.radius * _map.pixelSize);
            int fogradiussqr = fogradius * fogradius;
            DrawInfo info = new DrawInfo(_map, shape);
            float lineofsightradius = shape.CalculateMaxLineOfSightDistance() * _map.pixelSize;

            // view angle stuff
            float dotangle = 1 - shape.angle / 90;
            bool usedotangle = dotangle > -0.99f;

            for (int y = info.yMin; y <= info.yMax; ++y)
            {
                for (int x = info.xMin; x <= info.xMax; ++x)
                {
                    // is pixel within circle radius
                    Vector2 centeroffset = new Vector2(x, y) - info.fogCenterPos;
                    if (shape.visibleCells == null && centeroffset.sqrMagnitude >= fogradiussqr)
                        continue;

                    // check if in view angle
                    if (usedotangle && Vector2.Dot(centeroffset.normalized, info.fogForward) <= dotangle)
                        continue;

                    // can see pixel
                    Vector2i offset = new Vector2i(x, y) - info.fogEyePos;
                    if (!LineOfSightCanSee(shape, offset.vector2, lineofsightradius))
                        continue;

                    if (!LineOfSightCanSeeCell(shape, offset))
                        continue;

                    Unfog(x, y, shape.GetFalloff(centeroffset.magnitude / lineofsightradius));
                }
            }
        }

        protected override void DrawBox(FogOfWarShapeBox shape)
        {
            if (shape.rotateToForward)
                DrawRotatedBox(shape);
            else
                DrawAxisAlignedBox(shape);
        }

        void DrawAxisAlignedBox(FogOfWarShapeBox shape)
        {
            // convert size to fog space
            DrawInfo info = new DrawInfo(_map, shape);
            float lineofsightradius = shape.CalculateMaxLineOfSightDistance() * _map.pixelSize + 0.01f;

            byte brightness = shape.maxBrightness;
            bool drawtexture = shape.hasTexture && !_isMultithreaded;
            for (int y = info.yMin; y <= info.yMax; ++y)
            {
                for (int x = info.xMin; x <= info.xMax; ++x)
                {
                    // can see pixel
                    Vector2i offset = new Vector2i(x, y) - info.fogEyePos;
                    if (!LineOfSightCanSee(shape, offset.vector2, lineofsightradius))
                        continue;

                    if (!LineOfSightCanSeeCell(shape, offset))
                        continue;

                    // unfog
                    if (drawtexture)
                    {
                        float u = Mathf.InverseLerp(info.xMin, info.xMax, x);
                        float v = Mathf.InverseLerp(info.yMin, info.yMax, y);
                        Unfog(x, y, SampleTexture(shape.texture, u, v, shape.brightness));
                    }
                    else
                        Unfog(x, y, brightness);
                }
            }
        }

        void DrawRotatedBox(FogOfWarShapeBox shape)
        {
            // convert size to fog space
            DrawInfo info = new DrawInfo(_map, shape);
            float lineofsightradius = shape.CalculateMaxLineOfSightDistance() * _map.pixelSize;

            // rotation stuff
            Vector2 sizemul = shape.size * 0.5f * _map.pixelSize;
            Vector2 invfogsize = new Vector2(1.0f / (shape.size.x * _map.pixelSize), 1.0f / (shape.size.y * _map.pixelSize));
            float sin = Mathf.Sin(info.forwardAngle);
            float cos = Mathf.Cos(info.forwardAngle);

            byte brightness = shape.maxBrightness;
            bool drawtexture = shape.hasTexture && !_isMultithreaded;
            for (int y = info.yMin; y < info.yMax; ++y)
            {
                float yy = y - info.fogCenterPos.y;

                for (int x = info.xMin; x < info.xMax; ++x)
                {
                    float xx = x - info.fogCenterPos.x;

                    // get rotated uvs
                    float u = xx * cos - yy * sin;
                    if (u < -sizemul.x || u >= sizemul.x)
                        continue;
                    float v = yy * cos + xx * sin;
                    if (v < -sizemul.y || v >= sizemul.y)
                        continue;

                    // can see pixel
                    Vector2i offset = new Vector2i(x, y) - info.fogEyePos;
                    if (!LineOfSightCanSee(shape, offset.vector2, lineofsightradius))
                        continue;

                    if (!LineOfSightCanSeeCell(shape, offset))
                        continue;

                    // unfog
                    if (drawtexture)
                        Unfog(x, y, SampleTexture(shape.texture, 0.5f + u * invfogsize.x, 0.5f + v * invfogsize.y, shape.brightness));
                    else
                        Unfog(x, y, brightness);
                }
            }
        }

        public override void SetFog(Rect rect, byte value)
        {
            rect.xMin = Mathf.Max(rect.xMin, 0);
            rect.xMax = Mathf.Min(rect.xMax, _map.resolution.x);
            rect.yMin = Mathf.Max(rect.yMin, 0);
            rect.yMax = Mathf.Min(rect.yMax, _map.resolution.y);

            for (int y = (int)rect.yMin; y < (int)rect.yMax; ++y)
            {
                for (int x = (int)rect.xMin; x < (int)rect.xMax; ++x)
                    _values[y * _map.resolution.x + x] = value;
            }
        }
    }
}
