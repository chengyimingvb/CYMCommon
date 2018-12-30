//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System.Collections.Generic;
using Rewired;
using UnityEngine;
using static Rewired.Player;
using static Rewired.Player.ControllerHelper;

namespace CYM
{
    public enum InputBntType
    {
        Normal,
        Up,
        Down,
        DoublePressHold,
        DoublePressDown,
        DoublePressUp,
    }
    public enum InputAxisType
    {
        Normal,
        Raw,
        DoubleClick,
    }

    public class BaseInputMgr : BaseGlobalCoreMgr
    {
        #region readonly
        private const string category = "Default";
        private const string layout = "Default";
        #endregion

        #region Callback
        public event Callback Callback_OnInputMapChanged;
        public event Callback<InputMapper.InputMappedEventData> Callback_OnInputMapped;
        public event Callback<InputMapper.ConflictFoundEventData> Callback_OnConflictFound;
        #endregion

        #region prop
        public static readonly string UIHorizontal = "UIHorizontal";
        public static readonly string UIVertical = "UIVertical";
        public static readonly string UISubmit = "UISubmit";
        public static readonly string UICancel = "UICancel";

        public static readonly string UIPgUp = "UIPgUp";
        public static readonly string UIPgDn = "UIPgDn";

        public ControllerType CurControllerType { get; set; } = ControllerType.Keyboard;
        public Player Player { get; private set; }
        public List<InputAction> Actions { private set; get; }
        public MapHelper Map { get { return Player.controllers.maps; } }
        public ControllerHelper Controller { get { return Player.controllers; } }
        public InputMapper InputMaper { get { return InputMapper.Default; } }
        private InputMapper.ConflictFoundEventData conflictData;

        /// <summary>
        /// Keyboard 缓存
        /// </summary>
        private Dictionary<string, ActionElementMap> CacheKeyboardAEMName_Positive = new Dictionary<string, ActionElementMap>();
        private Dictionary<string, ActionElementMap> CacheKeyboardAEMName_Negative = new Dictionary<string, ActionElementMap>();

        protected BoolState IsDisablePlayerInput { get; set; } = new BoolState();

        protected Dictionary<string, IndexState> AxisCacheCount = new Dictionary<string, IndexState>();
        protected Timer AxisDoubleClickTimer = new Timer(0.15f);
        protected Timer AxisDoubleClickClearTimer = new Timer(0.15f);
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            Player = ReInput.players.GetPlayer(0);
            Actions = new List<InputAction>( ReInput.mapping.Actions);
            InputMaper.options.timeout = int.MaxValue;
            InputMaper.options.ignoreMouseXAxis = true;
            InputMaper.options.ignoreMouseYAxis = true;

            InputMaper.RemoveAllEventListeners();
            InputMaper.InputMappedEvent += OnInputMapped;
            InputMaper.ConflictFoundEvent += OnConflictFound;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            LoadData();
            CalcActionKeyboardElementMap();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (IsCanInput())
            {
                UpdateInput();
            }
            if (AxisDoubleClickTimer.CheckOverOnce())
            {
                foreach (var item in AxisCacheCount)
                {
                    item.Value.Reset();
                }
            }
        }
        protected virtual void UpdateInput()
        {

        }
        public override void OnDestroy()
        {
            InputMaper.RemoveAllEventListeners();
            base.OnDestroy();
        }
        #endregion

        #region 获得翻译
        /// <summary>
        /// 获得翻译后的Action name
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetActionName(InputAction data)
        {
            return BaseLanguageMgr.Get($"{BaseConstMgr.Prefix_Shortcut}{data.name}");
        }
        /// <summary>
        /// 获得键盘的映射名称
        /// </summary>
        /// <returns></returns>
        public string GetKeyboardAEMName(string actionName, Pole axisContribution = Pole.Positive)
        {
            //return GetActionElementMap(actionName,  ControllerType.Keyboard,axisContribution).keyCode.ToString();
            Dictionary<string, ActionElementMap> maps = CacheKeyboardAEMName_Positive;
            if (axisContribution == Pole.Positive)
                maps = CacheKeyboardAEMName_Positive;
            else if (axisContribution == Pole.Negative)
                maps = CacheKeyboardAEMName_Negative;

            if (!maps.ContainsKey(actionName))
                return BaseConstMgr.STR_None;
            return maps[actionName].keyCode.ToString();
        }

        #endregion

