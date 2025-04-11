using System.Collections.ObjectModel;

namespace BSE.Tunes.Maui.Client.Collections
{
    public class NavigableCollection<T> : ObservableCollection<T>
    {
        private int _index;

        public int Index => _index;
        public bool CanMoveNext => _index < (Count - 1);
        public bool CanMovePrevious => _index > 0;

        public bool MoveNext()
        {
            _index++;
            return (_index >= 0) && (_index < Count);
        }
        public bool MovePrevious()
        {
            bool flag = false;
            if (!CanMovePrevious)
            {
                return flag;
            }
            _index--;
            return (_index >= 0) && (_index < Count);
        }

        public T Current
        {
            get
            {
                if ((_index < 0) || (_index >= Count))
                {
                    throw new IndexOutOfRangeException();
                }
                return base[_index];
            }
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
