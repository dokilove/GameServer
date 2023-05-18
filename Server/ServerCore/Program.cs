using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    //class Lock
    //{
    //    //// bool이랑 같다고 보면되는데 커널 레벨에서의 bool
    //    //AutoResetEvent _available = new AutoResetEvent(true);

    //    //// auto reset 은 자동으로 닫아준다는 톨게이트 같은 느낌
    //    //public void Acquire()
    //    //{
    //    //    _available.WaitOne(); // 입장시도
    //    //    // _available.Reset(); // bool = false 수동일때
    //    //}

    //    //public void Release() 
    //    //{
    //    //    _available.Set(); // flag = true
    //    //}

    //    ManualResetEvent _available = new ManualResetEvent(true);

    //    public void Acquire()
    //    {
    //        _available.WaitOne(); // 입장시도
    //        // 여기에서 두단계로 나뉘어지게 되니 문제가 생긴다
    //        _available.Reset(); // 문을 닫는다
    //    }

    //    public void Release()
    //    {
    //        _available.Set(); // 문을 열어준다
    //    }
    //}
    class Progream
    {
        static int _num = 0;
        //static Lock _lock = new Lock();
        static Mutex _lock = new Mutex();

        static void Thread_1()
        {
            // 커널에 요청하는건 
            // 한번만 하더라도 큰 부담이 되서 숫자를 줄임
            // 느림
            for (int i =0; i < 10000; ++i)
            {
                _lock.WaitOne();
                _num++;
                _lock.ReleaseMutex();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; ++i)
            {
                _lock.WaitOne();
                _num--;
                _lock.ReleaseMutex();
            }

        }
        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(_num);
        }
    }
}