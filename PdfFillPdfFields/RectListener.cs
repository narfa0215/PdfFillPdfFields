using System.Collections.Generic;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfFillPdfFields
{
    public class RectListener : IEventListener
    {
        public List<Rectangle> Rectangles = new List<Rectangle>();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_PATH)
                return;

            var info = (PathRenderInfo)data;
            var path = info.GetPath();

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            bool hasPoint = false;

            foreach (var subpath in path.GetSubpaths())
            {
                foreach (var segment in subpath.GetSegments())
                {
                    var points = segment.GetBasePoints();
                    if (points == null) continue;

                    foreach (var p in points)
                    {
                        hasPoint = true;

                        double x = p.GetX();
                        double y = p.GetY();

                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;

                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (hasPoint)
            {
                Rectangles.Add(new Rectangle(
                    (float)minX,
                    (float)minY,
                    (float)(maxX - minX),
                    (float)(maxY - minY)
                ));
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new HashSet<EventType> { EventType.RENDER_PATH };
        }
    }
}