using System.Collections.Concurrent;

public class Example
{
    public static async Task Main()
    {
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        // 存储对任务的引用，以便我们可以等待它们并在取消后观察它们的状态。
        Task t;
        var tasks = new ConcurrentBag<Task>();

        Console.WriteLine("按任意键开始任务...");
        Console.ReadKey(true);
        Console.WriteLine("要终止示例，请按'c'取消并退出...");
        Console.WriteLine();

        // 当CancellationTokenSource取消时，请求取消单个任务。将令牌传递给用户委托DoSomeWork和Task，以便它可以正确处理异常。
        t = Task.Run(() => DoSomeWork(1, token), token);
        Console.WriteLine("Task {0} executing", t.Id);
        tasks.Add(t);

        // 请求取消任务及其子任务。注意，令牌传递给(1)用户委托，(2)作为Task的第二个参数。
        // 运行，以便任务实例能够正确处理OperationCanceledException
        t = Task.Run(() =>
        {
            // 创建一些可取消的子任务。
            Task tc;
            for (int i = 3; i <= 10; i++)
            {
                // 对于每个子任务，将相同的令牌传递给每个用户委托和task . run
                tc = Task.Run(() => DoSomeWork(i, token), token);
                Console.WriteLine("Task {0} executing", tc.Id);
                tasks.Add(tc);
                // 再次传递相同的令牌以执行父任务上的工作。所有这些都将通过调用tokenSource发出信号。取消下面。
                DoSomeWork(2, token);
            }
        }, token);

        Console.WriteLine("Task {0} executing", t.Id);
        tasks.Add(t);

        // 从UI线程取消请求。
        char ch = Console.ReadKey().KeyChar;
        if (ch == 'c' || ch == 'C')
        {
            tokenSource.Cancel();
            Console.WriteLine("\nTask cancellation requested.");

            //可选:观察任务状态属性的变化。
            //不需要等待已经取消的任务。然而,
            //如果要等待，必须将调用包含在try-catch块中捕获抛出的TaskCanceledExceptions。如果你不等待，如果令牌被传递给任务。方法是请求取消的令牌。
        }

        try
        {
            await Task.WhenAll(tasks.ToArray());
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown\n");
        }
        finally
        {
            tokenSource.Dispose();
        }

        // Display status of all tasks.
        foreach (var task in tasks)
            Console.WriteLine("Task {0} status is now {1}", task.Id, task.Status);
    }

    static void DoSomeWork(int taskNum, CancellationToken ct)
    {
        // Was cancellation already requested?
        if (ct.IsCancellationRequested)
        {
            Console.WriteLine("Task {0} was cancelled before it got started.",
                              taskNum);
            ct.ThrowIfCancellationRequested();
        }

        int maxIterations = 100;

        //请注意!!未处理TaskCanceledException
        //如果"Just My code"被抛出错误
        //在您的计算机上启用。在Express版本上JMC是
        //启用且不能禁用。例外是良性的。
        //按F5继续执行代码。
        for (int i = 0; i <= maxIterations; i++)
        {
            // Do a bit of work. Not too much.
            var sw = new SpinWait();
            for (int j = 0; j <= 100; j++)
                sw.SpinOnce();

            if (ct.IsCancellationRequested)
            {
                Console.WriteLine("Task {0} cancelled", taskNum);
                ct.ThrowIfCancellationRequested();
            }
        }
    }
}
// The example displays output like the following:
//       Press any key to begin tasks...
//    To terminate the example, press 'c' to cancel and exit...
//
//    Task 1 executing
//    Task 2 executing
//    Task 3 executing
//    Task 4 executing
//    Task 5 executing
//    Task 6 executing
//    Task 7 executing
//    Task 8 executing
//    c
//    Task cancellation requested.
//    Task 2 cancelled
//    Task 7 cancelled
//
//    OperationCanceledException thrown
//
//    Task 2 status is now Canceled
//    Task 1 status is now RanToCompletion
//    Task 8 status is now Canceled
//    Task 7 status is now Canceled
//    Task 6 status is now RanToCompletion
//    Task 5 status is now RanToCompletion
//    Task 4 status is now RanToCompletion
//    Task 3 status is now RanToCompletion