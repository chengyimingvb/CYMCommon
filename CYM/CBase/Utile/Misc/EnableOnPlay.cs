using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public class EnableOnPlay : MonoBehaviour
    {
        void Awake()
        {
            gameObject.SetActive(true);
        }

    }
}
