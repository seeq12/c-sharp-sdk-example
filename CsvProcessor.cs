using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Newtonsoft.Json.Linq;

namespace Seeq.Sdk.Examples {

    public class NumericCsvRecord {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
    }

    public class CsvProcessor {
        private const string TIME_COLUMN_NAME = "Timestamp";

        public static Dictionary<string, List<NumericCsvRecord>> ProcessCsvFile(string filePath) {
            var signalDictionary = new Dictionary<string, List<NumericCsvRecord>>();
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
                if (csv.Configuration.HasHeaderRecord) {
                    // read the header
                    csv.Read();
                    csv.ReadHeader();
                    // Get column names
                    foreach (var columnName in csv.HeaderRecord) {
                        if (columnName == TIME_COLUMN_NAME)
                            continue;
                        signalDictionary.Add(columnName, new List<NumericCsvRecord>());
                    }
                }

                // Process the data
                while (csv.Read()) {
                    List<NumericCsvRecord> records = new List<NumericCsvRecord>();
                    for (int i = 1; i < csv.HeaderRecord.Length; i++) {
                        var record = new NumericCsvRecord {
                            Timestamp = csv.GetField<DateTime>(0),
                            Value = csv.GetField<double>(i)
                        };
                        signalDictionary.ElementAt(i - 1).Value.Add(record);
                    }
                }

                return signalDictionary;
            }
        }
    }
}