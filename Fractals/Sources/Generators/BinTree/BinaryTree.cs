using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using QuickStart;
using Point = System.Windows.Point;

namespace Fractals {

    public class BinaryTreeFractal : FractalGenerator {
        [FractalParameter] public float Angle;
        [FractalParameter] public float Length;
        [FractalParameter] public float LengthScale;
        [FractalParameter] public float AngleDelta;
        [FractalParameter] public string LineColor;
        [FractalParameter] public bool UseRecursion = true;

        public override string Name { get; } = "Binary Tree Sample";

        [FractalParameter] public override int ShapeCount => (int)Math.Pow(2, Depth)-1;

        double[] rgb => stringToRGB(LineColor);
        

        public override async IAsyncEnumerable<double[]> GenerateShapes(Point startPoint) {
            Shapes.Clear();

            var radAngle = Angle * Math.PI / 180;
            var radDelta = AngleDelta * Math.PI / 180;
            
            await foreach (var shape in UseRecursion
                    ? GenerateFractal(startPoint, (int)Depth, radAngle, Length, LengthScale, radDelta)
                    : GenerateFractalNonRecursive(startPoint, radAngle, radDelta)) {
                Shapes.Add(shape);
                yield return shape;
            }
            Trace.WriteLine($"Shapes created: {Shapes.Count}");
        }

        private async IAsyncEnumerable<double[]> GenerateFractal(Point origin, int depth, double angle, double length, double lengthScale, double angleDelta)
        {
            if (depth == 0)
                yield break;
            
            double x2 = length * Math.Cos(angle) + origin.X;
            double y2 = length * Math.Sin(angle) + origin.Y;
            
            double[] packet = {
                origin.X, origin.Y, x2, y2,
                rgb[0], rgb[1], rgb[2]
            };

            yield return packet.Concat(rgb).ToArray();

            Point newOrigin = new(x2, y2);

            await foreach (var left in GenerateFractal(newOrigin,depth - 1, angle - angleDelta, length * lengthScale, lengthScale, angleDelta))
            {
                yield return left;
            }

            await foreach (var right in GenerateFractal(newOrigin,depth - 1, angle + angleDelta, length * lengthScale, lengthScale, angleDelta))
            {
                yield return right;
            }
        }

        private double[] stringToRGB(string s) {
            double[] rgb = s.Split(',').Select(n => {
                double.TryParse(n.Trim(), CultureInfo.CurrentCulture, out var nn);
                return nn;
            }).ToArray()[..3];

            return rgb;
        }
        
        private async IAsyncEnumerable<double[]> GenerateFractalNonRecursive(Point origin, double theta, double deltaTheta) {
            Point startPoint = origin;

            Stack<MyPoint> stack = new();
            stack.Push(new(startPoint, 0, theta));

            while (stack.Count > 0)
            {
                MyPoint myPoint = stack.Pop();

                Point point = myPoint.Point;
                int depth = myPoint.Depth;
                double angle = myPoint.Angle;

                if (depth < Depth)
                {
                    double x1 = point.X;
                    double y1 = point.Y;

                    double x2 = point.X + Length * Math.Pow(LengthScale, depth) * Math.Cos(angle);
                    double y2 = point.Y + Length * Math.Pow(LengthScale, depth) * Math.Sin(angle);
                    
                    double[] packet = {
                        origin.X, origin.Y, x2, y2,
                        rgb[0], rgb[1], rgb[2]
                    };

                    yield return packet.Concat(rgb).ToArray();
                    
                    stack.Push(new MyPoint(new Point(x2, y2), depth + 1, angle + deltaTheta));
                    stack.Push(new MyPoint(new Point(x2, y2), depth + 1, angle - deltaTheta));
                }
            }
        }

        private struct MyPoint {
            public readonly Point Point;
            public readonly int Depth;
            public readonly double Angle;

            public MyPoint(Point p, int d) {
                Point = p;
                Depth = d;
            }

            public MyPoint(Point p, int d, double a) : this(p, d) {
                Angle = a;
            }
        }

        public BinaryTreeFractal() {
            Depth = 10;
            Angle = -90;
            Length = 60;
            LengthScale = 0.75f;
            AngleDelta = 30;
            LineColor = "255, 0, 255";

            FieldNotes.Add("Angle", "in degrees");
            FieldNotes.Add("AngleDelta", "in degrees");
        }
    }
}
