using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    private List<string> urls;
    private static int counter = 0;
    private long msStart;
    private long msEnd;
    [SerializeField] private RunMode runMode;

    private enum RunMode
    {
        Normal,
        Coroutine,
        Job,
        Thread
    }

    void Start()
    {
        urls = new List<string>();
        //insert your direct download urls here
        //(urls.Add("url"));
    }

    void TimeElapsed() => Debug.Log($"Time elapsed in ms: {msEnd - msStart}, start: {msStart}, end: {msEnd}");

    IEnumerator GetRequest(string uri, Action callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            byte[] data = webRequest.downloadHandler.data;
            var path = Path.GetTempFileName();
            Debug.Log(path + " " + webRequest.downloadHandler.text);
            File.WriteAllBytes(path, data);

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
            callback();
        }
    }

    void RequestCallback()
    {
        counter++;
        if(counter == urls.Count)
        {
            counter = 0;
            msEnd = TicksNow();
            TimeElapsed();
        }
    }

    //THREADS
    void DownloadWithThreads()
    {
        foreach(var url in urls)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(DownloadFile));
            thread.Start(url);
        }
    }


    static void DownloadFile(object urlOb)
    {
        var url = urlOb.ToString();
        var request = WebRequest.Create(url);
        request.Method = "GET";
        using (var webResponse = request.GetResponse())
        {
            string[] pages = url.Split('/');
            int page = pages.Length - 1;
            using (Stream input = webResponse.GetResponseStream())
            {
                var path = Path.GetTempFileName();
                Debug.Log(path);
                var fileStream = File.Create(path);
                input.CopyTo(fileStream);
                fileStream.Close();
            }
        }
        counter++;
    }

    [BurstCompatible]
    public struct DownloadFileJob : IJob
    {
        [DeallocateOnJobCompletion]
        public NativeArray<byte> downloadUrl;
        [DeallocateOnJobCompletion]
        public NativeArray<byte> result; 

        public void Execute()
        {
            var url = Encoding.ASCII.GetString(downloadUrl.ToArray());
            var request = WebRequest.Create(url.ToString());
            request.Method = "GET";
            using (var webResponse = request.GetResponse())
            {
                string[] pages = url.ToString().Split('/');
                int page = pages.Length - 1;
                Debug.Log(pages[page] + ":\nReceived: " + webResponse.ContentType);
                var bytes = Encoding.ASCII.GetBytes(webResponse.ContentType);
                result = new NativeArray<byte>(bytes.Length, Allocator.Temp);
                result.CopyFrom(bytes);
                using (Stream input = webResponse.GetResponseStream())
                {
                    var path = Path.GetTempFileName();
                    Debug.Log(path);
                    var fileStream = File.Create(path);
                    input.CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            counter++;
        }
    }
    void ScheduleJobs()
    {
        List<JobHandle> dlJobs = new List<JobHandle>();
        for(int i = 0; i < urls.Count; i++)
        {
            var bytes = Encoding.ASCII.GetBytes(urls[i]);
            var na = new NativeArray<byte>(bytes.Length, Allocator.Persistent);
            var res = new NativeArray<byte>(bytes.Length, Allocator.Persistent);
            na.CopyFrom(bytes);
            res.CopyFrom(bytes);
            var job = new DownloadFileJob
            {
                downloadUrl = na,
                result = res
            };
            job.Schedule();        
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(
            "space"))
        {
            msStart = TicksNow();
            switch (runMode)
            {
                case RunMode.Coroutine:
                    foreach (var url in urls)
                    {
                        StartCoroutine(GetRequest(url, RequestCallback));
                    }
                    break;
                case RunMode.Thread:
                    DownloadWithThreads();
                    break;
                case RunMode.Job:
                    ScheduleJobs();
                    break;
                case RunMode.Normal:
                    break;
                default:
                    break;
            }
            
        }
        if (counter == urls.Count)
        {
            msEnd = TicksNow();
            TimeElapsed();
            counter = 0;
        }
    }

    long TicksNow() => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
}
