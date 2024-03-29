﻿using System;
using System.Collections.Generic;

using Confuser.Core.Services;
using Confuser.DynCipher;

using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Confuser.Protections.ReferenceProxy
{
    internal class NormalEncoding : IRPEncoding
    {
        readonly Dictionary<MethodDef, Tuple<int, int>> keys = new Dictionary<MethodDef, Tuple<int, int>>();

        public Instruction[] EmitDecode(MethodDef init, RPContext ctx, Instruction[] arg)
        {
            Tuple<int, int> key = GetKey(ctx.Random, init);
            var ret = new List<Instruction>();
            if (ctx.Random.NextBoolean())
            {
                ret.Add(Instruction.Create(OpCodes.Ldc_I4, key.Item1));
                ret.AddRange(arg);
            }
            else
            {
                ret.AddRange(arg);
                ret.Add(Instruction.Create(OpCodes.Ldc_I4, key.Item1));
            }
            ret.Add(Instruction.Create(OpCodes.Mul));
            return ret.ToArray();
        }

        public int Encode(MethodDef init, RPContext ctx, int value)
        {
            Tuple<int, int> key = GetKey(ctx.Random, init);
            return value * key.Item2;
        }

        Tuple<int, int> GetKey(RandomGenerator random, MethodDef init)
        {
            if (!keys.TryGetValue(init, out Tuple<int, int> ret))
            {
                int key = random.NextInt32() | 1;
                keys[init] = ret = Tuple.Create(key, (int)MathsUtils.ModInv((uint)key));
            }
            return ret;
        }
    }
}
