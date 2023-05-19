using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    class Program
    { 
        // 1. 근성
        // 2. 양보
        // 3. 갑질

        // 상호 배제
        // Monitor
        static object _lock = new object();
        static SpinLock _lock2 = new SpinLock();
        static Mutex _lock3 = new Mutex(); // 위의 2개보다 무겁다

        // 예를 들어 보상을 추가한다고 할떄
        // [] [] [] [] []
        class Reward
        {

        }

        // RWLock ReadweWriteLock
        static ReaderWriterLockSlim _lock4 = new ReaderWriterLockSlim();

        // 99.9999%
        static Reward GetRewardById(int id)
        {
            _lock4.EnterReadLock();

            _lock4.ExitReadLock();

            return null;
        }
        
        // 0.0001%
        // 일주일에 한번 호출될까 말까
        static void AddReward(Reward reward)
        {
            _lock4.EnterWriteLock();
            _lock4.ExitWriteLock();
        }


        static void Main(string[] args)
        {
            lock (_lock)
            {

            }

            bool lockTaken = false;
            try
            {
                _lock2.Enter(ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                {
                    _lock2.Exit();
                }
            }
        }
    }
}