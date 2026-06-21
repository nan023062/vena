// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    public interface IBlocklyHost
    {
        IBlocklyLogger Logger { get; }
        IBlocklyNodeFactory NodeFactory { get; }
        IBlocklyPool Pool { get; }
        IBlocklySerializer Serializer { get; }
        IBlocklyVariableStorageFactory VariableStorageFactory { get; }
    }
}
