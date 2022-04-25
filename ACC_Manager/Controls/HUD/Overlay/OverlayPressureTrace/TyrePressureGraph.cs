﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.Controls.HUD.Overlay.OverlayPressureTrace.TyrePressures;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayPressureTrace
{
    internal class TyrePressureGraph
    {
        private int X, Y;
        private int Width, Height;
        private LinkedList<float> TirePressures;

        public static TyrePressureRange PressureRange { get; set; }

        public TyrePressureGraph(int x, int y, int width, int height, LinkedList<float> tirePressures)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            this.TirePressures = tirePressures;
        }

        private int getRelativeNodeY(float value)
        {
            double range = (PressureRange.OptimalMaximum + 0.4) - (PressureRange.OptimalMinimum - 0.4);
            double percentage = 1d - (value - PressureRange.OptimalMinimum) / range;
            return (int)(percentage * Height);
        }

        public void Draw(Graphics g)
        {
            //this.SetMinAndMax();
            Rectangle graphRect = new Rectangle(X, Y, Width, Height);
            // draw background
            g.FillRectangle(new SolidBrush(Color.FromArgb(196, Color.Black)), graphRect);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (TirePressures.Count > 0)
            {
                Brush brush = Brushes.Green;

                if (TirePressures.First() < PressureRange.OptimalMinimum)
                    brush = Brushes.Blue;
                if (TirePressures.First() > PressureRange.OptimalMaximum)
                    brush = Brushes.Red;

                DrawData(g, TirePressures, brush);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;

                g.DrawRectangle(new Pen(Color.FromArgb(196, Color.Black)), graphRect);
            }
        }

        private void DrawData(Graphics g, LinkedList<float> Data, Brush color)
        {
            if (Data.Count > 0)
            {
                List<Point> points = new List<Point>();
                lock (Data)
                    for (int i = 0; i < Data.Count - 1; i++)
                    {
                        int x = X + Width - i * (Width / Data.Count);
                        lock (Data)
                        {
                            int y = Y + getRelativeNodeY(Data.ElementAt(i));
                            if (y > Y + Height)
                            {
                                y = Y + Height;
                            }
                            if (x < X)
                                break;

                            points.Add(new Point(x, y));
                        }

                    }

                if (points.Count > 0)
                {
                    GraphicsPath path = new GraphicsPath();
                    path.AddLines(points.ToArray());
                    g.DrawPath(new Pen(color, 2.9f), path);
                }
            }
        }
    }
}