using System;

using dnlib.DotNet;

namespace Confuser.Core.Project.Patterns
{
    /// <summary>
    ///     A function that indicate the visibility of members.
    /// </summary>
    public class IsPublicFunction : PatternFunction
    {
        internal const string FnName = "is-public";

        /// <inheritdoc />
        public override string Name
        {
            get { return FnName; }
        }

        /// <inheritdoc />
        public override int ArgumentCount
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public override object Evaluate(IDnlibDef definition)
        {
            if (!(definition is IMemberDef member))
                return false;

            var declType = ((IMemberDef)definition).DeclaringType;
            while (declType != null)
            {
                if (!declType.IsPublic)
                    return false;
                declType = declType.DeclaringType;
            }

            if (member is MethodDef def)
                return def.IsPublic;
            if (member is FieldDef field)
                return field.IsPublic;
            if (member is PropertyDef property)
                return property.IsPublic();
            if (member is EventDef evenement)
                return evenement.IsPublic();
            if (member is TypeDef type)
                return type.IsPublic || type.IsNestedPublic;

            throw new NotSupportedException();
        }
    }
}
