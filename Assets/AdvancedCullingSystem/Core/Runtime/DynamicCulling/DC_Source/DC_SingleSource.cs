using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public class DC_SingleSource : DC_Source
    {
        private ICullingTarget _cullingTarget;
        private bool _visible;


        public override void SetCullingTarget(ICullingTarget target)
        {
            _cullingTarget = target;
            _cullingTarget.MakeInvisible();
        }

        public override void RemoveCullingTarget(ICullingTarget target)
        {
            enabled = false;
            Destroy(gameObject);
        }


        protected override void OnHitInternal()
        {
            if (_visible)
                return;

            _cullingTarget.MakeVisible();
            _visible = true;
        }

        protected override void OnTimeout()
        {
            _cullingTarget.MakeInvisible();
            _visible = false;
        }
    }
}