        #region private
        /// <summary>
        /// 计算ActionKeyboardElementMap
        /// </summary>
        /// <param name="actid"></param>
        /// <param name="controllerType"></param>
        /// <param name="axisContribution"></param>
        /// <returns></returns>
        private void CalcActionKeyboardElementMap()
        {
            CacheKeyboardAEMName_Negative.Clear();
            CacheKeyboardAEMName_Positive.Clear();
            foreach (var actionItem in Actions)
            {
                List<ActionElementMap> el = GetActionElementMapsByAction(actionItem.name);
                foreach (var item in el)
                {
                    if (item.controllerMap.controllerType == ControllerType.Keyboard)
                    {
                        if (item.axisContribution == Pole.Negative)
                            CacheKeyboardAEMName_Negative.Add(actionItem.name, item);
                        else if (item.axisContribution == Pole.Positive)
                            CacheKeyboardAEMName_Positive.Add(actionItem.name, item);
                    }
                }

            }

        }
        #endregion

        #region get
        /// <summary>
        /// 获得action 的映射
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        public List<ActionElementMap> GetActionElementMapsByAction(string actionName)
        {
            List<ActionElementMap> el = new List<ActionElementMap>();

            el.AddRange(Controller.maps.ElementMapsWithAction(actionName, false));

            return el;
        }
        /// <summary>
        /// 获得action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public InputAction GetAction(string id)
        {
            return ReInput.mapping.GetAction(id);
        }
        /// <summary>
        /// 获得Element map
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ActionElementMap GetActionElementMap(string actid, ControllerType controllerType, Pole axisContribution)
        {
            List<ActionElementMap> el = new List<ActionElementMap>();

            el.AddRange(Controller.maps.ElementMapsWithAction(actid, false));

            foreach (var item in el)
            {
                if (item.controllerMap.controllerType == controllerType &&
                    item.axisContribution == axisContribution)
                    return item;
            }
            return null;
        }
        /// <summary>
        /// 获得map
        /// </summary>
        /// <param name="selectedControllerType"></param>
        /// <param name="selectedControllerId"></param>
        /// <returns></returns>
        public ControllerMap GetControllerMap(ControllerType selectedControllerType, int selectedControllerId = 0)
        {
            var controller = Player.controllers.GetController(selectedControllerType, selectedControllerId);

            if (controller == null) return null;
            return Player.controllers.maps.GetMap(controller.type, controller.id, category, layout);
        }
        /// <summary>
        /// 保存映射数据
        /// </summary>
        public void SaveData()
        {
            ReInput.userDataStore.Save();
            //RefreshActionElementMaps();
        }
        /// <summary>
        /// 加载映射数据
        /// </summary>
        public void LoadData()
        {
            ReInput.userDataStore.Load();
            //RefreshActionElementMaps();
        }
        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="actid"></param>
        /// <param name="controllerMap"></param>
        /// <param name="actionRange"></param>
        /// <param name="actionElementMap"></param>
        /// <returns></returns>
        public InputMapper.Context StartPoll(string actid, ControllerMap controllerMap, AxisRange actionRange, ControllerType controllerType, Pole axisContribution)
        {
            StopPoll();
            InputMapper.Context context = new InputMapper.Context()
            {
                actionName = actid,
                actionId = GetAction(actid).id, // the id of the Action being mapped
                controllerMap = controllerMap, // the Controller Map which will have the new mapping added
                actionRange = actionRange, // the range of the Action being mapped
                actionElementMapToReplace = GetActionElementMap(actid, controllerType, axisContribution), // the Action Element Map to be replaced (optional)
            };
            InputMaper.Start(context);
            return context;
        }
        /// <summary>
        /// 停止监听
        /// </summary>
        public void StopPoll()
        {
            InputMaper.Stop();
        }
        #endregion

