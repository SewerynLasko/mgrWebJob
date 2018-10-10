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
        public static string USER_ACCESS_TOKEN = "5be4f65b-4a3e-4718-b967-aeba2e2c62b9"; //USER_TOKEN
        public static string USER_ACCESS_TOKEN_SECRET = "itG28UlQTLc50FT8ttccuwcbZ93afVPzjJd"; // USER_TOKEN_PASSWORD
        public static string AZURE_DATABASE_CONN_STRING = "Server=tcp:mgrsewerynlaskodbserver.database.windows.net,1433;Initial Catalog=mgrsewerynlasko_db;Persist Security Info=False;User ID=mgrsewerynlasko;Password=Admin111;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        // 0Auth variables
        public static string OAUTH_CONSUMER_KEY = "f8ee8fab-4916-4c0f-8137-abd8358dba65";
        public static string OAUTH_TOKEN = "5be4f65b-4a3e-4718-b967-aeba2e2c62b9";

        public static TemperatureSummary temperatureSummary;
        public static ActivitySummary activitySummary;
        public static SleepSummary sleepSummary;


        static void Main()
        {
            var config = new JobHostConfiguration();
            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            getDatesAndConvertToUnixTimestamp();
            getTemperature();
            getSleepData();
            getActivityData();
            saveData();
        }

        static void saveData()
        {
            if (activitySummary != null && sleepSummary != null && temperatureSummary != null)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = @"INSERT INTO dbo.GarminData(BikingStartTime, BikingDurationInSec, BikingAvgHeartRate, BikingMaxHeartRate, Temperature, Date, SleepDate, SleepDurationInSec, SleepDeepInSec, SleepLightInSec, Pressure, Humidity, Visibility, WindSpeed, WindDeg, Clouds, Sunrise, Sunset) VALUES (@bikingStartTime, @bikingDurationInSec, @bikingAvgHeartRate, @bikingMaxHeartRate, @temperatureInCelc, @date, @sleepDate, @sleepDurationInSec, @sleepDeepInSec, @sleepLightInSec, @pressure, @humidity, @visibility, @windSpeed, @windDeg, @clouds, @sunrise, @sunset)";
                cmd.Parameters.AddWithValue("@bikingStartTime", UnixTimeStampToDateTime(activitySummary.startTimeInSeconds).ToString("HH:mm"));
                cmd.Parameters.AddWithValue("@bikingDurationInSec", activitySummary.durationInSeconds);
                cmd.Parameters.AddWithValue("@bikingAvgHeartRate", activitySummary.averageHeartRateInBeatsPerMinute);
                cmd.Parameters.AddWithValue("@bikingMaxHeartRate", activitySummary.maxHeartRateInBeatsPerMinute);

                cmd.Parameters.AddWithValue("@sleepDate", DateTime.ParseExact(sleepSummary.calendarDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture));
                cmd.Parameters.AddWithValue("@sleepDurationInSec", sleepSummary.durationInSeconds);
                cmd.Parameters.AddWithValue("@sleepDeepInSec", sleepSummary.deepSleepDurationInSeconds + sleepSummary.remSleepInSeconds);
                cmd.Parameters.AddWithValue("@sleepLightInSec", sleepSummary.lightSleepDurationInSeconds);

                cmd.Parameters.AddWithValue("@date", DateTime.UtcNow);

                cmd.Parameters.AddWithValue("@temperatureInCelc", Math.Round(temperatureSummary.main.temp - 273.15));
                cmd.Parameters.AddWithValue("@pressure", temperatureSummary.main.pressure);
                cmd.Parameters.AddWithValue("@humidity", temperatureSummary.main.humidity);
                cmd.Parameters.AddWithValue("@visibility", temperatureSummary.visibility);
                cmd.Parameters.AddWithValue("@windSpeed", temperatureSummary.wind.speed);
                cmd.Parameters.AddWithValue("@windDeg", temperatureSummary.wind.deg);
                cmd.Parameters.AddWithValue("@clouds", temperatureSummary.clouds.all);
                cmd.Parameters.AddWithValue("@sunrise", temperatureSummary.sys.sunrise);
                cmd.Parameters.AddWithValue("@sunset", temperatureSummary.sys.sunset);

                executeNonQuery(cmd);
            }
        }

        static void getTemperature()
        {
            string apiKey = "ec54c36691cff6d49cc39d7d5efa3707";
            string city = "katowice";
            string url = "http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=" + apiKey;
            var request = WebRequest.Create(url);
            request.Method = "GET";
            var response = request.GetResponse();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
            {
                string myJsonResponse = sr.ReadToEnd();
                //Deserialize response and capture relevant data
                temperatureSummary = JsonConvert.DeserializeObject<TemperatureSummary>(myJsonResponse);
            }
        }

        static void getActivityData()
        {
            WebRequest request = requestGenerator("activities", 1530594000, 1530648000);
            // Send request and get response
            var response = request.GetResponse();
            activitySummary = new ActivitySummary();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
            {
                string myJsonResponse = sr.ReadToEnd();

                //Deserialize response and capture relevant data
                var activitySummaryJson = JsonConvert.DeserializeObject<JArray>(myJsonResponse).First.ToString();
                if (activitySummaryJson != null)
                {
                    activitySummary = JsonConvert.DeserializeObject<ActivitySummary>(activitySummaryJson);
                }
            }
        }

        static void getSleepData()
        {
            WebRequest request = requestGenerator("sleeps", 1539162000, 1539201600);
            // Send request and get response
            var response = request.GetResponse();
            sleepSummary = new SleepSummary();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
            {
                string myJsonResponse = sr.ReadToEnd();

                //Deserialize response and capture relevant data
                var sleepSummaryJson = JsonConvert.DeserializeObject<JArray>(myJsonResponse).First.ToString();
                if (sleepSummaryJson != null)
                {
                    sleepSummary = JsonConvert.DeserializeObject<SleepSummary>(sleepSummaryJson);
                }
            }
        }

        static void executeNonQuery(SqlCommand cmd)
        {
            // Connect to Azure Database and save data
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = AZURE_DATABASE_CONN_STRING;
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;

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

                cmd.Dispose();
            }
        }

        static int fetchLastRecordIdFromDatabase()
        {
            int maxId = 0;
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = AZURE_DATABASE_CONN_STRING;
                cmd.Connection = conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT MAX(Id) FROM dbo.GarminData";

                try
                {
                    conn.Open();
                    maxId = Convert.ToInt32(cmd.ExecuteScalar());
                    conn.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.Message.ToString(), "Error Message");
                }

                cmd.Dispose();
            }

            return maxId;
        }

        static WebRequest requestGenerator(string summaryType, int uploadStartTimeInSeconds, int uploadEndTimeInSeconds)
        {
            // Prepare pieces of oAuth for request header
            OAuthBase oAuth = new OAuthBase();
            string oAuthTimestamp = oAuth.GenerateTimeStamp();
            string OAuthNonce = oAuth.GenerateNonce();
            string tempOut1, tempOut2;
            Uri uri = new Uri("https://healthapi.garmin.com/wellness-api/rest/" + summaryType + "?uploadStartTimeInSeconds="
                + uploadStartTimeInSeconds.ToString() + "&uploadEndTimeInSeconds=" + uploadEndTimeInSeconds.ToString());
            string signature = oAuth.GenerateSignature(uri, CONSUMER_KEY, CONSUMER_SECRET,
                USER_ACCESS_TOKEN, USER_ACCESS_TOKEN_SECRET, "GET", oAuthTimestamp, OAuthNonce,
               OAuthBase.SignatureTypes.HMACSHA1, out tempOut1, out tempOut2);
            string signatureEncoded = oAuth.EncodeSignature(signature);

            // Prepare request
            var request = WebRequest.Create(uri);
            request.Method = "GET";
            request.ContentType = "application/json; charset=utf-8";
            string auth = "OAuth " +
               "oauth_consumer_key=" + OAUTH_CONSUMER_KEY + "," +
               "oauth_token=" + OAUTH_TOKEN +"," +
               "oauth_signature_method=\"HMAC-SHA1\"," +
               "oauth_timestamp=" + oAuthTimestamp + "," +
               "oauth_nonce=" + OAuthNonce + "," +
               "oauth_version=\"1.0\"," +
               "oauth_signature=" + signatureEncoded;
            request.Headers.Add("Authorization", auth);
            return request;
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

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
