using UnityEngine;

namespace FoW
{
    [System.Serializable]
    public abstract class FogOfWarDrawer
    {
        protected FogOfWarMap _map;
        protected bool _isMultithreaded { get; private set; }

        public virtual void Initialise(FogOfWarMap map)
        {
            _map = map;
            OnInitialise();
        }

        protected virtual void OnInitialise() { }
        public abstract void Clear(byte value);
        public abstract void Fade(byte[] currentvalues, byte[] totalvalues, float partialfogamount, int amount);
        public abstract void GetValues(byte[] outvalues);
        public abstract void SetValues(byte[] values);

        protected abstract void DrawCircle(FogOfWarShapeCircle shape);
        protected abstract void DrawBox(FogOfWarShapeBox shape);
        public abstract void SetFog(Rect rect, byte value);

        public void Draw(FogOfWarShape shape, bool ismultithreaded)
        {
            _isMultithreaded = ismultithreaded;

            if (shape is FogOfWarShapeCircle)
                DrawCircle(shape as FogOfWarShapeCircle);
            else if (shape is FogOfWarShapeBox)
                DrawBox(shape as FogOfWarShapeBox);
        }
    }
}
