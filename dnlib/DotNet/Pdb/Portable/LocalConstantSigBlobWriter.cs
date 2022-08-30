using System;
using System.Text;

using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.Portable
{
    readonly struct LocalConstantSigBlobWriter
    {
        readonly IWriterError helper;
        readonly Metadata systemMetadata;

        LocalConstantSigBlobWriter(IWriterError helper, Metadata systemMetadata)
        {
            this.helper = helper;
            this.systemMetadata = systemMetadata;
        }

        public static void Write(IWriterError helper, Metadata systemMetadata, DataWriter writer, TypeSig type, object value)
        {
            var sigWriter = new LocalConstantSigBlobWriter(helper, systemMetadata);
            sigWriter.Write(writer, type, value);
        }

        void Write(DataWriter writer, TypeSig type, object value)
        {
            for (; ; type = type.Next)
            {
                if (type is null)
                    return;

                var et = type.ElementType;
                writer.WriteByte((byte)et);
                switch (et)
                {
                    case ElementType.Boolean:
                    case ElementType.Char:
                    case ElementType.I1:
                    case ElementType.U1:
                    case ElementType.I2:
                    case ElementType.U2:
                    case ElementType.I4:
                    case ElementType.U4:
                    case ElementType.I8:
                    case ElementType.U8:
                        WritePrimitiveValue(writer, et, value);
                        return;

                    case ElementType.R4:
                        if (value is float single)
                            writer.WriteSingle(single);
                        else
                        {
                            helper.Error("Expected a Single constant");
                            writer.WriteSingle(0);
                        }
                        return;

                    case ElementType.R8:
                        if (value is double virguleFlottante)
                            writer.WriteDouble(virguleFlottante);
                        else
                        {
                            helper.Error("Expected a Double constant");
                            writer.WriteDouble(0);
                        }
                        return;

                    case ElementType.String:
                        if (value is null)
                            writer.WriteByte(0xFF);
                        else if (value is string chaine)
                            writer.WriteBytes(Encoding.Unicode.GetBytes(chaine));
                        else
                            helper.Error("Expected a String constant");
                        return;

                    case ElementType.Ptr:
                    case ElementType.ByRef:
                        WriteTypeDefOrRef(writer, new TypeSpecUser(type));
                        return;

                    case ElementType.Object:
                        return;

                    case ElementType.ValueType:
                        var tdr = ((ValueTypeSig)type).TypeDefOrRef;
                        var td = tdr.ResolveTypeDef();
                        if (td is null)
                            helper.Error($"Couldn't resolve type 0x{tdr?.MDToken.Raw ?? 0:X8}");
                        else if (td.IsEnum)
                        {
                            var underlyingType = td.GetEnumUnderlyingType().RemovePinnedAndModifiers();
                            switch (underlyingType.GetElementType())
                            {
                                case ElementType.Boolean:
                                case ElementType.Char:
                                case ElementType.I1:
                                case ElementType.U1:
                                case ElementType.I2:
                                case ElementType.U2:
                                case ElementType.I4:
                                case ElementType.U4:
                                case ElementType.I8:
                                case ElementType.U8:
                                    writer.Position--;
                                    writer.WriteByte((byte)underlyingType.GetElementType());
                                    WritePrimitiveValue(writer, underlyingType.GetElementType(), value);
                                    WriteTypeDefOrRef(writer, tdr);
                                    return;
                                default:
                                    helper.Error("Invalid enum underlying type");
                                    return;
                            }
                        }
                        else
                        {
                            WriteTypeDefOrRef(writer, tdr);
                            bool valueWritten = false;
                            if (GetName(tdr, out var ns, out var name) && ns == stringSystem && tdr.DefinitionAssembly.IsCorLib())
                            {
                                if (name == stringDecimal)
                                {
                                    if (value is decimal virguleFlottante2)
                                    {
                                        var bits = decimal.GetBits(virguleFlottante2);
                                        writer.WriteByte((byte)((((uint)bits[3] >> 31) << 7) | (((uint)bits[3] >> 16) & 0x7F)));
                                        writer.WriteInt32(bits[0]);
                                        writer.WriteInt32(bits[1]);
                                        writer.WriteInt32(bits[2]);
                                    }
                                    else
                                    {
                                        helper.Error("Expected a Decimal constant");
                                        writer.WriteBytes(new byte[13]);
                                    }
                                    valueWritten = true;
                                }
                                else if (name == stringDateTime)
                                {
                                    if (value is DateTime horodate)
                                        writer.WriteInt64(horodate.Ticks);
                                    else
                                    {
                                        helper.Error("Expected a DateTime constant");
                                        writer.WriteInt64(0);
                                    }
                                    valueWritten = true;
                                }
                            }
                            if (!valueWritten)
                            {
                                if (value is byte[] octets)
                                    writer.WriteBytes(octets);
                                else if (value != null)
                                {
                                    helper.Error("Unsupported constant: " + value.GetType().FullName);
                                    return;
                                }
                            }
                        }
                        return;

                    case ElementType.Class:
                        WriteTypeDefOrRef(writer, ((ClassSig)type).TypeDefOrRef);
                        if (value is byte[] octets2)
                            writer.WriteBytes(octets2);
                        else if (value != null)
                            helper.Error("Expected a null constant");
                        return;

                    case ElementType.CModReqd:
                    case ElementType.CModOpt:
                        WriteTypeDefOrRef(writer, ((ModifierSig)type).Modifier);
                        break;

                    case ElementType.Var:
                    case ElementType.Array:
                    case ElementType.GenericInst:
                    case ElementType.TypedByRef:
                    case ElementType.I:
                    case ElementType.U:
                    case ElementType.FnPtr:
                    case ElementType.SZArray:
                    case ElementType.MVar:
                        WriteTypeDefOrRef(writer, new TypeSpecUser(type));
                        return;

                    case ElementType.End:
                    case ElementType.Void:
                    case ElementType.ValueArray:
                    case ElementType.R:
                    case ElementType.Internal:
                    case ElementType.Module:
                    case ElementType.Sentinel:
                    case ElementType.Pinned:
                    default:
                        helper.Error("Unsupported element type in LocalConstant sig blob: " + et.ToString());
                        return;
                }
            }
        }
        static readonly UTF8String stringSystem = new UTF8String("System");
        static readonly UTF8String stringDecimal = new UTF8String("Decimal");
        static readonly UTF8String stringDateTime = new UTF8String("DateTime");

        static bool GetName(ITypeDefOrRef tdr, out UTF8String @namespace, out UTF8String name)
        {
            if (tdr is TypeRef tr)
            {
                @namespace = tr.Namespace;
                name = tr.Name;
                return true;
            }

            if (tdr is TypeDef td)
            {
                @namespace = td.Namespace;
                name = td.Name;
                return true;
            }

            @namespace = null;
            name = null;
            return false;
        }

        void WritePrimitiveValue(DataWriter writer, ElementType et, object value)
        {
            switch (et)
            {
                case ElementType.Boolean:
                    if (value is bool booleen)
                        writer.WriteBoolean(booleen);
                    else
                    {
                        helper.Error("Expected a Boolean constant");
                        writer.WriteBoolean(false);
                    }
                    break;

                case ElementType.Char:
                    if (value is char caractere)
                        writer.WriteUInt16(caractere);
                    else
                    {
                        helper.Error("Expected a Char constant");
                        writer.WriteUInt16(0);
                    }
                    break;

                case ElementType.I1:
                    if (value is sbyte octetSigne)
                        writer.WriteSByte(octetSigne);
                    else
                    {
                        helper.Error("Expected a SByte constant");
                        writer.WriteSByte(0);
                    }
                    break;

                case ElementType.U1:
                    if (value is byte octet)
                        writer.WriteByte(octet);
                    else
                    {
                        helper.Error("Expected a Byte constant");
                        writer.WriteByte(0);
                    }
                    break;

                case ElementType.I2:
                    if (value is short entierCourt)
                        writer.WriteInt16(entierCourt);
                    else
                    {
                        helper.Error("Expected an Int16 constant");
                        writer.WriteInt16(0);
                    }
                    break;

                case ElementType.U2:
                    if (value is ushort entierCourtNonSigne)
                        writer.WriteUInt16(entierCourtNonSigne);
                    else
                    {
                        helper.Error("Expected a UInt16 constant");
                        writer.WriteUInt16(0);
                    }
                    break;

                case ElementType.I4:
                    if (value is int entier)
                        writer.WriteInt32(entier);
                    else
                    {
                        helper.Error("Expected an Int32 constant");
                        writer.WriteInt32(0);
                    }
                    break;

                case ElementType.U4:
                    if (value is uint entierNonSigne)
                        writer.WriteUInt32(entierNonSigne);
                    else
                    {
                        helper.Error("Expected a UInt32 constant");
                        writer.WriteUInt32(0);
                    }
                    break;

                case ElementType.I8:
                    if (value is long entierLong)
                        writer.WriteInt64(entierLong);
                    else
                    {
                        helper.Error("Expected an Int64 constant");
                        writer.WriteInt64(0);
                    }
                    break;

                case ElementType.U8:
                    if (value is ulong entierLongNonSigne)
                        writer.WriteUInt64(entierLongNonSigne);
                    else
                    {
                        helper.Error("Expected a UInt64 constant");
                        writer.WriteUInt64(0);
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        void WriteTypeDefOrRef(DataWriter writer, ITypeDefOrRef tdr)
        {
            if (!MD.CodedToken.TypeDefOrRef.Encode(systemMetadata.GetToken(tdr), out uint codedToken))
            {
                helper.Error("Couldn't encode a TypeDefOrRef");
                return;
            }
            writer.WriteCompressedUInt32(codedToken);
        }
    }
}
