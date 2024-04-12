using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public interface ICullingTarget
    {
        GameObject GameObject { get; }
        Bounds Bounds { get; }

        void MakeVisible();

        void MakeInvisible();
    }
}
