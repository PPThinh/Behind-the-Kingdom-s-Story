using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Title("Renderer Material")]
    [Category("Renderers/Renderer Material")]

    [Image(typeof(IconSkinMesh), ColorTheme.Type.Yellow)]
    [Description("The Material instance associated with a Renderer component")]

    [Serializable]
    public class SetMaterialRendererInstance : PropertyTypeSetMaterial
    {
        [SerializeField] private PropertyGetGameObject m_Renderer = GetGameObjectInstance.Create();

        public override void Set(Material value, Args args)
        {
            Renderer renderer = this.m_Renderer.Get<Renderer>(args);
            if (renderer == null) return;
            
            renderer.material = value;
        }

        public override Material Get(Args args)
        {
            Renderer renderer = this.m_Renderer.Get<Renderer>(args);
            return renderer != null ? renderer.material : null;
        }

        public override string String => this.m_Renderer.ToString();
    }
}