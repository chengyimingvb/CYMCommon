using UnityEngine;
namespace CYM
{
    public class EffectFollow : BaseMono
    {
        private Transform _owner;
        bool _isActive;
        public Vector3 offset = Vector3.zero;

        //public override void OnSetNeedFlag()
        //{
        //    base.OnSetNeedFlag();
        //    NeedUpdate = true;
        //}
        //public override void OnUpdate()
        //{

        //}
        private void Update()
        {
            if (_owner != null && _isActive)
            {
                Pos = _owner.position + offset;
            }
        }

        public void SetFollowObj(Transform unit, Vector3 offset)
        {
            _owner = unit;
            this.offset = offset;
            if (_owner != null)
            {
                Pos = _owner.position + offset;
                _isActive = true;
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _isActive = false;
            offset = Vector3.zero;

        }

        public override void OnDisable()
        {
            _isActive = false;
            offset = Vector3.zero;
        }
    }


}
