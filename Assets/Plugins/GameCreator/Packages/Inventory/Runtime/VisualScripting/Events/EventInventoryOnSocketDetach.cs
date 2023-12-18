using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Inventory.UnityUI
{
    [Title("On Socket Detach")]
    [Category("Inventory/Sockets/On Socket Detach")]
    [Description("Detects when an Item is detached from another Item's Socket")]
    
    [Image(typeof(IconSocket), ColorTheme.Type.Yellow, typeof(OverlayMinus))]

    [Serializable]
    public class EventInventoryOnSocketDetach : Event
    {
        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            
            RuntimeSockets.EventDetachRuntimeItem -= this.Callback;
            RuntimeSockets.EventDetachRuntimeItem += this.Callback;
        }

        protected override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            RuntimeSockets.EventDetachRuntimeItem -= this.Callback;
        }
        
        private void Callback(RuntimeItem runtimeItem, RuntimeItem attachment)
        {
            _ = this.m_Trigger.Execute(this.Self);   
        }
    }
}