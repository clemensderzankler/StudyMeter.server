using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace studie_meter.server {
    public class FeedbackData {
        public string StudentIP { get; set; }
        public int Comprehensibility { get; set; }
        public int Interest { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class FeedbackHub : Hub {
        private static readonly List<string> ConnectedStudents = new List<string>();
        private static readonly List<string> ConnectedLecturers = new List<string>();
        private static readonly List<FeedbackData> FeedbackHistory = new List<FeedbackData>();

        public async Task RegisterStudent() {
            var ipAddress = GetIpAddress();

            if (ipAddress != null && !ConnectedStudents.Contains(ipAddress)) {
                ConnectedStudents.Add(ipAddress);
            }
            await Clients.All.SendAsync("UpdateStudentList", ConnectedStudents);
        }

        public async Task RegisterLecturer() {
            var ipAddress = GetIpAddress();
            if (ipAddress == null) return;
            if (!ConnectedLecturers.Contains(ipAddress)) {
                ConnectedLecturers.Add(ipAddress);
                await Clients.Caller.SendAsync("ReceiveFeedback", FeedbackHistory);
                await Clients.Caller.SendAsync("UpdateStudentList", ConnectedStudents);
            }
        }

        public async Task RemoveStudent() {
            var ipAddress = GetIpAddress();
            if (ipAddress != null && ConnectedStudents.Contains(ipAddress)) {
                ConnectedStudents.Remove(ipAddress);
                await Clients.All.SendAsync("UpdateStudentList", ConnectedStudents);
            }
        }

        public async Task RemoveLecturer() {
            var ipAddress = GetIpAddress();
            if (ipAddress == null) return;
            if (ConnectedLecturers.Contains(ipAddress)) {
                ConnectedLecturers.Remove(ipAddress);
            }
        }

        public async Task SendFeedback(int comprehensibility, int interest) {
            var ipAddress = GetIpAddress();
            if (ipAddress == null) return;

            var feedback = new FeedbackData {
                StudentIP = ipAddress,
                Comprehensibility = comprehensibility,
                Interest = interest,
                Timestamp = DateTime.UtcNow
            };
            // FeedbackHistory.Add(feedback);
            AddOrUpdateFeedback(feedback);
            await Clients.All.SendAsync("ReceiveFeedback", FeedbackHistory);
        }

        public async Task RequestBreak() {
            await Clients.All.SendAsync("BreakRequested");
        }

        public override async Task OnDisconnectedAsync(Exception exception) {
            // Handle disconnections - in a real app, you'd identify which user disconnected
            // and remove them from the appropriate list
            await RemoveLecturer();
            await RemoveStudent();

            await base.OnDisconnectedAsync(exception);
        }
        private void AddOrUpdateFeedback(FeedbackData data) {
            var historyFeedback = FeedbackHistory.FirstOrDefault(d => d.StudentIP == data.StudentIP);
            if (historyFeedback == null) {
                FeedbackHistory.Add(data);
            }
            else {
                FeedbackHistory.Remove(historyFeedback);
                FeedbackHistory.Add(data);
            }
        }
        private string? GetIpAddress() {
            var httpContext = Context.GetHttpContext();
            return httpContext?.Connection.RemoteIpAddress?.ToString();
        }
    }
}
