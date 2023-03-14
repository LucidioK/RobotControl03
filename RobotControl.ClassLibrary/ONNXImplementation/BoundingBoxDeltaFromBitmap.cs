using System;

namespace RobotControl.ClassLibrary.ONNXImplementation
{
    public class BoundingBoxDeltaFromBitmap
    {
        private BoundingBoxDeltaFromBitmap()          { }

        public float CorrX                            { get; private set; }
        public float CorrY                            { get; private set; }
        public float BitmapWidth                      { get; private set; }
        public float BitmapHeight                     { get; private set; }
        public float XDeltaFromBitmapCenter           { get; private set; }
        public float YDeltaFromBitmapCenter           { get; private set; }
        public float XDeltaProportionFromBitmapCenter { get => BitmapWidth > 0 ? XDeltaFromBitmapCenter / BitmapWidth : 0; }
        public float YDeltaProportionFromBitmapCenter { get => BitmapHeight > 0 ? YDeltaFromBitmapCenter / BitmapHeight : 0; }

        public static BoundingBoxDeltaFromBitmap FromBitmap(int bitmapWidth, int bitmapHeight, BoundingBox box)
        {
            var bbdfb = new BoundingBoxDeltaFromBitmap()
            {
                BitmapWidth  = R0(bitmapWidth),
                BitmapHeight = R0(bitmapHeight),
                CorrX        = (float)bitmapWidth / ImageSettings.imageWidth,
                CorrY        = (float)bitmapHeight / ImageSettings.imageHeight,
            };

            var midXImg                  = bitmapWidth / 2;
            var midXBox                  = (box.Dimensions.X * bbdfb.CorrX) + (box.Dimensions.Width * bbdfb.CorrX / 2);
            bbdfb.XDeltaFromBitmapCenter = R1(midXBox - midXImg);

            var midYImg                  = bitmapHeight / 2;
            var midYBox                  = (box.Dimensions.Y * bbdfb.CorrY) + (box.Dimensions.Height * bbdfb.CorrY / 2);
            bbdfb.YDeltaFromBitmapCenter = R1(midYBox - midYImg);
            bbdfb.CorrX                  = R1(bbdfb.CorrX);
            bbdfb.CorrY                  = R1(bbdfb.CorrY);
            return bbdfb;
        }

        public static float R0(float n)                  => Round(n, 0);
        public static float R1(float n)                  => Round(n, 1);
        public static float Round(float n, int decimals) => (float)Math.Round((double)n, decimals);
    }
}
