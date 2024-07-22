using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheraBytes.BetterUi
{
    [AddComponentMenu("Better UI/Helpers/Transform Scaler", 30)]
    public class TransformScaler : ResolutionSizer<Vector3>
    {
        public Vector3SizeModifier ScaleSizer { get { return customScaleSizers.GetCurrentItem(scaleSizerFallback); } }

        protected override ScreenDependentSize<Vector3> sizer { get { return customScaleSizers.GetCurrentItem(scaleSizerFallback); } }

        [FormerlySerializedAs("scaleSizer")]
        [SerializeField]
        Vector3SizeModifier scaleSizerFallback = new Vector3SizeModifier(Vector3.one, Vector3.zero, 4 * Vector3.one);

        [SerializeField]
        Vector3SizeConfigCollection customScaleSizers = new Vector3SizeConfigCollection();

        protected override void ApplySize(Vector3 newSize)
        {
            this.transform.localScale = newSize;
        }
    }
}
