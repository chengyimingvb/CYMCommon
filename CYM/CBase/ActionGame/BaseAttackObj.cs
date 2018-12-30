using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    [Serializable]
    public class BaseAttackObj :ICloneable
    {
        public int Damage { get; set; }

        public Vector3 Size { get; set; } = Vector3.zero; //new Vector3();
        public float Distance { get; set; }
        public float Height { get; set; }

        public string[] StartPerform { get; set; }
        public string[] HitPerform { get; set; }
        public string[] HitBloodPerform { get; set; }
        public string[] HitNormalPerform { get; set; }

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
