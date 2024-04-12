using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem
{
    public static class LODGroupHelper
    {
        public static int Count(this LODGroup group, Func<Renderer, bool> filter)
        {
            LOD[] lods = group.GetLODs();

            return Count(lods, filter);
        }

        public static int Count(this LOD[] lods, Func<Renderer, bool> filter)
        {
            int count = 0;

            for (int i = 0; i < lods.Length; i++)
            {
                Renderer[] renderers = lods[i].renderers;

                for (int c = 0; c < renderers.Length; c++)
                {
                    Renderer renderer = renderers[c];

                    if (renderer == null)
                        continue;

                    if (filter(renderer))
                        count++;
                }
            }

            return count;
        }

        public static bool ContainsAny(this LOD[] lods, Func<Renderer, bool> filter)
        {
            for (int i = 0; i < lods.Length; i++)
            {
                Renderer[] renderers = lods[i].renderers;

                for (int c = 0; c < renderers.Length; c++)
                    if (filter(renderers[c]))
                        return true;
            }

            return false;
        }
    }
}
