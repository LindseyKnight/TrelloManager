using System.ComponentModel;
using System.Runtime.CompilerServices;
using Trello.Wpf.Annotations;

namespace Trello.Wpf.ViewModels
{
    public sealed class FilterViewModel: INotifyPropertyChanged
    {
        public FilterViewModel(string filterName, string filterId, bool isSelected)
        {
            m_filterName = filterName;
            m_filterId = filterId;
            m_isSelected = isSelected;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string FilterName
        {
            get
            {
                return m_filterName;
            }
        }

        public string FilterId
        {
            get
            {
                return m_filterId;
            }
        }

        public bool IsSelected
        {
            get
            {
                return m_isSelected;
            }
            set
            {
                m_isSelected = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        readonly string m_filterName;
        readonly string m_filterId;
        bool m_isSelected;
    }
}
