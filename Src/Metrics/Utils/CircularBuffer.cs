using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Metrics.Utils
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private T[] _buffer;
        private int _latestIndex = -1;
        private bool bufferFull = false;
        public int bufferSize { get; private set; }

        public int Count
        {
            get
            {
                if (bufferFull)
                    return bufferSize;
                else
                    return _latestIndex + 1;
            }
        }

        public CircularBuffer(int size)
        {
            bufferSize = size;
            _latestIndex = -1;
            _buffer = new T[bufferSize];
        }

        public void Add(T item)
        {
            _latestIndex++;
            if (_latestIndex == bufferSize)
            {
                bufferFull = true;
                _latestIndex = 0;
            }
            _buffer[_latestIndex] = item;
        }

        public void Clear()
        {
            _buffer = new T[bufferSize];
            _latestIndex = -1;
        }

        public T First()
        {
            if (_latestIndex < 0)
                return default(T);
            else
                return _buffer[_latestIndex];
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_latestIndex < 0)
                yield break;
            int max = bufferFull ? bufferSize : _latestIndex + 1;
            for (int i = _latestIndex + 1; Wrap(i) < max - 1; i++)
            {
                yield return _buffer[Wrap(i)];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int i]
        {
            get
            {
                if ((!bufferFull && i > _latestIndex) || i < 0)
                    throw new IndexOutOfRangeException();
                else
                    return _buffer[Wrap(i)];
            }

            set
            {
                if ((!bufferFull && i > _latestIndex) || i < 0)
                    throw new IndexOutOfRangeException();
                else
                    _buffer[Wrap(i)] = value;

            }
        }

        internal int Wrap(int i)
        {
            int max = bufferFull ? bufferSize : _latestIndex + 1;
            if (i < 0)
                i += max * (-i / max + 1);
            return i % max;
        }
    }
}
