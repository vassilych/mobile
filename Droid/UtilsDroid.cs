using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using SplitAndMerge;

namespace scripting.Droid
{
    public class UtilsDroid
    {
        public static Size GetScreenSize()
        {
            DisplayMetrics displayMetrics = new DisplayMetrics();
            var winManager = MainActivity.TheView.WindowManager;
            winManager.DefaultDisplay.GetMetrics(displayMetrics);

            return new Size(displayMetrics.WidthPixels, displayMetrics.HeightPixels);
        }

        public static void AddViewBorder(View view, Color borderColor = new Color(), int width = 1, int corner = 5)
        {
            GradientDrawable drawable = new GradientDrawable();
            drawable.SetShape(ShapeType.Rectangle);
            drawable.SetCornerRadius(corner);
            drawable.SetStroke(width, borderColor);
            //drawable.SetColor(borderColor);
            view.Background = drawable;
        }
        public static LayoutRules String2LayoutParam(DroidVariable location, bool isX)
        {
            View referenceView = isX ? location.ViewX : location.ViewY;
            string param       = isX ? location.RuleX : location.RuleY;

            bool useRoot = referenceView == null;

            UIVariable refLocation = isX ? location.RefViewX?.Location :
                                           location.RefViewY?.Location;

            // If the reference view has a margin, add it to the current view.
            // This is the iOS functionality.
            if (isX && refLocation != null) {
                location.TranslationX += refLocation.TranslationX;
            } else if (!isX && refLocation != null) {
                location.TranslationY += refLocation.TranslationY;
            }

            switch (param) {
                case "LEFT":
                    return useRoot ? LayoutRules.AlignParentLeft :
                                     LayoutRules.LeftOf;
                case "RIGHT":
                    return useRoot ? LayoutRules.AlignParentRight :
                                     LayoutRules.RightOf;
                case "TOP":
                    return useRoot ? LayoutRules.AlignParentTop :
                                     LayoutRules.Above;
                case "BOTTOM":
                    return useRoot ? LayoutRules.AlignParentBottom :
                                     LayoutRules.Below;
                case "CENTER":
                    if (useRoot) {
                        return isX ? LayoutRules.CenterHorizontal :
                                     LayoutRules.CenterVertical;
                    } else {
                        if (isX) {
                            location.TranslationX += (refLocation.Width - location.Width) / 2;
                            return LayoutRules.AlignLeft;
                        } else {
                            location.TranslationY += (refLocation.Height - location.Height) / 2;
                            return LayoutRules.AlignTop;
                        }
                    }
                case "ALIGN_LEFT":
                    return LayoutRules.AlignLeft;
                case "ALIGN_RIGHT":
                    return LayoutRules.AlignRight;
                case "ALIGN_TOP":
                    return LayoutRules.AlignTop;
                case "ALIGN_BOTTOM":
                    return LayoutRules.AlignBottom;
                case "ALIGN_PARENT_TOP":
                    return LayoutRules.AlignParentTop;
                case "ALIGN_PARENT_BOTTOM":
                    return LayoutRules.AlignParentBottom;
                default:
                    return LayoutRules.AlignStart;
            }
        }
    }
}
