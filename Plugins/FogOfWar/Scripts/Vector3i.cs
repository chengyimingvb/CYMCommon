using UnityEngine;

namespace FoW
{
    [System.Serializable]
    public struct Vector3i
    {
        public int x;
        public int y;
        public int z;

        public int this[int idx]
        {
            get { return idx == 0 ? x : y; }
            set
            {
                switch (idx)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    default:
                        z = value;
                        break;
                }
            }
        }

        public Vector2 vector2
        {
            get
            {
                return new Vector2(x, y);
            }
        }

        public Vector3 vector3
        {
            get
            {
                return new Vector3(x, y, z);
            }
        }

        public Vector3 normalized
        {
            get
            {
                float invmag = 1.0f / magnitude;
                return new Vector3(invmag * x, invmag * y, invmag * z);
            }
        }

        public float magnitude
        {
            get { return Mathf.Sqrt(x * x + y * y + z * z); }
        }

        public int sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }

        public int manhattanMagnitude
        {
            get { return Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z); }
        }

        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3i(Vector3 v)
        {
            x = Mathf.RoundToInt(v.x);
            y = Mathf.RoundToInt(v.y);
            z = Mathf.RoundToInt(v.z);
        }

        public static Vector3i operator +(Vector3i c1, Vector3i c2)
        {
            return new Vector3i(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z);
        }

        public static Vector3 operator +(Vector3i c1, Vector3 c2)
        {
            return new Vector3(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z);
        }

        public static Vector3 operator +(Vector3 c1, Vector3i c2)
        {
            return new Vector3(c1.x + c2.x, c1.y + c2.y, c1.z + c2.z);
        }

        public static Vector3i operator -(Vector3i c1, Vector3i c2)
        {
            return new Vector3i(c1.x - c2.x, c1.y - c2.y, c1.z - c2.z);
        }

        public static Vector3i operator *(Vector3i c1, int c2)
        {
            return new Vector3i(c1.x * c2, c1.y * c2, c1.z * c2);
        }

        public static Vector3 operator *(Vector3i c1, float c2)
        {
            return new Vector3(c1.x * c2, c1.y * c2, c1.z * c2);
        }

        public static Vector3i operator *(int c1, Vector3i c2)
        {
            return new Vector3i(c1 * c2.x, c1 * c2.y, c1 * c2.z);
        }

        public static Vector3 operator *(float c1, Vector3i c2)
        {
            return new Vector3(c1 * c2.x, c1 * c2.y, c1 * c2.z);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;

            Vector3i p = (Vector3i)obj;
            if ((System.Object)p == null)
                return false;

            return (x == p.x) && (y == p.y) && (z == p.z);
        }

        public bool Equals(Vector3i p)
        {
            if ((object)p == null)
                return false;

            return (x == p.x) && (y == p.y) && (z == p.z);
        }

        public static bool operator ==(Vector3i a, Vector3i b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(Vector3i a, Vector3i b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return x ^ y ^ z;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }

        public static Vector3i zero { get { return new Vector3i(0, 0, 0); } }
        public static Vector3i one { get { return new Vector3i(1, 1, 1); } }
    }
}