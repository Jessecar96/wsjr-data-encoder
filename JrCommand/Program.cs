using CommandLine;
using JrEncoderLib.StarAttributes;

namespace JrCommand
{
    internal class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string BASE_URL = "http://localhost:5000/";

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(ArgumentsParsed);
        }

        private static void ArgumentsParsed(Options options)
        {
            // Finally we can work with everything

            if (options.Flavor != null)
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "flavor", options.Flavor },
                });
                HttpResponseMessage response = client.PostAsync(BASE_URL + "/presentation/run", content).Result;
                string responseString = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine("Sent LF flavor: " + options.Flavor);
            } // flavor

            if (options.LDLStyle != null)
            {
                if (Enum.TryParse(options.LDLStyle, out LDLStyle style))
                {
                    //mqttClient.PublishStringAsync("jrencoder/LDL/style", style.ToString());
                    Console.WriteLine("Sent LDL style: " + options.LDLStyle);
                }
                else
                {
                    Console.WriteLine("Failed to parse LDL style: " + options.LDLStyle);
                }
            } // ldl style

            if (options.LDLBackground != null)
            {
                bool enableBg = options.LDLBackground.Equals("1");
                //mqttClient.PublishStringAsync("jrencoder/LDL/back", enableBg ? "1" : "0");
                Console.WriteLine(enableBg ? "LDL Background Enabled" : "LDL Background Disabled");
            } // ldl bg

            if (options.Warning != null)
            {
                FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "text", options.Warning },
                });
                HttpResponseMessage response = client.PostAsync(BASE_URL + "/alert/send", content).Result;
                string responseString = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine("Warning sent");
            } // warning

            if (options.Beep)
            {
                //mqttClient.PublishStringAsync("jrencoder/beep", "");
                Console.WriteLine("Beep sent");
            } // beep

            if (options.LoadConfig != null)
            {
                //mqttClient.PublishStringAsync("jrencoder/load-config", options.LoadConfig);
                Console.WriteLine("Loading config " + options.LoadConfig);
            } // load config

            if (options.ReloadData)
            {
                HttpResponseMessage response = client.PostAsync(BASE_URL + "/data/refresh", null).Result;
                string responseString = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Data reload sent");
            } // data reload
        }
    }
}