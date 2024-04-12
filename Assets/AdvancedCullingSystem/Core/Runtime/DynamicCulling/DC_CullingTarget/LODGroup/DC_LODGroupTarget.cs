using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_LODGroupTarget : DC_LODGroupTargetBase
    {
        private Renderer[] _renderers;

        public DC_LODGroupTarget(LODGroup group, Renderer[] renderers, Bounds bounds) 
            : base(group, renderers, bounds)
        {
            _renderers = Renderers;
        }

        public override void MakeVisible()
        {
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].enabled = true;
        }

        public override void MakeInvisible()
        {
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].enabled = false;
        }
    }
}
