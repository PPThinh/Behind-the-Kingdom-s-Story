using System;
using System.Threading.Tasks;
using DaimahouGames.Runtime.Characters;
using DaimahouGames.Runtime.Core.Common;
using DaimahouGames.Runtime.Core;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using Target = DaimahouGames.Runtime.Core.Common.Target;

namespace DaimahouGames.Runtime.Abilities
{
    [Category("Activation: Single")]
    
    [Serializable]
    public class AbilityActivatorSingle : AbilityActivator
    {
        //============================================================================================================||
        // ※  Variables: --------------------------------------------------------------------------------------------|
        // ---| Exposed State -------------------------------------------------------------------------------------->|
        
        [SerializeField] private ReactiveGesture m_Animation;
        [SerializeField] private bool m_WalkToTarget;
        
        // ---| Internal State ------------------------------------------------------------------------------------->|
        // ---| Dependencies --------------------------------------------------------------------------------------->|
        // ---| Properties ----------------------------------------------------------------------------------------->|

        public bool WalkToTarget => m_WalkToTarget;
        public override string Title => string.Format("Activation: Single [{0}]",
            m_Animation == null ? "(none)" : $"{m_Animation.name}"
        );

        // ---| Events --------------------------------------------------------------------------------------------->|
        // ※  Initialization Methods: -------------------------------------------------------------------------------|
        // ※  Public Methods: ---------------------------------------------------------------------------------------|

        public override async Task Activate(ExtendedArgs args)
        {
            var ability = args.Get<RuntimeAbility>();
            var caster = ability.Caster;

            if(!args.Has<Target>()) await ability.Targeting.ProcessInput(args);

            if (ability.IsCanceled) return;
            
            if (FaceTarget)
            {
                var character = ability.Caster.Get<Character>();
                if(character) character.FaceLocation(args.Get<Target>().GetLocation());
                else ability.Caster.Pawn.FaceLocation(args.Get<Target>().GetLocation());
            }

            if (WalkToTarget) await MoveToTarget(args);

            ability.CommitRequirements(args);
            var complete = false;

            if (!ability.IsCanceled)
            {
                var triggerReceipt = caster.Pawn.Message.Subscribe<MessageAbilityActivation>(_ =>
                {
                    ability.OnTrigger.Send(args);
                });
                var endReceipt = caster.Pawn.Message.Subscribe<MessageAbilityCompletion>(_ =>
                {
                    complete = true;
                });
                {
                    var gestureTask = caster.Get<Character>()?.PlayGesture(m_Animation, args);
                    if (gestureTask is {IsFaulted: false})
                    {
                        await Awaiters.Until(() => complete || gestureTask.IsCompleted);
                    }
                    else
                    {
                        ability.OnTrigger.Send(args);
                    }
                }
                triggerReceipt.Dispose();
                endReceipt.Dispose();
            }
            
            if (FaceTarget) caster.Get<Character>()?.StopFacingLocation();
        }

        // ※  Virtual Methods: --------------------------------------------------------------------------------------|
        // ※  Private Methods: --------------------------------------------------------------------------------------|
        
        private async Task MoveToTarget(ExtendedArgs args)
        {
            var ability = args.Get<RuntimeAbility>();
            var character = ability.Caster.Get<Character>();

            if (character == null)
            {
                return;
            }
            
            character.Motion?.MoveToDirection(Vector3.zero, Space.World, 0);
            
            if (ability.IsInRange(args))
            {
                ability.OnStatus.Send("Already in range");
                return;
            }
            
            ability.Caster.Get<Character>().Motion?.MoveToLocation
            (
                args.Get<Target>().GetLocation(),
                0,
                null,
                0
            );
            
            ability.OnStatus.Send("Moving to location");
            
            await Awaiters.Until(() => ability.IsInRange(args) || ability.IsCanceled);
            
            ability.OnStatus.Send("Reached destination");
            
            ability.Caster.Get<Character>().Motion?.MoveToDirection(Vector3.zero, Space.World, 0);
        }
        
        //============================================================================================================||
    }
}