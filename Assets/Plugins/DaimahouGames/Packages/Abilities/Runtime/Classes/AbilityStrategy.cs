﻿using System;
using DaimahouGames.Runtime.Core.Common;
using GameCreator.Runtime.Common;
using DaimahouGames.Runtime.Core;
using UnityEngine;

namespace DaimahouGames.Runtime.Abilities
{
    [Serializable]
    public abstract class AbilityStrategy : IGenericItem
    {
        //============================================================================================================||
        // -----------------------------------------------------------------------------------------------------------|
        
        #region EditorInfo
        [SerializeField] private bool m_IsExpanded;
        public virtual string Title => "feature - (no name)";
        public virtual Color Color => ColorTheme.Get(ColorTheme.Type.TextNormal);
        public bool IsExpanded { get => m_IsExpanded; set => m_IsExpanded = value; }
        public virtual string[] Info { get; } = Array.Empty<string>();
        #endregion
        
        // ※  Variables: -------------------------------------------------------------------------------------------|※
        // ---|　Exposed State ----------------------------------------------------------------------------------->|
        // ---|　Internal State ---------------------------------------------------------------------------------->|
        // ---|　Dependencies ------------------------------------------------------------------------------------>|
        // ---|　Properties -------------------------------------------------------------------------------------->|
        // ---|　Events ------------------------------------------------------------------------------------------>|
        //============================================================================================================||
        // ※  Constructors: ----------------------------------------------------------------------------------------|※
        // ※  Initialization Methods: ------------------------------------------------------------------------------|※
        // ※  Public Methods: --------------------------------------------------------------------------------------|※
        // ※  Virtual Methods: -------------------------------------------------------------------------------------|※
        // ※  Protected Methods: -----------------------------------------------------------------------------------|※
        // ※  Private Methods: -------------------------------------------------------------------------------------|※
        //============================================================================================================||
    }
}