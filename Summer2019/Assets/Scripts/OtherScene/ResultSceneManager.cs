using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Summer2019
{
    public class ResultSceneManager : MonoBehaviour
    {
        [System.Serializable]
        /// <summary> 表示版のまとめクラス </summary>
        public class ScorePlates
        {
            private RectTransform rectTime = null;
            /// <summary> 時間表示背面画像 </summary>
            public RectTransform RectTime
            {
                get
                {
                    if (rectTime == null)
                        rectTime = _textTime.transform.parent.GetComponent<RectTransform>();
                    return rectTime;
                }
            }

            [SerializeField]
            private Text _textTime;
            /// <summary> 時間表示テキスト </summary>
            public Text TextTime => _textTime;

            private RectTransform rectSpeed = null;
            /// <summary> 速度表示背面画像 </summary>
            public RectTransform RectSpeed
            {
                get
                {
                    if (rectSpeed == null)
                        rectSpeed = _textSpeed.transform.parent.GetComponent<RectTransform>();
                    return rectSpeed;
                }
            }

            [SerializeField]
            private Text _textSpeed;
            /// <summary> 速度表示テキスト </summary>
            public Text TextSpeed => _textSpeed;

            private RectTransform rectScore = null;
            /// <summary> スコア表示背面画像 </summary>
            public RectTransform RectScore
            {
                get
                {
                    if (rectScore == null)
                        rectScore = _textScore.transform.parent.GetComponent<RectTransform>();
                    return rectScore;
                }
            }

            [SerializeField]
            private Text _textSpeedStage;
            /// <summary> 速度段階テキスト </summary>
            public Text TextSpeedStage => _textSpeedStage;

            [SerializeField]
            private Text _textScore;
            /// <summary> スコア表示テキスト </summary>
            public Text TextScore => _textScore;

            /// <summary> Text群をコンテナ型のように扱うためのプロパティ </summary>
            public IEnumerable<Text> Texts
            {
                get
                {
                    yield return TextTime;
                    yield return TextSpeed;
                    yield return TextScore;
                }
            }
            /// <summary> RectTransform群をコンテナ型のように扱うためのプロパティ </summary>
            public IEnumerable<RectTransform> Rects
            {
                get
                {
                    yield return RectTime;
                    yield return RectSpeed;
                    yield return RectScore;
                }
            }

            /// <summary> 移動目標位置 </summary>
            public float TargetPosX { get; set; }

            /// <summary> 到達時間 </summary>
            public float Time { get; set; }
            /// <summary> 最終速度 </summary>
            public float Speed { get; set; }
            /// <summary> 到達時間によるスコア </summary>
            public int TimeScore => (int)(Mathf.Tan(20 / Time) * 10000);
            /// <summary> 最終速度の段階 </summary>
            public int SpeedStage
            {
                get
                {
                    if (Speed < 30) return 0;
                    if (Speed < 40) return 1;
                    if (Speed < 60) return 2;
                    return 3;
                }
            }
            /// <summary> 最終速度の段階に対応する補正値 </summary>
            public float SpeedScore
            {
                get
                {
                    switch (SpeedStage)
                    {
                        case 0:
                            return 1.2f;
                        case 1:
                            return 1f;
                        case 2:
                            return 1 - Speed * Speed / 10000;
                        case 3:
                            return -Speed * Speed / 10000;
                        default:
                            throw new System.Exception("Out of range Speed.");
                    }
                }
            }
            /// <summary> 最終速度の段階に対応する文字列 </summary>
            public string SpeedStageString
            {
                get
                {
                    switch (SpeedStage)
                    {
                        case 0:
                            return "着地成功！";
                        case 1:
                            return "捻挫！";
                        case 2:
                            return "骨折！";
                        case 3:
                            return "重体！";
                        default:
                            throw new System.Exception("Out of range Speed.");
                    }
                }
            }
            /// <summary> 最終スコア </summary>
            public int Score => (int)(TimeScore * SpeedScore);
        }

        [SerializeField]
        /// <summary> 音声を再生するためのクラス </summary>
        private AudioSource _audioSource;
        [SerializeField]
        /// <summary> BGMクリップ </summary>
        private AudioClip _bgmClip;

        [SerializeField]
        /// <summary> 1p2p個別の表示版クラス </summary>
        private List<ScorePlates> _plates;
        [SerializeField]
        /// <summary> 表示版それぞれが移動にかかる時間 </summary>
        private float _plateMoveTime = 0.5f;
        [SerializeField]
        /// <summary> スコアのカウントアップにかかる時間 </summary>
        private float _countUpScoreTime = 0.5f;
        [SerializeField]
        /// <summary> アニメーション処理それぞれの間隔時間 </summary>
        private float _animationInterval = 0.2f;
        [SerializeField]
        /// <summary> 最終速度の段階ごとの表示文字色 </summary>
        private Color[] _speedStageTextColors;

        /// <summary> アニメーション処理中かどうか、処理中なら操作を受け付けない </summary>
        public bool IsAnimation { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            // null check
            if (_plates.Count != 2)
                Debug.LogError($"_plates size is not 2.");
            var inspectorItems = _plates.SelectMany(p => new[] { p.TextTime, p.TextSpeed, p.TextScore })
                .Union(_plates.Select(p => p.TextSpeedStage));
            if (inspectorItems.Any(o => o == null))
                Debug.LogError($"Not set inspector {inspectorItems.First(o => o == null)}.");

            // アニメーションは操作を受け付けないようにフラグを立てる
            IsAnimation = true;
            // BGM再生処理
            _audioSource.loop = true;
            _audioSource.clip = _bgmClip;
            _audioSource.Play();
            // 板の目標位置保存
            _plates.ForEach(p => p.TargetPosX = p.RectTime.localPosition.x);
            // 板の初期位置設定
            _plates.Select((p, i) => p.Rects.Select(r => new { Rect = r, Flag = i == 0 }))
                .SelectMany(x => x).ToList()
                .ForEach(x => x.Rect.localPosition = x.Rect.localPosition.SetVector3(x: x.Rect.localPosition.x +
                    x.Rect.sizeDelta.x * (x.Flag ? -1 : 1) * 1.2f));
            // 到達時間と最終速度をプロパティに入力することでスコアを計算できるように
            GameData.Instance.Scores.Zip(_plates, (Score, Plate) => new { Score, Plate }).ToList().ForEach(x =>
            {
                x.Plate.Time = x.Score.Time;
                x.Plate.Speed = x.Score.Speed;
            });
            // 表示設定
            _plates.ForEach(p =>
            {
                // 到達時間と最終速度をTextに反映
                p.Texts.Zip(new[] { p.Time, p.Speed }, (Text, Data) => new { Text, Data })
                    .ToList().ForEach(y => y.Text.text = y.Data.ToString("#00.00"));
                // スコア表示Textを0に設定
                p.TextScore.text = "0";
                // 着陸の段階の文字、文字色を設定し、非表示に
                p.TextSpeedStage.text = p.SpeedStageString;
                p.TextSpeedStage.color = _speedStageTextColors[p.SpeedStage];
                p.TextSpeedStage.gameObject.SetActive(false);
            });
            // コルーチンの組み合わせクラスを設定
            // Insertは指定した時間待った後に再生、Apend系は登録した順にInsertで登録したものの後に再生
            // Play関数を呼ばない糸実行されない
            var cs0 = new CoroutineSequence(this);
            var cs1 = new CoroutineSequence(this);
            // 各表示板を表示位置まで移動する処理
            _plates.ForEach(p => p.Rects.Select((Rect, Index) => new { Rect, Index }).ToList().ForEach(x =>
                 cs0.Insert(MovePlateEnumerator(x.Rect, p.TargetPosX), _animationInterval * x.Index)));
            // スコアをカウントアップする処理を登録しておく
            _plates.ForEach(p => cs1.Insert(CountUpScoreEnumerator(p.TextScore, p.Score)));
            // 表示板移動後、少し待機
            cs0.AppendInterval(_animationInterval)
                // 着陸の段階を表示
                .AppendCallback(() => _plates.ForEach(p => p.TextSpeedStage.gameObject.SetActive(true)))
                // 少し待機
                .AppendInterval(_animationInterval)
                // スコアのカウントアップ処理を再生
                .Append(cs1)
                // アニメーション再生フラグをfalseにし、操作を受け付けるように
                .AppendCallback(() => IsAnimation = false)
                // 再生
                .Play();
        }

        /// <summary> 特定のオブジェクトをX軸方向に移動させるコルーチン </summary>
        /// <param name="rect">移動オブジェクト</param>
        /// <param name="targetPosX">目標位置のX座標(localPosition)</param>
        private IEnumerator MovePlateEnumerator(RectTransform rect, float targetPosX)
        {
            // 1秒間に移動する距離
            float deltaX = (targetPosX - rect.localPosition.x) / _plateMoveTime;
            bool direFlag = targetPosX > rect.localPosition.x;
            while (direFlag ? targetPosX > rect.localPosition.x : targetPosX < rect.localPosition.x)
            {
                rect.localPosition += new Vector3(deltaX * Time.deltaTime, 0, 0);
                yield return null;
            }
            // 正しい位置に補正
            rect.localPosition = rect.localPosition.SetVector3(x: targetPosX);
        }

        /// <summary> スコアをカウントアップさせる処理 </summary>
        /// <param name="text">表示Text</param>
        /// <param name="score">スコア</param>
        private IEnumerator CountUpScoreEnumerator(Text text, int score)
        {
            // 1秒間に増える表示数
            float deltaScore = score / _countUpScoreTime;
            float displayScore = 0;
            bool isPositive = score > 0;
            while (isPositive ? displayScore < score : displayScore > score)
            {
                displayScore += deltaScore * Time.deltaTime;
                text.text = ((int)displayScore).ToString();
                yield return null;
            }
            // 正しい数値に補正
            text.text = score.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            if (Utility.GetKeyDownMultiple(KeyCode.E, KeyCode.N, KeyCode.D))
            {
                SceneManager.LoadScene("StartScene");
                _audioSource.Stop();
            }
            if (Utility.GetKeyDownMultiple(KeyCode.E, KeyCode.S, KeyCode.C))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
            }
            // アニメーション中でなく、何らかの操作を受け付けたらStartSceneに移動する
            if (!IsAnimation && Input.anyKey)
            {
                SceneManager.LoadScene("StartScene");
                _audioSource.Stop();
            }
        }
    }
}