        #region set
        protected void AddHideAction(string key)
        {
            Actions.RemoveAll((x)=>x.name==key);
        }
        /// <summary>
        /// 注册Axis
        /// </summary>
        /// <param name="key"></param>
        protected void RegisterAxis(string key)
        {
            if (!AxisCacheCount.ContainsKey(key + "0"))
                AxisCacheCount.Add(key + "0", new IndexState(3));
            if (!AxisCacheCount.ContainsKey(key + "-1"))
                AxisCacheCount.Add(key + "-1", new IndexState(3));
            if (!AxisCacheCount.ContainsKey(key + "1"))
                AxisCacheCount.Add(key + "1", new IndexState(3));
        }
        /// <summary>
        /// 检测按键
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool GetBnt(string key, InputBntType type = InputBntType.Down)
        {
            if (type == InputBntType.Normal)
                return Player.GetButton(key);
            else if (type == InputBntType.Up)
                return Player.GetButtonUp(key);
            else if (type == InputBntType.Down)
                return Player.GetButtonDown(key);
            else if (type == InputBntType.DoublePressHold)
                return Player.GetButtonDoublePressHold(key);
            else if (type == InputBntType.DoublePressDown)
                return Player.GetButtonDoublePressDown(key);
            else if (type == InputBntType.DoublePressUp)
                return Player.GetButtonDoublePressUp(key);
            return false;
        }
        float preAxisValue = 0;
        /// <summary>
        /// 获得轴的信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public float GetAxis(string key, InputAxisType type = InputAxisType.Normal)
        {
            if (type == InputAxisType.Normal)
                return Player.GetAxis(key);
            else if (type == InputAxisType.Raw)
                return Player.GetAxisRaw(key);
            else if (type == InputAxisType.DoubleClick)
            {
                var value = Player.GetAxisRaw(key);
                float time = 0.2f;

                if (value != 0.0f )
                {
                    string realKey = key + Mathf.RoundToInt(value);
                    IndexState index = AxisCacheCount[realKey];
                    preAxisValue = value;
                    int step = index.Cur();
                    if (step == 0)
                    {
                        index.Reset(1);
                        AxisDoubleClickTimer.Restart(time);
                        return 0.0f;
                    }
                    else if (step == 2 && !AxisDoubleClickTimer.IsOver()) 
                    {
                        AxisDoubleClickTimer.Restart();
                        return 1.0f;
                    }
                }
                else
                {
                    string realKey = key + Mathf.RoundToInt(preAxisValue);
                    IndexState index = AxisCacheCount[realKey];
                    int step = index.Cur();
                    if (step == 1)
                    {
                        index.Reset(2);
                        AxisDoubleClickClearTimer.Restart(time);
                        return 0.0f;
                    }
                    else if (step == 2 && AxisDoubleClickTimer.IsOver())
                    {
                        AxisDoubleClickTimer.Gone();
                        index.Reset(0);
                        return 0.0f;
                    }
                }
            }
            return 0.0f;
        }
        public bool IsAnyKey()
        {
            return Input.anyKey;
        }
        /// <summary>
        /// 还原快捷键设置
        /// </summary>
        public void Revert()
        {
            Player.controllers.maps.LoadDefaultMaps(ControllerType.Custom);
            Player.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
            Player.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
            Player.controllers.maps.LoadDefaultMaps(ControllerType.Mouse);
            Callback_OnInputMapChanged?.Invoke();
        }
        public virtual void EnablePlayerInput(bool b)
        {
            IsDisablePlayerInput.Push(!b);
        }
        public virtual void ResumePlayerInput()
        {
            IsDisablePlayerInput.Reset();
        }
        public bool GetMouseDown(int index)
        {
            return Input.GetMouseButtonDown(index);
        }
        public bool GetMouseUp(int index)
        {
            return Input.GetMouseButtonUp(index);
        }
        public bool GetMouse(int index)
        {
            return Input.GetMouseButton(index);
        }
        public bool GetKeyDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);
        }
        public bool GetKeyUp(KeyCode keyCode)
        {
            return Input.GetKeyUp(keyCode);
        }
        public bool GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }
        #endregion

        #region is
        public virtual bool IsCanInput()
        {
            if (SelfBaseGlobal == null)
                return false;
            if (SelfBaseGlobal.DevConsoleMgr.IsShow())
                return false;
            return true;
        }
        public virtual bool IsCanPlayerInput()
        {
            if (SelfBaseGlobal == null)
                return false;
            if (IsDisablePlayerInput.IsIn())
                return false;
            if (SelfBaseGlobal.DevConsoleMgr.IsShow())
                return false;
            if (SelfBaseGlobal.IsPause)
                return false;
            return true;
        }
        #endregion

        #region UI
        protected bool GetUICancle(InputBntType type = InputBntType.Down)
        {
            return GetBnt(UICancel, type);
        }

        protected bool GetUISubmit(InputBntType type = InputBntType.Down)
        {
            return GetBnt(UISubmit, type);
        }
        protected float GetUIVertical(InputAxisType type = InputAxisType.Normal)
        {
            return GetAxis(UIVertical, type);
        }
        protected float GetUIHorizontal(InputAxisType type = InputAxisType.Normal)
        {
            return GetAxis(UIHorizontal, type);
        }
        protected bool GetUIPgUp(InputBntType type = InputBntType.Down)
        {
            return GetBnt(UIPgUp, type);
        }
        protected bool GetUIPgDn(InputBntType type = InputBntType.Down)
        {
            return GetBnt(UIPgDn, type);
        }
        #endregion

        #region Callback
        private void OnInputMapped(InputMapper.InputMappedEventData obj)
        {
            //throw new NotImplementedException();
            Callback_OnInputMapped?.Invoke(obj);
            Callback_OnInputMapChanged?.Invoke();
        }
        private void OnConflictFound(InputMapper.ConflictFoundEventData data)
        {
            conflictData = data; // store the event data for use in user response

            if (data.isProtected)
            { // the conflicting assignment was protected and cannot be replaced
              // Display some message to the user asking whether to cancel or add the new assignment.
              // Protected assignments cannot be replaced.
              // ...
            }
            else
            {
                // Display some message to the user asking whether to cancel, replace the existing,
                // or add the new assignment.
                // ...
            }
            Callback_OnConflictFound?.Invoke(data);
        }
        #endregion
    }

}