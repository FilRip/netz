﻿using dnlib.DotNet;

namespace Confuser.Core.Services
{
    internal class RuntimeService : IRuntimeService
    {
        ModuleDef rtModule;

        /// <inheritdoc />
        public TypeDef GetRuntimeType(string fullName)
        {
            if (rtModule == null)
            {
                rtModule = ModuleDefMD.Load(typeof(RuntimeService).Assembly.Location, new ModuleCreationOptions() { TryToLoadPdbFromDisk = true });
                rtModule.EnableTypeDefFindCache = true;
            }
            return rtModule.Find(fullName, true);
        }
    }

    /// <summary>
    ///     Provides methods to obtain runtime library injection type.
    /// </summary>
    public interface IRuntimeService
    {
        /// <summary>
        ///     Gets the specified runtime type for injection.
        /// </summary>
        /// <param name="fullName">The full name of the runtime type.</param>
        /// <returns>The requested runtime type.</returns>
        TypeDef GetRuntimeType(string fullName);
    }
}
