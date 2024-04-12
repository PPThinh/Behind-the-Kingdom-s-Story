using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public interface IDC_SourceSettingsStrategy
    {
        int Layer { get; set; }
        bool Baked { get; }

        bool CheckCompatibilityAndGetComponents(out string incompatibilityReason);

        void Bake();

        void ClearBakedData();


        bool TryGetBounds(ref Bounds bounds);

        ICullingTarget CreateCullingTarget(CullingMethod cullingMethod);

        IEnumerable<Collider> GetColliders();
    }
}
