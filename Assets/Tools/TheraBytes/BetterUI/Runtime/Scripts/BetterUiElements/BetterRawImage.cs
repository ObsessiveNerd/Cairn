using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
    [AddComponentMenu("Better UI/Controls/Better Raw Image", 30)]
    public class BetterRawImage : RawImage, IImageAppearanceProvider
    {
        public string MaterialType
        {
            get { return materialType; }
            set { ImageAppearanceProviderHelper.SetMaterialType(value, this, materialProperties, ref materialEffect, ref materialType); }
        }

        public MaterialEffect MaterialEffect
        {
            get { return materialEffect; }
            set { ImageAppearanceProviderHelper.SetMaterialEffect(value, this, materialProperties, ref materialEffect, ref materialType); }
        }

        public VertexMaterialData MaterialProperties { get { return materialProperties; } }

        public ColorMode ColoringMode { get { return colorMode; } set { colorMode = value; } }
        public Color SecondColor { get { return secondColor; } set { secondColor = value; } }
        
        [SerializeField]
        ColorMode colorMode = ColorMode.Color;

        [SerializeField]
        Color secondColor = Color.white;

        [SerializeField]
        VertexMaterialData materialProperties = new VertexMaterialData();

        [SerializeField]
        string materialType;

        [SerializeField]
        MaterialEffect materialEffect;

        [SerializeField]
        float materialProperty1, materialProperty2, materialProperty3;


        protected override void OnEnable()
        {
            base.OnEnable();

            if (MaterialProperties.FloatProperties != null)
            {
                if (MaterialProperties.FloatProperties.Length > 0)
                    materialProperty1 = MaterialProperties.FloatProperties[0].Value;

                if (MaterialProperties.FloatProperties.Length > 1)
                    materialProperty2 = MaterialProperties.FloatProperties[1].Value;

                if (MaterialProperties.FloatProperties.Length > 2)
                    materialProperty3 = MaterialProperties.FloatProperties[2].Value;
            }
        }

        public float GetMaterialPropertyValue(int propertyIndex)
        {
            return ImageAppearanceProviderHelper.GetMaterialPropertyValue(propertyIndex,
                ref materialProperty1, ref materialProperty2, ref materialProperty3);
        }

        public void SetMaterialProperty(int propertyIndex, float value)
        {
            ImageAppearanceProviderHelper.SetMaterialProperty(propertyIndex, value, this, materialProperties,
                ref materialProperty1, ref materialProperty2, ref materialProperty3);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            Rect rect = GetPixelAdjustedRect();

            Vector2 pMin = new Vector2(rect.x, rect.y);
            Vector2 pMax = new Vector2(rect.x + rect.width, rect.y + rect.height);

            float w = (texture != null) ? (float)texture.width * texture.texelSize.x : 1;
            float h = (texture != null) ? (float)texture.height * texture.texelSize.y : 1;
            Vector2 uvMin = new Vector2(this.uvRect.xMin * w, this.uvRect.yMin * h);
            Vector2 uvMax = new Vector2(this.uvRect.xMax * w, this.uvRect.yMax * h);

            vh.Clear();
            ImageAppearanceProviderHelper.AddQuad(vh, rect,
                pMin, pMax,
                colorMode, color, secondColor,
                uvMin, uvMax,
                materialProperties);
        }
    }
}
