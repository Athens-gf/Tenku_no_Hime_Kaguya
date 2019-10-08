using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Summer2019
{
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] _spriteFalling;
        [SerializeField]
        private Sprite[] _spriteLanding;
        [SerializeField]
        private float _cameraPosY = -3.5f;
        [SerializeField]
        private float _speed = 0.5f;
        [SerializeField]
        private float _moveLimitX = 4f;
        public Rigidbody2D Rigidbody2D { get; private set; }
        public float Timer
        {
            get { return GameData.Instance.Scores[PlayerNumber].Time; }
            set
            {
                GameData.Instance.Scores[PlayerNumber].Time = value;
                TextTimer.text = value.ToString("00.00");
            }
        }
        public StageManager StageManager { get; set; }
        public GameManager GameManager => StageManager.GameManager;
        public int PlayerNumber => StageManager.PlayerNumber;
        public Transform CameraTransform => StageManager.Camera.transform;
        public bool IsLanding { get; private set; }
        public Text TextTimer => GameManager.TextTimer[PlayerNumber];
        public RectTransform PositionArrow => GameManager.PositionArrow[PlayerNumber];
        public float PositionArrowParentHeight => GameManager.PositionArrowParent.sizeDelta.y;
        public RectTransform SpeedArrow => GameManager.SpeedArrow[PlayerNumber];
        public float SpeedArrowParentHeight { get; private set; }
        public float SpeedArrowParentMaxHeight { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            SpriteRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer.sprite = _spriteFalling[PlayerNumber];
            Timer = 0;
            IsLanding = false;
            Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            SpeedArrowParentHeight = SpeedArrow.parent.GetComponent<RectTransform>().sizeDelta.y;
            SpeedArrowParentMaxHeight = SpeedArrow.parent.parent.GetComponent<RectTransform>().sizeDelta.y;
        }

        // Update is called once per frame
        void Update()
        {
            // 場所表示
            var paPosY = -(Mathf.Abs(transform.localPosition.y / GameManager.Height) - 0.5f) * PositionArrowParentHeight;
            PositionArrow.localPosition = PositionArrow.localPosition.SetVector3(y: paPosY);
            // 速度表示
            var saPosY = Mathf.Min(Mathf.Max(GameData.Instance.Scores[PlayerNumber].Speed / 100, 0), SpeedArrowParentMaxHeight) * SpeedArrowParentHeight;
            SpeedArrow.localPosition = SpeedArrow.localPosition.SetVector3(y: saPosY);
            if (IsLanding || !GameManager.IsStarting)
                return;
            // 速度
            GameData.Instance.Scores[PlayerNumber].Speed = -Rigidbody2D.velocity.y;
            if (CameraTransform.position.y > -GameManager.Height + 5)
                CameraTransform.position = CameraTransform.position.SetVector3(y: transform.position.y + _cameraPosY);
            // 左右の向き反転
            /*
            if (Rigidbody2D.velocity.magnitude > 1)
                if (Rigidbody2D.velocity.x > 0 && transform.localScale.x > 0)
                    SpriteRenderer.flipX = true;
                else if (Rigidbody2D.velocity.x < 0 && transform.localScale.x < 0)
                    SpriteRenderer.flipX = false;
                    */
            // 入力所得・移動
            Timer += Time.deltaTime;
            var horizontal = Input.GetAxis($"PadKey{PlayerNumber + 1}_Horizontal");
            if (!(transform.localPosition.x < -_moveLimitX && horizontal < 0) &&
                !(transform.localPosition.x > _moveLimitX && horizontal > 0))
            {
                transform.localPosition += Vector3.right * horizontal * _speed;
                if (!horizontal.IsNearZero())
                    Rigidbody2D.velocity = Rigidbody2D.velocity.SetVector2(x: 0);
            }
            if (!horizontal.IsNearZero())
                SpriteRenderer.flipX = horizontal > 0;
        }

        public void SetLanding()
        {
            IsLanding = true;
            CameraTransform.position = CameraTransform.position.SetVector3(y: -GameManager.Height + 5);
            SpriteRenderer.sprite = _spriteLanding[PlayerNumber];
            SpriteRenderer.flipX = false;
            GameManager.SoundLanding(PlayerNumber);
        }
    }
}