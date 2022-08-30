using System;
using System.Collections.Generic;

using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.W32Resources;

namespace Confuser.Core
{
    /// <summary>
    ///     The listener of module writer event.
    /// </summary>
    public class ModuleWriterListener : ModuleWriterBase
    {
        readonly ModuleWriterOptions _options;

        public ModuleWriterListener(ModuleDef module) : base()
        {
            _options = new ModuleWriterOptions(module);
            TheOptions.WriterEvent += TheOptions_WriterEvent;
        }

        private void TheOptions_WriterEvent(object sender, ModuleWriterEventArgs e)
        {
            if (e.Event == ModuleWriterEvent.PESectionsCreated)
                NativeEraser.Erase(e.Writer as NativeModuleWriter, e.Writer.Module as ModuleDefMD);
        }

        public override ModuleWriterOptionsBase TheOptions => _options;

        public override List<PESection> Sections { get; }

        public override PESection TextSection { get; }

        public override PESection RsrcSection { get; }

        public override ModuleDef Module { get; }

        protected override Win32Resources GetWin32Resources()
        {
            return null;
        }

        protected override long WriteImpl()
        {
            return 0;
        }
    }
}
