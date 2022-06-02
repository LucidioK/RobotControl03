using System;
using System.Threading.Tasks;

namespace RobotControl.ClassLibrary
{
    public interface IRobotCommunication : IDisposable
    {
        Task<RobotCommunicationResult> ReadAsync();
        Task SetMotorsAsync(int l, int r);
        Task WriteAsync(string s);
        Task StartAsync();
        Task StopMotorsAsync();

        RobotCommunicationResult Read();
        void SetMotors(int l, int r);
        void Write(string s);
        void Start();
        void StopMotors();
    }
}