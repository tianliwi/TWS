using System;

namespace HistoricalDataDownloader
{
    public interface IClient
    {
        /// Properties
        long ServerTime { get; }
        string Account { get; }
        int ClientId { get; set; }
        long NextValidOrderId { get; }
        bool RequestPositionsOnStartup { get; set; }
        int ServerVersion { get; }
        bool isServerConnected { get; }

        void Connect();
        void Disconnect();

        // Outgoing Messages
        void RequestHistoricalData(BarRequest br, bool useRTH = false);

        /// Delegates triggered by incoming messages
        event Action<Bar> GotHistoricalBarDelegate;
        event Action<string> GotServerInitializedDelegate;
        event Action<string> SendDebugEventDelegate;
    }
}
