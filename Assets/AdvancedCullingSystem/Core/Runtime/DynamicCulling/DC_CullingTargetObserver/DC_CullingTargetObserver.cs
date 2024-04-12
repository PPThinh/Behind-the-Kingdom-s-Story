using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_CullingTargetObserver : MonoBehaviour
    {
        private DC_Source _source;
        private ICullingTarget _target;

        public void Initialize(DC_Source source, ICullingTarget target)
        {
            _source = source;
            _target = target;
        }

        private void OnDestroy()
        {
            if (!gameObject.scene.isLoaded)
                return;

            _source?.RemoveCullingTarget(_target);
        }
    }
}