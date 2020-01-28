using Microsoft.Azure.Search.Models;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Citolab.Azure.BlobStorage.Search.Helpers
{
    public static class IndexExtension
    {
        public static Index AddDefaultWordFields(this Index index, string language = "en", bool useLucine = true)
        {

            index.Fields ??= new List<Field>();
            if (!index.Fields.Exists("content"))
            {
                var analyzer = useLucine ? "lucene" : "microsoft";
                
                var item = new Field("content", AnalyzerName.EnMicrosoft)
                {

                    Analyzer = $"{language.ToLower()}.{analyzer}",
                    Name = "content",
                    Type = DataType.String,
                    IsRetrievable = true,
                    IsSearchable = true
                };
                index.Fields.Add(item);
            }
            if (!index.Fields.Exists("metadata_storage_path"))
            {
                index.Fields.Add(new Field("metadata_storage_path", AnalyzerName.NlLucene)
                {
                    Type = DataType.String,
                    IsKey = true
                });
            }
            return index;
        }



        public static Index AddField(this Index index, Field fieldToAdd)
        {
            if (index.Fields.All(f => f.Name != fieldToAdd.Name))
            {
                index.Fields.Add(fieldToAdd);
            }
            return index;
        }

        public static bool Exists(this IList<Field> fields, string name) =>
            fields.Any(f => f.Name == name);
    }
}
