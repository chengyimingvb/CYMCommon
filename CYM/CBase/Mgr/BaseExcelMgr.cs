//------------------------------------------------------------------------------
// BaseExcelMgr.cs
// Copyright 2019 2019/1/30 
// Created by CYM on 2019/1/30
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Data;
using NPOI.HSSF.UserModel;
using System.IO;
using Excel;
using System.Collections.Generic;
using System.Reflection;
using System;
using NPOI.SS.UserModel;

namespace CYM
{
    public class BaseExcelMgr : BaseGFlowMgr
    {
        #region static excel
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
        #endregion
    }
}