using System;

namespace AssemblyConfigurationDetector
{
    [Flags()]
    public enum CorFlags : uint
    {
        F32BitsRequired = 2,
        ILOnly = 1,
        StrongNameSigned = 8,
        TrackDebugData = 0x10000
    }
}
