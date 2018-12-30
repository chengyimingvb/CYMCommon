using UnityEngine;

namespace FoW
{
    [System.Serializable]
    public struct Vector2i
    {
        public int x;
        public int y;

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
                    default:
                        y = value;
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

        public Vector2Int vector2Int
        {
            get
            {
                return new Vector2Int(x, y);
            }
        }

        public Vector2i perp
        {
            get
            {
                return new Vector2i(y, x);
            }
        }

        public Vector2 normalized
        {
            get
            {
                float invmag = 1.0f / magnitude;
                return new Vector2(invmag * x, invmag * y);
            }
        }

        public float magnitude
        {
            get { return Mathf.Sqrt(x * x + y * y); }
        }

        public int sqrMagnitude
        {
            get { return x * x + y * y; }
        }

        public int manhattanMagnitude
        {
            get { return Mathf.Abs(x) + Mathf.Abs(y); }
        }

        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2i(Vector2 v)
        {
            x = Mathf.RoundToInt(v.x);
            y = Mathf.RoundToInt(v.y);
        }

        public Vector2i(Vector2Int v)
        {
            x = v.x;
            y = v.y;
        }

        public static Vector2i operator +(Vector2i c1, Vector2i c2)
        {
            return new Vector2i(c1.x + c2.x, c1.y + c2.y);
        }

        public static Vector2i operator +(Vector2i c1, int c2)
        {
            return new Vector2i(c1.x + c2, c1.y + c2);
        }

        public static Vector2 operator +(Vector2i c1, Vector2 c2)
        {
            return new Vector3(c1.x + c2.x, c1.y + c2.y);
        }

        public static Vector2 operator +(Vector2 c1, Vector2i c2)
        {
            return new Vector3(c1.x + c2.x, c1.y + c2.y);
        }

        public static Vector2i operator -(Vector2i c1, Vector2i c2)
        {
            return new Vector2i(c1.x - c2.x, c1.y - c2.y);
        }

        public static Vector2i operator *(Vector2i c1, int c2)
        {
            return new Vector2i(c1.x * c2, c1.y * c2);
        }

        public static Vector2 operator *(Vector2i c1, float c2)
        {
            return new Vector2(c1.x * c2, c1.y * c2);
        }

        public static Vector2i operator *(int c1, Vector2i c2)
        {
            return new Vector2i(c1 * c2.x, c1 * c2.y);
        }

        public static Vector2 operator *(float c1, Vector2i c2)
        {
            return new Vector2(c1 * c2.x, c1 * c2.y);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;

            Vector2i p = (Vector2i)obj;
            if ((System.Object)p == null)
                return false;

            return (x == p.x) && (y == p.y);
        }

        public bool Equals(Vector2i p)
        {
            if ((object)p == null)
                return false;

            return (x == p.x) && (y == p.y);
        }

        public static bool operator ==(Vector2i a, Vector2i b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public static Vector2i zero { get { return new Vector2i(0, 0); } }
        public static Vector2i one { get { return new Vector2i(1, 1); } }
    }
}