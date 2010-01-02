using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Sprocket
{
    public static class TreeTraversal
    {
        public static FrameworkElement GetFirstChildControl(this DependencyObject searchNode, Predicate<FrameworkElement> criteria)
        {
            foreach (var item in FindVisualChildren(searchNode))
                if (item is FrameworkElement && criteria((FrameworkElement)item))
                    return (FrameworkElement)item;
            return null;
        }

        public static IEnumerable<DependencyObject> FindVisualChildren(DependencyObject depObj) 
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null)
                    {
                        yield return child;
                    }

                    foreach (var childOfChild in FindVisualChildren(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
