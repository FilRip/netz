using System.Collections.Generic;

#pragma warning disable 1591	// XML doc comments

namespace dnlib.DotNet.MD
{
    /// <summary>
    /// Equality comparer for all raw rows
    /// </summary>
    public sealed class RawRowEqualityComparer : IEqualityComparer<RawModuleRow>,
        IEqualityComparer<RawTypeRefRow>, IEqualityComparer<RawTypeDefRow>,
        IEqualityComparer<RawFieldPtrRow>, IEqualityComparer<RawFieldRow>,
        IEqualityComparer<RawMethodPtrRow>, IEqualityComparer<RawMethodRow>,
        IEqualityComparer<RawParamPtrRow>, IEqualityComparer<RawParamRow>,
        IEqualityComparer<RawInterfaceImplRow>, IEqualityComparer<RawMemberRefRow>,
        IEqualityComparer<RawConstantRow>, IEqualityComparer<RawCustomAttributeRow>,
        IEqualityComparer<RawFieldMarshalRow>, IEqualityComparer<RawDeclSecurityRow>,
        IEqualityComparer<RawClassLayoutRow>, IEqualityComparer<RawFieldLayoutRow>,
        IEqualityComparer<RawStandAloneSigRow>, IEqualityComparer<RawEventMapRow>,
        IEqualityComparer<RawEventPtrRow>, IEqualityComparer<RawEventRow>,
        IEqualityComparer<RawPropertyMapRow>, IEqualityComparer<RawPropertyPtrRow>,
        IEqualityComparer<RawPropertyRow>, IEqualityComparer<RawMethodSemanticsRow>,
        IEqualityComparer<RawMethodImplRow>, IEqualityComparer<RawModuleRefRow>,
        IEqualityComparer<RawTypeSpecRow>, IEqualityComparer<RawImplMapRow>,
        IEqualityComparer<RawFieldRVARow>, IEqualityComparer<RawENCLogRow>,
        IEqualityComparer<RawENCMapRow>, IEqualityComparer<RawAssemblyRow>,
        IEqualityComparer<RawAssemblyProcessorRow>, IEqualityComparer<RawAssemblyOSRow>,
        IEqualityComparer<RawAssemblyRefRow>, IEqualityComparer<RawAssemblyRefProcessorRow>,
        IEqualityComparer<RawAssemblyRefOSRow>, IEqualityComparer<RawFileRow>,
        IEqualityComparer<RawExportedTypeRow>, IEqualityComparer<RawManifestResourceRow>,
        IEqualityComparer<RawNestedClassRow>, IEqualityComparer<RawGenericParamRow>,
        IEqualityComparer<RawMethodSpecRow>, IEqualityComparer<RawGenericParamConstraintRow>,
        IEqualityComparer<RawDocumentRow>, IEqualityComparer<RawMethodDebugInformationRow>,
        IEqualityComparer<RawLocalScopeRow>, IEqualityComparer<RawLocalVariableRow>,
        IEqualityComparer<RawLocalConstantRow>, IEqualityComparer<RawImportScopeRow>,
        IEqualityComparer<RawStateMachineMethodRow>, IEqualityComparer<RawCustomDebugInformationRow>
    {

        /// <summary>
        /// Default instance
        /// </summary>
        public static readonly RawRowEqualityComparer Instance = new RawRowEqualityComparer();

        static int Rol(uint val, int shift) => (int)((val << shift) | (val >> (32 - shift)));

        public bool Equals(RawModuleRow x, RawModuleRow y) =>
            x.Generation == y.Generation &&
            x.Name == y.Name &&
            x.Mvid == y.Mvid &&
            x.EncId == y.EncId &&
            x.EncBaseId == y.EncBaseId;

        public int GetHashCode(RawModuleRow obj) =>
            obj.Generation +
            Rol(obj.Name, 3) +
            Rol(obj.Mvid, 7) +
            Rol(obj.EncId, 11) +
            Rol(obj.EncBaseId, 15);

        public bool Equals(RawTypeRefRow x, RawTypeRefRow y) =>
            x.ResolutionScope == y.ResolutionScope &&
            x.Name == y.Name &&
            x.Namespace == y.Namespace;

        public int GetHashCode(RawTypeRefRow obj) =>
            (int)obj.ResolutionScope +
            Rol(obj.Name, 3) +
            Rol(obj.Namespace, 7);

        public bool Equals(RawTypeDefRow x, RawTypeDefRow y) =>
            x.Flags == y.Flags &&
            x.Name == y.Name &&
            x.Namespace == y.Namespace &&
            x.Extends == y.Extends &&
            x.FieldList == y.FieldList &&
            x.MethodList == y.MethodList;

        public int GetHashCode(RawTypeDefRow obj) =>
            (int)obj.Flags +
            Rol(obj.Name, 3) +
            Rol(obj.Namespace, 7) +
            Rol(obj.Extends, 11) +
            Rol(obj.FieldList, 15) +
            Rol(obj.MethodList, 19);

        public bool Equals(RawFieldPtrRow x, RawFieldPtrRow y) => x.Field == y.Field;

        public int GetHashCode(RawFieldPtrRow obj) => (int)obj.Field;

        public bool Equals(RawFieldRow x, RawFieldRow y) =>
            x.Flags == y.Flags &&
            x.Name == y.Name &&
            x.Signature == y.Signature;

        public int GetHashCode(RawFieldRow obj) =>
            obj.Flags +
            Rol(obj.Name, 3) +
            Rol(obj.Signature, 7);

        public bool Equals(RawMethodPtrRow x, RawMethodPtrRow y) => x.Method == y.Method;

        public int GetHashCode(RawMethodPtrRow obj) => (int)obj.Method;

        public bool Equals(RawMethodRow x, RawMethodRow y) =>
            x.RVA == y.RVA &&
            x.ImplFlags == y.ImplFlags &&
            x.Flags == y.Flags &&
            x.Name == y.Name &&
            x.Signature == y.Signature &&
            x.ParamList == y.ParamList;

        public int GetHashCode(RawMethodRow obj) =>
            (int)obj.RVA +
            Rol(obj.ImplFlags, 3) +
            Rol(obj.Flags, 7) +
            Rol(obj.Name, 11) +
            Rol(obj.Signature, 15) +
            Rol(obj.ParamList, 19);

        public bool Equals(RawParamPtrRow x, RawParamPtrRow y) => x.Param == y.Param;

        public int GetHashCode(RawParamPtrRow obj) => (int)obj.Param;

        public bool Equals(RawParamRow x, RawParamRow y) =>
            x.Flags == y.Flags &&
            x.Sequence == y.Sequence &&
            x.Name == y.Name;

        public int GetHashCode(RawParamRow obj) =>
            obj.Flags +
            Rol(obj.Sequence, 3) +
            Rol(obj.Name, 7);

        public bool Equals(RawInterfaceImplRow x, RawInterfaceImplRow y) =>
            x.Class == y.Class &&
            x.Interface == y.Interface;

        public int GetHashCode(RawInterfaceImplRow obj) =>
            (int)obj.Class +
            Rol(obj.Interface, 3);

        public bool Equals(RawMemberRefRow x, RawMemberRefRow y) =>
            x.Class == y.Class &&
            x.Name == y.Name &&
            x.Signature == y.Signature;

        public int GetHashCode(RawMemberRefRow obj) =>
            (int)obj.Class +
            Rol(obj.Name, 3) +
            Rol(obj.Signature, 7);

        public bool Equals(RawConstantRow x, RawConstantRow y) =>
            x.Type == y.Type &&
            x.Padding == y.Padding &&
            x.Parent == y.Parent &&
            x.Value == y.Value;

        public int GetHashCode(RawConstantRow obj) =>
            obj.Type +
            Rol(obj.Padding, 3) +
            Rol(obj.Parent, 7) +
            Rol(obj.Value, 11);

        public bool Equals(RawCustomAttributeRow x, RawCustomAttributeRow y) =>
            x.Parent == y.Parent &&
            x.Type == y.Type &&
            x.Value == y.Value;

        public int GetHashCode(RawCustomAttributeRow obj) =>
            (int)obj.Parent +
            Rol(obj.Type, 3) +
            Rol(obj.Value, 7);

        public bool Equals(RawFieldMarshalRow x, RawFieldMarshalRow y) =>
            x.Parent == y.Parent &&
            x.NativeType == y.NativeType;

        public int GetHashCode(RawFieldMarshalRow obj) =>
            (int)obj.Parent +
            Rol(obj.NativeType, 3);

        public bool Equals(RawDeclSecurityRow x, RawDeclSecurityRow y) =>
            x.Action == y.Action &&
            x.Parent == y.Parent &&
            x.PermissionSet == y.PermissionSet;

        public int GetHashCode(RawDeclSecurityRow obj) =>
            obj.Action +
            Rol(obj.Parent, 3) +
            Rol(obj.PermissionSet, 7);

        public bool Equals(RawClassLayoutRow x, RawClassLayoutRow y) =>
            x.PackingSize == y.PackingSize &&
            x.ClassSize == y.ClassSize &&
            x.Parent == y.Parent;

        public int GetHashCode(RawClassLayoutRow obj) =>
            obj.PackingSize +
            Rol(obj.ClassSize, 3) +
            Rol(obj.Parent, 7);

        public bool Equals(RawFieldLayoutRow x, RawFieldLayoutRow y) =>
            x.OffSet == y.OffSet &&
            x.Field == y.Field;

        public int GetHashCode(RawFieldLayoutRow obj) =>
            (int)obj.OffSet +
            Rol(obj.Field, 3);

        public bool Equals(RawStandAloneSigRow x, RawStandAloneSigRow y) => x.Signature == y.Signature;

        public int GetHashCode(RawStandAloneSigRow obj) => (int)obj.Signature;

        public bool Equals(RawEventMapRow x, RawEventMapRow y) =>
            x.Parent == y.Parent &&
            x.EventList == y.EventList;

        public int GetHashCode(RawEventMapRow obj) =>
            (int)obj.Parent +
            Rol(obj.EventList, 3);

        public bool Equals(RawEventPtrRow x, RawEventPtrRow y) => x.Event == y.Event;

        public int GetHashCode(RawEventPtrRow obj) => (int)obj.Event;

        public bool Equals(RawEventRow x, RawEventRow y) =>
            x.EventFlags == y.EventFlags &&
            x.Name == y.Name &&
            x.EventType == y.EventType;

        public int GetHashCode(RawEventRow obj) =>
            obj.EventFlags +
            Rol(obj.Name, 3) +
            Rol(obj.EventType, 7);

        public bool Equals(RawPropertyMapRow x, RawPropertyMapRow y) =>
            x.Parent == y.Parent &&
            x.PropertyList == y.PropertyList;

        public int GetHashCode(RawPropertyMapRow obj) =>
            (int)obj.Parent +
            Rol(obj.PropertyList, 3);

        public bool Equals(RawPropertyPtrRow x, RawPropertyPtrRow y) => x.Property == y.Property;

        public int GetHashCode(RawPropertyPtrRow obj) => (int)obj.Property;

        public bool Equals(RawPropertyRow x, RawPropertyRow y) =>
            x.PropFlags == y.PropFlags &&
            x.Name == y.Name &&
            x.Type == y.Type;

        public int GetHashCode(RawPropertyRow obj) =>
            obj.PropFlags +
            Rol(obj.Name, 3) +
            Rol(obj.Type, 7);

        public bool Equals(RawMethodSemanticsRow x, RawMethodSemanticsRow y) =>
            x.Semantic == y.Semantic &&
            x.Method == y.Method &&
            x.Association == y.Association;

        public int GetHashCode(RawMethodSemanticsRow obj) =>
            obj.Semantic +
            Rol(obj.Method, 3) +
            Rol(obj.Association, 7);

        public bool Equals(RawMethodImplRow x, RawMethodImplRow y) =>
            x.Class == y.Class &&
            x.MethodBody == y.MethodBody &&
            x.MethodDeclaration == y.MethodDeclaration;

        public int GetHashCode(RawMethodImplRow obj) =>
            (int)obj.Class +
            Rol(obj.MethodBody, 3) +
            Rol(obj.MethodDeclaration, 7);

        public bool Equals(RawModuleRefRow x, RawModuleRefRow y) => x.Name == y.Name;

        public int GetHashCode(RawModuleRefRow obj) => (int)obj.Name;

        public bool Equals(RawTypeSpecRow x, RawTypeSpecRow y) => x.Signature == y.Signature;

        public int GetHashCode(RawTypeSpecRow obj) => (int)obj.Signature;

        public bool Equals(RawImplMapRow x, RawImplMapRow y) =>
            x.MappingFlags == y.MappingFlags &&
            x.MemberForwarded == y.MemberForwarded &&
            x.ImportName == y.ImportName &&
            x.ImportScope == y.ImportScope;

        public int GetHashCode(RawImplMapRow obj) =>
            obj.MappingFlags +
            Rol(obj.MemberForwarded, 3) +
            Rol(obj.ImportName, 7) +
            Rol(obj.ImportScope, 11);

        public bool Equals(RawFieldRVARow x, RawFieldRVARow y) =>
            x.RVA == y.RVA &&
            x.Field == y.Field;

        public int GetHashCode(RawFieldRVARow obj) =>
            (int)obj.RVA +
            Rol(obj.Field, 3);

        public bool Equals(RawENCLogRow x, RawENCLogRow y) =>
            x.Token == y.Token &&
            x.FuncCode == y.FuncCode;

        public int GetHashCode(RawENCLogRow obj) =>
            (int)obj.Token +
            Rol(obj.FuncCode, 3);

        public bool Equals(RawENCMapRow x, RawENCMapRow y) => x.Token == y.Token;

        public int GetHashCode(RawENCMapRow obj) => (int)obj.Token;

        public bool Equals(RawAssemblyRow x, RawAssemblyRow y) =>
            x.HashAlgId == y.HashAlgId &&
            x.MajorVersion == y.MajorVersion &&
            x.MinorVersion == y.MinorVersion &&
            x.BuildNumber == y.BuildNumber &&
            x.RevisionNumber == y.RevisionNumber &&
            x.Flags == y.Flags &&
            x.PublicKey == y.PublicKey &&
            x.Name == y.Name &&
            x.Locale == y.Locale;

        public int GetHashCode(RawAssemblyRow obj) =>
            (int)obj.HashAlgId +
            Rol(obj.MajorVersion, 3) +
            Rol(obj.MinorVersion, 7) +
            Rol(obj.BuildNumber, 11) +
            Rol(obj.RevisionNumber, 15) +
            Rol(obj.Flags, 19) +
            Rol(obj.PublicKey, 23) +
            Rol(obj.Name, 27) +
            Rol(obj.Locale, 31);

        public bool Equals(RawAssemblyProcessorRow x, RawAssemblyProcessorRow y) => x.Processor == y.Processor;

        public int GetHashCode(RawAssemblyProcessorRow obj) => (int)obj.Processor;

        public bool Equals(RawAssemblyOSRow x, RawAssemblyOSRow y) =>
            x.OSPlatformId == y.OSPlatformId &&
            x.OSMajorVersion == y.OSMajorVersion &&
            x.OSMinorVersion == y.OSMinorVersion;

        public int GetHashCode(RawAssemblyOSRow obj) =>
            (int)obj.OSPlatformId +
            Rol(obj.OSMajorVersion, 3) +
            Rol(obj.OSMinorVersion, 7);

        public bool Equals(RawAssemblyRefRow x, RawAssemblyRefRow y) =>
            x.MajorVersion == y.MajorVersion &&
            x.MinorVersion == y.MinorVersion &&
            x.BuildNumber == y.BuildNumber &&
            x.RevisionNumber == y.RevisionNumber &&
            x.Flags == y.Flags &&
            x.PublicKeyOrToken == y.PublicKeyOrToken &&
            x.Name == y.Name &&
            x.Locale == y.Locale &&
            x.HashValue == y.HashValue;

        public int GetHashCode(RawAssemblyRefRow obj) =>
            obj.MajorVersion +
            Rol(obj.MinorVersion, 3) +
            Rol(obj.BuildNumber, 7) +
            Rol(obj.RevisionNumber, 11) +
            Rol(obj.Flags, 15) +
            Rol(obj.PublicKeyOrToken, 19) +
            Rol(obj.Name, 23) +
            Rol(obj.Locale, 27) +
            Rol(obj.HashValue, 31);

        public bool Equals(RawAssemblyRefProcessorRow x, RawAssemblyRefProcessorRow y) =>
            x.Processor == y.Processor &&
            x.AssemblyRef == y.AssemblyRef;

        public int GetHashCode(RawAssemblyRefProcessorRow obj) =>
            (int)obj.Processor +
            Rol(obj.AssemblyRef, 3);

        public bool Equals(RawAssemblyRefOSRow x, RawAssemblyRefOSRow y) =>
            x.OSPlatformId == y.OSPlatformId &&
            x.OSMajorVersion == y.OSMajorVersion &&
            x.OSMinorVersion == y.OSMinorVersion &&
            x.AssemblyRef == y.AssemblyRef;

        public int GetHashCode(RawAssemblyRefOSRow obj) =>
            (int)obj.OSPlatformId +
            Rol(obj.OSMajorVersion, 3) +
            Rol(obj.OSMinorVersion, 7) +
            Rol(obj.AssemblyRef, 11);

        public bool Equals(RawFileRow x, RawFileRow y) =>
            x.Flags == y.Flags &&
            x.Name == y.Name &&
            x.HashValue == y.HashValue;

        public int GetHashCode(RawFileRow obj) =>
            (int)obj.Flags +
            Rol(obj.Name, 3) +
            Rol(obj.HashValue, 7);

        public bool Equals(RawExportedTypeRow x, RawExportedTypeRow y) =>
            x.Flags == y.Flags &&
            x.TypeDefId == y.TypeDefId &&
            x.TypeName == y.TypeName &&
            x.TypeNamespace == y.TypeNamespace &&
            x.Implementation == y.Implementation;

        public int GetHashCode(RawExportedTypeRow obj) =>
            (int)obj.Flags +
            Rol(obj.TypeDefId, 3) +
            Rol(obj.TypeName, 7) +
            Rol(obj.TypeNamespace, 11) +
            Rol(obj.Implementation, 15);

        public bool Equals(RawManifestResourceRow x, RawManifestResourceRow y) =>
            x.Offset == y.Offset &&
            x.Flags == y.Flags &&
            x.Name == y.Name &&
            x.Implementation == y.Implementation;

        public int GetHashCode(RawManifestResourceRow obj) =>
            (int)obj.Offset +
            Rol(obj.Flags, 3) +
            Rol(obj.Name, 7) +
            Rol(obj.Implementation, 11);

        public bool Equals(RawNestedClassRow x, RawNestedClassRow y) =>
            x.NestedClass == y.NestedClass &&
            x.EnclosingClass == y.EnclosingClass;

        public int GetHashCode(RawNestedClassRow obj) =>
            (int)obj.NestedClass +
            Rol(obj.EnclosingClass, 3);

        public bool Equals(RawGenericParamRow x, RawGenericParamRow y) =>
            x.Number == y.Number &&
            x.Flags == y.Flags &&
            x.Owner == y.Owner &&
            x.Name == y.Name &&
            x.Kind == y.Kind;

        public int GetHashCode(RawGenericParamRow obj) =>
            obj.Number +
            Rol(obj.Flags, 3) +
            Rol(obj.Owner, 7) +
            Rol(obj.Name, 11) +
            Rol(obj.Kind, 15);

        public bool Equals(RawMethodSpecRow x, RawMethodSpecRow y) =>
            x.Method == y.Method &&
            x.Instantiation == y.Instantiation;

        public int GetHashCode(RawMethodSpecRow obj) =>
            (int)obj.Method +
            Rol(obj.Instantiation, 3);

        public bool Equals(RawGenericParamConstraintRow x, RawGenericParamConstraintRow y) =>
            x.Owner == y.Owner &&
            x.Constraint == y.Constraint;

        public int GetHashCode(RawGenericParamConstraintRow obj) =>
            (int)obj.Owner +
            Rol(obj.Constraint, 3);

        public bool Equals(RawDocumentRow x, RawDocumentRow y) =>
            x.Name == y.Name &&
            x.HashAlgorithm == y.HashAlgorithm &&
            x.Hash == y.Hash &&
            x.Language == y.Language;

        public int GetHashCode(RawDocumentRow obj) =>
            (int)obj.Name +
            Rol(obj.HashAlgorithm, 3) +
            Rol(obj.Hash, 7) +
            Rol(obj.Language, 11);

        public bool Equals(RawMethodDebugInformationRow x, RawMethodDebugInformationRow y) =>
            x.Document == y.Document &&
            x.SequencePoints == y.SequencePoints;

        public int GetHashCode(RawMethodDebugInformationRow obj) =>
            (int)obj.Document +
            Rol(obj.SequencePoints, 3);

        public bool Equals(RawLocalScopeRow x, RawLocalScopeRow y) =>
            x.Method == y.Method &&
            x.ImportScope == y.ImportScope &&
            x.VariableList == y.VariableList &&
            x.ConstantList == y.ConstantList &&
            x.StartOffset == y.StartOffset &&
            x.Length == y.Length;

        public int GetHashCode(RawLocalScopeRow obj) =>
            (int)obj.Method +
            Rol(obj.ImportScope, 3) +
            Rol(obj.VariableList, 7) +
            Rol(obj.ConstantList, 11) +
            Rol(obj.StartOffset, 15) +
            Rol(obj.Length, 19);

        public bool Equals(RawLocalVariableRow x, RawLocalVariableRow y) =>
            x.Attributes == y.Attributes &&
            x.Index == y.Index &&
            x.Name == y.Name;

        public int GetHashCode(RawLocalVariableRow obj) =>
            obj.Attributes +
            Rol(obj.Index, 3) +
            Rol(obj.Name, 7);

        public bool Equals(RawLocalConstantRow x, RawLocalConstantRow y) =>
            x.Name == y.Name &&
            x.Signature == y.Signature;

        public int GetHashCode(RawLocalConstantRow obj) =>
            (int)obj.Name +
            Rol(obj.Signature, 3);

        public bool Equals(RawImportScopeRow x, RawImportScopeRow y) =>
            x.Parent == y.Parent &&
            x.Imports == y.Imports;

        public int GetHashCode(RawImportScopeRow obj) =>
            (int)obj.Parent +
            Rol(obj.Imports, 3);

        public bool Equals(RawStateMachineMethodRow x, RawStateMachineMethodRow y) =>
            x.MoveNextMethod == y.MoveNextMethod &&
            x.KickoffMethod == y.KickoffMethod;

        public int GetHashCode(RawStateMachineMethodRow obj) =>
            (int)obj.MoveNextMethod +
            Rol(obj.KickoffMethod, 3);

        public bool Equals(RawCustomDebugInformationRow x, RawCustomDebugInformationRow y) =>
            x.Parent == y.Parent &&
            x.Kind == y.Kind &&
            x.Value == y.Value;

        public int GetHashCode(RawCustomDebugInformationRow obj) =>
            (int)obj.Parent +
            Rol(obj.Kind, 3) +
            Rol(obj.Value, 7);
    }
}
