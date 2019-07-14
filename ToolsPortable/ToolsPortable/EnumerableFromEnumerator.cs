using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToolsPortable
{
    public class EnumerableFromEnumerator<T> : IEnumerable<T>
    {
        private IEnumerator<T> _enumerator;
        private bool _hasBeenReturnedOnce;

        public EnumerableFromEnumerator(IEnumerator<T> enumerator)
        {
            _enumerator = enumerator;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_hasBeenReturnedOnce)
                _enumerator.Reset();
            else
                _hasBeenReturnedOnce = true;

            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
