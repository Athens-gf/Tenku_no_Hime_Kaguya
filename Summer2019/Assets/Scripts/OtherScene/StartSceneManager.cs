using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Summer2019
{
    public class StartSceneManager : MonoBehaviour
    {
        [SerializeField]
        /// <summary> 音声を再生するためのクラス </summary>
        private AudioSource _audioSource;
        [SerializeField]
        /// <summary> BGMクリップ </summary>
        private AudioClip _bgmClip;
        [SerializeField]
        /// <summary> 難易度選択画面 </summary>
        private GameObject _difficultyPlate;
        [SerializeField]
        /// <summary> 難易度選択肢の選択矢印 </summary>
        private List<GameObject> _difficultySelects;

        public Difficulty Difficulty
        {
            get
            {
                return GameData.Instance.Difficulty;
            }
            set
            {
                GameData.Instance.Difficulty = value;
                if (_difficultySelects.Count == (int)Difficulty.Max)
                {
                    _difficultySelects.ForEach(s => s.SetActive(false));
                    _difficultySelects[(int)value].SetActive(true);
                }
                else
                    throw new System.Exception("Miss DifficultySelects count");
            }
        }

        public float InputTimer { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            // null check
            var inspectorItems = (new object[] { _audioSource, _bgmClip, _difficultyPlate, }).Union(_difficultySelects);
            if (inspectorItems.Any(o => o == null))
                Debug.LogError($"Not set inspector {inspectorItems.First(o => o == null)}.");

            // BGM設定
            _audioSource.loop = true;
            _audioSource.clip = _bgmClip;
            _audioSource.Play();

            // 難易度選択画面非表示
            _difficultyPlate.SetActive(false);
            // 難易度をNormalに
            Difficulty = Difficulty.Normal;

            InputTimer = 0;
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
            if (_difficultyPlate.activeSelf)
            {
                if (Input.GetButtonDown("Submit"))
                {
                    SceneManager.LoadScene("GameScene");
                    _audioSource.Stop();
                }
                else
                {
                    int count = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        var horizontal = Input.GetAxis($"PadKey{i + 1}_Horizontal");
                        if (!horizontal.IsNearZero())
                        {
                            if (InputTimer == 0)
                                Difficulty = (Difficulty)((int)(Difficulty + (horizontal > 0 ? 1 : (int)Difficulty.Max - 1)) % (int)Difficulty.Max);
                            InputTimer += Time.deltaTime;
                            if (InputTimer > 0.5f)
                                InputTimer = 0;
                            count++;
                        }
                    }
                    if (count == 0)
                        InputTimer = 0;

                }
            }
            else if (Input.anyKeyDown)
            {
                // 難易度選択画面表示
                _difficultyPlate.SetActive(true);
            }
        }
    }
}