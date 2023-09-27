using System;
using System.Collections.Generic;
using System.Linq;
using Seeq.Sdk.Api;
using Seeq.Sdk.Client;
using Seeq.Sdk.Model;

namespace Seeq.Sdk.Examples {

    // This example looks at various signal operations using the Seeq SDK.
    // In this example, we will create a new signal and add samples to it.
    // Note this example is for demonstration purposes only.
    public class SignalExample {

        public static void go(ApiClient apiClient, string csvFile) {
            // First create instances of the APIs that we will need
            DatasourcesApi datasourcesApi = new DatasourcesApi(apiClient);
            SignalsApi signalsApi = new SignalsApi(apiClient);
            var data = CsvProcessor.ProcessCsvFile(csvFile);

            // Signal properties are set using SignalInputV1. This information is used to customize a signal.
            // Note that the DatasourceClass, DatasourceID and DataID are all meant to be identifiers that you, as the Seeq API user, control.
            // You can map them to whatever makes sense for your use case. You can use CreateSignal() if you don't have any external identifiers
            // and you want Seeq to assign one.
            const string DatasourceClass = "Push CSV Datasource";    // The general identifier for this class of data. You may have multiple datasources /connections with the same class.
            const string DatasourceID = "PushCSVDatasource1";  // The identifier for this specific datasource/connection.

            // First create a datasource definition in Seeq as a home for the samples we will be adding.
            DatasourceInputV1 datasourceInput = new DatasourceInputV1 {
                DatasourceClass = DatasourceClass,
                DatasourceId = DatasourceID,
                Name = "Example CSV Source",
                StoredInSeeq = true         // Seeq will be the system of record -- we will write samples into Seeq's internal database
            };

            // CreateDatasource will either create a datasource or return an already created datasource that matches your DatasourceClass and DatasourceID
            datasourcesApi.CreateDatasource(datasourceInput);

            Dictionary<string, string> dataIdMap = new Dictionary<string, string>();

            // Create the signals based off of the name in the header of the file.
            // assuming linear interpolation with a maximum interpolation of 40hr
            foreach (var signal in data) {
                SignalInputV1 signalInput = new SignalInputV1 {
                    Name = signal.Key,
                    InterpolationMethod = "linear",
                    MaximumInterpolation = "40hr"
                };
                SignalOutputV1 signalOutput = signalsApi.PutSignalByDataId(DatasourceClass, DatasourceID, signal.Key, signalInput);
                String signalId = signalOutput.Id;
                dataIdMap.Add(signal.Key, signalId);
            }

            Console.WriteLine("\nCreated signal...");

            // First, we create a list of samples.
            // Let's create a sample, using the SignalSampleV1 helper class, for every hour from October 20, 2015 00:00:00 UTC to October 20, 2015 23:00:00 UTC.
            foreach (var signal in data) {
                List<SampleInputV1> inputSamples = new List<SampleInputV1>();
                foreach (var sample in signal.Value) {
                    SampleInputV1 inputSample = new SampleInputV1 {
                        Key = new DateTimeOffset(sample.Timestamp.ToUniversalTime()).ToUnixTimeMilliseconds() * 1000000,
                        Value = sample.Value
                    };
                    inputSamples.Add(inputSample);
                }

                var sortedInputSamples = inputSamples.OrderBy(obj => obj.Key).ToList();
                // Using the SamplesInputV1 helper class, we can add the samples to each signal
                SamplesInputV1 addSamplesInput = new SamplesInputV1 {
                    Samples = sortedInputSamples
                };
                var result = signalsApi.AddSamples(dataIdMap[signal.Key], addSamplesInput);
                Console.WriteLine("Writing " + signal.Key + " result " + result.StatusMessage);
            }

            Console.WriteLine("You can now find this signal and visualize it in Seeq.");
        }
    }
}