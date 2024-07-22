using System;
using UnityEngine;

namespace TheraBytes.BetterUi
{
        [Serializable]
        public class RectTransformData : IScreenConfigConnection
        {
            public Vector3 LocalPosition;
            public Vector2 AnchoredPosition;
            public Vector2 SizeDelta;
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
            public Vector3 Scale;
            public Quaternion Rotation;


            /// <summary>
            ///   <para>The offset of the upper right corner of the rectangle relative to the upper right anchor.</para>
            /// </summary>
            public Vector2 OffsetMax
            {
                get
                {
                    return this.AnchoredPosition + Vector2.Scale(this.SizeDelta, Vector2.one - this.Pivot);
                }
                set
                {
                    Vector2 v = value - (this.AnchoredPosition + Vector2.Scale(this.SizeDelta, Vector2.one - this.Pivot));
                    this.SizeDelta = this.SizeDelta + v;
                    this.AnchoredPosition = this.AnchoredPosition + Vector2.Scale(v, this.Pivot);
                }
            }

            /// <summary>
            ///   <para>The offset of the lower left corner of the rectangle relative to the lower left anchor.</para>
            /// </summary>
            public Vector2 OffsetMin
            {
                get
                {
                    return this.AnchoredPosition - Vector2.Scale(this.SizeDelta, this.Pivot);
                }
                set
                {
                    Vector2 v = value - (this.AnchoredPosition - Vector2.Scale(this.SizeDelta, this.Pivot));
                    this.SizeDelta = this.SizeDelta - v;
                    this.AnchoredPosition = this.AnchoredPosition + Vector2.Scale(v, Vector2.one - this.Pivot);
                }
            }

            [SerializeField]
            string screenConfigName;
            public string ScreenConfigName { get { return screenConfigName; } set { screenConfigName = value; } }

            public void PullFromTransform(RectTransform transform)
            {
                this.LocalPosition = transform.localPosition;
                this.AnchorMin = transform.anchorMin;
                this.AnchorMax = transform.anchorMax;
                this.Pivot = transform.pivot;
                this.AnchoredPosition = transform.anchoredPosition;
                this.SizeDelta = transform.sizeDelta;
                this.Rotation = transform.localRotation;
                this.Scale = transform.localScale;
            }


            public void PushToTransform(RectTransform transform)
            {
                transform.localPosition = this.LocalPosition;
                transform.anchorMin = this.AnchorMin;
                transform.anchorMax = this.AnchorMax;
                transform.pivot = this.Pivot;
                transform.anchoredPosition = this.AnchoredPosition;
                transform.sizeDelta = this.SizeDelta;
                transform.localRotation = this.Rotation;
                transform.localScale = this.Scale;
            }

            public static RectTransformData Lerp(RectTransformData a, RectTransformData b, float amount)
            {
                return new RectTransformData()
                {
                    AnchoredPosition = Vector2.Lerp(a.AnchoredPosition, b.AnchoredPosition, amount),
                    AnchorMax = Vector2.Lerp(a.AnchorMax, b.AnchorMax, amount),
                    AnchorMin = Vector2.Lerp(a.AnchorMin, b.AnchorMin, amount),
                    LocalPosition = Vector3.Lerp(a.LocalPosition, b.LocalPosition, amount),
                    Pivot = Vector2.Lerp(a.Pivot, b.Pivot, amount),
                    Rotation = Quaternion.Lerp(a.Rotation, b.Rotation, amount),
                    Scale = Vector3.Lerp(a.Scale, b.Scale, amount),
                    SizeDelta = Vector2.Lerp(a.SizeDelta, b.SizeDelta, amount)
                };
            }

            public override string ToString()
            {
                return string.Format("RectTransformData: sizeDelta {{{0}, {1}}} - anchoredPosition {{{2}, {3}}}",
                    SizeDelta.x, SizeDelta.y, AnchoredPosition.x, AnchoredPosition.y);
            }
        }
}