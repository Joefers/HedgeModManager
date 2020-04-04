﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HedgeModManager.UI;

namespace HedgeModManager.Controls
{
    /// <summary>
    /// Interaction logic for FormItem.xaml
    /// </summary>
    public partial class FormItem : UserControl
    {
        public FormSchema Schema { get; }
        public FormItem()
        {
            InitializeComponent();
        }

        public FormItem(FormElement element, FormSchema schema)
        {
            InitializeComponent();
            Schema = schema;
            DisplayName.ToolTip = new ToolTip(){ Content = string.Join("\r\n", element.Description)};
            DisplayName.Content = element.DisplayName;
            DisplayValue.Child = CreateUiElement(element);
        }

        public UIElement CreateUiElement(FormElement element)
        {
            if (element.DefaultValue == null && element.Value == null)
            {
                if(FormBuilder.TypeDatabase.TryGetValue(element.Type, out Type t))
                    element.DefaultValue = Activator.CreateInstance(t);
            }

            if (element.Value == null)
                element.Value = element.DefaultValue;
            
            switch (element.Type)
            {
                case "double":
                case "float":
                    return CreateTextBox(element, "ValueDouble");
                case "uint":
                case "int":
                    return CreateTextBox(element, "ValueLong");
                case "string":
                    return CreateTextBox(element, "Value");

                case "bool":
                {
                    var box = new CheckBox()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    box.SetBinding(ToggleButton.IsCheckedProperty, "Value");
                    box.DataContext = element;
                    return box;
                }
                default:
                {
                    if (Schema.Enums.TryGetValue(element.Type, out var enums))
                    {
                        var box = new ComboBox
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Height = 20,
                            DataContext = element
                        };
                        enums.ForEach(x => box.Items.Add(x));
                        
                        box.SetBinding(Selector.SelectedValueProperty, "Value");
                        
                        if (element.Value != null)
                        {
                            var value = enums.FirstOrDefault(x => x.Value == element.Value.ToString());
                            box.SelectedValue = value ?? enums.First();
                        }
                        else
                        {
                            box.SelectedValue = enums.First();
                        }

                        return box;
                    }
                    else
                    {
                        throw new NotImplementedException($"Element of type {element.Type} is not supported.");
                    }
                }
            }
        }

        TextBox CreateTextBox(FormElement context, string binding)
        {
            var box = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            box.SetBinding(TextBox.TextProperty, binding);
            box.DataContext = context;

            if (context.MinValue.HasValue || context.MaxValue.HasValue)
            {
                var rule = new RangeValidator(context.MinValue ?? double.NegativeInfinity, context.MaxValue ?? double.PositiveInfinity);
                BindingOperations.GetBinding(box, TextBox.TextProperty)?.ValidationRules.Add(rule);
            }

            return box;
        }
    }
}
