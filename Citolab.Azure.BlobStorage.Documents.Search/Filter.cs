using System;
using System.Collections.Generic;
using System.Text;

namespace Citolab.Azure.BlobStorage.Search
{
    public class Filter
    {
        public Filter(string fieldName, FilterOperator filterOperator, string value)
        {
            FieldName = fieldName;
            Value = value;
            FilterOperator = filterOperator;
        }

        public string FieldName { get; set; }
        public string Value { get; set; }

        public FilterOperator FilterOperator { get; set; }
    }

    public enum FilterOperator
    {
        eq = 0,
        ne,
        gt,
        ge,
        lt,
        le
    }
}
