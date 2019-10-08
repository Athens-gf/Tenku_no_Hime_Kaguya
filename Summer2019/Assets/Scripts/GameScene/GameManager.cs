using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Summer2019
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        /// <summary> BGMクリップ </summary>
        private AudioClip _bgmClip;
        [SerializeField]
        /// <summary> スタート時SEクリップ </summary>
        private AudioClip _seStart;
        [SerializeField]
        /// <summary> 着地時SEクリップ </summary>
        private AudioClip _seLanding;
        [SerializeField]
        /// <summary> 反射時SEクリップ </summary>
        private AudioClip _seBounce;
        [SerializeField]
        /// <summary> 減速時SEクリップ </summary>
        private AudioClip _seSlow;
        /// <summary> 音声を再生するためのクラス(0:BGM,1:1P-SE,2:2P-SE) </summary>
        public AudioSource[] AudioSource { get; private set; }

        [SerializeField]
        private GameObject _prefabStage;
        [SerializeField]
        private GameObject _startPlate;
        [SerializeField]
        private GameObject _finishPlate;
        [SerializeField]
        private Text _textStartTimer;
        [SerializeField]
        private float _height = 1000f;
        public float Height => _height;
        [SerializeField]
        private int _objectCount = 100;
        [SerializeField]
        private int _noneObjectAreaHeight = 12;
        [SerializeField]
        private RectTransform[] _positionArrow;
        public RectTransform[] PositionArrow => _positionArrow;
        [SerializeField]
        private RectTransform[] _speedArrow;
        public RectTransform[] SpeedArrow => _speedArrow;
        public RectTransform PositionArrowParent { get; private set; }
        [SerializeField]
        private Text[] _textTimer;
        public Text[] TextTimer => _textTimer;
        public Dictionary<bool, List<Vector2>> ObjectPos { get; private set; } = new Dictionary<bool, List<Vector2>>();
        public bool IsStarting { get; set; }
        public bool IsEnding { get; set; }
        [SerializeField]
        private float _startTimer = 3;
        public float StartTimer { get; set; }
        public List<StageManager> Stages { get; set; } = new List<StageManager>();

        // Start is called before the first frame update
        void Awake()
        {
            // null check
            var inspectorItems = (new object[] { _prefabStage, _startPlate, _finishPlate, _textStartTimer,
                _bgmClip, _seStart, _seLanding, _seBounce, _seSlow })
                .Union(_textTimer)
                .Union(_positionArrow);
            if (inspectorItems.Any(o => o == null))
                Debug.LogError($"Not set inspector {inspectorItems.First(o => o == null)}.");

            // 音声再生の設定、0はBGM用、1は1PのSE用、2は2PのSE用
            AudioSource = gameObject.GetComponents<AudioSource>();
            AudioSource[0].loop = true;
            AudioSource[1].loop = false;
            AudioSource[1].panStereo = -1;
            AudioSource[2].loop = false;
            AudioSource[2].panStereo = 1;

            // パネルの表示設定
            _startPlate.SetActive(true);
            _finishPlate.SetActive(false);

            GameData.Instance.Scores = Enumerable.Range(0, 2).Select(_ => new GameData.ScoreData()).ToList();
            // オブジェクト配置位置生成
            ObjectPos[true] = new List<Vector2>();
            ObjectPos[false] = new List<Vector2>();
            for (int i = 0; i < _objectCount - 5; i++)
            {
                float x = Random.Range(-4f, 4f);
                float y = -Random.Range(_noneObjectAreaHeight, Height - _noneObjectAreaHeight * 1.5f);
                bool isBounce = Random.Range(0, 2) == 0;
                ObjectPos[isBounce].Add(new Vector2(x, y));
            }
            // 5個は最低でもステージ下部に生成する(1個は反射オブジェクト)
            for (int i = 0; i < 5; i++)
            {
                float x = Random.Range(-4f, 4f);
                float y = -Random.Range(Height - _noneObjectAreaHeight, Height - _noneObjectAreaHeight * 2);
                bool isBounce = i == 0;
                ObjectPos[isBounce].Add(new Vector2(x, y));
            }
            // ステージ生成
            Stages.Clear();
            for (int i = 0; i < 2; i++)
            {
                GameObject obj = Instantiate(_prefabStage);
                StageManager stage = obj.GetComponent<StageManager>();
                Stages.Add(stage);
                stage.GameManager = this;
                stage.PlayerNumber = i;
            }
            PositionArrowParent = PositionArrow.First().parent.GetComponent<RectTransform>();
            IsEnding = false;
            StartCoroutine(StartTimerEnumerator());
        }

        private IEnumerator StartTimerEnumerator()
        {
            IsStarting = false;
            StartTimer = _startTimer;
            AudioSource[1].clip = _seStart;
            AudioSource[1].Play();
            while (StartTimer > 0)
            {
                StartTimer -= Time.deltaTime;
                _textStartTimer.text = ((int)StartTimer + 1).ToString();
                yield return null;
            }
            foreach (var stage in Stages)
            {
                stage.StartGame();
            }
            IsStarting = true;
            _startPlate.SetActive(false);
            AudioSource[0].clip = _bgmClip;
            AudioSource[0].Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (Utility.GetKeyDownMultiple(KeyCode.E, KeyCode.N, KeyCode.D))
            {
                SceneManager.LoadScene("StartScene");
                AudioSource[0].Stop();
            }
            if (Utility.GetKeyDownMultiple(KeyCode.E, KeyCode.S, KeyCode.C))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    UnityEngine.Application.Quit();
#endif
            }
            if (!IsEnding && Stages.All(s => s.Player.IsLanding))
            {
                IsEnding = true;
                AudioSource[0].Stop();
                _finishPlate.SetActive(true);
            }
            else if (IsEnding)
            {
                if (Input.anyKey)
                    SceneManager.LoadScene("ResultScene");
            }
        }

        private void SoundPlay(AudioClip clip, int playerNumber)
        {
            AudioSource audioSource = AudioSource[playerNumber == 0 ? 1 : 2];
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void SoundBounce(int playerNumber) => SoundPlay(_seBounce, playerNumber);

        public void SoundSlow(int playerNumber) => SoundPlay(_seSlow, playerNumber);

        public void SoundLanding(int playerNumber) => SoundPlay(_seLanding, playerNumber);
    }
}