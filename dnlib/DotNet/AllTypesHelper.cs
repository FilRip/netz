using System.Collections.Generic;

namespace dnlib.DotNet
{
    /// <summary>
    /// Returns types without getting stuck in an infinite loop
    /// </summary>
    readonly struct AllTypesHelper
    {
        /// <summary>
        /// Gets a list of all types and nested types
        /// </summary>
        /// <param name="types">A list of types</param>
        public static IEnumerable<TypeDef> Types(IEnumerable<TypeDef> types)
        {
            var visited = new Dictionary<TypeDef, bool>();
            var stack = new Stack<IEnumerator<TypeDef>>();
            if (types != null)
                stack.Push(types.GetEnumerator());
            while (stack.Count > 0)
            {
                var enumerator = stack.Pop();
                while (enumerator.MoveNext())
                {
                    var type = enumerator.Current;
                    if (visited.ContainsKey(type))
                        continue;
                    visited[type] = true;
                    yield return type;
                    if (type.NestedTypes.Count > 0)
                    {
                        stack.Push(enumerator);
                        enumerator = type.NestedTypes.GetEnumerator();
                    }
                }
            }
        }
    }
}
