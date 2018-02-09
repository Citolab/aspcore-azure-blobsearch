using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Citolab.Azure.BlobStorage.Search.Helpers
{
    public static class IndexExtension
    {
        public static Index AddDefaultWordFields(this Index index)
        {
            index.Fields = index.Fields ?? new List<Field>();
            if (!index.Fields.Exists("content"))
            {
                index.Fields.Add(new Field() { Name = "content", Type = DataType.String, IsRetrievable = true, IsSearchable = true });
            }
            if (!index.Fields.Exists("metadata_storage_path"))
            {
                index.Fields.Add(new Field() { Name = "metadata_storage_path", Type = DataType.String, IsKey = true, IsRetrievable = true });
            }
            return index;
        }

        public static Index AddField(this Index index, Field fieldToAdd)
        {
            if (!index.Fields.Any(f => f.Name == fieldToAdd.Name))
            {
                index.Fields.Add(fieldToAdd);
            }
            return index;
        }

        public static bool Exists(this IList<Field> fields, string name) =>
            fields.Any(f => f.Name == name);
    }
}
