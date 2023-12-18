using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Melee
{
    [Title("Capsule")]
    [Category("Capsule")]
    
    [Image(typeof(IconCapsuleSolid), ColorTheme.Type.Yellow)]
    [Description("A capsule shape that detects any overlapping game objects with a collider")]
    
    [Serializable]
    public class StrikerCapsule : TStrikerShape
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private Vector3 m_Position = Vector3.zero;
        [SerializeField] private float m_Height = 1f;
        [SerializeField] private float m_Radius = 0.1f;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override Vector3 Position => this.m_Position;
        protected override Vector3 Rotation => Vector3.zero;

        // PROTECTED METHODS: ---------------------------------------------------------------------

        protected override int Cast(Vector3 position, Quaternion rotation, LayerMask layerMask)
        {
            Vector3 direction = rotation * Vector3.right;
            Vector3 point1 = position + direction * (+this.m_Height * 0.5f);
            Vector3 point2 = position + direction * (-this.m_Height * 0.5f);
            
            int numHits = Physics.OverlapCapsuleNonAlloc(
                point1, point2, this.m_Radius, HITS, 
                layerMask, QueryTriggerInteraction.Ignore
            );

            return numHits;
        }
        
        protected override Vector3 GetPoint(GameObject hit, Vector3 position, Quaternion rotation)
        {
            Vector3 direction = rotation * Vector3.right;
            Vector3 point1 = position + direction * (+this.m_Height * 0.5f);
            Vector3 point2 = position + direction * (-this.m_Height * 0.5f);

            return hit.transform.position.OnSegment(point1, point2);
        }

        // GIZMOS: --------------------------------------------------------------------------------

        protected override void DrawGizmos(Vector3 position, Quaternion rotation)
        {
            GizmosExtension.Capsule(
                position, 
                rotation, 
                this.m_Radius, 
                this.m_Height,
                24, 
                (int) GizmosExtension.CapsuleDirection.AxisX
            );
        }
    }
}