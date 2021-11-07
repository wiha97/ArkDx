using System.Windows.Media;

namespace ArkDx.Data
{
    public class Colour
    {
        BrushConverter bc = new BrushConverter();
        public Brush LightGray()
        {
            return new SolidColorBrush(Colors.Gray);
        }
        public Brush Gray()
        {
            return (Brush)bc.ConvertFrom("#252525");
        }
        public Brush DarkGray()
        {
            return (Brush)bc.ConvertFrom("#323232");
        }
        public Brush Blue()
        {
            return new SolidColorBrush(Colors.Blue);
        }
        public Brush Red()
        {
            return new SolidColorBrush(Colors.Maroon);
        }
        public Brush White()
        {
            return new SolidColorBrush(Colors.White);
        }
        public Brush Green()
        {
            return new SolidColorBrush(Colors.Green);
        }
        public Brush Cyan()
        {
            return new SolidColorBrush(Colors.DarkCyan);
        }
        public Brush Orange()
        {
            return new SolidColorBrush(Colors.Orange);
        }
        public Brush Black()
        {
            return new SolidColorBrush(Colors.Black);
        }
    }
}
