using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class CombinationsEnumerable<T> : IEnumerable<T[]>
    {
        private T[] _source;

        public CombinationsEnumerable(IEnumerable<T> source)
        {
            _source = source.ToArray();
        }

        private class CombinationsEnumerator<TInnerType> : IEnumerator<TInnerType[]>
        {
            private TInnerType[] _source;
            private CombinationsEnumerator _enumerator;

            public CombinationsEnumerator(TInnerType[] source)
            {
                _source = source;
                _enumerator = new CombinationsEnumerator(source.Length);
            }

            private TInnerType[] _current;

            public TInnerType[] Current
            {
                get { return _current; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (_enumerator.MoveNext())
                {
                    int[] indexes = _enumerator.Current;

                    _current = new TInnerType[indexes.Length];

                    for (int i = 0; i < _current.Length; i++)
                        _current[i] = _source[indexes[i]];

                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _enumerator = new CombinationsEnumerator(_source.Length);
            }

            public void Dispose()
            {

            }
        }

        public IEnumerator<T[]> GetEnumerator()
        {
            return new CombinationsEnumerator<T>(_source);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class CombinationsEnumerable : IEnumerable<int[]>
    {
        public int Size { get; private set; }

        public CombinationsEnumerable(int size)
        {
            Size = size;
        }

        public int TotalNumberOfCombinations
        {
            get
            {
                return (int)Math.Pow(2, Size) - 1;
            }
        }

        public IEnumerator<int[]> GetEnumerator()
        {
            return new CombinationsEnumerator(Size);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        
    }


    public class CombinationsEnumerator : IEnumerator<int[]>
    {
        private int _size;
        private int _maxNumber;

        public CombinationsEnumerator(int size)
        {
            _size = size;
            _maxNumber = size - 1;
        }

        private int[] _current;

        public int[] Current
        {
            get { return _current; }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (_size == 0)
                return false;

            if (_current == null)
            {
                _current = new int[] { 0 };
                return true;
            }

            //only one array of the actual size, thus definitely an end point
            if (_current.Length == _size)
                return false;



            int indexOfIncrement = indexOfNextIncrement();

            //if something can be incremented
            if (indexOfIncrement != -1)
            {
                increment(indexOfIncrement);
                return true;
            }


            //otherwise we have to grow the size
            _current = new int[_current.Length + 1];

            //initialize with default sequential numbers
            for (int i = 0; i < _current.Length; i++)
                _current[i] = i;

            return true;
        }

        private void increment(int index)
        {
            _current = _current.ToArray();

            //increment that point
            _current[index]++;

            //and set everything after it to be sequential
            for (int i = index + 1; i < _current.Length; i++)
                _current[i] = _current[i - 1] + 1;
        }

        private int indexOfNextIncrement()
        {
            //if it's the last that can be incremented
            if (_current.Last() < _maxNumber)
                return _current.Length - 1;

            for (int i = _current.Length - 2; i >= 0; i--)
            {
                //if the number at the index, when incremented, is less than the next number to the right,
                //then it can be incremented
                if (_current[i] + 1 < _current[i + 1])
                    return i;
            }

            //nothing can be incremented
            return -1;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}
