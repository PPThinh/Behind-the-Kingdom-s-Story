using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public interface IDC_SourcesProvider
    {
        DC_Source GetSource(ICullingTarget cullingTarget);
    }
}
