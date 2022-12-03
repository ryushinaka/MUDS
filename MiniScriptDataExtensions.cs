using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Miniscript;

namespace Miniscript.Unity3DDataSystem
{
    public static class MiniScriptDataExtensions
    {
        public static ValMap Clone(this ValMap a)
        {
            ValMap result = new ValMap();
            var it = a.map.GetEnumerator();
            while (it.MoveNext())
            {
                result.map.Add(it.Current.Key, it.Current.Value);
            }

            return result;
        }

        public static ValMap Merge(this ValMap a, ValMap b)
        {
            var it = b.map.GetEnumerator();
            while (it.MoveNext())
            {
                if (!a.map.ContainsKey(it.Current.Key))
                {
                    a.map.Add(it.Current.Key, it.Current.Value);
                }
            }
            return a;
        }
    }
}

