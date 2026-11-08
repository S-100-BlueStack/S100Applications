using ArcGIS.Core.Data;
using S100FC;
using S100FC.S101;
using S100Framework.Applications;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ImporterNIS.Common
{
    internal static class AttributeBindings
    {
        internal static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
        }.AppendTypeInfoResolver();

        private static ConcurrentDictionary<UInt128, Guid> _dictionary = new();

        public static Guid AddOrGet(FeatureType instance, Table attributeBindings) {
            var flatten = instance.Flatten();
            var hash = System.IO.Hashing.XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(flatten));

            return _dictionary.GetOrAdd(hash, (hash) => {
                using var buffer = attributeBindings.CreateRowBuffer();
                buffer["ps"] = S100FC.S101.Summary.ProductId;
                buffer["code"] = instance.S100FC_code;
                buffer["attributebindings"] = instance.Flatten();
                buffer["informationbindings"] = System.Text.Json.JsonSerializer.Serialize(instance.GetInformationBindings(), jsonSerializerOptions);
                buffer["sourceIdentifier"] = instance.sourceIdentifier;

                using var row = attributeBindings.CreateRow(buffer);
                return row.GetGlobalID();
            });
        }
    }
}
