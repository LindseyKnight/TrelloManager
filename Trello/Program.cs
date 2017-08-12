using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Trello.Library;

namespace Trello
{
    // TODO: export directly to Excel
    class Program
    {
        static void Main(string[] args)
        {
            ICollection<Card> cards = TrelloUtility.GetTrelloCards();
            ICollection<CardList> lists = TrelloUtility.GetTrelloLists();
            ICollection<CardMember> members = TrelloUtility.GetTrelloMembers();

            // get cards in Current/Next/Issues
            // order by High/Medium/Low/SOP/other
            int rowNumber = 2;
            string[] listIds = { "5807b075211094166c145959", "5807b0731f74ffb9328d427b", "58c30d8955af62910bf96423" };
            File.WriteAllLines(@"C:\Temp\Trello.txt",
                cards.Where(x => !x.IsClosed && listIds.Contains(x.ListId))
                .OrderBy(x =>
                {
                    int criticalIndex = x.Name.IndexOf("critical", StringComparison.OrdinalIgnoreCase);
                    int highIndex = x.Name.IndexOf("high", StringComparison.OrdinalIgnoreCase);
                    int mediumIndex = x.Name.IndexOf("medium", StringComparison.OrdinalIgnoreCase);
                    int lowIndex = x.Name.IndexOf("low", StringComparison.OrdinalIgnoreCase);
                    int sopIndex = x.Name.IndexOf("sop", StringComparison.OrdinalIgnoreCase);
                    return criticalIndex == 0 ? 0
                        : highIndex == 0 ? 1
                        : mediumIndex == 0 && highIndex > 0 && highIndex < 10 ? 2
                        : mediumIndex == 0 ? 3
                        : lowIndex == 0 && mediumIndex > 0 && mediumIndex < 7 ? 4
                        : lowIndex == 0 ? 5
                        : lowIndex < 7 ? 6
                        : sopIndex == 0 ? 7
                        : 8;
                })
                .ThenBy(TrelloUtility.GetDateCreated)
                .Select(x => string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", 
                    string.Format("=IF(ISNUMBER(SEARCH(G1,F{0}))=TRUE, \"Yes\", \"\")", rowNumber++),
                    x.Name,
                    string.Format("=hyperlink(\"{0}\")", x.ShortUrl),
                    TrelloUtility.GetDateCreated(x).ToString("yy/MM/dd"),
                    TrelloUtility.GetListName(x, lists),
                    TrelloUtility.GetAssignedTo(x, members))));
        }
    }
}
