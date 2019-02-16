//------------------------------------------------------------------------------
// BaseDateMgr.cs
// Copyright 2018 2018/11/10 
// Created by CYM on 2018/11/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;

namespace CYM
{
    public enum DateTimeAgeType
    {
        AD,
        BC,
    }
    public class BaseDateTimeMgr : BaseGFlowMgr
    {
        #region prop
        readonly string DateStrFormat = "{0} {1}.{2}.{3}";
        public DateTimeAgeType CurDateTimeAgeType { get; protected set; }
        public DateTimeAgeType StartDateTimeAgeType { get; protected set; }
        public DateTime CurDateTime { get; protected set; }
        public DateTime StartDateTime { get; protected set; }
        public DateTime EndDateTime { get; protected set; }
        int PreMonth { get; set; }
        int PreYear { get; set; }
        int PreDay { get; set; }
        #endregion

        #region Callback
        public event Callback Callback_OnDayChanged;
        public event Callback Callback_OnMonthChanged;
        public event Callback Callback_OnYearChanged;
        #endregion

        #region set
        public void AddDay(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddDays(1);
            CheckChange();
        }
        public void AddMonth(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddMonths(1);
            CheckChange();
        }
        public void AddYear(int val)
        {
            RecodePreDate();
            CurDateTime = CurDateTime.AddYears(1);
            CheckChange();
        }
        #endregion

        #region get
        public string GetCurDateString()
        {
            int curYear = 0;
            if (CurDateTimeAgeType == DateTimeAgeType.BC)
                curYear = (StartDateTime.Year - CurDateTime.Year + 1);
            else if (CurDateTimeAgeType == DateTimeAgeType.AD)
            {
                if (StartDateTimeAgeType == DateTimeAgeType.AD)
                    curYear = (StartDateTime.Year + CurDateTime.Year - 1);
                else
                    curYear = (CurDateTime.Year - StartDateTime.Year - 1);
            }
            curYear = Mathf.Clamp(curYear, 0, int.MaxValue);
            string dateTypeStr = "AD";
            if (CurDateTimeAgeType == DateTimeAgeType.BC)
                dateTypeStr = "BC";
            return string.Format(DateStrFormat, dateTypeStr, curYear, CurDateTime.Month, CurDateTime.Day);
        }
        #endregion

        #region utile
        void RecodePreDate()
        {
            PreMonth = CurDateTime.Month;
            PreYear = CurDateTime.Year;
            PreDay = CurDateTime.Day;
        }
        void CheckChange()
        {
            bool isDayChanged = CurDateTime.Day != PreDay;
            bool isMonthChanged = CurDateTime.Month != PreMonth;
            bool isYearChanged = CurDateTime.Year != PreYear;
            if (isDayChanged)Callback_OnDayChanged?.Invoke();
            if (isMonthChanged) Callback_OnMonthChanged?.Invoke();
            if (isYearChanged)
            {
                UpdateDateTimeType();
                Callback_OnYearChanged?.Invoke();
            }
        }
        void UpdateDateTimeType()
        {
            if (CurDateTimeAgeType == DateTimeAgeType.BC &&
                (StartDateTime.Year - CurDateTime.Year) <= 0)
            {
                CurDateTimeAgeType = DateTimeAgeType.AD;
            }
        }
        #endregion
    }
}