using System;
using UnityEngine;
using CYM;
using System.Collections.Generic;
using MoonSharp.Interpreter;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class NarrationFragment : TDValue
    {
        /// <summary>
        /// 内容,如果为none 则更具翻译索引规则显示
        /// </summary>
        public string Desc { get; set; } = BaseConstMgr.STR_None;
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; } = BaseConstMgr.STR_None;
        /// <summary>
        /// 换页
        /// </summary>
        public bool IsNewPage { get; set; } = true;

        #region prop
        public int CurPage = 0;
        public int Index = 0;
        public string NarrationId;
        #endregion

        #region get
        public virtual string GetName()
        {
            return BaseLanguageMgr.Get(NarrationId);
        }
        public virtual string GetDesc(params object[] ps)
        {
            if (!Desc.IsInvStr())
                return BaseLanguageMgr.Get(Desc, ps);
            return BaseLanguageMgr.Get(NarrationId + "_" + Index, ps);
        }
        public virtual Sprite GetIcon()
        {
            if (!Icon.IsInvStr())
                return BaseGlobal.Ins.GRMgr.GetIcon(Icon);
            return BaseGlobal.Ins.GRMgr.GetIcon(NarrationId + "_" + Index, false);
        }
        #endregion
    }

    public class TDBaseNarrationData : BaseConfig<TDBaseNarrationData>
    {
        #region prop
        public List<NarrationFragment> Fragments { get; set; } = new List<NarrationFragment>();
        #endregion

        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            int index = 0;
            foreach (var item in Fragments)
            {
                item.Index = index;
                item.NarrationId = TDID;
                if (index == 0)
                    item.IsNewPage = true;
                if (item.IsNewPage&& index>0)
                {
                    item.CurPage++;
                }
                index++;
            }
        }
    }
}