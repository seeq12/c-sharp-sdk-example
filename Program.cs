using System;
using Seeq.Sdk.Api;
using Seeq.Sdk.Client;
using Seeq.Sdk.Model;

namespace Seeq.Sdk.Examples {

    internal class Program {

        private static void Main(string[] args) {
            Console.Write(
                "This is the Seeq C# SDK Sample application.\n\n");

            Console.WriteLine("Logging into Seeq.");

            // Seeq requires a minimum TLS version of 1.2
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            // Access the Seeq API using ApiClient, specifying where Seeq server is running.
            ApiClient apiClient = null;
            try {
                apiClient = new ApiClient("<seeq url>");
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            // Authenticate against the API using a user's credentials or an API key and secret.
            AuthApi authApi = new AuthApi(apiClient);
            AuthInputV1 input = new AuthInputV1();
            input.Username = "<username>";
            input.Password = "<password>";
            try {
                authApi.Login(input);
                Console.WriteLine("Seeq login successful.");
            } catch (Exception ex) {
                Console.WriteLine("Login to Seeq failed due to exception: " + ex.Message);
                return;
            }

            Console.WriteLine("\n----------Signal Example----------");
            SignalExample.go(apiClient, "sampleDataNumeric.csv");
            Console.WriteLine("\nSignal Example complete.");
            Console.ReadLine();
        }
    }
}