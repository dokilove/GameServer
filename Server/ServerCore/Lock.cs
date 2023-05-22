using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 재귀적 락을 허용할지 (Yes) WriteLock->WriteLock OK, WriteLock->ReadLock OK, ReadLock->WriteLock NO    
    // 스핀락 정책 (5000번 -> Yield)
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // 32비트
        // [Unused(1)] [WriteThreadId(15)] [ReadCount(16)]
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId) 
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while(true)
            {
                for (int i=0; i <MAX_SPIN_COUNT; i++) 
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                    // 시도를 해서 성공하면 return
                    // 의사코드
                    // if (_flag == EMPTY_FLAG)                    
                    //    _flag = desired;                    
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if (lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
            // 다 0으로 밀어준다 초기화
        }

        public void ReadLock() 
        {
            // 동일 쓰레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadId = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다
            while (true) 
            {
                for (int i =0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                    // 의사 코드
                    //if ((_flag & WRITE_MASK) == 0)
                    //{
                    //    _flag = _flag + 1;
                    //    return;
                    //}
                    // A(0->1) B(0->1) 둘이 동시에 0에서 1로 바꿔달라고 요청
                    // B(0->1) A가 먼저 성사됨 _flag가 1로 바뀐상태라 B는 실패
                }
                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag); // 1을 줄여준다
        }

        // 재귀적 락을 허용할지 (No)
        // 스핀락 정책 (5000번 -> Yield)
        //class Lock
        //{
        //    const int EMPTY_FLAG = 0x00000000;
        //    const int WRITE_MASK = 0x7FFF0000;
        //    const int READ_MASK = 0x0000FFFF;
        //    const int MAX_SPIN_COUNT = 5000;

        //    // 32비트
        //    // [Unused(1)] [WriteThreadId(15)] [ReadCount(16)]
        //    int _flag = EMPTY_FLAG;

        //    public void WriteLock()
        //    {
        //        // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다
        //        int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
        //        while (true)
        //        {
        //            for (int i = 0; i < MAX_SPIN_COUNT; i++)
        //            {
        //                if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
        //                    return;
        //                // 시도를 해서 성공하면 return
        //                // 의사코드
        //                // if (_flag == EMPTY_FLAG)                    
        //                //    _flag = desired;                    
        //            }

        //            Thread.Yield();
        //        }
        //    }

        //    public void WriteUnlock()
        //    {
        //        Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        //        // 다 0으로 밀어준다 초기화
        //    }

        //    public void ReadLock()
        //    {
        //        // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다
        //        while (true)
        //        {
        //            for (int i = 0; i < MAX_SPIN_COUNT; i++)
        //            {
        //                int expected = (_flag & READ_MASK);
        //                if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
        //                    return;
        //                // 의사 코드
        //                //if ((_flag & WRITE_MASK) == 0)
        //                //{
        //                //    _flag = _flag + 1;
        //                //    return;
        //                //}
        //                // A(0->1) B(0->1) 둘이 동시에 0에서 1로 바꿔달라고 요청
        //                // B(0->1) A가 먼저 성사됨 _flag가 1로 바뀐상태라 B는 실패
        //            }
        //            Thread.Yield();
        //        }
        //    }

        //    public void ReadUnlock()
        //    {
        //        Interlocked.Decrement(ref _flag); // 1을 줄여준다
        //    }
    }
}
