using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Gurock.TestRail;
using Newtonsoft.Json;
using UnityEngine;
/* 
 * Made by Kalina Yevhen 27.11.17
 * Specifically for the 
 * Room 8 Studio
 */
class TestRailApiRequest
{

    //Обходим проверку сертификатов безопасности mono
    public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
    //Текущая дата
    public static string Date = DateTimeOffset.Now.ToString();
    //Имя текущего тестрана
    public static string TestRunName;
    //Номер кейса(всегда перезаписываеться)
    public static string CaseId;
    //Уникальный ID тестрана
    public static string TestrunId;
    //Запись логина и пароля для входа
    public static APIClient Client()
    {
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
        APIClient client = new APIClient("https://qa.room8studio.com/");
        client.User = "ТВОЙ_ЛОГИН";
        client.Password = "API_ПАРОЛЬ";
        return client;
    }
    //Доступные запросы к тестрану
    public class TestRuns
    {
        public int id { get; set; }
        public int suite_id { get; set; }
        public string name { get; set; }
        public object description { get; set; }
        public object milestone_id { get; set; }
        public object assignedto_id { get; set; }
        public bool include_all { get; set; }
        public bool is_completed { get; set; }
        public object completed_on { get; set; }
        public object config { get; set; }
        public List<object> config_ids { get; set; }
        public int passed_count { get; set; }
        public int blocked_count { get; set; }
        public int untested_count { get; set; }
        public int retest_count { get; set; }
        public int failed_count { get; set; }
        public int custom_status1_count { get; set; }
        public int custom_status2_count { get; set; }
        public int custom_status3_count { get; set; }
        public int custom_status4_count { get; set; }
        public int custom_status5_count { get; set; }
        public int custom_status6_count { get; set; }
        public int custom_status7_count { get; set; }
        public int project_id { get; set; }
        public object plan_id { get; set; }
        public int created_on { get; set; }
        public int created_by { get; set; }
        public string url { get; set; }
    }
    //Получения текущего тестрана
    public static TestRuns CurrentTestRun()
    {
        var client = Client();
        var response = client.SendGet("get_runs/2&is_completed=0").ToString();
        var allNoComplitedTestRunses = JsonConvert.DeserializeObject<List<TestRuns>>(response);
        var currentTestRun = allNoComplitedTestRunses.FirstOrDefault(t => t.name.Equals("Automation_TestRun_" + TestSuite.AppName + TestSuite.DeviceModel + Date));

        if (currentTestRun == null)
        {
            Debug.LogError("InArray == Null");
            return null;
        }

        TestRunName = currentTestRun.name;
        return currentTestRun;
    }

    //Создание тестрана
    public static void CreateTestRun()
    {
        var client = Client();

        var data = new Dictionary<string, object>
            {
                { "name", "Automation_TestRun_" + TestSuite.AppName + TestSuite.DeviceModel + Date },
                { "suite_id", 369 },
                { "description", "Detailed info about build.." }
            };

        client.SendPost("add_run/2", data);
        CurrentTestRun();
    }

    //Выбрать кейс выставить ему пасс
    public static void TestPass()
    {
        var client = Client();
        var testRunId = CurrentTestRun().id;

        var data = new Dictionary<string, object>
                {
                    {"status_id", 1},
                    {"comment", "Cool"},
                };

        client.SendPost("add_result_for_case/" + testRunId + "/" + CaseId, data);
    }

    //Выбрать кейс выставить ему фейл
    //Хорошим тоном считаеться если у вас всегда есть actual и expected
    public static void TestFail(string actual, string expected)
    {
        var client = Client();

        var testRunId = CurrentTestRun().id;

        var data = new Dictionary<string, object>
            {
                {"status_id", 5},
                {"comment", $"Actual Result: {actual} \nExpected Result: {expected}"}
            };

        client.SendPost("add_result_for_case/" + testRunId + "/" + CaseId, data);
    }
}

