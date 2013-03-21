using System;
namespace AsynchronousLog4NetAppenders
{
    public class RingBuffer<T> : IQueue<T>
    {
        private readonly object lockObject = new object();
        private readonly T[] buffer;
        private readonly int size;
        private int readIndex = 0;
        private int writeIndex = 0;
        private bool bufferFull = false;

        public event Action<object, EventArgs> BufferOverflow;

        public RingBuffer(int size)
        {
            this.size = size;
            buffer = new T[size];
        }

        public void Enqueue(T item)
        {
            lock (lockObject)
            {
                buffer[writeIndex] = item;
                writeIndex = (++writeIndex) % size;
                if (bufferFull)
                {
                    if (BufferOverflow != null)
                    {
                        BufferOverflow(this, EventArgs.Empty);
                    }
                    readIndex = writeIndex;
                }
                else if (writeIndex == readIndex)
                {
                    bufferFull = true;
                }
            }
        }

        public bool TryDequeue(out T ret)
        {
            if (readIndex == writeIndex && !bufferFull)
            {
                ret = default(T);
                return false;
            }
            lock (lockObject)
            {
                if (readIndex == writeIndex && !bufferFull)
                {
                    ret = default(T);
                    return false;
                }

                ret = buffer[readIndex];
                readIndex = (++readIndex) % size;
                bufferFull = false;
                return true;
            }
        }
    }
}