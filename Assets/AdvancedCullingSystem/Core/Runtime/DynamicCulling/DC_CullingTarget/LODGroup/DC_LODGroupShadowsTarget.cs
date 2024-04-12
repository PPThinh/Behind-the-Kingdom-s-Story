using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_LODGroupShadowsTarget : DC_LODGroupTargetBase
    {
        private Renderer[] _renderers;
        private ShadowCastingMode _shadowMode;

        public DC_LODGroupShadowsTarget(LODGroup group, Renderer[] renderers, Bounds bounds)
            : base(group, renderers, bounds)
        {
            _renderers = Renderers;
            _shadowMode = _renderers[0].shadowCastingMode;
        }

        public override void MakeVisible()
        {
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].shadowCastingMode = _shadowMode;
        }

        public override void MakeInvisible()
        {
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }
}
