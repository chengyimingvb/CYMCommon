using UnityEngine;
using System.Collections;
namespace CYM
{

    public class DisableOnPlay : MonoBehaviour
    {

        void Awake()
        {
            gameObject.SetActive(false);
        }
    }

}