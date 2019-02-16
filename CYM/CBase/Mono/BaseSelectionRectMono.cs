namespace CYM
{
    using UnityEngine;
    public class BaseSelectionRectMono : BaseCoreMono
    {
        public float startDeltaThreshold = 3.0f;

        private Camera _selectionVisualCamera;
        private Transform _selectionVisual;
        private Vector3 _lastSelectDownPos;
        Vector3 p1 = Vector3.zero;
        Vector3 p2 = Vector3.zero;
        private bool isSelecting = false;
        private bool isStartSelecting = false;
        private bool IgnoreFrameSelection = false;
        private Timer IgnoreFrameSelectionTimer = new Timer();
        public override void Awake()
        {
            base.Awake();
            _selectionVisualCamera = this.GetComponentInChildren<Camera>();
            _selectionVisual = this.GetComponentInChildren<MeshRenderer>().transform;

            ToggleEnabled(false);
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        protected virtual void StartSelect()
        {
            isStartSelecting = true;
            _lastSelectDownPos = Input.mousePosition;
        }
        protected virtual void RealStartSelect()
        {
            ToggleEnabled(true);
            if (Callback_OnStartSelect != null)
                Callback_OnStartSelect();
        }

        internal bool HasSelection(Vector3 startScreen, Vector3 endScreen)
        {
            if ((Mathf.Abs(startScreen.x - endScreen.x) < this.startDeltaThreshold) || (Mathf.Abs(startScreen.y - endScreen.y) < this.startDeltaThreshold))
            {
                return false;
            }

            DrawSelectionRect(startScreen, endScreen);

            return true;
        }

        protected virtual void EndSelect()
        {
            ToggleEnabled(false);
            if (Callback_OnEndSelect != null)
                Callback_OnEndSelect();
            if (Callback_OnChangeSelect!=null)
                Callback_OnChangeSelect();
        }

        private void ToggleEnabled(bool enabled)
        {
            _selectionVisualCamera.enabled = enabled;
            if (!enabled)
            {
                _selectionVisual.localScale = Vector3.zero;
            }
        }

        private void DrawSelectionRect(Vector3 startScreen, Vector3 endScreen)
        {
            var startWorld = _selectionVisualCamera.ScreenToWorldPoint(startScreen);
            var endWorld = _selectionVisualCamera.ScreenToWorldPoint(endScreen);

            var dx = endWorld.x - startWorld.x;
            var dy = endWorld.y - startWorld.y;

            _selectionVisual.position = new Vector3(
                startWorld.x + (dx / 2.0f),
                startWorld.y + (dy / 2.0f));

            _selectionVisual.localScale = new Vector3(Mathf.Abs(dx), Mathf.Abs(dy), 1.0f);
        }
        public void SetIgnoreFrameSelection()
        {
            IgnoreFrameSelection = true;
            IgnoreFrameSelectionTimer.Restart();
        }
        public override void OnUpdate()
        {
            if (IgnoreFrameSelectionTimer.Elapsed() > 0.01 && IgnoreFrameSelection)
            {
                IgnoreFrameSelection = false;
            }
            if (IgnoreFrameSelection)
                return;
            if (Input.GetMouseButtonDown(0))
            {
                StartSelect();
            }
            else if (Input.GetMouseButton(0) && isStartSelecting)
            {
                Vector3 selectRectPos = _lastSelectDownPos - Input.mousePosition;
                if (Mathf.Abs(selectRectPos.x) > 3.0f &&
                    Mathf.Abs(selectRectPos.y) > 3.0f)
                {
                    if(!IsSelecting)
                        RealStartSelect();
                    IsSelecting = true;
                }
                if (HasSelection(_lastSelectDownPos,Input.mousePosition) )
                {
                }
            }
            else if(Input.GetMouseButtonUp(0)&& IsSelecting)
            {
                EndSelect();
                IsSelecting = false;
            }
            if(Input.GetMouseButtonUp(0))
            {
                isStartSelecting = false;
            }
        }
        public bool IsSelected(GameObject go)
        {
            if (!IsSelecting)
                return false;
            p1 = Vector3.zero;
            p2 = Vector3.zero;
            if (_lastSelectDownPos.x > Input.mousePosition.x)
            {
                p1.x = Input.mousePosition.x;
                p2.x = _lastSelectDownPos.x;
            }
            else
            {
                p1.x = _lastSelectDownPos.x;
                p2.x = Input.mousePosition.x;
            }
            if (_lastSelectDownPos.y > Input.mousePosition.y)
            {
                p1.y = Input.mousePosition.y;
                p2.y = _lastSelectDownPos.y;
            }
            else
            {
                p1.y = _lastSelectDownPos.y;
                p2.y = Input.mousePosition.y;
            }
            Vector3 location = Camera.main.WorldToScreenPoint(go.transform.position);
            if (location.x < p1.x || location.x > p2.x || location.y < p1.y || location.y > p2.y
                || location.z < Camera.main.nearClipPlane || location.z > Camera.main.farClipPlane)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public Callback Callback_OnStartSelect { get; set; }
        public Callback Callback_OnEndSelect { get; set; }
        public Callback Callback_OnChangeSelect { get; set; }

        protected bool IsSelecting
        {
            get
            {
                return isSelecting;
            }

            set
            {
                isSelecting = value;
            }
        }
    }
}
