using Newtonsoft.Json;
using TitanTechnologyView.Models;

namespace TitanTechnologyView.Extensions
{
    public static class dropdownList
    {
        public static async Task<List<DropdownMaster>> GetDropdownAsync(
           this HttpClient client, string apiOrigin, string name)
        {
            var response = await client.GetAsync($"{apiOrigin}/api/DropdownMaster/by-name/{Uri.EscapeDataString(name)}");

            if (!response.IsSuccessStatusCode)
                return new List<DropdownMaster>();

            var json = await response.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<DropdownMaster>>(json) ?? new();
            return list.Where(x => x.IsActive).ToList();
        }
    }
}
