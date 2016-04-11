
namespace CheckComboBoxControl
{
    public class CheckedComboBoxItem
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public CheckedComboBoxItem()
        {
        }

        public CheckedComboBoxItem(string name, object val)
        {
            Name = name;
            Value = val;
        }

        public override string ToString()
        {
            return string.Format("name: '{0}', value: {1}", Name, Value);
        }
    }
}
