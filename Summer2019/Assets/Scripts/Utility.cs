using System.Linq;
using UnityEngine;

namespace Summer2019
{
    public static class Utility
    {
        /// <summary> 対象のVector2の特定要素だけ置き換えたVectorを返す関数 </summary>
        /// <param name="baseVector">対象のVector</param>
        /// <param name="x">nullの場合そのまま、それ以外の場合その数値で置き換える</param>
        /// <param name="y">nullの場合そのまま、それ以外の場合その数値で置き換える</param>
        /// <returns>置換後のVector2</returns>
        public static Vector2 SetVector2(this Vector2 baseVector, float? x = null, float? y = null)
            => new Vector2(x ?? baseVector.x, y ?? baseVector.y);

        /// <summary> 対象のVector3の特定要素だけ置き換えたVector3を返す関数 </summary>
        /// <param name="baseVector">対象のVector</param>
        /// <param name="x">nullの場合そのまま、それ以外の場合その数値で置き換える</param>
        /// <param name="y">nullの場合そのまま、それ以外の場合その数値で置き換える</param>
        /// <param name="z">nullの場合そのまま、それ以外の場合その数値で置き換える</param>
        /// <returns>置換後のVector3</returns>
        public static Vector3 SetVector3(this Vector3 baseVector, float? x = null, float? y = null, float? z = null)
            => new Vector3(x ?? baseVector.x, y ?? baseVector.y, z ?? baseVector.z);

        /// <summary> 指定したComponentを削除する </summary>
        /// <typeparam name="T">指定するComponent</typeparam>
        /// <param name="self">削除される対象の親</param>
        public static void RemoveComponent<T>(this Component self) where T : Component
            => Object.Destroy(self.GetComponent<T>());

        /// <summary> floatが0に近いかどうかの判定 </summary>
        /// <param name="value">判定数値</param>
        /// <param name="border">基準値</param>
        /// <returns>0に近いかどうか</returns>
        public static bool IsNearZero(this float value, float border = 0.001f) => Mathf.Abs(value) < border;

        public static bool GetKeyMultiple(params KeyCode[] keyCodes)
        {
            if (keyCodes.All(Input.GetKey))
                return true;
            return false;
        }

        public static bool GetKeyDownMultiple(params KeyCode[] keyCodes)
        {
            if (keyCodes.All(Input.GetKey) && keyCodes.Any(Input.GetKeyDown))
                return true;
            return false;
        }
    }
}
