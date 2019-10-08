using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Summer2019
{
    public class SlowObject : MonoBehaviour
    {
        [SerializeField]
        private float _slowRate = 0.3f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary> 抜ける時の減速処理 </summary>
        /// <param name="collision">当たった対象</param>
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.attachedRigidbody == null)
                return;
            collision.attachedRigidbody.AddForce(Vector2.up * collision.attachedRigidbody.velocity.magnitude * _slowRate, ForceMode2D.Impulse);
            Player player = collision.GetComponent<Player>();
            player.GameManager.SoundSlow(player.PlayerNumber);
        }
    }
}