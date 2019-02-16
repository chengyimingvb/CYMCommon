using UnityEngine;
using CYM;
using System.Collections.Generic;
namespace CYM
{
    public enum Direct:int
    {
        Right = -1,
        Left = 1,
        Up = 2,
        Down = -2,
    }
    //数学计算通用类
    public class BaseMathUtils
    {
        public static float XZSqrDistance(Vector3 a, Vector3 b)
        {
            Vector3 c = a - b;
            c.y = 0;
            return c.sqrMagnitude;
        }

        public static float XZDistance(Vector3 a, Vector3 b)
        {
            Vector3 c = a - b;
            c.y = 0;
            return c.magnitude;
        }

        /// <summary>
        /// 两个向量是否近似
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Approximately(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                Mathf.Approximately(a.y, b.y) &&
                Mathf.Approximately(a.z, b.z);
        }
        /// <summary>
        /// 比较两个位置点是否相近
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="k"></param>
        /// <param name="invers"></param>
        /// <returns></returns>
        public static bool PosCompare(Vector3 a, Vector3 b,float k,bool invers=false)
        {
            if (!invers)
            {
                return Mathf.Abs(a.x - b.x) <= k &&
                Mathf.Abs(a.y - b.y) <= k &&
                Mathf.Abs(a.z - b.z) <= k;
            }
            else
            {
                return Mathf.Abs(a.x - b.x) >= k ||
                Mathf.Abs(a.y - b.y) >= k ||
                Mathf.Abs(a.z - b.z) >= k;
            }
        }
        /// <summary>
        /// 比较两个位置点是否相近
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="k"></param>
        /// <param name="invers"></param>
        /// <returns></returns>
        public static bool XZPosCompare(Vector3 a, Vector3 b, float k, bool invers = false)
        {
            if (!invers)
            {
                return Mathf.Abs(a.x - b.x) <= k &&
                    Mathf.Abs(a.z - b.z) <= k;
            }
            else
            {
                return Mathf.Abs(a.x - b.x) >= k ||
                    Mathf.Abs(a.z - b.z) >= k;
            }
        }

