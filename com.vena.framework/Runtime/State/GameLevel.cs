// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Framework
{
    /// <summary>
    ///  game level interface, used for game state machine
    /// </summary>
    public abstract class GameLevel : BasedStackGameMachine
    {
        public readonly string name;
        
        public GameMode mode => current as GameMode;
        
        protected GameLevel()
        {
            name = GetType().Name;
        }
        
        protected GameLevel(string name)
        {
            this.name = name;
        }
        
        protected sealed override void StateFocus()
        {
            // init modules
            base.StateFocus();
            
            // enter world
            OnEnter(); 
        }
        
        protected sealed override void StateUpdate(float time, float deltaTime)
        {
            base.StateUpdate(time, deltaTime);
            
            OnUpdate(time, deltaTime);
        }

        protected sealed override void StateUnFocus()
        {
            OnExit();
            
            base.StateUnFocus();
        }
        
        protected abstract void OnEnter();
        
        protected abstract void OnUpdate(float time, float deltaTime );
        
        protected abstract void OnExit();
    }
}