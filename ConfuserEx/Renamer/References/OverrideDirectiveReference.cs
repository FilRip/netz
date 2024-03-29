﻿using System.Linq;

using Confuser.Core;

using dnlib.DotNet;

namespace Confuser.Renamer.References
{
    internal class OverrideDirectiveReference : INameReference<MethodDef>
    {
        readonly VTableSlot baseSlot;
        readonly VTableSlot thisSlot;

        public OverrideDirectiveReference(VTableSlot thisSlot, VTableSlot baseSlot)
        {
            this.thisSlot = thisSlot;
            this.baseSlot = baseSlot;
        }

        void AddImportReference(ConfuserContext context, INameService service, ModuleDef module, MethodDef method, MemberRef methodRef)
        {
            if (method.Module != module && context.Modules.Contains((ModuleDefMD)method.Module))
            {
                var declType = (TypeRef)methodRef.DeclaringType.ScopeType;
                service.AddReference(method.DeclaringType, new TypeRefReference(declType, method.DeclaringType));
                service.AddReference(method, new MemberRefReference(methodRef, method));

                var typeRefs = methodRef.MethodSig.Params.SelectMany(param => param.FindTypeRefs()).ToList();
                typeRefs.AddRange(methodRef.MethodSig.RetType.FindTypeRefs());
                foreach (var typeRef in typeRefs)
                {
                    var def = typeRef.ResolveTypeDefThrow();
                    if (def.Module != module && context.Modules.Contains((ModuleDefMD)def.Module))
                        service.AddReference(def, new TypeRefReference((TypeRef)typeRef, def));
                }
            }
        }

        public bool UpdateNameReference(ConfuserContext context, INameService service)
        {
            MethodDef method = thisSlot.MethodDef;

            IMethod target;
            if (baseSlot.MethodDefDeclType is GenericInstSig declType)
            {
                MemberRef targetRef = new MemberRefUser(method.Module, baseSlot.MethodDef.Name, baseSlot.MethodDef.MethodSig, declType.ToTypeDefOrRef());
                targetRef = new Importer(method.Module, ImporterOptions.TryToUseTypeDefs).Import(targetRef);
                service.AddReference(baseSlot.MethodDef, new MemberRefReference(targetRef, baseSlot.MethodDef));

                target = targetRef;
            }
            else
            {
                target = baseSlot.MethodDef;
                if (target.Module != method.Module)
                {
                    target = new Importer(method.Module, ImporterOptions.TryToUseTypeDefs).Import(baseSlot.MethodDef);
                    if (target is MemberRef memberRef)
                        service.AddReference(baseSlot.MethodDef, new MemberRefReference(memberRef, baseSlot.MethodDef));
                }
            }

            target.MethodSig = new Importer(method.Module, ImporterOptions.TryToUseTypeDefs).Import(method.MethodSig);
            if (target is MemberRef member)
                AddImportReference(context, service, method.Module, baseSlot.MethodDef, member);

            if (method.Overrides.Any(impl =>
                                     new SigComparer().Equals(impl.MethodDeclaration.MethodSig, target.MethodSig) &&
                                     new SigComparer().Equals(impl.MethodDeclaration.DeclaringType.ResolveTypeDef(), target.DeclaringType.ResolveTypeDef())))
                return true;

            method.Overrides.Add(new MethodOverride(method, (IMethodDefOrRef)target));

            return true;
        }

        public bool ShouldCancelRename()
        {
            return baseSlot.MethodDefDeclType is GenericInstSig && thisSlot.MethodDef.Module.IsClr20;
        }
    }
}
