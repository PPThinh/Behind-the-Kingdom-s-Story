using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_RendererShadowsTarget : DC_RendererTargetBase
    {
        private Renderer _renderer;
        private ShadowCastingMode _shadowMode;

        public DC_RendererShadowsTarget(Renderer renderer) 
            : base(renderer)
        {
            _renderer = Renderer;
            _shadowMode = _renderer.shadowCastingMode;
        }

        public override void MakeVisible()
        {
            _renderer.shadowCastingMode = _shadowMode;
        }

        public override void MakeInvisible()
        {
            _renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }
    }
}
