using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace Trello.Library
{
    public static class TrelloUtility
    {
        public static ReadOnlyCollection<Card> GetAllTrelloCards()
        {
            return GetTrelloCards("open").Concat(GetTrelloCards("closed")).ToList().AsReadOnly();
        }

        public static ICollection<Card> GetTrelloCards(string statusId)
        {
            string[] filter = statusId != null ? new[] { "filter=" + statusId } : null;
            HttpResponseMessage result = s_httpClient.GetAsync(GetTrelloUrl("cards", filter)).Result;
            if (result.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ICollection<Card>>(result.Content.ReadAsStringAsync().Result);

            return null;
        }

        public static ReadOnlyCollection<CardList> GetTrelloLists()
        {
            HttpResponseMessage result = s_httpClient.GetAsync(GetTrelloUrl("lists")).Result;
            if (result.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ReadOnlyCollection<CardList>>(result.Content.ReadAsStringAsync().Result);

            return null;
        }

        public static ReadOnlyCollection<CardMember> GetTrelloMembers()
        {
            HttpResponseMessage result = s_httpClient.GetAsync(GetTrelloUrl("members")).Result;
            if (result.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ReadOnlyCollection<CardMember>>(result.Content.ReadAsStringAsync().Result);

            return null;
        }

        public static Priority GetPriority(Card card)
        {
            if (card.Name.StartsWith("critical", StringComparison.OrdinalIgnoreCase))
                return Priority.Critical;

            if (card.Name.StartsWith("high", StringComparison.OrdinalIgnoreCase))
                return Priority.High;

            if (card.Name.StartsWith("medium", StringComparison.OrdinalIgnoreCase))
                return Priority.Medium;

            if (card.Name.StartsWith("low", StringComparison.OrdinalIgnoreCase))
                return Priority.Low;

            if (card.Name.StartsWith("sop", StringComparison.OrdinalIgnoreCase))
                return Priority.Sop;

            return Priority.None;
        }

        public static string GetListName(Card card, IEnumerable<CardList> lists)
        {
            CardList list = lists.FirstOrDefault(x => x.Id == card.ListId);
            return list != null ? list.Name : null;
        }

        public static float? GetListPosition(Card card, IEnumerable<CardList> lists)
        {
            CardList list = lists.FirstOrDefault(x => x.Id == card.ListId);
            return list != null ? list.Position : (float?) null;
        }

        public static string GetAssignedTo(Card card, IEnumerable<CardMember> members)
        {
            return card.MemberIds == null || card.MemberIds.Length == 0 ? null : string.Join(", ", card.MemberIds
                .Select(id => members.FirstOrDefault(m => m.Id == id))
                .Where(m => m != null)
                .Select(m => m.FullName)
                .OrderBy(m => m));
        }

        public static bool IsAssignedTo(Card card, CardMember member)
        {
            return (card.MemberIds != null && member != null && card.MemberIds.Contains(member.Id)) ||
                ((card.MemberIds == null || card.MemberIds.Length == 0) && member == null);
        }

        public static DateTime GetDateCreated(Card card)
        {
            return s_epoch.AddSeconds(ulong.Parse(card.Id.Substring(0, 8), NumberStyles.HexNumber));
        }

        private static string GetTrelloUrl(string relativeUrl, ICollection<string> parameters = null)
        {
            List<string> parametersList = parameters != null ? parameters.ToList() : new List<string>();
            parametersList.Add("key=" + c_oauthKey);
            parametersList.Add("token=" + c_oauthTotken);
            return string.Format("https://api.trello.com/1/boards/{0}/{1}?{2}", c_boardId, relativeUrl, string.Join("&", parametersList));
        }

        const string c_oauthKey = "";
        const string c_oauthTotken = "";
        const string c_boardId = "";
        static readonly HttpClient s_httpClient = new HttpClient();
        static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
