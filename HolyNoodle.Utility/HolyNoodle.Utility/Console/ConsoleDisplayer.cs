using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility.Console
{
    public class ConsoleDisplayer
    {
        private const ushort WORD_WIDTH = 32;

        private Dictionary<string, Dictionary<string, Rectangle>> _properties;

        public ConsoleDisplayer(INotifyPropertyChanged model, int left, int top)
        {
            System.Console.CursorVisible = false;
            _properties = new Dictionary<string, Dictionary<string, Rectangle>>();
            var type = model.GetType();
            var properties = type.GetProperties();
            var i = 0;
            foreach (var property in properties.Where(p => p.PropertyType.IsValueType))
            {
                var row = new Dictionary<string, Rectangle>
                {
                    { "Label", new Rectangle(left, top + i, WORD_WIDTH, 1) },
                    { "Value", new Rectangle(left + WORD_WIDTH, top + i, WORD_WIDTH, 1) }
                };
                _properties.Add(property.Name, row);
                Draw(property.Name, row["Label"]);
                Draw(property.GetValue(model).ToString(), row["Value"]);
                ++i;
            }
            model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var row = _properties[e.PropertyName];
            var value = sender.GetType().GetProperty(e.PropertyName).GetValue(sender).ToString();
            Draw(value, row["Value"]);
        }

        private void Draw(string value, Rectangle rectangle)
        {
            System.Console.CursorLeft = rectangle.X;
            System.Console.CursorTop = rectangle.Y;
            var limit = Math.Min(WORD_WIDTH, value.Length);
            for(var i = 0; i < limit; ++i)
            {
                System.Console.Write(value[i]);
            }
        }
    }
}
