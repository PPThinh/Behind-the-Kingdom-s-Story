using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.Characters
{
    [Title("Character with Offset")]
    [Category("Characters/Character with Offset")]
    
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow, typeof(OverlayArrowRight))]
    [Description("The position and rotation of the Character plus an offset in local space")]

    [Serializable]
    public class GetLocationCharacterLocalOffset : PropertyTypeGetLocation
    {
        [SerializeField]
        protected PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();

        [SerializeField] private EnablerVector3 m_Offset = new EnablerVector3(true, Vector3.forward);
        [SerializeField] private EnablerVector3 m_Rotate = new EnablerVector3(true, Vector3.zero);

        public override Location Get(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            return character != null 
                ? new Location(
                    character.transform,
                    Space.Self,
                    this.m_Offset.IsEnabled ? this.m_Offset.Value : Vector3.zero,
                    this.m_Rotate.IsEnabled, this.m_Rotate.IsEnabled
                        ? Quaternion.Euler(this.m_Rotate.Value) 
                        : Quaternion.identity
                    )
                : new Location();
        }

        public static PropertyGetLocation Create => new PropertyGetLocation(
            new GetLocationCharacterLocalOffset()
        );

        public override string String => $"{this.m_Character}";
    }
}