        public static float Round(float val, int count)
        {
            return (float)System.Math.Round((double)val, count);
        }
        /// <summary>
        /// 0-1.0f
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool Rand(float prop)
        {
            if (prop >= 1.0f)
                return true;
            if (prop <= 0.0f)
                return false;
            return prop * 100.0f >= UnityEngine.Random.Range(0.0f, 100.0f);
        }
        /// <summary>
        /// min [inclusive] and max [exclusive]
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandInt(int min,int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        public static float Clamp0(float val)
        {
            return Mathf.Clamp(val, 0.0f, float.MaxValue);
        }
        public static int Clamp0(int val)
        {
            return Mathf.Clamp(val, 0, int.MaxValue);
        }
        public static float Clamp(float val,float min,float max)
        {
            return Mathf.Clamp(val, min, max);
        }
        public static float Clamp(int val, int min, int max)
        {
            return Mathf.Clamp(val, min, max);
        }
        /// <summary>
        /// 随机范围,包含min,不包含max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Range(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        /// <summary>
        /// 随机范围,包含mini,不包含max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Range(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        public static int RangeArray(int max)
        {
            if (max <= 0)
                return 0;
            return Range(0, max);
        }
        public static T RandArray<T>(T[] array)
        {
            if (array == null)
                return default(T);
            if (array.Length <= 0)
                return default(T);
            return array[RangeArray(array.Length)];
        }
        public static T RandArray<T>(HashList<T> array)
        {
            if (array == null)
                return default(T);
            if (array.Count <= 0)
                return default(T);
            return array[RangeArray(array.Count)];
        }
        public static T RandArray<T>(List<T> array)
        {
            if (array == null)
                return default(T);
            if (array.Count <= 0)
                return default(T);
            return array[RangeArray(array.Count)];
        }
        public static T GetSafeArray<T>(T[] array,int index)
        {
            if (array == null)
                return default(T);
            if (array.Length <= 0)
                return default(T);
            if(index>= array.Length)
                return default(T);
            return array[index];
        }
        public static T GetSafeArray<T>(List<T> array, int index)
        {
            if (array == null)
                return default(T);
            if (array.Count <= 0)
                return default(T);
            if (index >= array.Count)
                return default(T);
            return array[index];
        }
        public static Vector3 RadomCirclePoint(Vector3 center, float radius)
        {
            Quaternion rotation = Quaternion.Euler(0f, (int)UnityEngine.Random.Range(0, 36) * 10, 0f);
            Vector3 newPos = center + rotation * new Vector3(0, 0f, radius);
            return newPos;
        }
        public static Quaternion RandomQuaterion()
        {
            return Quaternion.Euler(0f, (int)UnityEngine.Random.Range(0, 36) * 10, 0f);
        }
        public static Vector3 RadomInsideCirclePoint(Vector3 center, float radius)
        {
            Vector2 radom_pos = UnityEngine.Random.insideUnitCircle * radius;
            Vector3 pos = center + new Vector3(radom_pos.x, 0, radom_pos.y);
            return pos;
        }

        public static Vector3 RadomOnSpherePoint(Vector3 center, float radius)
        {
            Vector3 radom_pos = UnityEngine.Random.onUnitSphere * radius;
            Vector3 pos = center + radom_pos;
            return pos;
        }

        public static int GaussianRandomRange(int min, int max)
        {
            return GaussianRandom.Range(min, max);
        }

        public static Direct UpOrDown(Vector3 up, Vector3 forward2)
        {
            float val = Vector3.Dot(up, forward2);
            return val > 0 ? Direct.Up : Direct.Down;
        }
        public static Direct UpOrDown(BaseMono self, BaseMono target)
        {
            if (self == null)
                throw new System.Exception("self 为空");
            if (target == null)
                throw new System.Exception("target 为空");
            return UpOrDown(self.Trans.forward, target.Pos - self.Pos);
        }
        public static Direct LeftOrRight(Vector3 forward, Vector3 forward2)
        {
            float val = Vector3.Cross(forward, forward2).y;
            return val > 0 ? Direct.Right : Direct.Left;
        }
        public static Direct LeftOrRight(BaseMono self, BaseMono target)
        {
            if (self == null)
                throw new System.Exception("self 为空");
            if (target == null)
                throw new System.Exception("target 为空");
            return LeftOrRight(self.Trans.forward, target.Pos - self.Pos);
        }
        public static Direct Invert(Direct dir)
        {
            return (Direct)(-(int)dir);
        }

        //curve calculation for ease out effect
        public static float Sinerp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
        }

        //curve calculation for ease in effect
        public static float Coserp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
        }

