using GameCreator.Editor.Common;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Melee;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameCreator.Editor.Melee
{
    [CustomEditor(typeof(Skill))]
    public class SkillEditor : UnityEditor.Editor
    {
        private const string ERR_ANIM = "A Skill requires an Animation Clip";

        // MEMBERS: -------------------------------------------------------------------------------

        private VisualElement m_Root;
        
        private Button m_ButtonToggleStage;
        
        private Button m_ButtonCharacter;
        private ObjectField m_CharacterField;

        private MeleeSequenceTool m_SequenceTool;
        
        // INITIALIZERS: --------------------------------------------------------------------------

        private void OnEnable()
        {
            SkillConfigurationStage.EventOpenStage -= this.RefreshSkillState;
            SkillConfigurationStage.EventOpenStage += this.RefreshSkillState;
            
            SkillConfigurationStage.EventCloseStage -= this.RefreshSkillState;
            SkillConfigurationStage.EventCloseStage += this.RefreshSkillState;
        }

        private void OnDisable()
        {
            SkillConfigurationStage.EventOpenStage -= this.RefreshSkillState;
            SkillConfigurationStage.EventCloseStage -= this.RefreshSkillState;
        }

        [OnOpenAsset]
        public static bool OpenSkillExecute(int instanceID, int line)
        {
            Skill skill = EditorUtility.InstanceIDToObject(instanceID) as Skill;
            if (skill == null) return false;

            if (SkillConfigurationStage.InStage) StageUtility.GoToMainStage();
            Selection.activeObject = skill;
            
            string skillPath = AssetDatabase.GetAssetPath(skill);
            SkillConfigurationStage.EnterStage(skillPath);
            
            return true;
        }

        // INSPECTOR METHODS: ---------------------------------------------------------------------
        
        public override VisualElement CreateInspectorGUI()
        {
            this.m_Root = new VisualElement();
            
            StyleSheet[] styleSheets = StyleSheetUtils.Load();
            foreach (StyleSheet styleSheet in styleSheets) this.m_Root.styleSheets.Add(styleSheet);

            SerializedProperty title = this.serializedObject.FindProperty("m_Title");
            SerializedProperty description = this.serializedObject.FindProperty("m_Description");
            
            this.m_Root.Add(new PropertyField(title));
            this.m_Root.Add(new PropertyField(description));
            
            SerializedProperty icon = this.serializedObject.FindProperty("m_Icon");
            SerializedProperty color = this.serializedObject.FindProperty("m_Color");
            
            this.m_Root.Add(new PropertyField(icon));
            this.m_Root.Add(new PropertyField(color));

            SerializedProperty charge = this.serializedObject.FindProperty("m_Charge");
            SerializedProperty strike = this.serializedObject.FindProperty("m_Strike");
            SerializedProperty trail = this.serializedObject.FindProperty("m_Trail");
            SerializedProperty effects = this.serializedObject.FindProperty("m_Effects");
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(charge));
            this.m_Root.Add(new PropertyField(strike));
            this.m_Root.Add(new PropertyField(trail));
            this.m_Root.Add(new PropertyField(effects));

            SerializedProperty animation = this.serializedObject.FindProperty("m_Animation");
            SerializedProperty mask = this.serializedObject.FindProperty("m_Mask");

            SerializedProperty gravity = this.serializedObject.FindProperty("m_Gravity");
            SerializedProperty transitionIn = this.serializedObject.FindProperty("m_TransitionIn");
            SerializedProperty transitionOut = this.serializedObject.FindProperty("m_TransitionOut");
            
            ErrorMessage animationError = new ErrorMessage(ERR_ANIM);
            PropertyField animationField = new PropertyField(animation);
            PropertyField maskField = new PropertyField(mask);

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(animationError);
            this.m_Root.Add(animationField);
            this.m_Root.Add(maskField);

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(gravity));
            this.m_Root.Add(new PropertyField(transitionIn));
            this.m_Root.Add(new PropertyField(transitionOut));
            
            SerializedProperty motion = this.serializedObject.FindProperty("m_Motion");
            SerializedProperty syncReaction = this.serializedObject.FindProperty("m_SyncReaction");
            
            PropertyField motionField = new PropertyField(motion);
            PropertyField syncReactionField = new PropertyField(syncReaction);
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(motionField);
            this.m_Root.Add(syncReactionField);

            motionField.RegisterValueChangeCallback(changeEvent =>
            {
                MeleeMotion newValue = (MeleeMotion) changeEvent.changedProperty.enumValueIndex;
                syncReactionField.style.display = newValue == MeleeMotion.MotionWarp
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            });
            
            syncReactionField.style.display = motion.enumValueIndex == (int) MeleeMotion.MotionWarp
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            
            this.m_ButtonToggleStage = new Button(this.ToggleSkillMode)
            {
                style = { height = new Length(30f, LengthUnit.Pixel)}
            };

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(this.m_ButtonToggleStage);
            
            PadBox sequenceContent = new PadBox();
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(sequenceContent);
            
            this.m_CharacterField = new ObjectField(string.Empty)
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true,
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                }
            };

            this.m_ButtonCharacter = new Button(this.ChangeCharacter)
            {
                text = "Change Character",
                style =
                {
                    marginRight = 0,
                    marginTop = 0,
                    marginBottom = 0,
                }
            };

            HorizontalBox changeCharacterContent = new HorizontalBox(
                HorizontalBox.FlexMode.FirstGrows,
                this.m_CharacterField,
                this.m_ButtonCharacter
            );
            
            sequenceContent.Add(changeCharacterContent);

            SerializedProperty sequence = this.serializedObject
                .FindProperty("m_MeleeSequence")
                .FindPropertyRelative(RunMeleeSequenceDrawer.NAME_SEQUENCE);

            this.m_SequenceTool = new MeleeSequenceTool(sequence)
            {
                AnimationClip = animation.objectReferenceValue as AnimationClip
            };

            sequenceContent.Add(new SpaceSmall());
            sequenceContent.Add(this.m_SequenceTool);

            motionField.RegisterValueChangeCallback(changeEvent =>
            {
                bool showRootMotion = changeEvent.changedProperty.enumValueIndex == 1;
                bool showMotionWarping = changeEvent.changedProperty.enumValueIndex == 2;
                
                this.m_SequenceTool.ShowTrack(MeleeSequence.TRACK_ROOT_MOTION_POSITION, showRootMotion);
                this.m_SequenceTool.ShowTrack(MeleeSequence.TRACK_ROOT_MOTION_ROTATION, showRootMotion);
                this.m_SequenceTool.ShowTrack(MeleeSequence.TRACK_MOTION_WARPING, showMotionWarping);
            });
            
            bool showRootMotion = motion.enumValueIndex == 1;
            bool showMotionWarping = motion.enumValueIndex == 2;
            
            this.m_SequenceTool.ShowTrack(MeleeSequence.TRACK_ROOT_MOTION_POSITION, showRootMotion);
            this.m_SequenceTool.ShowTrack(MeleeSequence.TRACK_ROOT_MOTION_ROTATION, showRootMotion);
            this.m_SequenceTool.ShowTrack(MeleeSequence.TRACK_MOTION_WARPING, showMotionWarping);
            
            animationField.RegisterValueChangeCallback(changeEvent =>
            {
                Object newValue = changeEvent.changedProperty.objectReferenceValue;
                animationError.style.display = newValue == null
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
                
                this.m_SequenceTool.AnimationClip = newValue as AnimationClip;
            });
            
            animationError.style.display = animation.objectReferenceValue == null
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            SerializedProperty speedA = this.serializedObject.FindProperty("m_SpeedAnticipation");
            SerializedProperty speedB = this.serializedObject.FindProperty("m_SpeedStrike");
            SerializedProperty speedC = this.serializedObject.FindProperty("m_SpeedRecovery");

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(speedA));
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(speedB));
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(speedC));
            
            SerializedProperty poiseArmor = this.serializedObject.FindProperty("m_PoiseArmor");
            SerializedProperty poiseDamage = this.serializedObject.FindProperty("m_PoiseDamage");

            PadBox poiseBox = new PadBox();
            
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(poiseBox);
            poiseBox.Add(new PropertyField(poiseArmor));
            poiseBox.Add(new SpaceSmaller());
            poiseBox.Add(new PropertyField(poiseDamage));
            
            SerializedProperty power = this.serializedObject.FindProperty("m_Power");

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new PropertyField(power));

            SerializedProperty onStart = this.serializedObject.FindProperty("m_OnStart");
            SerializedProperty onFinish = this.serializedObject.FindProperty("m_OnFinish");
            SerializedProperty canHit = this.serializedObject.FindProperty("m_CanHit");
            SerializedProperty onHit = this.serializedObject.FindProperty("m_OnHit");

            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new LabelTitle("On Start:"));
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(onStart));
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new LabelTitle("On Finish:"));
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(onFinish));
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new LabelTitle("Can Hit:"));
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(canHit));
            this.m_Root.Add(new SpaceSmall());
            this.m_Root.Add(new LabelTitle("On Hit:"));
            this.m_Root.Add(new SpaceSmaller());
            this.m_Root.Add(new PropertyField(onHit));

            this.RefreshSkillState();
            
            return this.m_Root;
        }
        
        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void ChangeCharacter()
        {
            GameObject character = this.m_CharacterField.value as GameObject;
            SkillConfigurationStage.ChangeCharacter(character);
            
            if (SkillConfigurationStage.InStage)
            {
                this.m_SequenceTool.Target = SkillConfigurationStage.Stage.Animator != null
                    ? SkillConfigurationStage.Stage.Animator.gameObject
                    : null;
            }
        }

        private void ToggleSkillMode()
        {
            if (SkillConfigurationStage.InStage)
            {
                StageUtility.GoToMainStage();
                this.RefreshSkillState();
                
                this.m_SequenceTool.DisablePreview();
                return;
            }

            Skill skill = this.target as Skill;
            if (skill == null) return;

            string path = AssetDatabase.GetAssetPath(skill);
            SkillConfigurationStage.EnterStage(path);
            
            this.m_SequenceTool.Target = SkillConfigurationStage.Stage.Animator != null
                ? SkillConfigurationStage.Stage.Animator.gameObject
                : null; 
        }
        
        private void RefreshSkillState()
        {
            if (this.m_ButtonToggleStage == null) return;
            
            bool isSkillMode = SkillConfigurationStage.InStage;
            this.m_ButtonToggleStage.text = isSkillMode
                ? "Close Skill Mode" 
                : "Enter Skill Mode";

            Color borderColor = isSkillMode
                ? ColorTheme.Get(ColorTheme.Type.Green)
                : ColorTheme.Get(ColorTheme.Type.Dark);
            
            this.m_ButtonToggleStage.style.borderTopColor = borderColor;
            this.m_ButtonToggleStage.style.borderBottomColor = borderColor;
            this.m_ButtonToggleStage.style.borderLeftColor = borderColor;
            this.m_ButtonToggleStage.style.borderRightColor = borderColor;

            this.m_ButtonToggleStage.style.color = isSkillMode
                ? ColorTheme.Get(ColorTheme.Type.Green)
                : ColorTheme.Get(ColorTheme.Type.TextNormal);

            this.m_ButtonCharacter.SetEnabled(isSkillMode);
            this.m_CharacterField.SetEnabled(isSkillMode);
            this.m_SequenceTool.IsEnabled = isSkillMode;
            
            if (isSkillMode)
            {
                this.m_CharacterField.value = SkillConfigurationStage.CharacterReference;
                this.m_SequenceTool.Target = SkillConfigurationStage.Stage.Animator != null
                    ? SkillConfigurationStage.Stage.Animator.gameObject
                    : null; 
            }
        }
    }
}