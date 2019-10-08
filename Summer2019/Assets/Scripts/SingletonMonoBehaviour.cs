using UnityEngine;

namespace Summer2019
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                    if (_instance == null)
                        Debug.LogError(typeof(T) + "is nothing");
                }
                return _instance;
            }
        }

        virtual protected void Awake()
        {
            // 他のゲームオブジェクトにアタッチされているか調べる
            // アタッチされている場合は破棄する。
            CheckInstance();
        }

        /// <summary> 自分が唯一の存在かの確認 </summary>
        protected bool CheckInstance()
        {
            if (Instance == this)
                return true;
            if (_instance == null)
            {
                _instance = this as T;
                return true;
            }
            Destroy(this);
            return false;
        }
    }
}
