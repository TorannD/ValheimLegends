using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimLegends
{
    public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
        public AnimationClip this[string name]
        {
            get
            {
                return Find((KeyValuePair<AnimationClip, AnimationClip> x) => x.Key.name == name).Value;
            }
            set
            {
                int num = FindIndex((KeyValuePair<AnimationClip, AnimationClip> x) => x.Key.name == name);
                if (num != -1)
                {
                    base[num] = new KeyValuePair<AnimationClip, AnimationClip>(base[num].Key, value);
                }
            }
        }

        public AnimationClipOverrides(int capacity)
            : base(capacity)
        {
        }
    }
}
