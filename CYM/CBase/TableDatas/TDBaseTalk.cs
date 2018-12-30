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
    public enum TalkType
    {
        Left,
        Right,
        Mid,
    }

    public class TalkFragment : TDValue
    {
        /// <summary>
        /// 对话的方向
        /// </summary>
        public TalkType Type { get; set; } = TalkType.Left;
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; } = BaseConstMgr.STR_None;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = BaseConstMgr.STR_None;
        /// <summary>
        /// 内容,如果为none 则更具翻译索引规则显示
        /// </summary>
        public string Desc { get; set; } = BaseConstMgr.STR_None;
        /// <summary>
        /// 语音
        /// </summary>
        public string Audio { get; set; } = BaseConstMgr.STR_None;

        #region Prop
        public int Index = 0;
        public string TalkId;
        public bool IsLasted { get; set; } = false;
        public string TalkDescId { get; set; }
        #endregion

        public virtual string GetName()
        {
            return BaseLanguageMgr.Get(Name);
        }
        public virtual string GetDesc(params object[] ps)
        {
            if (!Desc.IsInvStr())
                return BaseLanguageMgr.Get(Desc, ps);
            return BaseLanguageMgr.Get(TalkDescId, ps);
        }
        public virtual Sprite GetIcon()
        {
            if(!Icon.IsInvStr())
                return BaseGlobal.Ins.GRMgr.GetIcon(Icon);
            return BaseGlobal.Ins.GRMgr.GetIcon(Name);
        }
    }

    public class TDBaseTalkData : BaseConfig<TDBaseTalkData>
    {
        public List<TalkFragment> Fragments { get; set; } = new List<TalkFragment>();

        /// <summary>
        /// 选项
        /// </summary>
        public List<string> Option { get; set; } = new List<string>();

        /// <summary>
        /// 是否有选项
        /// </summary>
        /// <returns></returns>
        public bool IsHaveOption()
        {
            if (Option == null)
                return false;
            if (Option.Count == 0)
                return false;
            return true;
        }

        /// <summary>
        /// 获得Option
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetOption(int index)
        {
            if (Option.Count <=0)
                return BaseConstMgr.STR_Inv;
            if (index < 0)
                return Option[0];
            if (index>=Option.Count)
                return Option[Option.Count-1];
            return Option[index];
        }
        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            int index = 0;
            TalkFragment Lasted = null;
            foreach (var item in Fragments)
            {
                item.Index = index;
                index++;
                item.TalkId = TDID;
                Lasted = item;
                item.TalkDescId = item.TalkId + "_" + item.Index;
            }
            if (Lasted!=null)
            {
                Lasted.IsLasted = true;
            }
            //获取Op
            for (int i = 0; i < BaseConstMgr.MaxTalkOptionCount; i++)
            {
                string opKey = TDID + BaseConstMgr.Suffix_Op + "_" + i;
                if (BaseLanguageMgr.IsContain(opKey))
                {
                    Option.Add(opKey);
                }
            }
        }
    }

}