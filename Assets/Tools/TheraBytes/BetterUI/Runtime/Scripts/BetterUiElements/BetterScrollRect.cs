using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
    [AddComponentMenu("Better UI/Controls/Better Scroll Rect", 30)]
    public class BetterScrollRect : ScrollRect
    {
#if UNITY_5_5_OR_NEWER
        /// <summary>
        /// Exposes the "m_ContentStartPosition" variable which is used as reference position during drag.
        /// It is a variable of the base ScrollRect class which is not accessible by default. 
        /// Use the setter at your own risk.
        /// </summary>
        public Vector2 DragStartPosition
        {
            get { return base.m_ContentStartPosition; }
            set { base.m_ContentStartPosition = value; }
        }

        /// <summary>
        /// Exposes the "m_ContentBounds" variable which is used to evaluate the size of the content.
        /// It is a variable of the base ScrollRect class which is not accessible by default. 
        /// Use ther setter at your own risk.
        /// </summary>
        public Bounds ContentBounds
        {
            get { return base.m_ContentBounds; }
            set { base.m_ContentBounds = value; }
        }
#endif

        public float HorizontalStartPosition
        {
            get { return this.horizontalStartPosition; }
            set { this.horizontalStartPosition = value; }
        }
        
        public float VerticalStartPosition
        {
            get { return this.verticalStartPosition; }
            set { this.verticalStartPosition = value; }
        }

        [SerializeField]
        [Range(0, 1)]
        float horizontalStartPosition = 0;

        [SerializeField]
        [Range(0, 1)]
        float verticalStartPosition = 1;

        protected override void Start()
        {
            base.Start();

            if(Application.isPlaying)
            {
                ResetToStartPosition();
            }
        }

        public void ResetToStartPosition()
        { 
            if (horizontalScrollbar != null)
            {
                horizontalScrollbar.value = horizontalStartPosition;
            }
            else if (horizontal)
            {
                horizontalNormalizedPosition = horizontalStartPosition;
            }

            if (verticalScrollbar != null)
            {
                verticalScrollbar.value = verticalStartPosition;
            }
            else if (vertical)
            {
                verticalNormalizedPosition = verticalStartPosition;
            }
        }
    }
}
