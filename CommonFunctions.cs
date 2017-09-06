using System;
using System.Collections.Generic;
using SplitAndMerge;

namespace scripting
{
    public class AutoScaleFunction : ParserFunction
    {
        public static int MIN_WIDTH = 640;

        public static double ScaleX { get; private set; }
        public static double ScaleY { get; private set; }

        protected override Variable Evaluate(ParsingScript script)
        {
            bool isList = false;
            List<Variable> args = Utils.GetArgs(script,
                Constants.START_ARG, Constants.END_ARG, out isList);
            Utils.CheckArgs(args.Count, 1, m_name);

            ScaleX = Utils.GetSafeDouble(args, 0);
            ScaleY = Utils.GetSafeDouble(args, 1, ScaleX);

            return Variable.EmptyInstance;
        }

        public static double GetScale(double configOverride, bool isWidth)
        {
            if (configOverride != 0.0) {
                return configOverride;
            }
            return isWidth ? ScaleX : ScaleY;
        }
        public static void TransformSizes(ref int width, ref int height, int screenWidth, string option, double extra = 0.0)
        {
            if (!string.IsNullOrWhiteSpace(option) && option != "auto") {
                return;
            }
            if (extra == 0.0) {
                extra = ScaleX;
                if (extra == 0.0) {
                    return;
                }
            }

            int newWidth = TransformSize(width, screenWidth, extra);
            if (width != 0) {
                double ratio = (double)newWidth / (double)width;
                height = (int)(height * ratio);
            } else {
                height = TransformSize(height, screenWidth, extra);
            }
            width = newWidth;
 
            return;
        }
        public static int TransformSize(int size, int screenWidth, double extra = 1.0)
        {
            if (screenWidth <= MIN_WIDTH) {
                return size;
            }
            return (int)(size * screenWidth * extra / MIN_WIDTH);
        }
    }
}
