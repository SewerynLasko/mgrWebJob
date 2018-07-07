using System;
using Microsoft.Azure.WebJobs;
using OAuth;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;

namespace mgrWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        public static Int32 sleepTimeStart = 0;
        public static Int32 sleepTimeEnd = 0;
        public static Int32 trainingTimeStart = 0;
        public static Int32 trainingTimeEnd = 0;

        public static string CONSUMER_KEY = "f8ee8fab-4916-4c0f-8137-abd8358dba65"; //APPLICATION_ID
        public static string CONSUMER_SECRET = "QicVJ03lzjkwXWDW7FyVpXijV6FNOYLnNRb"; //APPLICATION_PASSWORD
        public static string USER_ACCESS_TOKEN = "a6352297-0593-49b9-ab13-5c3a29a50d6a"; //USER_TOKEN
        public static string USER_ACCESS_TOKEN_SECRET = "iXzP9riRMnTRKmfAqD6XZUTDpJD6ulfOSj4"; // USER_TOKEN_PASSWORD

        public static string AZURE_DATABASE_CONN_STRING = "Server=tcp:mgrsewerynlaskodbserver.database.windows.net,1433;Initial Catalog=mgrsewerynlasko_db;Persist Security Info=False;User ID=mgrsewerynlasko;Password=Admin111;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public static object MessgeBox { get; private set; }

        static void Main()
        {
            var config = new JobHostConfiguration();
            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            getDatesAndConvertToUnixTimestamp();
            getAndSaveSleepData();

            // var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();
        }

        static void getDatesAndConvertToUnixTimestamp()
        {
            DateTime currentDate = DateTime.UtcNow;

            // Sleep start (yesterday 21 PM)
            TimeSpan requestredHour = new TimeSpan(21, 0, 0);
            DateTime requestedDate = currentDate.Date;
            requestedDate = currentDate.AddDays(-1);
            requestedDate = requestedDate.Date + requestredHour;
            sleepTimeStart = (Int32)(requestedDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            // Sleep end (today 8 AM)
            requestredHour = new TimeSpan(8, 0, 0);
            requestedDate = currentDate.Date;
            requestedDate = requestedDate.Date + requestredHour;
            sleepTimeEnd = (Int32)(requestedDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            // Training start (today 6 AM)
            requestredHour = new TimeSpan(6, 0, 0);
            requestedDate = currentDate.Date;
            requestedDate = requestedDate.Date + requestredHour;
            trainingTimeStart = (Int32)(requestedDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            // Training end (today 9 AM)
            requestredHour = new TimeSpan(9, 0, 0);
            requestedDate = currentDate.Date;
            requestedDate = requestedDate.Date + requestredHour;
            trainingTimeEnd = (Int32)(requestedDate.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        static void getAndSaveSleepData()
        {
            // Prepare pieces of oAuth for request header
            OAuthBase oAuth = new OAuthBase();
            string oAuthTimestamp = oAuth.GenerateTimeStamp();
            string OAuthNonce = oAuth.GenerateNonce();
            string tempOut1, tempOut2;
            Uri uri = new Uri("https://healthapi.garmin.com/wellness-api/rest/sleeps?uploadStartTimeInSeconds="
                + 1530784800 + "&uploadEndTimeInSeconds=" + 1530864000);
            string signature = oAuth.GenerateSignature(uri, CONSUMER_KEY, CONSUMER_SECRET,
                USER_ACCESS_TOKEN, USER_ACCESS_TOKEN_SECRET, "GET", oAuthTimestamp, OAuthNonce,
               OAuthBase.SignatureTypes.HMACSHA1, out tempOut1, out tempOut2);
            string signatureEncoded = oAuth.EncodeSignature(signature);

            // Prepare request
            var request = WebRequest.Create(uri);
            request.Method = "GET";
            request.ContentType = "application/json; charset=utf-8";
            string auth = "OAuth " +
               "oauth_consumer_key=\"f8ee8fab-4916-4c0f-8137-abd8358dba65\"," +
               "oauth_token=\"a6352297-0593-49b9-ab13-5c3a29a50d6a\"," +
               "oauth_signature_method=\"HMAC-SHA1\"," +
               "oauth_timestamp=" + oAuthTimestamp + "," +
               "oauth_nonce=" + OAuthNonce + "," +
               "oauth_version=\"1.0\"," +
               "oauth_signature=" + signatureEncoded;
            request.Headers.Add("Authorization", auth);

            // Send request and get response
            var response = request.GetResponse();
            SleepSummary sleepSummary = new SleepSummary();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
            {
                string myJsonResponse = sr.ReadToEnd();

                //Deserialize response and capture relevant data
                var sleepSummaryJson = JsonConvert.DeserializeObject<JArray>(myJsonResponse).First.ToString();
                sleepSummary = JsonConvert.DeserializeObject<SleepSummary>(sleepSummaryJson);
            }

            // Connect to Azure Database and save data
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = AZURE_DATABASE_CONN_STRING;
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"INSERT INTO dbo.GarminData(Date, SleepDurationInSec, SleepDeepInSec, SleepLightInSec) VALUES (@date, @sleepDurationInSec, @sleepDeepInSec, @sleepLightInSec)";
                    cmd.Parameters.AddWithValue("@date", DateTime.ParseExact(sleepSummary.calendarDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@sleepDurationInSec", sleepSummary.durationInSeconds);
                    cmd.Parameters.AddWithValue("@sleepDeepInSec", sleepSummary.deepSleepDurationInSeconds + sleepSummary.remSleepInSeconds);
                    cmd.Parameters.AddWithValue("@sleepLightInSec", sleepSummary.lightSleepDurationInSeconds);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    catch (SqlException e)
                    {
                        Console.WriteLine(e.Message.ToString(), "Error Message");
                    }
                }
            }
        }
    }
}
