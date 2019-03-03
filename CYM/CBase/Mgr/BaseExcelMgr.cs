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
        #region set
        #endregion

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
        public static bool WriteExcelNPOI(DataTable dt, string filePath)
        {
            bool result = false;
            IWorkbook workbook = null;
            FileStream fs = null;
            IRow row = null;
            ISheet sheet = null;
            ICell cell = null;
            try
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    workbook = new HSSFWorkbook();
                    sheet = workbook.CreateSheet("Sheet0");//创建一个名称为Sheet0的表  
                    int rowCount = dt.Rows.Count;//行数  
                    int columnCount = dt.Columns.Count;//列数  

                    //设置列头  
                    row = sheet.CreateRow(0);//excel第一行设为列头  
                    for (int c = 0; c < columnCount; c++)
                    {
                        cell = row.CreateCell(c);
                        cell.SetCellValue(dt.Columns[c].ColumnName);
                    }

                    //设置每行每列的单元格,  
                    for (int i = 0; i < rowCount; i++)
                    {
                        row = sheet.CreateRow(i + 1);
                        for (int j = 0; j < columnCount; j++)
                        {
                            cell = row.CreateCell(j);//excel第二行开始写入数据  
                            cell.SetCellValue(dt.Rows[i][j].ToString());
                        }
                    }
                    if (!Directory.Exists(filePath))
                    {
                        File.Create(filePath).Dispose();
                    }
                    using (fs = File.OpenWrite(filePath))
                    {
                        workbook.Write(fs);//向打开的这个xls文件中写入数据  
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                CLog.Error(ex.ToString());
                if (fs != null)
                {
                    fs.Close();
                }
                return false;
            }
        }
        #endregion
    }
}