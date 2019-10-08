using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Summer2019
{
    public class Floor : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            collision.gameObject.GetComponent<Player>().SetLanding();
        }
    }
}