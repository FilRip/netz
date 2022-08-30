using System.Collections.Generic;

using Confuser.Core;

using dnlib.DotNet;

namespace Confuser.Renamer.Analyzers
{
    internal class JsonAnalyzer : IRenamer
    {
        public JsonAnalyzer()
        {
        }

        const string JsonProperty = "Newtonsoft.Json.JsonPropertyAttribute";
        const string JsonIgnore = "Newtonsoft.Json.JsonIgnoreAttribute";
        const string JsonObject = "Newtonsoft.Json.JsonObjectAttribute";
        static readonly HashSet<string> JsonContainers = new HashSet<string> {
            "Newtonsoft.Json.JsonArrayAttribute",
            "Newtonsoft.Json.JsonContainerAttribute",
            "Newtonsoft.Json.JsonDictionaryAttribute",
            "Newtonsoft.Json.JsonObjectAttribute"
        };

        static CustomAttribute GetJsonContainerAttribute(IHasCustomAttribute attrs)
        {
            foreach (var attr in attrs.CustomAttributes)
            {
                if (JsonContainers.Contains(attr.TypeFullName))
                    return attr;
            }
            return null;
        }

        static bool ShouldExclude(TypeDef type, IDnlibDef def)
        {
            CustomAttribute attr;

            if (def.CustomAttributes.IsDefined(JsonProperty))
            {
                attr = def.CustomAttributes.Find(JsonProperty);
                if (attr.HasConstructorArguments || attr.GetProperty("PropertyName") != null)
                    return false;
            }

            attr = GetJsonContainerAttribute(type);
            if (attr == null || attr.TypeFullName != JsonObject)
                return false;

            if (def.CustomAttributes.IsDefined(JsonIgnore))
                return false;

            int serialization = 0;
            if (attr.HasConstructorArguments && attr.ConstructorArguments[0].Type.FullName == "Newtonsoft.Json.MemberSerialization")
                serialization = (int)attr.ConstructorArguments[0].Value;
            else
            {
                foreach (var property in attr.Properties)
                {
                    if (property.Name == "MemberSerialization")
                        serialization = (int)property.Value;
                }
            }

            if (serialization == 0)
            { // OptOut
                return (def is PropertyDef property && property.IsPublic()) ||
                    (def is FieldDef field && field.IsPublic);
            }
            else if (serialization == 1) // OptIn
                return false;
            else if (serialization == 2) // Fields
                return def is FieldDef;
            else  // Unknown
                return false;
        }

        public void Analyze(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            if (def is TypeDef type)
                Analyze(service, type);
            else if (def is MethodDef method)
                Analyze(service, method);
            else if (def is PropertyDef property)
                Analyze(service, property);
            else if (def is FieldDef field)
                Analyze(service, field);
        }

        void Analyze(INameService service, TypeDef type)
        {
            var attr = GetJsonContainerAttribute(type);
            if (attr == null)
                return;

            bool hasId = false;
            if (attr.HasConstructorArguments && attr.ConstructorArguments[0].Type.FullName == "System.String")
                hasId = true;
            else
            {
                foreach (var property in attr.Properties)
                {
                    if (property.Name == "Id")
                        hasId = true;
                }
            }
            if (!hasId)
                service.SetCanRename(type, false);
        }

        void Analyze(INameService service, MethodDef method)
        {
            if (GetJsonContainerAttribute(method.DeclaringType) != null && method.IsConstructor)
            {
                service.SetParam(method, "renameArgs", "false");
            }
        }

        void Analyze(INameService service, PropertyDef property)
        {
            if (ShouldExclude(property.DeclaringType, property))
            {
                service.SetCanRename(property, false);
            }
        }

        void Analyze(INameService service, FieldDef field)
        {
            if (ShouldExclude(field.DeclaringType, field))
            {
                service.SetCanRename(field, false);
            }
        }

        public void PreRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            //
        }

        public void PostRename(ConfuserContext context, INameService service, ProtectionParameters parameters, IDnlibDef def)
        {
            //
        }
    }
}
