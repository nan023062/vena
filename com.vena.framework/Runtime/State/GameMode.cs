// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Framework
{
    /// <summary>
    /// game mode 作为GameWorld的子状态机
    /// 所有游戏的模式都继承自此类
    /// </summary>
    public abstract class GameMode : BasedStackGameState
    {
        protected sealed override void StateFocus()
        {
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
        }
        
        protected override void StateDestroy()
        {
        }
        
        protected abstract void OnEnter();
        
        protected abstract void OnUpdate(float time, float deltaTime);
        
        protected abstract void OnExit();
        
     
    }
}