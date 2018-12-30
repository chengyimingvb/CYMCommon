using UnityEngine;
using System.Collections.Generic;
using CYM.UI;
using System;
using UnityEngine.UI;
using System.Globalization;
//**********************************************
// Class Name	: CYMBaseScreenController
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class BaseUIUtils
    {
        #region UI Number
        #region interface
        /// <summary>
        /// 1-100数值越大,颜色越红
        /// </summary>
        /// <param name="num"></param>
        /// <param name="isReverseColor"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static string RiseColorInt(float num, bool isReverseColor = false, int min = 0, int max = 100)
        {
            int round = Mathf.RoundToInt(num);
            int small = Mathf.RoundToInt((max - min) * 0.3f + min);
            int big = Mathf.RoundToInt((max - min) * 0.7f + min);
            float colorSign = GetRiseColorSign(round, small, big);
            if (isReverseColor)
            {
                colorSign = -colorSign;
            }

            return Decorate(round.ToString(), GetColor(colorSign));
        }
        /// <summary>
        /// 1-100数值越小,颜色越红
        /// </summary>
        /// <param name="num"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static string DeriseColorInt(float num, int min = 0, int max = 100)
        {
            return RiseColorInt(num, true, min, max);
        }
        /// <summary>
        /// 1-100数值越小,颜色越红
        /// </summary>
        /// <param name="num"></param>
        /// <param name="isReverseColor"></param>
        /// <returns></returns>
        public static string RiseColorPercentOptionTwoDigit(float num, bool isReverseColor = false, bool isHaveSignal = true)
        {
            string percentStr = Percent(num, isHaveSignal);
            if (num > 0.7)
                return Green(percentStr);
            if (num <= 0.7f)
            {
                return Yellow(percentStr);
            }
            if (num <= 0.3f)
            {
                return Red(percentStr);
            }
            return percentStr;
        }
        public static float GetRiseColorSign(int num, int small, int big)
        {
            return num < small ? -1 : (big < 70 ? 0 : 1);
        }

        public static string Floor(float f)
        {
            return Mathf.FloorToInt(f).ToString();
        }

        public static string Ceil(float f)
        {
            return Mathf.CeilToInt(f).ToString();
        }

        public static string Round(float f)
        {
            return Mathf.RoundToInt(f).ToString();
        }

        public static string RoundSign(float f)
        {
            return GetSign(f) + Round(Mathf.Abs(f));
        }

        public static string RoundColor(float f)
        {
            return DecorateStr(Round(f), f, false, false);
        }

        public static string Plain(int i)
        {
            return i.ToString();
        }

        public static string Plain(float f)
        {
            return f.ToString();
        }

        public static string ColorAndSign(int number)
        {
            return DecorateStr(number.ToString(), number, true, false);
        }

        public static string ColorAndSign(float number, bool reverseColor = false)
        {
            return DecorateStr(number.ToString(), number, true, reverseColor);
        }

        public static string Color(int number, bool reverseColor = false)
        {
            return DecorateStr(number.ToString(), number, false, reverseColor);
        }

        public static string Color(float number, bool reverseColor = false)
        {
            return DecorateStr(number.ToString(), number, false, reverseColor);
        }

        /// <summary>
        /// 小于1的时候显示1位小数,否则返回整数
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string RoundDigit(float f)
        {
            if(f==0.0f)
                return Round(f);
            if (f<1.0f&&f>-1.0f)
                return string.Format("{0:0.0}", f);
            else
                return Round(f);
        }

        public static string TwoDigit(float f)
        {
            return string.Format("{0:0.00}", f);
        }

        public static string OptionalTwoDigit(float f)
        {
            return string.Format("{0:0.##}", f);
        }
        public static string OptionalOneDigit(float f)
        {
            return string.Format("{0:0.#}", f);
        }
        public static string CDStyle(float f)
        {
            if (f > 1.0f)
                return Round(f);
            return string.Format("{0:0.0}", f);
        }
        public static string OptionalTwoDigitSign(float f)
        {
            string sign = GetSign(f);
            return string.Format("{1}{0:0.##}", f, sign);
        }
        public static string OptionalTwoDigitColor(float f)
        {
            string color = GetColor(f);
            return string.Format("<color={0}>{1:0.##}</color>", color, f);
        }
        public static string OptionalTwoDigitColorAndsign(float f)
        {
            string sign = GetSign(f);
            string color = GetColor(f);
            return string.Format("<color={0}>{1}{2:0.##}</color>", color, sign, f);
        }

        public static string KMG(int number)
        {
            float f = GetKMGNumber(number);
            return ValidDigit(f, 3) + GetKMGSuffix(number);
        }

        public static string KMG(float number)
        {
            return KMG(Mathf.RoundToInt(number));
        }

        public static string KMGColor(float number, bool reverseColor = false)
        {
            return KMGColor(Mathf.RoundToInt(number), reverseColor);
        }

        public static string KMGColor(int number, bool reverseColor = false)
        {
            return DecorateStr(KMG(number), number, false, reverseColor);
        }

        public static string KMGColorAndSign(int number)
        {
            return DecorateStr(KMG(number), number, true, false);
        }
        public static string KMGColorAndSign(float number)
        {
            return DecorateStr(KMG((int)number), number, true, false);
        }

        // 裁剪小数部分
        public static string PercentByInt(int percent)
        {
            return string.Format("{0}%", percent);
        }

        // 保留小数部分
        public static string Percent(float percent)
        {
            return string.Format("{0}%", OptionalOneDigit(percent * 100));
        }

        //百分号带小数
        public static string Percent(float percent, bool isHaveSignal = true)
        {
            return (string.Format("{0}", OptionalOneDigit(percent * 100)) + (isHaveSignal ? "%" : ""));
        }


        public static string PercentSign(float percent)
        {
            return percent > 0 ? "+" + Percent(percent) : Percent(percent);
        }

        public static string PercentColor(float percent, bool reverseColor = false)
        {
            return DecorateStr(Percent(percent), percent, false, reverseColor);
        }

        public static string Sign(float val)
        {
            return val > 0 ? "+" + val.ToString() : val.ToString();
        }

        public static string Sign(float val, string str)
        {
            return val > 0 ? "+" + str : str;
        }

        public static string PercentColorAndSign(float percent, bool reverseColor = false)
        {
            return DecorateStr(Percent(percent), percent, true, reverseColor);
        }

        /// <summary>
        /// 天枰颜色
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="reverseColor"></param>
        /// <param name="colorLeft"></param>
        /// <param name="colorRight"></param>
        /// <returns></returns>
        public static string PercentApparentlyColor(float percent, string colorLeft = "yellow", string colorRight = "green")
        {
            return string.Format("<color={0}>{1}</color>", percent <= 0.0f ? colorLeft : colorRight, Percent(Mathf.Abs(percent)));
        }

        public static string Decorate(string numberStr, string color)
        {
            return string.Format("<color={0}>{1}</color>", color, numberStr);
        }
        #endregion

        // 根据sign是否大于0决定一些东西
        // positiveSign:是否给正数写加号
        static string DecorateStr(string numberStr, float sign, bool positiveSign, bool reverseColor)
        {
            return string.Format("<color={0}>{1}{2}</color>", reverseColor ? GetColor(-sign) : GetColor(sign), positiveSign && sign > 0 ? "+" : "", numberStr);
        }

        static string GetSign(float number)
        {
            if (number > 0)
            {
                return "+";
            }
            else if (number < 0)
            {
                return "";
            }
            else
            {
                return "";
            }
        }

        static string GetColor(float number)
        {
            if (number > 0)
            {
                return "green";
            }
            else if (number < 0)
            {
                return "red";
            }
            else
            {
                return "yellow";
            }
        }



        static string ValidDigit(float f, int digit)
        {
            if (digit <= 0)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            string e = string.Format("{0:e" + (digit - 1) + "}", f);
            float fd = float.Parse(e);
            return fd.ToString();
        }

        static float GetKMGNumber(int number)
        {
            int abs = Mathf.Abs(number);
            if (abs >= 1000000)
            {
                return (number / 1000000.0f);
            }
            else if (abs >= 1000)
            {
                return (number / 1000.0f);
            }
            else
            {
                return number;
            }
        }

        static string GetKMGSuffix(int number)
        {
            int abs = Mathf.Abs(number);

            if (abs >= 1000000)
            {
                return "M";
            }
            else if (abs >= 1000)
            {
                return "K";
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region UIColor
        public static string Nation(string name)
        {
            return string.Format("<color=yellow>{0}</color>", name);
        }

        public static string Castle(string name)
        {
            return string.Format("<color=green>{0}</color>", name);
        }

        public static string Yellow(string name)
        {
            return string.Format("<color=yellow>{0}</color>", name);
        }

        public static string Red(string name)
        {
            return string.Format("<color=red>{0}</color>", name);
        }

        public static string Green(string name)
        {
            return string.Format("<color=green>{0}</color>", name);
        }

        public static string Religion(string name)
        {
            return string.Format("<color=yellow>{0}</color>", name);
        }

        public static string TradeRes(string name)
        {
            return string.Format("<color=grey>{0}</color>", name);
        }

        public static Color FromHex(string str)
        {
            Color color = new Color();
            ColorUtility.TryParseHtmlString(str, out color);
            return color;
        }
        public static string ColorToString(Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
        }

        public static Color32 StringToColor(string colorString)
        {
            int num = int.Parse(colorString, NumberStyles.HexNumber);

            Color32 result;

            if (colorString.Length == 8)
            {
                result = new Color32((byte)(num >> 24 & 255), (byte)(num >> 16 & 255), (byte)(num >> 8 & 255), (byte)(num & 255));
            }
            else
            {
                if (colorString.Length == 6)
                {
                    result = new Color32((byte)(num >> 16 & 255), (byte)(num >> 8 & 255), (byte)(num & 255), 255);
                }
                else
                {
                    if (colorString.Length == 4)
                    {
                        result = new Color32((byte)((num >> 12 & 15) * 17), (byte)((num >> 8 & 15) * 17), (byte)((num >> 4 & 15) * 17), (byte)((num & 15) * 17));
                    }
                    else
                    {
                        if (colorString.Length != 3)
                        {
                            throw new FormatException("Support only RRGGBBAA, RRGGBB, RGBA, RGB formats");
                        }
                        result = new Color32((byte)((num >> 8 & 15) * 17), (byte)((num >> 4 & 15) * 17), (byte)((num & 15) * 17), 255);
                    }
                }
            }
            return result;
        }
        #endregion

        #region UIFormat
        /// <summary>
        /// 条件
        /// </summary>
        /// <param name="isTrue"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Condition(bool isTrue,string str)
        {
            if (isTrue)
                return BaseConstMgr.STR_Indent + "<Color=green>" + str + "</Color>";
            return BaseConstMgr.STR_Indent + "<Color=red>" + str + "</Color>";
        }
        /// <summary>
        /// 分数
        /// </summary>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        public static string Rational(string numerator, string denominator)
        {
            return string.Format("{0}/{1}", numerator, denominator);
        }

        /// <summary>
        /// 标题内容
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string TitleContent(string title, string content)
        {
            return string.Format("{0}\n------------------------\n{1}", title, content);
        }

        /// <summary>
        /// 短日期
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string DateTimeShort(DateTime date)
        {
            return date.ToString("M月d日 HH:mm");
        }

        /// <summary>
        /// 日期
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static string TimeSpan(TimeSpan span)
        {
            int totalHours = span.Days * 24 + span.Hours;
            if (totalHours > 0)
            {
                return string.Format("{1}{0}{2}{0}{3}{0}{4}", BaseLanguageMgr.Space, totalHours, BaseLanguageMgr.Get("Unit_小时"), span.Minutes, BaseLanguageMgr.Get("Unit_分钟"));
            }
            else
            {
                return string.Format("{1}{0}{2}", BaseLanguageMgr.Space, span.Minutes, BaseLanguageMgr.Get("Unit_分钟"));
            }
        }

        /// <summary>
        /// buff后缀
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string BuffSuffix(string str)
        {
            return string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, BaseLanguageMgr.Get("Unit_Buff"));
        }


        /// <summary>
        /// Attr name 后缀
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AttrTypeNameSuffix(string str, Enum type)
        {
            return string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, type.GetName());
        }

        /// <summary>
        /// 天后缀
        /// </summary>
        /// <returns></returns>
        public static string DaySuffix(string str)
        {
            return string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, BaseLanguageMgr.Get("Unit_天"));
        }

        /// <summary>
        /// 天后缀
        /// </summary>
        /// <returns></returns>
        public static string MonthSuffix(string str)
        {
            return string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, BaseLanguageMgr.Get("Unit_月"));
        }

        /// <summary>
        /// 天后缀
        /// </summary>
        /// <returns></returns>
        public static string YearSuffix(string str)
        {
            return string.Format("{0}{1}{2}", str, BaseLanguageMgr.Space, BaseLanguageMgr.Get("Unit_年"));
        }
        #endregion

        #region other
        public static string GetStr(string key,params object[] ps)
        {
            return BaseLanguageMgr.Get(key,ps);
        }
        public static string GetPath(GameObject go)
        {
            return go.transform.parent == null ? "/" + go.name : GetPath(go.transform.parent.gameObject) + "/" + go.name;
        }
        /// <summary>
        /// Finds the component in the game object's parents.
        /// </summary>
        /// <returns>The component.</returns>
        /// <param name="go">Game Object.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null)
                return null;

            var comp = go.GetComponent<T>();

            if (comp != null)
                return comp;

            Transform t = go.transform.parent;

            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }

            return comp;
        }
        #endregion

        #region presenter
        public static void ResetRectTransform(RectTransform rectTransform)
        {
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        #endregion
    }

}