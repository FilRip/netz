namespace netz.AssemblyConfigurationDetector
{
    public enum SubSystem
    {
        ERREUR = -1,
        NATIVE = 1,
        WINDOWS_GUI = 2,
        WINDOWS_CONSOLE = 3,
        OS_2_CONSOLE = 5,
        POSIX_CONSOLE = 7,
        NATIVE_WINDOWS = 8,
        WINDOWS_CE_GUI = 9,
        EFI_APPLICATION = 10,
        EFI_BOOT_SERVICE_DRIVER = 11,
        EFI_RUNTIME_DRIVER = 12,
        EFI_ROM = 13,
        XBOX = 14,
        WINDOWS_BOOT_APPLICATION = 16,
    }
}
