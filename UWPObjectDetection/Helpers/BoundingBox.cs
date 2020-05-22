namespace UWPObjectDetection
{
    public class BoundingBox
    {
        public BoundingBox(float left, float top, float width, float height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public float Left { get; private set; }
        public float Top { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }
    }
}
