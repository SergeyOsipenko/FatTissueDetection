namespace UWPObjectDetection
{
    public class PredictionModel
    {
        public PredictionModel(float probability, string tagName, BoundingBox boundingBox)
        {
            Probability = probability;
            TagName = tagName;
            BoundingBox = boundingBox;
        }

        public float Probability { get; private set; }
        public string TagName { get; private set; }
        public BoundingBox BoundingBox { get; private set; }
    }
}
