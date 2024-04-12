using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public abstract class DC_LODGroupTargetBase : ICullingTarget
    {
        public GameObject GameObject 
        { 
            get 
            {
                return Group.gameObject;
            } 
        }
        public Bounds Bounds { get; private set; }

        protected LODGroup Group { get; private set; }
        protected Renderer[] Renderers { get; private set; }

        public DC_LODGroupTargetBase(LODGroup group, Renderer[] renderers, Bounds bounds)
        {
            Group = group;
            Renderers = renderers;
            Bounds = bounds;
        }

        public abstract void MakeVisible();

        public abstract void MakeInvisible();


        private void GetRenderers(bool calculateBounds)
        {
            Bounds bounds = new Bounds(Group.transform.position, Vector3.zero);

            LOD[] lods = Group.GetLODs();

            int count = lods.Count(r => r != null);

            if (count == 0)
                throw new Exception("Can't find renderers in LODGroup " + GameObject.name);

            Renderers = new Renderer[count];

            int idx = 0;
            for (int i = 0; i < lods.Length; i++)
            {
                Renderer[] lodRenderers = lods[i].renderers;

                for (int c = 0; c < lodRenderers.Length; c++)
                {
                    Renderer renderer = lodRenderers[c];

                    if (renderer != null)
                    {
                        Renderers[idx++] = renderer;

                        if (calculateBounds)
                            bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            if (calculateBounds)
                Bounds = bounds;
        }
    }
}
