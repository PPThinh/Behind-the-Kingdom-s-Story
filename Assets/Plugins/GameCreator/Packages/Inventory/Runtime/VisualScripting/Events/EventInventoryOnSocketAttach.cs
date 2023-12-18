using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Inventory.UnityUI
{
    [Title("On Socket Attach")]
    [Category("Inventory/Sockets/On Socket Attach")]
    [Description("Detects when an Item's Socket gets another Item attached")]
    
    [Image(typeof(IconSocket), ColorTheme.Type.Yellow, typeof(OverlayPlus))]

    [Serializable]
    public class EventInventoryOnSocketAttach : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            
            RuntimeSockets.EventAttachRuntimeItem -= this.Callback;
            RuntimeSockets.EventAttachRuntimeItem += this.Callback;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            RuntimeSockets.EventAttachRuntimeItem -= this.Callback;
        }
        
        private void Callback(RuntimeItem runtimeItem, RuntimeItem attachment)
        {
            _ = this.m_Trigger.Execute(this.Self);   
        }
    }
}