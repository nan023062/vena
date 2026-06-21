// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    public interface IBlocklyLogger
    {
        void Debug(string message);
        void Warning(string message);
        void Error(string message);
    }
}
