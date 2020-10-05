using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;
using System.Net;
using CoreTweet;
using Microsoft.Extensions.Configuration;

namespace IsItFriday
{
    public static class FridayCheckTimer
    {
        [FunctionName("FridayCheckTimer")]
        public static void Run([TimerTrigger("0 0 16 * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string term = "disappointment";
            string dayStatement = "Checking to see if it is Friday...";

            // check to see if it is a friday
            var today = DateTime.Now.ToUniversalTime().AddHours(-7);

            switch (today.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                case DayOfWeek.Saturday:
                    return;
                case DayOfWeek.Monday:
                    term = "ugh monday";
                    dayStatement = "Ugh, Monday.";
                    break;
                case DayOfWeek.Tuesday:
                    break;
                case DayOfWeek.Wednesday:
                    break;
                case DayOfWeek.Thursday:
                    break;
                case DayOfWeek.Friday:
                    term = "friyay";
                    break;
                default:
                    term = "disappointment";
                    break;
            }
            if (config["debug"] == "true") term = config["debug_term"];

            var g = new Giphy(config["GiphyKey"]);
            var gifresult = g.RandomGif(new RandomParameter()
            {
                Tag = term,
                Rating = Rating.Pg13
            });

            var url = gifresult.Result.Data.ImageUrl;

            WebClient wc = new WebClient();
            byte[] imageData = wc.DownloadData(url);

            var tokens = Tokens.Create(config["APIKey"], config["APISecret"], config["AccessToken"], config["AccessSecret"]);
            var uploadedMedia = tokens.Media.Upload(imageData);
            var ids = new long[] { uploadedMedia.MediaId };
            try
            {
                tokens.Statuses.UpdateAsync($"{dayStatement}\n\n\n(powered by GIPHY)", null, null, null, null, null, null, null, ids, null, null, null, null, null, null, null);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
            }
        }
    }
}
