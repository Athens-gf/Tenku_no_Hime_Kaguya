using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Summer2019
{
    public class StageManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _prefabBackGround;
        [SerializeField]
        private GameObject _prefabBounce;
        [SerializeField]
        private GameObject _prefabSlow;
        [SerializeField]
        private GameObject _prefabBounceAlert;
        [SerializeField]
        private GameObject _prefabSlowAlert;
        [SerializeField]
        private Transform _parentBackGround;
        [SerializeField]
        private Transform _parentObject;
        [SerializeField]
        private Transform _parentAlert;
        [SerializeField]
        private List<Transform> _walls;
        [SerializeField]
        /// <summary> 床 </summary>
        private Transform _floor;
        [SerializeField]
        /// <summary> ゴール画像 </summary>
        private Transform _goal;
        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private Player _player;
        [SerializeField]
        private float _backGroundImageSlideHeight = -20f;
        [SerializeField]
        private float _slideGoalY = 9f;
        [SerializeField]
        private float _slideX = 50f;
        [SerializeField]
        private float _alertHeight = -15f;
        [SerializeField]
        private float _alertPosY = -8.5f;
        public GameManager GameManager { get; set; }
        public int PlayerNumber { get; set; } = 1;
        public Camera Camera => _camera;
        public Player Player => _player;

        public List<GameObject> Objects { get; private set; } = new List<GameObject>();
        public LinkedList<GameObject> AlertList { get; private set; } = new LinkedList<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            // null check
            var inspectorItems = (new object[] { _prefabBackGround, _prefabBounce, _prefabSlow,
                _prefabBounceAlert, _prefabSlowAlert,
                _parentBackGround, _parentObject, _parentAlert, _floor, _goal, _camera, _player })
                .Union(_walls);
            if (inspectorItems.Any(o => o == null))
                Debug.LogError($"Not set inspector {inspectorItems.First(o => o == null)}.");

            // 別のプレイヤーのと場所がかぶらないよう移動
            transform.localPosition = new Vector3(_slideX * PlayerNumber, 0, 0);
            // 壁・床・ゴールの長さ・位置変更
            _walls.ForEach(w =>
            {
                w.localPosition = w.localPosition.SetVector3(y: -GameManager.Height / 2);
                w.localScale = w.localScale.SetVector3(y: GameManager.Height);
            });
            _floor.localPosition = _floor.localPosition.SetVector3(y: -GameManager.Height);
            _goal.localPosition = _goal.localPosition.SetVector3(y: -GameManager.Height + _slideGoalY);
            // 背景生成
            for (int i = 0; i < (int)Mathf.Abs(GameManager.Height / _backGroundImageSlideHeight) + 1; i++)
            {
                GameObject obj = Instantiate(_prefabBackGround, _parentBackGround);
                obj.transform.localPosition = obj.transform.localPosition.SetVector3(y: obj.transform.localPosition.y + _backGroundImageSlideHeight * i);
            }
            // オブジェクト生成
            Objects.Clear();
            foreach (var item in GameManager.ObjectPos)
            {
                GameObject prefab = item.Key ? _prefabBounce : _prefabSlow;
                foreach (var pos in item.Value)
                {
                    GameObject obj = Instantiate(prefab, _parentObject);
                    obj.transform.localPosition = pos.SetVector2(x: pos.x * (PlayerNumber == 0 ? 1 : -1));
                    Objects.Add(obj);
                }
            }
            _player.StageManager = this;
            Camera.pixelRect = new Rect(new Vector2(Camera.pixelRect.size.x * PlayerNumber, 0), Camera.pixelRect.size);
            AlertList.Clear();
        }

        // Update is called once per frame
        void Update()
        {
            float alertY = _player.transform.localPosition.y + _alertHeight;
            Objects.Except(AlertList)
                .Where(obj => Mathf.Abs(obj.transform.localPosition.y - alertY) < 0.5f)
                .ToList().ForEach(obj => StartCoroutine(AlertEnumerator(obj)));
        }

        private IEnumerator AlertEnumerator(GameObject obj)
        {
            var node = AlertList.AddLast(obj);
            bool isBounce = obj.GetComponent<BounceObject>() != null;
            GameObject alert = Instantiate(isBounce ? _prefabBounceAlert : _prefabSlowAlert, _parentAlert);
            while (alert.transform.localPosition.y > obj.transform.localPosition.y)
            {
                alert.transform.localPosition = obj.transform.localPosition.SetVector3(y: _player.transform.localPosition.y + _alertPosY);
                yield return null;
            }
            Destroy(alert);
            AlertList.Remove(node);
        }

        public void StartGame()
        {
            _player.Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}