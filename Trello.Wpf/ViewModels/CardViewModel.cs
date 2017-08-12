using System;
using Trello.Library;

namespace Trello.Wpf.ViewModels
{
    public sealed class CardViewModel
    {
        public CardViewModel(string groupKey, string groupTitle, string title, string description, string list, string assignedTo, bool assignedToMe, string url, DateTime creationDate, Priority priority)
        {
            m_groupKey = groupKey;
            m_groupTitle = groupTitle;
            m_title = title;
            m_description = description;
            m_list = list;
            m_assignedTo = assignedTo;
            m_assignedToMe = assignedToMe;
            m_url = url;
            m_creationDate = creationDate;
            m_priority = priority;
        }

        public string GroupKey
        {
            get { return m_groupKey; }
        }

        public string GroupTitle
        {
            get { return m_groupTitle; }
        }

        public string Title
        {
            get
            {
                return m_title;
            }
        }

        public string Description
        {
            get
            {
                return m_description;
            }
        }

        public string List
        {
            get
            {
                return m_list;
            }
        }

        public string AssignedTo
        {
            get
            {
                return m_assignedTo;
            }
        }

        public bool AssignedToMe
        {
            get
            {
                return m_assignedToMe;
            }
        }

        public string Url
        {
            get
            {
                return m_url;
            }
        }

        public DateTime CreationDate
        {
            get
            {
                return m_creationDate;
            }
        }

        public Priority Priority
        {
            get { return m_priority; }
        }

        readonly string m_groupKey;
        readonly string m_groupTitle;
        readonly string m_title;
        readonly string m_description;
        readonly string m_list;
        readonly string m_assignedTo;
        readonly bool m_assignedToMe;
        readonly string m_url;
        readonly DateTime m_creationDate;
        readonly Priority m_priority;
    }
}
