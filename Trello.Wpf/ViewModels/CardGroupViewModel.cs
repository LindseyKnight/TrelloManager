using System.Collections.Generic;

namespace Trello.Wpf.ViewModels
{
    public sealed class CardGroupViewModel
    {
        public CardGroupViewModel(string title, List<CardViewModel> cards)
        {
            m_title = title;
            m_cards = cards;
        }

        public string Title
        {
            get
            {
                return m_title;
            }
        }

        public List<CardViewModel> Cards
        {
            get
            {
                return m_cards;
            }
        }

        readonly string m_title;
        readonly List<CardViewModel> m_cards;
    }
}