        //curve calculation for easing at start + end
        public static float CoSinLerp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
        }

        /// <summary>
        /// 检测Y轴碰撞体,防止下沉
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="yoffset"></param>
        /// <param name="mask"></param>
        public static void RaycastY(Transform trans, float yoffset, LayerData layer)
        {
            trans.position = GetRaycastY(trans, yoffset, layer);
        }
        /// <summary>
        /// 检测Y轴碰撞体,防止下沉
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="yoffset"></param>
        /// <param name="layer"></param>
        public static void RaycastY(Transform trans, float yoffset, LayerMask mask)
        {
            trans.position = GetRaycastY(trans, yoffset, mask);
        }
        /// <summary>
        /// 获得Y轴坐标
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="yoffset"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static Vector3 GetRaycastY(Transform trans, float yoffset, LayerData layer)
        {
            return GetRaycastY(trans, yoffset, (LayerMask)layer);
        }
        /// <summary>
        /// 获得Y轴坐标
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="yoffset"></param>
        /// <param name="mask"></param>
        /// <returns></returns>
        public static Vector3 GetRaycastY(Transform trans, float yoffset, LayerMask mask)
        {
            RaycastHit hitInfo;
            Vector3 opos = trans.position + Vector3.up * 999.0f;
            Physics.Raycast(new Ray(opos, trans.position - opos), out hitInfo, float.MaxValue, mask);
            return new Vector3(hitInfo.point.x, yoffset + hitInfo.point.y, hitInfo.point.z);
        }


        /// <summary>
        /// 是否方向相同
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static bool IsSameDirect(Vector3 dir1,Vector3 dir2,float val=0.5f)
        {
            return Vector3.Dot(dir1,dir2)>=val;
        }
        /// <summary>
        /// 是否方向相反
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static bool IsDiffDirect(Vector3 dir1, Vector3 dir2, float val = 0.0f)
        {
            return Vector3.Dot(dir1, dir2) <= val;
        }
        /// <summary>
        /// 是否面对
        /// </summary>
        /// <returns></returns>
        public static bool IsFace(BaseCoreMono self, BaseCoreMono target)
        {
            return Vector3.Dot(self.Forward, target.Pos-self.Pos) >=0?true:false;
        }
        /// <summary>
        /// 是否面对
        /// </summary>
        /// <returns></returns>
        public static bool IsFace(BaseCoreMono self, Vector3 target)
        {
            return Vector3.Dot(self.Forward, target - self.Pos) >= 0 ? true : false;
        }

        /// <summary>
        /// 转换屏幕坐标X
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public static float PosScreenX(Vector3 WorldPos)
        {
            var Pos = Camera.main.WorldToScreenPoint(WorldPos);
            float x = Pos.x / Screen.width;
            return x;
        }
        /// <summary>
        /// 转换屏幕坐标Y
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public static float PosScreenY(Vector3 WorldPos)
        {
            var Pos = Camera.main.WorldToScreenPoint(WorldPos);
            float y = Pos.y / Screen.height;
            return y;
        }
        /// <summary>
        /// 世界坐标转换到屏幕坐标
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public static Vector2 PosScreen(Vector3 WorldPos)
        {
            var Pos = Camera.main.WorldToScreenPoint(WorldPos);
            Vector2 retPos = new Vector2(Pos.x, Pos.y);

            return retPos;
        }
        /// <summary>
        /// 转换角度
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        static public float WrapAngle(float a)
        {
            while (a < -180.0f) a += 360.0f;
            while (a > 180.0f) a -= 360.0f;
            return a;
        }

        #region GaussianRandom
        class GaussianRandom
        {
            // x是右边的面积，y是标准差的数量
            // 精确度0.01
            // 计算器: http://onlinestatbook.com/2/calculators/inverse_normal_dist.html
            static readonly float[] QTable ={
            3f,//0.00
            2.327f,
            2.054f,
            1.881f,
            1.751f,
            1.654f,
            1.555f,
            1.476f,
            1.405f,
            1.341f,
            1.282f,//0.10
            1.227f,
            1.175f,
            1.126f,
            1.08f,
            1.036f,
            0.994f,
            0.954f,
            0.915f,
            0.878f,
            0.841f,//0.20
            0.806f,
            0.772f,
            0.739f,
            0.706f,
            0.674f,
            0.643f,
            0.612f,
            0.582f,
            0.553f,
            0.524f,//0.30
            0.495f,
            0.467f,
            0.439f,
            0.412f,
            0.385f,
            0.358f,
            0.331f,
            0.305f,
            0.279f,
            0.253f,//0.40
            0.227f,
            0.202f,
            0.176f,
            0.151f,
            0.125f,
            0.1f,
            0.075f,
            0.05f,
            0.025f,
            0f,//0.50
        };

            // f是右边的面积
            // f>=0 && f<=0.5
            static float LookUpTableDirect(float f)
            {
                int x = Mathf.FloorToInt(f * 100);
                int xPlus = x + 1;
                float y = QTable[x];
                float yPlus = 0;
                // 考虑数组越界情况
                if (xPlus > 50)
                {
                    yPlus = 0;
                }
                else
                {
                    yPlus = QTable[xPlus];
                }
                //float t = f - x;
                return Mathf.Lerp(y, yPlus, f - x);
            }

            // f>=0 && f<=1
            static float LookUpTable(float f)
            {
                if (f <= 0.5f)
                {
                    return LookUpTableDirect(f);
                }
                else
                {
                    return -LookUpTableDirect(1 - f);
                }
            }

            // p是左边的面积
            // n是标注差数量
            static float GetNForP(float p)
            {
                return LookUpTable(1 - p);
            }

            public static float NextN
            {
                get
                {
                    return GetNForP(UnityEngine.Random.value);
                }
            }

            // mean平均值
            // deviation标准差
            private static float GetGaussianRandom(float mean, float deviation)
            {
                float n = NextN;
                return n * deviation + mean;
            }

            // 假定范围为6个标准差
            public static int Range(int min, int max)
            {
                float mean = (max + min) / 2f;
                float deviation = (max - min) / 6f;
                return Mathf.Clamp(Mathf.RoundToInt(GetGaussianRandom(mean, deviation)), min, max);
            }
        }
        #endregion
    }


}

