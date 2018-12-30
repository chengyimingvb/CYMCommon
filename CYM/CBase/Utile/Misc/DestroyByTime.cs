using UnityEngine;
using System.Collections;
namespace CYM
{
    
    public class DestroyByTime : MonoBehaviour
    {
        //CYMTimer timer = new CYMTimer();
        public float Time = 0;
        float curTime = 0;
        private void Awake()
        {

        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            curTime += UnityEngine.Time.deltaTime;
            if (curTime > Time)
                Destroy(gameObject);
        }
    }

}