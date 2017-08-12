using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Trello.Library;
using Trello.Wpf.Annotations;
using Trello.Wpf.Models;

namespace Trello.Wpf.ViewModels
{
    public sealed class MainWindowViewModel: INotifyPropertyChanged
    {
        public MainWindowViewModel(Settings settings)
        {
            // load settings
            m_trelloFullName = settings.TrelloName;
            m_groupByList = settings.GroupByList;
            m_groupByMember = settings.GroupByMember;
            m_groupByPriority = settings.GroupByPriority;
            m_listFilters = new ObservableCollection<FilterViewModel>((settings.ListFilters ?? new List<string>())
                .Select(x => new FilterViewModel("", x, true)));
            m_memberFilters = new ObservableCollection<FilterViewModel>((settings.MemberFilters ?? new List<string>())
                .Select(x => new FilterViewModel("", x, true)));
            m_priorityFilters = new ObservableCollection<FilterViewModel>((settings.PriorityFilters ?? new List<string>())
                .Select(x => new FilterViewModel("", x, true)));
            m_statusFilters = new ObservableCollection<FilterViewModel>((settings.StatusFilters ?? new List<string>())
                .Select(x => new FilterViewModel("", x, true)));

            Refresh();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string TrelloFullName { get { return m_trelloFullName; } }
        public string CardsSortPropertyName { get; set; }
        public ListSortDirection CardsSortDirection { get; set; }

        public ObservableCollection<CardViewModel> Cards
        {
            get { return m_cards; }
            set
            {
                m_cards = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FilterViewModel> ListFilters
        {
            get { return m_listFilters; }
            set
            {
                m_listFilters = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FilterViewModel> MemberFilters
        {
            get { return m_memberFilters; }
            set
            {
                m_memberFilters = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FilterViewModel> PriorityFilters
        {
            get { return m_priorityFilters; }
            set
            {
                m_priorityFilters = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FilterViewModel> StatusFilters
        {
            get { return m_statusFilters; }
            set
            {
                m_statusFilters = value;
                OnPropertyChanged();
            }
        }

        public bool GroupByList
        {
            get
            {
                return m_groupByList;
            }
            set
            {
                m_groupByList = value;
                OnPropertyChanged();
            }
        }

        public bool GroupByMember
        {
            get
            {
                return m_groupByMember;
            }
            set
            {
                m_groupByMember = value;
                OnPropertyChanged();
            }
        }

        public bool GroupByPriority
        {
            get
            {
                return m_groupByPriority;
            }
            set
            {
                m_groupByPriority = value;
                OnPropertyChanged();
            }
        }

        public void Refresh()
        {
            m_allCards = TrelloUtility.GetAllTrelloCards();
            m_lists = TrelloUtility.GetTrelloLists();
            m_members = TrelloUtility.GetTrelloMembers();
            FilterCards();
        }

        public void FilterCards()
        {
            ListFilters = new ObservableCollection<FilterViewModel>(GetListFilters());
            MemberFilters = new ObservableCollection<FilterViewModel>(GetMemberFilters());
            PriorityFilters = new ObservableCollection<FilterViewModel>(GetPriorityFilters());
            StatusFilters = new ObservableCollection<FilterViewModel>(GetStatusFilters());
            Cards = new ObservableCollection<CardViewModel>(Sort(GetCards()));
        }

        public IEnumerable<CardViewModel> Sort(IEnumerable<CardViewModel> cards)
        {
            IOrderedEnumerable<CardViewModel> groupedCards = cards.OrderBy(x => x.GroupKey);
            if (CardsSortPropertyName != null)
            {
                PropertyInfo property = typeof(CardViewModel).GetProperty(CardsSortPropertyName);
                return CardsSortDirection == ListSortDirection.Ascending
                    ? groupedCards.ThenBy(x => property.GetValue(x))
                    : groupedCards.ThenByDescending(x => property.GetValue(x));
            }

            return groupedCards;
        }

        private IEnumerable<CardViewModel> GetCards()
        {
            List<Card> cards = m_allCards.Where(FilterCard).ToList();
            List<CardViewModel> cardViews = new List<CardViewModel>();

            if (!GroupByMember)
            {
                cardViews.AddRange(cards.Select(x => Map(x, null)));
            }
            else
            {
                foreach (CardMember member in m_members.Where(FilterMember))
                    cardViews.AddRange(cards.Where(x => TrelloUtility.IsAssignedTo(x, member)).Select(x => Map(x, member)));
            }

            return cardViews;
        }

        private IEnumerable<FilterViewModel> GetListFilters()
        {
            return m_lists.Select(x => new FilterViewModel(x.Name, x.Id,
                    m_listFilters != null && m_listFilters.Where(l => l.FilterId == x.Id).Select(l => l.IsSelected).FirstOrDefault()))
                .ToList();
        }

        private IEnumerable<FilterViewModel> GetMemberFilters()
        {
            return m_members.Concat(new[] { new CardMember { FullName = "Unassigned", UserName = "Unassigned", Id = "" } })
                .Select(x => new FilterViewModel(x.FullName, x.Id,
                    m_memberFilters != null && m_memberFilters.Where(l => l.FilterId == x.Id).Select(l => l.IsSelected).FirstOrDefault()))
                .ToList();
        }

        private IEnumerable<FilterViewModel> GetPriorityFilters()
        {
            return ((Priority[]) Enum.GetValues(typeof(Priority))).Select(x => new FilterViewModel(x.ToString(), ((int) x).ToString(),
                m_priorityFilters != null && m_priorityFilters.Where(p => p.FilterId == ((int) x).ToString()).Select(p => p.IsSelected).FirstOrDefault()))
                .ToList();
        }

        private IEnumerable<FilterViewModel> GetStatusFilters()
        {
            return new List<FilterViewModel>
            {
                new FilterViewModel("Open", "open", m_statusFilters != null && m_statusFilters.Where(x => x.FilterId == "open").Select(x => x.IsSelected).FirstOrDefault()),
                new FilterViewModel("Closed", "closed", m_statusFilters != null && m_statusFilters.Where(x => x.FilterId == "closed").Select(x => x.IsSelected).FirstOrDefault())
            };
        }

        private GroupDetails GetGroupDetails(Card card, CardMember cardMember)
        {
            List<string> key = new List<string>();
            List<string> title = new List<string>();
            if (GroupByMember)
            {
                key.Add(cardMember != null ? cardMember.FullName : "Unassigned");
                title.Add(cardMember != null ? cardMember.FullName : "Unassigned");
            }
            if (GroupByPriority)
            {
                Priority priority = TrelloUtility.GetPriority(card);
                key.Add(((int) priority).ToString());
                title.Add(priority.ToString());
            }
            if (GroupByList)
            {
                key.Add(string.Format("{0:#.###}", TrelloUtility.GetListPosition(card, m_lists)).PadLeft(10, '0'));
                title.Add(TrelloUtility.GetListName(card, m_lists));
            }
            return new GroupDetails
            {
                Key = string.Join("|", key),
                Title = string.Join(" | ", title)
            };
        }

        private bool FilterCard(Card card)
        {
            return (
                m_listFilters.All(x => !x.IsSelected) ||
                m_listFilters.Any(x => x.IsSelected && x.FilterId == card.ListId)
                ) && (
                m_memberFilters.All(x => !x.IsSelected) ||
                m_memberFilters.Any(x => x.IsSelected && (
                    (x.FilterId != "" && card.MemberIds.Contains(x.FilterId)) ||
                    (x.FilterId == "" && (card.MemberIds == null || card.MemberIds.Length == 0))))
                ) && (
                m_priorityFilters.All(x => !x.IsSelected) ||
                m_priorityFilters.Any(x => x.IsSelected && x.FilterId == ((int) TrelloUtility.GetPriority(card)).ToString())
                ) && (
                m_statusFilters.All(x => !x.IsSelected) ||
                m_statusFilters.Any(x => x.IsSelected && (
                    (x.FilterId == "open" && !card.IsClosed) ||
                    (x.FilterId == "closed" && card.IsClosed)))
                );
        }

        private bool FilterMember(CardMember member)
        {
            return m_memberFilters.All(x => !x.IsSelected) ||
                m_memberFilters.Any(x => x.IsSelected && (x.FilterId != "" ? member != null && member.Id == x.FilterId : member == null));
        }

        private CardViewModel Map(Card card, CardMember member)
        {
            GroupDetails groupDetails = GetGroupDetails(card, member);
            string assignedTo = TrelloUtility.GetAssignedTo(card, m_members);
            return new CardViewModel(
                groupKey: groupDetails.Key,
                groupTitle: groupDetails.Title,
                title: card.Name,
                description: card.Description,
                list: TrelloUtility.GetListName(card, m_lists),
                assignedTo: assignedTo,
                assignedToMe: assignedTo != null && m_trelloFullName != null && assignedTo.Contains(m_trelloFullName),
                url: card.ShortUrl,
                creationDate: TrelloUtility.GetDateCreated(card),
                priority: TrelloUtility.GetPriority(card));
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        readonly string m_trelloFullName;
        ObservableCollection<CardViewModel> m_cards;
        ObservableCollection<FilterViewModel> m_listFilters;
        ObservableCollection<FilterViewModel> m_memberFilters;
        ObservableCollection<FilterViewModel> m_priorityFilters;
        ObservableCollection<FilterViewModel> m_statusFilters;
        ReadOnlyCollection<Card> m_allCards;
        ReadOnlyCollection<CardList> m_lists;
        ReadOnlyCollection<CardMember> m_members;
        bool m_groupByList;
        bool m_groupByMember;
        bool m_groupByPriority;
    }
}
