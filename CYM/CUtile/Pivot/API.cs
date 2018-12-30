using UnityEngine;

namespace CYM.Utile
{
    public static class API
    {
        public const float Version = 1.34f;

        public enum Space
        {
            /// <summary>
            /// The pivot position is expressed in World space
            /// </summary>
            Global,

            /// <summary>
            /// The pivot position is expressed in Local space
            /// </summary>
            Local
        }

    
        /// <summary>
        /// Return the pivot position of the Transform object
        /// </summary>
        /// <param name="space">Specify if you want the pivot position in World or Local space</param>
        /// <returns>The pivot position</returns>
        public static Vector3 GetPivotPosition(this Transform self, Space space = Space.Global)
        {
            return (space == Space.Global) ? self.position : self.localPosition;
        }

        /// <summary>
        /// Returns if you are allowed to change the pivot position of 'target' Transform
        /// Please note that this function is quite slow, so do not call it per frame.
        /// </summary>
        /// <param name="target">The Transform object you want to edit</param>
        /// <returns>true if you can change its pivot position, false otherwise</returns>
        public static bool CanChangePivot(Transform target)
        {
            return CanChangePivotPrivate(target) == null;
        }

        /// <summary>
        /// Returns if you are allowed to change the pivot position of 'target' Transform
        /// Please note that this function is quite slow, so do not call it per frame.
        /// </summary>
        /// <param name="target">The Transform object you want to edit</param>
        /// <param name="reason">A message explaining why you cannot edit the pivot, null otherwise</param>
        /// <returns>true if you can change its pivot position, false otherwise</returns>
        public static bool CanChangePivot(Transform target, out string reason)
        {
            reason = CanChangePivotPrivate(target);
            return reason == null;
        }

        /// <summary>
        /// Returns if you are allowed to change at least one pivot position from 'targets' Transforms array
        /// Please note that this function is quite slow, so do not call it per frame.
        /// </summary>
        /// <param name="targets">An array containing multiple Transform objects you want to edit</param>
        /// <returns>true if you can change at least one pivot position, false otherwise</returns>
        public static bool CanChangeAtLeastOnePivot(Transform[] targets)
        {
            foreach (var t in targets)
                if (CanChangePivotPrivate(t) == null)
                    return true;
            return false;
        }


        static string CanChangePivotPrivate(Transform target)
        {
            if (target == null)
                return string.Format("The target Transform is null");

            if (target.childCount == 0)
                return string.Format("'{0}' is not a group, since it has no children", target.name);

            var renderer = target.GetComponent<Renderer>();
            if (renderer)
                return string.Format("Can't edit '{0}' because it has a '{1}' attached", target.name, renderer.GetType());

            if (target.GetComponent<Terrain>())
                return string.Format("Can't edit '{0}' because it has a Terrain attached", target.name);

            return null;
        }

        /// <summary>
        /// Change the pivot position of "target"
        /// </summary>
        /// <param name="target">The GameObject you want to modify</param>
        /// <param name="pivotPos">The new pivot position</param>
        /// <param name="space">Doesn't the new pivot position is expressed in World or Local space?</param>
        public static void SetPivot(Transform target, Vector3 pivotPos, Space space = Space.Global)
        {
            Debug.Assert(target, "Invalid target entity");
            if (target.GetPivotPosition(space) == pivotPos)
                 return;

            var children = new Transform[target.childCount];
            for (int i = 0; i < children.Length; ++i)
            {
                children[i] = target.GetChild(i);
            }

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(target, "Changed Pivot Position");
            if (children.Length > 0)
                UnityEditor.Undo.RecordObjects(children, "Changed Pivot Position");
#endif
            var oldToNewMatrix = target.localToWorldMatrix;

            for (int i = 0; i < children.Length; ++i)
            {
                children[i].SetParent(null);
            }

            if (space == Space.Global)
                target.position = pivotPos;
            else
                target.localPosition = pivotPos;

            for (int i = 0; i < children.Length; ++i)
            {
                children[i].SetParent(target);
            }


            // Deal with colliders
            var colliders = target.GetComponents<Collider>();
            if (colliders.Length > 0)
            {
                oldToNewMatrix = target.worldToLocalMatrix * oldToNewMatrix;
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObjects(colliders, "Changed Pivot Position");
#endif
                foreach (var collider in colliders)
                {
                    if (collider is BoxCollider)
                        ((BoxCollider)collider).center = oldToNewMatrix.MultiplyPoint(((BoxCollider)collider).center);
                    else if (collider is CapsuleCollider)
                        ((CapsuleCollider)collider).center = oldToNewMatrix.MultiplyPoint(((CapsuleCollider)collider).center);
                    else if (collider is SphereCollider)
                        ((SphereCollider)collider).center = oldToNewMatrix.MultiplyPoint(((SphereCollider)collider).center);
                }
            }
        }

    }
}