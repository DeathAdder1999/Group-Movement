using UnityEngine;

namespace Extensions
{
    public static class UnityExtensions
    {
        public static Transform[] GetChildren(this Transform transform)
        {
            var count = transform.childCount;
            var children = new Transform[count];

            for (var i = 0; i < count; i++)
            {
                children[i] = transform.GetChild(i);
            }

            return children;
        }

        public static void LookAt2D(this Transform transform, Vector3 target)
        {
            var dir = target - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public static GameObject GetTopParent(this GameObject gameObject)
        {
            return gameObject.transform.root.gameObject;
        }
    }
}
