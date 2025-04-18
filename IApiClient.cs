using System.Collections.Generic;
using System.Threading.Tasks;

namespace AardaLibrary
{
    public interface IApiClient
    {
        Task<string> FetchApiKeyAsync(string username, string password);
        Task<List<Character>> FetchCharactersAsync(string apiKey);
    }
}
