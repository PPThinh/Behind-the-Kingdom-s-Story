using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_RendererTarget : DC_RendererTargetBase
    {
        private Renderer _renderer;

        public DC_RendererTarget(Renderer renderer) 
            : base(renderer)
        {
            _renderer = Renderer;
        }

        public override void MakeVisible()
        {
            _renderer.enabled = true;
        }

        public override void MakeInvisible()
        {
            _renderer.enabled = false;
        }
    }
}
