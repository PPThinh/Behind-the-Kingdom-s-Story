using System;
using UnityEngine;

namespace GameCreator.Runtime.Common
{
    [Title("Renderer Material")]
    [Category("Renderers/Renderer Material")]
    
    [Image(typeof(IconSkinMesh), ColorTheme.Type.Yellow)]
    [Description("A reference to the main Material instance of a Renderer component")]

    [Keywords("Material", "Shader")]
    
    [Serializable]
    public class GetMaterialRendererInstance : PropertyTypeGetMaterial
    {
        [SerializeField] private PropertyGetGameObject m_Renderer = GetGameObjectInstance.Create();

        public override Material Get(Args args)
        {
            Renderer renderer = this.m_Renderer.Get<Renderer>(args);
            return renderer != null ? renderer.material : null;
        }

        public static PropertyGetMaterial Create() => new PropertyGetMaterial(
            new GetMaterialRendererInstance()
        );

        public override string String => $"{this.m_Renderer} Material";
    }
}