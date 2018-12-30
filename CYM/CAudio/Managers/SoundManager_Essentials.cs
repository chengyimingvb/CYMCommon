using UnityEngine;
using System.Collections;

namespace CYM.Audio
{
    public partial class SoundManager : Singleton<SoundManager>
    {

        /// <summary>
        /// Gets or sets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static SoundManager Instance
        {
            get
            {
                return ((SoundManager)mInstance);
            }
            set
            {
                mInstance = value;
            }
        }

        /// <summary>
        /// Enum representing what method to play songs
        /// </summary>
        public enum PlayMethod
        {
            ContinuousPlayThrough,
            ContinuousPlayThroughWithDelay,
            ContinuousPlayThroughWithRandomDelayInRange,
            OncePlayThrough,
            OncePlayThroughWithDelay,
            OncePlayThroughWithRandomDelayInRange,
            ShufflePlayThrough,
            ShufflePlayThroughWithDelay,
            ShufflePlayThroughWithRandomDelayInRange,
        }
    }
}
