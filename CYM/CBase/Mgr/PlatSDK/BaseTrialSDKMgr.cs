using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CYM
{
    public class BaseTrialSDKMgr : BasePlatSDKMgr
    {

        public override Distribution DistributionType => Distribution.Trial;
    }

}