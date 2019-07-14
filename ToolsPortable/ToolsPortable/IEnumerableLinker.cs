using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class IEnumerableLinker<T> : IEnumerable<T>
    {
        private IEnumerable<T>[] _listsToLink;

        /// <summary>
        /// Will avoid null lists
        /// </summary>
        /// <param name="listsToLink"></param>
        public IEnumerableLinker(params IEnumerable<T>[] listsToLink)
        {
            _listsToLink = listsToLink.Where(i => i != null).ToArray();
        }

        private class Enumerator : IEnumerator<T>
        {
            private IEnumerable<T>[] _listsToLink;
            private int _index;
            private IEnumerator<T> _enumerator;

            public Enumerator(IEnumerable<T>[] listsToLink)
            {
                _listsToLink = listsToLink;

                Reset();
            }

            public T Current
            {
                get
                {
                    return _enumerator.Current;
                }
            }

            public bool MoveNext()
            {
                if (_enumerator.MoveNext())
                    return true;

                //otherwise, we'll try to move to the next list
                _index++;

                //no other list left
                if (_index >= _listsToLink.Length)
                    return false;

                _enumerator = _listsToLink[_index].GetEnumerator();

                return MoveNext();
            }

            public void Reset()
            {
                _index = 0;
                _enumerator = _listsToLink[0].GetEnumerator();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                //nothing
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_listsToLink);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class IEnumerableLinker : IEnumerable
    {
        private IEnumerable[] _listsToLink;

        public IEnumerableLinker(params IEnumerable[] listsToLink)
        {
            _listsToLink = listsToLink.Where(i => i != null).ToArray();
        }

        public class Enumerator : IEnumerator
        {
            private IEnumerable[] _listsToLink;
            private int _index;
            private IEnumerator _enumerator;

            public Enumerator(IEnumerable[] listsToLink)
            {
                _listsToLink = listsToLink;

                Reset();
            }

            public object Current
            {
                get
                {
                    return _enumerator.Current;
                }
            }

            public bool MoveNext()
            {
                if (_enumerator.MoveNext())
                    return true;

                //otherwise, we'll try to move to the next list
                _index++;

                //no other list left
                if (_index >= _listsToLink.Length)
                    return false;

                _enumerator = _listsToLink[_index].GetEnumerator();

                return MoveNext();
            }

            public void Reset()
            {
                _index = 0;
                _enumerator = _listsToLink[0].GetEnumerator();
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(_listsToLink);
        }
    }
}
