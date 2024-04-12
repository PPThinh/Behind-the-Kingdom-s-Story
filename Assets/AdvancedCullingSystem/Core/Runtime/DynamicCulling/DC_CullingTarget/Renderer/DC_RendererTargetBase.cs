using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public abstract class DC_RendererTargetBase : ICullingTarget
    {
        public GameObject GameObject
        {
            get
            {
                return Renderer.gameObject;
            }
        }
        public Bounds Bounds
        {
            get
            {
                return Renderer.bounds;
            }
        }

        protected Renderer Renderer { get; private set; }

        public DC_RendererTargetBase(Renderer renderer)
        {
            Renderer = renderer;
        }

        public abstract void MakeInvisible();

        public abstract void MakeVisible();
    }
}
