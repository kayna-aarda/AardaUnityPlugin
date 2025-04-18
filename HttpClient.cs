using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AardaLibrary
{
    public class HttpApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _httpApiUrl;

        public HttpApiClient(string httpApiUrl)
        {
            _httpClient = new HttpClient();
            _httpApiUrl = httpApiUrl.TrimEnd('/');
        }

        public async Task<string> FetchApiKeyAsync(string username, string password)
        {
            var url = _httpApiUrl + "/token";
            var formData = new FormUrlEncodedContent(new Dictionary<string,string>
            {
                {"username", username},
                {"password", password}
            });

            var response = await _httpClient.PostAsync(url, formData);
            response.EnsureSuccessStatusCode();
            string responseText = await response.Content.ReadAsStringAsync();
            TokenResponse tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseText);
            return tokenResponse.access_token;
        }

        public async Task<List<Character>> FetchCharactersAsync(string apiKey)
        {
            var url = _httpApiUrl + "/project/characters";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", "Bearer " + apiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            List<Character> characters = JsonConvert.DeserializeObject<List<Character>>(jsonResponse);
            return characters;
        }
    }
}
