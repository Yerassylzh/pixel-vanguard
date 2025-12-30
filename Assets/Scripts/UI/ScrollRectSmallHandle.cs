using UnityEngine;
using UnityEngine.UI;

namespace PixelVanguard.UI
{
    /// <summary>
    /// Forces scrollbar handle to stay a fixed size instead of auto-sizing.
    /// Attach to the Scrollbar GameObject.
    /// </summary>
    public class ScrollRectSmallHandle : ScrollRect {
        public float horizontalScrollbarSize = 0.001f;
        public float verticalScrollbarSize = 0.001f;

        override protected void LateUpdate() {
            base.LateUpdate();
            if (this.verticalScrollbar) {
                this.verticalScrollbar.size=verticalScrollbarSize;
            }
            if (this.horizontalScrollbar) {
                this.horizontalScrollbar.size=horizontalScrollbarSize;
            }
        }
        
        override public void Rebuild(CanvasUpdate executing) {
            base.Rebuild(executing);
            if (this.verticalScrollbar) {
                this.verticalScrollbar.size=verticalScrollbarSize;
            }
            if (this.horizontalScrollbar) {
                this.horizontalScrollbar.size=horizontalScrollbarSize;
            }
        }
    }
}
