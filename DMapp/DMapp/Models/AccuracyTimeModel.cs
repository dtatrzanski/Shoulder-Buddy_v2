using System;
using System.Collections.Generic;
using System.Text;

namespace DMapp.Models
{
    public class AccuracyTimeModel
    {
        public string  ColumnName { get; set; }
        public double Value { get; set; }

        public AccuracyTimeModel(string name, double value)
        {
            ColumnName = name;
            Value = value;
        }
    }
}
