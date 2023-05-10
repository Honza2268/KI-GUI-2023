using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Point = System.Windows.Point;

namespace QuickStart {

    public abstract class FractalGenerator {
        public virtual string Name { get; } = "You didn't set a name";
        public virtual int ShapeCount { get; } = -1;

        public List<DataField> Fields { get; } = new();
        public List<double[]> Shapes { get; } = new();

        public abstract IAsyncEnumerable<double[]> GenerateShapes(Point startPoint);

        public Dictionary<string, string> FieldNotes { get; } = new() {
            {"Depth", "# of iterations"}
        };

        [FractalParameter] public int Depth;

        protected FractalGenerator() {
            IEnumerable<MemberInfo> fields = GetType().GetFields()
                .Where(prop => prop.IsDefined(typeof(FractalParameterAttribute), false)).Select(p => p as MemberInfo).Concat(
                GetType().GetProperties().Where(prop => prop.IsDefined(typeof(FractalParameterAttribute), false))
                );

            foreach (var field in fields) {
                Fields.Add(new DataField(field, this));
            }
        }
    }
    
    public struct DataField {
        private MemberInfo field;
        private FractalGenerator generator;

        public string Name => field.Name;

        public string Value {
            get { 
                if (field.GetType().IsAssignableTo(typeof(FieldInfo))) return (field as FieldInfo).GetValue(generator).ToString();
                if (field.GetType().IsAssignableTo(typeof(PropertyInfo))) return (field as PropertyInfo).GetValue(generator).ToString();
                return "??";
            }
            set {
                if (field.GetType().IsAssignableTo(typeof(PropertyInfo))) return;
                var f = field as FieldInfo;
                //Trace.WriteLine(value);
                switch (f.GetValue(generator)) {
                    case float:
                        if (float.TryParse(value, CultureInfo.CurrentCulture, out var p1)) f.SetValue(generator, p1);
                        break;
                    case int:
                        if (int.TryParse(value, CultureInfo.CurrentCulture, out var p2)) f.SetValue(generator, p2);
                        break;
                    case double:
                        if (double.TryParse(value, CultureInfo.CurrentCulture, out var p3)) f.SetValue(generator, p3);
                        break;
                    case string:
                        f.SetValue(generator, value);
                        break;
                    case bool:
                        var v = value.ToLower().Trim();
                        if (v is "0" or "f" or "false") f.SetValue(generator, false);
                        else if (v is "1" or "t" or "true") f.SetValue(generator, true);
                        break;
                }
            }
        }

        public string Note {
            get {
                if (generator.FieldNotes.TryGetValue(field.Name, out var val)) return val;
                return "";
            }
        }

        public DataField(MemberInfo f, FractalGenerator g) {
            field = f;
            generator = g;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class FractalParameterAttribute : Attribute { }
}
