using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Title("Renderer Shared Material")]
    [Category("Renderers/Renderer Shared Material")]

    [Image(typeof(IconSkinMesh), ColorTheme.Type.Blue)]
    [Description("The Material shared instance associated with a Renderer component")]

    [Serializable]
    public class SetMaterialRendererShared : PropertyTypeSetMaterial
    {
        [SerializeField] private PropertyGetGameObject m_Renderer = GetGameObjectInstance.Create();

        public override void Set(Material value, Args args)
        {
            Renderer renderer = this.m_Renderer.Get<Renderer>(args);
            if (renderer == null) return;
            
            renderer.sharedMaterial = value;
        }

        public override Material Get(Args args)
        {
            Renderer renderer = this.m_Renderer.Get<Renderer>(args);
            return renderer != null ? renderer.sharedMaterial : null;
        }
        
        public override string String => this.m_Renderer.ToString();
    }
}