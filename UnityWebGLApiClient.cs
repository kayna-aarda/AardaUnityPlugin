using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace AardaLibrary
{
    public class UnityWebGLApiClient : IApiClient
    {
        private readonly string _httpApiUrl;

        public UnityWebGLApiClient(string httpApiUrl)
        {
            _httpApiUrl = httpApiUrl.TrimEnd('/');
        }

        public async Task<string> FetchApiKeyAsync(string username, string password)
        {
            string url = _httpApiUrl + "/token";

            try
            {
                // Step 1: Create form data
                var formData = new WWWForm();
                formData.AddField("username", username);
                formData.AddField("password", password);

                // Step 2: Initialize UnityWebRequest
                using (UnityWebRequest request = UnityWebRequest.Post(url, formData))
                {
                    // Optional: Log headers or modify them if needed
                    request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                    // Step 3: Send the request
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }

                    // Step 4: Check for errors
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        throw new Exception($"Error fetching API key: {request.error} ({request.downloadHandler.text})");
                    }

                    // Step 5: Parse the response
                    string responseText = request.downloadHandler.text;
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseText);

                    // Step 6: Return the access token
                    return tokenResponse.access_token;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred while fetching API key: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Character>> FetchCharactersAsync(string apiKey)
        {
            var url = _httpApiUrl + "/project/characters";

            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + apiKey);

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"Error fetching characters: {request.error}");
                }

                string responseText = request.downloadHandler.text;
                var characters = JsonConvert.DeserializeObject<List<Character>>(responseText);
                return characters;
            }
        }
    }
}
