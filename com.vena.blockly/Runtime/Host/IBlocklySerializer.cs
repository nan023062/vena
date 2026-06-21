// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

namespace Vena.Blockly
{

    public interface IBlocklySerializer
    {
        int ReadInt32();
        void WriteInt32(int value);
        string ReadString();
        void WriteString(string value);
        bool ReadBoolean();
        void WriteBoolean(bool value);
    }
}
