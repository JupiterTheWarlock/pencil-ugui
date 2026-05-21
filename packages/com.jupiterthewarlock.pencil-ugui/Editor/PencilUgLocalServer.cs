using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace OpenPencilUGUI
{
    public static class PencilUgLocalServer
    {
        const string PackageVersion = "0.1.0";

        static HttpListener listener;
        static Thread listenerThread;
        static volatile bool running;
        static PencilUgConfig cachedConfig;
        static readonly Queue<Action> MainThreadQueue = new Queue<Action>();
        static readonly object QueueLock = new object();

        public static bool IsRunning => running;

        public static int Port { get; private set; }

        public static bool TryGetHealth(out string error)
        {
            error = null;
            if (!running)
            {
                error = "Server is not running.";
                return false;
            }

            try
            {
                var config = cachedConfig ?? PencilUgConfigStore.ReadFromDisk();
                if (config == null || string.IsNullOrWhiteSpace(config.provider))
                {
                    error = "Harness config is invalid.";
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        public static void Start(PencilUgConfig config)
        {
            if (running)
            {
                return;
            }

            Port = config.serverPort > 0 ? config.serverPort : 47123;
            cachedConfig = config;
            listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
            listener.Start();
            running = true;

            listenerThread = new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "PencilUgLocalServer"
            };
            listenerThread.Start();

            EditorApplication.update -= ProcessMainThreadQueue;
            EditorApplication.update += ProcessMainThreadQueue;
            Debug.Log($"Pencil UGUI local server started on http://127.0.0.1:{Port}/");
        }

        public static void Stop()
        {
            if (!running)
            {
                return;
            }

            running = false;
            cachedConfig = null;
            listener?.Stop();
            listener?.Close();
            listener = null;
            listenerThread = null;
            EditorApplication.update -= ProcessMainThreadQueue;
            Debug.Log("Pencil UGUI local server stopped.");
        }

        static void ListenLoop()
        {
            while (running)
            {
                try
                {
                    var context = listener.GetContext();
                    HandleRequest(context);
                }
                catch (HttpListenerException)
                {
                    if (!running)
                    {
                        return;
                    }
                }
                catch (Exception exception)
                {
                    if (running)
                    {
                        Debug.LogWarning($"Pencil UGUI server request failed: {exception.Message}");
                    }
                }
            }
        }

        static void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var path = request.Url.AbsolutePath.TrimEnd('/');

            try
            {
                if (request.HttpMethod == "GET" && (path == "" || path == "/health"))
                {
                    WriteJson(response, BuildHealthResponse());
                    return;
                }

                if (request.HttpMethod == "GET" && path == "/config")
                {
                    WriteJson(response, PencilUgConfigStore.ReadFromDisk());
                    return;
                }

                if (request.HttpMethod == "GET" && path == "/targets/selection")
                {
                    RunOnMainThreadAndRespond(response, () => PencilUgImportService.DescribeSelectionTarget());
                    return;
                }

                if (request.HttpMethod == "POST" && path == "/import")
                {
                    var body = ReadBody(request);
                    var importRequest = JsonUtility.FromJson<ImportRequestBody>(body);
                    if (importRequest == null || string.IsNullOrWhiteSpace(importRequest.uiIrPath))
                    {
                        WriteError(response, 400, "uiIrPath is required.");
                        return;
                    }

                    var targetMode = string.IsNullOrWhiteSpace(importRequest.target)
                        ? "selection"
                        : importRequest.target;

                    RunOnMainThreadAndRespond(response, () =>
                    {
                        var jsonPath = PencilUgHarnessPaths.ResolveProjectPath(importRequest.uiIrPath);
                        if (!File.Exists(jsonPath))
                        {
                            return ImportResult.Fail($"UI IR file not found: {jsonPath}");
                        }

                        return PencilUgImportService.Import(jsonPath, targetMode);
                    });
                    return;
                }

                WriteError(response, 404, $"Unknown route: {request.HttpMethod} {path}");
            }
            catch (Exception exception)
            {
                WriteError(response, 500, exception.Message);
            }
        }

        static HealthResponse BuildHealthResponse()
        {
            var config = cachedConfig ?? PencilUgConfigStore.ReadFromDisk();
            return new HealthResponse
            {
                ok = true,
                service = "pencil-ugui",
                version = PackageVersion,
                port = Port,
                provider = config.provider,
                configPath = PencilUgHarnessPaths.ConfigRelativePath
            };
        }

        static void RunOnMainThreadAndRespond<T>(HttpListenerResponse response, Func<T> action)
        {
            T result = default;
            Exception error = null;
            var completed = new ManualResetEvent(false);

            lock (QueueLock)
            {
                MainThreadQueue.Enqueue(() =>
                {
                    try
                    {
                        result = action();
                    }
                    catch (Exception exception)
                    {
                        error = exception;
                    }
                    finally
                    {
                        completed.Set();
                    }
                });
            }

            if (!completed.WaitOne(TimeSpan.FromSeconds(30)))
            {
                WriteError(response, 504, "Timed out waiting for Unity main thread.");
                return;
            }

            if (error != null)
            {
                WriteError(response, 500, error.Message);
                return;
            }

            WriteJson(response, result);
        }

        static void ProcessMainThreadQueue()
        {
            while (true)
            {
                Action action;
                lock (QueueLock)
                {
                    if (MainThreadQueue.Count == 0)
                    {
                        return;
                    }

                    action = MainThreadQueue.Dequeue();
                }

                action();
            }
        }

        static string ReadBody(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        static void WriteJson(HttpListenerResponse response, object payload)
        {
            var json = JsonUtility.ToJson(payload, true);
            var bytes = Encoding.UTF8.GetBytes(json);
            response.StatusCode = 200;
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        static void WriteError(HttpListenerResponse response, int statusCode, string message)
        {
            response.StatusCode = statusCode;
            var json = JsonUtility.ToJson(new ErrorResponse
            {
                ok = false,
                error = message,
                status = statusCode
            }, true);
            var bytes = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
        }

        [Serializable]
        class ImportRequestBody
        {
            public string uiIrPath;
            public string target;
        }

        [Serializable]
        class HealthResponse
        {
            public bool ok;
            public string service;
            public string version;
            public int port;
            public string provider;
            public string configPath;
        }

        [Serializable]
        class ErrorResponse
        {
            public bool ok;
            public string error;
            public int status;
        }
    }
}
