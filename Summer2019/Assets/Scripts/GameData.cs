using System.Collections.Generic;
using UnityEngine;

namespace Summer2019
{
    /// <summary> 難易度列挙体 </summary>
    public enum Difficulty
    {
        Easy = 0,
        Normal,
        Hard,

        Max,
    }

    /// <summary> シーン間データ共有用シングルトン </summary>
    public class GameData : SingletonMonoBehaviour<GameData>
    {

        /// <summary> スコアデータ </summary>
        public class ScoreData
        {
            /// <summary> スコアデータ </summary>
            public float Time { get; set; }
            public float Speed { get; set; }
        }

        /// <summary> プレイヤーごとのスコア </summary>
        public List<ScoreData> Scores { get; set; } = new List<ScoreData>();

        /// <summary> 難易度 </summary>
        public Difficulty Difficulty { get; set; } = Difficulty.Normal;

        // Start is called before the first frame update
        void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}
