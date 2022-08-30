using System.Collections.Generic;
using System.Linq;

using Confuser.Core;

using dnlib.DotNet;

namespace Confuser.Renamer
{
    internal class RenamePhase : ProtectionPhase
    {
        public RenamePhase(NameProtection parent)
            : base(parent) { }

        public override ProtectionTargets Targets
        {
            get { return ProtectionTargets.AllDefinitions; }
        }

        public override string Name
        {
            get { return "Renaming"; }
        }

        protected internal override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {
            var service = (NameService)context.Registry.GetService<INameService>();

            context.Logger.Debug("Renaming...");
            foreach (IRenamer renamer in service.Renamers)
            {
                foreach (IDnlibDef def in parameters.Targets)
                    renamer.PreRename(context, service, parameters, def);
                context.CheckCancellation();
            }

            var targets = parameters.Targets.ToList();
            service.GetRandom().Shuffle(targets);
            var pdbDocs = new HashSet<string>();
            foreach (IDnlibDef def in targets.WithProgress(context.Logger))
            {
                if (def is ModuleDef module && parameters.GetParameter(context, def, "rickroll", false))
                    RickRoller.CommenceRickroll(context, module);

                bool canRename = service.CanRename(def);
                RenameMode mode = service.GetRenameMode(def);

                if (def is MethodDef method)
                {
                    if ((canRename || method.IsConstructor) && parameters.GetParameter(context, def, "renameArgs", true))
                    {
                        foreach (ParamDef param in ((MethodDef)def).ParamDefs)
                            param.Name = null;
                    }

                    if (parameters.GetParameter(context, def, "renPdb", false) && method.HasBody)
                    {
                        foreach (var instr in method.Body.Instructions)
                        {
                            if (instr.SequencePoint != null && !pdbDocs.Contains(instr.SequencePoint.Document.Url))
                            {
                                instr.SequencePoint.Document.Url = service.ObfuscateName(instr.SequencePoint.Document.Url, mode);
                                pdbDocs.Add(instr.SequencePoint.Document.Url);
                            }
                        }
                        foreach (var local in method.Body.Variables)
                        {
                            if (!string.IsNullOrEmpty(local.Name))
                                local.Name = service.ObfuscateName(local.Name, mode);
                        }
                        method.Body.PdbMethod = null;
                    }
                }

                if (!canRename)
                    continue;

                IList<INameReference> references = service.GetReferences(def);
                bool cancel = false;
                foreach (INameReference refer in references)
                {
                    cancel |= refer.ShouldCancelRename();
                    if (cancel) break;
                }
                if (cancel)
                    continue;

                if (def is TypeDef typeDef)
                {
                    if (parameters.GetParameter(context, def, "flatten", true))
                    {
                        typeDef.Name = service.ObfuscateName(typeDef.FullName, mode);
                        typeDef.Namespace = "";
                    }
                    else
                    {
                        typeDef.Namespace = service.ObfuscateName(typeDef.Namespace, mode);
                        typeDef.Name = service.ObfuscateName(typeDef.Name, mode);
                    }
                    foreach (var param in typeDef.GenericParameters)
                        param.Name = ((char)(param.Number + 1)).ToString();
                }
                else if (def is MethodDef methodDef)
                {
                    foreach (var param in methodDef.GenericParameters)
                        param.Name = ((char)(param.Number + 1)).ToString();

                    def.Name = service.ObfuscateName(def.Name, mode);
                }
                else
                    def.Name = service.ObfuscateName(def.Name, mode);

                foreach (INameReference refer in references.ToList())
                {
                    if (!refer.UpdateNameReference(context, service))
                    {
                        context.Logger.ErrorFormat("Failed to update name reference on '{0}'.", def);
                        throw new ConfuserException(null);
                    }
                }
                context.CheckCancellation();
            }
        }
    }
}
