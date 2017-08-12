using System.Collections.Generic;

namespace Trello.Wpf.Models
{
    public sealed class Settings
    {
        public double? Top { get; set; }
        public double? Left { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public string TrelloName { get; set; }
        public bool GroupByList { get; set; }
        public bool GroupByMember { get; set; }
        public bool GroupByPriority { get; set; }
        public ICollection<string> ListFilters { get; set; }
        public ICollection<string> MemberFilters { get; set; }
        public ICollection<string> PriorityFilters { get; set; }
        public ICollection<string> StatusFilters { get; set; }
    }
}
