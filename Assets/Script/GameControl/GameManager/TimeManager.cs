using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using System.Linq;

public static class TimeManager
{
    // Attempts to get the network time from an NTP server with retries and timeout
    private static async Task<DateTime> GetNetworkTimeStrictAsync(int retries = 3, int timeoutMilliseconds = 3000)
    {
        const string ntpServer = "time.windows.com";
        const byte serverReplyTime = 40;
        byte[] ntpData = new byte[48];
        ntpData[0] = 0x1B;

        for (int attempt = 0; attempt < retries; attempt++)
        {
            try
            {
                var addresses = await Dns.GetHostEntryAsync(ntpServer);
                var address = addresses.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                if (address == null)
                    throw new Exception("No valid IPv4 address found.");

                var ipEndPoint = new IPEndPoint(address, 123);

                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Connect(ipEndPoint);
                    socket.ReceiveTimeout = timeoutMilliseconds;

                    await Task.Factory.FromAsync(socket.BeginSend(ntpData, 0, ntpData.Length, SocketFlags.None, null, null), socket.EndSend);
                    await Task.Factory.FromAsync(socket.BeginReceive(ntpData, 0, ntpData.Length, SocketFlags.None, null, null), socket.EndReceive);
                }

                uint intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
                uint fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                long milliseconds = (intPart * 1000L) + ((fractPart * 1000L) / 0x100000000L);
                var networkTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milliseconds);

                return networkTime.ToLocalTime();
            }
            catch when (attempt < retries - 1)
            {
                UIController.Instance.ShowConnecting();
                await Task.Delay(500);
            }
        }

        throw new Exception("Unable to get time from NTP server.");
    }

    // Swaps the endianness of a 32-bit unsigned integer
    static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0x000000ff) << 24) +
                    ((x & 0x0000ff00) << 8) +
                    ((x & 0x00ff0000) >> 8) +
                    ((x & 0xff000000) >> 24));
    }

    // Continuously tries to get the network time until successful
    public static async Task<DateTime> TryGetNetworkTimeUntilSuccess()
    {
        try
        {
            DateTime networkTime = await GetNetworkTimeStrictAsync();
            UIController.Instance.HideConnectPanel();
            return networkTime;
        }
        catch (Exception ex)
        {
            Debug.LogError("Unable to get network time: " + ex.Message);

            await WaitForRetryButtonAsync();

            return await TryGetNetworkTimeUntilSuccess();
        }
    }
    
    private static TaskCompletionSource<bool> retryTcs;

    // Waits for the user to press the retry button
    private static Task WaitForRetryButtonAsync()
    {
        retryTcs = new TaskCompletionSource<bool>();
        UIController.Instance.ShowDisconnect();
        return retryTcs.Task;
    }

    // Called when the retry button is clicked
    public static void OnRetryClicked()
    {
        UIController.Instance.ShowConnecting();
        retryTcs?.TrySetResult(true);
    }
}
