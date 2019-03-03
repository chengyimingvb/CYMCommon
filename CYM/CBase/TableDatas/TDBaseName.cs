//------------------------------------------------------------------------------
// TDBaseName.cs
// Copyright 2019 2019/2/18 
// Created by CYM on 2019/2/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;

namespace CYM
{
    public class TDBaseNameData : BaseConfig<TDBaseNameData>
    {
        #region prop
        public List<string> Last { get; set; } = new List<string>();
        public List<string> Female { get; set; } = new List<string>();
        public List<string> Male { get; set; } = new List<string>();
        #endregion

        #region get
        public string GetMale(bool isTrans=false)
        {
            string ret = BaseMathUtils.RandArray(Last) + BaseLanguageMgr.Space + BaseMathUtils.RandArray(Male);
            if(isTrans)
                return BaseLanguageMgr.Get(ret);
            return ret;
        }
        public string GetFemale(bool isTrans = false)
        {
            string ret = BaseMathUtils.RandArray(Last) + BaseLanguageMgr.Space + BaseMathUtils.RandArray(Female);
            if (isTrans)
                return BaseLanguageMgr.Get(ret);
            return ret;
        }
        #endregion

        #region utile
        public void ExportNameLocationData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID",typeof(string));
            dt.Columns.Add("中文", typeof(string));
            dt.Columns.Add("英文", typeof(string));
            dt.Columns.Add("繁体", typeof(string));
            dt.Columns.Add("日语", typeof(string));
            dt.Columns.Add("西语", typeof(string));
            dt.Columns.Add("文言文", typeof(string));
            dt.Rows.Add(">>姓氏>>>>>>>>>>>>>");
            foreach (var item in Last)
            {
               dt.Rows.Add(
                   BaseConstMgr.Prefix_SurnameTrans+item, 
                   item, 
                   BaseStrUtils.ToPinying(item), 
                   BaseStrUtils.ToTraditional(item),
                   item,
                   item,
                   item
                   );
            }

            dt.Rows.Add(">>女名>>>>>>>>>>>>>");
            foreach (var item in Female)
            {
                dt.Rows.Add(
                    BaseConstMgr.Prefix_NameTrans+item,
                    item,
                    BaseStrUtils.ToPinying(item),
                    BaseStrUtils.ToTraditional(item),
                    item,
                    item,
                    item
                    );
            }

            dt.Rows.Add(">>男名>>>>>>>>>>>>>");
            foreach (var item in Male)
            {
                dt.Rows.Add(
                    BaseConstMgr.Prefix_NameTrans+item,
                    item,
                    BaseStrUtils.ToPinying(item),
                    BaseStrUtils.ToTraditional(item),
                    item,
                    item,
                    item
                    );
            }

            BaseExcelMgr.WriteExcelNPOI(dt,Path.Combine(BaseConstMgr.Path_Dev,TDID+".xls"));
        }
        #endregion
    }
}