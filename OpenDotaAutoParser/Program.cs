using System.Configuration;
using System.Net.Mime;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using OpenDotaAutoParser;
using OpenDotaAutoParser.Services;
using OpenDotaDotNet;
using OpenDotaDotNet.Models.Players;
using STRATZ;

public class Program {
    private static List<AbstractDotaApiService> _dotaApiServices;
    
    public static int Main(String[] args) {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        KeyValueConfigurationCollection settings = config.AppSettings.Settings;
        
        var playerIdSetting = settings["playerIds"];
        
        if (playerIdSetting == null || string.IsNullOrWhiteSpace(playerIdSetting.Value)) {
            Console.WriteLine("No configuration file or invalid. A new one will be generated.");
            Console.WriteLine("This can be changed manually later at any time.");
            Console.WriteLine("Please write a comma-separated list of players to consider (123456, 123457, ...)");
            string filteredInput;
            do {
                Console.WriteLine("Player IDs: ");
                filteredInput = new string(Console.ReadLine().Where(c => char.IsDigit(c) || c == ',').ToArray());
            } while(string.IsNullOrWhiteSpace(filteredInput));
            config.AppSettings.Settings.Add("playerIds",filteredInput); // filter out spaces

            Console.WriteLine("If you also want to parse Stratz matches, please provide the Stratz API Key.");
            Console.WriteLine("See https://stratz.com/api (My Tokens -> Show Token Information)");
            Console.WriteLine("(Optional - can be left empty) Enter Stratz API Key: ");
            config.AppSettings.Settings.Add("stratzApiKey", Console.ReadLine());
            
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        _dotaApiServices = [ new OpenDotaService() ];
        
        if (args.Length == 1) {
            _dotaApiServices.Add(new StratzService(args[0]));
        } else {
            var apiKeySetting = settings["stratzApiKey"];
            if (apiKeySetting != null && !string.IsNullOrWhiteSpace(apiKeySetting.Value)) {
                _dotaApiServices.Add(new StratzService(apiKeySetting.Value));
            }
        }
        
        List<long> playerIds = settings["playerIds"].Value.Split(',').ToList().Select(long.Parse).ToList();
        foreach (var service in _dotaApiServices) {
            Console.WriteLine($"Running {service.GetType().Name}");
            foreach (long playerId in playerIds) {
                List<long> openDotaMatches = service.GetUnparsedMatches(playerId);
                Thread.Sleep(1000); // Don't spam API
                foreach (long match in openDotaMatches) {
                    service.ParseMatch(match);
                    Thread.Sleep(1000); // Don't spam API
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine("Finished! Press any enter to exit.");
        Console.Read();
        return 0;
    }
}