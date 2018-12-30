//------------------------------------------------------------------------------
// BaseExcelUtils.cs
// Copyright 2018 2018/9/27 
// Created by CYM on 2018/9/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using System;
using System.Reflection;
using Excel;
using System.Data;
//using NUnit.Framework;

namespace CYM
{
    public class BaseExcelUtils 
    {
        #region excel
        public static DataSet ReadExcel(string filePath)
        {
            IExcelDataReader excelReader = null;
            using (FileStream MyAddress_Read = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(MyAddress_Read);
            }           

            DataSet Result = excelReader.AsDataSet();

            return Result;
        }
        public static HSSFWorkbook ReadExcelNPOI(string filePath)
        {
            HSSFWorkbook MyBook;

            using (FileStream MyAddress_Read = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                MyBook = new HSSFWorkbook(MyAddress_Read);
            }
            return MyBook;
        }
        public static List<T> ReadExcelNPOI<T>(string filePath) where T:new()
        {
            List<T> ret = new List<T>();
            HSSFWorkbook dataSet;
            using (FileStream MyAddress_Read = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                dataSet = new HSSFWorkbook(MyAddress_Read);
            }

            if (dataSet == null)
            {
                CLog.Error("无法读取下面文件{0}:", filePath);
                return ret;
            }

            Dictionary<string, PropertyInfo> propertyInfos = new Dictionary<string, PropertyInfo>();
            List<string> firstRow = new List<string>();
            Type type = typeof(T);
            var temppropertyInfos = type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance);
            if (temppropertyInfos != null)
            {
                foreach (var item in temppropertyInfos)
                {
                    propertyInfos.Add(item.Name, item);
                }
            }

            for (int i = 0; i < dataSet.NumberOfSheets; ++i)
            {
                ISheet sheet = dataSet.GetSheetAt(i);
                var collect = sheet.GetRowEnumerator();
                //读取每一行
                while (collect.MoveNext())
                {
                    //读取第一行
                    IRow row = (IRow)collect.Current;
                    if (row.RowNum == 0)
                    {
                        foreach (var cell in row.Cells)
                        {
                            if (cell.ColumnIndex == 0)
                                continue;
                            else
                            {
                                try
                                {
                                    firstRow.Add(cell.ToString());
                                }
                                catch (Exception e)
                                {
                                    CLog.Error("解析Excel表格错误:" + e.ToString());
                                }
                            }
                        }
                    }
                    //读取内容行
                    else
                    {
                        //读取每一列
                        T temp = new T();
                        for (int cellIndex = 0; cellIndex < firstRow.Count; ++cellIndex)
                        {
                            if (row.Cells.Count <= cellIndex)
                                continue;
                            var cell = row.Cells[cellIndex];
                            string propName = firstRow[cellIndex];
                            if (!propertyInfos.ContainsKey(propName))
                            {
                                CLog.Error("没有这个属性:{0}", propName);
                                continue;
                            }
                            PropertyInfo property = propertyInfos[propName];
                            string cellStr = cell.ToString();
                            property.SetValue(temp, cellStr, null);
                        }
                        ret.Add(temp);
                    }
                    
                }
            }

            return ret;
        }
        #endregion

        #region utile
        static object ConvertVal(PropertyInfo info, string str)
        {
            if (info.DeclaringType == typeof(string))
                return str;
            else if (info.DeclaringType == typeof(int))
                return Convert.ToInt32(str);
            else if (info.DeclaringType == typeof(float))
                return Convert.ToDecimal(str);
            return str;
        }
        #endregion
    }
}