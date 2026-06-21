// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    public interface IBlocklyPool
    {
        T Get<T>() where T : class, new();
        void Return(object instance);
    }
}
