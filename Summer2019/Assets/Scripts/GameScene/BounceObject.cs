using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Summer2019
{
    public class BounceObject : MonoBehaviour
    {
        [SerializeField]
        /// <summary> 反射速度の倍率、難易度Easyの場合は乱数の上限 </summary>
        private float _bounceRateTop = 0.5f;
        [SerializeField]
        /// <summary> 難易度Easyの場合の乱数の下限 </summary>
        private float _bounceRateUnder = 0.2f;
        [SerializeField]
        /// <summary> 難易度Hardの場合の反射時の左右のブレ </summary>
        private float _bounceDirection = 2f;

        /// <summary> 入場した時のはじき返し処理 </summary>
        /// <param name="collision">当たった対象</param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody == null)
                return;
            // 反射時の速度(通常は元の速度の_bounceRateTop倍)
            float mag = collision.attachedRigidbody.velocity.magnitude * _bounceRateTop;
            // 反射時の角度(通常は上方)
            Vector2 vel = Vector2.up;
            switch (GameData.Instance.Difficulty)
            {
                case Difficulty.Easy:
                    // 難易度Easyの場合は反射時の速度の倍率を乱数化(_bounceRateUnder~_bounceRateTop)する
                    mag = collision.attachedRigidbody.velocity.magnitude * Random.Range(_bounceRateUnder, _bounceRateTop);
                    break;
                case Difficulty.Normal:
                    break;
                case Difficulty.Hard:
                    // 難易度Easyの場合は反射時の角度に左右の乱数が加わる
                    vel = Vector2.up + Vector2.right * Random.Range(-_bounceDirection, _bounceDirection);
                    break;
                default:
                    throw new System.Exception("Out of Range Difficulty");
            }
            collision.attachedRigidbody.velocity = vel.normalized * mag;
            Player player = collision.GetComponent<Player>();
            player.GameManager.SoundSlow(player.PlayerNumber);
        }
    }
}