using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_SingleSourcesProvider : IDC_SourcesProvider
    {
        public DC_Source GetSource(ICullingTarget cullingTarget)
        {
            GameObject go = new GameObject("DC_SingleSource");

            DC_SingleSource source = go.AddComponent<DC_SingleSource>();
            source.SetCullingTarget(cullingTarget);

            return source;
        }
    }
}
